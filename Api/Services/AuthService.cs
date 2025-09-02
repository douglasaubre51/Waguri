using Api.Models;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using MimeKit;
using System.Text;

namespace Api.Services
{
    public class AuthService(UserManager<User> userManager)
    {
        private readonly UserManager<User> _userManager = userManager;

        public async Task GetConfirmationEmail(User user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var callbackUrl = $"http://localhost:5000/sign-up/confirm/{user.Id}/{code}";
            var appPassword = Environment.GetEnvironmentVariable("EMAIL_APP_PASSWORD");
            var senderMail = "douglasaubre@gmail.com";
            var senderName = "douglas aubre";

            // craft email
            var email = new MimeMessage();
            email.From.Add(
                new MailboxAddress(senderName, senderMail)
                );
            email.To.Add(
                new MailboxAddress(user.FirstName, user.Email)
                );
            email.Subject = "Waguri account confirmation";
            email.Body = new TextPart("html")
            {
                Text = $"<h1>Confirmation code :</h1><strong>{callbackUrl}</strong><br>only lasts for a day!"
            };

            Console.WriteLine($"sending email to : {user.Email}");
            using var smtpClient = new SmtpClient();
            await smtpClient.ConnectAsync(
                "smtp.gmail.com",
                587,
                MailKit.Security.SecureSocketOptions.StartTls
                );
            await smtpClient.AuthenticateAsync(
                senderMail,
                appPassword
                );

            await smtpClient.SendAsync(email);
            Console.WriteLine($"email has been sent to : {user.Email}");

            await smtpClient.DisconnectAsync(true);
        }
    }
}
