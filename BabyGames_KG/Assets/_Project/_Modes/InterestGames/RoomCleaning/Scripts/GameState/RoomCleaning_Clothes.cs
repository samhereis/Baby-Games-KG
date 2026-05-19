using DG.Tweening;
using FX;
using Gameplay;
using Helpers;
using InterestGames;
using Services;
using Spine.Unity;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Project.Scripts.GameFeel;
using UnityEngine;

namespace Coocking
{
    public class RoomCleaning_Clothes : RoomCleaning_StateBase
    {
        public Transform[] holder;

        public List<SpriteRenderer> hints_dress = new();
        public List<Dropable_Basic> clochtes_dress = new();

        public List<SpriteRenderer> hints_shirt = new();
        public List<Dropable_Basic> clochtes_shirt = new();

        public List<SpriteRenderer> hints_sock = new();
        public List<Dropable_Basic> clochtes_sock = new();

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
            foreach (var item in clochtes_dress)
            {
                item.targetPosition = hints_dress[index].GetComponent<PlacesHolder>();
                item.onFinish += OnClothePut_dress;
                index++;

                hintHand_Drag.objects.Add(item.transform);
                hintHand_Drag.targets.Add(item.targetPosition.main);

                if (item != null && item.TryGetComponent<ObjectJuicer>(out var target)) { target.StartJamming(); }
            }

            index = 0;
            foreach (var item in clochtes_sock)
            {
                item.targetPosition = hints_sock[index].GetComponent<PlacesHolder>();
                item.onFinish += OnClothePut_sock;
                index++;

                hintHand_Drag.objects.Add(item.transform);
                hintHand_Drag.targets.Add(item.targetPosition.main);

                if (item != null && item.TryGetComponent<ObjectJuicer>(out var target)) { target.StartJamming(); }
            }

            index = 0;
            foreach (var item in clochtes_shirt)
            {
                item.targetPosition = hints_shirt[index].GetComponent<PlacesHolder>();
                item.onFinish += OnClothePut_shirt;
                item.GetComponentInChildren<SkeletonAnimation>(true).gameObject.SetActive(false);
                index++;

                hintHand_Drag.objects.Add(item.transform);
                hintHand_Drag.targets.Add(item.targetPosition.main);

                if (item != null && item.TryGetComponent<ObjectJuicer>(out var target)) { target.StartJamming(); }
            }

            await FadeCurtain(0);

            hintHand_Drag.SetIsActive(true);
        }

        public override async Task Exit()
        {
            await base.Exit();
            foreach (var item in holder)
            {
                item.transform.DOMoveX(-25, 0);
            }

            hintHand_Drag.SetIsActive(false);
        }

        private async void OnClothePut_dress(Dropable_Basic basic)
        {
            if (basic != null && basic.TryGetComponent<ObjectJuicer>(out var target)) { target.StopJamming(); }

            var index = clochtes_dress.IndexOf(basic);
            var hint = hints_dress[index];

            basic.transform.DOMove(basic.targetPosition.position, 0.25f);
            basic.GetComponentInChildren<SpriteRenderer>().DOFade(0, 0.25f);

            hint.sharedMaterial = basic.GetComponent<SpriteRenderer>().sharedMaterial;
            basic.boxCollider.enabled = false;

            TryWin();
        }

        private async void OnClothePut_sock(Dropable_Basic basic)
        {
            if (basic != null && basic.TryGetComponent<ObjectJuicer>(out var target)) { target.StopJamming(); }

            var index = clochtes_sock.IndexOf(basic);
            var hint = hints_sock[index];

            basic.transform.DOMove(basic.targetPosition.position, 0.25f);
            basic.GetComponentInChildren<SpriteRenderer>().DOFade(0, 0.25f);

            hint.sharedMaterial = basic.GetComponent<SpriteRenderer>().sharedMaterial;
            basic.boxCollider.enabled = false;

            TryWin();
        }

        private async void OnClothePut_shirt(Dropable_Basic basic)
        {
            if (basic != null && basic.TryGetComponent<ObjectJuicer>(out var target)) { target.StopJamming(); }

            var index = clochtes_shirt.IndexOf(basic);
            var hint = hints_shirt[index];

            hint.DOFade(0, 0.25f);
            basic.transform.DOMove(basic.targetPosition.position, 0.25f);
            basic.GetComponentInChildren<SpriteRenderer>().DOFade(0, 0.25f);

            var sa = basic.GetComponentInChildren<SkeletonAnimation>(true);
            sa.transform.parent = basic.targetPosition.transform.parent;
            sa.transform.position = basic.targetPosition.position;
            sa.gameObject.SetActive(true);
            sa.AnimationName = "action";

            TryWin();
        }

        public async void TryWin()
        {
            if (_isDone) return;

            var list = new List<Dropable_Basic>();
            list.AddRange(clochtes_dress);
            list.AddRange(clochtes_sock);
            list.AddRange(clochtes_shirt);

            if (list.TrueForAll(x => x.isDropped.value))
            {
                _isDone = true;
                DiService.Get<StateEnd_FX>()?.DoFX();

                await AsyncHelper.DelayFloat(_delayForWin);
                _model.hasWon.ChangeValue(true);
            }
        }
    }
}