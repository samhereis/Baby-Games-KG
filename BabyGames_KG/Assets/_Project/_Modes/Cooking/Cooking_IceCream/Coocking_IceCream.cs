using DataClasses;
using DG.Tweening;
using Gameplay;
using Identifiers;
using Sirenix.OdinInspector;
using Spine.Unity;
using System.Collections.Generic;
using UnityEngine;

namespace Coocking
{
    public class Coocking_IceCream : MonoBehaviour
    {
        public List<KeyedObject<Transform, List<SpriteRenderer>>> siropSprites = new();

        [Space]
        public SkeletonAnimation skeletonAnimation;

        [Space]
        public List<PanelItem> panelItems = new();
        public List<Dropable_Basic> iceCreamBalls = new();
        public List<Dropable_Basic> fruits = new();

        [Space]
        public PlacesHolder iceCreamBallPositions;
        public PlacesHolder siropPosition;
        public List<PlacesHolder> posipkaPositions;
        public List<PlacesHolder> fruitPositions;

        private void OnEnable()
        {
            iceCreamBallPositions = GetComponent<PlacesHolder>();
            skeletonAnimation = GetComponent<SkeletonAnimation>();
        }

        [Button]
        public async void EnableSirop(int index)
        {
            foreach (var sirop in siropSprites)
            {
                var ind = 0;

                var ball = sirop.key.GetComponentInChildren<Dropable_Basic>();

                foreach (var item in sirop.value)
                {
                    if (ind == index)
                    {
                        await item.DOFade(0, 0).AsyncWaitForCompletion();
                        item.gameObject.SetActive(true);
                        item.DOFade(1, 1);

                        if (ball?.GetComponentInChildren<SpriteRenderer>() is SpriteRenderer spriteRenderer)
                        {
                            item.sortingLayerID = spriteRenderer.sortingLayerID;
                            item.sortingLayerName = spriteRenderer.sortingLayerName;
                            item.sortingOrder = spriteRenderer.sortingOrder + 1;
                        }
                    }
                    else
                    {
                        item.DOFade(0, 1);
                    }
                    ind++;
                }
            }
        }
    }
}
