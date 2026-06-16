using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Data;
using Loggers;
using Sirenix.OdinInspector;
using SO;
using UnityEngine;
using UnityEngine.Networking;

namespace _Project.Scripts.Services
{
    public class GameEventsService : MonoBehaviour
    {
        public static GameEventsService Instance { get; private set; }

        [SerializeField] private DevelopmentConfigs _developmentConfigs;

        [ShowInInspector] private string _serverUrl = "https://1111-213-109-66-241.ngrok-free.app/event";
        private bool _appClosedSent;

        private static string GetPlatform() => Application.platform switch
        {
            RuntimePlatform.IPhonePlayer => "iOS",
            RuntimePlatform.Android => "Android",
            RuntimePlatform.WindowsPlayer => "Windows",
            RuntimePlatform.OSXPlayer => "macOS",
            RuntimePlatform.WebGLPlayer => "WebGL",
            RuntimePlatform.WindowsEditor
                or RuntimePlatform.OSXEditor
                or RuntimePlatform.LinuxEditor => "Editor",
            _ => Application.platform.ToString()
        };

        public void SetServerUrl(string url)
        {
            _serverUrl = url;
        }

        public void Initialize(Activities_SO activitiesSo)
        {
            try
            {
                SetServerUrl(activitiesSo.stringSettings.Find(x => x.key == nameof(_serverUrl)).value);

            } catch (Exception e) { CustomLogger.instance?.LogException(e); }
        }

        private void Awake()
        {
            Instance = this;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
        }

        private void OnDestroy()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#endif
        }

#if UNITY_EDITOR
        private void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                TrySendAppClosed();
            }
        }
#endif

        private void OnApplicationPause(bool pause)
        {
            if (pause) { TrySendAppClosed(); }
            else { _appClosedSent = false; }
        }

        private void OnApplicationQuit()
        {
            TrySendAppClosed();
        }

        private void TrySendAppClosed()
        {
            if (_appClosedSent) { return; }
            _appClosedSent = true;
            GameEvents.AppClosed();
        }

        public async void SendCustomEvent(string id)
        {
            await PostRaw(id, new Dictionary<string, object>());
        }

        public async void SendCustomEvent(string id, Dictionary<string, string> props)
        {
            var converted = new Dictionary<string, object>();
            foreach (var kv in props) { converted[kv.Key] = kv.Value; }
            await PostRaw(id, converted);
        }

        public async void SendCustomEvent(string id, Dictionary<string, object> props)
        {
            await PostRaw(id, props);
        }

        public async void SendCustomEvent(string id, Dictionary<string, int> props)
        {
            var converted = new Dictionary<string, object>();
            foreach (var kv in props) { converted[kv.Key] = kv.Value; }
            await PostRaw(id, converted);
        }

        public async void SendCustomEvent(string id, Dictionary<string, float> props)
        {
            var converted = new Dictionary<string, object>();
            foreach (var kv in props) { converted[kv.Key] = kv.Value; }
            await PostRaw(id, converted);
        }

        private async Task PostRaw(string eventName, Dictionary<string, object> props)
        {
            try
            {
                if (_developmentConfigs.eventsEnabled == false) { return; }

                var platform = GetPlatform();
                var allProps = new Dictionary<string, object>(props)
                {
                    ["platform"] = platform,
                    ["country"] = RegionInfo.CurrentRegion.TwoLetterISORegionName
                };
                string json = BuildJsonRaw(eventName, allProps);

                using var req = new UnityWebRequest(_serverUrl, "POST");
                req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");

                var tcs = new TaskCompletionSource<bool>();
                req.SendWebRequest().completed += _ => tcs.TrySetResult(true);
                await tcs.Task;

                if (req.result != UnityWebRequest.Result.Success)
                {
                    CustomLogger.instance?.LogWarning("Analytics", $"Failed to send '{eventName}': {req.error}", gameObject, LogTypes.Analytics, Color.yellow);
                }
                else
                {
                    CustomLogger.instance?.Log("Analytics", $"Sent '{eventName}'", gameObject, LogTypes.Analytics, Color.green);
                }
            } catch (Exception e)
            {
                CustomLogger.instance?.LogException(e, "Error sending analytics event");
            }
        }

        private static string BuildJsonRaw(string eventName, Dictionary<string, object> props)
        {
            var parts = new List<string> { $"\"event\":\"{Escape(eventName)}\"" };
            foreach (var kv in props)
            {
                string key = $"\"{Escape(kv.Key)}\"";
                string val = kv.Value switch
                {
                    int i => i.ToString(),
                    long l => l.ToString(),
                    float f => f.ToString("G"),
                    double d => d.ToString("G"),
                    bool b => b ? "true" : "false",
                    null => "null",
                    _ => $"\"{Escape(kv.Value.ToString())}\""
                };
                parts.Add($"{key}:{val}");
            }
            return "{" + string.Join(",", parts) + "}";
        }

        private static string Escape(string s)
        {
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }
}