using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SleepysToolbox.Models;
using SleepysToolbox.JwtHelpers;

namespace SleepysToolbox.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly JwtSettings jwtSettings;
        public AccountController(JwtSettings jwtSettings)
        {
            this.jwtSettings = jwtSettings;
        }
        private IEnumerable<Users> logins = new List<Users>() {
            new Users() {
                    Id = Guid.NewGuid(),
                        EmailId = "adminakp@gmail.com",
                        UserName = "Admin",
                        Password = CryptoHelper.HashPW("Admin", "UadAM8KXHZqRER5d3BjoYostndLTZ3xodfZGjQKrl4rG55io9mjL8j3M7TcJZIDMuMsljhGVhWm8geVJYrit2w==").Combined
                },
                new Users() {
                    Id = Guid.NewGuid(),
                        EmailId = "adminakp@gmail.com",
                        UserName = "User1",
                        Password = CryptoHelper.HashPW("admin", "UadAM8KXHZqRER5d3BjoYostndLTZ3xodfZGjQKrl4rG55io9mjL8j3M7TcJZIDMuMsljhGVhWm8geVJYrit2w==").Combined
                }
        };
        [HttpPost]
        public IActionResult GetToken(UserLogins userLogins)
        {
            try
            {
                var Token = new UserTokens();
                var user = logins.FirstOrDefault(x => x.UserName.Equals(userLogins.UserName, StringComparison.OrdinalIgnoreCase));
                if (user != null)
                {
                    if(CryptoHelper.VerifyHashedPW(user, userLogins.Password))
                    {
                        Token = JwtHelpers.JwtHelpers.GenTokenkey(new UserTokens()
                        {
                            EmailId = user.EmailId,
                            GuidId = Guid.NewGuid(),
                            UserName = user.UserName,
                            Id = user.Id,
                        }, jwtSettings);
                    }
                    else
                    {
                        return BadRequest($"Password incorrect.");
                    }
                }
                else
                {
                    return BadRequest($"User does not exist.");
                }
                return Ok(Token);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        /// <summary>
        /// Get List of UserAccounts
        /// </summary>
        /// <returns>List Of UserAccounts</returns>
        [HttpGet]
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetList()
        {
            return Ok(logins);
        }
    }
}