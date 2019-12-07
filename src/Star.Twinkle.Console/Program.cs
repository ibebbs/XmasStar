using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Star.Twinkle.Console
{
    class Program
    {
        private static readonly Random Random = new Random();

        static async Task Main(string[] args)
        {
            var star = await Core.Star.Create(new IPEndPoint(IPAddress.Parse("192.168.2.105"), 8888));

            while (true)
            {
                uint frame = Enumerable
                    .Range(0, Core.Star.Leds)
                    .Select(i => Random.NextDouble() > 0.05 ? (uint) 1 << i : 0)
                    .Aggregate((uint)0, (f, b) => (f | b));

                System.Console.WriteLine(Convert.ToString(frame, 2).PadLeft(25, '0'));

                await star.WriteAsync(frame);

                //await Task.Delay(TimeSpan.FromSeconds(0.1));
            }
        }
    }
}
