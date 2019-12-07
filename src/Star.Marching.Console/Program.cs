using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Star.Marching.Console
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var star = await Core.Star.Create(new IPEndPoint(IPAddress.Parse("192.168.2.105"), 8888));

            int mask = 0;

            while (true)
            {
                mask = (mask + 1) % 5;

                uint frame = Enumerable
                    .Range(0, Core.Star.Leds)
                    .Select(i => i % 5 == mask ? (uint)1 << i : 0)
                    .Aggregate((uint)0, (f, b) => (f | b));

                System.Console.WriteLine(Convert.ToString(frame, 2).PadLeft(25, '0'));

                await star.Write(frame);

                //await Task.Delay(TimeSpan.FromSeconds(0.1));
            }
        }
    }
}
