# Adafruit Light Sensor TSL2591
This project is an example on how to use the Adafruit light sensor TSL2591 in Raspberry PI and Windows 10 IoT Core.

## What is this sensor?
https://www.adafruit.com/products/1980

![image](https://cloud.githubusercontent.com/assets/13704023/22854428/2092a836-f077-11e6-9ec5-2808ea62d6fc.png)

The TSL2591 luminosity sensor is an advanced digital light sensor, ideal for use in a wide range of light situations. 
This sensor is very precise, allowing for exact lux calculations and can be configured for different gain/timing ranges 
to detect light ranges from 188 uLux up to 88,000 Lux on the fly.

## How to connect this sensor into Raspberry PI?
To connect this sensor to Raspberry PI you need 4 wires. Two of the wires used for voltage Vin (+3V from Raspberry) and ground GND and remaining two are used for data. 
As this is digital sensor, it uses I2C protocol to communicate with the Raspberry. For I2C we need two wires, Data (SDA) and Clock (SCL).
Connect sensor SDA and SCL pins accordingly to Raspberry SDA and SCL pins. 

## How do I write the code?
I made it very simple for you. You just need to add NuGet package RobootikaCOM.TSL2591 to your project and you are almost done :)

Right-click in your project name and then "Manage NuGet packages"
![image](https://cloud.githubusercontent.com/assets/13704023/22802711/964f83d6-ef1a-11e6-9e7e-398257c2eda0.png)

Open "Browse" tab, write "robootika" in search-window and then install RobootikaCOM.TSL2591 package into your project.
![image](https://cloud.githubusercontent.com/assets/13704023/22802827/0ba11ed8-ef1b-11e6-8f46-64a8bf8fd432.png)

After adding this NuGet package, you just need to write 2 lines of code.

1. Create an object for sensor: 
````C#
        private TSL2591 TSL2591Sensor = new TSL2591();
````

2. Write a while-loop, to read data from the sensor in every 1 sec.
````C#
            while (true)
            {
                double currentLux = TSL2591Sensor.GetLux();
                Task.Delay(1000).Wait();
            }
````

Final code looks like this. 
If you run it, you do not see anything, because it just reads the data, but it doesnt show it anywhere.
You need to integrate this project with my other example, where I teach how to send this data into Azure.

````C#
using Windows.ApplicationModel.Background;
using System.Threading.Tasks;

namespace LessonLightSensorTSL2591
{
    public sealed class StartupTask : IBackgroundTask
    {
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
````

## Advanced sensor tuning: timing and gain
This sensor support five different timing and three gain options. 

1. Timing: this means how many samples the sensor will take before calculating the light level.
Changing the integration time gives you a longer time over which to sense light. Longer timelines are slower, but are good in very low light situtations!
This sensor has three parameters for timing. 
   1. 100ms the shortest measure time, use in bright light
   2. 200ms average measure time, use in medium light
   3. 300ms average measure time, use in medium light
   4. 400ms long measure time, use in dim light
   5. 500ms long measure time, use in dim light
````C#
        TSL2591.INT_TIME_100MS;
        TSL2591.INT_TIME_200MS;
        TSL2591.INT_TIME_300MS;
        TSL2591.INT_TIME_400MS;
        TSL2591.INT_TIME_500MS;
````
2. Gain: You can change the gain on the fly, to adapt to brighter/dimmer light situations. 
   1. TSL2591.GAIN_LOW: use in bright light
   2. TSL2591.GAIN_MED: use in average light
   3. TSL2591.GAIN_HIGH: use in very dim light
   4. TSL2591.GAIN_MAX: use in darkness

Tu use timing and gain, you need to write one additional line of code. First parameter for method SetTiming is gain and second parameter is timing. 
For example, setting gain MAX and timing maximum (500ms), you can measure almost complete darkness.
````C#
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            TSL2591Sensor.SetGain(TSL2591.GAIN_HIGH, TSL2591.INT_TIME_500MS);
            while (true)
            {
                double currentLux = TSL2591Sensor.GetLux();
                Task.Delay(1000).Wait();
            }
        }
````
## Can I change I2C address?
I2C address is used to communicate with the sensor. Many sensors have I2C address hardcoded, and this sensor exactly this.
**You cannot change I2C address, it is fixed to 0x29**

## Sensor technical Details

* Extremely wide dynamic range 600,000,000:1
* Lux Range: 188 uLux sensitivity, up to 88,000 Lux input measurements.
* Temperature range: -30 to 80 *C
* Voltage range: 3.3-5V into onboard regulator
* Interface: I2C
* This board/chip uses I2C 7-bit address 0x29 (fixed)


## Indoor and outdoor lighting conditions

Illuminance | Example
--- | --- 
0.002 lux | Moonless clear night sky
0.2 lux | Design minimum for emergency lighting (AS2293).
0.27 - 1 lux | Full moon on a clear night
3.4 lux | Dark limit of civil twilight under a clear sky
50 lux | Family living room
80 lux | Hallway/toilet
100 lux | Very dark overcast day
300 - 500 lux | Sunrise or sunset on a clear day. Well-lit office area.
1,000 lux | Overcast day; typical TV studio lighting
10,000 - 25,000 lux | Full daylight (not direct sun)
32,000 - 130,000 lux | Direct sunlight
