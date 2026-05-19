using Newtonsoft.Json;

namespace Helpers
{
    public class JsonHelper
    {
        public static string ToJson<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static T FromJson<T>(string obj)
        {
            return JsonConvert.DeserializeObject<T>(obj);
        }
    }
}