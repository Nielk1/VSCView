using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace VSCView
{
    public class SensorFusion
    {
        public sealed class EMACalc
        {
            readonly double _alpha;
            double _lastDataPoint = double.NaN, _lastEMA = double.NaN, _emaBeta = double.NaN;

            public double EMA { get { return _lastEMA; } private set { _lastEMA = value; } }

            public EMACalc(int lookBack) => _alpha = 2f / (lookBack + 1);

            public double NextValue(double value)
            {
                _lastDataPoint = value;
                _lastEMA = double.IsNaN(_lastEMA) ? _lastDataPoint : (_lastDataPoint - _lastEMA) * _alpha + _lastEMA;
                _emaBeta = _lastDataPoint - _lastEMA;
                return _lastEMA;
            }
        }

        public sealed class OTFCalibrator
        {
            public double OffsetY { get; private set; }
            public double OffsetP { get; private set; }
            public double OffsetR { get; private set; }

            int SamplingTime;
            double Elapsed;
            Stopwatch watch = new Stopwatch();

            /// <summary>
            /// Calculates an offset for IMU data streams (YPR/AHR) using a timer and zero-point diffing
            /// </summary>
            /// <param name="bufferTime">Seconds to buffer before calculating offset</param>
            public OTFCalibrator(int bufferTime)
            {
                OffsetY = 0f;
                OffsetP = 0f;
                OffsetR = 0f;
                SamplingTime = bufferTime * 1000;
                watch.Start();
            }

            /// <summary>
            /// Uses raw IMU sensor data to determine if sensors are idle so offsets for AHR can be calculated
            /// </summary>
            /// <param name="yaw">Heading input to calculate the offset for</param>
            /// <param name="pitch">Pitch input to calculate the offset for</param>
            /// <param name="roll">Roll input to calculate the offset for</param>
            /// <param name="gyroMag">Smoothed and normalized Gyro sensor magnitude in absolute radian/sec</param>
            public void Calibrate(double yaw, double pitch, double roll, float gyroMag)
            {
                if (!watch.IsRunning)
                    watch.Restart();

                Elapsed = watch.ElapsedMilliseconds;
                // we expect smoothed, normalized, and otherwise raw Gyro data here
                // 0.16 is the rough noise ceiling - arrived at by trial and error
                bool signal = gyroMag <= 0.16f ? false : true;

                //Debug.WriteLine($"{gyroMag} => [{signal}] <= 0.16 ==> [{Elapsed} ~= {SamplingTime}]");

                // offset here once our 'bucket' of matching samples is full
                if (Elapsed >= SamplingTime)
                {
                    OffsetY = 1.0f - 1.0f - yaw;
                    OffsetP = 1.0f - 1.0f - pitch;
                    OffsetR = 1.0f - 1.0f - roll;
                    watch.Stop();
                    return;
                }

                if (signal)
                    watch.Restart();
            }
        }
    }
}
