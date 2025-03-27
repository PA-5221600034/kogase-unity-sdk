using System.Collections.Generic;
using UnityEngine.Networking;

namespace Kogase.Core.Http
{
    /// <summary>
    /// Interface defining the contract for HTTP requests
    /// </summary>
    public interface IHttpRequest
    {
        /// <summary>
        /// The HTTP method (GET, POST, etc.)
        /// </summary>
        HttpMethod Method { get; }
        
        /// <summary>
        /// The complete URL for the request
        /// </summary>
        string Url { get; }
        
        /// <summary>
        /// Headers to include with the request
        /// </summary>
        Dictionary<string, string> Headers { get; }
        
        /// <summary>
        /// Creates a UnityWebRequest from this request
        /// </summary>
        UnityWebRequest CreateWebRequest();
    }
} 