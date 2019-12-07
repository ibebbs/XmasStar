using Microsoft.Extensions.Options;
using System;
using System.Net.Mqtt;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Star.Mqtt.Console.Mqtt
{
    public interface ISource
    {
        IObservable<uint> Create();
    }

    public class Source : ISource
    {
        private readonly IOptions<Configuration> _configuration;

        public Source(IOptions<Configuration> configuration)
        {
            _configuration = configuration;
        }

        public IObservable<uint> Create()
        {
            return Observable.Create<uint>(
                async observer =>
                {
                    var config = new MqttConfiguration
                    {
                        Port = _configuration.Value.Port,
                        MaximumQualityOfService = MqttQualityOfService.ExactlyOnce,
                        AllowWildcardsInTopicFilters = true
                    };

                    var client = await MqttClient.CreateAsync(_configuration.Value.Broker, config);

                    _ = await client.ConnectAsync();

                    await client.SubscribeAsync(_configuration.Value.Topic, MqttQualityOfService.AtLeastOnce);

                    var subscription = client
                        .MessageStream
                        .Where(message => message.Payload.Length == 4)
                        .Select(message => BitConverter.ToUInt32(message.Payload))
                        .Subscribe(observer);

                    return new CompositeDisposable(
                        subscription,
                        client
                    );
                }
            );
        }
    }
}
