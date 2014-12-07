using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Dynamic;
using System.IO;

using Newtonsoft.Json;

using DynamicRestProxy.PortableHttpClient;

namespace GoogleStorage
{
    public class GoogleStorageApi
    {
        public string Project { get; private set; }

        public string UserAgent { get; private set; }

        public string access_token { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        private dynamic _googleStorage;

        public GoogleStorageApi(string project, string agent, string token, CancellationToken cancelToken)
        {
            Project = project;
            UserAgent = agent;
            access_token = token;
            CancellationToken = cancelToken;

            _googleStorage = CreateClient().storage.v1;
        }

        public async Task<Tuple<dynamic, string>> ExportObject(Tuple<dynamic, string> item, bool includeMetaData)
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

            return item;
        }

        public async Task<dynamic> GetBuckets()
        {
            return await _googleStorage.b.get(CancellationToken, project: Project);
        }

        public async Task<dynamic> GetBucket(string bucket)
        {
            return await _googleStorage.b(bucket).get(CancellationToken);
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
                UserAgent = UserAgent
            }; 
            
            if (string.IsNullOrEmpty(access_token))
            {
                return new DynamicRestClient("https://www.googleapis.com/", defaults);
            }

            return new DynamicRestClient("https://www.googleapis.com/", defaults, async (request, cancelToken) =>
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("OAuth", access_token);
            });
        }
    }
}
