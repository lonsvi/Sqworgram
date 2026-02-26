using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace _1ДЛЯ_ТЕСТА_ДИЗАЙНА_ПРОСТО
{
    /// <summary>
    /// Interface for database operations.
    /// Enables dependency injection and mocking in unit tests.
    /// </summary>
    public interface IDatabaseHelper
    {
        // User operations
        bool RegisterUser(string login, string password);
        (int? UserId, string Login, string AvatarUrl) LoginUser(string login, string password);
        User GetUserById(int userId);
        List<(int Id, string Login, string AvatarUrl, bool IsOnline, DateTime? LastSeen, bool IsTyping)> GetUsers(int excludeUserId);
        void UpdateAvatarUrl(int userId, string avatarUrl);
        (bool IsOnline, DateTime? LastSeen, bool IsTyping) GetUserStatus(int userId);
        void SetUserOnline(int userId, bool isOnline);
        void SetUserTyping(int userId, bool isTyping);

        // Chat operations
        int GetOrCreateChat(int user1Id, int user2Id);

        // Message operations
        int SaveMessageAndGetId(int chatId, int senderId, string messageText, string attachmentUrl, DateTime timestamp);
        List<(int Id, int SenderId, string MessageText, string AttachmentUrl, DateTime Timestamp)> GetMessages(int chatId);
        List<(int Id, int SenderId, string MessageText, string AttachmentUrl, DateTime Timestamp)> GetMessagesAfter(int chatId, DateTime afterTime);

        // Favorite chats operations
        void AddFavoriteChat(int userId, int chatId);
        void RemoveFavoriteChat(int userId, int chatId);
        List<int> GetFavoriteChats(int userId);
    }
}
