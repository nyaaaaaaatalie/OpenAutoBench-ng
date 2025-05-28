using Microsoft.Extensions.ObjectPool;
using OpenAutoBench_ng.Communication.Instrument;
using OpenAutoBench_ng.OpenAutoBench;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.RSSRepeaterBase
{
    public class MotorolaRSSRepeaterBaseTestParams
    {
        public bool doRefoscTest = true;
        public bool doPowerTest = true;
        public bool doDeviationTest = true;
        public bool doRssiTest = true;

        public IBaseInstrument? instrument;
        public Action<string>? callback;
        public MotorolaRSSRepeaterBase? radio;

        public TestReport report;

        public CancellationToken ct;

        public MotorolaRSSRepeaterBaseTestParams()
        {
        }


    }
}
