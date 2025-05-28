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
    }
}
