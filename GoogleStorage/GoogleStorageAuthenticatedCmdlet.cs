using System;
using System.Threading;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Security;

using DynamicRestProxy.PortableHttpClient;

namespace GoogleStorage
{
    public abstract class GoogleStorageAuthenticatedCmdlet : GoogleStorageCmdlet
    {
        [Parameter(Mandatory = false)]
        public SwitchParameter NoAuth { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter Persist { get; set; }

        protected DynamicRestClient CreateClient()
        {
            if (NoAuth)
            {
                return new DynamicRestClient("https://www.googleapis.com/");
            }

            return new DynamicRestClient("https://www.googleapis.com/", null, async (request, cancelToken) =>
            {
                var authToken = await GetAccessToken(cancelToken);
                request.Headers.Authorization = new AuthenticationHeaderValue("OAuth", authToken);
            });
        }


        protected async Task<string> GetAccessToken(CancellationToken cancelToken)
        {
            Debug.Assert(!NoAuth, "You should really check NoAuth prior to calling this method");

            if (NoAuth)
            {
                return "";
            }

            var access = this.GetPersistedVariableValue<dynamic>("access", d =>
                {
                    d.access_token = ((string)d.access_token).FromEncyptedString();
                    return d;
                });

            if (access == null)
            {
                throw new AccessViolationException("Access token not set. Call Grant-GoogleStorageAuth first");
            }

            if (DateTime.UtcNow >= access.expiry)
            {
                var oauth = new GoogleOAuth2("https://www.googleapis.com/auth/devstorage.read_write");
                access = await oauth.RefreshAccessToken(access, GetConfig(), cancelToken);
                SetPersistedVariableValue("access", access, Persist);
            }

            SecureString token = access.access_token;
            return token.ToUnsecureString();
        }
    }
}
