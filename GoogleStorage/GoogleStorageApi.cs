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
    public sealed class GoogleStorageApi : IDisposable
    {
        public const string AuthScope = "https://www.googleapis.com/auth/devstorage.full_control https://www.googleapis.com/auth/devstorage.read_write";

        public string Project { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        private readonly dynamic _googleStorage;
        private readonly dynamic _googleStorageUpload;

        private readonly FileDownloader _downloader;

        public GoogleStorageApi(string project, string agent, string token, CancellationToken cancelToken)
        {
            Project = project;
            _downloader = new FileDownloader(agent, token);
            CancellationToken = cancelToken;

            dynamic client = CreateClient(agent, token);
            _googleStorage = client.storage.v1;
            _googleStorageUpload = client.upload.storage.v1;
        }

        public void Dispose()
        {
            if (_downloader != null)
            {
                _downloader.Dispose();
            }

            if (_googleStorage != null)
            {
                ((IDisposable)_googleStorage).Dispose();
            }
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

        public async Task RemoveObject(string bucket, string objectName)
        {
            await _googleStorage.b(bucket).o(objectName).delete(CancellationToken);
        }

        public async Task<dynamic> UpdateObjectMetaData(string bucket, string objectName, string propertName, string propertyValue)
        {
            IDictionary<string, object> body = new ExpandoObject();
            body.Add(propertName, propertyValue == "" ? null : propertyValue);

            return await _googleStorage.b(bucket).o(objectName).patch(CancellationToken, body, fields: propertName);
        }

        public async Task<dynamic> ImportObject(FileInfo file, string bucket)
        {
            return await ImportObject(file, file.Name, bucket);
        }

        public async Task<dynamic> ImportObject(FileInfo file, string name, string bucket)
        {
            using (var stream = new StreamInfo(file.OpenRead(), file.GetContentType()))
            {
                return await _googleStorageUpload.b(bucket).o.post(CancellationToken, stream, name: new PostUrlParam(name), uploadType: new PostUrlParam("media"));
            }
        }

        public async Task ExportObject(Tuple<dynamic, string> item, bool includeMetaData)
        {
            await _downloader.Download(item.Item1.mediaLink, item.Item2, item.Item1.contentType, CancellationToken);

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

        public async Task RemoveBucket(string bucket)
        {
            await _googleStorage.b(bucket).delete(CancellationToken);
        }

        public async Task<dynamic> AddBucket(string bucket)
        {
            dynamic args = new ExpandoObject();
            args.name = bucket;

            return await _googleStorage.b.post(CancellationToken, args, project: new PostUrlParam(Project));
        }

        private static dynamic CreateClient(string agent, string access_token)
        {
            var defaults = new DynamicRestClientDefaults()
            {
                UserAgent = agent,
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
