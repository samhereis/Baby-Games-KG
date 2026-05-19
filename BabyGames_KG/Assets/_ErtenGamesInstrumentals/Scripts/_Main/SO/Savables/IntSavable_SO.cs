using Helpers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Settings
{
    [CreateAssetMenu(fileName = "FloatSavable_SO", menuName = "Scriptables/Settings/FloatSavable_SO")]
    public class IntSavable_SO : BaseSavable_SO<int>, ISelfValidator
    {
        public void Validate(SelfValidationResult result)
        {
            currentValue = PlayerPrefs.GetInt(key, defaultValue);

#if UNITY_EDITOR
            if (name.StartsWith("_Key") == false)
            {
                base.key = _keyStartsWith + name + _keyEndsWith;
                this.TrySetDirty();
            }
#endif
        }

        public override void Initialize()
        {
            currentValue = PlayerPrefs.GetInt(key, defaultValue);
        }

        public override void SetData(int value)
        {
            base.SetData(value);

            PlayerPrefs.SetInt(key, currentValue);
            PlayerPrefs.Save();
        }
    }
}