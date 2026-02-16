using OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase;

namespace OpenAutoBench_ng.Communication.Radio.Motorola.APX
{
    public class MotorolaAPX_RefData
    {
        public static string ModelName(MotorolaXCMPRadioBase radio)
        {
            /*
             * Get Radio Model Name
             */
            string RadioModel = "Unknown";

            // APX900
            if (radio.ModelNumber.Contains("H92"))
            {
                RadioModel = "APX900";
            }
            // APX1000
            else if (radio.ModelNumber.Contains("H84"))
            {
                RadioModel = "APX1000";
            }
            // APX2000
            else if (radio.ModelNumber.Contains("H52"))
            {
                RadioModel = "APX2000";
            }
            // APX3000
            else if (radio.ModelNumber.Contains("H59"))
            {
                RadioModel = "APX3000";
            }
            // APX4000
            else if (radio.ModelNumber.Contains("H51"))
            {
                if (radio.ModelNumber.Contains("H51V"))
                    RadioModel = "APX4000XH";
                else
                    RadioModel = "APX4000";
            }
            // APX6000
            else if (radio.ModelNumber.Contains("H98"))
            {
                RadioModel = "APX6000";
            }
            // APX7000
            else if (radio.ModelNumber.Contains("H49") ||
                     radio.ModelNumber.Contains("H97"))
            {
                RadioModel = "APX7000";
            }
            // APX8000
            else if (radio.ModelNumber.Contains("H98") ||
                     radio.ModelNumber.Contains("H91"))
            {
                RadioModel = "APX8000";
            }
            // APX1500
            else if (radio.ModelNumber.Contains("M36"))
            {
                RadioModel = "APX1500";
            }
            // APX4500
            else if (radio.ModelNumber.Contains("M22"))
            {
                RadioModel = "APX4500";
            }
            // APX6500
            else if (radio.ModelNumber.Contains("M25"))
            {
                RadioModel = "APX6500";
            }
            // APX7000
            else if (radio.ModelNumber.Contains("M30"))
            {
                RadioModel = "APX7500";
            }
            // APX8500
            else if (radio.ModelNumber.Contains("M37"))
            {
                RadioModel = "APX8500";
            }

            return RadioModel;
        }

