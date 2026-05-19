using _Project.Scripts.Sound;
using CustomAttributes;
using DG.Tweening;
using Helpers;
using Identifiers;
using Loggers;
using Observables;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Project._Modes.Coloring;
using _Project.Scripts.GameFeel;
using Coocking;
using Modes.Coloring;
using Services;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace ColorfulTrain
{
    public class ColorfulTrain_Wheel : IdentifierBase, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public ObservableValue<bool> isDropped = new ObservableValue<bool>("isDropped");
        public Action<ColorfulTrain_Wheel> onDropped;

        public Transform holder;
        public Sprite wheelSprite;
        public SpriteRenderer spriteRenderer;
        public SpriteRenderer spriteRenderer_Transparent;

        public bool canDrag = false;
        public bool autoPlace = false;
        public float dropDistance = 5;
        public Transform targetPlace;
        public List<Transform> wrongAnswers = new();

        public Vector3 _initialLocalPosition;
        public float _initialLocalPositionYOffset = 0.25f;
        [FoldoutGroup("FX")] public bool doPunchAnimation = true;
        [FoldoutGroup("FX")] public float dropParticleScale = 1;
        [FoldoutGroup("FX")] public float dragParticleScale = 1;

        public Sound_FX _soundFX;
        public Move_FX _moveFX;
        [FoldoutGroup("FX")] public TrailParticle trailParticle;
        [FoldoutGroup("FX")] public ObjectJuicer objectJuicer;

        [Fg_De] public ColorfulTrain_Wheel_UI currentUI_Copy;
        [Fg_De] public Dropable_Basic currentPanel_Copy;
        [Fg_De] public bool isInTrain;

        [Inject] private Content _content;

        public Vector3 iconPosition => holder.transform.position;

        public void Initialize()
        {
            DiService.Inject(this);

            if (spriteRenderer == null) { spriteRenderer = Get<SpriteRenderer>(); }
            if (wheelSprite == null) { wheelSprite = spriteRenderer.sprite; }

            _initialLocalPosition = holder.transform.localPosition;

            holder.DOKill();
            holder.DOLocalMove(holder.transform.localPosition + Vector3.right * _initialLocalPositionYOffset, 1);

            spriteRenderer_Transparent.sprite = spriteRenderer.sprite;

            spriteRenderer_Transparent.enabled = true;
            spriteRenderer.enabled = false;

            if (_soundFX == null) { _soundFX = GetComponent<Sound_FX>(); }
            if (_moveFX == null) { _moveFX = GetComponent<Move_FX>(); }
            if (objectJuicer == null) { objectJuicer = GetComponent<ObjectJuicer>(); }

            if (_soundFX == null) { _soundFX = gameObject.AddComponent<Sound_FX>(); }
            if (_moveFX == null) { _moveFX = gameObject.AddComponent<Move_FX>(); }
            if (objectJuicer == null) { objectJuicer = gameObject.AddComponent<ObjectJuicer>(); }
            if (trailParticle == null)
            {
                trailParticle = gameObject.AddComponent<TrailParticle>();
                trailParticle.dropParticleScale = dragParticleScale;
                trailParticle.enabled = false;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!canDrag) return;
            if (!isInTrain) return;

            if (_soundFX != null) { _soundFX?.Play(Sound_Effect.StartDrag); }
            if (trailParticle != null) { trailParticle.enabled = true; }
            spriteRenderer.sortingOrder += 1000;

            var list = FindObjectsByType<ColorfulTrain_Wheel>(FindObjectsSortMode.None);

            foreach (var item in list)
            {
                if (item == null || item.targetPlace == null) { continue; }
                wrongAnswers.SafeAdd(item.targetPlace);
            }
        }

        public async void OnDrag(PointerEventData eventData)
        {
            if (canDrag == false) { return; }
            if (isInTrain == false) { return; }

            Vector3 sp = new Vector3(eventData.position.x, eventData.position.y, 0f);
            Vector3 mousePosition = Camera.main != null ? Camera.main.ScreenToWorldPoint(sp) : holder.position;
            mousePosition.z = 0f;

            holder.position = mousePosition;

            if (autoPlace && CanPlace())
            {
                await Place(false);
            }
        }

        public async void OnEndDrag(PointerEventData eventData)
        {
            spriteRenderer.sortingOrder = 0;

            if (!canDrag) return;
            if (!isInTrain) return;
            if (isDropped.value) { return; }

            if (trailParticle != null) { trailParticle.enabled = false; }

            if (autoPlace == false && CanPlace())
            {
                await Place(true);
            }
            else
            {
                canDrag = false;

                await TryPlayFailureSounds();

                holder.DOKill();
                await holder.DOLocalMove(_initialLocalPosition, 0.25f).SetEase(Ease.OutBack).AsyncWaitForCompletion();
                if (isInTrain == true) { canDrag = true; }
            }
        }

        private bool CanPlace()
        {
            if (targetPlace == null) return false;
            if (!isInTrain) return false;

            var canPlace = targetPlace.gameObject.activeInHierarchy && Vector3.Distance(holder.position, targetPlace.position) < dropDistance;
            return canPlace;
        }

        public async Task Place(bool waitForSound)
        {
            if (!isInTrain) return;

            canDrag = false;
            if (trailParticle != null) { trailParticle.enabled = false; }
            if (objectJuicer != null) { objectJuicer.StopJamming(); }

            try
            {
                var clip = await _soundFX?.PlayAsync(Sound_Effect.Success);
                _moveFX?.Success(holder, clip.length);
                if (waitForSound) { await AsyncHelper.DelayFloat(clip.length); }
                _soundFX?.Play(Sound_Effect.MoveToTarget_Fast);

                Move_FX.MakeDoneParticle(targetPlace.position, dropParticleScale);
            } catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
            }

            holder.DOKill();
            holder.DOScale(0.5f, 0.5f).SetEase(Ease.OutBack);
            await holder.DOMove(targetPlace.position, 0.25f).SetEase(Ease.OutBack).AsyncWaitForCompletion();
            holder.gameObject.SetActive(false);
            isDropped.ChangeValue(true);
            onDropped?.Invoke(this);
        }

        public async void PlaceToTrain(bool playFX)
        {
            if (isInTrain) { return; }
            if (isDropped.value) { return; }

            spriteRenderer_Transparent.enabled = false;
            spriteRenderer.enabled = true;
            if (trailParticle != null) { trailParticle.enabled = false; }

            try
            {
                if (playFX)
                {
                    var clip = await _soundFX?.PlayAsync(Sound_Effect.Success);
                    await AsyncHelper.DelayFloat(clip.length);
                    _moveFX?.Success(holder, clip.length);
                    _soundFX?.Play(Sound_Effect.MoveToTarget_Fast);
                }
            } catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
            }

            isDropped.ChangeValue(true);
            onDropped?.Invoke(this);

            canDrag = false;
            isInTrain = true;
        }

        public void Restore()
        {
            holder.DOKill();
            holder.DOLocalMove(_initialLocalPosition, 0.25f);
            if (trailParticle != null) { trailParticle.enabled = false; }
        }

        private async Task TryPlayFailureSounds()
        {
            try
            {
                foreach (var item in wrongAnswers)
                {
                    if (item == this) { continue; }

                    var disance = Vector3.Distance(holder.position, item.position);
                    if (disance < dropDistance)
                    {
                        var clip = await _soundFX?.PlayAsync(Sound_Effect.Failure);
                        _moveFX.Fail(holder, clip.length);
                        await AsyncHelper.DelayFloat(clip.length);

                        break;
                    }
                }
            } catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
            }
        }
    }
}