using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Authentication;

namespace Arh
{
    /// <summary>
    /// A Sets of configuration that can be passed to constructor of <see cref="AdvancedRestHandler">AdvancedRestHandler</see> class for initialization.
    /// </summary>
    [SuppressMessage("ReSharper", "RedundantDefaultMemberInitializer")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class RestHandlerInitializerOptions
    {
        /// <summary>
        /// In some web framework, existence or not existence of slash '/' can cause to invoke an incorrect url; 
        /// This feature can be turned of, but requires a manually handling of URL's slash, at the end of base url or begin of partial urls
        /// </summary>
        /// <remarks>
        /// Supports only when creating HttpClient internal.<br/>
        /// It is not supported when providing `HttpClient`, or `IHttpClientFactory` 
        /// </remarks>
        public bool FixEndOfUrl { get; set; }

        /// <summary>
        /// Set global Timeout of requests
        /// </summary>
        public TimeSpan Timeout { get; set; }
        
        /// <summary>
        /// Set global SslProtocols
        /// </summary>
        public SslProtocols? SslProtocols { get; set; } = null;
    }
}