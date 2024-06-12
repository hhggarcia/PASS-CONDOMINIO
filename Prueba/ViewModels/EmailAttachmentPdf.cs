using System.Net.Mail;

namespace Prueba.ViewModels
{
    public class EmailAttachmentPdf
    {
        public string From { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public byte[] Pdf { get; set; } = null!;
        public string FileName { get; set; } = string.Empty;
        public IFormFile Attachment { get; set; } = null!;
    }
}
