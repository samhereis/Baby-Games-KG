using DG.Tweening;
using FX;
using Helpers;
using InterestGames;
using Services;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Project.Scripts.GameFeel;
using UnityEngine;

namespace Coocking
{
    public class RoomCleaning_BookSorting : RoomCleaning_StateBase
    {
        public Transform[] holder;

        public List<SpriteRenderer> bookOnShelf_Yellow = new();
        public List<SpriteRenderer> bookOnShelf_Red = new();
        public List<SpriteRenderer> bookOnShelf_Green = new();

        public List<Dropable_Basic> dropableBook_Yellow = new();
        public List<Dropable_Basic> dropableBook_Red = new();
        public List<Dropable_Basic> dropableBook_Green = new();

        public HintHand_Drag hintHand_Drag;

        public override async Task Enter()
        {
            await FadeCurtain(1);
            await base.Enter();

            foreach (var item in holder)
            {
                item.DOMoveX(0, 0.25f);
            }

            hintHand_Drag.objects.Clear();
            hintHand_Drag.targets.Clear();

            int index = 0;
            foreach (var item in dropableBook_Yellow)
            {
                item.onFinish -= YellowPlaced;
                item.onFinish += YellowPlaced;

                bookOnShelf_Yellow[index].DOFade(0, 0);
                index++;

                hintHand_Drag.objects.Add(item.transform);
                hintHand_Drag.targets.Add(item.targetPosition.main);

                if (item != null && item.TryGetComponent<ObjectJuicer>(out var target)) { target.StartJamming(); }
            }

            index = 0;
            foreach (var item in dropableBook_Red)
            {
                item.onFinish -= RedPlaced;
                item.onFinish += RedPlaced;

                bookOnShelf_Red[index].DOFade(0, 0);
                index++;

                hintHand_Drag.objects.Add(item.transform);
                hintHand_Drag.targets.Add(item.targetPosition.main);

                if (item != null && item.TryGetComponent<ObjectJuicer>(out var target)) { target.StartJamming(); }
            }

            index = 0;
            foreach (var item in dropableBook_Green)
            {
                item.onFinish -= GreenPlaced;
                item.onFinish += GreenPlaced;

                bookOnShelf_Green[index].DOFade(0, 0);
                index++;

                hintHand_Drag.objects.Add(item.transform);
                hintHand_Drag.targets.Add(item.targetPosition.main);

                if (item != null && item.TryGetComponent<ObjectJuicer>(out var target)) { target.StartJamming(); }
            }

            await FadeCurtain(0);

            hintHand_Drag.SetIsActive(true);
        }

        public override void Tick()
        {
            base.Tick();
            TryWin();
        }

        public override async Task Exit()
        {
            await base.Exit();

            foreach (var item in holder)
            {
                item.DOMoveX(-25, 0.25f);
            }

            hintHand_Drag.SetIsActive(false);
        }

        int _yellowIndex = 0;

        private void YellowPlaced(Dropable_Basic basic)
        {
            if (basic != null && basic.TryGetComponent<ObjectJuicer>(out var target)) { target.StopJamming(); }

            hintHand_Drag.objects.Remove(basic.transform);
            hintHand_Drag.targets.Remove(basic.targetPosition.transform);

            basic.boxCollider.enabled = false;
            basic.GetComponent<SpriteRenderer>().DOFade(0, 0.5f);

            var element = bookOnShelf_Yellow[_yellowIndex];
            element.DOFade(1, 1);

            _yellowIndex++;
        }

        int _redIndex = 0;

        private void RedPlaced(Dropable_Basic basic)
        {
            if (basic != null && basic.TryGetComponent<ObjectJuicer>(out var target)) { target.StopJamming(); }

            hintHand_Drag.objects.Remove(basic.transform);
            hintHand_Drag.targets.Remove(basic.targetPosition.transform);

            basic.boxCollider.enabled = false;
            basic.GetComponent<SpriteRenderer>().DOFade(0, 0.5f);

            var element = bookOnShelf_Red[_redIndex];
            element.DOFade(1, 1);

            _redIndex++;
        }

        int _greenIndex = 0;

        private void GreenPlaced(Dropable_Basic basic)
        {
            if (basic != null && basic.TryGetComponent<ObjectJuicer>(out var target)) { target.StopJamming(); }

            hintHand_Drag.objects.Remove(basic.transform);
            hintHand_Drag.targets.Remove(basic.targetPosition.transform);

            basic.boxCollider.enabled = false;
            basic.GetComponent<SpriteRenderer>().DOFade(0, 0.5f);

            var element = bookOnShelf_Green[_greenIndex];
            element.DOFade(1, 1);

            _greenIndex++;
        }

        [Button]
        private async void TryWin()
        {
            if (_isDone) return;

            var list = new List<Dropable_Basic>();
            list.AddRange(dropableBook_Yellow);
            list.AddRange(dropableBook_Red);
            list.AddRange(dropableBook_Green);

            if (list.TrueForAll(x => x.isDropped.value))
            {
                _isDone = true;
                DiService.Get<StateEnd_FX>()?.DoFX();

                await AsyncHelper.DelayFloat(_delayForWin);
                _nextState = _nextStateOnWin;
            }
        }
    }
}