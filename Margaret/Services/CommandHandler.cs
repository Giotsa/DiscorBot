using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;

namespace Margaret
{
    internal class CommandHandler : InitializedService
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly IConfiguration _config;
        private readonly IServiceProvider _provider;


        // DiscordSocketClient, CommandService, IConfigurationRoot, and IServiceProvider are injected automatically from the IServiceProvider
        public CommandHandler(DiscordSocketClient discord, CommandService commands, IConfiguration config, IServiceProvider provider)
        {
            _discord = discord;
            _commands = commands;
            _config = config;
            _provider = provider;
        }

        public override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            _discord.MessageReceived += OnMessageReceived;

            _commands.CommandExecuted += OnCommandExecuted;

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }

        private async Task OnMessageReceived(SocketMessage socketMessage)
        {
            // Ensure the message is from a user/bot
            SocketUserMessage message = socketMessage as SocketUserMessage;
            if (message == null) return;
            // Ignore self when checking commands
            if (message.Author.Id == _discord.CurrentUser.Id) return;      

            // Create the command context
            var context = new SocketCommandContext(_discord, message);     

            // Check if the message has a valid command prefix
            int argPos = 0;
            if (message.HasStringPrefix(_config["prefix"], ref argPos) || message.HasMentionPrefix(_discord.CurrentUser, ref argPos))
            {
                // Execute the command
                var result = await _commands.ExecuteAsync(context, argPos, _provider);     // Execute the command

                // If not successful, reply with the error.
                if (!result.IsSuccess)
                    await context.Channel.SendMessageAsync(result.ToString());
            }
        }

        private async Task OnCommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (command.IsSpecified && !result.IsSuccess)
            {
                await context.Channel.SendMessageAsync($"Error: {result}");
            }
        }
    }
}