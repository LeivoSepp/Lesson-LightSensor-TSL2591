using System;
using Windows.Devices.I2c;

namespace LessonLightSensorTSL2591
{
    class TSL2591
    {
        // TSL Address Constants
        public const int TSL2591_ADDR = 0x29;    // address with '0' shorted on board 
        // TSL Commands
        private const int TSL2591_CMD = 0xA0;
        //private const int TSL2591_CMD_CLEAR = 0xE7;
        // TSL Registers
        private const int TSL2591_REG_ENABLE = 0x00;
        private const int TSL2591_REG_CONTROL = 0x01;
        //private const int TSL2561_REG_TIMING = 0x01;
        //private const int TSL2591_REG_THRESH_L = 0x04;
        //private const int TSL2591_REG_THRESH_H = 0x06;
        //private const int TSL2561_REG_INTCTL = 0x06;
        private const int TSL2591_REG_ID = 0x12;
        private const int TSL2591_REG_DATA_0 = 0x14;
        private const int TSL2591_REG_DATA_1 = 0x16;

        //private const int TSL2591_STATUS_REG = 0x13;

        public const int TSL2591_GAIN_1X = 0x00;
        public const int TSL2591_GAIN_25X = 0x10;
        public const int TSL2591_GAIN_428X = 0x20;
        public const int TSL2591_GAIN_9876X = 0x30;
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

        public TSL2591(ref I2cDevice I2CDevice)
        {
            this.I2C = I2CDevice;
        }

        // TSL2561 Sensor Power up
        public void PowerUp()
        {
            write8(TSL2591_REG_ENABLE, 0x03);
        }

        // TSL2561 Sensor Power down
        public void PowerDown()
        {
            write8(TSL2591_REG_ENABLE, 0x00);
        }

        // Retrieve TSL ID
        public byte GetId()
        {
            return I2CRead8(TSL2591_REG_ID);
        }

        public void SetGain(byte gain, byte int_time)
        {
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
        public double GetLux(uint gain, uint itime, uint CH0, uint CH1)
        {
            double d0, d1;
            double lux = 0.0;

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
                default: again = 0; break;
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