        // Initialize data based on radio type
        public static int[] TxFrequencies(MotorolaXCMPRadioBase radio)
        {

            List<int> TxFrequencies = new List<int>();
            /*
             * Portable Frequency Lists
             */

            //900 APX4000
            if (radio.ModelNumber.Contains("H51W") ||
                // 900 APX1000
                radio.ModelNumber.Contains("H84W"))
            {
                TxFrequencies.Add(896012500);
                TxFrequencies.Add(899012500);
                TxFrequencies.Add(901987500);
                TxFrequencies.Add(935012500);
                TxFrequencies.Add(938012500);
                TxFrequencies.Add(940937500);
            }

            // 8/900 APX4000XH
            else if (radio.ModelNumber.Contains("H51V"))
            {
                TxFrequencies.Add(806012500);
                TxFrequencies.Add(820987500);
                TxFrequencies.Add(824987500);
                TxFrequencies.Add(851012500);
                TxFrequencies.Add(865987500);
                TxFrequencies.Add(869987500);
                TxFrequencies.Add(896012500);
                TxFrequencies.Add(901987500);
                TxFrequencies.Add(935012500);
                TxFrequencies.Add(940987500);
            }

            // 7/800 APX900
            else if (radio.ModelNumber.Contains("H92U") ||
                // 7/8000 APX1000
                radio.ModelNumber.Contains("H84U") ||
                // 7/800 APX2000
                radio.ModelNumber.Contains("H52U") ||
                // 7/800 APX3000
                radio.ModelNumber.Contains("H59U") ||
                // 7/800 APX4000
                radio.ModelNumber.Contains("H51U") ||
                // 7/800 APX 5000 & APX6000
                radio.ModelNumber.Contains("H98U"))
            {
                TxFrequencies.Add(764012500);
                TxFrequencies.Add(769012500);
                TxFrequencies.Add(775987500);
                TxFrequencies.Add(794012500);
                TxFrequencies.Add(809012500);
                TxFrequencies.Add(823987500);
                TxFrequencies.Add(851012500);
                TxFrequencies.Add(860012500);
                TxFrequencies.Add(869887500);
            }

            //UHF2 APX 900
            else if (radio.ModelNumber.Contains("H92S"))
            {
                TxFrequencies.Add(450025000);
                TxFrequencies.Add(460025000);
                TxFrequencies.Add(470000000);
                TxFrequencies.Add(470005000);
                TxFrequencies.Add(485025000);
                TxFrequencies.Add(495025000);
                TxFrequencies.Add(500025000);
                TxFrequencies.Add(510025000);
                TxFrequencies.Add(519975000);
            }

            //UHF2 APX1000
            else if (radio.ModelNumber.Contains("H84S") ||
                // UHF2 APX2000
                radio.ModelNumber.Contains("H52S") ||
                //UHF2 APX3000
                radio.ModelNumber.Contains("H59S") ||
                //UHF2 APX4000
                radio.ModelNumber.Contains("H51S") ||
                // UHF2 APX5000 & APX6000
                radio.ModelNumber.Contains("H98S"))
            {
                TxFrequencies.Add(450025000);
                TxFrequencies.Add(460025000);
                TxFrequencies.Add(471025000);
                TxFrequencies.Add(484975000);
                TxFrequencies.Add(485025000);
                TxFrequencies.Add(495025000);
                TxFrequencies.Add(506025000);
                TxFrequencies.Add(519750000);
            }

            //UHF1 APX900
            else if (radio.ModelNumber.Contains("H92Q"))
            {
                TxFrequencies.Add(380025000);
                TxFrequencies.Add(392525000);
                TxFrequencies.Add(405025000);
                TxFrequencies.Add(417525000);
                TxFrequencies.Add(429925000);
                TxFrequencies.Add(430025000);
                TxFrequencies.Add(442525000);
                TxFrequencies.Add(455025000);
                TxFrequencies.Add(467525000);
                TxFrequencies.Add(479925000);
            }

            // UHF1 APX1000
            else if (radio.ModelNumber.Contains("H84Q") ||
                //UHF1 APX2000
                radio.ModelNumber.Contains("H52Q") ||
                //UHF1 APX3000
                radio.ModelNumber.Contains("H59Q") ||
                //UHF1 APX4000
                radio.ModelNumber.Contains("H51Q"))
            {
                TxFrequencies.Add(380025000);
                TxFrequencies.Add(390025000);
                TxFrequencies.Add(400025000);
                TxFrequencies.Add(411025000);
                TxFrequencies.Add(424925000);
                TxFrequencies.Add(435025000);
                TxFrequencies.Add(444975000);
                TxFrequencies.Add(445025000);
                TxFrequencies.Add(457025000);
                TxFrequencies.Add(469925000);
            }

            //UHF1 APX5000 & APX6000
            else if (radio.ModelNumber.Contains("H98Q"))
            {
                TxFrequencies.Add(380025000);
                TxFrequencies.Add(395025000);
                TxFrequencies.Add(411025000);
                TxFrequencies.Add(424925000);
                TxFrequencies.Add(425025000);
                TxFrequencies.Add(440025000);
                TxFrequencies.Add(455325000);
                TxFrequencies.Add(469925000);
                TxFrequencies.Add(479925000);
            }

            //VHF APX900
            else if (radio.ModelNumber.Contains("H92K") ||
                //VHF APX1000
                radio.ModelNumber.Contains("H84K") ||
                //VHF APX2000
                radio.ModelNumber.Contains("H52K") ||
                //VHF APX3000
                radio.ModelNumber.Contains("H59K") ||
                //VHF APX4000
                radio.ModelNumber.Contains("H51K") ||
                //VHF APX5000 & APX6000
                radio.ModelNumber.Contains("H98K"))
            {
                TxFrequencies.Add(136025000);
                TxFrequencies.Add(142125000);
                TxFrequencies.Add(154225000);
                TxFrequencies.Add(160125000);
                TxFrequencies.Add(168075000);
                TxFrequencies.Add(173975000);
            }

            /*
             * Multiband Portable Frequencies
             */

            // APX7000
            else if (radio.ModelNumber.Contains("H49T") ||
                     radio.ModelNumber.Contains("H97T"))
            {
                MotorolaBand[] bands = radio.GetBands();

                if (bands.Contains(MotorolaBand.BAND_7800))
                {
                    TxFrequencies.Add(764012500);
                    TxFrequencies.Add(769012500);
                    TxFrequencies.Add(775987500);
                    TxFrequencies.Add(794012500);
                    TxFrequencies.Add(809012500);
                    TxFrequencies.Add(823987500);
                    TxFrequencies.Add(851012500);
                    TxFrequencies.Add(860012500);
                    TxFrequencies.Add(869887500);
                }

                if (bands.Contains(MotorolaBand.BAND_UHF2))
                {
                    TxFrequencies.Add(450025000);
                    TxFrequencies.Add(460025000);
                    TxFrequencies.Add(471025000);
                    TxFrequencies.Add(484975000);
                    TxFrequencies.Add(485025000);
                    TxFrequencies.Add(495025000);
                    TxFrequencies.Add(506025000);
                    TxFrequencies.Add(511975000);
                }

                if (bands.Contains(MotorolaBand.BAND_UHF1))
                {
                    TxFrequencies.Add(380025000);
                    TxFrequencies.Add(390025000);
                    TxFrequencies.Add(400025000);
                    TxFrequencies.Add(411025000);
                    TxFrequencies.Add(424925000);
                    TxFrequencies.Add(435025000);
                    TxFrequencies.Add(444975000);
                    TxFrequencies.Add(445025000);
                    TxFrequencies.Add(457025000);
                    TxFrequencies.Add(469925000);

                }

                if (bands.Contains(MotorolaBand.BAND_VHF))
                {
                    TxFrequencies.Add(136025000);
                    TxFrequencies.Add(142125000);
                    TxFrequencies.Add(154225000);
                    TxFrequencies.Add(160125000);
                    TxFrequencies.Add(168075000);
                    TxFrequencies.Add(173975000);
                }
            }

            // APX8000
            else if (radio.ModelNumber.Contains("H91U") ||
                     radio.ModelNumber.Contains("H91T"))
            {
                // VHF
                TxFrequencies.Add(136012500);
                TxFrequencies.Add(155012500);
                TxFrequencies.Add(173987500);

                // UHF
                TxFrequencies.Add(380012500);
                TxFrequencies.Add(425012500);
                TxFrequencies.Add(469987500);
                TxFrequencies.Add(470012500);
                TxFrequencies.Add(495012500);
                TxFrequencies.Add(519987500);

                // 7/800
                TxFrequencies.Add(764012500);
                TxFrequencies.Add(785012500);
                TxFrequencies.Add(805987500);
                TxFrequencies.Add(806012500);
                TxFrequencies.Add(838012500);
                //TxFrequencies.Add(851012500);
                TxFrequencies.Add(869887500);
            }

            /*
             * MOBILES
             */


            // UHF2 APX4500
            else if (radio.ModelNumber.Contains("M22S"))
            {
                TxFrequencies.Add(450012500);
                TxFrequencies.Add(455012500);
                TxFrequencies.Add(465012500);
                TxFrequencies.Add(470012500);
                TxFrequencies.Add(484987500);
                TxFrequencies.Add(485012500);
                TxFrequencies.Add(498012500);
                TxFrequencies.Add(511987500);
                TxFrequencies.Add(512012500);
                TxFrequencies.Add(519987500);
            }

            // VHF 6500
            else if (radio.ModelNumber.Contains("M25K"))
            {
                TxFrequencies.Add(136012500);
                TxFrequencies.Add(140762500);
                TxFrequencies.Add(145512500);
                TxFrequencies.Add(150262500);
                TxFrequencies.Add(154987500);
                TxFrequencies.Add(155012500);
                TxFrequencies.Add(159762500);
                TxFrequencies.Add(164512500);
                TxFrequencies.Add(169262500);
                TxFrequencies.Add(173987500);
            }

            // all else, error out
            else
            {
                throw new Exception($"I don't know frequencies for your model {radio.ModelNumber}");
            }

            return TxFrequencies.ToArray();
        }

        public static bool isFreon(MotorolaXCMPRadioBase radio)
        {
            if (
                radio.ModelNumber.EndsWith("BN") ||
                radio.ModelNumber.EndsWith("CN") ||
                //APX900
                radio.ModelNumber.StartsWith("H92") ||
                //APX8000
                radio.ModelNumber.StartsWith("H91") ||
                //APX8500
                radio.ModelNumber.StartsWith("H91"))
            {
                return true;
            }
            return false;
        }
    }
}
