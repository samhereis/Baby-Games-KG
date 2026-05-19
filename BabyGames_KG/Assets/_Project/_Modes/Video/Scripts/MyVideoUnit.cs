using DataClasses;
using Sirenix.OdinInspector;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Video;

namespace _Project
{
    public class MyVideoUnit : MonoBehaviour, IPointerClickHandler
    {
        private Gameplay_GameState_Video_Model _model;

        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _name;

        private Activity _activity;

        [ShowInInspector] public bool hasModel => _model != null;

        public async Task Initialize(Gameplay_GameState_Video_Model model, Activity activity)
        {
            _model = model;
            _activity = activity;

            _icon.sprite = await _activity.GetIcon(model.contentDeliveryService);
            _name.text = _activity.GetName();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_model.areControlsLocked.value == false)
            {
                _model.onChangeVideoRequested?.Invoke(_activity);
            }
        }
    }
}