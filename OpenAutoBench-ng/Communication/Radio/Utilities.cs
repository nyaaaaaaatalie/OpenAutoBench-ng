using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace OpenAutoBench_ng.Communication.Radio
{
    public class Utilities
    {
        public class RSSILookupTable
        {
            private SortedDictionary<byte, float> table;
            /// <summary>
            /// Create a new RSSI lookup table
            /// </summary>
            /// <param name="lookupTable">sorted dictionary of (RSSI, dBm) pairs</param>
            public RSSILookupTable(SortedDictionary<byte, float> lookupTable)
            {
                table = lookupTable;
            }

            /// <summary>
            /// Get DBM value for an RSSI based on the lookup table
            /// </summary>
            /// <param name="rssi"></param>
            /// <returns></returns>
            public float GetDBM(byte rssi)
            {
                // Find the nearest RSSI key
                int index = new List<byte>(table.Keys).BinarySearch(rssi);
                // 0 or positive is an extant entry in the table, so just return
                if (index >= 0)
                {
                    return table[rssi];
                }
                // If we got a negative number, it's the bitwise complement of the theoretical index (see the BinarySearch docs for more info)
                index = ~index;
                // We're going to use two existing points to linearly interpolate
                KeyValuePair<byte, float> p0;
                KeyValuePair<byte, float> p1;
                // If the index equals the length of the dict, our RSSI value is above the top
                if (index >= table.Count)
                {
                    // Get the two highest points in the table
                    p0 = table.ElementAt(table.Count - 1);
                    p1 = table.ElementAt(table.Count);
                }
                // If the index is 0, our point is below the bottom
                else if (index == 0)
                {
                    // Get the two lowest points in the table
                    p0 = table.ElementAt(0);
                    p1 = table.ElementAt(1);
                }
                // Finally, do a normal linear interoplation if our number ends up in the middle
                else
                {
                    p0 = table.ElementAt(index - 1);
                    p1 = table.ElementAt(index);
                }
                // Calculate slope
                float m = (float)((p1.Value - p0.Value) / (float)(p1.Key - p0.Key));
                // Calculate intercept
                float b = p1.Value - (m * (float)p1.Key);
                // Once we've gotten our slope and intercept, we can calculate the correct dBm value
                return (m * (float)rssi) + b;
            }
        }
    }
}
