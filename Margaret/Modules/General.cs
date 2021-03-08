using System.Threading.Tasks;
using System.Data;
using System.IO;
using System.Linq;

using Microsoft.Extensions.Logging;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using DiscordDatabase;
using ImageProcessor;

namespace Margaret.Modules
{
    public class General : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient client;
        private readonly Users users;
        private readonly ProfileGenerator generator;
        private readonly ILogger<General> logger;

        public General(DiscordSocketClient client, ILogger<General> logger, Users users, ProfileGenerator generator)
        {
            this.client = client;
            this.logger = logger;
            this.users = users;
            this.generator = generator;
        }

        [Command("ping")]
        public async Task Ping()
        {
            await ReplyAsync("Pong! 🏓 **" + client.Latency + "ms**");
            logger.LogInformation($"{Context.User.Username} executed the Pong command!");
        }

        [Command("register")]
        public async Task Register(SocketGuildUser mentionedUser = null)
        {
            SocketGuildUser user = mentionedUser ?? Context.User as SocketGuildUser;

            if (users.Exists(user.Id))
            {
                await ReplyAsync($"User { user.Mention } already is registered!");
            }
            else
            {
                User dbUser = new User
                {
                    Id = user.Id,
                    Username = user.Username,
                    Discriminator = user.Discriminator,
                    AvatarUrl = user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl(),
                    BackgroundUrl = "https://images.unsplash.com/photo-1534796636912-3b95b3ab5986?ixid=MXwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHw%3D&ixlib=rb-1.2.1&auto=format&fit=crop&w=751&q=80",
                    CreatedAt = user.CreatedAt,
                    JoinedAt = user.JoinedAt.GetValueOrDefault()
                };
                await users.AddUser(dbUser);

                await ReplyAsync($"User { user.Mention } registered!");
            }

            logger.LogInformation($"{ Context.User.Username } executed the register command on { user.Username }!");
        }

        [Command("profile")]
        public async Task Profile(SocketGuildUser mentionedUser = null)
        {
            SocketGuildUser user = mentionedUser ?? Context.User as SocketGuildUser;

            if (!users.Exists(user.Id))
            {
                await ReplyAsync($"User { user.Mention } is not  registered!");
            }
            else
            {
                Stream stream = await generator.DrawProfile(await users.GetUser(user.Id));
                string fileName = $"{ user.Username }_banner.png";
                await Context.Channel.SendFileAsync(stream, fileName);
            }
        }

        [Command("profile")]
        public async Task Profile(string a, string b)
        {
            switch (a)
            {
                case "background":
                    users.UpdateBackground(Context.User.Id, b);
                    await ReplyAsync($"User { Context.User.Mention } updated background!");
                    logger.LogInformation($"{ Context.User.Username } updated background");
                    break;
                default:
                    break;
            }
        }
    }
}
