using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Newtonsoft.Json;

namespace Arh
{
    /// <summary>
    /// Provide more information when the model returns, when inherited from
    /// </summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public abstract class ArhResponse
    {
        /// <summary>
        /// Won't Link to any JSON property
        /// </summary>
        [JsonIgnore]
        public HttpStatusCode ResponseStatusCode { get; set; }

        /// <summary>
        /// Won't Link to any JSON property
        /// </summary>
        [JsonIgnore]
        public string RequestText { get; set; }

        /// <summary>
        /// Won't Link to any JSON property
        /// </summary>
        [JsonIgnore]
        public string ResponseText { get; set; }

        /// <summary>
        /// Won't Link to any JSON property
        /// </summary>
        [JsonIgnore]
        public Exception Exception { get; set; }

        /// <summary>
        /// Won't Link to any JSON property<br/>
        /// if main model doesn't match, try to convert to requested model, defined within the request options.
        /// </summary>
        [JsonIgnore]
        public object FallbackModel { get; set; }
    }


    /// <summary>
    /// provide more information and store it in the given type inside the model
    /// <br/>
    /// In case the class is sealed, or we are unable to inherit from RhResponse
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public sealed class ArhResponse<TResponse> : ArhResponse
    {
        /// <summary>
        /// Won't Link to any JSON property
        /// </summary>
        [JsonIgnore]
        public TResponse ResultModel { get; set; }
    }

    /// <summary>
    /// To get direct response, or non-serialized text content
    /// </summary>
    public class ArhStringResponse : ArhResponse
    {
    }
}