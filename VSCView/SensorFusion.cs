using System;
using System.Diagnostics;

namespace VSCView
{
    public class SensorFusion
    {
        public sealed class EMACalc
        {// adapted from: https://stackoverflow.com/a/44073605
            readonly double _alpha;
            double _lastAverage = double.NaN;
            double _lastVariance = double.NaN;
            double _lastDataPoint = double.NaN;

            public EMACalc(int lookBack) => _alpha = 2f / (lookBack + 1);

            public double NextValue(double value)
            {
                _lastDataPoint = value;
                return _lastAverage = double.IsNaN(_lastAverage) ?
                    _lastDataPoint : (_lastDataPoint - _lastAverage) * _alpha + _lastAverage;
            }

            public double Deviation() => _lastVariance = double.IsNaN(_lastVariance) ?
                0f : Math.Sqrt(_alpha * Math.Pow(_lastDataPoint - _lastAverage, 2) + (1 - _alpha) * Math.Pow(_lastVariance, 2));
        }

        public sealed class OTFCalibrator
        {
            public double OffsetY { get; private set; }
            public double OffsetP { get; private set; }
            public double OffsetR { get; private set; }

            EMACalc velocity, velEMA;
            int SampleSize, OffsetThreshold;
            double _lastEMD;

            /// <summary>
            /// Calculates an offset for IMU data streams (YPR/AHR) using a timer and zero-point diffing
            /// </summary>
            /// <param name="bufferTime">Seconds to buffer before calculating offset</param>
            public OTFCalibrator(int bufferTime)
            {
                OffsetY = 0f;
                OffsetP = 0f;
                OffsetR = 0f;
                SampleSize = (int)(66.667f * bufferTime); // timer paints at ~66fps x buffer window (s)
                OffsetThreshold = SampleSize;
                velocity = new EMACalc(SampleSize);
                velEMA = new EMACalc(bufferTime);
            }

            /// <summary>
            /// Uses raw IMU sensor data to determine if sensors are idle so offsets for AHR can be calculated
            /// </summary>
            /// <param name="yaw">Heading input to calculate the offset for</param>
            /// <param name="pitch">Pitch input to calculate the offset for</param>
            /// <param name="roll">Roll input to calculate the offset for</param>
            /// <param name="gx">Raw Gyro data of the X axis in degree seconds</param>
            /// <param name="gy">Raw Gyro data of the Y axis in degree seconds</param>
            /// <param name="gz">Raw Gyro data of the Z axis in degree seconds</param>
            /// <param name="ax">Raw Accelerometer data of the X axis</param>
            /// <param name="ay">Raw Accelerometer data of the Y axis</param>
            /// <param name="ay">Raw Accelerometer data of the Z axis</param>
            public void Calibrate(double yaw, double pitch, double roll,
                double gx, double gy, double gz, double ax, double ay, double az)
            {
                // use the normalized sum of gyro and accel readings to derive idle
                double normGyro = Math.Sqrt(gx * gx + gy * gy + gz * gz);
                double normAccel = Math.Sqrt(ax * ax + ay * ay + az * az);
                velocity.NextValue(normGyro + normAccel);
                double velEMD = velEMA.NextValue(velocity.Deviation());
                float floor = 0.01f, ceiling = 0.5f;

#if DEBUG
                Debug.WriteLine($"[ {ax},{ay},{az} ] => {velEMD} = [ {OffsetY},{OffsetP},{OffsetR} ]");
#endif

                if (OffsetThreshold == SampleSize)
                {// if full offset then reset the counter
                    OffsetY = 1.0f - 1.0f - yaw;
                    OffsetP = 1.0f - 1.0f - pitch;
                    OffsetR = 1.0f - 1.0f - roll;
                    OffsetThreshold = 0;
                }

                // only collect when emd acceleration velocities are very near idle
                if ((_lastEMD > floor && _lastEMD <= ceiling) && (velEMD > floor && velEMD <= ceiling))
                    OffsetThreshold++;
                else
                    OffsetThreshold = 0;

                _lastEMD = velEMD;
            }
        }
    }
}
