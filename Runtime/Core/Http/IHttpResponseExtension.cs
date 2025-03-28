using Kogase.Models;

namespace Kogase.Core
{
    public static class IHttpResponseExtension
    {
        public static KogasePromise<TOkType, TErrorType> ToPromise<TOkType, TErrorType>(this IHttpResponse response)
        {
            // KogasePromise<TOkType, TErrorType> promise = new KogasePromise<TOkType, TErrorType>();
            //
            // Error error = HttpParser.ParseError(response);
            //
            // if (response == null)
            // {
            //     
            // }

            return null;
        }
    }
}