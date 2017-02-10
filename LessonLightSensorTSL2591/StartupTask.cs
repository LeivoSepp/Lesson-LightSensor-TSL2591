using Windows.ApplicationModel.Background;
using System.Threading.Tasks;

namespace LessonLightSensorTSL2591
{
    public sealed class StartupTask : IBackgroundTask
    {
        // TSL2591 Sensor
        private TSL2591 TSL2591Sensor = new TSL2591();
        //byte gain = TSL2591.GAIN_MED;
        //byte itime = TSL2591.INT_TIME_200MS;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            while (true)
            {
                double currentLux = TSL2591Sensor.GetLux();
                Task.Delay(1000).Wait();
            }
        }
    }
}
