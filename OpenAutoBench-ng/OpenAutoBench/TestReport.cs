using OpenAutoBench_ng.Communication.Instrument;
using OpenAutoBench_ng.Communication.Radio;
using PdfSharpCore.Pdf;
using PdfSharpCore;
using PdfSharpCore.Pdf.IO;
using PdfSharpCore.Drawing;
using PdfSharpCore.Drawing.Layout;
using System.Reflection;
using OpenAutoBench_ng.Pages.Tests;
using OpenAutoBench_ng.Communication.Radio.Motorola.APX;
using PdfSharpCore.Fonts;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using OpenAutoBench_ng.OpenAutoBench.Fonts;

namespace OpenAutoBench_ng.OpenAutoBench
{
    public enum ReportType
    {
        TEST,
        ALIGNMENT
    }

    public enum ResultType
    {
        REF_OSC,
        FREQ_ERROR,
        TX_POWER,
        TX_DEVIATION,
        TX_DEVIATION_BAL,
        BIT_ERROR_RATE,
        RSSI
    }

    public enum Result
    {
        PASS,
        FAIL,
        ERROR
    }

    public class TestError
    {
        /// <summary>
        /// Type of the test that was going to be done
        /// </summary>
        public ResultType Type { get; set; }
        /// <summary>
        /// Error message
        /// </summary>
        public string ErrorMessage { get; set; }
        /// <summary>
        /// Create a new test error message for the test report
        /// </summary>
        /// <param name="type"></param>
        /// <param name="errorMessage"></param>
        public TestError(ResultType type, string errorMessage)
        {
            Type = type;
            ErrorMessage = errorMessage;
        }
    }

    public class TestResult
    {
        /// <summary>
        /// Type of test result
        /// </summary>
        public ResultType Type { get; }
        /// <summary>
        /// Target measurement value
        /// </summary>
        public double TargetValue { get; }
        /// <summary>
        /// Lower limit of acceptable measurement
        /// </summary>
        public double LowerLimit { get; }
        /// <summary>
        /// Upper limit of acceptable measurement
        /// </summary>
        public double UpperLimit { get; }
        /// <summary>
        /// Measured value
        /// </summary>
        public double MeasuredValue { get; private set; }
        /// <summary>
        /// Time the result was measured
        /// </summary>
        public DateTime MeasurementTime { get; private set; }
        /// <summary>
        /// Frequency the result was measured at
        /// </summary>
        public int MeasurementFrequency { get; }
        /// <summary>
        /// Whether the result is passing or not
        /// </summary>
        public Result Result { get
            {
                return testResult();
            } }

        /// <summary>
        /// Whether a value was measured or not
        /// </summary>
        private bool measured = false;

        /// <summary>
        /// Create a new test result
        /// </summary>
        /// <param name="type"></param>
        /// <param name="targetValue"></param>
        /// <param name="lowerLimit"></param>
        /// <param name="upperLimit"></param>
        /// <param name="measuredValue"></param>
        public TestResult(ResultType type, double targetValue, double lowerLimit, double upperLimit, int frequency = -1)
        {
            Type = type;
            TargetValue = targetValue;
            LowerLimit = lowerLimit;
            UpperLimit = upperLimit;
            MeasurementFrequency = frequency;
        }
        
        /// <summary>
        /// Create a new test result with a measurement value
        /// </summary>
        /// <param name="type"></param>
        /// <param name="measurement"></param>
        /// <param name="targetValue"></param>
        /// <param name="lowerLimit"></param>
        /// <param name="upperLimit"></param>
        /// <param name="frequency"></param>
        public TestResult(ResultType type, double measurement, double targetValue, double lowerLimit, double upperLimit, int frequency = -1)
        {
            Type = type;
            TargetValue = targetValue;
            LowerLimit = lowerLimit;
            UpperLimit = upperLimit;
            MeasurementFrequency = frequency;
            Measure(measurement);
        }

