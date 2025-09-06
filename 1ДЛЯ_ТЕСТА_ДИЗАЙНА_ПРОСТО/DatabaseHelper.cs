using System;
using System.Collections.Generic;
using System.Data.SQLite; // <-- Изменено: Используем библиотеку для SQLite
using System.IO;

namespace _1ДЛЯ_ТЕСТА_ДИЗАЙНА_ПРОСТО
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string AvatarUrl { get; set; }
    }

    public class DatabaseHelper
    {
        // --- Изменения для SQLite ---
        // 1. Строка подключения теперь указывает на локальный файл базы данных.
        //    Файл 'messenger.db' будет создан в той же папке, где запускается программа.
        private readonly string connectionString = "Data Source=messenger.db;Version=3;";
        private readonly string dbFileName = "messenger.db";

        public DatabaseHelper()
        {
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            // Создаем файл БД, если он не существует
            if (!File.Exists(dbFileName))
            {
                SQLiteConnection.CreateFile(dbFileName);
            }

            // 2. Используем SQLiteConnection вместо MySqlConnection
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // 3. Синтаксис SQL адаптирован под SQLite
                //    - AUTO_INCREMENT -> AUTOINCREMENT
                //    - INT -> INTEGER
                //    - VARCHAR -> TEXT
                //    - BOOLEAN -> INTEGER (0 для false, 1 для true)
                //    - DATETIME -> TEXT (храним в формате ISO 8601)
                //    - Убраны лишние проверки таблиц, т.к. мы создаем их с нуля один раз.
                string createTablesQuery = @"
                    CREATE TABLE IF NOT EXISTS Users (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Login TEXT NOT NULL UNIQUE,
                        Password TEXT NOT NULL,
                        AvatarUrl TEXT,
                        IsOnline INTEGER DEFAULT 0,
                        LastSeen TEXT,
                        IsTyping INTEGER DEFAULT 0
                    );

                    CREATE TABLE IF NOT EXISTS Chats (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        User1Id INTEGER NOT NULL,
                        User2Id INTEGER NOT NULL,
                        FOREIGN KEY (User1Id) REFERENCES Users(Id),
                        FOREIGN KEY (User2Id) REFERENCES Users(Id)
                    );

                    CREATE TABLE IF NOT EXISTS Messages (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        ChatId INTEGER NOT NULL,
                        SenderId INTEGER NOT NULL,
                        MessageText TEXT NULL,
                        AttachmentUrl TEXT NULL,
                        Timestamp TEXT NOT NULL,
                        FOREIGN KEY (ChatId) REFERENCES Chats(Id),
                        FOREIGN KEY (SenderId) REFERENCES Users(Id)
                    );

                    CREATE TABLE IF NOT EXISTS FavoriteChats (
                        UserId INTEGER NOT NULL,
                        ChatId INTEGER NOT NULL,
                        PRIMARY KEY (UserId, ChatId),
                        FOREIGN KEY (UserId) REFERENCES Users(Id),
                        FOREIGN KEY (ChatId) REFERENCES Chats(Id)
                    );

                    CREATE INDEX IF NOT EXISTS idx_users_id ON Users(Id);
                    CREATE INDEX IF NOT EXISTS idx_chats_users ON Chats(User1Id, User2Id);
                    CREATE INDEX IF NOT EXISTS idx_messages_chatid_timestamp ON Messages(ChatId, Timestamp);
                ";

                // 4. Используем SQLiteCommand
                using (var command = new SQLiteCommand(createTablesQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public void AddFavoriteChat(int userId, int chatId)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                // 5. INSERT IGNORE в MySQL заменен на INSERT OR IGNORE в SQLite
                string query = "INSERT OR IGNORE INTO FavoriteChats (UserId, ChatId) VALUES (@UserId, @ChatId)";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@ChatId", chatId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void RemoveFavoriteChat(int userId, int chatId)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "DELETE FROM FavoriteChats WHERE UserId = @UserId AND ChatId = @ChatId";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@ChatId", chatId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<int> GetFavoriteChats(int userId)
        {
            var favoriteChats = new List<int>();
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT ChatId FROM FavoriteChats WHERE UserId = @UserId";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            favoriteChats.Add(reader.GetInt32(0));
                        }
                    }
                }
            }
            return favoriteChats;
        }

        public User GetUserById(int userId)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT Id, Login, AvatarUrl FROM Users WHERE Id = @UserId";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Login = reader.GetString(reader.GetOrdinal("Login")),
                                AvatarUrl = reader.IsDBNull(reader.GetOrdinal("AvatarUrl")) ? null : reader.GetString(reader.GetOrdinal("AvatarUrl"))
                            };
                        }
                    }
                }
            }
            return null;
        }

        public bool RegisterUser(string login, string password)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO Users (Login, Password) VALUES (@Login, @Password)";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Login", login);
                    command.Parameters.AddWithValue("@Password", password);
                    try
                    {
                        command.ExecuteNonQuery();
                        return true;
                    }
                    // 6. MySqlException заменен на SQLiteException
                    catch (SQLiteException)
                    {
                        // Скорее всего, ошибка из-за уникальности логина
                        return false;
                    }
                }
            }
        }

        public (int? UserId, string Login, string AvatarUrl) LoginUser(string login, string password)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT Id, Login, AvatarUrl FROM Users WHERE Login = @Login AND Password = @Password";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Login", login);
                    command.Parameters.AddWithValue("@Password", password);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string avatarUrl = reader.IsDBNull(2) ? null : reader.GetString(2);
                            return (reader.GetInt32(0), reader.GetString(1), avatarUrl);
                        }
                    }
                }
            }
            return (null, null, null);
        }

        public List<(int Id, string Login, string AvatarUrl, bool IsOnline, DateTime? LastSeen, bool IsTyping)> GetUsers(int excludeUserId)
        {
            var users = new List<(int Id, string Login, string AvatarUrl, bool IsOnline, DateTime? LastSeen, bool IsTyping)>();
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT Id, Login, AvatarUrl, IsOnline, LastSeen, IsTyping FROM Users WHERE Id != @UserId ORDER BY Login ASC";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", excludeUserId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string avatarUrl = reader.IsDBNull(2) ? null : reader.GetString(2);
                            bool isOnline = reader.GetInt32(3) == 1;
                            DateTime? lastSeen = reader.IsDBNull(4) ? (DateTime?)null : DateTime.Parse(reader.GetString(4));
                            bool isTyping = reader.GetInt32(5) == 1;
                            users.Add((reader.GetInt32(0), reader.GetString(1), avatarUrl, isOnline, lastSeen, isTyping));
                        }
                    }
                }
            }
            return users;
        }

        public void UpdateAvatarUrl(int userId, string avatarUrl)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE Users SET AvatarUrl = @AvatarUrl WHERE Id = @UserId";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@AvatarUrl", (object)avatarUrl ?? DBNull.Value);
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public int GetOrCreateChat(int user1Id, int user2Id)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT Id FROM Chats WHERE (User1Id = @User1Id AND User2Id = @User2Id) OR (User1Id = @User2Id AND User2Id = @User1Id)";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@User1Id", user1Id);
                    command.Parameters.AddWithValue("@User2Id", user2Id);
                    var result = command.ExecuteScalar();
                    if (result != null)
                    {
                        return Convert.ToInt32(result);
                    }
                }

                // 7. LAST_INSERT_ID() в MySQL заменен на last_insert_rowid() в SQLite
                string insertQuery = "INSERT INTO Chats (User1Id, User2Id) VALUES (@User1Id, @User2Id); SELECT last_insert_rowid();";
                using (var command = new SQLiteCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@User1Id", user1Id);
                    command.Parameters.AddWithValue("@User2Id", user2Id);
                    // ExecuteScalar вернет ID новой записи
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public int SaveMessageAndGetId(int chatId, int senderId, string messageText, string attachmentUrl, DateTime timestamp)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = @"
                INSERT INTO Messages (ChatId, SenderId, MessageText, AttachmentUrl, Timestamp) 
                VALUES (@ChatId, @SenderId, @MessageText, @AttachmentUrl, @Timestamp); 
                SELECT last_insert_rowid();";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ChatId", chatId);
                    command.Parameters.AddWithValue("@SenderId", senderId);
                    command.Parameters.AddWithValue("@MessageText", (object)messageText ?? DBNull.Value);
                    command.Parameters.AddWithValue("@AttachmentUrl", (object)attachmentUrl ?? DBNull.Value);
                    // Сохраняем дату в стандартном формате, чтобы ее можно было сортировать как текст
                    command.Parameters.AddWithValue("@Timestamp", timestamp.ToString("o"));
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public List<(int Id, int SenderId, string MessageText, string AttachmentUrl, DateTime Timestamp)> GetMessages(int chatId)
        {
            var messages = new List<(int Id, int SenderId, string MessageText, string AttachmentUrl, DateTime Timestamp)>();
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT Id, SenderId, MessageText, AttachmentUrl, Timestamp FROM Messages WHERE ChatId = @ChatId ORDER BY Timestamp";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ChatId", chatId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            messages.Add((
                                reader.GetInt32(0),
                                reader.GetInt32(1),
                                reader.IsDBNull(2) ? null : reader.GetString(2),
                                reader.IsDBNull(3) ? null : reader.GetString(3),
                                DateTime.Parse(reader.GetString(4))
                            ));
                        }
                    }
                }
            }
            return messages;
        }

        public List<(int Id, int SenderId, string MessageText, string AttachmentUrl, DateTime Timestamp)> GetMessagesAfter(int chatId, DateTime afterTime)
        {
            var messages = new List<(int Id, int SenderId, string MessageText, string AttachmentUrl, DateTime Timestamp)>();
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT Id, SenderId, MessageText, AttachmentUrl, Timestamp FROM Messages WHERE ChatId = @ChatId AND Timestamp > @AfterTime ORDER BY Timestamp ASC";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ChatId", chatId);
                    command.Parameters.AddWithValue("@AfterTime", afterTime.ToString("o"));
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            messages.Add((
                                reader.GetInt32(0),
                                reader.GetInt32(1),
                                reader.IsDBNull(2) ? null : reader.GetString(2),
                                reader.IsDBNull(3) ? null : reader.GetString(3),
                                DateTime.Parse(reader.GetString(4))
                            ));
                        }
                    }
                }
            }
            return messages;
        }

        public void SetUserOnline(int userId, bool isOnline)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE Users SET IsOnline = @IsOnline, LastSeen = @LastSeen WHERE Id = @UserId";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@IsOnline", isOnline ? 1 : 0);
                    command.Parameters.AddWithValue("@LastSeen", isOnline ? (object)DBNull.Value : DateTime.Now.ToString("o"));
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void SetUserTyping(int userId, bool isTyping)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE Users SET IsTyping = @IsTyping WHERE Id = @UserId";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@IsTyping", isTyping ? 1 : 0);
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public (bool IsOnline, DateTime? LastSeen, bool IsTyping) GetUserStatus(int userId)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT IsOnline, LastSeen, IsTyping FROM Users WHERE Id = @UserId";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            bool isOnline = reader.GetInt32(0) == 1;
                            DateTime? lastSeen = reader.IsDBNull(1) ? (DateTime?)null : DateTime.Parse(reader.GetString(1));
                            bool isTyping = reader.GetInt32(2) == 1; //
                            return (isOnline, lastSeen, isTyping);
                        }
                    }
                }
            }
            return (false, null, false);
        }
    }
}
