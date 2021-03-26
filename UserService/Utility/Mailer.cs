using System.Net;
using System.Net.Mail;

namespace API.Utility
{
    public static class Mailer
    {
        private const string FROM = "support@moneymoon.app";
        private const string SERVER = "smtp.ionos.de";
        private const int PORT = 587;
        private const string METHOD = "STARTTLS";

        public static void CreateMessage(string to, string subject, string body)
        {
            MailMessage message = new MailMessage(FROM, to, subject, body);
            SmtpClient client = new SmtpClient(SERVER, PORT);
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(FROM, "Sup3rG3h31m!");
            client.Send(message);
            client.Dispose();
        }
    }
}
