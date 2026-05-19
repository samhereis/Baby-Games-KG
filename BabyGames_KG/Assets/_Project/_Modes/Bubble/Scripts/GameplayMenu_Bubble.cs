using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UI.Menus;
using UnityEngine;
using UnityEngine.UI;

namespace Bubble
{
    public class GameplayMenu_Bubble : MenuBase
    {
        public Button backButton;
        public RectTransform content;

        public List<BubbleContainer> bubbleContainers = new();

        [Button]
        public void PopulateContainer()
        {
            bubbleContainers.Clear();
            foreach (var container in GetComponentsInChildren<BubbleContainer>(true))
            {
                bubbleContainers.Add(container);
            }
        }

        internal void Deactivcate()
        {
            backButton.transform.DOScale(0, 1);

            Get<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
            Get<Canvas>().worldCamera = Camera.main;
            Get<Canvas>().planeDistance = 5;
            Get<Canvas>().sortingOrder = 0;
        }
    }
}