using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SleepysToolbox.Models;
using SleepysToolbox.Helpers;
using MySql.Data.MySqlClient;

namespace SleepysToolbox.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly JwtSettings jwtSettings;
        private readonly IConfiguration _config;
        public AccountController(JwtSettings jwtSettings, IConfiguration config)
        {
            this.jwtSettings = jwtSettings;
            this._config = config;
        }
        private IEnumerable<User> logins = new List<User>() {
            new User() {
                    Id = Guid.NewGuid(),
                        EmailId = "adminakp@gmail.com",
                        UserName = "Admin",
                        Password = CryptoHelper.HashPW("Admin", "UadAM8KXHZqRER5d3BjoYostndLTZ3xodfZGjQKrl4rG55io9mjL8j3M7TcJZIDMuMsljhGVhWm8geVJYrit2w==").Combined
                },
                new User() {
                    Id = Guid.NewGuid(),
                        EmailId = "adminakp@gmail.com",
                        UserName = "User1",
                        Password = CryptoHelper.HashPW("admin", "UadAM8KXHZqRER5d3BjoYostndLTZ3xodfZGjQKrl4rG55io9mjL8j3M7TcJZIDMuMsljhGVhWm8geVJYrit2w==").Combined
                }
        };

        [HttpPost]
        public IActionResult GetToken(UserLogin userLogins)
        {
            try
            {
                var Token = new UserToken();
                var user = logins.FirstOrDefault(x => x.UserName.Equals(userLogins.UserName, StringComparison.OrdinalIgnoreCase));
                if (user != null)
                {
                    if(CryptoHelper.VerifyHashedPW(user, userLogins.Password))
                    {
                        Token = JwtHelper.GenTokenkey(new UserToken()
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

        [HttpPost]
        public IActionResult Register(UserLogin passedLogin)
        {
            try
            {
                var Token = new UserToken();
                var user = UserExists(passedLogin.UserName);
                if (user == null)
                {
                    ServerConn settings = new ServerConn();
                    _config.GetSection("ServerConn").Bind(settings);

                    MySqlConnectionStringBuilder csb = new MySqlConnectionStringBuilder
                    {
                        Server = "127.0.0.1",
                        Port = (uint)settings.DBPort,
                        UserID = settings.DBUser,
                        Password = settings.DBPassword,
                        Database = settings.Database
                    };

                    var conn = new MySqlConnection(csb.ConnectionString);

                    var cmnd = "insert into users(username, password) values(" + passedLogin.UserName + "," + CryptoHelper.HashPW(passedLogin.Password.Split(':')[0]).Combined + ")";

                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(passedLogin.Password, conn);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    conn.Close();

                    return Ok();
                }
                else
                {
                    return BadRequest($"Duplicate email");
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpGet]
        public IActionResult GetSalt(string user)
        {
            ServerConn settings = new ServerConn();
            _config.GetSection("ServerConn").Bind(settings);

            MySqlConnectionStringBuilder csb = new MySqlConnectionStringBuilder
            {
                Server = "127.0.0.1",
                Port = (uint)settings.DBPort,
                UserID = settings.DBUser,
                Password = settings.DBPassword,
                Database = settings.Database
            };

            var conn = new MySqlConnection(csb.ConnectionString);

            conn.Open();
            MySqlCommand cmd = new MySqlCommand("select * from users where username = '" + user + "'", conn);
            var reader = cmd.ExecuteReader();
            var userLogin = new UserLogin();
            while (reader.Read())
            {
                userLogin.UserName = reader.GetString(1);
                userLogin.Password = reader.GetString(2);
            }
            cmd.Dispose();
            conn.Close();

            return Ok(userLogin.Password.Split(':'));
        }

        private UserLogin UserExists(string user)
        {
            ServerConn settings = new ServerConn();
            _config.GetSection("ServerConn").Bind(settings);

            MySqlConnectionStringBuilder csb = new MySqlConnectionStringBuilder
            {
                Server = "127.0.0.1",
                Port = (uint)settings.DBPort,
                UserID = settings.DBUser,
                Password = settings.DBPassword,
                Database = settings.Database
            };

            var conn = new MySqlConnection(csb.ConnectionString);

            conn.Open();
            MySqlCommand cmd = new MySqlCommand("select * from users where username = '" + user + "'", conn);
            var reader = cmd.ExecuteReader();
            UserLogin userLogin = new UserLogin();
            while (reader.Read())
            {
                userLogin.UserName = reader.GetString(1);
                userLogin.Password = reader.GetString(2);
            }
            reader.Close();

            cmd.Dispose();
            conn.Close();

            return String.IsNullOrEmpty(userLogin.UserName) ? null : userLogin;
        }
    }
}