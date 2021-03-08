using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;


namespace DiscordDatabase
{
    public class Users
    {
        private readonly RootDatabase database;
        private readonly ILogger<Users> logger;

        public Users(RootDatabase database, ILogger<Users> logger)
        {
            this.database = database;
            this.logger = logger;
        }

        public async Task AddUser(User user)
        {
            if (database.Users.Any(u => u.Id == user.Id))
            {
                logger.LogWarning($"User {user.Username} already registered!");
            }
            else
            {
                database.Users.Add(user);
                await database.SaveChangesAsync();
            }
        }

        public async Task<User> GetUser(ulong id)
        {
            User user = database.Users.Where(u => u.Id == id).FirstOrDefault();
            return await Task.FromResult(user);
        }

        public void UpdateUser(ulong id, User newUser)
        {
            User user = database.Users.Where(u => u.Id == id).FirstOrDefault();
            user = newUser;
            database.SaveChanges();
        }

        public void UpdateBackground(ulong id, string newBackground)
        {
            database.Users.Where(u => u.Id == id)
                          .FirstOrDefault()
                          .BackgroundUrl = newBackground;
            database.SaveChanges();
        }
        
        public bool Exists(ulong id)
        {
            if (database.Users.Any(u => u.Id == id))
                return true;
            else
                return false;
        }
    }
}
