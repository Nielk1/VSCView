using System;
using System.Threading;

namespace VSCView
{
    /// <summary>
    /// Functions as both a cache for sensor data and a filter for auto-calibration
    /// </summary>
    public sealed class SensorCollector
    {
        public class SensorData
        {// data struct for SensorCollector
            public int gX = 0;
            public int gY = 0;
            public int gZ = 0;

            public int aX = 0;
            public int aY = 0;
            public int aZ = 0;

            public double qW = 0f;
            public double qX = 0f;
            public double qY = 0f;
            public double qZ = 0f;

            public double calGyroX = 0f;
            public double calGyroY = 0f;
            public double calGyroZ = 0f;

            /*
            public double calAccelX = 0f;
            public double calAccelY = 0f;
            public double calAccelZ = 0f;
            */

            public double Yaw = 0f;
            public double Pitch = 0f;
            public double Roll = 0f;

            public float GyroTiltFactorX = 0f;
            public float GyroTiltFactorY = 0f;
            public float GyroTiltFactorZ = 0f;
            public float QuatTiltFactorX = 0f;
            public float QuatTiltFactorY = 0f;
            public float QuatTiltFactorZ = 0f;

            public float NormGyroMag = 0f;
        }

        public SensorData Data { get; private set; }

        const double deg2rad = 0.01745329251994329577f;
        int Lookback = 0, usingResource = 0;
        bool Smoothing;
        
        SensorFusion.EMACalc qwEMA;
        SensorFusion.EMACalc qxEMA;
        SensorFusion.EMACalc qyEMA;
        SensorFusion.EMACalc qzEMA;

        SensorFusion.EMACalc gxEMA;
        SensorFusion.EMACalc gyEMA;
        SensorFusion.EMACalc gzEMA;

        /*
        SensorFusion.EMACalc axEMA;
        SensorFusion.EMACalc ayEMA;
        SensorFusion.EMACalc azEMA;
        */

        SensorFusion.EMACalc normData;

        SensorFusion.OTFCalibrator calib;

        public SensorCollector(int lookback, bool smoothing)
        {
            Lookback = lookback;
            Smoothing = smoothing;
            calib = new SensorFusion.OTFCalibrator(8); // buffer ~8s of samples

            qwEMA = new SensorFusion.EMACalc(Lookback);
            qxEMA = new SensorFusion.EMACalc(Lookback);
            qyEMA = new SensorFusion.EMACalc(Lookback);
            qzEMA = new SensorFusion.EMACalc(Lookback);

            gxEMA = new SensorFusion.EMACalc(Lookback);
            gyEMA = new SensorFusion.EMACalc(Lookback);
            gzEMA = new SensorFusion.EMACalc(Lookback);

            /*
            axEMA = new SensorFusion.EMACalc(Lookback);
            ayEMA = new SensorFusion.EMACalc(Lookback);
            azEMA = new SensorFusion.EMACalc(Lookback);
            */
            normData = new SensorFusion.EMACalc(Lookback);
            Data = new SensorData();
        }

