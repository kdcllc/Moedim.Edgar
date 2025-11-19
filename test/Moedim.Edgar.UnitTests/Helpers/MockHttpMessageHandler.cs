using System.Net;

namespace Moedim.Edgar.UnitTests.Helpers;

/// <summary>
/// Mock HTTP message handler for testing HTTP requests
/// </summary>
public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Queue<HttpResponseMessage> _responses = new();
    private HttpResponseMessage? _defaultResponse;
    private TimeSpan _delay = TimeSpan.Zero;

    public int RequestCount { get; private set; }

    public void SetResponse(HttpStatusCode statusCode, string? content = null)
    {
        _defaultResponse = new HttpResponseMessage(statusCode);
        if (content != null)
        {
            _defaultResponse.Content = new StringContent(content);
        }
    }

    public void SetResponses(params HttpResponseMessage[] responses)
    {
        _responses.Clear();
        foreach (var response in responses)
        {
            _responses.Enqueue(response);
        }
    }

    public void SetDelayedResponse(HttpStatusCode statusCode, string content, TimeSpan delay)
    {
        _delay = delay;
        SetResponse(statusCode, content);
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        RequestCount++;

        if (_delay > TimeSpan.Zero)
        {
            await Task.Delay(_delay, cancellationToken);
        }

        if (_responses.Count > 0)
        {
            return _responses.Dequeue();
        }

        return _defaultResponse ?? new HttpResponseMessage(HttpStatusCode.OK);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _defaultResponse?.Dispose();
            while (_responses.Count > 0)
            {
                _responses.Dequeue().Dispose();
            }
        }
        base.Dispose(disposing);
    }
}
