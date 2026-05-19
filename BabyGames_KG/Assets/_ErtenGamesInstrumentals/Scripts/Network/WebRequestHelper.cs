#if UNITY_2023_2_OR_NEWER
using UnityEngine;
#else
using System.Threading.Tasks;
#endif

using DataClasses;
using System.Text;

using UnityEngine.Networking;

namespace Helpers
{
    public static class WebRequestHelper
    {
        public static async
#if UNITY_2023_2_OR_NEWER
            Awaitable<UnityWebRequest>
#else
            Task<UnityWebRequest>
#endif
            Post(string uri, string json)
        {
            UnityWebRequest webRequest = UnityWebRequest.PostWwwForm(uri, "POST");

            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json)) as UploadHandler;

            var sentRequest = webRequest.SendWebRequest();

            while (sentRequest.isDone == false) await AsyncHelper.Skip();

            return sentRequest.webRequest;
        }

        public static async
#if UNITY_2023_2_OR_NEWER
            Awaitable<UnityWebRequest>
#else
            Task<UnityWebRequest>
#endif
            Get(string uri, Token token = null)
        {
            UnityWebRequest webRequest = UnityWebRequest.Get(uri);

            if (token != null) webRequest.SetRequestHeader("Authorization", token.token);
            webRequest.SetRequestHeader("Content-Type", "application/json");

            webRequest.url = uri;

            webRequest.uri = new System.Uri(uri);
            webRequest.url = uri;

            var sentRequest = webRequest.SendWebRequest();

            while (sentRequest.isDone == false) await AsyncHelper.Skip();

            return sentRequest.webRequest;
        }

        public static async
#if UNITY_2023_2_OR_NEWER
            Awaitable<UnityWebRequest>
#else
            Task<UnityWebRequest>
#endif
             Update(string uri, string json, Token token = null, string method = "PUT")
        {
            UnityWebRequest webRequest = UnityWebRequest.Put(uri, method);

            webRequest.method = method;

            if (token != null) webRequest.SetRequestHeader("Authorization", token.token);
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json)) as UploadHandler;

            var sentRequest = webRequest.SendWebRequest();

            while (sentRequest.isDone == false) await AsyncHelper.Skip();

            return sentRequest.webRequest;
        }

        public static async
#if UNITY_2023_2_OR_NEWER
            Awaitable<UnityWebRequest>
#else
            Task<UnityWebRequest>
#endif
            Delete(string uri, Token token = null)
        {
            UnityWebRequest webRequest = UnityWebRequest.Delete(uri);

            if (token != null) webRequest.SetRequestHeader("Authorization", token.token);
            webRequest.SetRequestHeader("Content-Type", "application/json");

            var sentRequest = webRequest.SendWebRequest();

            while (sentRequest.isDone == false) await AsyncHelper.Skip();

            return sentRequest.webRequest;
        }

        public class ForceAcceptAll : CertificateHandler
        {
            protected override bool ValidateCertificate(byte[] certificateData)
            {
                return true;
            }
        }
    }
}
