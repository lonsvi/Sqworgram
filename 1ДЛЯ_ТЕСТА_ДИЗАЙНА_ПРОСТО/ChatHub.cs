using Microsoft.AspNet.SignalR;
using System;
using System.Threading.Tasks;

namespace _1ДЛЯ_ТЕСТА_ДИЗАЙНА_ПРОСТО
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(int chatId, int senderId, string message, DateTime timestamp)
        {
            // Рассылаем сообщение всем в группе chatId
            await Clients.Group(chatId.ToString()).ReceiveMessage(chatId, senderId, message, timestamp);
        }

        public async Task JoinChat(int chatId)
        {
            // Добавляем клиента в группу
            await Groups.Add(Context.ConnectionId, chatId.ToString());
        }
    }
}