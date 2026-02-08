using SendGrid;
using SendGrid.Helpers.Mail;

namespace Api.Services;

public class AuthService(UserManager<User> userManager)
{
    private readonly UserManager<User> _userManager = userManager;

    public async Task GetConfirmationEmail(User user)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        string urlBase = Environment.GetEnvironmentVariable("WAGURI_BASE_URI");

        var callbackUrl = $"{urlBase}/api/Auth/ConfirmEmail/{user.Id}/{code}";

        var appPassword = Environment.GetEnvironmentVariable("EMAIL_APP_PASSWORD");

        var senderMail = "douglasaubre@gmail.com";
        var senderName = "douglas aubre";

        string apiKey = Environment.GetEnvironmentVariable("SEND_GRID_API_KEY");
        var client = new SendGridClient(apiKey);

        var fromEmail = new EmailAddress(senderMail,senderName);
        var subject = "WAGURI account confirmation!";
        var to = new EmailAddress(user.Email);
        var html = $"<h1>Confirmation code :</h1><strong>{callbackUrl}</strong><br>only lasts for a day!";
        var msg = MailHelper.CreateSingleEmail(fromEmail, to, subject,string.Empty, html);
        var response = await client.SendEmailAsync(msg);
    }
}
