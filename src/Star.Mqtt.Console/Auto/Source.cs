using System;
using System.Linq;
using System.Reactive.Linq;

namespace Star.Mqtt.Console.Auto
{
    public interface ISource
    {
        IObservable<uint> Create();
    }

    public class Source : ISource
    {
        private static readonly Random Random = new Random();

        private static readonly IObservable<uint>[] Sources = new[]
        {
            Marching,
            Twinkle
        };

        private static uint GenerateMarch(uint mask)
        {
            return Enumerable
                .Range(0, Core.Star.Leds)
                .Select(i => i % 5 == mask ? (uint)1 << i : 0)
                .Aggregate((uint)0, (f, b) => (f | b));
        }

        private static uint GenerateTwinkle()
        {
            return Enumerable
                .Range(0, Core.Star.Leds)
                .Select(i => Random.NextDouble() > 0.05 ? (uint)1 << i : 0)
                .Aggregate((uint)0, (f, b) => (f | b));
        }

        private static IObservable<uint> Marching
        {
            get 
            {
                return Observable.Interval(TimeSpan.FromSeconds(0.2)).Select((_, index) => GenerateMarch((uint)index % 5));
            }
        }

        private static IObservable<uint> Twinkle
        {
            get
            {
                return Observable.Interval(TimeSpan.FromSeconds(0.2)).Select(_ => GenerateTwinkle());
            }
        }

        public IObservable<uint> Create()
        {
            return Observable
                .Interval(TimeSpan.FromSeconds(30))
                .StartWith(0)
                .Select(_ => Random.Next(0, Sources.Length))
                .Select(index => Sources[index])
                .Switch();
        }
    }
}