        /// <summary>
        /// Make the test result measurement, recording the time of measurement
        /// </summary>
        /// <param name="measurement"></param>
        public void Measure(double measurement)
        {
            MeasuredValue = measurement;
            MeasurementTime = DateTime.Now;
            measured = true;
        }

        /// <summary>
        /// Generate an array of strings for inserting into a results table
        /// </summary>
        /// <returns>a 6-string array in format: [ Measurement Type, Measurement, Target Value, Limits, Frequency, Pass/Fail ]</returns>
        public string[] ReportLine()
        {
            switch (Type)
            {
                case ResultType.REF_OSC:
                    return new string[] {
                        "Reference Oscillator", $"{MeasuredValue:F2} Hz", $"{TargetValue:F2} Hz",
                        $"{toleranceString()} Hz", $"{MeasurementFrequency / 1E6} MHz", $"{Enum.GetName(typeof(Result), Result)}" };
                case ResultType.FREQ_ERROR:
                    return new string[] {
                        "Frequency Error", $"{MeasuredValue:F2} Hz", $"{TargetValue:F2} Hz",
                        $"{toleranceString()} Hz", $"{MeasurementFrequency / 1E6} MHz", $"{Enum.GetName(typeof(Result), Result)}" };
                case ResultType.TX_POWER:
                    return new string[] {
                        "Transmit Power", $"{MeasuredValue:F2} W", TargetValue > 0 ? $"{TargetValue:F2} W" : "N/A",
                        $"{toleranceString()} W", $"{MeasurementFrequency / 1E6} MHz", $"{Enum.GetName(typeof(Result), Result)}" };
                case ResultType.TX_DEVIATION:
                    return new string[] {
                        "Transmit Deviation", $"{MeasuredValue:F3} Hz", $"{TargetValue:F3} Hz",
                        $"{toleranceString()} Hz", $"{MeasurementFrequency / 1E6} MHz", $"{Enum.GetName(typeof(Result), Result)}" };
                case ResultType.TX_DEVIATION_BAL:
                    return new string[] {
                        "Deviation Balance", $"{MeasuredValue:F2}%", $"{TargetValue:F2}%",
                        $"{toleranceString()} %", $"{MeasurementFrequency / 1E6} MHz", $"{Enum.GetName(typeof(Result), Result)}" };
                case ResultType.BIT_ERROR_RATE:
                    return new string[] {
                        "Bit Error Rate (BER)", $"{MeasuredValue:F2}%", $"{TargetValue:F2}%",
                        $"{toleranceString()} %", $"{MeasurementFrequency / 1E6} MHz", $"{Enum.GetName(typeof(Result), Result)}" };
                case ResultType.RSSI:
                    return new string[] {
                        "Received Signal Strength", $"{MeasuredValue}", $"{TargetValue}",
                        $"{toleranceString()}", $"{MeasurementFrequency / 1E6} MHz", $"{Enum.GetName(typeof(Result), Result)}" };
                default:
                    return new string[6];
            }
        }

        /// <summary>
        /// Returns whether the test passed, i.e. whether the measured value was within the range
        /// </summary>
        /// <returns></returns>
        private Result testResult()
        {
            // We haven't passed if we haven't tested yet
            if (!measured)
            {
                return Result.FAIL;
            }
            if (LowerLimit <= MeasuredValue && MeasuredValue <= UpperLimit)
            {
                return Result.PASS;
            }
            else
            {
                return Result.FAIL;
            }
        }

        private string toleranceString()
        {
            if ((TargetValue - LowerLimit) == (UpperLimit - TargetValue))
            {
                // Show +/- same value
                return $"±{(UpperLimit - TargetValue)}";
            }
            else
            {
                // Show +/+ if both limits are above target
                if (TargetValue - LowerLimit < 0)
                {
                    return $"+{(UpperLimit - TargetValue)}/+{(TargetValue - LowerLimit)}";
                }
                // Show +/- different values
                return $"+{(UpperLimit - TargetValue)}/-{(TargetValue - LowerLimit)}";
            }
        }
    }

