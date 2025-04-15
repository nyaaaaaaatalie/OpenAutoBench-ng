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
                    connection = new SerialConnection(settings.InstrumentSerialPort, settings.InstrumentBaudrate);
                    break;
                case Settings.InstrumentConnectionTypeEnum.IP:
                    connection = new IPConnection(settings.InstrumentIPAddress, settings.InstrumentIPPort);
                    break;
                default:
                    throw new Exception("Unsupported connection type. Dying.");
            }

            switch (settings.InstrumentType)
            {
                case Settings.InstrumentTypeEnum.HP_8900:
                    if (!settings.IsGPIB)
                    {
                        throw new Exception("GPIB disabled and HP 8900 selected. This is an impossible combination.");
                    }
                     
                    instrument = new HP_8900Instrument(connection, settings.InstrumentGPIBAddress);
                    await instrument.Connect();
                    await Task.Delay(500);

                    try
                    {
                        string instInfo = await instrument.GetInfo();
                        if (!(instInfo.Length > 0))
                        {
                            throw new Exception("Get info succeeded but returned a zero length");
                        }
                    }
                    catch (Exception e)
                    {
                        await instrument.Disconnect();
                        throw new Exception("Connection to instrument failed: " + e.ToString());
                    }
                    break;


                case Settings.InstrumentTypeEnum.R2670:
                    if (settings.IsGPIB)
                    {
                        throw new Exception("GPIB enabled and R2670 selected. GPIB is not supported on this instrument.");
                    }
                    int serialPort = 0;
                    serialPort = int.Parse(Regex.Match(settings.InstrumentSerialPort, @"\d+").Value);
                    instrument = new GeneralDynamics_R2670Instrument(connection, serialPort);
                    await instrument.Connect();
                    await Task.Delay(500);

                    try
                    {
                        string instInfo = await instrument.GetInfo();
                        if (!(instInfo.Length > 0))
                        {
                            throw new Exception("Get info succeeded but returned a zero length");
                        }
                    }
                    catch (Exception e)
                    {
                        await instrument.Disconnect();
                        throw new Exception("Connection to instrument failed: " + e.ToString());
                    }
                    break;


                case Settings.InstrumentTypeEnum.IFR_2975:
                    if (settings.IsGPIB)
                    {
                        throw new Exception("GPIB enabled and IFR 2975 selected. GPIB is not supported on this instrument.");
                    }
                    instrument = new IFR_2975Instrument(connection);
                    await instrument.Connect();
                    await Task.Delay(500);

                    try
                    {
                        string instInfo = await instrument.GetInfo();
                        if (!(instInfo.Length > 0))
                        {
                            throw new Exception("Get info succeeded but returned a zero length");
                        }
                    }
                    catch (Exception e)
                    {
                        await instrument.Disconnect();
                        throw new Exception("Connection to instrument failed: " + e.ToString());
                    }
                    break;
                case Settings.InstrumentTypeEnum.Astronics_R8000:
                    if (settings.IsGPIB)
                    {
                        throw new Exception("GPIB enabled and Astronics R8000 selected. GPIB is not supported on this instrument.");
                    }

                    if (connection is SerialConnection)
                    {
                        throw new Exception("Serial selected and Astronics R8000 selected. Serial is not supported in this instrument.");
                    }

                    instrument = new Astronics_R8000Instrument(connection);
                    await instrument.Connect();
                    await Task.Delay(500);

                    try
                    {
                        string instInfo = await instrument.GetInfo();
                        if (!(instInfo.Length > 0))
                        {
                            throw new Exception("Get info succeeded but returned a zero length");
                        }
                    }
                    catch (Exception e)
                    {
                        await instrument.Disconnect();
                        throw new Exception("Connection to instrument failed: " + e.ToString());
                    }
                    break;
                case Settings.InstrumentTypeEnum.Viavi_8800SX:
                    if (settings.IsGPIB)
                    {
                        throw new Exception("GPIB enabled and Viavi 8800SX selected. GPIB is not supported on this instrument.");
                    }

                    if (connection is SerialConnection)
                    {
                        throw new Exception("Serial selected and Viavi 8800SX selected. Serial is not supported in this instrument.");
                    }

                    instrument = new Viavi_8800SXInstrument(connection);
                    await instrument.Connect();
                    await Task.Delay(500);

                    try
                    {
                        string instInfo = await instrument.GetInfo();
                        if (!(instInfo.Length > 0))
                        {
                            throw new Exception("Get info succeeded but returned a zero length");
                        }
                    }
                    catch (Exception e)
                    {
                        await instrument.Disconnect();
                        throw new Exception("Connection to instrument failed: " + e.ToString());
                    }
                    break;
                default:
                    // this shouldn't happen!
                    throw new Exception("Unsupported instrument somehow selected. Dying.");
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
