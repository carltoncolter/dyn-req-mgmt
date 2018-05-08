using System;
using System.Net;

namespace Plugins.Entities.account
{
    public interface IWebClient : IDisposable
    {
        WebHeaderCollection Headers { get; }

        // Required methods (subset of `System.Net.WebClient` methods).
        byte[] DownloadData(Uri address);
        byte[] UploadData(string address, string method, byte[] data);
    }
}
