using Settings;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = nameof(GameSavableSettings), menuName = "ScriptableObjects/" + nameof(GameSavableSettings))]
    public class GameSavableSettings : ScriptableObject
    {
        public BoolSavable_SO notifications;
        public BoolSavable_SO backgroundMusic;

        public void Initialize()
        {
            notifications.Initialize();

#if UNITY_EDITOR
            backgroundMusic.Initialize();
            backgroundMusic.SetData(false);
#else
            backgroundMusic.Initialize();
#endif
        }
    }
}