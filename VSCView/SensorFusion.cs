using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace VSCView
{
    public class SensorFusion
    {
        public sealed class EMACalc
        {
            readonly double _alpha;
            double _lastDataPoint = double.NaN, _lastEMA = double.NaN, _lastEMD = double.NaN;

            public double EMA { get { return _lastEMA; } private set { _lastEMA = value; } }
            public double EMD { get { return _lastEMD; } private set { _lastEMD = value; } }

            public EMACalc(int lookBack) => _alpha = 2f / (lookBack + 1);

            public double NextValue(double value)
            {
                _lastDataPoint = value;
                _lastEMA = double.IsNaN(_lastEMA) ? _lastDataPoint : (_lastDataPoint - _lastEMA) * _alpha + _lastEMA;
                _lastEMD = double.IsNaN(_lastEMD) ? _lastEMA : Math.Sqrt(_alpha * Math.Pow(_lastDataPoint - _lastEMA, 2) + (1 - _alpha) * Math.Pow(_lastEMD, 2));
                return _lastEMA;
            }
        }

        public sealed class OTFCalibrator
        {
            public double OffsetY { get; private set; }
            public double OffsetP { get; private set; }
            public double OffsetR { get; private set; }
            const double framerate = 66.6666666667f;

            EMACalc velocity;
            int SampleSize, ThresholdCounter;

            /// <summary>
            /// Calculates an offset for IMU data streams (YPR/AHR) using a timer and zero-point diffing
            /// </summary>
            /// <param name="bufferTime">Seconds to buffer before calculating offset</param>
            public OTFCalibrator(int bufferTime)
            {
                OffsetY = 0f;
                OffsetP = 0f;
                OffsetR = 0f;
                SampleSize = (int)(framerate * bufferTime); // timer paints at ~66fps x buffer window (s)
                velocity = new EMACalc(SampleSize);
                
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
                // using the EMD along with normalized sensor magnitude helps track zero-motion state
                double normGyro = Math.Sqrt(gx * gx + gy * gy + gz * gz);
                double normAccel = Math.Sqrt(ax * ax + ay * ay + az * az);
                double magNorm = normGyro + normAccel, mfloor = 0.001f, mceiling = 0.15f, vceiling = 0.3f;
                velocity.NextValue(magNorm);

#if DEBUG
                Debug.WriteLine($"[ {magNorm} & {velocity.EMD} ] => [ {OffsetY},{OffsetP},{OffsetR} ]");
#endif

                // offset here once our 'bucket' of matching samples is full
                if (ThresholdCounter == SampleSize)
                {
                    OffsetY = 1.0f - 1.0f - yaw;
                    OffsetP = 1.0f - 1.0f - pitch;
                    OffsetR = 1.0f - 1.0f - roll;
                    ThresholdCounter = 0;
                    return; // bail
                }

                // accumulate only when nmag < mceiling > mfloor or EMD <= vceiling > mfloor:
                // should be responsive to both sudden changes and steady-state idle
                if (magNorm > mfloor && magNorm < mceiling || velocity.EMD > mfloor && velocity.EMD <= vceiling)
                    ThresholdCounter++; // just count up to the amount of lookBack samples
                else
                    ThresholdCounter = 0; // reset on spikes
            }
        }
    }
}
