using DataClasses;
using Sirenix.OdinInspector;
using System.Collections.Generic;


#if UNITY_ANDROID
using System.IO;
#endif
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(DevelopmentConfigs), menuName = nameof(DevelopmentConfigs))]
public class DevelopmentConfigs : ScriptableObject
{
    public static bool contentDeliveryDebugMode => Application.isEditor && _contentDeliveryDebugMode;
    [ShowInInspector] private static bool _contentDeliveryDebugMode = false;
    public static bool alwaysPurchased => Application.isEditor && _alwaysPurchasedInEditor;
    [ShowInInspector] private static bool _alwaysPurchasedInEditor = true;

    public static bool debugMode => Application.isEditor && _debugMode;
    [ShowInInspector] private static bool _debugMode = true;

    public List<string> freeTextField;

    [SerializeField] private string keyFileName = "saratan.keystore";
    [SerializeField] private string keyFilePath;

    public List<KeyedObject<string, List<Object>>> easyAccess = new();

    [TextArea(25, 100)]
    public string notes;

#if UNITY_EDITOR
    private void Awake()
    {
        FindKeyAndKeyStoreSetPassword();
    }

    private void OnEnable()
    {
        FindKeyAndKeyStoreSetPassword();
    }

    [Button]
    public void FindKeyAndKeyStoreSetPassword()
    {
#if UNITY_ANDROID
        string assetsPath = Application.dataPath;
        string scriptableObjectDirectory = Path.GetDirectoryName(assetsPath);
        keyFilePath = Path.Combine(scriptableObjectDirectory, "..", "Keystore", keyFileName);
        keyFilePath = Path.GetFullPath(keyFilePath);

        PlayerSettings.Android.useCustomKeystore = true;
        PlayerSettings.Android.keystoreName = keyFilePath;

        SetKeystorePasswords();
#endif
    }

    [MenuItem("Project/SetKeystorePasswords", priority = 1)]
    public static void SetKeystorePasswords()
    {
#if UNITY_ANDROID
        PlayerSettings.Android.keystorePass = "saratan.coloring.keyPass1";

        PlayerSettings.Android.keyaliasName = "babygameskg";
        PlayerSettings.Android.keyaliasPass = "saratan.coloring.keyPass1"; // was: alias pass babyGamesKGPass1

        Debug.Log("Key store passes are set");
#endif
    }
#endif
}