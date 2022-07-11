using System;
using System.Collections.Generic;
using System.Linq;
using VkNet.Model.Attachments;

namespace VkToTg.Models
{
    public class MessageModel
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string FullText => GetFullText();
        public List<Uri> PhotosLinks { get; set; } = new List<Uri>();
        public List<Uri> DocumentsLinks { get; set; } = new List<Uri>();
        public Uri AudioMessageLink { get; set; }

        private string GetFullText()
        {
            return $"{Title}\n {Message}";
        }

        public void AppendAllAttachments(ICollection<Attachment> attachments)
        {
            if (attachments == null)
                throw new ArgumentNullException(nameof(attachments));

            foreach (var attachment in attachments)
            {
                if (attachment.Type == typeof(Photo))
                {
                    AppendPhotoAttachment(attachment.Instance as Photo);
                }
                else if (attachment.Type == typeof(Sticker))
                {
                    AppendStickerAttachment(attachment.Instance as Sticker);
                }
                else if (attachment.Type == typeof(AudioMessage))
                {
                    AppendAudioMessageAttachment(attachment.Instance as AudioMessage);
                }
                else if (attachment.Type == typeof(Document))
                {
                    AppendDocumentAttachment(attachment.Instance as Document);
                }
                else if (attachment.Type == typeof(Wall))
                {
                    AppendWallAttachment(attachment.Instance as Wall);
                }
            }
        }

        private void AppendPhotoAttachment(Photo photoAttachment)
        {
            if (photoAttachment.Sizes == null)
                return;

            if (photoAttachment.Sizes.Count() == 0)
                return;

            // select biggest photo
            var photosPixelsUrl = photoAttachment.Sizes.Select(ps => (ps.Width * ps.Height, ps.Url)).ToList();
            photosPixelsUrl.Sort((x, y) => y.Item1.CompareTo(x.Item1));

            PhotosLinks.Add(photosPixelsUrl.First().Url);
        }

        private void AppendStickerAttachment(Sticker stickerAttachment)
        {
            if (stickerAttachment.Images == null)
                return;

            if (stickerAttachment.Images.Count() == 0)
                return;

            PhotosLinks.Add(stickerAttachment.Images.First().Url);
            Message += "🏞 Sticker";
        }

        private void AppendDocumentAttachment(Document documentAttachment)
        {
            DocumentsLinks.Add(new Uri(documentAttachment.Uri));
        }

        private void AppendAudioMessageAttachment(AudioMessage audioMessageAttachment)
        {
            AudioMessageLink = audioMessageAttachment.LinkMp3;

            Message += $"\nAudio Message: \n{audioMessageAttachment.Transcript}";
        }

        private void AppendWallAttachment(Wall wallAttachment)
        {
            Message += $"\n--- Wall\n{wallAttachment.Text}";
        }
    }
}