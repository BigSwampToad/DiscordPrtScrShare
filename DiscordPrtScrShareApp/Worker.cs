using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Drawing;
using System.Windows.Input;

namespace DiscordPrtScrShareApp
{
    public class Worker : BackgroundService, IDisposable
    {
        private readonly ILogger<Worker> _logger;
        private readonly DiscordClient _discord;
        private readonly KeyboardInput _input;

        private readonly string _discordBotToken;
        private readonly string _discordChannelId;
        private readonly int _prtScrTriggerKey;

        public Worker(ILogger<Worker> logger, string[] args)
        {
            _discordBotToken = args[0];
            _discordChannelId = args[1];
            _prtScrTriggerKey = int.Parse(args[2]);

            _logger = logger;
            _logger.LogInformation("DiscordPrtScrShareApp is initiating.");
            _input = new KeyboardInput();
            _input.KeyPressed += _input_KeyPressed;
            _logger.LogInformation($"DiscordPrtScrShareApp KeyInput is : {_prtScrTriggerKey}, {(Key)_prtScrTriggerKey}");
            _discord = new DiscordClient(new DiscordConfiguration
            {
                Token = _discordBotToken,
                TokenType = TokenType.Bot
            });
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
                Thread.Sleep(100);
        }

        public override async Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("DiscordPrtScrShareApp is starting.");
            _discord.MessageCreated += async e =>
            {
                try
                {
                    if (e.Message.Content.ToLower().StartsWith("ping"))
                        await e.Message.RespondAsync("pong");

                    if (e.Message.Content.ToLower().StartsWith("pong"))
                    {

                        var stream = new MemoryStream(Properties.Resources.asrasz);
                        await e.Message.RespondWithFileAsync(stream, "asrasz.jpg");
                    }
                }
                catch (Exception ex)
                {
                    await e.Message.RespondAsync("I've shat myself and I can't get up. Please call The Big Frog.");
                    _logger.LogError($"DiscordPrtScrShareService caught an exception while responding to user message : {e.Message.Content}. \n " +
                        "You might be missing the necessary resources for this response to work properly. Please try downloading this bot software again. \n" +
                        $"Exception : {ex.Message}");
                }
            };
            try
            {
                await _discord.ConnectAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("DiscordPrtScrShareService caught an exception while connecting to Discord. \n " +
                        $"Please check your discordToken and try again. Exception : {ex.Message}");
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("DiscordPrtScrShareApp is stopping.");
            await _discord.DisconnectAsync();
            _input.KeyPressed -= _input_KeyPressed;
            _input.Dispose();
        }

        private async void _input_KeyPressed(int vKey)
        {
            _logger.LogInformation($"DiscordPrtScrShareApp registered vkey input : {KeyInterop.KeyFromVirtualKey(vKey)}. Trigger vkey is : {KeyInterop.KeyFromVirtualKey(_prtScrTriggerKey)}");
            if (KeyInterop.KeyFromVirtualKey(vKey) == (Key)_prtScrTriggerKey)
                await SendPrtScrToDiscord();
        }

        private async Task SendPrtScrToDiscord()
        {
            try
            {
                var channelId = _discordChannelId;
                var channel = await _discord.GetChannelAsync(ulong.Parse(channelId));
                using (var bmpCapture = new Bitmap(1900, 1200))
                using (var graphics = Graphics.FromImage(bmpCapture))
                {
                    graphics.CopyFromScreen(Point.Empty, Point.Empty, bmpCapture.Size);

                    var stream = new MemoryStream(ImageToByte(bmpCapture));
                    await channel.SendFileAsync(stream, "prtscr.jpg");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("DiscordPrtScrShareService caught an exception while responding to user message with a desktop screenshot. \n " +
                        $"Please check your channelId and discordToken and try again. Exception : {ex.Message}");
            }
        }

        private byte[] ImageToByte(Image img)
        {
            using (var stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }
    }
}