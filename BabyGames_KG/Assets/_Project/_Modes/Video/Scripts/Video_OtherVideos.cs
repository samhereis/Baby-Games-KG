using _Project;
using Assets._Project._Modes.Video.Scripts;
using DataClasses.AssetReferences;
using UnityEngine;
using UnityEngine.UI;

namespace Video
{
    public class Video_OtherVideos : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private SwipeDetector _swipeDetector;
        [SerializeField] private RectTransform _myVideosHolder;

        [SerializeField] private GameplayMenu_Video _gameplayMenu;

        [SerializeField] private CanvasGroup _controlsCG;

        [SerializeField] private ExternalAssetReference_HasComponent<MyVideoUnit> _myVideoUnit_PrefabReference;

        private async void Start()
        {
            foreach (var myVideo in _myVideosHolder.GetComponentsInChildren<MyVideoUnit>(true))
            {
                Destroy(myVideo.gameObject);
            }

            foreach (var item in Gameplay_GameState_Video_Model.videosToWatch)
            {
                if (item.activityName == _gameplayMenu.model.activity.activityName)
                {
                    continue;
                }

                var myVidep = await _myVideoUnit_PrefabReference.InstantiateAsync(_myVideosHolder);
                await myVidep.Initialize(_gameplayMenu.model, item);
            }
        }

        private void OnEnable()
        {
            _swipeDetector.onSwipeDown += OpenControlls;
            _button?.onClick.AddListener(OpenControlls);
        }

        private void OnDisable()
        {
            _swipeDetector.onSwipeDown -= OpenControlls;
            _button?.onClick.RemoveListener(OpenControlls);
        }

        private void OpenControlls()
        {
            _gameplayMenu.SetVideoWindow(_controlsCG);
        }
    }
}