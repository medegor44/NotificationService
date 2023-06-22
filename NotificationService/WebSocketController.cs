using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationExchange.Abstractions;
using NotificationService.Auth;

namespace NotificationService;

[ApiController]
public class WebSocketController : Controller
{
    private readonly IExchangeConnector _exchange;
    private readonly IClaimsStore _claimsStore;

    public WebSocketController(IExchangeConnector exchange, IClaimsStore claimsStore)
    {
        _exchange = exchange;
        _claimsStore = claimsStore;
    }

    [Authorize]
    [HttpGet("/post/feed/posted")]
    public async Task AcceptAsync(CancellationToken cancellationToken)
    {
        var store = _claimsStore.FromClaims(HttpContext.User.Claims);
        var userId = store.GetUserId();
        
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

            var session = _exchange.ConnectUserSession(new(userId));

            foreach (var message in session.Messages(cancellationToken))
            {
                var bytes = Encoding.UTF8.GetBytes(message.Content);
                await webSocket.SendAsync(
                    new ArraySegment<byte>(bytes), 
                    WebSocketMessageType.Text, 
                    WebSocketMessageFlags.EndOfMessage, 
                    cancellationToken);
            }
            
            _exchange.DisconnectUserSession(new(userId), session);
            
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by server", cancellationToken);
        }
        else
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
    }
}