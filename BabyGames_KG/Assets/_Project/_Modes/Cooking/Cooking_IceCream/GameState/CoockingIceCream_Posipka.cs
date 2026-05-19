using CustomAttributes;
using DataClasses;
using DG.Tweening;
using FX;
using Gameplay;
using Helpers;
using Identifiers;
using Loggers;
using Spine;
using Spine.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Coocking
{
    public class CoockingIceCream_Posipka : CoockingIceCream_StateBase
    {
        public CoockingIceCream_Form forms;

        public List<Dropable_PlaceWhileDrag> posipki = new();
        public List<KeyedObject<Dropable_PlaceWhileDrag, List<string>>> posipkiParts;
        public List<KeyedObject<Dropable_PlaceWhileDrag, List<Sprite>>> posipkaSprites = new();
        public Vector3 posipkiPartsOffset;
        public Vector3 posipkiPartsScale = Vector3.one / 2;
        public Transform holder;
        public HintHand_Drag hintHand_Drag;

        private bool _hasPosipka = false;

        public override async Task Enter()
        {
            await base.Enter();

            int index = 0;
            foreach (var item in posipki)
            {
                item.placesHolder = forms.currentForm.posipkaPositions[index];
                item.onCopyAdded += OnCopyAdded;
                index++;
            }

            holder.DOMoveX(0, 1);

            _controller.panel_World.PrepareForAnimation(posipki.Select(x => x.GetComponent<PanelItem>()).ToList());
            await _controller.panel_World.Appear();
            _controller.panel_World.AnimateItems();

            try
            {
                hintHand_Drag = GetComponent<HintHand_Drag>();
                hintHand_Drag?.SetIsActive(true);

                hintHand_Drag.objects.Clear();
                hintHand_Drag.targets.Clear();
                hintHand_Drag.objects.AddRange(posipki.Select(x => x.transform));
                hintHand_Drag.targets.AddRange(posipki.Select(x => x.placesHolder.secondary.GetRandom()));
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
            }
        }

        private void OnCopyAdded(Dropable_PlaceWhileDrag drag, GameObject copy)
        {
            var spriteGroup = posipkaSprites.Find(x => x.key == drag);
            copy.GetComponentInChildren<SpriteRenderer>().sprite = spriteGroup.value.GetRandom();

            _model.requestCompleteButtonShow?.Invoke();
            _model.onCompleteButtonPressed += Win;
        }

        private async void Win()
        {
            _model.onCompleteButtonPressed -= Win;
            await _controller.panel_World.HideItems();

            _nextState = _nextStateOnWin;
        }
    }
}