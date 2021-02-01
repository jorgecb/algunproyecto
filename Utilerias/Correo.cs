using System.Net.Mail;
using System.Threading.Tasks;

namespace Utilerias
{
    public class Correo
    {
        private MailMessage _mailMessage;
        private SmtpClient _smtpClient;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="smtpClient">Configuración SmtpClient</param>
        public Correo(SmtpClient smtpClient)
        {
            _smtpClient = smtpClient;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mailMessage">Configuración MailMessage</param>
        /// <param name="smtpClient">Configuración SmtpClient</param>
        public Correo(MailMessage mailMessage, SmtpClient smtpClient)
        {
            _mailMessage = mailMessage;
            _smtpClient = smtpClient;
        }

        /// <summary>
        /// Envío de Correo
        /// </summary>
        public void EnviarCorreo()
        {
            _smtpClient.Send(_mailMessage);
        }

        /// Envío de Correo Asíncrono
        public async Task EnviarCorreoAsync()
        {
            await _smtpClient.SendMailAsync(_mailMessage);
        }
    }
}
