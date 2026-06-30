using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;

namespace SALT.WebApi.Template.Net;

/// <summary>
/// An example client service, that relies on the <see cref="HttpClient"/> instance.
/// </summary>
/// <param name="client">The given <see cref="HttpClient"/> instance.</param>
public class ExampleHttpClient(HttpClient client)
{
    private readonly HttpClient _client = client;

    /// <summary>
    /// Returns an <see cref="IAsyncEnumerable{T}"/> of <see cref="int"/>s.
    /// </summary>
    public IAsyncEnumerable<int> GetCommentsAsync(CancellationToken cancellationToken = default) =>
        _client.GetFromJsonAsAsyncEnumerable<int>("/int", cancellationToken);
}