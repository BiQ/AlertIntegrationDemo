using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace BiQ.AlertIntegrationDemo
{
    public class BiqAuthenticationHandler : DelegatingHandler
    {
        private readonly SemaphoreSlim semaphoreLock = new(1, 1);
        private readonly string apiKey;
        private string accessToken;

        public BiqAuthenticationHandler(
            Uri authUrl, 
            string apiKey, 
            HttpMessageHandler innerHandler) : base(innerHandler)
        {
            AuthUrl = authUrl ?? throw new ArgumentNullException(nameof(authUrl));
            this.apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            this.accessToken = "";
        }

        public Uri AuthUrl { get; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                await RequestAndRefreshAccessToken(accessToken, cancellationToken);
            }
            HttpRequestMessage authenticatedRequest = CreateAuthenticatedRequest(request);
            HttpResponseMessage response = await base.SendAsync(authenticatedRequest, cancellationToken);
            if (response.StatusCode != HttpStatusCode.Unauthorized)
                return response;

            if (authenticatedRequest.Headers.Authorization?.Parameter is null)
                throw new InvalidOperationException("Unable to read authorization token from HTTP header.");

            await RequestAndRefreshAccessToken(
                authenticatedRequest.Headers.Authorization.Parameter, cancellationToken);

            HttpRequestMessage retryRequest = CreateAuthenticatedRequest(request);
            response = await base.SendAsync(retryRequest, cancellationToken);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new InvalidOperationException("Unable to get access token in second try.");
            }
            return response;
        }

        private async Task RequestAndRefreshAccessToken(
            string triedAccessToken,
            CancellationToken cancellationToken)
        {
            // Using semaphore to limit refreshing access tokens to only one try at a time
            // SemaphoreSlim is used because it supports async
            try
            {
                await semaphoreLock.WaitAsync(cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                    return;
                if (triedAccessToken == accessToken)
                {
                    HttpResponseMessage tokenResponseInit = await RequestToken(cancellationToken);
                    await RefreshAccessToken(tokenResponseInit);
                }
            }
            finally
            {
                semaphoreLock.Release();
            }
        }

        private HttpRequestMessage CreateAuthenticatedRequest(HttpRequestMessage request)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            return request;
        }

        private async Task RefreshAccessToken(HttpResponseMessage tokenResponse)
        {
            string json = await tokenResponse.Content.ReadAsStringAsync();
            dynamic? jsonObject = JsonConverter.DeserializeAsDynamic(json);
            string? newToken = jsonObject?.access_token;
            if (string.IsNullOrWhiteSpace(newToken))
            {
                throw new InvalidOperationException("Could not refresh access token.");
            }
            accessToken = newToken;
        }

        private async Task<HttpResponseMessage> RequestToken(CancellationToken cancellationToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, AuthUrl);
            var content = 
                new StringContent(JsonConvert.SerializeObject(new { apikey = apiKey }));
            if (content.Headers.ContentType is null)
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            else
                content.Headers.ContentType.MediaType = "application/json";
            request.Content = content;
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
