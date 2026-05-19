using Services;
using SO;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Coloring.ForParents
{
    public class PanelLinks : MonoBehaviour
    {
        [SerializeField] private Button _buttonPrivacyPolicy;
        [SerializeField] private Button _buttonTermsOfUse;

        [Inject] private GameConfigs_Coloring_SO _gameSettings;

        private void Start()
        {
            DiService.Inject(this);

            _buttonPrivacyPolicy.onClick.AddListener(() => OpenURL(_gameSettings.linkSettings.privacyPolicy));
            _buttonTermsOfUse.onClick.AddListener(() => OpenURL(_gameSettings.linkSettings.termsOfUse));
        }

        private void OpenURL(string url)
        {
            Application.OpenURL(url);
        }
    }
}