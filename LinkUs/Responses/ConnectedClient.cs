namespace LinkUs.Responses
{
    public class ConnectedClient
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string MachineName { get; set; }
        public string OperatingSystem { get; set; }
        public string PublicIp { get; set; }
    }
}