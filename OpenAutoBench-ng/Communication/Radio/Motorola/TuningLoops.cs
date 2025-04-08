using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OpenAutoBench_ng.Communication.Radio;
using OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase;
using OpenAutoBench_ng.OpenAutoBench;

namespace OpenAutoBench_ng.Communication.Radio.Motorola
{
    public class PIDGains
    {
        public double Kp { get; set; }
        public double Ki { get; set; }
        public double Kd { get; set; }

        /// <summary>
        /// Create a new set of PID loop gains
        /// </summary>
        /// <param name="kp">proportional gain</param>
        /// <param name="ki">integral gain</param>
        /// <param name="kd">derivative gain</param>
        public PIDGains(double kp, double ki, double kd)
        {
            Kp = kp;
            Ki = ki;
            Kd = kd;
        }
    }

    public class TuningLoops
    {
        public class SoftpotTuningLoop
        {
            // The radio we're tuning
            public MotorolaXCMPRadioBase Radio { get; private set; }

            // The softpot we're adjusting
            public MotorolaXCMPRadioBase.SoftpotType SoftpotType { get; private set; }

            // Name of the softpot
            public string SoftpotName { 
                get
                {
                    return Enum.GetName(typeof(MotorolaXCMPRadioBase.SoftpotType), SoftpotType);
                } 
            }

            // The function used to obtain the process measurement
            public Func<Task<float>> MeasFunc { get; private set; }

            // Target measurement value
            public double MeasTarget { get; private set; }

            // Measurement tolerance
            public double MeasTolerance { get; private set; }

            // Log print callback
            public Action<string> LogCallback { get; private set; }

            // The byte length of the softpot value (variable, so we determine it by reading the softpots first)
            private uint spByteLength = 4;

            // Softpot min/max limits
            private int softpotMin;
            private int softpotMax;

            // Duration the tune process will run for before timing out
            private uint timeOut;

            // PID controller
            private CSPID.PIDController pid;

            // PID gains
            private PIDGains gains;

            // Error range for PID controller (determined from provided measurement range
            private CSPID.Range<double> eRange;

            // Whether we've been setup
            private bool setup = false;

            // Moving average for error measurement
            private OABMath.MovingAverage errAvg = new OABMath.MovingAverage(5);

            // Iteration delay (ms)
            private int iterDelay = 500;

            // Loop cancellation token
            private CancellationToken ct;

            /// <summary>
            /// Create a new softpot tuning loop
            /// </summary>
            /// <param name="softpotType">softpot to adjust</param>
            /// <param name="measFunc">function which returns the measurement</param>
            /// <param name="measTarget">target measurement value</param>
            /// <param name="measTolerance">measurement +/- tolerance</param>
            /// <param name="measRange">Expected range of the measurement value</param>
            /// <param name="timeout">tuning routine timeout in seconds</param>
            public SoftpotTuningLoop(MotorolaXCMPRadioBase radio, MotorolaXCMPRadioBase.SoftpotType softpotType, Func<Task<float>> measFunc, double measTarget, double measTolerance, CSPID.Range<double> measRange, PIDGains gains, uint timeout, Action<string> logCallback, CancellationToken ct)
            {
                Radio = radio;
                SoftpotType = softpotType;
                MeasFunc = measFunc;
                MeasTarget = measTarget;
                MeasTolerance = measTolerance;
                timeOut = timeout * 1000;
                this.gains = gains;
                LogCallback = logCallback;
                this.ct = ct;

                // Calculate error range from provided measurement range
                double measAvg = (measRange.Maximum + measRange.Minimum) / 2;
                eRange = new CSPID.Range<double>(measRange.Minimum - measAvg, measRange.Maximum - measAvg);
            }

            /// <summary>
            /// Configure the tuning loop, reading the softpot length
            /// </summary>
            /// <returns></returns>
            public void Setup()
            {
                // Read the softpot current value and determine the softpot value byte length
                byte[] valueBytes = Radio.SoftpotGetValue(SoftpotType);
                spByteLength = (uint)valueBytes.Length;
                // Get the softpot min & max values
                softpotMin = spBytesToVal(Radio.SoftpotGetMinimum(SoftpotType));
                softpotMax = spBytesToVal(Radio.SoftpotGetMaximum(SoftpotType));
                // Calculate control range from softpot min/max
                double softpotAvg = (softpotMin + softpotMax) / 2;
                CSPID.Range<double> cRange = new CSPID.Range<double>(softpotMin - softpotAvg, softpotMax - softpotAvg);

                // Create the PID controller based on the min/max values
                pid = new CSPID.PIDController(
                    errorRange: eRange,
                    controlRange: cRange)
                {
                    ProportionalGain = gains.Kp,
                    IntegralGain = gains.Ki,
                    DerivativeGain = gains.Kd,
                };

                // Debug print
                Console.WriteLine($"Createing new tuning loop with error range ({eRange.Minimum},{eRange.Maximum}), control range ({cRange.Minimum},{cRange.Maximum}), kp {gains.Kp}, ki {gains.Ki}, kd {gains.Kd}");

                // Flag that we're all set up
                setup = true;
            }

