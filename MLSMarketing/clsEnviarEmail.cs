using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;
using System.IO;


namespace MLSMarketing
{
    public class clsEnviarEmail
    {
        public string EnviarEmail(string smtpDominio, string smtpUsuario, string smtpSenha, string smtpPorta, string EmailTo, string EmailAssunto, string EmailBody, string PatchAnexo, string ComCopia, MemoryStream pdfStream)
        {
            try
            {
                var body = "<p>Email From: {0} ({1})</p><p>Mensagem:</p><p>{2}</p>";
                var message = new MailMessage();
                message.To.Add(new MailAddress(EmailTo));  // replace with valid value 
                if (ComCopia != string.Empty)
                {
                    message.CC.Add(new MailAddress(ComCopia));
                }
                message.From = new MailAddress(smtpUsuario);  // replace with valid value
                message.Subject = EmailAssunto;
                message.Body = string.Format(body, smtpUsuario, "Manager", EmailBody);
                if (PatchAnexo != string.Empty)
                {
                    //Attachment Patch = new Attachment(PatchAnexo);
                    message.Attachments.Add(new Attachment(pdfStream, PatchAnexo));
                    //message.Attachments.Add(Patch);
                }
                message.IsBodyHtml = true;
                using (var smtp = new SmtpClient(smtpDominio))
                {
                    smtp.EnableSsl = true; // GMail requer SSL

                    smtp.Port = Convert.ToInt32(smtpPorta);       // porta para SSL
                    //smtp.DeliveryMethod = SmtpDeliveryMethod.Network; // modo de envio
                    smtp.UseDefaultCredentials = false; // vamos utilizar credencias especificas

                    // seu usuário e senha para autenticação
                    smtp.Credentials = new NetworkCredential(smtpUsuario, smtpSenha);

                    // envia o e-mail
                    smtp.Send(message);
                    message.Attachments.Clear();
                    message.Dispose();
                    smtp.Dispose();
                    return "Email enviado com Sucesso";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

    }
}
