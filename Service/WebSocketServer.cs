using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;

namespace CieReader.Service
{
    public class WebSocketServer
    {

        private readonly ConcurrentDictionary<Guid, WebSocket> _connectedSockets = new();
        private bool _cancelling = false;
        private string apiKey;

        public WebSocketServer(WebSocketConfig webSocketConfig)
        {
            this.apiKey = webSocketConfig.WsApiKey;
            _ = StartServer(webSocketConfig.Host, webSocketConfig.Port);
        }
        public async Task StartServer(string ipAddress, int port)
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add($"http://{ipAddress}:{port}/");
            listener.Start();

            Debug.WriteLine("Server started. Waiting for connections...");

            while (!_cancelling)
            {
                HttpListenerContext context = await listener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    _ = ProcessWebSocketRequest(context);
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
        }

        public async void StopServer()
        {
            Debug.WriteLine("Stopping websocket...");
            _cancelling = true;
            foreach (var (id, socket) in _connectedSockets)
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server shutting down", CancellationToken.None);
            }
        }

        private async Task ProcessWebSocketRequest(HttpListenerContext context)
        {
            string[] ApiKey = context.Request.Headers.GetValues("X-API-Key");
            HttpListenerWebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null);
            
            if (ApiKey?.Length != 0 && ApiKey?[0] == apiKey)
            {                               
                WebSocket socket = webSocketContext.WebSocket;

                Guid socketId = Guid.NewGuid();
                _connectedSockets.TryAdd(socketId, socket);
                // Handle incoming messages
                byte[] buffer = new byte[1024];
                try
                {
                    while (socket.State == WebSocketState.Open)
                    {
                        WebSocketReceiveResult result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        if (result.MessageType == WebSocketMessageType.Text)
                        {
                            string receivedMessage = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
                            Debug.WriteLine($"Received message: {receivedMessage}");

                            // Echo back the received message
                            await socket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                        else if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
                finally
                {
                    _connectedSockets.TryRemove(socketId, out _);
                    Debug.WriteLine($"Connection closed: {socketId}");
                }
            }
            else {
                await webSocketContext.WebSocket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "Unauthorized", CancellationToken.None);                              
            }
        }

        public async Task BroadcastMessageAsync(string message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            var tasks = _connectedSockets.Values
                .Where(s => s.State == WebSocketState.Open)
                .Select(s => s.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None));

            await Task.WhenAll(tasks);
        }

    }
}
