using DataClasses;
using Helpers;
using Identifiers;
using Services;
using Sirenix.OdinInspector;
using Spine.Unity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Modes.Coloring
{
    [DisallowMultipleComponent]
    public class HasOverrides_Identifier : IdentifierBase, ISelfValidator
    {
        [SerializeField] public List<KeyedObject<string, List<string>>> _copiables_dominantColor = new();
        [SerializeField] public List<KeyedObject<string, List<string>>> _copiables_CopyTexture = new();

        [Inject] GameController _gameController;

        public void Validate(SelfValidationResult result)
        {
            _components.Clear();
        }

        [Button]
        private void AutoSet()
        {
            _copiables_dominantColor.Clear();

            var slotsDatas = Get<SkeletonAnimation>().skeletonDataAsset.GetSkeletonData(false).Slots;

            foreach (var item in slotsDatas)
            {
                if (item.Name.Contains("-outline")) { continue; }

                KeyedObject<string, List<string>> copiablesData = new KeyedObject<string, List<string>>(item.Name, new());

                string copiableName = $"{item.Name}-copy";
                if (slotsDatas.Exists(x => x.Name == copiableName))
                {
                    copiablesData.value.Add(copiableName);
                }

                for (int i = 0; i < 10; i++)
                {
                    copiableName = $"{item.Name}-copy_{i}";
                    if (slotsDatas.Exists(x => x.Name == copiableName))
                    {
                        copiablesData.value.Add(copiableName);
                    }
                }

                if (copiablesData.value.Count > 0) { _copiables_dominantColor.Add(copiablesData); }
            }
        }

        private void OnEnable()
        {
            DiService.Inject(this);

            _gameController.model.onAnimationStarting -= Mirror;
            _gameController.model.onAnimationStarting += Mirror;
        }

        private void OnDisable()
        {
            _gameController.model.onAnimationStarting -= Mirror;
        }

        public async void Initialize()
        {
            SetMirrorOverrides();

            if (TryGetComponent<HasOverrides_Identifier>(out var hasOverrides))
            {
                foreach (var item in hasOverrides.TryGetAll_List<OverrideBase>())
                {
                    await item.Initialize();
                }
            }

            foreach (var item in hasOverrides.TryGetAll_List<Overrode_MirrorTexture>())
            {
                item.Initialize();
            }
        }

        private void SetMirrorOverrides()
        {
            var paintables = TryGetAll_List<Paintable_Identifier_SlotSeparation>();

            foreach (var copiableData in _copiables_dominantColor)
            {
                var original = paintables.Find(x => x.name == copiableData.key)?.gameObject?.AddComponent<Overrode_MirrorTexture>();
                if (original == null) { continue; }

                var copiables = paintables.FindAll(x => copiableData.value.Contains(x.name)).ToList();
                if (copiables == null || copiables.Count < 1) { continue; }

                original.SetCopyiables(copiables, Overrode_MirrorTexture.MirrorMode.DominantColor);
            }

            foreach (var copiableData in _copiables_CopyTexture)
            {
                var original = paintables.Find(x => x.name == copiableData.key)?.gameObject?.AddComponent<Overrode_MirrorTexture>();
                if (original == null) { continue; }

                var copiables = paintables.FindAll(x => copiableData.value.Contains(x.name)).ToList();
                if (copiables == null || copiables.Count < 1) { continue; }

                original.SetCopyiables(copiables, Overrode_MirrorTexture.MirrorMode.CopyTexture);
            }
        }

        private async Task Mirror()
        {
            foreach (var item in TryGetAll_List<Overrode_MirrorTexture>())
            {
                item.Mirror();
            }

            await AsyncHelper.NextFrame();
        }
    }
}