using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        private readonly IOptions<Configuration> _configuration;
        private readonly ILogger<Service> _logger;

        private IDisposable _subscription;

        public Service(Auto.ISource autoSource, Mqtt.ISource mqttSource, IOptions<Configuration> configuration, ILogger<Service> logger)
        {
            _frames = Frames(autoSource, mqttSource);
            _configuration = configuration;
            _logger = logger;
        }

        private Task<Core.Star> CreateStar(CancellationToken cancellationToken)
        {
            return Core.Star.Create(new IPEndPoint(IPAddress.Parse(_configuration.Value.Host), _configuration.Value.Port));
        }

        private Task<IObservable<Unit>> CreateObservable(Core.Star star, CancellationToken cancellationToken)
        {
            var observable = _frames
                .Do(frame => _logger.LogInformation($"Writing frame: {Convert.ToString(frame, 2)}"))
                .SelectMany(frame => Observable.StartAsync(() => star.WriteAsync(frame)));

            return Task.FromResult(observable);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _subscription = Observable
                .Using(CreateStar, CreateObservable)
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
