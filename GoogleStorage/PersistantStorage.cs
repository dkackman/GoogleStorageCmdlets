using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Diagnostics;
using System.Dynamic;
using System.Security;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GoogleStorage 
{
    class PersistantStorage
    {
        public void RemoveObject(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new InvalidOperationException("name cannot be null or empty");
            }

            if (ObjectExists(name))
            {
                using (var storage = GetStorage())
                {
                    storage.DeleteFile(name);
                }
            }
        }

        public bool ObjectExists(string name)
        {
            using (var storage = GetStorage())
            {
                return storage.FileExists(name);
            }
        }

        public dynamic RetrieveObject(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new InvalidOperationException("name cannot be null or empty");
            }
            Debug.Assert(ObjectExists(name));
            try
            {
                using (var storage = GetStorage())
                using (var file = storage.OpenFile(name, FileMode.Open, FileAccess.Read))
                using (var reader = new StreamReader(file))
                {
                    string json = reader.ReadToEnd();

                    return JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());
                }
            }
            catch (Exception e)
            {
                Debug.Assert(false, e.Message);
            }

            return null;
        }

        public void StoreObject(string name, object value)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new InvalidOperationException("name cannot be null or empty");
            }

            if (object.ReferenceEquals(value, null))
            {
                throw new NullReferenceException("value cannot be null");
            }

            using (var storage = GetStorage())
            using (var stream = storage.OpenFile(name, FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(stream))
            {
                // ensure we serialize the secure string as encrypted
                string json = JsonConvert.SerializeObject(value, new SecureStringConverter());
                writer.Write(json);
            }
        }

        private static IsolatedStorageFile GetStorage()
        {
            return IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
        }

        /// <summary>
        /// This guy is user to ensure that secure strings get serilized as encrypted values
        /// Deserializers, because we are using dynamic objects, will need to know what properties to convert back to secure strings
        /// </summary>
        class SecureStringConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(SecureString);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                SecureString s = value as SecureString;
                serializer.Serialize(writer, s.ToEncyptedString());
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                // this will never be called because we are not deserializing to type objects
                throw new NotImplementedException();
            }
        }
    }
}
