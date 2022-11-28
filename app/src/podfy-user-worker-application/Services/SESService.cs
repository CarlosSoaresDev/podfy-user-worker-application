using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using podfy_user_worker_application.Model;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;

namespace podfy_user_worker_application.Services;

public class SESService : ISESService
{
    private string SENDER_EMAIL = Environment.GetEnvironmentVariable("EMAIL_SENDER");
    private string HTML_TEMPLATE = "<div><h1>Olá {0}</h1><br><p>Seja bem-vindo a plataforma, aproveite para ouvir os melhores podcast</p></div>";

    public async Task<bool> SendEmailAsync(string body)
    {
        return await SendEmailWithSDKAsync(body);
    }

    private async Task<bool> SendEmailWithSDKAsync(string body)
    {
        try
        {
            var result = JsonSerializer.Deserialize<UserEmail>(body);

            using (var client = GetSESClient())
            {
                var htmlBody = string.Format(HTML_TEMPLATE, result.Name);
                var subject = "Seja bem-vindo";


                var sendRequest = new SendEmailRequest
                {
                    Source = SENDER_EMAIL,
                    Destination = new Destination
                    {
                        ToAddresses =
                        new List<string> { result.Email }
                    },
                    Message = new Message
                    {
                        Subject = new Content(subject),
                        Body = new Body
                        {
                            Html = new Content
                            {
                                Charset = "UTF-8",
                                Data = htmlBody
                            },
                            Text = new Content
                            {
                                Charset = "UTF-8",
                                Data = htmlBody
                            }
                        }
                    }
                };

                var response = await client.SendEmailAsync(sendRequest);

                return response.HttpStatusCode == HttpStatusCode.OK ? true : false;
            }
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    private async Task<bool> SendEmailWithSMTPAsync(string body)
    {
        try
        {
            var result = JsonSerializer.Deserialize<UserEmail>(body);

            var htmlBody = string.Format(HTML_TEMPLATE, result.Name);
            var subject = "Seja bem-vindo";

            using (MailMessage mail = new MailMessage())
            {
                mail.To.Add(result.Email);
                mail.From = new MailAddress(SENDER_EMAIL, "Podcast", Encoding.UTF8);
                mail.Subject = subject;
                mail.SubjectEncoding = Encoding.UTF8;
                mail.Body = htmlBody;
                mail.BodyEncoding = Encoding.UTF8;
                mail.IsBodyHtml = true;
                mail.Priority = MailPriority.High;

                using (var client = new SmtpClient())
                {
                    client.Credentials = new NetworkCredential(Environment.GetEnvironmentVariable("KEY_CREDENTIAL_SMTP"), Environment.GetEnvironmentVariable("SECRETE_CREDENTIAL_SMTP"));
                    client.Port = 587;
                    client.Host = "email-smtp.us-east-1.amazonaws.com";
                    client.EnableSsl = true;
                    client.UseDefaultCredentials = false;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;

                    await client.SendMailAsync(mail);

                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    private AmazonSimpleEmailServiceClient GetSESClient()
    {
        if (Debugger.IsAttached)
            return new AmazonSimpleEmailServiceClient();

        return new AmazonSimpleEmailServiceClient(Environment.GetEnvironmentVariable("ACCESS_KEY"), Environment.GetEnvironmentVariable("SECRET_KEY"), Amazon.RegionEndpoint.USEast1);
    }
}
