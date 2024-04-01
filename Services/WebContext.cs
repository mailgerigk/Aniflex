using Aniflex.Core;
using Aniflex.Diagnostic;

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Aniflex.Services;

public sealed class WebContext
{
    private static readonly HttpClient client = new();
    private static readonly WebCache cache = new();

    public void ClearCache()
    {
        cache.Clear();
    }

    public async Task<string> Get(string uri)
    {
        for (int i = 0; i < 20; i++)
        {
            try
            {
                return await cache.Get(uri, async () =>
                {
                    return await client.GetStringAsync(uri);
                }, TimeSpan.FromDays(1));
            }
            catch (HttpRequestException ex)
            {
                Log.Default.Exception(ex);
                if (ex.StatusCode == HttpStatusCode.BadGateway)
                    continue;
                throw;
            }
        }
        throw new NotImplementedException();
    }

    public async Task DownloadFile(string uri, string localFile)
    {
        byte[] bytes = await client.GetByteArrayAsync(uri);
        EnsurePath.Exists(localFile);
        for (int i = 0; i < 20; i++)
        {
            try
            {
                File.WriteAllBytes(localFile, bytes);
                return;
            }
            catch (IOException ex)
            {
                Log.Default.Exception(ex);
                if (ex.Message.StartsWith("The process cannot access the file '") && ex.Message.EndsWith("' because it is being used by another process."))
                {
                    Thread.Sleep(100);
                }
                else
                {
                    throw;
                }
            }
        }

    }

    public async Task<string> Post(string uri, string content)
    {
        HttpResponseMessage response = await client.PostAsync(uri, new StringContent(content));
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
