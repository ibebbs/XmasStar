namespace Star.Mqtt.Console.Mqtt
{
    public class Configuration
    {
        public string Broker { get; set; } = "192.168.1.24";

        public int Port { get; set; } = 1883;

        public string Topic { get; set; } = "home/xmastree";
    }
}
