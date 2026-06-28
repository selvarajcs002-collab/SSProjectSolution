using System;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SSProjectSolution.Services
{
    public class PrinterHealthService : IPrinterHealthService
    {
        public async Task<(bool IsSuccess, string ErrorCode, string ErrorDescription)> CheckPrinterStatusAsync(string printerName)
        {
            if (string.IsNullOrWhiteSpace(printerName))
            {
                return (false, "PRINTER_NOT_CONFIGURED", "Printer name is not configured.");
            }

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // WMI is only supported on Windows
                return (true, string.Empty, string.Empty);
            }

            return await Task.Run(() =>
            {
                try
                {
                    string query = $"SELECT * FROM Win32_Printer WHERE Name = '{printerName.Replace("'", "''")}'";
                    using var searcher = new ManagementObjectSearcher(query);
                    using var results = searcher.Get();

                    if (results.Count == 0)
                    {
                        return (false, "PRINTER_NOT_INSTALLED", "No printer is installed with the specified name.");
                    }

                    foreach (ManagementObject printer in results)
                    {
                        // 1 = Other, 2 = Unknown, 3 = Idle, 4 = Printing, 5 = Warmup, 6 = Stopped Printing, 7 = Offline
                        uint status = printer["PrinterStatus"] != null ? (uint)(ushort)printer["PrinterStatus"] : 3;
                        uint extendedStatus = printer["ExtendedPrinterStatus"] != null ? (uint)(ushort)printer["ExtendedPrinterStatus"] : 0;
                        
                        // Status mapping
                        if (status == 7) // Offline
                        {
                            return (false, "PRINTER_OFFLINE", "The printer is offline.");
                        }

                        // ExtendedPrinterStatus codes
                        // 1 = Other, 2 = Unknown, 3 = Idle, 4 = Printing, 5 = Warming Up, 6 = Stopped Printing, 7 = Offline, 8 = Paused, 9 = Error, 10 = Busy, 11 = Not Available, 12 = Waiting, 13 = Processing, 14 = Initialization, 15 = Power Save, 16 = Pending Deletion, 17 = I/O Active, 18 = Manual Feed
                        if (extendedStatus == 7) 
                        {
                            return (false, "PRINTER_OFFLINE", "The printer is offline.");
                        }
                        else if (extendedStatus == 8)
                        {
                            return (false, "PRINTER_PAUSED", "The printer is currently paused.");
                        }
                        
                        // Checking ErrorState and other properties
                        uint printerState = printer["PrinterState"] != null ? (uint)printer["PrinterState"] : 0;
                        // Some common bits in PrinterState:
                        // 0x00000001 = Paused
                        // 0x00000002 = Error
                        // 0x00000010 = Out of Paper
                        // 0x00000080 = Offline

                        if ((printerState & 0x00000010) != 0)
                        {
                            return (false, "OUT_OF_PAPER", "The printer is out of paper.");
                        }
                        if ((printerState & 0x00000080) != 0)
                        {
                            return (false, "PRINTER_OFFLINE", "The printer is offline.");
                        }
                        if ((printerState & 0x00000001) != 0)
                        {
                            return (false, "PRINTER_PAUSED", "The printer is currently paused.");
                        }
                        if ((printerState & 0x00000002) != 0)
                        {
                            return (false, "PRINTER_ERROR", "The printer is in an error state.");
                        }

                        // WMI WorkOffline property
                        if (printer["WorkOffline"] != null && (bool)printer["WorkOffline"] == true)
                        {
                            return (false, "PRINTER_OFFLINE", "The printer is offline.");
                        }
                    }

                    return (true, string.Empty, string.Empty);
                }
                catch (Exception ex)
                {
                    return (false, "UNKNOWN_ERROR", $"An unexpected printer error occurred: {ex.Message}");
                }
            });
        }
    }
}
