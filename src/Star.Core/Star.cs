using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Star.Core
{
    public class Star : IDisposable
    {
        private static readonly int[] PinMapping = new[] { 8, 7, 12, 21, 20, 16, 26, 19, 13, 6, 5, 11, 9, 10, 22, 27, 17, 4, 3, 14, 23, 18, 15, 24, 25 };
        public static readonly int Leds = PinMapping.Length;

        public static async Task<Star> Create(IPEndPoint endpoint)
        {
            var driver = new Iot.Device.Pigpio.Driver(endpoint);

            await driver.ConnectAsync();

            // Line above doesn't current wait for connection so
            // delay here to give the socket time to get connected
            await Task.Delay(TimeSpan.FromSeconds(2));

            var gpioController = new System.Device.Gpio.GpioController(System.Device.Gpio.PinNumberingScheme.Logical, driver);

            foreach (int pin in PinMapping)
            {
                gpioController.OpenPin(pin);
                gpioController.SetPinMode(pin, System.Device.Gpio.PinMode.Output);
            }

            return new Star(driver, gpioController);
        }

        private Iot.Device.Pigpio.Driver _driver;
        private System.Device.Gpio.GpioController _gpioController;

        private Star(Iot.Device.Pigpio.Driver driver, System.Device.Gpio.GpioController gpioController)
        {
            _driver = driver;
            _gpioController = gpioController;
        }

        public void Dispose()
        {
            if (_gpioController != null)
            {
                _gpioController.Dispose();
                _gpioController = null;
            }

            if (_driver != null)
            {
                _driver.Dispose();
                _driver = null;
            }
        }

        public Task WriteAsync(uint frame)
        {
            var pins = Enumerable
                .Range(0, PinMapping.Length)
                .Select(i => new System.Device.Gpio.PinValuePair(PinMapping[i], (System.Device.Gpio.PinValue)((1 << i) & frame)))
                .ToArray();

            _gpioController.Write(pins);

            return Task.CompletedTask;
        }
    }
}
