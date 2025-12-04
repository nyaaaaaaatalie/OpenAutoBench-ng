using System.Security.Principal;

namespace OpenAutoBench_ng.OpenAutoBench
{
    public class OABMath
    {
        public class MovingAverage
        {
            public double Value { 
                get
                {
                    if (samples.Count > 0)
                    {
                        return accumulator / samples.Count;
                    }
                    else
                    {
                        return 0;
                    }
                } }

            public int WindowSize { get; private set; }

            private Queue<double> samples = new Queue<double>();
            private double accumulator;

            public MovingAverage(int windowSize)
            {
                this.WindowSize = windowSize;
            }

            public void Add(double sample)
            {
                accumulator += sample;
                samples.Enqueue(sample);
                if (samples.Count > WindowSize)
                {
                    accumulator -= samples.Dequeue();
                }
            }
        }

        public class Variance
        {
            public double Value
            {
                get
                {
                    // Calculate average for all samples
                    double mean = samples.Sum() / samples.Count;
                    // Sum of all the squared differences
                    double diffSum = 0;
                    foreach(double sample in samples.ToArray())
                    {
                        // Calculate difference and square it, add to sum
                        diffSum += Math.Pow((sample - mean), 2);
                    }
                    // Return the sum divided by the number of samples - 1
                    return diffSum / (WindowSize - 1);
                }
            }

            public int WindowSize { get; private set; }

            private Queue<double> samples = new Queue<double>();

            public Variance(int windowSize)
            {
                this.WindowSize = windowSize;
            }

            public void Add(double sample)
            {
                samples.Enqueue(sample);
                if (samples.Count > WindowSize)
                    samples.Dequeue();
            }
        }
    }
}
