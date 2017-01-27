using System;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace LessonLightSensorTSL2591
{
    class TSL2591
    {
        // Address Constant
        public const int TSL2591_ADDR = 0x29;
        // Commands
        private const int TSL2591_CMD = 0xA0;

        // Registers
        private const int TSL2591_REG_ENABLE = 0x00;
        private const int TSL2591_REG_CONTROL = 0x01;
        private const int TSL2591_REG_ID = 0x12;
        private const int TSL2591_REG_DATA_0 = 0x14;
        private const int TSL2591_REG_DATA_1 = 0x16;

        //private const int TSL2591_STATUS_REG = 0x13;

        /*
         LOW gain: use in bright light to avoid sensor saturation
         MED: use in low light to boost sensitivity 
         HIGH: use in very low light condition
         */
        public const int TSL2591_GAIN_LOW = 0x00;
        public const int TSL2591_GAIN_MED = 0x10;
        public const int TSL2591_GAIN_HIGH = 0x20;
        public const int TSL2591_GAIN_MAX = 0x30;
        /*
         100ms: fast reading but low resolution
         600ms: slow reading but best accuracy
         */
        public const int TSL2591_INT_TIME_100MS = 0x00;
        public const int TSL2591_INT_TIME_200MS = 0x01;
        public const int TSL2591_INT_TIME_300MS = 0x02;
        public const int TSL2591_INT_TIME_400MS = 0x03;
        public const int TSL2591_INT_TIME_500MS = 0x04;
        public const int TSL2591_INT_TIME_600MS = 0x05;
        // Constants for LUX calculation
        private const double LUX_DF = 408.0;
        private const double LUX_COEFB = 1.64;  // CH0 coefficient
        private const double LUX_COEFC = 0.59;  // CH1 coefficient A
        private const double LUX_COEFD = 0.86;  //CH2 coefficient B

        // I2C Device
        private I2cDevice I2C;
        private int I2C_ADDRESS { get; set; } = TSL2591_ADDR;
        public TSL2591(int i2cAddress = TSL2591_ADDR)
        {
            I2C_ADDRESS = i2cAddress;
        }
        public static bool IsInitialised { get; private set; } = false;
        private void Initialise()
        {
            if (!IsInitialised)
            {
                EnsureInitializedAsync().Wait();
            }
        }
        private async Task EnsureInitializedAsync()
        {
            if (IsInitialised) { return; }
            try
            {
                var settings = new I2cConnectionSettings(I2C_ADDRESS);
                settings.BusSpeed = I2cBusSpeed.FastMode;
                settings.SharingMode = I2cSharingMode.Shared;
                string aqs = I2cDevice.GetDeviceSelector("I2C1");         /* Find the selector string for the I2C bus controller */
                var dis = await DeviceInformation.FindAllAsync(aqs);      /* Find the I2C bus controller device with our selector string           */
                I2C = await I2cDevice.FromIdAsync(dis[0].Id, settings);   /* Create an I2cDevice with our selected bus controller and I2C settings */

                PowerUp();
                IsInitialised = true;
            }
            catch (Exception ex)
            {
                throw new Exception("I2C Initialization Failed", ex);
            }
        }
        // Sensor Power up
        public void PowerUp()
        {
            write8(TSL2591_REG_ENABLE, 0x03);
        }

        // Sensor Power down
        public void PowerDown()
        {
            write8(TSL2591_REG_ENABLE, 0x00);
        }

        // Retrieve sensor ID
        public byte GetId()
        {
            return I2CRead8(TSL2591_REG_ID);
        }

        public void SetGain(byte gain, byte int_time)
        {
            Initialise();
            write8(TSL2591_REG_CONTROL, (byte)(gain + int_time));
        }

        public uint[] GetData()
        {
            uint[] Data = new uint[2];

            Data[0] = I2CRead16(TSL2591_REG_DATA_0);
            Data[1] = I2CRead16(TSL2591_REG_DATA_1);

            return Data;
        }
        // Calculate Lux
        public double GetLux(uint gain, uint itime)
        {
            Initialise();
            uint[] Data = GetData();
            uint CH0 = Data[0];
            uint CH1 = Data[1];

            double d0, d1;
            double lux = 0.0;

            // Determine if either sensor saturated (0xFFFF)
            if ((CH0 == 0xFFFF) || (CH1 == 0xFFFF))
            {
                lux = 0.0;
                return lux;
            }
            // Convert from unsigned integer to floating point
            d0 = CH0; d1 = CH1;

            int atime = (int)(itime + 1) * 100;
            double again;
            switch (gain)
            {
                case 0x00: again = 1; break;
                case 0x10: again = 25; break;
                case 0x20: again = 428; break;
                case 0x30: again = 9876; break;
                default: again = 1; break;
            }
            double cpl = (atime * again) / LUX_DF;
            double lux1 = (d0 - (LUX_COEFB * d1)) / cpl;
            double lux2 = ((LUX_COEFC * d0) - (LUX_COEFD * d1)) / cpl;
            return Math.Max(lux1, lux2);
        }
        // Write byte
        private void write8(byte addr, byte cmd)
        {
            byte[] Command = new byte[] { (byte)((addr) | TSL2591_CMD), cmd };

            I2C.Write(Command);
        }
        // Read byte
        private byte I2CRead8(byte addr)
        {
            byte[] aaddr = new byte[] { (byte)((addr) | TSL2591_CMD) };
            byte[] data = new byte[1];

            I2C.WriteRead(aaddr, data);

            return data[0];
        }
        // Read integer
        private ushort I2CRead16(byte addr)
        {
            byte[] aaddr = new byte[] { (byte)((addr) | TSL2591_CMD) };
            byte[] data = new byte[2];

            I2C.WriteRead(aaddr, data);

            return (ushort)((data[1] << 8) | (data[0]));
        }
    }
}
