using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using VkToTg.Commands.Core;
using VkToTg.Models;

namespace VkToTg.Commands.Messages
{
    public class BaseMessagesCommand : BaseCommand
    {
        public BaseMessagesCommand(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected async Task PrintMesssagesAync(IAsyncEnumerable<MessageModel> messages, CancellationToken cancellationToken)
        {
            await foreach (var vkMsg in messages)
            {
                try
                {
                    if (vkMsg.PhotosLinks.Count != 0)
                    {
                        await TelegramBotClient.SendChatActionAsync(BotConfiguration.AllowedChatId, ChatAction.UploadPhoto, cancellationToken);

                        if(vkMsg.PhotosLinks.Count == 1)
                        {
                            await TelegramBotClient.SendPhotoAsync(BotConfiguration.AllowedChatId, new InputOnlineFile(vkMsg.PhotosLinks.First()), vkMsg.Text);
                        } else
                        {
                            await TelegramBotClient.SendMediaGroupAsync(BotConfiguration.AllowedChatId, 
                                GetInputMediasFromUris(vkMsg.PhotosLinks).Select(input => new InputMediaPhoto(input)));
                            await TelegramBotClient.SendTextMessageAsync(BotConfiguration.AllowedChatId, vkMsg.Text);
                        }
                    }
                    else if (vkMsg.DocumentsLinks.Count != 0)
                    {
                        await TelegramBotClient.SendChatActionAsync(BotConfiguration.AllowedChatId, ChatAction.UploadDocument, cancellationToken);

                        if (vkMsg.DocumentsLinks.Count == 1)
                        {
                            await TelegramBotClient.SendPhotoAsync(BotConfiguration.AllowedChatId,
                                new InputOnlineFile(vkMsg.PhotosLinks.First()), vkMsg.Text);
                        }
                        else
                        {
                            await TelegramBotClient.SendMediaGroupAsync(BotConfiguration.AllowedChatId,
                                GetInputMediasFromUris(vkMsg.DocumentsLinks).Select(input => new InputMediaDocument(input)));
                            await TelegramBotClient.SendTextMessageAsync(BotConfiguration.AllowedChatId, vkMsg.Text);
                        }
                    }
                    else if (vkMsg.AudioMessageLink != null)
                    {
                        await TelegramBotClient.SendChatActionAsync(BotConfiguration.AllowedChatId, ChatAction.RecordVoice, cancellationToken);
                        await TelegramBotClient.SendAudioAsync(BotConfiguration.AllowedChatId, new InputOnlineFile(vkMsg.AudioMessageLink), vkMsg.Text);
                    }
                    else
                    {
                        await TelegramBotClient.SendChatActionAsync(BotConfiguration.AllowedChatId, ChatAction.Typing, cancellationToken);
                        await TelegramBotClient.SendTextMessageAsync(BotConfiguration.AllowedChatId, vkMsg.Text);
                    }
                } catch (Exception ex)
                {
                    Logger.LogError(ex.Message);
                }
            }
        }

        private Stream GetStreamFromUrl(string url)
        {
            byte[] data = null;

            using (System.Net.WebClient wc = new System.Net.WebClient())
            {
                data = wc.DownloadData(url);
            }

            return new MemoryStream(data);
        }

        private List<InputMedia> GetInputMediasFromUris(List<Uri> uris)
            => uris.Select(uri => new InputMedia(GetStreamFromUrl(uri.AbsoluteUri), uri.AbsolutePath)).ToList();
    }
}
