using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace _1ДЛЯ_ТЕСТА_ДИЗАЙНА_ПРОСТО
{
    /// <summary>
    /// Client-side SignalR chat connection handler
    /// </summary>
    public class ChatHub
    {
        private HubConnection _connection;

        public ChatHub(string url)
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(url)
                .WithAutomaticReconnect()
                .Build();
        }

        public async Task ConnectAsync()
        {
            if (_connection?.State != HubConnectionState.Connected)
            {
                await _connection.StartAsync();
            }
        }

        public async Task DisconnectAsync()
        {
            if (_connection?.State == HubConnectionState.Connected)
            {
                await _connection.StopAsync();
            }
        }

        public async Task SendMessageAsync(int chatId, int senderId, string message, DateTime timestamp)
        {
            // Send message to the server
            await _connection.InvokeAsync("SendMessage", chatId, senderId, message, timestamp);
        }

        public async Task JoinChatAsync(int chatId)
        {
            // Join a chat group
            await _connection.InvokeAsync("JoinChat", chatId);
        }

        public void OnReceiveMessage(Action<int, int, string, DateTime> callback)
        {
            _connection.On<int, int, string, DateTime>("ReceiveMessage", callback);
        }

        public HubConnectionState GetConnectionState()
        {
            return _connection?.State ?? HubConnectionState.Disconnected;
        }
    }
}