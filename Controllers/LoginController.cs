using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Models;
using Microsoft.IdentityModel.Tokens;
using static System.Net.Mime.MediaTypeNames;

namespace Pharmacy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private IConfiguration _config;

        public LoginController(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Authenticates user and generates JWT token
        /// </summary>
        /// <param name="userLogin"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Login([FromBody] UserLoginDto userLogin)
        {
            var user = Authenticate(userLogin);

            if (user != null)
            {
                var token = Generate(user);
                return Ok(token);
            }

            return NotFound("User not found");
        }

        private string Generate(User user)
        {
            Token token = new();
            //Security Key'in simetriğini alıyoruz.
            SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            //Şifrelenmiş kimliği oluşturuyoruz.
            SigningCredentials signingCredentials = new(securityKey, SecurityAlgorithms.HmacSha256);
            //Oluşturulacak token ayarlarını veriyoruz.
            token.Expiration = DateTime.UtcNow.AddHours(5);
            JwtSecurityToken securityToken = new(
             audience: _config["Jwt:Audience"],
             issuer: _config["Jwt:Issuer"],
             expires: token.Expiration,
             notBefore: DateTime.UtcNow,
             signingCredentials: signingCredentials,
             claims: new List<Claim> { new(ClaimTypes.Name, user.username) }
             );

            JwtSecurityTokenHandler tokenHandler = new();
            token.AccessToken = tokenHandler.WriteToken(securityToken);



            // Will be deleted just for correcting the key
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]!);
            var tokenValidationParameters = new TokenValidationParameters
            {
                // EmitIssuerValidation: Yayın yapanın (issuer) doğrulanması gerekip gerekmediğini belirtir
                ValidateIssuer = true,
                ValidIssuer = _config["Jwt:Issuer"], // Geçerli yayın yapanın adı
                                                              // EmitAudienceValidation: Hedef kitlenin (audience) doğrulanması gerekip gerekmediğini belirtir
                ValidateAudience = true,
                ValidAudience = _config["Jwt:Audience"], // Geçerli hedef kitlenin adı
                                                                  // EmitLifetimeValidation: Tokenin zaman aşımını kontrol edip etmemeyi belirtir
                ValidateLifetime = true,
                // EmitIssuerSigningKeyValidation: İmza anahtarının doğrulanıp doğrulanmayacağını belirtir
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key) // İmza anahtarı
            };

            SecurityToken securityToken1;
            var principal = tokenHandler.ValidateToken(token.AccessToken, tokenValidationParameters, out securityToken1);

            return token.AccessToken;
            

        }

        private User Authenticate(UserLoginDto userLogin)
        {
            var currentUser = UserConstants.Users.FirstOrDefault(o => o.username.ToLower() == userLogin.username.ToLower() && o.password == userLogin.password);

            if (currentUser != null)
            {
                return currentUser;
            }

            return null;
        }
    }
}