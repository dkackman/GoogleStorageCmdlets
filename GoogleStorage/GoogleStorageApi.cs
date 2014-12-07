using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Dynamic;
using System.IO;

using Newtonsoft.Json;

using DynamicRestProxy.PortableHttpClient;

namespace GoogleStorage
{
    public class GoogleStorageApi
    {
        public const string AuthScope = "https://www.googleapis.com/auth/devstorage.full_control https://www.googleapis.com/auth/devstorage.read_write";

        public string Project { get; private set; }

        public string UserAgent { get; private set; }

        public string access_token { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        private readonly dynamic _googleStorage;
        private readonly dynamic _googleStorageUpload;

        public GoogleStorageApi(string project, string agent, string token, CancellationToken cancelToken)
        {
            Project = project;
            UserAgent = agent;
            access_token = token;
            CancellationToken = cancelToken;

            dynamic client = CreateClient();
            _googleStorage = client.storage.v1;
            _googleStorageUpload = client.upload.storage.v1;
        }

        public async Task<bool> FindObject(string bucket, string objectName)
        {
            try
            {
                await GetObject(bucket, objectName);
                return true;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        public async Task<dynamic> GetObject(string bucket, string objectName)
        {
            return await _googleStorage.b(bucket).o(objectName).get(CancellationToken);
        }

        public async Task<dynamic> UpdateObjectMetaData(string bucket, string objectName, string propertName, string propertyValue)
        {
            IDictionary<string, object> body = new ExpandoObject();
            body.Add(propertName, propertyValue == "" ? null : propertyValue);

            return await _googleStorage.b(bucket).o(objectName).patch(CancellationToken, body, fields: propertName);
        }

        public async Task<dynamic> ImportObject(FileInfo file, string name)
        {
            using (var stream = new StreamInfo(file.OpenRead(), file.GetContentType()))
            {
                return await _googleStorageUpload.b.unit_tests.o.post(stream, name: new PostUrlParam(name), uploadType: new PostUrlParam("media"));
            }
        }

        public async Task ExportObject(Tuple<dynamic, string> item, bool includeMetaData)
        {
            var downloader = new FileDownloader(item.Item1.mediaLink, item.Item2, item.Item1.contentType, UserAgent);

            await downloader.Download(CancellationToken, access_token);

            if (includeMetaData)
            {
                using (var writer = new StreamWriter(item.Item2 + ".metadata.json"))
                {
                    string json = JsonConvert.SerializeObject(item.Item1);
                    writer.Write(json);
                }
            }
        }

        public async Task<dynamic> GetBuckets()
        {
            return await _googleStorage.b.get(CancellationToken, project: Project);
        }

        public async Task<dynamic> GetBucket(string bucket)
        {
            return await _googleStorage.b(bucket).get(CancellationToken);
        }

        public async Task<bool> FindBucket(string bucket)
        {
            try
            {
                await GetBucket(bucket);
                return true;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        public async Task<dynamic> GetBucketContents(string bucket)
        {
            return await _googleStorage.b(bucket).o.get(CancellationToken);
        }

        public async Task<dynamic> RemoveBucket(string bucket)
        {
            return await _googleStorage.b(bucket).delete(CancellationToken);
        }

        public async Task<dynamic> AddBucket(string bucket)
        {
            dynamic args = new ExpandoObject();
            args.name = bucket;

            return await _googleStorage.b.post(CancellationToken, args, project: new PostUrlParam(Project));
        }

        private dynamic CreateClient()
        {
            var defaults = new DynamicRestClientDefaults()
            {
                UserAgent = UserAgent,
            };

            if (!string.IsNullOrEmpty(access_token))
            {
                defaults.AuthScheme = "OAuth";
                defaults.AuthToken = access_token;
            }

            return new DynamicRestClient("https://www.googleapis.com/", defaults);
        }
    }
}
