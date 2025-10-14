using System.Security.Cryptography;
using OpenAutoBench_ng.Communication.Instrument;
using OpenAutoBench_ng.OpenAutoBench;

namespace OpenAutoBench_ng.Communication.Radio
{
    public abstract class IBaseTest
    {
        public string Name { get; }

        public bool Passed { get; }
        public bool Completed { get; }

        public IBaseInstrument Instrument { get; }

        public Action<string> LogCallback { get; }

        public CancellationToken Ct { get; }

        public TestReport Report { get; }

        public IBaseTest(string name, TestReport report, IBaseInstrument instrument, Action<string> logCallback, CancellationToken ct)
        {
            Name = name;
            Report = report;
            Instrument = instrument;
            LogCallback = logCallback;
            Ct = ct;
        }

        public abstract bool IsRadioEligible();

        public abstract Task Setup();

        public abstract Task PerformTest();

        public virtual Task PerformAlignment()
        {
            throw new NotImplementedException();
        }

        public abstract Task Teardown();

        
    }
}
