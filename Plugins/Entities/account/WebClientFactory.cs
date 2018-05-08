using System.Diagnostics.CodeAnalysis;

namespace Plugins.Entities.account
{
    [ExcludeFromCodeCoverage]
    class WebClientFactory
    {
        public static IWebClient Create()
        {
            return new MyWebClient();
        }
    }
}