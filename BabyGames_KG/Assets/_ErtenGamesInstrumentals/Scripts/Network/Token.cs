#if NewtonsoftInstalled
using Newtonsoft.Json;
#endif

using System;

namespace DataClasses
{
    [Serializable]
    public class Token
    {
        private const string FOLDER_NAME = "DataBase";
        private const string FILE_NAME = "Token";

#if NewtonsoftInstalled
        [JsonProperty] 
#endif
        public string token { get; set; }

        public Token(string token)
        {
            this.token = token;
        }
    }
}