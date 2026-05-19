using DataClasses.AssetReferences;
using Identifiers;
using Loggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Modes.Coloring
{
    public class PalletePanel : IdentifierBase
    {
        private Action _unlockAction;

        public List<PalletetemBase> palletetems { get; private set; } = new();

        [field: SerializeField] public ToolType toolType { get; private set; }
        [field: SerializeField] public CanvasGroup canvasGroup { get; private set; }
        [field: SerializeField] public Transform content { get; private set; }
        [field: SerializeField] public ExternalAssetReference_HasComponent<PalletetemBase> panneltemPrefab { get; private set; }

        [SerializeField] private RectTransform _lockState;

        private bool _isInitialized = false;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            palletetems = GetComponentsInChildren<PalletetemBase>(true).ToList();
        }

        public async Task Initialize(InstrumentSO instrument)
        {
            if (_isInitialized == true) { return; }

            try
            {
                foreach (var item in instrument.colorInfo)
                {
                    PalletetemBase palletetemBase = await panneltemPrefab.InstantiateAsync(content);
                    palletetemBase.Initialize(item);
                }

                palletetems = TryGetAll_List<PalletetemBase>();

                _isInitialized = true;
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex, $"Error during initialize PalletePanel {toolType.ToString()}");
            }

        }

        public void SetAvailability(bool isAvailable, Action callback)
        {
            _unlockAction = callback;
            _lockState.gameObject.SetActive(isAvailable == false);

            foreach (var item in _lockState.GetComponentsInChildren<Button>(true))
            {
                if (isAvailable)
                {
                    item.onClick.RemoveListener(OnTryUnlock);
                }
                else
                {
                    item.onClick.AddListener(OnTryUnlock);
                }
            }
        }

        private void OnTryUnlock()
        {
            _unlockAction?.Invoke();
        }
    }
}