            /// <summary>
            /// The tuning routine task
            /// </summary>
            /// <returns>True on success, false on failure</returns>
            public async Task<bool> Tune()
            {
                // Make sure we're setup
                if (!setup)
                {
                    throw new InvalidOperationException("Softpot tuning loop must be Setup() before Tune()");
                }

                // Read initial softpot value
                byte[] softpotBytes = Radio.SoftpotGetValue(SoftpotType);
                double softpotValue = spBytesToVal(softpotBytes);

                // Initial measurement & error
                double measurement = 0;
                double error = 0;

                // Iteration counter
                uint i = 0;

                // Routine timer
                Stopwatch stopwatch = Stopwatch.StartNew();

                while (stopwatch.Elapsed.TotalMilliseconds < timeOut)
                {
                    // Measure and calculate error
                    measurement = await MeasFunc();
                    error = measurement - MeasTarget;

                    // Add error to running average
                    errAvg.Add(Math.Abs(error));

                    // Exit loop if we achieved our target
                    if (errAvg.Value <= MeasTolerance)
                    {
                        // Debug print
                        Console.WriteLine($"Softpot tuning loop for {SoftpotName} hit threshold of <= {MeasTolerance:F3}, exiting tune loop!");
                        // Break out of loop
                        break;
                    }

                    // Control loop update (we invert the measurement error so the feedback works right)
                    double control = pid.Next(error, 1);

                    // Update softpot
                    softpotValue = softpotValue + control;
                    // Clamp
                    if (softpotValue > softpotMax) { softpotValue = softpotMax; }
                    else if (softpotValue < softpotMin) { softpotValue = softpotMin; }

                    LogCallback($"RefOsc tuning iter {i}: err {error:F3} (avg {errAvg.Value:F3}), new softpot setting {softpotValue:F0} ({(control >= 0?"+":"")}{control:F3})");

                    // Update softpot
                    softpotBytes = spValToBytes((int)Math.Round(softpotValue));
                    Radio.SoftpotUpdate(SoftpotType, softpotBytes);

                    i++;

                    await Task.Delay(iterDelay, ct);
                }

                // If we're within tolerance, write the new value
                if (Math.Abs(error) <= MeasTolerance)
                {
                    Radio.SoftpotWrite(SoftpotType, softpotBytes);
                    LogCallback($"Softpot tuning for {SoftpotName} success! Total error: {error:F3}, wrote new softpot value {softpotValue:F0}");
                    return true;
                }
                else
                {
                    LogCallback($"Softpot tuning for {SoftpotName} FAILED: Total error {error:F3} greater than max allowable {MeasTolerance:F0}");
                    return false;
                }
            }

            /// <summary>
            /// Convert an array of bytes to an integer value
            /// </summary>
            /// <param name="bytes">bytes to convert</param>
            /// <returns>converted integer value</returns>
            /// <exception cref="NotImplementedException">if byte size is not 8, 4, 2, or 1</exception>
            private int spBytesToVal(byte[] bytes)
            {
                // flip byte array since softpot bytes are little-endian
                bytes = bytes.Reverse().ToArray();
                // Convert
                switch (spByteLength)
                {
                    case 4:
                        return BitConverter.ToInt32(bytes);
                    case 2:
                        return BitConverter.ToInt16(bytes);
                    case 1:
                        return bytes[0];
                    default:
                        throw new NotImplementedException($"Value byte length of {spByteLength} not supported!");
                }
            }

            /// <summary>
            /// Convert an integer value to an array of bytes
            /// </summary>
            /// <param name="val">value to convert</param>
            /// <returns>byte array</returns>
            /// <exception cref="NotImplementedException"></exception>
            private byte[] spValToBytes(int val)
            {
                // Bytes holder
                byte[] bytes;
                // Convert
                switch (spByteLength)
                {
                    case 4:
                        bytes = BitConverter.GetBytes(val);
                        break;
                    case 2:
                        bytes = BitConverter.GetBytes((short)val);
                        break;
                    case 1:
                        bytes = new byte[] { (byte)val };
                        break;
                    default:
                        throw new NotImplementedException($"Value byte length of {spByteLength} not supported!");
                }
                // Swap for little-endian
                bytes = bytes.Reverse().ToArray();
                // Return
                return bytes;
            }
        }
    }
}
