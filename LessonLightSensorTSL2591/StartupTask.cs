using Windows.ApplicationModel.Background;
using System.Threading.Tasks;

namespace LessonLightSensorTSL2591
{
    public sealed class StartupTask : IBackgroundTask
    {
        // TSL2591 Sensor
        private TSL2591 TSL2591Sensor = new TSL2591();

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
