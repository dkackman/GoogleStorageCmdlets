using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Diagnostics;
using System.Dynamic;

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
                string json = JsonConvert.SerializeObject(value);
                writer.Write(json);
            }
        }

        private IsolatedStorageFile GetStorage()
        {
            return IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
        }
    }
}