    public class TestReport
    {
        // Report type
        public ReportType Type { get; private set; }

        // Report start & end time
        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }

        public TimeSpan Duration { 
            get 
            {
                return EndTime.Subtract(StartTime);
            } 
        }
        
        // Radio Information
        public IBaseRadio Radio { get; private set; }

        // Test Equipment Information
        public IBaseInstrument Instrument { get; private set; }

        // Is the test report currently open
        public bool IsOpen { get; private set; }
        // Is the test report done
        public bool IsFinished { get; private set; }

        // List of test results
        public List<TestResult> Results { get; private set; }

        // List of test errors
        public List<TestError> Errors { get; private set; }

        // Report Comments
        public string Comments { get; set; }

        public TestReport(ReportType type)
        {
            Type = type;
            // Default values
            IsOpen = false;
            IsFinished = false;
            Results = new List<TestResult>();
            Errors = new List<TestError>();
            Type = type;
        }

        /// <summary>
        /// Whether all tests in this report passed
        /// </summary>
        public bool Passed { get
            {
                return testsPassed();
            } }

        /// <summary>
        /// Record the start of a test and open the test report
        /// </summary>
        /// <returns></returns>
        public DateTime Start(IBaseInstrument instrument, IBaseRadio radio)
        {
            Instrument = instrument;
            Radio = radio;
            StartTime = DateTime.Now;
            IsOpen = true;
            return StartTime;
        }

        /// <summary>
        /// Record the end of a test and close the report
        /// </summary>
        /// <returns></returns>
        public DateTime End()
        {
            EndTime = DateTime.Now;
            IsOpen = false;
            IsFinished = true;
            return EndTime;
        }

        public void AddResult(TestResult result)
        {
            Results.Add(result);
        }

        /// <summary>
        /// Add a new test result to the test report
        /// </summary>
        /// <param name="type"></param>
        /// <param name="measurement"></param>
        /// <param name="targetValue"></param>
        /// <param name="lowerLimit"></param>
        /// <param name="upperLimit"></param>
        /// <param name="frequency"></param>
        public void AddResult(ResultType type, double measurement, double targetValue, double lowerLimit, double upperLimit, int frequency = -1)
        {
            TestResult result = new TestResult(type, measurement, targetValue, lowerLimit, upperLimit, frequency);
            AddResult(result);
        }

        /// <summary>
        /// Add a new test error to the test report
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        public void AddError(ResultType type, string message)
        {
            TestError error = new TestError(type, message);
            Errors.Add(error);
        }

        /// <summary>
        /// Generate an ASCII table of the test results
        /// </summary>
        /// <returns></returns>
        public string GenerateTestReportString()
        {
            string reportText = "";
            // Top Header
            reportText +=  "~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=\n";
            reportText +=  "| OpenAutoBench-ng Test Report                                                                         |\n";
            reportText +=  "|                                                                                                      |\n";
            reportText += $"| Radio: {((Radio != null) ? Radio.ModelNumber : "No Radio"),-23}                                                                       |\n";
            // Date
            reportText +=  "~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=\n";
            reportText += $"| Report Start: {StartTime,-16}  | End: {EndTime,16}  | Duration: {Duration.ToString(@"hh\:mm\:ss")}                 |\n";
            // Table Header
            reportText +=  "~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=\n";
            reportText += $"| {"Measurement Type",-34}{"Value",-12}{"Target",-12}{"Limits",-12}{"Frequency",-16}{"Result",-8}       |\n";
            reportText +=  "~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=\n";
            foreach (TestResult result in Results)
            {
                string[] strings = result.ReportLine();
                reportText += $"| {strings[0],-34}{strings[1],-12}{strings[2],-12}{strings[3],-12}{strings[4],-16}{strings[5],-8}       |\n";
            }
            reportText += "~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=~=\n";

            return reportText;
        }

