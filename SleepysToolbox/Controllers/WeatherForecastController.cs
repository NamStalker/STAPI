using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Renci.SshNet;
using System.Security.Cryptography;

namespace SleepysToolbox.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IConfiguration _config;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet]
        [Route("GetTimers")]
        public IEnumerable<string> GetTimers()
        {
            ServerConn settings = new ServerConn();
            _config.GetSection("ServerConn").Bind(settings);
            //var (sshClient, localPort) = ConnectSsh(settings);
            //using (sshClient)
            //{

                MySqlConnectionStringBuilder csb = new MySqlConnectionStringBuilder
                {
                    Server = "127.0.0.1",
                    Port = (uint)settings.DBPort,
                    UserID = settings.DBUser,
                    Password = settings.DBPassword,
                    Database = settings.Database
                };

                var conn = new MySqlConnection(csb.ConnectionString);

                List<string> timers = new List<string>();

                conn.Open();
                //MySqlCommand cmd = new MySqlCommand("insert into timers(timerName, runTime, waitTime, userId) values ('testTimer', '0000:01:00', '0000:00:30', 1)", conn);
                //cmd.ExecuteNonQuery();
                MySqlCommand cmd = new MySqlCommand("select * from timers", conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    timers.Add(reader.GetString(1));
                }
                reader.Close();
                cmd.Dispose();
                conn.Close();
                return timers;
            //}
        }

        public static (SshClient SshClient, uint Port) ConnectSsh(ServerConn settings)
        {
            // define the authentication methods to use (in order)
            var authenticationMethods = new List<AuthenticationMethod>();

            authenticationMethods.Add(new PrivateKeyAuthenticationMethod(settings.SshUser,
                new PrivateKeyFile(settings.SshKeyFile, null)));

            // connect to the SSH server
            var sshClient = new SshClient(new Renci.SshNet.ConnectionInfo(settings.SshServer, settings.SshPort, settings.SshUser, authenticationMethods.ToArray()));
            sshClient.Connect();

            // forward a local port to the database server and port, using the SSH server
            var forwardedPort = new ForwardedPortLocal("127.0.0.1", settings.DatabaseServer, (uint)settings.DBPort);
            sshClient.AddForwardedPort(forwardedPort);
            forwardedPort.Start();

            return (sshClient, forwardedPort.BoundPort);
        }
    }
}