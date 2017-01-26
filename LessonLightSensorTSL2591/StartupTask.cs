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
        private const string I2C_CONTROLLER_NAME = "I2C1";
        private I2cDevice I2CDev;

        // TSL2591 Sensor
        private TSL2591 TSL2591Sensor;
        byte gain = TSL2591.TSL2591_GAIN_MED;
        byte itime = TSL2591.TSL2591_INT_TIME_200MS;
        private static double CurrentLux = 0;

        private async void InitializeI2CDevice()
        {
            try
            {
                // Initialize I2C device 
                var settings = new I2cConnectionSettings(TSL2591.TSL2591_ADDR);
                settings.BusSpeed = I2cBusSpeed.FastMode;
                settings.SharingMode = I2cSharingMode.Shared;

                string aqs = I2cDevice.GetDeviceSelector(I2C_CONTROLLER_NAME);
                var dis = await DeviceInformation.FindAllAsync(aqs);
                I2CDev = await I2cDevice.FromIdAsync(dis[0].Id, settings);
            }
            catch
            {
                return;
            }
            initializeSensor();
        }

        private void initializeSensor()
        {
            TSL2591Sensor = new TSL2591(ref I2CDev);
            TSL2591Sensor.SetGain(gain, itime);
            TSL2591Sensor.PowerUp();
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
            InitializeI2CDevice();
            initDevice();
            while (true)
            {
                uint[] Data = TSL2591Sensor.GetData(); //data[0]:VISIBLE, data[1]:INFRARED
                CurrentLux = TSL2591Sensor.GetLux(gain, itime, Data[0], Data[1]);
                String strLux = String.Format("{0:0.00}", CurrentLux);
                SendMessages(strLux);
                Task.Delay(1000).Wait();
            }
        }
    }
}
