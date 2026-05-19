using Helpers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Settings
{
    [CreateAssetMenu(fileName = "BoolSavable_SO", menuName = "Scriptables/Settings/BoolSavable_SO")]
    public sealed class BoolSavable_SO : BaseSavable_SO<bool>, ISelfValidator
    {
        public void Validate(SelfValidationResult result)
        {
            Initialize();

#if UNITY_EDITOR
            if (name.StartsWith("_Key") == false)
            {
                key = _keyStartsWith + name + _keyEndsWith;
                this.TrySetDirty();
            }
#endif
        }

        [Button]
        public override void Initialize()
        {
            currentValue = PlayerPrefs.GetString(key, defaultValue.ToString()) == true.ToString();
        }

        public override void SetData(bool value)
        {
            base.SetData(value);

            PlayerPrefs.SetString(key, value.ToString());
            PlayerPrefs.Save();
        }
    }
}