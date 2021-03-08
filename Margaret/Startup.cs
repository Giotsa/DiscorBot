using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using Serilog;

using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;

using DiscordDatabase;
using ImageProcessor;

namespace Margaret
{
    class Startup
    {
        private readonly IConfiguration _config;

        public Startup(string[] args)
        {
            // Setup Config
            ConfigurationBuilder configBuilder = new ConfigurationBuilder();
            configBuilder.SetBasePath(Path.Combine(AppContext.BaseDirectory, "Common/Config"));
            configBuilder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            configBuilder.AddCommandLine(args);

            _config = configBuilder.Build();

            // Setup Logger
            var LogConfg = new LoggerConfiguration();
            LogConfg.ReadFrom.Configuration(_config);
            LogConfg.Enrich.FromLogContext();
            LogConfg.WriteTo.Console();

            Log.Logger = LogConfg.CreateLogger();

        }

        public async Task StartAsync()
        {
            Log.Logger.Information("Application Starting");

            var hostBuilder = Host.CreateDefaultBuilder();

            // Discord Configuration
            var discordConfig = new DiscordSocketConfig
            {
                LogLevel = _config.GetSection("discordHost").GetValue<LogSeverity>("logLevel"),
                AlwaysDownloadUsers = _config.GetSection("discordHost").GetValue<bool>("AlwaysDownloadUsers"),
                MessageCacheSize = _config.GetSection("discordHost").GetValue<int>("messageCacheSize"),
            };
            hostBuilder.ConfigureDiscordHost<DiscordSocketClient>((context, discordHostConfig) =>
            {
                discordHostConfig.SocketConfig = discordConfig;
                discordHostConfig.Token = _config.GetValue<string>("token");
            });

            // Discord Command configuration
            var commandConfig = new CommandServiceConfig
            {
                DefaultRunMode = _config.GetSection("commandService").GetValue<RunMode>("runMode"),
                LogLevel = _config.GetSection("commandService").GetValue<LogSeverity>("logLevel"),
                CaseSensitiveCommands = _config.GetSection("commandService").GetValue<bool>("caseSensitiveCommands")
            };
            hostBuilder.UseCommandService((context, commandServiceConfig) =>
            {
                commandServiceConfig = commandConfig;
            });

            // Services
            hostBuilder.ConfigureServices((context, services) =>
            {
                services.AddSingleton(_config);
                services.AddHostedService<CommandHandler>();
                services.AddSingleton<ProfileGenerator>();
                services.AddDbContext<RootDatabase>();
                services.AddSingleton<Users>();
            });

            hostBuilder.UseConsoleLifetime();
            hostBuilder.UseSerilog();

            try
            {
                var host = hostBuilder.Build();
                using (host)
                {
                    await host.RunAsync();
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.Message);
            }
        }
    }
}
