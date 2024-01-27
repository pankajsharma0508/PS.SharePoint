using Microsoft.SharePoint.Client;
using PS.SharePoint.Core.Entities;
using PS.SharePoint.Core.Interfaces;
using System.Runtime.InteropServices;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

#if NETSTANDARD1_0_OR_GREATER
using System.Text.Json;
using System.Net.Http;
using System.Net.Http.Headers;
#endif

namespace PS.SharePoint.Core
{
    public class SharePointContextManager : IContextManager
    {
        public SharePointConfiguration Configuration { get; set; }
        public SharePointContextManager(SharePointConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ExecuteQuery(string message, Action<ClientContext> action)
        {
            string url = string.Empty;
            string correlationId = string.Empty;
            try
            {
                using (var clientContext = GetClientContext())
                {
                    url = clientContext.Url;
                    correlationId = clientContext.TraceCorrelationId;
                    action(clientContext);
                }
            }
            catch (Exception ex)
            {
                throw new SpException(message, correlationId, url, ex);
            }
        }

        public void ExecuteQuery(ClientContext ctx, string message)
        {
            ctx.ExecuteQuery();
        }

        public void CheckInDocument(ClientContext clientContext, File file)
        {
            if (file != null && file.LockedByUser != null)
            {
                var message = "Creating major version overiding minor version";
                file.CheckIn(message, CheckinType.OverwriteCheckIn);
                ExecuteQuery(clientContext, message);
            }
        }

        private ClientContext GetClientContext()
        {
            var ctx = new ClientContext(new Uri(this.Configuration.SharePointUrl));

        #if NETSTANDARD1_0_OR_GREATER
            if (!RuntimeInformation.FrameworkDescription.Contains(".NET Framework"))
            {
                ctx.ExecutingWebRequest += new EventHandler<WebRequestEventArgs>(AddWindowsAuthRequestHeader);
            }
        #endif
            return ctx;
        }

        #if NETSTANDARD1_0_OR_GREATER

        private void AddWindowsAuthRequestHeader(object sender, WebRequestEventArgs e)
        {
            try
            {
                var token = GetDigestValueAsync(new CancellationToken()).Result;
                e.WebRequestExecutor.WebRequest.UseDefaultCredentials = true;
                e.WebRequestExecutor.RequestHeaders["X-RequestDigest"] = token;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        private async Task<string> GetDigestValueAsync(CancellationToken cancellationToken)
        {
            var endpointUrl = $"{this.Configuration.SharePointUrl}/_api/contextinfo";
            var handler = new HttpClientHandler
            {
                UseDefaultCredentials = true,
                PreAuthenticate = true
            };
            var client = new HttpClient(handler);

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await client.PostAsync(endpointUrl, null, cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error fetching digest value: {response.ReasonPhrase}");

            var responseContent = await response.Content.ReadAsStreamAsync();
            var sharePointResp = await JsonSerializer.DeserializeAsync<DigestRoot>(responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, cancellationToken);

            return sharePointResp?.FormDigestValue ?? throw new Exception("Failed to extract FormDigestValue");
        }

        #endif

    }

#if NETSTANDARD1_0_OR_GREATER
    public class DigestRoot
    {
        public string odatametadata { get; set; }
        public int FormDigestTimeoutSeconds { get; set; }
        public string FormDigestValue { get; set; }
        public string LibraryVersion { get; set; }
        public string SiteFullUrl { get; set; }
        public List<string> SupportedSchemaVersions { get; set; }
        public string WebFullUrl { get; set; }
    }

#endif

}
