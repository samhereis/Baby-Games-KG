namespace DataClasses.Consts
{
    public static class Constants_CD
    {
        public static string RemoteContentPath => "https://pub-f1899befea8c459c94a70fa577707ae2.r2.dev/AssetBundles/";

        public static string GetAssetBundlePath(string force = null)
        {
            if (force != null) { return RemoteContentPath + force; }

#if UNITY_ANDROID
            return RemoteContentPath + "Android";
#endif
#if UNITY_IOS
            return RemoteContentPath + "iOS";
#endif
#if UNITY_STANDALONE_WIN
            return RemoteContentPath + "StandaloneWindows64";
#endif
#if UNITY_STANDALONE_OSX
            return RemoteContentPath + "StandaloneOSX";
#endif
        }
    }
}