using OpenAutoBench_ng.Communication.Radio.Motorola.Quantar;

namespace OpenAutoBench_ng.Communication.Radio
{
    // TODO: Use this instead of the single tx list
    public class RadioFrequencies
    {
        public List<int> TxFrequencies { get; set; }
        public List<int> RxFrequencies { get; set; }

        public RadioFrequencies()
        {
            TxFrequencies = new List<int>();
            RxFrequencies = new List<int>();
        }
    }

    public interface IBaseRadio
    {
        public string Name { get; }

        public string ModelName { get; }

        public string ModelNumber { get; }

        public string SerialNumber { get; }

        public string FirmwareVersion { get; }

        public string InfoHeader { get; }
    }
}
