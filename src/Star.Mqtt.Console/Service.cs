using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace Star.Mqtt.Console
{
    public class Service : IHostedService
    {
        private static IObservable<uint> Frames(Auto.ISource autoSource, Mqtt.ISource mqttSource)
        {
            IObservable<uint> autoFrames = autoSource.Create().Publish().RefCount();
            IObservable<uint> mqttFrames = mqttSource.Create().Publish().RefCount();
            IConnectableObservable<uint> mqttFramesWithReplay = mqttFrames.Replay(1);

            var mqttFrameTimeout = mqttFrames
                .Timeout(TimeSpan.FromSeconds(10))
                .IgnoreElements()
                .Materialize()
                .Where(notification => notification.Kind == NotificationKind.OnError);

            var autoFramesUntilMqttFrames = autoFrames.TakeUntil(mqttFrames);
            var mqttFramesUntilTimeout = mqttFramesWithReplay.TakeUntil(mqttFrameTimeout);

            return Observable.Using(
                () => mqttFramesWithReplay.Connect(),
                _ => Observable.Defer(() => Observable.Concat(autoFramesUntilMqttFrames, mqttFramesUntilTimeout)).Repeat()
            );
        }

        private readonly IObservable<uint> _frames;
        private readonly ILogger<Service> _logger;

        private IDisposable _subscription;

        public Service(Auto.ISource autoSource, Mqtt.ISource mqttSource, ILogger<Service> logger)
        {
            _frames = Frames(autoSource, mqttSource);
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _subscription = Observable
                .Using(
                    cancellationToken => Core.Star.Create(new IPEndPoint(IPAddress.Parse("192.168.2.105"), 8888)),
                    (star, cancellationToken) => Task.FromResult(_frames
                        .Do(frame => _logger.LogInformation($"Writing frame: {Convert.ToString(frame, 2)}"))
                        .SelectMany(frame => Observable.StartAsync(() => star.WriteAsync(frame)))))
                .Subscribe();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (_subscription != null)
            {
                _subscription.Dispose();
                _subscription = null;
            }

            return Task.CompletedTask;
        }
    }
}
