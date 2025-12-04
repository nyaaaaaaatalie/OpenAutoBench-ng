using OpenAutoBench_ng.Communication.Instrument;
using System.Net.NetworkInformation;
using OpenAutoBench_ng.Communication.Instrument.Connection;
using OpenAutoBench_ng.Communication.Instrument.HP_8900;
using OpenAutoBench_ng.Communication.Instrument.IFR_2975;
using System;
using System.Text.RegularExpressions;
using System.IO.Ports;
using System.ComponentModel;
using PdfSharpCore.Pdf;
using PdfSharpCore;
using PdfSharpCore.Pdf.IO;
using PdfSharpCore.Drawing;
using PdfSharpCore.Drawing.Layout;
using OpenAutoBench_ng.Communication.Instrument.Astronics_R8000;
using OpenAutoBench_ng.Communication.Instrument.Viavi_8800SX;
using OpenAutoBench_ng.Communication.Instrument.GeneralDynamics_R2670;

namespace OpenAutoBench_ng.OpenAutoBench
{
    public class MainLogic
    {

        public MainLogic()
        {
            //
        }
        public static string[] GetSerialPorts()
        {
            return SerialPort.GetPortNames();
        }

        public static void OnStart()
        {
            //preferences = new Preferences();
            //preferences.Load();
            //Console.WriteLine("Loaded settings");
            //Console.WriteLine("Serial port for instrument: {0}", preferences.settings.InstrumentSerialPort);
        }

        public static async Task<IBaseInstrument> CreateInstrument()
        {
            Preferences prefs = new Preferences();
            Settings settings = prefs.Load();
            IBaseInstrument instrument = null;
            IInstrumentConnection connection = null;

            switch (settings.InstrumentConnectionType)
            {
                case Settings.InstrumentConnectionTypeEnum.Serial:
                    connection = new SerialConnection(settings.SerialPort, settings.SerialBaudrate, settings.SerialNewline, settings.SerialDTR);
                    Console.WriteLine($"Creating new serial instrument connection to {settings.SerialPort} at {settings.SerialBaudrate} baud");
                    break;
                case Settings.InstrumentConnectionTypeEnum.IP:
                    if (string.IsNullOrEmpty(settings.IPAddress))
                        throw new Exception("IP address cannot be blank!");
                    if (settings.IPPort <= 0)
                        throw new Exception("IP port cannot be <= 0");
                    connection = new IPConnection(settings.IPAddress, settings.IPPort);
                    Console.WriteLine($"Creating new IP instrument connection to {settings.IPAddress}:{settings.IPPort}");
                    break;
                case Settings.InstrumentConnectionTypeEnum.VISA:
                    if (string.IsNullOrEmpty(settings.VISAResourceName))
                        throw new Exception("VISA resource name cannot be blank!");
                    connection = new VISAConnection(settings.VISAResourceName);
                    Console.WriteLine($"Creating new VISA instrument connection to {settings.VISAResourceName}");
                    break;
                default:
                    throw new Exception("Unsupported connection type. Dying.");
            }

            switch (settings.InstrumentType)
            {
                // HP 8920, 8921, and 8935 use GPIB via serial or VISA
                case Settings.InstrumentTypeEnum.HP_8900:
                    if (settings.InstrumentConnectionType == Settings.InstrumentConnectionTypeEnum.Serial)
                    {
                        // Serial requires GPIB to be enabled
                        if (!settings.SerialIsGPIB) { throw new Exception("HP 89xx control via serial requires GPIB to be enabled"); }
                        // Create new serial GPIB instrument
                        instrument = new HP_8900Instrument(connection, settings.SerialGPIBAddress);
                    }
                    else if (settings.InstrumentConnectionType == Settings.InstrumentConnectionTypeEnum.VISA)
                    {
                        // Create new VISA instrument
                        instrument = new HP_8900Instrument(connection);
                    }
                    else
                    {
                        throw new Exception("HP89xx requires serial or VISA control");
                    }
                    break;

                // Motorola R2670 supports serial control only
                case Settings.InstrumentTypeEnum.R2670:
                    // Validation
                    if (settings.InstrumentConnectionType != Settings.InstrumentConnectionTypeEnum.Serial)
                        throw new Exception("Motorola R2670 requires serial control");
                    if (settings.SerialIsGPIB)
                        throw new Exception("GPIB enabled and Motorola R2670 selected. GPIB is not supported on this instrument.");
                    
                    instrument = new GeneralDynamics_R2670Instrument(connection);
                    break;

                // IFR 2975 is serial control only
                case Settings.InstrumentTypeEnum.IFR_2975:
                    if (settings.InstrumentConnectionType != Settings.InstrumentConnectionTypeEnum.Serial)
                        throw new Exception("IFR R2975 requires serial control");
                    if (settings.SerialIsGPIB)
                        throw new Exception("GPIB enabled and IFR R2975 selected. GPIB is not supported on this instrument.");

                    instrument = new IFR_2975Instrument(connection);
                    break;
                
                // Astronics R8000 is IP only
                case Settings.InstrumentTypeEnum.Astronics_R8000:
                    if (settings.InstrumentConnectionType != Settings.InstrumentConnectionTypeEnum.IP)
                        throw new Exception("Astronics R8000 requires IP control");

                    instrument = new Astronics_R8000Instrument(connection);
                    break;

                // Viavi 880SX is IP only
                case Settings.InstrumentTypeEnum.Viavi_8800SX:
                    if (settings.InstrumentConnectionType != Settings.InstrumentConnectionTypeEnum.IP)
                        throw new Exception("Viavi 8800SX requires IP control");

                    instrument = new Viavi_8800SXInstrument(connection);
                    break;

                default:
                    // this shouldn't happen!
                    throw new Exception("Unsupported instrument somehow selected. Dying.");
            }

            // Connect to new instrument and get info
            try
            {
                await instrument.Connect();
                await Task.Delay(500);
                await instrument.GetInfo();
            }
            catch (Exception e)
            {
                await instrument.Disconnect();
                throw new Exception("Connection to instrument failed: " + e.ToString());
            }

            return instrument;
        }

