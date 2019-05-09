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
            double _lastVariance = 0f, _lastZScore = 0f;

            public double EMA { get { return _lastEMA; } private set { _lastEMA = value; } }
            public double EMD { get { return _lastEMD; } private set { _lastEMD = value; } }
            public double ZScore { get { return _lastZScore; } private set { _lastZScore = value; } }

            public EMACalc(int lookBack) => _alpha = 2f / (lookBack + 1);

            public double NextValue(double value)
            {
                _lastDataPoint = value;
                _lastEMA = double.IsNaN(_lastEMA) ? _lastDataPoint : (_lastDataPoint - _lastEMA) * _alpha + _lastEMA;
                _lastVariance = (1 - _alpha) * (_lastVariance + _alpha * Math.Pow(_lastDataPoint - _lastEMA,2));
                _lastEMD = double.IsNaN(_lastEMD) ? 0f : Math.Sqrt(_lastVariance);
                _lastZScore = (_lastDataPoint - _lastEMA) / _lastEMD;
                return _lastEMA;
            }
        }

        public sealed class OTFCalibrator
        {
            public double OffsetY { get; private set; }
            public double OffsetP { get; private set; }
            public double OffsetR { get; private set; }
            const double framerate = 30.0f;

            EMACalc velocity;
            int SampleSize, ThresholdCounter;
            double _lastEMD = 0f;

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
                double mgx = Math.Abs(gx), mgy = Math.Abs(gy), mgz = Math.Abs(gz);
                double max = Math.Abs(ax), may = Math.Abs(ay), maz = Math.Abs(az);
                double normGyro = Math.Sqrt(mgx * mgx + mgy * mgy + mgz * mgz);
                double normAccel = Math.Sqrt(max * max + may * may + maz * maz);
                double magNorm = normGyro + normAccel, ceiling = 0.22f;
                velocity.NextValue(magNorm);
                // search for motion based on sensor input magnitude:
                // movement > z-score:EMD > previous reading (local maxima search)
                bool signal = magNorm <= ceiling || velocity.ZScore < velocity.EMD || velocity.EMD < _lastEMD ? false : true;
                _lastEMD = velocity.EMD;

#if DEBUG
                Debug.WriteLine($"[{magNorm} & {velocity.EMD} & {velocity.ZScore}] : [{signal}] => [ {OffsetY},{OffsetP},{OffsetR} ]");
#endif

                // offset here once our 'bucket' of matching samples is full
                if (ThresholdCounter == SampleSize)
                {
                    OffsetY = 1.0f - 1.0f - yaw;
                    OffsetP = 1.0f - 1.0f - pitch;
                    OffsetR = 1.0f - 1.0f - roll;
                    ThresholdCounter = 0;
                    return;
                }

                if (!signal)
                    ThresholdCounter++; // just count up to the size of our buffer
                else
                    ThresholdCounter = 0; // reset on spikes
            }
        }
    }
}
