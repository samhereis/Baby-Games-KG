using _Project.Scripts.Sound;
using DG.Tweening;
using FX;
using Helpers;
using InterestGames;
using Loggers;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _Project.Scripts.GameFeel;
using UnityEngine;

namespace Coocking
{
    public class RoomCleaning_Toys : RoomCleaning_StateBase
    {
        public Transform[] holder;

        //dropable toy
        public List<Dropable_Basic> dropableBook_Green = new();
        public Transform basket;

        public HintHand_Drag hintHand_Drag;

        public override async Task Enter()
        {
            await base.Enter();

            foreach (var item in holder)
            {
                item.DOMoveX(0, 0.25f);
            }

            foreach (var item in dropableBook_Green)
            {
                item.GetComponent<SpriteRenderer>().sortingLayerName = "Front";
                item.GetComponent<SpriteRenderer>().sortingOrder += 10;
                item.onFinish -= ToyPlaced;
                item.onFinish += ToyPlaced;
            }

            await FadeCurtain(0);

            hintHand_Drag.objects.Clear();
            hintHand_Drag.targets.Clear();
            hintHand_Drag.objects.AddRange(dropableBook_Green.Select(x => x.transform));
            hintHand_Drag.targets.AddRange(dropableBook_Green.Select(x => x.targetPosition.transform));
            hintHand_Drag.SetIsActive(true);

            foreach (var item in dropableBook_Green)
            {
                if (item.TryGetComponent<ObjectJuicer>(out var objectJuicer)) { objectJuicer.StartJamming(); }
            }

            if (basket != null && basket.TryGetComponent<ObjectJuicer>(out var target)) { target.StartJamming(); }
        }

        public override async Task Exit()
        {
            await base.Exit();
            foreach (var item in holder)
            {
                item.transform.DOMoveX(-25, 0);
            }

            foreach (var item in dropableBook_Green)
            {
                item.transform.DOLocalMoveX(0, 1);
            }

            hintHand_Drag.SetIsActive(false);
        }

        private async void ToyPlaced(Dropable_Basic basic)
        {
            if (basic.TryGetComponent<ObjectJuicer>(out var objectJuicer)) { objectJuicer.StopJamming(); }
            if (basket != null && basket.TryGetComponent<ObjectJuicer>(out var target)) { target.StopJamming(); }

            hintHand_Drag.objects.Remove(basic.transform);
            hintHand_Drag.targets.Remove(basic.targetPosition.transform);

            basic.onFinish -= ToyPlaced;
            basic.boxCollider.enabled = false;

            try
            {
                basic._soundFX?.Play(Sound_Effect.MoveToTarget_Fast);
            } catch (Exception ex) { CustomLogger.instance?.LogException(ex); }

            await basic.transform.DOMove(basic.targetPosition.position + Vector3.up * 2, 1f).SetEase(Ease.OutBack).AsyncWaitForCompletion();
            basic.GetComponent<SpriteRenderer>().sortingLayerName = "Default";
            await basic.transform.DOMove(basic.targetPosition.position, 1f).AsyncWaitForCompletion();

            await AsyncHelper.DelayFloat(2f);
            await TryWinAsync();
        }

        private async Task TryWinAsync()
        {
            if (_isDone) { return; }

            if (dropableBook_Green.TrueForAll(x => x.isDropped.value))
            {
                _isDone = true;

                DiService.Get<StateEnd_FX>()?.DoFX();

                await AsyncHelper.DelayFloat(_delayForWin);
                _nextState = _nextStateOnWin;
            }
        }

        public override void ForceWin()
        {
            _nextState = _nextStateOnWin;
        }
    }
}