using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSCView
{
    /// <summary>
    /// MahonyFilter class adapted from MahonyAHRS and Madgwick's implementation of Mayhony's AHRS algorithm.
    /// </summary>
    /// <remarks>
    /// See: http://www.x-io.co.uk/node/8#open_source_ahrs_and_imu_algorithms
    /// </remarks>
    public class MahonyFilter
    {
        /// <summary>
        /// Gets or sets the sample period.
        /// </summary>
        public float SamplePeriod { get; set; }

        /// <summary>
        /// Gets or sets the algorithm proportional gain.
        /// </summary>
        public float Kp { get; set; }

        /// <summary>
        /// Gets or sets the algorithm integral gain.
        /// </summary>
        public float Ki { get; set; }

        /// <summary>
        /// Gets or sets the Quaternion output.
        /// </summary>
        public float[] Quaternion { get; set; }

        /// <summary>
        /// Gets or sets the integral error.
        /// </summary>
        private float[] eInt { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MahonyFilter"/> class.
        /// </summary>
        /// <param name="samplePeriod">
        /// Sample period.
        /// </param>
        public MahonyFilter(float samplePeriod)
            : this(samplePeriod, 1f, 0f)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MahonyFilter"/> class.
        /// </summary>
        /// <param name="samplePeriod">
        /// Sample period.
        /// </param>
        /// <param name="kp">
        /// Algorithm proportional gain.
        /// </param> 
        public MahonyFilter(float samplePeriod, float kp)
            : this(samplePeriod, kp, 0f)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MahonyFilter"/> class.
        /// </summary>
        /// <param name="samplePeriod">
        /// Sample period.
        /// </param>
        /// <param name="kp">
        /// Algorithm proportional gain.
        /// </param>
        /// <param name="ki">
        /// Algorithm integral gain.
        /// </param>
        public MahonyFilter(float samplePeriod, float kp, float ki)
        {
            SamplePeriod = samplePeriod;
            Kp = kp;
            Ki = ki;
            Quaternion = new float[] { 1f, 0f, 0f, 0f };
            eInt = new float[] { 0f, 0f, 0f };
        }


        /// <summary>
        /// Algorithm IMU update method. Requires only gyroscope and accelerometer data.
        /// </summary>
        /// <param name="gx">
        /// Gyroscope x axis measurement in degrees/s.
        /// </param>
        /// <param name="gy">
        /// Gyroscope y axis measurement in degrees/s.
        /// </param>
        /// <param name="gz">
        /// Gyroscope z axis measurement in degrees/s.
        /// </param>
        /// <param name="ax">
        /// Accelerometer x axis measurement in any calibrated units.
        /// </param>
        /// <param name="ay">
        /// Accelerometer y axis measurement in any calibrated units.
        /// </param>
        /// <param name="az">
        /// Accelerometer z axis measurement in any calibrated units.
        /// </param>
        public float[] UpdateIMU(float gx, float gy, float gz, float ax, float ay, float az)
        {
            float q1 = Quaternion[0], q2 = Quaternion[1], q3 = Quaternion[2], q4 = Quaternion[3];   // short name local variable for readability
            float norm;
            float vx, vy, vz;
            float ex, ey, ez;
            float pa, pb, pc;

            // coefficients for degrees/sec <-> radians/sec
            float radcoeff = 0.0174533f;
            float degcoeff = 1 / radcoeff;

            // convert from gyro degrees/sec -> radians/sec
            float _gx = gx * radcoeff, _gy = gy * radcoeff, _gz = gz * radcoeff;

            // Normalise accelerometer measurement
            norm = (float)Math.Sqrt(ax * ax + ay * ay + az * az);
            if (norm == 0f) return new float[] { 1f, 0f, 0f, 0f }; // handle NaN
            norm = 1 / norm;        // use reciprocal for division
            ax *= norm;
            ay *= norm;
            az *= norm;

            // Estimated direction of gravity
            vx = 2.0f * (q2 * q4 - q1 * q3);
            vy = 2.0f * (q1 * q2 + q3 * q4);
            vz = q1 * q1 - q2 * q2 - q3 * q3 + q4 * q4;

            // Error is cross product between estimated direction and measured direction of gravity
            ex = (ay * vz - az * vy);
            ey = (az * vx - ax * vz);
            ez = (ax * vy - ay * vx);
            if (Ki > 0f)
            {
                eInt[0] += ex;      // accumulate integral error
                eInt[1] += ey;
                eInt[2] += ez;
            }
            else
            {
                eInt[0] = 0.0f;     // prevent integral wind up
                eInt[1] = 0.0f;
                eInt[2] = 0.0f;
            }

            // Apply feedback terms (use gyro data in radians/sec)
            _gx = _gx + Kp * ex + Ki * eInt[0];
            _gy = _gy + Kp * ey + Ki * eInt[1];
            _gz = _gz + Kp * ez + Ki * eInt[2];

            // Integrate rate of change of quaternion
            pa = q2;
            pb = q3;
            pc = q4;
            // use gyro data in radians/sec
            q1 = q1 + (-q2 * _gx - q3 * _gy - q4 * _gz) * (0.5f * SamplePeriod);
            q2 = pa + (q1 * _gx + pb * _gz - pc * _gy) * (0.5f * SamplePeriod);
            q3 = pb + (q1 * _gy - pa * _gz + pc * _gx) * (0.5f * SamplePeriod);
            q4 = pc + (q1 * _gz + pa * _gy - pb * _gx) * (0.5f * SamplePeriod);

            // Normalise quaternion
            norm = (float)Math.Sqrt(q1 * q1 + q2 * q2 + q3 * q3 + q4 * q4);
            norm = 1.0f / norm;
            // convert back to degrees/sec in [ ??? ] format
            Quaternion[0] = q1 * norm * degcoeff;
            Quaternion[1] = q2 * norm * degcoeff;
            Quaternion[2] = q3 * norm * degcoeff;
            Quaternion[3] = q4 * norm * degcoeff;
            return Quaternion;
        }
    }
}
