using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseWebSockets();

var clients = new ConcurrentDictionary<string, WebSocket>();

app.MapGet("/", () => "Server upp and running");

app.Map("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var socket = await context.WebSockets.AcceptWebSocketAsync();
        var id = Guid.NewGuid().ToString();
        clients[id] = socket;

        Console.WriteLine($"Client connected: {id}");

        // Ask client for username
        await SendJson(socket, new { type = "system", text = "Welcome! Please send your username first." });

        string? username = null;
        var buffer = new byte[1024 * 4];

        try
        {
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close) break;

                var msg = Encoding.UTF8.GetString(buffer, 0, result.Count);

                if (username == null)
                {
                    username = msg.Trim();
                    await BroadcastAsync(new { type = "system", text = $"{username} joined the chat" }, clients);
                    continue;
                }

                var messageObj = new
                {
                    type = "chat",
                    user = username,
                    text = msg,
                    time = DateTime.Now.ToString("HH:mm")
                };

                await BroadcastAsync(messageObj, clients);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            clients.TryRemove(id, out _);

            if (socket.State == WebSocketState.Open || socket.State == WebSocketState.CloseReceived)
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
            }

            if (username != null)
                await BroadcastAsync(new { type = "system", text = $"{username} left the chat" }, clients);

            Console.WriteLine($"Client disconnected: {id}");
        }
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

app.Run();

static async Task BroadcastAsync(object message, ConcurrentDictionary<string, WebSocket> clients)
{
    var json = JsonSerializer.Serialize(message);
    var bytes = Encoding.UTF8.GetBytes(json);
    var deadClients = new List<string>();

    foreach (var kvp in clients)
    {
        var id = kvp.Key;
        var socket = kvp.Value;

        // Skip if socket is closed or aborted
        if (socket.State != WebSocketState.Open)
        {
            deadClients.Add(id);
            continue;
        }

        try
        {
            await socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
        }
        catch (WebSocketException ex)
        {
            Console.WriteLine($"⚠️ Error sending to {id}: {ex.Message}");
            deadClients.Add(id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ General error for {id}: {ex.Message}");
            deadClients.Add(id);
        }
    }

    // Remove dead clients
    foreach (var id in deadClients)
    {
        clients.TryRemove(id, out _);
        Console.WriteLine($"🗑️ Removed dead client: {id}");
    }
}

static Task SendJson(WebSocket socket, object message)
{
    var json = JsonSerializer.Serialize(message);
    var bytes = Encoding.UTF8.GetBytes(json);
    return socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
}
