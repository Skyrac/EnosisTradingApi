using System.Net;
using System.Net.Mail;

namespace API.Utility
{
    public static class Mailer
    {
        private const string FROM = "hitziger.fabian@live.de";
        private const string SERVER = "SMTP.office365.com";
        private const int PORT = 587;
        private const string METHOD = "STARTTLS";

        public static void CreateMessage(string to, string subject, string body)
        {
            MailMessage message = new MailMessage(FROM, to, subject, body);
            SmtpClient client = new SmtpClient(SERVER, PORT);
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential("hitziger.fabian@live.de", "Fabi!Alina?");
            client.Send(message);
            client.Dispose();
        }
    }
}
