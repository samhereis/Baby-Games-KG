using AYellowpaper;
using Interfaces.Providers;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Core.DI
{
    public class MainMenu_DI : MonoInstaller
    {
        [SerializeField, FoldoutGroup("Services")] private InterfaceReference<IActivityCategory_UIUnits_Provider> _activityCategory_UIUnits_Provider;
        [SerializeField, FoldoutGroup("Services")] private InterfaceReference<IActivity_UIUnits_Provider> _activity_UIUnits_Provider;

        public override void InstallBindings()
        {
            Container.Bind<IActivityCategory_UIUnits_Provider>().FromInstance(_activityCategory_UIUnits_Provider.Value).AsSingle();
            Container.Bind<IActivity_UIUnits_Provider>().FromInstance(_activity_UIUnits_Provider.Value).AsSingle();
        }
    }
}