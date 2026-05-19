using _Project._Modes.Hiding.Scripts.SO;
using _Project._Modes.Orchestra.Scripts.SO;
using _Project.Scripts.Services;
using _Project.Scripts.SO.Configs;
using _Project.Scripts.Sound;
using AYellowpaper;
using Coloring.Notification;
using DataClasses;
using DataClasses.AssetReferences;
using FX;
using GameState;
using Interfaces;
using Interfaces.Providers;
using Interfaces.Services;
using Modes.Coloring;
using ScriptableObjects;
using Services;
using Sirenix.OdinInspector;
using SO;
using System.Threading.Tasks;
using _Project.Scripts.Data;
using _Project.Scripts.Services.Purchase;
using UnityEngine;
using Zenject;

namespace Core.DI
{
    public class Global_DI : MonoInstaller
    {
        public enum PreloadStatus
        {
            None,
            InjectingServices,
            InjectingConfigs,
            DoneSuccessfuly
        }

        [ShowInInspector] public static PreloadStatus preloadStatus = PreloadStatus.None;

        [SerializeField, FoldoutGroup("SO")] private ExternalAssetReference<GameConfigs_Coloring_SO> _gameConfig_SO;
        [SerializeField, FoldoutGroup("SO")] private ExternalAssetReference<ListOfAllScenes_SO> _listOfAllScenes;
        [SerializeField, FoldoutGroup("SO")] private ExternalAssetReference<ListOfAllMenus_SO> _listOfAllMenus;
        [SerializeField, FoldoutGroup("SO")] private ExternalAssetReference<Content> _content;
        [SerializeField, FoldoutGroup("SO")] private ExternalAssetReference<GameSavableSettings> _gameSavableSettings;
        [SerializeField, FoldoutGroup("SO")] private SoundConfig_SO _soundConfig_SO;

        [SerializeField, FoldoutGroup("Services")] private InterfaceReference<ISceneLoader> _sceneLoader;
        [SerializeField, FoldoutGroup("Services")] private InterfaceReference<ISoundPlayer> _soundPlayer;
        [SerializeField, FoldoutGroup("Services")] private InterfaceReference<IContentDeliveryService> _contentDeliveryService;
        [SerializeField, FoldoutGroup("Services")] private InterfaceReference<IActivities_SO_Provider> _activities_SO_Provider;
        [SerializeField, FoldoutGroup("Services")] private InterfaceReference<IActivity_Identifier_Provider> _activity_Identifier_Provider;
        [SerializeField, FoldoutGroup("Services")] private InterfaceReference<IPurchasesService> _purchasesService;
        [SerializeField, FoldoutGroup("Services")] private InterfaceReference<ISubscriptionManager> _subscriptionManager;
        [SerializeField, FoldoutGroup("Services")] private InterfaceReference<ISubscriptionChecker> _purchaseChecker;
        [SerializeField, FoldoutGroup("Services")] private SubscriptionController _subscriptionController;
        [SerializeField, FoldoutGroup("Services")] private BackgroundMusicService _backgroundMusicService;
        [SerializeField, FoldoutGroup("Services")] private StateEnd_FX _stateEnd_FX;

        [Space]
        [SerializeField, FoldoutGroup("Services")] private AndroidNotificationController _androidNotificationController;
        [SerializeField, FoldoutGroup("Services")] private iOSNotificationController _iOSNotificationController;
        [SerializeField, FoldoutGroup("Services")] private InterfaceReference<RateUs> _rateUs;

        [SerializeField, FoldoutGroup("ModeDatas")] private Orchestra_Data _orchestra_Data;
        [SerializeField, FoldoutGroup("ModeDatas")] private Puzzle_Data _puzzle_Data;
        [SerializeField, FoldoutGroup("ModeDatas")] private Hiding_Data _hiding_Data;
        [SerializeField, FoldoutGroup("ModeDatas")] private Bubble_Data _bubble_Data;

