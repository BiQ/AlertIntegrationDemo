using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Linq;

namespace BiQ.AlertIntegrationDemo
{
    public static class JsonConverter
    {
        public static T? DeserializeAs<T>(string json) where T : class
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static DateTimeOffset DeserializeDateTimeOffset(string json) 
        {
            return JsonConvert.DeserializeObject<DateTimeOffset>(json);
        }

        public static T? DeserializeContainerAs<T>(object? json) where T : class
        {
            if (json is null) return null;
            var container = JsonConvert.DeserializeObject<JObject>(json.ToString() ?? "");
            var containerName = container!.Properties().Select(p => p.Name).FirstOrDefault();
            if(containerName is null)
                return null;
            return container[containerName]!.ToObject<T>();
        }

        public static dynamic? DeserializeAsDynamic(string json)
        {
            return JsonConvert.DeserializeObject(json);
        }

        public static string Serialize(object obj, bool useCamelCasePropertyNames = false)
        {
            if (useCamelCasePropertyNames)
                return SerializeWithCamelCase(obj);

            return JsonConvert.SerializeObject(obj);
        }

        private static string SerializeWithCamelCase(object obj)
        {
            return JsonConvert.SerializeObject(obj,
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
        }
    }
}
