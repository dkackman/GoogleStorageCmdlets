using System;
using System.Threading;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Diagnostics;
using System.Net.Http.Headers;

using DynamicRestProxy.PortableHttpClient;

namespace GoogleStorage
{
    public abstract class GoogleStorageAuthenticatedCmdlet : GoogleStorageCmdlet
    {
        private CancellationTokenSource _cancelTokenSource;

        [Parameter(Mandatory = false)]
        public SwitchParameter NoAuth { get; set; }

        protected DynamicRestClient CreateClient()
        {
            if (NoAuth)
            {
                return new DynamicRestClient("https://www.googleapis.com/");
            }

            return new DynamicRestClient("https://www.googleapis.com/", null, async (request, cancelToken) =>
            {
                var authToken = await GetAccessToken(); 
                request.Headers.Authorization = new AuthenticationHeaderValue("OAuth", authToken);
            });
        }

        protected async Task<string> GetAccessToken()
        {
            Debug.Assert(!NoAuth, "You should really check NoAuth prior to calling this method");

            if (NoAuth)
            {
                return "";
            }

            var auth = this.GetPersistedVariableValue<dynamic>("auth", o => o);
            if (auth == null)
            {
                throw new AccessViolationException("Access token not set. Call Grant-GoogleStorageAuth first");
            }
            
            var oauth = new GoogleOAuth2("https://www.googleapis.com/auth/devstorage.read_write");
            return await oauth.GetAccessToken(auth, GetConfig(), GetCancellationToken());
        }

        protected CancellationToken GetCancellationToken()
        {
            if (_cancelTokenSource == null)
            {
                throw new NullReferenceException("CancellationTokenSource is null. The base class BeginProccsing was not called.");
            }

            return _cancelTokenSource.Token;
        }
        
        protected void Cancel()
        {
            if (_cancelTokenSource == null)
            {
                throw new NullReferenceException("CancellationTokenSource is null. The base class BeginProccsing was not called.");
            }

            if (!_cancelTokenSource.IsCancellationRequested)
            {
                _cancelTokenSource.Cancel();
            }
        }

        protected override void BeginProcessing()
        {
            Debug.Assert(_cancelTokenSource == null);
            _cancelTokenSource = new CancellationTokenSource();
        }

        protected override void EndProcessing()
        {
            try
            {
                if (_cancelTokenSource != null)
                {
                    _cancelTokenSource.Dispose();
                }
            }
            catch (Exception e)
            {
                Debug.Assert(false, e.Message);
            }
        }

        protected override void StopProcessing()
        {
            WriteVerbose("Cancelling...");
            Cancel();
        }
    }
}