        [SerializeField, FoldoutGroup("Data")] private ClearablesHolder _clearablesHolder;

        private GameSaveService _gameSaveService;

        private IGameStateService _gameStateService;

        private void OnDestroy()
        {
            preloadStatus = PreloadStatus.None;
        }

        public override async void InstallBindings()
        {
            await BindSO();
            BindServices();
            BindDatas();

            preloadStatus = PreloadStatus.DoneSuccessfuly;
        }

        private void BindServices()
        {
            preloadStatus = PreloadStatus.InjectingServices;

            _clearablesHolder = new();

            _gameStateService = new SimpleGameStatesChanger();
            _gameSaveService = new();

            Sound_FX.audioPlayer = _soundPlayer.Value;

            Container.Bind<ISceneLoader>().FromInstance(_sceneLoader.Value).AsSingle();
            Container.Bind<ISoundPlayer>().FromInstance(_soundPlayer.Value).AsSingle();
            Container.Bind<IGameStateService>().FromInstance(_gameStateService).AsSingle();

            Container.Bind<ClearablesHolder>().FromInstance(_clearablesHolder).AsSingle();
            Container.Bind<GameSaveService>().FromInstance(_gameSaveService).AsSingle();

            Container.Bind<IContentDeliveryService>().FromInstance(_contentDeliveryService.Value).AsSingle();
            Container.Bind<IActivities_SO_Provider>().FromInstance(_activities_SO_Provider.Value).AsSingle();
            Container.Bind<IActivity_Identifier_Provider>().FromInstance(_activity_Identifier_Provider.Value).AsSingle();

            Container.Bind<IPurchasesService>().FromInstance(_purchasesService.Value).AsSingle();
            Container.Bind<ISubscriptionChecker>().FromInstance(_purchaseChecker.Value).AsSingle();
            Container.Bind<ISubscriptionManager>().FromInstance(_subscriptionManager.Value).AsSingle();
            Container.Bind<SubscriptionController>().FromInstance(_subscriptionController).AsSingle();

            Container.Bind<BackgroundMusicService>().FromInstance(_backgroundMusicService).AsSingle();

            var _notificationController = _iOSNotificationController as NotificationBase;
#if UNITY_ANDROID
            _notificationController = _androidNotificationController;
#endif
            Container.Bind<NotificationBase>().FromInstance(_notificationController).AsSingle();
            Container.Bind<RateUs>().FromInstance(_rateUs).AsSingle();

            Container.Bind<StateEnd_FX>().FromInstance(_stateEnd_FX).AsSingle();
        }

        private async Task BindSO()
        {
            preloadStatus = PreloadStatus.InjectingConfigs;

            Container.Bind<GameConfigs_Coloring_SO>().FromInstance(await _gameConfig_SO.GetAssetAsync()).AsSingle();
            Container.Bind<ListOfAllScenes_SO>().FromInstance(await _listOfAllScenes.GetAssetAsync()).AsSingle();
            Container.Bind<ListOfAllMenus_SO>().FromInstance(await _listOfAllMenus.GetAssetAsync()).AsSingle();
            Container.Bind<Content>().FromInstance(await _content.GetAssetAsync()).AsSingle();
            Container.Bind<GameSavableSettings>().FromInstance(await _gameSavableSettings.GetAssetAsync()).AsSingle();
        }

        private void BindDatas()
        {
            Container.Bind<Orchestra_Data>().FromInstance(_orchestra_Data).AsSingle();
            Container.Bind<Puzzle_Data>().FromInstance(_puzzle_Data).AsSingle();
            Container.Bind<Hiding_Data>().FromInstance(_hiding_Data).AsSingle();
            Container.Bind<Bubble_Data>().FromInstance(_bubble_Data).AsSingle();

            Container.Bind<SoundConfig_SO>().FromInstance(_soundConfig_SO).AsSingle();
        }
    }
}