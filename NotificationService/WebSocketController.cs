using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using NotificationExchange;
using NotificationExchange.Abstractions;

namespace NotificationService;

[ApiController]
public class WebSocketController : Controller
{
    private readonly IExchangeConnector _exchange;

    public WebSocketController(IExchangeConnector exchange)
    {
        _exchange = exchange;
    }

    [HttpGet("/ws/{userName}")]
    public async Task AcceptAsync([FromRoute] string userName, CancellationToken cancellationToken)
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

            var session = _exchange.ConnectUserSession(new(userName));

            foreach (var message in session.Messages(cancellationToken))
            {
                var bytes = Encoding.UTF8.GetBytes(message.Content);
                await webSocket.SendAsync(
                    new ArraySegment<byte>(bytes), 
                    WebSocketMessageType.Text, 
                    WebSocketMessageFlags.EndOfMessage, 
                    cancellationToken);
            }
            
            _exchange.DisconnectUserSession(new(userName), session);
            
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by server", cancellationToken);
        }
        else
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
    }
}