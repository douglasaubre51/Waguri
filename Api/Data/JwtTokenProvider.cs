using Api.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api.Data
{
    public sealed class JwtTokenProvider(IConfiguration _configuration)
    {
        public string CreateToken(User user)
        {
            var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
            SymmetricSecurityKey securityKey =
                new(Encoding.UTF8.GetBytes(secretKey));

            SigningCredentials credentials = new(
                securityKey,
                SecurityAlgorithms.HmacSha256
                );

            JwtSecurityToken tokenDescriptor = new(
                claims: [
                    new Claim(JwtRegisteredClaimNames.Sub,user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email,user.Email),
                    new Claim("email_confirmed",user.EmailConfirmed.ToString()),
                    new Claim(JwtRegisteredClaimNames.Aud,_configuration["JWT:Unohana"]),
                    new Claim(JwtRegisteredClaimNames.Aud,_configuration["JWT:Shiemi"])
                ],

                expires: DateTime.UtcNow.AddMinutes(
                    _configuration.GetValue<int>("JWT:Expires_After")
                    ),

                signingCredentials: credentials
            );

            var handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(tokenDescriptor);
        }
    }
}