        public PdfDocument GeneratePDFReport()
        {
            // Configure font resolver
            if (GlobalFontSettings.FontResolver is not FontResolver)
                GlobalFontSettings.FontResolver = new FontResolver();

            // Create document and section
            PdfDocument doc = new PdfDocument();
            PdfPage page = doc.AddPage();

            // Set doc info
            doc.Info.Title = $"OAB-NG Test Report for Radio S/N {Radio.SerialNumber}";

            // XGraphics Object
            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Font Setup
            XFont headerFont = new XFont("Jost", 12, XFontStyle.Bold);
            XFont bodyFont = new XFont("OpenSans", 10, XFontStyle.Regular);
            XFont bodyBoldFont = new XFont("OpenSans", 10, XFontStyle.Bold);
            XFont monoFont = new XFont("RobotoMono", 10, XFontStyle.Regular);

            int lineHeight = bodyFont.Height;

            // Page Setup
            int margin = 50; // Margin around the text
            int line = margin; // Current line Y value
            page.Size = PageSize.Letter; // Set the page size to US Letter
            int pageNumber = 1;

            // Left Header
            gfx.DrawString($"OpenAutoBench-NG v{Assembly.GetExecutingAssembly().GetName().Version.ToString()}", headerFont, XBrushes.Black, margin, line, XStringFormats.BottomLeft);

            // Right Header
            gfx.DrawString($"{EndTime.ToString()}", headerFont, XBrushes.Black, page.Width - margin, line, XStringFormats.BottomRight);

            // Footer page number
            gfx.DrawString($"{pageNumber}", bodyFont, XBrushes.Black, page.Width / 2, page.Height - margin, XStringFormats.TopCenter);

            // Go down two lines
            line += (2 * lineHeight);

            // Radio Information Header & overall test results
            gfx.DrawString("Radio Information", headerFont, XBrushes.Black, margin, line);
            gfx.DrawString($"Overall Test Result: {Enum.GetName(typeof(Result), overallTestResult())}", headerFont, XBrushes.Black, margin + 235, line);
            line += lineHeight;
            // Radio Model Name & Comments Header
            gfx.DrawString("Model: ", bodyBoldFont, XBrushes.Black, margin, line);
            gfx.DrawString(Radio.ModelName, monoFont, XBrushes.Black, margin + 50, line);
            gfx.DrawString("Report Comments:", bodyBoldFont, XBrushes.Black, margin + 235, line);
            line += lineHeight;
            // Radio Model # & Comment Box
            gfx.DrawString("Model #:", bodyBoldFont, XBrushes.Black, margin, line);
            gfx.DrawString(Radio.ModelNumber, monoFont, XBrushes.Black, margin + 50, line);
            XRect commentsBox = new XRect(margin + 235, line - (int)(lineHeight / 1.5), (page.Width - (2 * margin) - 235), lineHeight * 2);
            gfx.DrawString(Comments != null ? Comments : "None", bodyFont, Comments != null ? XBrushes.Black : XBrushes.Gray, commentsBox, XStringFormats.TopLeft);
            line += lineHeight;
            // Radio Serial
            gfx.DrawString("Serial:", bodyBoldFont, XBrushes.Black, margin, line);
            gfx.DrawString(Radio.SerialNumber, monoFont, XBrushes.Black, margin + 50, line);

            // Skip a line
            line += (int)(1.5 * lineHeight);

            // Table Headers
            gfx.DrawString("Measurement", bodyBoldFont, XBrushes.Black, margin, line);
            gfx.DrawString("Value", bodyBoldFont, XBrushes.Black, margin + 160, line);
            gfx.DrawString("Target", bodyBoldFont, XBrushes.Black, margin + 235, line);
            gfx.DrawString("Limits", bodyBoldFont, XBrushes.Black, margin + 305, line);
            gfx.DrawString("Frequency", bodyBoldFont, XBrushes.Black, margin + 385, line);
            gfx.DrawString("Result", bodyBoldFont, XBrushes.Black, margin + 475, line);

            line += (lineHeight / 2);

            // Horizontal Rule
            gfx.DrawLine(XPens.Black, margin, line, page.Width - margin, line);

            line += lineHeight;

            // Test Results
            foreach (ResultType resultType in Enum.GetValues(typeof(ResultType)))
            {
                // Get the items for this type
                List<TestResult> results = Results.Where(x => x.Type == resultType).ToList();
                // Skip if empty
                if (results.Count == 0)
                {
                    continue;
                }
                // Print them out
                foreach (TestResult result in results)
                {
                    // Check if we need to start a new page
                    if (line >= page.Height - margin - lineHeight)
                    {
                        // New page & gfx
                        pageNumber++;
                        page = doc.AddPage();
                        gfx = XGraphics.FromPdfPage(page);
                        line = margin;
                        // Headers
                        gfx.DrawString($"OpenAutoBench-NG v{Assembly.GetExecutingAssembly().GetName().Version.ToString()} {reportType()}", headerFont, XBrushes.Black, margin, line, XStringFormats.BottomLeft);
                        gfx.DrawString($"{EndTime.ToString()}", headerFont, XBrushes.Black, page.Width - margin, line, XStringFormats.BottomRight);
                        line += (2 * lineHeight);
                        // Table header
                        gfx.DrawString("Measurement", headerFont, XBrushes.Black, margin, line);
                        gfx.DrawString("Value", headerFont, XBrushes.Black, margin + 160, line);
                        gfx.DrawString("Target", headerFont, XBrushes.Black, margin + 235, line);
                        gfx.DrawString("Limits", headerFont, XBrushes.Black, margin + 305, line);
                        gfx.DrawString("Frequency", headerFont, XBrushes.Black, margin + 385, line);
                        gfx.DrawString("Result", headerFont, XBrushes.Black, margin + 475, line);
                        line += (lineHeight / 2);
                        // Horizontal Rule
                        gfx.DrawLine(XPens.Black, margin, line, page.Width - margin, line);
                        line += lineHeight;
                        // Footer page number
                        gfx.DrawString($"{pageNumber}", bodyFont, XBrushes.Black, page.Width / 2, page.Height - margin, XStringFormats.TopCenter);
                    }
                    // Get the data
                    string[] result_data = result.ReportLine();
                    // Print Each Entry
                    gfx.DrawString(result_data[0], monoFont, XBrushes.Black, margin, line);
                    gfx.DrawString(result_data[1], monoFont, XBrushes.Black, margin + 160, line);
                    gfx.DrawString(result_data[2], monoFont, XBrushes.Black, margin + 235, line);
                    gfx.DrawString(result_data[3], monoFont, XBrushes.Black, margin + 305, line);
                    gfx.DrawString(result_data[4], monoFont, XBrushes.Black, margin + 385, line);
                    gfx.DrawString(result_data[5], monoFont, XBrushes.Black, margin + 475, line);
                    // Increment Line
                    line += lineHeight;
                }
                // Increment line
                line += lineHeight;
            }

            return doc;
        }

        /// <summary>
        /// Whether all the test results are passing or not
        /// </summary>
        /// <returns></returns>
        private bool testsPassed()
        {
            // If we had any errors, we failed
            if (Errors.Count > 0)
            {
                return false;
            }
            
            bool passing = true;

            foreach (TestResult result in Results)
            {
                if (result.Result != Result.PASS)
                {
                    passing = false;
                }
            }

            return passing;
        }

        private Result overallTestResult()
        {
            if (testsPassed())
            {
                return Result.PASS;
            }
            else
            {
                return Result.FAIL;
            }
        }

        private string reportType()
        {
            if (Type == ReportType.ALIGNMENT)
            {
                return "Auto-Align Report";
            }
            else
            {
                return "Test Report";
            }
        }
    }
}
