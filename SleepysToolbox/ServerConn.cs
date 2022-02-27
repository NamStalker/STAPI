namespace SleepysToolbox
{
    public class ServerConn
    {
        public string SshServer { get; set; }
        public string SshUser { get; set; }
        public string SshKeyFile { get; set; }
        public int SshPort { get; set; }
        public string DatabaseServer { get; set; }
        public string DBUser { get; set; }
        public string DBPassword { get; set; }
        public string Database { get; set; }
        public int DBPort { get; set; }
        public string SslMode { get; set; }

    }
}
