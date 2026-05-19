using Helpers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Settings
{
    [CreateAssetMenu(fileName = "FloatSavable_SO", menuName = "Scriptables/Settings/FloatSavable_SO")]
    public class FloatSavable_SO : BaseSavable_SO<float>, ISelfValidator
    {
        [field: SerializeField] public float minValue { get; protected set; } = 0;
        [field: SerializeField] public float maxValue { get; protected set; } = 100;

        public void Validate(SelfValidationResult result)
        {
            currentValue = PlayerPrefs.GetFloat(key, Mathf.Clamp(defaultValue, minValue, maxValue));

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
            currentValue = PlayerPrefs.GetFloat(key, Mathf.Clamp(defaultValue, minValue, maxValue));
        }

        public override void SetData(float value)
        {
            base.SetData(value);

            PlayerPrefs.SetFloat(key, Mathf.Clamp(currentValue, minValue, maxValue));
            PlayerPrefs.Save();
        }
    }
}