        public SensorData Update(ControllerState stateData)
        {// atomic updates
            if (0 == Interlocked.Exchange(ref usingResource, 1))
            {
                Data = new SensorData();
                if (Smoothing)
                {
                    Data.qW = qwEMA.NextValue(stateData.OrientationW * 1.0f / 32768);
                    Data.qX = qxEMA.NextValue(stateData.OrientationX * 1.0f / 32768);
                    Data.qY = qyEMA.NextValue(stateData.OrientationY * 1.0f / 32768);
                    Data.qZ = qzEMA.NextValue(stateData.OrientationZ * 1.0f / 32768);

                    Data.gX = (int)gxEMA.NextValue(stateData.AngularVelocityX);
                    Data.gY = (int)gyEMA.NextValue(stateData.AngularVelocityY);
                    Data.gZ = (int)gzEMA.NextValue(stateData.AngularVelocityZ);

                    /*
                    Data.aX = (int)axEMA.NextValue(stateData.AccelerometerX);
                    Data.aY = (int)ayEMA.NextValue(stateData.AccelerometerY);
                    Data.aZ = (int)azEMA.NextValue(stateData.AccelerometerZ);
                    */
                }
                else if (stateData != null)
                {
                    Data.qW = stateData.OrientationW * 1.0f / 32768;
                    Data.qX = stateData.OrientationX * 1.0f / 32768;
                    Data.qY = stateData.OrientationY * 1.0f / 32768;
                    Data.qZ = stateData.OrientationZ * 1.0f / 32768;

                    Data.gX = stateData.AngularVelocityX;
                    Data.gY = stateData.AngularVelocityY;
                    Data.gZ = stateData.AngularVelocityZ;

                    /*
                    Data.aX = stateData.AccelerometerX;
                    Data.aY = stateData.AccelerometerY;
                    Data.aZ = stateData.AccelerometerZ;
                    */
                }

                Data.GyroTiltFactorX = (float)Data.gX * 0.0001f;
                Data.GyroTiltFactorY = (float)Data.gY * 0.0001f;
                Data.GyroTiltFactorZ = (float)Data.gZ * 0.0001f * -90;
                // sensitivity scale factor 4 -> radian/sec
                Data.calGyroX = Data.gX / 16.4f * deg2rad;
                Data.calGyroY = Data.gY / 16.4f * deg2rad;
                Data.calGyroZ = Data.gZ / 16.4f * deg2rad;
                // sensitivity scale factor 0 -> units/g
                /*
                Data.calAccelX = Data.aX * 1.0f / 16384;
                Data.calAccelY = Data.aY * 1.0f / 16384;
                Data.calAccelZ = Data.aZ * 1.0f / 16384;
                */

                // accumulate smoothed statistical data on normalized gyro sensor magnitude
                Data.NormGyroMag = (float)Math.Sqrt(
                    Math.Abs(Data.calGyroX * Data.calGyroX) +
                    Math.Abs(Data.calGyroY * Data.calGyroY) +
                    Math.Abs(Data.calGyroZ * Data.calGyroZ)
                );
                normData.NextValue(Data.NormGyroMag);

                double[] eulAnglesYPR = ToEulerAngles(Data.qW, Data.qY, Data.qZ, Data.qX);
                Data.Yaw = eulAnglesYPR[0] * 2.0f / Math.PI;
                Data.Pitch = eulAnglesYPR[1] * 2.0f / Math.PI;
                Data.Roll = -(eulAnglesYPR[2] * 2.0f / Math.PI);
                if (double.IsNaN(Data.Yaw)) Data.Yaw = 0f;
                if (double.IsNaN(Data.Pitch)) Data.Pitch = 0f;
                if (double.IsNaN(Data.Roll)) Data.Roll = 0f;

                // auto-calibrate on the fly over several seconds when near idle
                calib.Calibrate(Data.Yaw, Data.Pitch, Data.Roll, Data.NormGyroMag);
                Data.Yaw += calib.OffsetY;
                Data.Pitch += calib.OffsetP;
                Data.Roll += calib.OffsetR;

                Data.QuatTiltFactorX = (float)((2 * Math.Abs(Mod((Data.Pitch - 1) * 0.5f, 2) - 1)) - 1);
                Data.QuatTiltFactorY = (float)((2 * Math.Abs(Mod((Data.Roll - 1) * 0.5f, 2) - 1)) - 1);
                Data.QuatTiltFactorZ = (float)(Data.Yaw * -90.0f);

                Interlocked.Exchange(ref usingResource, 0);
            }
            return Data;
        }

        // HELPERS
        public static double[] ToEulerAngles(double QuaternionW, double QuaternionX, double QuaternionY, double QuaternionZ)
        {
            double sqw = QuaternionW * QuaternionW;
            double sqx = QuaternionX * QuaternionX;
            double sqy = QuaternionY * QuaternionY;
            double sqz = QuaternionZ * QuaternionZ;

            // If quaternion is normalised the unit is one, otherwise it is the correction factor
            double unit = sqx + sqy + sqz + sqw;
            double test = QuaternionX * QuaternionY + QuaternionZ * QuaternionW;

            if (test > 0.4999f * unit)                              // 0.4999f OR 0.5f - EPSILON
            {
                // Singularity at north pole
                return new double[] {
                    2f * Math.Atan2(QuaternionX, QuaternionW),  // Yaw
                    Math.PI * 0.5f,                         // Pitch
                    0f                                // Roll
                };
            }
            else if (test < -0.4999f * unit)                        // -0.4999f OR -0.5f + EPSILON
            {
                // Singularity at south pole
                return new double[] {
                    -2f * Math.Atan2(QuaternionX, QuaternionW), // Yaw
                    -Math.PI * 0.5f,                        // Pitch
                    0f                                // Roll
                };
            }
            else
            {
                return new double[] {
                    Math.Atan2(2f * QuaternionY * QuaternionW - 2f * QuaternionX * QuaternionZ, sqx - sqy - sqz + sqw),       // Yaw
                    Math.Asin(2f * test / unit),                                             // Pitch
                    Math.Atan2(2f * QuaternionX * QuaternionW - 2f * QuaternionY * QuaternionZ, -sqx + sqy - sqz + sqw)      // Roll
                };
            }
        }

        public static double Mod(double x, double m)
        {
            return (x % m + m) % m;
        }
    }
}
