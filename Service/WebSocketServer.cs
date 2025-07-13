using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.WebSockets;
using System.Text;

namespace CieReader.Service
{
    public class WebSocketServer
    {

        private readonly ConcurrentDictionary<Guid, WebSocket> _connectedSockets = new();

        public WebSocketServer()
        {
            _ = StartServer("localhost", 8080);
        }
        public async Task StartServer(string ipAddress, int port)
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add($"http://{ipAddress}:{port}/");
            listener.Start();

            Console.WriteLine("Server started. Waiting for connections...");

            while (true)
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

        private async Task ProcessWebSocketRequest(HttpListenerContext context)
        {
            HttpListenerWebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null);
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
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                _connectedSockets.TryRemove(socketId, out _);
                Console.WriteLine($"Connection closed: {socketId}");
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
