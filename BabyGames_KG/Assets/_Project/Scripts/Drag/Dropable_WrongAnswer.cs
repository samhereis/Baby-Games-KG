using _Project.Scripts.Sound;
using DG.Tweening;
using Helpers;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Coocking
{
    [RequireComponent(typeof(Move_FX))]
    [RequireComponent(typeof(Sound_FX))]
    public class Dropable_WrongAnswer : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public Action<Dropable_WrongAnswer> onWrong;

        public Transform targetPosition;
        public Vector3 initialPosition;
        public float dropDistance = 2;

        private bool _hasInteracted = false;

        private Move_FX _move;
        private Sound_FX _sound;

        private void OnEnable()
        {
            _move = GetComponent<Move_FX>();
            _sound = GetComponent<Sound_FX>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_hasInteracted == false)
            {
                initialPosition = transform.position;
            }

            transform.DOKill();
        }

        public void OnDrag(PointerEventData eventData)
        {
            var sp = new Vector3(eventData.position.x, eventData.position.y, 0f);
            var world = Camera.main != null ? Camera.main.ScreenToWorldPoint(sp) : transform.position;
            world.z = 0f;
            transform.position = world;
        }

        public async void OnPointerUp(PointerEventData eventData)
        {
            var sound = await _sound.PlayAsync(Sound_Effect.Failure);
            _move.Fail(transform, sound.length);
            await AsyncHelper.DelayFloat(sound.length);

            onWrong?.Invoke(this);
            transform.DOMove(initialPosition, 1);
        }
    }
}
