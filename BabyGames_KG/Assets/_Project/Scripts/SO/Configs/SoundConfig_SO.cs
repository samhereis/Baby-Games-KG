using _Project.Scripts.Sound;
using DataClasses;
using Loggers;
using Sounds;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.SO.Configs
{
    [Serializable]
    public class SoundConfig_SO_Data
    {
        public string name;
        public List<KeyedObject<Sound_Effect, SoundQueue_Advanced>> soundEffects;
    }

    public enum Panel_Sounds { Move, Scale }
    [CreateAssetMenu(fileName = nameof(SoundConfig_SO), menuName = "ScriptableObjects/" + nameof(SoundConfig_SO))]
    public class SoundConfig_SO : ScriptableObject
    {
        public List<SoundConfig_SO_Data> data = new();

        public List<KeyedObject<Panel_Sounds, SoundQueue_Advanced>> pannelSounds = new();

        public SoundQueue_Advanced GetSound(Sound_Effect sound, string addionalData = "Default")
        {
            if (string.IsNullOrEmpty(addionalData)) { addionalData = "Default"; }
            SoundQueue_Advanced result = null;

            try
            {
                if (data.Exists(x => x.name == addionalData) == false)
                {
                    addionalData = "Default";
                }
                else
                {
                    var foundData = data.Find(x => x.name == addionalData);
                    if (foundData.soundEffects.Exists(x => x.key == sound) == false) { addionalData = "Default"; }
                }

                var soundData = data.Find(x => x.name == addionalData);
                result = soundData.soundEffects.Find(x => x.key == sound).value;
            }
            catch (Exception e)
            {
                CustomLogger.instance?.LogException(e);
            }

            return result;
        }
    }
}