        public static MemoryStream GeneratePdf(string text)
        {
            using (PdfDocument pdfDocument = new PdfDocument())
            {
                pdfDocument.Info.Title = "OpenAutoBench-ng Radio Bench Test/Calibration Report";

                XFont font = new XFont("Times New Roman", 12, XFontStyle.Regular);
                int margin = 50; // Margin around the text
                var pageSize = PageSize.Letter; // Set the page size to US Letter

                // Calculate the line height based on the font
                double lineHeight = font.GetHeight();

                // Split the text into lines
                var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                int currentLine = 0;

                while (currentLine < lines.Length)
                {
                    PdfPage page = pdfDocument.AddPage();
                    page.Size = pageSize;
                    XGraphics gfx = XGraphics.FromPdfPage(page);
                    XTextFormatter tf = new XTextFormatter(gfx);

                    // Create a rectangle for text layout
                    XRect rect = new XRect(margin, margin, page.Width - 2 * margin, lineHeight);

                    // Draw text line by line
                    while (currentLine < lines.Length)
                    {
                        tf.DrawString(lines[currentLine], font, XBrushes.Black, rect, XStringFormats.TopLeft);
                        rect.Y += lineHeight; // Move to the next line
                        currentLine++;

                        // Check if we need a new page
                        if (rect.Y + lineHeight > page.Height - margin)
                        {
                            break; // Exit the loop to create a new page
                        }
                    }
                }

                // Save the document to a MemoryStream
                using (MemoryStream stream = new MemoryStream())
                {
                    pdfDocument.Save(stream, false);
                    return stream;
                }
            }
        }
    }
}
