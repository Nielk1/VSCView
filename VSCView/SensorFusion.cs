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
            double _lastDataPoint = double.NaN, _lastEMA = double.NaN, _lastEMD = double.NaN;
            double _lastVariance = 0f, _lastZScore = 0f, _emaBeta = 0f;

            public double EMA { get { return _lastEMA; } private set { _lastEMA = value; } }
            //public double EMD { get { return _lastEMD; } private set { _lastEMD = value; } }
            //public double ZScore { get { return _lastZScore; } private set { _lastZScore = value; } }

            public EMACalc(int lookBack) => _alpha = 2f / (lookBack + 1);

            public double NextValue(double value)
            {
                _lastDataPoint = value;
                _lastEMA = double.IsNaN(_lastEMA) ? _lastDataPoint : (_lastDataPoint - _lastEMA) * _alpha + _lastEMA;
                _emaBeta = _lastDataPoint - _lastEMA;
                //_lastVariance = (1 - _alpha) * (_lastVariance + _alpha * _emaBeta * _emaBeta);
                //_lastEMD = double.IsNaN(_lastEMD) ? 0f : ApproxSqrt((float)_lastVariance);
                //_lastZScore = (_lastDataPoint - _lastEMA) / _lastEMD;
                return _lastEMA;
            }

            public static float ApproxSqrt(float z)
            {// see: https://www.compuphase.com/cmetric.htm
                if (z == 0) return 0;
                FloatIntUnion u;
                u.tmp = 0;
                u.f = z;
                u.tmp -= 1 << 23; /* Subtract 2^m. */
                u.tmp >>= 1; /* Divide by 2. */
                u.tmp += 1 << 29; /* Add ((b + 1) / 2) * 2^m. */
                return u.f;
            }

            [StructLayout(LayoutKind.Explicit)]
            private struct FloatIntUnion
            {
                [FieldOffset(0)]
                public float f;

                [FieldOffset(0)]
                public int tmp;
            }
        }

        public sealed class OTFCalibrator
        {
            public double OffsetY { get; private set; }
            public double OffsetP { get; private set; }
            public double OffsetR { get; private set; }

            int SampleSize, ThresholdCounter;

            /// <summary>
            /// Calculates an offset for IMU data streams (YPR/AHR) using a timer and zero-point diffing
            /// </summary>
            /// <param name="bufferTime">Seconds to buffer before calculating offset</param>
            public OTFCalibrator(int bufferTime, int framerate)
            {
                OffsetY = 0f;
                OffsetP = 0f;
                OffsetR = 0f;
                SampleSize = (int)(framerate * bufferTime);
                
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
                // search for motion based on sensor input magnitude...
                // this is a tradeoff between capturing fine movement and triggering false positives:
                // EMA smoothed and normalized magnitude of movement (<= ceiling)
                bool signal = gyroMag <= 0.16f ? false : true;

#if DEBUG
                Debug.WriteLine($"[ {gyroMag} ] : [ {signal} ] => [ {OffsetY},{OffsetP},{OffsetR} ]");
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
