using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Arh
{
    /// <summary>
    /// A Sets of configuration that can be passed to each methods in <see cref="AdvancedRestHandler">AdvancedRestHandler</see> instance, for modifying the way request should be processed.
    /// </summary>
    [SuppressMessage("ReSharper", "RedundantDefaultMemberInitializer")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
    public class RestHandlerRequestOptions
    {
        /// <summary>
        /// Provide custom headers that can be set within the request
        /// </summary>
        public IEnumerable<KeyValuePair<string, IEnumerable<string>>> Headers { get; set; } = null;


        /// <summary>
        /// Set Timeout of the request
        /// </summary>
        public TimeSpan? Timeout { get; set; }

        /// <summary>
        /// If We Should Decode Returning Data Using GZip Method
        /// </summary>
        public bool UseGZip { get; set; } = false;

        /// <summary>
        /// Type of request, which affect the way that object will serialize (default json)
        /// </summary>
        public RequestType RequestType { get; set; } = RequestType.Json;

        /// <summary>
        /// If the request is not convert-able to the provided type, then we can try to convert it to this type instead
        /// </summary>
        public List<Type> FallbackModels { get; set; } = new List<Type>();

        /// <summary>
        /// Use another encoding for the content passed to StringContent Object used by JSON serializer
        /// </summary>
        public Encoding StringContentEncoding { get; set; } = null;

        /// <summary>
        /// Use another encoding for the content passed to StringContent Object used by JSON serializer
        /// </summary>
        public Encoding StringResponseEncoding { get; set; } = null;

        /// <summary>
        /// Remove default "; charset=utf-8" from Content-Type in header
        /// </summary>
        public bool OmitContentTypeCharSet { get; set; } = false;
    }
}