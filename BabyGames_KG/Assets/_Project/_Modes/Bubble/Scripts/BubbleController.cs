using FX;
using Helpers;
using Observables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Bubble
{
    [Serializable]
    public class BubbleContainerData
    {
        public bool isCompleted = false;

        public Action<BubbleContainerData, BubbleUnit> onCaught;
        public Action<BubbleContainerData> onCompleted;

        public BubbleContainer container;
        public List<BubbleUnit> bubbleUnits = new();

        public List<BubbleUnit> currentBubbleUnits;
        public List<RectTransform> positions;

        public int capacity = 5;

        public void OnCatch(BubbleUnit bubbleUnit)
        {
            onCaught?.Invoke(this, bubbleUnit);
        }

        public void Complete()
        {
            isCompleted = true;
            onCompleted?.Invoke(this);
        }
    }

    public class BubbleController : MonoBehaviour
    {
        public ObservableValue<bool> hasWon;

        public List<BubbleContainerData> bubbleContainerDatas = new();
        public Vector2 spawnRate = new(0.25f, 1);
        public List<BubbleUnit> emptyBubbles = new();
        [Range(0, 1)] public float emptyBubbleProbability = 0.75f;

        [Space]
        [SerializeField] private HintHand_Click _hintHandClick;

        private GameplayMenu_Bubble _gameplayMenu_Bubble;

        private CancellationToken _dct;


        private void Awake()
        {
            _dct = destroyCancellationToken;
        }

        public void Construct(GameplayMenu_Bubble gameplayMenu_Bubble)
        {
            _gameplayMenu_Bubble = gameplayMenu_Bubble;

            foreach (var bubbleContainer in _gameplayMenu_Bubble.bubbleContainers)
            {
                bubbleContainerDatas.Find(x => x.container == null).container = bubbleContainer;
            }

            if (_hintHandClick == null)
            {
                _hintHandClick = gameObject.AddComponent<HintHand_Click>();
            }

            foreach (var bubbleContainerData in bubbleContainerDatas)
            {
                bubbleContainerData.container.Construct(bubbleContainerData, _hintHandClick);
            }

            _hintHandClick?.SetIsActive(true);
        }

        public async void Initialize()
        {
            while (hasWon.value == false && _dct.IsCancellationRequested == false)
            {
                if (bubbleContainerDatas.TrueForAll(x => x.isCompleted))
                {
                    hasWon.value = true;

                    _hintHandClick.SetIsActive(false);
                    return;
                }

                BubbleContainerData data = data = bubbleContainerDatas.Where(x => x.isCompleted == false).GetRandom();

                var randomNumber = UnityEngine.Random.Range(0f, 1f);
                bool isEmpty = randomNumber < emptyBubbleProbability;
                data.container.Spawn(isEmpty ? emptyBubbles.GetRandom() : null);

                await AsyncHelper.DelayFloat(spawnRate.GetRandom());
            }
        }
    }
}