using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Arh
{
    /// <summary>
    /// Advanced Rest Handler class, A tools that simplifies API calls. <br />
    /// This class by default handle json properties with PascalCase keys, for camelCase or score_case (snake_case) please refer to <see cref="ArhCamelCaseWrapper"/> and <see cref="ArhSnakeCaseWrapper"/>.
    /// </summary>
    public class AdvancedRestHandler
    {
        private readonly string _baseUrl;
        /// <summary>
        /// Provide custom headers that can be set within the request
        /// </summary>
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public List<KeyValuePair<string, IEnumerable<string>>> ClientDefaultHeaders { get; set; }

        /// <summary>
        /// If set, will be used globally for requests
        /// </summary>
        public TimeSpan? GlobalTimeout { get; set; }

        // ReSharper disable once UnusedMember.Global
        /// <summary>
        /// The Constructor
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <param name="fixEndOfUrl">In some web framework, existence or not existence of slash '/' can cause to invoke an incorrect url; 
        /// This feature can be turned of, but requires a manually handling of URL's slash, at the end of base url or begin of partial urls</param>
        [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
        public AdvancedRestHandler(string baseUrl, bool fixEndOfUrl = true)
        {
            if (!string.IsNullOrWhiteSpace(baseUrl) && fixEndOfUrl)
            {
                _baseUrl = baseUrl.TrimEnd('/') + '/';
            }
            else
            {
                _baseUrl = baseUrl;
            }
        }

        /// <summary>
        /// The Constructor
        /// </summary>
        [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
        public AdvancedRestHandler() : this(null)
        {
        }

        /// <summary>
        /// The Constructor
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <param name="options"></param>
        public AdvancedRestHandler(string baseUrl, RestHandlerInitializerOptions options): this(baseUrl, options.FixEndOfUrl)
        {
            
        }

        #region GET

        /// <summary>
        /// Sending a GET Request to the Server and Returns Data
        /// </summary>
        /// <typeparam name="TResponse">Type of Returning Model</typeparam>
        /// <param name="partialUrl">
        /// The Whole or Partial-Variable Part of the URL<br/> 
        /// Start with no Slash (Normally Lead to Generation of A Wrong URL)
        /// </param>
        /// <param name="headers">Provide custom headers that can be set within the request </param>
        /// <param name="useGZip">If We Should Decode Returning Data Using GZip Method</param>
        /// <returns>Returns Instance of Type <see cref="TResponse"/></returns>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public TResponse GetData<TResponse>(string partialUrl, IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers = null, bool useGZip = false)
        {
            return GetData<TResponse>(partialUrl, new RestHandlerRequestOptions
            {
                Headers = headers,
                UseGZip = useGZip
            });
        }

        /// <summary>
        /// Sending a GET Request to the Server and Returns Data
        /// </summary>
        /// <typeparam name="TResponse">Type of Returning Model</typeparam>
        /// <param name="partialUrl">
        /// The Whole or Partial-Variable Part of the URL<br/> 
        /// Start with no Slash (Normally Lead to Generation of A Wrong URL)
        /// </param>
        /// <param name="options">Set of options that affect the way that the request will be sent</param>
        /// <returns>Returns Instance of Type <see cref="TResponse"/></returns>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public TResponse GetData<TResponse>(string partialUrl, RestHandlerRequestOptions options)
        {
            string jsonString;
            HttpStatusCode statusCode;

            using (HttpClient client = GetHttpClient(options))
            {
                client.DefaultRequestHeaders.Accept.Clear();
                //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (options.UseGZip)
                {
                    client.DefaultRequestHeaders.AcceptEncoding.Add(
                        // ReSharper disable once StringLiteralTypo
                        new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
                }

                SetHeaders(options.Headers, client);

                var response = client.GetAsync(partialUrl).Result;
                statusCode = response.StatusCode;

                jsonString = GetResponseContentString(response.Content, options);
            }

            return MakeResponse<TResponse>(null, jsonString, statusCode, options.FallbackModels);
        }

        #endregion GET

        #region POST

        /// <summary>
        /// Sending a POST Request to the Server Without a Request Model and Returns Data
        /// </summary>
        /// <typeparam name="TResponse">Type of Returning Model</typeparam>
        /// <param name="partialUrl">
        /// The Whole or Partial-Variable Part of the URL<br/> 
        /// Start with no Slash (Normally Lead to Generation of A Wrong URL)
        /// </param>
        /// <param name="headers">Provide custom headers that can be set within the request </param>
        /// <param name="useGZip">If We Should Decode Returning Data Using GZip Method</param>
        /// <returns>Returns Instance of Type <see cref="TResponse"/></returns>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public TResponse PostData<TResponse>(string partialUrl, IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers = null, bool useGZip = false)
        {
            return PostData<TResponse>(partialUrl, new RestHandlerRequestOptions
            {
                Headers = headers,
                UseGZip = useGZip
            });
        }

        /// <summary>
        /// Sending a POST Request to the Server Without a Request Model and Returns Data
        /// </summary>
        /// <typeparam name="TResponse">Type of Returning Model</typeparam>
        /// <param name="partialUrl">
        /// The Whole or Partial-Variable Part of the URL<br/> 
        /// Start with no Slash (Normally Lead to Generation of A Wrong URL)
        /// </param>
        /// <param name="options">Set of options that affect the way that the request will be sent</param>
        /// <returns>Returns Instance of Type <see cref="TResponse"/></returns>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public TResponse PostData<TResponse>(string partialUrl, RestHandlerRequestOptions options)
        {
            string jsonString;
            HttpStatusCode statusCode;

            using (HttpClient client = GetHttpClient(options))
            {
                client.DefaultRequestHeaders.Accept.Clear();
                //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (options.UseGZip)
                {
                    client.DefaultRequestHeaders.AcceptEncoding.Add(
                        // ReSharper disable once StringLiteralTypo
                        new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
                }

                SetHeaders(options.Headers, client);

                //var content = new FormUrlEncodedContent(new[]
                //{
                //    new KeyValuePair("", "login")
                //});

                var content = new StringContent(string.Empty);
                var response = client.PostAsync(partialUrl, content).Result;
                statusCode = response.StatusCode;

                jsonString = GetResponseContentString(response.Content, options);
            }

            return MakeResponse<TResponse>(null, jsonString, statusCode, options.FallbackModels);
        }

        /// <summary>
        /// Sending a POST Request to the Server With Data that are not directly serializable
        /// </summary>
        /// <typeparam name="TResponse">Type of Returning Model</typeparam>
        /// <param name="partialUrl">
        /// The Whole or Partial-Variable Part of the URL<br/> 
        /// Start with no Slash (Normally Lead to Generation of A Wrong URL)
        /// </param>
        /// <param name="content">Any type of content that are not serializable with our serializer</param>
        /// <param name="headers">Provide custom headers that can be set within the request </param>
        /// <param name="useGZip">If We Should Decode Returning Data Using GZip Method</param>
        /// <returns>Returns Instance of Type <see cref="TResponse"/></returns>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public TResponse PostNonModelData<TResponse>(string partialUrl, HttpContent content, IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers = null, bool useGZip = false)
        {
            return PostNonModelData<TResponse>(partialUrl, content, new RestHandlerRequestOptions
            {
                UseGZip = useGZip,
                Headers = headers
            });
        }

        /// <summary>
        /// Sending a POST Request to the Server With Data that are not directly serializable
        /// </summary>
        /// <typeparam name="TResponse">Type of Returning Model</typeparam>
        /// <param name="partialUrl">
        /// The Whole or Partial-Variable Part of the URL<br/> 
        /// Start with no Slash (Normally Lead to Generation of A Wrong URL)
        /// </param>
        /// <param name="content">Any type of content that are not serializable with our serializer</param>
        /// <param name="options">Set of options that affect the way that the request will be sent</param>
        /// <returns>Returns Instance of Type <see cref="TResponse"/></returns>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public TResponse PostNonModelData<TResponse>(string partialUrl, HttpContent content, RestHandlerRequestOptions options)
        {
            string jsonString;
            HttpStatusCode statusCode;

            using (HttpClient client = GetHttpClient(options))
            {
                client.DefaultRequestHeaders.Accept.Clear();
                if (options.UseGZip)
                {
                    client.DefaultRequestHeaders.AcceptEncoding.Add(
                        // ReSharper disable once StringLiteralTypo
                        new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
                }

                SetHeaders(options.Headers, client);

                var response = client.PostAsync(partialUrl, content).Result;
                statusCode = response.StatusCode;

                jsonString = GetResponseContentString(response.Content, options);
            }

            return MakeResponse<TResponse>(null, jsonString, statusCode, options.FallbackModels);
        }

        /// <summary>
        /// Sending a POST Request to the Server With a Request Model and Returns Data
        /// </summary>
        /// <typeparam name="TResponse">Type of Returning Model</typeparam>
        /// <typeparam name="TRequest">Type of Sending Model</typeparam>
        /// <param name="partialUrl">
        /// The Whole or Partial-Variable Part of the URL<br/> 
        /// Start with no Slash (Normally Lead to Generation of A Wrong URL)
        /// </param>
        /// <param name="request">The Requesting Model Instance of Type <see cref="TRequest"/></param>
        /// <param name="headers">Provide custom headers that can be set within the request </param>
        /// <param name="useGZip">If We Should Decode Returning Data Using GZip Method</param>
        /// <param name="requestType">Type of request, which affect the way that object will serialize (default json)</param>
        /// <returns>Returns Instance of Type <see cref="TResponse"/></returns>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public TResponse PostData<TResponse, TRequest>(string partialUrl, TRequest request, IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers = null, bool useGZip = false, RequestType requestType = RequestType.Json)
            where TRequest : class, new()
        {
            return PostData<TResponse, TRequest>(partialUrl, request, new RestHandlerRequestOptions
            {
                Headers = headers,
                RequestType = requestType,
                UseGZip = useGZip
            });
        }

        /// <summary>
        /// Sending a POST Request to the Server With a Request Model and Returns Data
        /// </summary>
        /// <typeparam name="TResponse">Type of Returning Model</typeparam>
        /// <typeparam name="TRequest">Type of Sending Model</typeparam>
        /// <param name="partialUrl">
        /// The Whole or Partial-Variable Part of the URL<br/> 
        /// Start with no Slash (Normally Lead to Generation of A Wrong URL)
        /// </param>
        /// <param name="request">The Requesting Model Instance of Type <see cref="TRequest"/></param>
        /// <param name="options">Set of options that affect the way that the request will be sent</param>
        /// <returns>Returns Instance of Type <see cref="TResponse"/></returns>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public TResponse PostData<TResponse, TRequest>(string partialUrl, TRequest request, RestHandlerRequestOptions options)
            where TRequest : class, new()
        {
            string jsonString;
            string requestString;
            HttpStatusCode statusCode;

            using (HttpClient client = GetHttpClient(options))
            {
                client.DefaultRequestHeaders.Accept.Clear();
                //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (options.UseGZip)
                {
                    client.DefaultRequestHeaders.AcceptEncoding.Add(
                        // ReSharper disable once StringLiteralTypo
                        new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
                }

                SetHeaders(options.Headers, client);

                //var content = new FormUrlEncodedContent(new[]
                //{
                //    new KeyValuePair("", "login")
                //});

                // ReSharper disable once RedundantAssignment
                HttpContent content = null;
                content = ObjectSerializer(request, options);
                requestString = content?.ReadAsStringAsync().Result;

                var response = client.PostAsync(partialUrl, content).Result;
                statusCode = response.StatusCode;

                jsonString = GetResponseContentString(response.Content, options);
            }

            return MakeResponse<TResponse>(requestString, jsonString, statusCode, options.FallbackModels);
        }

        #endregion POST

        #region PUT

        /// <summary>
        /// Sending a PUT Request to the Server Without a Request Model and Returns Data
        /// </summary>
        /// <typeparam name="TResponse">Type of Returning Model</typeparam>
        /// <param name="partialUrl">
        /// The Whole or Partial-Variable Part of the URL<br/> 
        /// Start with no Slash (Normally Lead to Generation of A Wrong URL)
        /// </param>
        /// <param name="options">Set of options that affect the way that the request will be sent</param>
        /// <returns>Returns Instance of Type <see cref="TResponse"/></returns>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public TResponse PutData<TResponse>(string partialUrl, RestHandlerRequestOptions options)
        {
            string jsonString;
            HttpStatusCode statusCode;

            using (HttpClient client = GetHttpClient(options))
            {
                client.DefaultRequestHeaders.Accept.Clear();
                //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (options.UseGZip)
                {
                    client.DefaultRequestHeaders.AcceptEncoding.Add(
                        // ReSharper disable once StringLiteralTypo
                        new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
                }

                SetHeaders(options.Headers, client);

                //var content = new FormUrlEncodedContent(new[]
                //{
                //    new KeyValuePair("", "login")
                //});

                var content = new StringContent(string.Empty);
                var response = client.PutAsync(partialUrl, content).Result;
                statusCode = response.StatusCode;

                jsonString = GetResponseContentString(response.Content, options);
            }

            return MakeResponse<TResponse>(null, jsonString, statusCode, options.FallbackModels);
        }

        /// <summary>
        /// Sending a PUT Request to the Server With Data that are not directly serializable
        /// </summary>
        /// <typeparam name="TResponse">Type of Returning Model</typeparam>
        /// <param name="partialUrl">
        /// The Whole or Partial-Variable Part of the URL<br/> 
        /// Start with no Slash (Normally Lead to Generation of A Wrong URL)
        /// </param>
        /// <param name="content">Any type of content that are not serializable with our serializer</param>
        /// <param name="headers">Provide custom headers that can be set within the request </param>
        /// <param name="useGZip">If We Should Decode Returning Data Using GZip Method</param>
        /// <returns>Returns Instance of Type <see cref="TResponse"/></returns>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public TResponse PutNonModelData<TResponse>(string partialUrl, HttpContent content, IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers = null, bool useGZip = false)
        {
            return PutNonModelData<TResponse>(partialUrl, content, new RestHandlerRequestOptions
            {
                UseGZip = useGZip,
                Headers = headers
            });
        }

        /// <summary>
        /// Sending a PUT Request to the Server With Data that are not directly serializable
        /// </summary>
        /// <typeparam name="TResponse">Type of Returning Model</typeparam>
        /// <param name="partialUrl">
        /// The Whole or Partial-Variable Part of the URL<br/> 
        /// Start with no Slash (Normally Lead to Generation of A Wrong URL)
        /// </param>
        /// <param name="content">Any type of content that are not serializable with our serializer</param>
        /// <param name="options">Set of options that affect the way that the request will be sent</param>
        /// <returns>Returns Instance of Type <see cref="TResponse"/></returns>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public TResponse PutNonModelData<TResponse>(string partialUrl, HttpContent content, RestHandlerRequestOptions options)
        {
            string jsonString;
            HttpStatusCode statusCode;

            using (HttpClient client = GetHttpClient(options))
            {
                client.DefaultRequestHeaders.Accept.Clear();
                if (options.UseGZip)
                {
                    client.DefaultRequestHeaders.AcceptEncoding.Add(
                        // ReSharper disable once StringLiteralTypo
                        new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
                }

                SetHeaders(options.Headers, client);

                var response = client.PutAsync(partialUrl, content).Result;
                statusCode = response.StatusCode;

                jsonString = GetResponseContentString(response.Content, options);
            }

            return MakeResponse<TResponse>(null, jsonString, statusCode, options.FallbackModels);
        }

        /// <summary>
        /// Sending a PUT Request to the Server With a Request Model and Returns Data
        /// </summary>
        /// <typeparam name="TResponse">Type of Returning Model</typeparam>
        /// <typeparam name="TRequest">Type of Sending Model</typeparam>
        /// <param name="partialUrl">
        /// The Whole or Partial-Variable Part of the URL<br/> 
        /// Start with no Slash (Normally Lead to Generation of A Wrong URL)
        /// </param>
        /// <param name="request">The Requesting Model Instance of Type <see cref="TRequest"/></param>
        /// <param name="headers">Provide custom headers that can be set within the request </param>
        /// <param name="useGZip">If We Should Decode Returning Data Using GZip Method</param>
        /// <param name="requestType">Type of request, which affect the way that object will serialize (default json)</param>
        /// <returns>Returns Instance of Type <see cref="TResponse"/></returns>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public TResponse PutData<TResponse, TRequest>(string partialUrl, TRequest request, IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers = null, bool useGZip = false, RequestType requestType = RequestType.Json)
            where TRequest : class, new()
        {
            return PutData<TResponse, TRequest>(partialUrl, request, new RestHandlerRequestOptions
            {
                Headers = headers,
                RequestType = requestType,
                UseGZip = useGZip
            });
        }

        /// <summary>
        /// Sending a PUT Request to the Server With a Request Model and Returns Data
        /// </summary>
        /// <typeparam name="TResponse">Type of Returning Model</typeparam>
        /// <typeparam name="TRequest">Type of Sending Model</typeparam>
        /// <param name="partialUrl">
        /// The Whole or Partial-Variable Part of the URL<br/> 
        /// Start with no Slash (Normally Lead to Generation of A Wrong URL)
        /// </param>
        /// <param name="request">The Requesting Model Instance of Type <see cref="TRequest"/></param>
        /// <param name="options">Set of options that affect the way that the request will be sent</param>
        /// <returns>Returns Instance of Type <see cref="TResponse"/></returns>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public TResponse PutData<TResponse, TRequest>(string partialUrl, TRequest request, RestHandlerRequestOptions options)
            where TRequest : class, new()
        {
            string jsonString;
            string requestString;
            HttpStatusCode statusCode;

            using (HttpClient client = GetHttpClient(options))
            {
                client.DefaultRequestHeaders.Accept.Clear();
                //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (options.UseGZip)
                {
                    client.DefaultRequestHeaders.AcceptEncoding.Add(
                        // ReSharper disable once StringLiteralTypo
                        new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
                }

                SetHeaders(options.Headers, client);

                //var content = new FormUrlEncodedContent(new[]
                //{
                //    new KeyValuePair("", "login")
                //});

                // ReSharper disable once RedundantAssignment
                HttpContent content = null;
                content = ObjectSerializer(request, options);
                requestString = content?.ReadAsStringAsync().Result;

                var response = client.PutAsync(partialUrl, content).Result;
                statusCode = response.StatusCode;

                jsonString = GetResponseContentString(response.Content, options);
            }

            return MakeResponse<TResponse>(requestString, jsonString, statusCode, options.FallbackModels);
        }

        #endregion PUT

        #region PATCH

        /// <summary>
        /// Sending a PATCH Request to the Server Without a Request Model and Returns Data
        /// </summary>
        /// <typeparam name="TResponse">Type of Returning Model</typeparam>
        /// <param name="partialUrl">
        /// The Whole or Partial-Variable Part of the URL<br/> 
        /// Start with no Slash (Normally Lead to Generation of A Wrong URL)
        /// </param>
        /// <param name="options">Set of options that affect the way that the request will be sent</param>
        /// <returns>Returns Instance of Type <see cref="TResponse"/></returns>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public TResponse PatchData<TResponse>(string partialUrl, RestHandlerRequestOptions options)
        {
            string jsonString;
            HttpStatusCode statusCode;

            using (HttpClient client = GetHttpClient(options))
            {
                client.DefaultRequestHeaders.Accept.Clear();
                //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (options.UseGZip)
                {
                    client.DefaultRequestHeaders.AcceptEncoding.Add(
                        // ReSharper disable once StringLiteralTypo
                        new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
                }

                SetHeaders(options.Headers, client);

                //var content = new FormUrlEncodedContent(new[]
                //{
                //    new KeyValuePair("", "login")
                //});

                var content = new StringContent(string.Empty);
                HttpRequestMessage reqMsg = new HttpRequestMessage(new HttpMethod("PATCH"), partialUrl);
                reqMsg.Content = content;
                var response = client.SendAsync(reqMsg).Result;
                statusCode = response.StatusCode;

                jsonString = GetResponseContentString(response.Content, options);
            }

            return MakeResponse<TResponse>(null, jsonString, statusCode, options.FallbackModels);
        }

        /// <summary>
        /// Sending a PATCH Request to the Server With Data that are not directly serializable
        /// </summary>
        /// <typeparam name="TResponse">Type of Returning Model</typeparam>
        /// <param name="partialUrl">
        /// The Whole or Partial-Variable Part of the URL<br/> 
        /// Start with no Slash (Normally Lead to Generation of A Wrong URL)
        /// </param>
        /// <param name="content">Any type of content that are not serializable with our serializer</param>
        /// <param name="headers">Provide custom headers that can be set within the request </param>
        /// <param name="useGZip">If We Should Decode Returning Data Using GZip Method</param>
        /// <returns>Returns Instance of Type <see cref="TResponse"/></returns>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public TResponse PatchNonModelData<TResponse>(string partialUrl, HttpContent content, IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers = null, bool useGZip = false)
        {
            return PatchNonModelData<TResponse>(partialUrl, content, new RestHandlerRequestOptions
            {
                UseGZip = useGZip,
                Headers = headers
            });
        }

        /// <summary>
        /// Sending a PATCH Request to the Server With Data that are not directly serializable
        /// </summary>
        /// <typeparam name="TResponse">Type of Returning Model</typeparam>
        /// <param name="partialUrl">
        /// The Whole or Partial-Variable Part of the URL<br/> 
        /// Start with no Slash (Normally Lead to Generation of A Wrong URL)
        /// </param>
        /// <param name="content">Any type of content that are not serializable with our serializer</param>
        /// <param name="options">Set of options that affect the way that the request will be sent</param>
        /// <returns>Returns Instance of Type <see cref="TResponse"/></returns>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public TResponse PatchNonModelData<TResponse>(string partialUrl, HttpContent content, RestHandlerRequestOptions options)
        {
            string jsonString;
            HttpStatusCode statusCode;

            using (HttpClient client = GetHttpClient(options))
            {
                client.DefaultRequestHeaders.Accept.Clear();
                if (options.UseGZip)
                {
                    client.DefaultRequestHeaders.AcceptEncoding.Add(
                        // ReSharper disable once StringLiteralTypo
                        new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
                }

                SetHeaders(options.Headers, client);

                HttpRequestMessage reqMsg = new HttpRequestMessage(new HttpMethod("PATCH"), partialUrl);
                reqMsg.Content = content;
                var response = client.SendAsync(reqMsg).Result;
                statusCode = response.StatusCode;

                jsonString = GetResponseContentString(response.Content, options);
            }

            return MakeResponse<TResponse>(null, jsonString, statusCode, options.FallbackModels);
        }

        /// <summary>
        /// Sending a PATCH Request to the Server With a Request Model and Returns Data
        /// </summary>
        /// <typeparam name="TResponse">Type of Returning Model</typeparam>
        /// <typeparam name="TRequest">Type of Sending Model</typeparam>
        /// <param name="partialUrl">
        /// The Whole or Partial-Variable Part of the URL<br/> 
        /// Start with no Slash (Normally Lead to Generation of A Wrong URL)
        /// </param>
        /// <param name="request">The Requesting Model Instance of Type <see cref="TRequest"/></param>
        /// <param name="headers">Provide custom headers that can be set within the request </param>
        /// <param name="useGZip">If We Should Decode Returning Data Using GZip Method</param>
        /// <param name="requestType">Type of request, which affect the way that object will serialize (default json)</param>
        /// <returns>Returns Instance of Type <see cref="TResponse"/></returns>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public TResponse PatchData<TResponse, TRequest>(string partialUrl, TRequest request, IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers = null, bool useGZip = false, RequestType requestType = RequestType.Json)
            where TRequest : class, new()
        {
            return PatchData<TResponse, TRequest>(partialUrl, request, new RestHandlerRequestOptions
            {
                Headers = headers,
                RequestType = requestType,
                UseGZip = useGZip
            });
        }

        /// <summary>
        /// Sending a PATCH Request to the Server With a Request Model and Returns Data
        /// </summary>
        /// <typeparam name="TResponse">Type of Returning Model</typeparam>
        /// <typeparam name="TRequest">Type of Sending Model</typeparam>
        /// <param name="partialUrl">
        /// The Whole or Partial-Variable Part of the URL<br/> 
        /// Start with no Slash (Normally Lead to Generation of A Wrong URL)
        /// </param>
        /// <param name="request">The Requesting Model Instance of Type <see cref="TRequest"/></param>
        /// <param name="options">Set of options that affect the way that the request will be sent</param>
        /// <returns>Returns Instance of Type <see cref="TResponse"/></returns>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public TResponse PatchData<TResponse, TRequest>(string partialUrl, TRequest request, RestHandlerRequestOptions options)
            where TRequest : class, new()
        {
            string jsonString;
            string requestString;
            HttpStatusCode statusCode;

            using (HttpClient client = GetHttpClient(options))
            {
                client.DefaultRequestHeaders.Accept.Clear();
                //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (options.UseGZip)
                {
                    client.DefaultRequestHeaders.AcceptEncoding.Add(
                        // ReSharper disable once StringLiteralTypo
                        new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
                }

                SetHeaders(options.Headers, client);

                //var content = new FormUrlEncodedContent(new[]
                //{
                //    new KeyValuePair("", "login")
                //});

                // ReSharper disable once RedundantAssignment
                HttpContent content = null;
                content = ObjectSerializer(request, options);
                requestString = content?.ReadAsStringAsync().Result;

                HttpRequestMessage reqMsg = new HttpRequestMessage(new HttpMethod("PATCH"), partialUrl);
                reqMsg.Content = content;
                var response = client.SendAsync(reqMsg).Result;
                statusCode = response.StatusCode;

                jsonString = GetResponseContentString(response.Content, options);
            }

            return MakeResponse<TResponse>(requestString, jsonString, statusCode, options.FallbackModels);
        }

        #endregion Patch

        #region DELETE

        /// <summary>
        /// Sending a DELETE Request to the Server Without a Request Model and Returns Data
        /// </summary>
        /// <typeparam name="TResponse">Type of Returning Model</typeparam>
        /// <param name="partialUrl">
        /// The Whole or Partial-Variable Part of the URL<br/> 
        /// Start with no Slash (Normally Lead to Generation of A Wrong URL)
        /// </param>
        /// <param name="headers">Provide custom headers that can be set within the request </param>
        /// <param name="useGZip">If We Should Decode Returning Data Using GZip Method</param>
        /// <returns>Returns Instance of Type <see cref="TResponse"/></returns>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public TResponse DeleteData<TResponse>(string partialUrl, IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers = null, bool useGZip = false)
        {
            return DeleteData<TResponse>(partialUrl, new RestHandlerRequestOptions
            {
                Headers = headers,
                UseGZip = useGZip
            });
        }

        /// <summary>
        /// Sending a DELETE Request to the Server Without a Request Model and Returns Data
        /// </summary>
        /// <typeparam name="TResponse">Type of Returning Model</typeparam>
        /// <param name="partialUrl">
        /// The Whole or Partial-Variable Part of the URL<br/> 
        /// Start with no Slash (Normally Lead to Generation of A Wrong URL)
        /// </param>
        /// <param name="options">Set of options that affect the way that the request will be sent</param>
        /// <returns>Returns Instance of Type <see cref="TResponse"/></returns>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public TResponse DeleteData<TResponse>(string partialUrl, RestHandlerRequestOptions options)
        {
            string jsonString;
            HttpStatusCode statusCode;

            using (HttpClient client = GetHttpClient(options))
            {
                client.DefaultRequestHeaders.Accept.Clear();
                //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (options.UseGZip)
                {
                    client.DefaultRequestHeaders.AcceptEncoding.Add(
                        // ReSharper disable once StringLiteralTypo
                        new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
                }

                SetHeaders(options.Headers, client);

                //var content = new FormUrlEncodedContent(new[]
                //{
                //    new KeyValuePair("", "login")
                //});

                var content = new StringContent(string.Empty);
                HttpRequestMessage reqMsg = new HttpRequestMessage(HttpMethod.Delete, partialUrl);
                reqMsg.Content = content;
                var response = client.SendAsync(reqMsg).Result;
                statusCode = response.StatusCode;

                jsonString = GetResponseContentString(response.Content, options);
            }

            return MakeResponse<TResponse>(null, jsonString, statusCode, options.FallbackModels);
        }

        /// <summary>
        /// Sending a DELETE Request to the Server With Data that are not directly serializable
        /// </summary>
        /// <typeparam name="TResponse">Type of Returning Model</typeparam>
        /// <param name="partialUrl">
        /// The Whole or Partial-Variable Part of the URL<br/> 
        /// Start with no Slash (Normally Lead to Generation of A Wrong URL)
        /// </param>
        /// <param name="content">Any type of content that are not serializable with our serializer</param>
        /// <param name="headers">Provide custom headers that can be set within the request </param>
        /// <param name="useGZip">If We Should Decode Returning Data Using GZip Method</param>
        /// <returns>Returns Instance of Type <see cref="TResponse"/></returns>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public TResponse DeleteNonModelData<TResponse>(string partialUrl, HttpContent content, IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers = null, bool useGZip = false)
        {
            return DeleteNonModelData<TResponse>(partialUrl, content, new RestHandlerRequestOptions
            {
                UseGZip = useGZip,
                Headers = headers
            });
        }

        /// <summary>
        /// Sending a DELETE Request to the Server With Data that are not directly serializable
        /// </summary>
        /// <typeparam name="TResponse">Type of Returning Model</typeparam>
        /// <param name="partialUrl">
        /// The Whole or Partial-Variable Part of the URL<br/> 
        /// Start with no Slash (Normally Lead to Generation of A Wrong URL)
        /// </param>
        /// <param name="content">Any type of content that are not serializable with our serializer</param>
        /// <param name="options">Set of options that affect the way that the request will be sent</param>
        /// <returns>Returns Instance of Type <see cref="TResponse"/></returns>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public TResponse DeleteNonModelData<TResponse>(string partialUrl, HttpContent content, RestHandlerRequestOptions options)
        {
            string jsonString;
            HttpStatusCode statusCode;

            using (HttpClient client = GetHttpClient(options))
            {
                client.DefaultRequestHeaders.Accept.Clear();
                if (options.UseGZip)
                {
                    client.DefaultRequestHeaders.AcceptEncoding.Add(
                        // ReSharper disable once StringLiteralTypo
                        new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
                }

                SetHeaders(options.Headers, client);

                HttpRequestMessage reqMsg = new HttpRequestMessage(HttpMethod.Delete, partialUrl);
                reqMsg.Content = content;
                var response = client.SendAsync(reqMsg).Result;
                statusCode = response.StatusCode;

                jsonString = GetResponseContentString(response.Content, options);
            }

            return MakeResponse<TResponse>(null, jsonString, statusCode, options.FallbackModels);
        }

        /// <summary>
        /// Sending a DELETE Request to the Server With a Request Model and Returns Data
        /// </summary>
        /// <typeparam name="TResponse">Type of Returning Model</typeparam>
        /// <typeparam name="TRequest">Type of Sending Model</typeparam>
        /// <param name="partialUrl">
        /// The Whole or Partial-Variable Part of the URL<br/> 
        /// Start with no Slash (Normally Lead to Generation of A Wrong URL)
        /// </param>
        /// <param name="request">The Requesting Model Instance of Type <see cref="TRequest"/></param>
        /// <param name="headers">Provide custom headers that can be set within the request </param>
        /// <param name="useGZip">If We Should Decode Returning Data Using GZip Method</param>
        /// <param name="requestType">Type of request, which affect the way that object will serialize (default json)</param>
        /// <returns>Returns Instance of Type <see cref="TResponse"/></returns>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public TResponse DeleteData<TResponse, TRequest>(string partialUrl, TRequest request, IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers = null, bool useGZip = false, RequestType requestType = RequestType.Json)
            where TRequest : class, new()
        {
            return DeleteData<TResponse, TRequest>(partialUrl, request, new RestHandlerRequestOptions
            {
                Headers = headers,
                RequestType = requestType,
                UseGZip = useGZip
            });
        }

        /// <summary>
        /// Sending a DELETE Request to the Server With a Request Model and Returns Data
        /// </summary>
        /// <typeparam name="TResponse">Type of Returning Model</typeparam>
        /// <typeparam name="TRequest">Type of Sending Model</typeparam>
        /// <param name="partialUrl">
        /// The Whole or Partial-Variable Part of the URL<br/> 
        /// Start with no Slash (Normally Lead to Generation of A Wrong URL)
        /// </param>
        /// <param name="request">The Requesting Model Instance of Type <see cref="TRequest"/></param>
        /// <param name="options">Set of options that affect the way that the request will be sent</param>
        /// <returns>Returns Instance of Type <see cref="TResponse"/></returns>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public TResponse DeleteData<TResponse, TRequest>(string partialUrl, TRequest request, RestHandlerRequestOptions options)
            where TRequest : class, new()
        {
            string jsonString;
            string requestString;
            HttpStatusCode statusCode;

            using (HttpClient client = GetHttpClient(options))
            {
                client.DefaultRequestHeaders.Accept.Clear();
                //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (options.UseGZip)
                {
                    client.DefaultRequestHeaders.AcceptEncoding.Add(
                        // ReSharper disable once StringLiteralTypo
                        new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
                }

                SetHeaders(options.Headers, client);

                //var content = new FormUrlEncodedContent(new[]
                //{
                //    new KeyValuePair("", "login")
                //});

                // ReSharper disable once RedundantAssignment
                HttpContent content = null;
                content = ObjectSerializer(request, options);
                requestString = content?.ReadAsStringAsync().Result;

                HttpRequestMessage reqMsg = new HttpRequestMessage(HttpMethod.Delete, partialUrl);
                reqMsg.Content = content;
                var response = client.SendAsync(reqMsg).Result;
                statusCode = response.StatusCode;

                jsonString = GetResponseContentString(response.Content, options);
            }

            return MakeResponse<TResponse>(requestString, jsonString, statusCode, options.FallbackModels);
        }

        #endregion DELETE

        #region HelperMethods

        /// <summary>
        /// Create new Instance of the HttpClient depend on the initialization conditions
        /// </summary>
        /// <returns></returns>
        private HttpClient GetHttpClient(RestHandlerRequestOptions requestOptions)
        {
            HttpClient httpClient;
            if (!string.IsNullOrWhiteSpace(_baseUrl))
            {
                httpClient = new HttpClient
                {
                    BaseAddress = new Uri(_baseUrl)
                };
            }
            else
            {
                httpClient = new HttpClient();
            }

            if (requestOptions.Timeout.HasValue)
                httpClient.Timeout = requestOptions.Timeout.Value;
            else if (GlobalTimeout.HasValue)
                httpClient.Timeout = GlobalTimeout.Value;

            return httpClient;
        }

        /// <summary>
        /// Convert object to "Url Encoded" Key Value
        /// Based On: https://geeklearning.io/serialize-an-object-to-an-url-encoded-string-in-csharp/
        /// </summary>
        /// <param name="metaToken"></param>
        /// <returns></returns>
        [SuppressMessage("ReSharper", "ConstantConditionalAccessQualifier")]
        private static IDictionary<string, string> ToKeyValue(object metaToken)
        {
            if (metaToken == null)
            {
                return null;
            }

            JToken token = metaToken as JToken;
            if (token == null)
            {
                return ToKeyValue(JObject.FromObject(metaToken));
            }

            if (token.HasValues)
            {
                var contentData = new Dictionary<string, string>();
                foreach (var child in token.Children().ToList())
                {
                    var childContent = ToKeyValue(child);
                    if (childContent != null)
                    {
                        contentData = contentData.Concat(childContent)
                            .ToDictionary(k => k.Key, v => v.Value);
                    }
                }

                return contentData;
            }

            var jValue = token as JValue;
            if (jValue?.Value == null)
            {
                return null;
            }

            var value = jValue?.Type == JTokenType.Date ?
                jValue?.ToString("o", CultureInfo.InvariantCulture) :
                jValue?.ToString(CultureInfo.InvariantCulture);

            return new Dictionary<string, string> { { token.Path, value } };
        }

        /// <summary>
        /// Set Headers inside the client
        /// </summary>
        /// <param name="headers"></param>
        /// <param name="client"></param>
        private void SetHeaders(IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers, HttpClient client)
        {
            //Set Global Headers
            if (ClientDefaultHeaders != null)
            {
                foreach (var pair in ClientDefaultHeaders)
                {
                    client.DefaultRequestHeaders.Add(pair.Key, pair.Value);
                }
            }
            //Set Local Headers
            if (headers != null)
            {
                foreach (var pair in headers)
                {
                    client.DefaultRequestHeaders.Add(pair.Key, pair.Value);
                    // Content-Type -> req.Content = new StringContent(rcString, Encoding.UTF8, "application/json");
                    // client.DefaultRequestHeaders.TryAddWithoutValidation(pair.Key, pair.Value);
                }
            }
        }

        /// <summary>
        /// Make response based on type of data
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="requestString">request text</param>
        /// <param name="responseString">response text</param>
        /// <param name="statusCode">result status code</param>
        /// <param name="fallbackTypes">type of data other that return type, that may match with incoming object</param>
        /// <returns></returns>
        private TResponse MakeResponse<TResponse>(string requestString, string responseString,
            HttpStatusCode statusCode, List<Type> fallbackTypes)
        {
            // What to do if response is merely just a string?
            if (typeof(TResponse) == typeof(string))
            {
                return (TResponse)Convert.ChangeType(responseString, typeof(TResponse));
            }
            //What to do if response is RestHandlerResponse?
            if (typeof(TResponse) == typeof(ArhResponse) || typeof(TResponse).IsSubclassOf(typeof(ArhResponse)))
            {
                var result = default(TResponse);
                try
                {
                    //It's Normal Deserialization (So we do not need to pre-initialize it)
                    if (typeof(TResponse) != typeof(ArhStringResponse) &&
                        !typeof(TResponse).IsSubclassOf(typeof(ArhResponse)))
                    {
                        result = DeserializeToObject<TResponse>(responseString);
                    }
                    //It's Special Deserialization using RhResponse
                    else
                    {
                        //It contains just string value, And will sits over RhStringResponse (So we need to initialize Model ourselves)
                        if (typeof(TResponse) == typeof(ArhStringResponse))
                        {
                            result = GenerateTResponseRhResponse<TResponse>();
                        }
                        //It contains model data, But sits inside RhResponse Model (So we need to initialize Model ourselves)
                        else if (typeof(TResponse).IsGenericType &&
                                 typeof(TResponse).GetGenericTypeDefinition() == typeof(ArhResponse<>))
                        {
                            result = GenerateTResponseRhResponse<TResponse>();
                            //if Generic Model is String
                            var genericTypeArgument = typeof(TResponse).GenericTypeArguments[0];
                            if (genericTypeArgument == typeof(string))
                            {
                                SetValueOnProperty(result, nameof(ArhResponse<object>.ResultModel), responseString);
                            }
                            //if Generic Model is Object
                            else
                            {
                                // ReSharper disable once PossibleNullReferenceException
                                object output =
                                    DeserializeToType(responseString, genericTypeArgument);

                                SetValueOnProperty(result, nameof(ArhResponse<object>.ResultModel), output);
                                //SetValueOnProperty(result, nameof(RhResponse<object>.ResultModel),
                                //    DeserializeToObject<TResponse>(jsonString);
                            }
                        }
                        //It contains model data, and uses an inherited version of RhResponse (So we do not need to pre-initialize it)
                        else
                        {
                            result = DeserializeToObject<TResponse>(responseString);
                        }
                    }
                }
                catch (Exception ex)
                {
                    result = GenerateTResponseRhResponse<TResponse>();

                    object fallbackResult = null;
                    Dictionary<Type, Exception> fallbackExceptions = new Dictionary<Type, Exception>();
                    foreach (var fallbackType in fallbackTypes)
                    {
                        try
                        {
                            if (fallbackType == typeof(string))
                            {
                                fallbackResult = Convert.ChangeType(responseString, typeof(string));
                            }
                            else if (requestString.StartsWith("[") || responseString.StartsWith("{"))
                            {
                                fallbackResult = DeserializeToType(responseString, fallbackType);
                            }

                            SetValueOnProperty(result, nameof(ArhResponse.FallbackModel), fallbackResult);
                            break;
                        }
                        catch (Exception exF)
                        {
                            fallbackExceptions.Add(fallbackType, exF);
                        }
                    }


                    ArhException arhEx = new ArhException(
                        fallbackTypes.Any()
                            ? "An Error occurred while deserializing the data. Please look at the inner exception and fallback exceptions for more details."
                            : "An Error occurred while deserializing Data. Please look at inner exception for the details.",
                        ex,
                        fallbackExceptions
                    );
                    SetValueOnProperty(result, nameof(ArhResponse.Exception), arhEx);
                }
                finally
                {
                    if (result != null)
                    {
                        SetValueOnProperty(result, nameof(ArhResponse.ResponseStatusCode), statusCode);
                        SetValueOnProperty(result, nameof(ArhResponse.ResponseText), responseString);
                        SetValueOnProperty(result, nameof(ArhResponse.RequestText), requestString);
                    }
                }
                return result;
            }

            //Normal (Non-ARH Model) Deserialization of Response
            return DeserializeToObject<TResponse>(responseString);
        }

        /// <summary>
        /// Get string content from HttpContent of the response object
        /// </summary>
        /// <param name="content"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        private string GetResponseContentString(HttpContent content, RestHandlerRequestOptions options)
        {
            string result;
            // ReSharper disable once StringLiteralTypo
            if (content.Headers.ContentEncoding.Any(x => x == "gzip"))
            {
                using (Stream stream = content.ReadAsStreamAsync().Result)
                using (Stream decompressed = new GZipStream(stream, CompressionMode.Decompress))
                using (StreamReader reader = options.StringResponseEncoding == null
                    ? new StreamReader(decompressed)
                    : new StreamReader(decompressed, options.StringResponseEncoding))
                {
                    result = reader.ReadToEnd();
                }
            }
            else
            {
                //result = content.ReadAsStringAsync().Result;
                using (Stream stream = content.ReadAsStreamAsync().Result)
                using (StreamReader reader = options.StringResponseEncoding == null
                    ? new StreamReader(stream)
                    : new StreamReader(stream, options.StringResponseEncoding))
                {
                    result = reader.ReadToEnd();
                }
            }
            return result;
        }

        /// <summary>
        /// Change Deserializer On Overriden Class
        /// </summary>
        /// <param name="jsonString"></param>
        /// <param name="genericTypeArgument"></param>
        /// <returns></returns>
        [SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
        protected virtual object DeserializeToType(string jsonString, Type genericTypeArgument)
        {
            return typeof(JsonConvert)
                .GetMethod(nameof(JsonConvert.DeserializeObject), new[] { typeof(string), typeof(Type) })
                //?.MakeGenericMethod(genericTypeArgument)
                .Invoke(null, new object[] { jsonString, genericTypeArgument });
        }

        /// <summary>
        /// Change Deserializer On Overridden Class
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        [SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
        protected virtual TResponse DeserializeToObject<TResponse>(string jsonString)
        {
            return JsonConvert.DeserializeObject<TResponse>(jsonString);
        }

        /// <summary>
        /// Change Serializer which include all provided serializer On Overridden Class
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="req"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        [SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
        protected virtual HttpContent ObjectSerializer<TRequest>(TRequest req, RestHandlerRequestOptions options)
        {
            if (options.RequestType == RequestType.Json)
            {
                var jsonRequest = JsonSerializeObject(req, options);
                var encoding = Encoding.UTF8;
                if (options.StringContentEncoding != null)
                {
                    encoding = options.StringContentEncoding;
                }

                var stringContent = new StringContent(jsonRequest, encoding, "application/json");

                if (options.OmitContentTypeCharSet)
                {
                    stringContent.Headers.ContentType.CharSet = null;
                }
                return stringContent;
            }
            //else if (options.RequestType == RequestType.Xml)
            //{
            //    // TODO: XML Serializer
            //}
            if (options.RequestType == RequestType.FormUrlEncoded)
            {
#warning ...???
                var keyValueOnObject = ToKeyValue(req);
                var keyValueRequest = new List<KeyValuePair<string, string>>();
                foreach (KeyValuePair<string, string> p in keyValueRequest)
                {
                    if (p.Value != null)
                        keyValueRequest.Add(new KeyValuePair<string, string>(p.Key, p.Value));
                }
                //var content = new StringContent(keyValueRequest, Encoding.UTF8, "application/x-www-form-urlencoded");
                return new FormUrlEncodedContent(keyValueRequest);
            }

            throw new IndexOutOfRangeException(
                "The requested method of data serialization is not implemented within the system.");
        }

        /// <summary>
        /// Override JsonSerializer which is called within the ObjectSerializer
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="req"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        [SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
        protected virtual string JsonSerializeObject<TRequest>(TRequest req, RestHandlerRequestOptions options)
        {
            return JsonConvert.SerializeObject(req);
        }

        /// <summary>
        /// Set Value on Public Instance Property
        /// </summary>
        /// <param name="object"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        private static void SetValueOnProperty(object @object, string propertyName, object value)
        {
            @object.GetType()
                .GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public)
                ?.SetValue(@object, value);
        }

        /// <summary>
        /// Generate TResponse of RhResponse type
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <returns></returns>
        private static TResponse GenerateTResponseRhResponse<TResponse>()
        {
            var constructorInfo = typeof(TResponse).GetConstructor(new Type[0]);
            if (constructorInfo == null)
            {
                throw new NotImplementedException(
                    "Constructor is not supported, TResponse requires a public constructor with no parameters.");
            }
            var result = (TResponse)Convert.ChangeType(
                constructorInfo.Invoke(new object[0]),
                typeof(TResponse));
            return result;
        }

        #endregion HelperMethods
    }
}
