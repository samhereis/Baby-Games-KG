using Identifiers;
using Spine.Unity;
using System.Linq;
using UnityEngine;

namespace Carwash
{
    public class CarwashCar_Identifier : IdentifierBase
    {
        public SpriteRenderer dirt;
        public SkeletonAnimation blik;

        public bool isFinished;

        public GeneratePointsInBox generatePointsInBox => Get<GeneratePointsInBox>();

        private void Awake()
        {
            if (dirt == null) { dirt = GetComponentsInChildren<SpriteRenderer>().First(x => x.name == "Dirt"); }
            if (blik == null) { blik = Get<SkeletonAnimation>(); }

            gameObject.SetActive(false);
        }
    }
}