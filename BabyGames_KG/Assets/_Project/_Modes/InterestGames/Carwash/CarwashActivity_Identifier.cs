using Carwash;
using DataClasses;
using Helpers;
using Sirenix.OdinInspector;
using Spine.Unity;
using System.Collections.Generic;
using UnityEngine;

namespace Identifiers
{
    public class CarwashActivity_Identifier : _ActivityBase_Identifier
    {
        public SkeletonAnimation _background;
        public List<CarSelecButton> selecButtons = new();
        public List<CarwashCar_Identifier> cars = new();

        public Transform carwashPosition;

        private void Awake()
        {
            cars = TryGetAll_List<CarwashCar_Identifier>();
        }

        public override void UpdateData(Activity activity)
        {
            base.UpdateData(activity);

            type = DataClasses.ActivityType.Carwash;
        }

        [Button]
        private async void SeparateBackground()
        {
            await _background.Separate();
        }
    }
}