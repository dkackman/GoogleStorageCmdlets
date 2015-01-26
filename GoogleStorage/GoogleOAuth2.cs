using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Security;

using Microsoft.CSharp.RuntimeBinder;

using DynamicRestProxy.PortableHttpClient;

namespace GoogleStorage
{
    /// <summary>
    /// Helper class to deal with google oauth for unit testing
    /// The first time a machine authenticates user interaction is required
    /// Subsequent unit test runs will use a stored token that will refresh with google
    /// </summary>
    public sealed class GoogleOAuth2 : IDisposable
    {
        // the set of scopes to authorize
        private string _scope;
        private dynamic _google;

        public GoogleOAuth2(string scope)
        {
            Debug.Assert(!string.IsNullOrEmpty(scope));

            _google = new DynamicRestClient("https://accounts.google.com/o/oauth2/");
            _scope = scope;
        }

        public void Dispose()
        {
            ((IDisposable)_google).Dispose();
        }

        /// <summary>
        /// Begines the device oauth flow
        /// </summary>
        /// <param name="clientId">The app clientId</param>
        /// <returns>dynamic object with the verfication_url and user_code</returns>
        public async Task<dynamic> StartAuthentication(string clientId)
        {
            return await StartAuthentication(clientId, CancellationToken.None);
        }

        /// <summary>
        /// Begines the device oauth flow
        /// </summary>
        /// <param name="clientId">The app clientId</param>
        /// <param name="cancelToken">async cancellation token</param>
        /// <returns>dynamic object with the verfication_url and user_code</returns>
        public async Task<dynamic> StartAuthentication(string clientId, CancellationToken cancelToken)
        {
            return await _google.device.code.post(cancelToken, client_id: clientId, scope: _scope);
        }

        /// <summary>
        /// Refreshes an expired access_token
        /// </summary>
        /// <param name="access">dynamic object with current access_token and the refresh_token</param>
        /// <param name="config">dynamic object with clientid and client secret</param>
        /// <param name="cancelToken">async cancellation token</param>
        /// <returns>refeshed acess_token and the refresh_token</returns>
        public async Task<dynamic> RefreshAccessToken(dynamic access, dynamic config, CancellationToken cancelToken)
        {
            SecureString clientSecret = config.ClientSecret;

            var response = await _google.token.post(cancelToken, client_id: config.ClientId, client_secret: clientSecret.ToUnsecureString(), refresh_token: access.refresh_token, grant_type: "refresh_token");

            response.refresh_token = access.refresh_token; // the new access token doesn't have a new refresh token so move our current one here for long term storage
            response.expiry = DateTime.UtcNow + TimeSpan.FromSeconds(response.expires_in);

            // replace the plain text access_token with a securestring
            response.access_token = ((string)response.access_token).ToSecureString();

            return response;
        }

        /// <summary>
        /// Here we spin wait and poll google to see if the user has authenticated and authorized the app
        /// Once the auth code is supplied to google, the proper access_token and refresh_token are returned here
        /// </summary>
        /// <returns></returns>
        public async Task<dynamic> WaitForConfirmation(dynamic confirmToken, string clientId, SecureString clientSecret, CancellationToken cancelToken)
        {
            long expiration = confirmToken.expires_in;
            long interval = confirmToken.interval;
            long time = interval;

            // we are using the device flow so enter the code in the browser
            // here poll google for success
            while (time < expiration)
            {
                Thread.Sleep((int)interval * 1000);
                cancelToken.ThrowIfCancellationRequested();

                dynamic tokenResponse = await _google.token.post(cancelToken, client_id: clientId, client_secret: clientSecret.ToUnsecureString(), code: confirmToken.device_code, grant_type: "http://oauth.net/grant_type/device/1.0");
                try
                {
                    if (tokenResponse.access_token != null)
                    {
                        tokenResponse.expiry = DateTime.UtcNow + TimeSpan.FromSeconds(tokenResponse.expires_in);
                        // replace the plain text access_token with a securestring
                        tokenResponse.access_token = ((string)tokenResponse.access_token).ToSecureString();
                        return tokenResponse;
                    }
                }
                catch (RuntimeBinderException)
                {
                }

                time += interval;
            }

            throw new OperationCanceledException("Authorization from user timed out");
        }
    }
}
