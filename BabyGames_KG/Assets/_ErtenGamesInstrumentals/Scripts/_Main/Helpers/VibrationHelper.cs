#if FeelInstalled
using Lofelt.NiceVibrations;
using UnityEngine;
#endif

namespace Helpers
{
    public class VibrationHelper
    {
        public bool canVibrate
        {
            get
            {
#if FeelInstalled
                return HapticController.hapticsEnabled;
#else
                return false;
#endif
            }
        }

        public virtual void SetActive(bool active)
        {
#if FeelInstalled
            HapticController.hapticsEnabled = active;
#endif
        }

        public virtual void LightVibration()
        {
#if FeelInstalled
            Debug.Log("LightImpact");
            try { HapticPatterns.PlayPreset(HapticPatterns.PresetType.LightImpact); } catch { }
#endif
        }

        public virtual void MeduimVibration()
        {
#if FeelInstalled
            Debug.Log("MediumImpact");
            try { HapticPatterns.PlayPreset(HapticPatterns.PresetType.MediumImpact); } catch { }
#endif
        }

        public virtual void HardVibration()
        {
#if FeelInstalled
            Debug.Log("HeavyImpact");
            try { HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact); } catch { }
#endif
        }
    }
}