using System;
using System.Text;
using Windows.ApplicationModel.Background;
using Windows.Devices.I2c;
using Windows.Devices.Enumeration;
using Microsoft.Devices.Tpm;
using Microsoft.Azure.Devices.Client;
using System.Threading.Tasks;

namespace LessonLightSensorTSL2591
{
    public sealed class StartupTask : IBackgroundTask
    {
        // TSL2591 Sensor
        private TSL2591 TSL2591Sensor;
        byte gain = TSL2591.TSL2591_GAIN_MED;
        byte itime = TSL2591.TSL2591_INT_TIME_200MS;
        private static double CurrentLux = 0;

        private void initializeSensor()
        {
            TSL2591Sensor = new TSL2591();
            TSL2591Sensor.SetGain(gain, itime);
        }
        private void initDevice()
        {
            TpmDevice device = new TpmDevice(0);
            string hubUri = device.GetHostName();
            string deviceId = device.GetDeviceId();
            string sasToken = device.GetSASToken();
            _sendDeviceClient = DeviceClient.Create(hubUri, AuthenticationMethodFactory.CreateAuthenticationWithToken(deviceId, sasToken), TransportType.Amqp);
        }
        private DeviceClient _sendDeviceClient;
        private async void SendMessages(string strMessage)
        {
            string messageString = strMessage;
            var message = new Message(Encoding.ASCII.GetBytes(messageString));
            await _sendDeviceClient.SendEventAsync(message);
        }
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            initDevice();
            initializeSensor();
            while (true)
            {
                CurrentLux = TSL2591Sensor.GetLux(gain, itime);
                String strLux = String.Format("{0:0.00}", CurrentLux);
                SendMessages(strLux);
                Task.Delay(1000).Wait();
            }
        }
    }
}
