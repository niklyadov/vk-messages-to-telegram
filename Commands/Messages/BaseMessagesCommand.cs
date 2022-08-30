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

                        if (vkMsg.PhotosLinks.Count == 1)
                        {
                            await TelegramBotClient.SendPhotoAsync(BotConfiguration.AllowedChatId, new InputOnlineFile(vkMsg.PhotosLinks.First()), vkMsg.FullText);
                        }
                        else
                        {
                            await TelegramBotClient.SendTextMessageAsync(BotConfiguration.AllowedChatId, vkMsg.FullText);
                            await TelegramBotClient.SendMediaGroupAsync(BotConfiguration.AllowedChatId,
                                GetInputMediasFromVkDocuments(vkMsg.PhotosLinks).Select(input => new InputMediaPhoto(input)));
                        }
                    }
                    else if (vkMsg.Documents.Count != 0)
                    {
                        await TelegramBotClient.SendChatActionAsync(BotConfiguration.AllowedChatId, ChatAction.UploadDocument, cancellationToken);

                        await TelegramBotClient.SendTextMessageAsync(BotConfiguration.AllowedChatId, vkMsg.FullText);
                        await TelegramBotClient.SendMediaGroupAsync(BotConfiguration.AllowedChatId,
                            GetInputMediasFromVkDocuments(vkMsg.Documents).Select(input => new InputMediaDocument(input)));
                    }
                    else if (vkMsg.AudioMessageLink != null)
                    {
                        await TelegramBotClient.SendChatActionAsync(BotConfiguration.AllowedChatId, ChatAction.RecordVoice, cancellationToken);
                        await TelegramBotClient.SendAudioAsync(BotConfiguration.AllowedChatId, new InputOnlineFile(vkMsg.AudioMessageLink), vkMsg.FullText);
                    }
                    else
                    {
                        await TelegramBotClient.SendChatActionAsync(BotConfiguration.AllowedChatId, ChatAction.Typing, cancellationToken);
                        await TelegramBotClient.SendTextMessageAsync(BotConfiguration.AllowedChatId, vkMsg.FullText);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
            }
        }

        private Stream GetStreamFromUrl(Uri url)
        {
            byte[] data = null;

            using (System.Net.WebClient wc = new System.Net.WebClient())
            {
                data = wc.DownloadData(url);
            }

            return new MemoryStream(data);
        }

        private List<InputMedia> GetInputMediasFromVkDocuments(List<DocumentModel> documents)
            => documents.Select(document => new InputMedia(GetStreamFromUrl(document.Uri), document.FileName)).ToList();

        private List<InputMedia> GetInputMediasFromVkDocuments(List<Uri> documentUris)
            => documentUris.Select(documentUri => new InputMedia(GetStreamFromUrl(documentUri), string.Empty)).ToList();
    }
}
