using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IntuneNetworkPrintMapping
{
    internal class NetworkPrintMapping
    {
        private PolicyRetrival myPolicyRetrival = null;
        private UpdateHandler myUpdateHandler = null;
        private LogWriter myLogWriter = null;
        private bool isStartedFromStartmenu = false;

        public NetworkPrintMapping(PolicyRetrival policyRetrival)
        {
            myPolicyRetrival = policyRetrival;
            myUpdateHandler = new UpdateHandler();
            myLogWriter = new LogWriter("NetworkPrintMapping");
        }

        private void MapPrinters()
        {
                if (myPolicyRetrival.isEnabled())
                try
                {

                    List<NetworkPrintMappingPolicy> policies = myPolicyRetrival.Policies;
                    PrinterSettings myPrinterSettings = new PrinterSettings();

                    foreach (NetworkPrintMappingPolicy policy in policies)
                    {
                        if (policy.PrinterName != null)
                            try
                            {
                                if (policy.Operation == "Add")
                                {
                                    if (myPrinterSettings.IsPrinterInstalled(policy.PrinterName) == false)
                                    {
                                        PrinterSettings.AddPrinter(policy.PrinterName);
                                        Thread.Sleep(500);
                                        if (myPrinterSettings.IsPrinterInstalled(policy.PrinterName) == true)
                                        {
                                            myLogWriter.LogWrite("Mapped printer " + policy.PrinterName, 1);
                                            if (policy.setDefault)
                                            {
                                                PrinterSettings.SetDefaultPrinter(policy.PrinterName);
                                                myLogWriter.LogWrite("Set " + policy.PrinterName + " as default printer", 1);
                                            }
                                        }
                                        else
                                        {
                                            myLogWriter.LogWrite("Printer " + policy.PrinterName + " could not be mapped.", 2);
                                        }
                                    }
                                    if (policy.PrinterDisplayName != null)
                                        PrinterSettings.RenamePrinter(policy.PrinterName, policy.PrinterDisplayName);


                                }
                                else if (policy.Operation == "Delete")
                                    if (myPrinterSettings.IsPrinterInstalled(policy.PrinterName) == true)
                                    {
                                        PrinterSettings.DeletePrinter(policy.PrinterName);
                                        myLogWriter.LogWrite("Removed printer " + policy.PrinterName, 1);
                                    }


                                //myLogWriter.LogWrite("Mapped networkdrive " + policy.driveLetter + " to " + policy.uncPath);
                            }
                            catch (Exception e)
                            {
                                myLogWriter.LogWrite("Failed to " + policy.Operation + " printer " + policy.PrinterName + "\nException: " + e.ToString(), 2);
                                // do nothing
                            }
                    }
                }
                catch (Exception e)
                {
                    myLogWriter.LogWrite("An unknown error occured.\nException: " + e.ToString(), 3);
                }
        }

        public void Execute()
        {
            DateTime dateLastUpdate = DateTime.Now;
            DateTime dateLastPrinterConfigurationRefresh = DateTime.Now;
            DateTime currentDate = DateTime.Now;
            long elapsedTicks = 0;
            TimeSpan elapsedSpan = new TimeSpan();

            NetworkChangeDetector myNetworkChangeDetector = new NetworkChangeDetector();
            myLogWriter.LogWrite("Initialized NetworkChangeDetector");

            Task<bool> searchUpdateTask = null;
            Task<bool> installUpdateTask = null;

            myLogWriter.LogWrite("Initialized UpdateTask");
            int retryCount = 0;
            bool shouldRun = true;
            int updateInterval = myPolicyRetrival.getUpdateInterval();
            int printerConfigurationRefreshInterval = myPolicyRetrival.getPrinterConfigurationRefreshIntervall();
            while (shouldRun)
            {
                int sleepTime = myPolicyRetrival.getRefreshInterval();

                if (myPolicyRetrival.isNetworkTestEnabled())
                {
                    if (myNetworkChangeDetector.CheckNetworkChange())
                    {
                        myLogWriter.LogWrite("CheckNetworkChange() indicated a network change.");
                        retryCount = myPolicyRetrival.getRetryCount();
                    }
                }
                else
                {
                    retryCount = 1;
                }

                currentDate = DateTime.Now;
                elapsedTicks = currentDate.Ticks - dateLastPrinterConfigurationRefresh.Ticks;
                elapsedSpan = new TimeSpan(elapsedTicks);
                if ((elapsedSpan.TotalSeconds >= printerConfigurationRefreshInterval))
                {
                    retryCount = myPolicyRetrival.getRetryCount();
                    myLogWriter.LogWrite("Policy Refresh interval is expired.");
                }

                if (retryCount > 0)
                {
                    MapPrinters();
                    dateLastPrinterConfigurationRefresh = DateTime.Now;
                    Thread.Sleep(sleepTime);
                }
                else
                {
                    if (retryCount == 0)
                    {
                        myLogWriter.LogWrite("Will now go to sleep.");
                    }

                    Thread.Sleep(sleepTime);
                    bool updatesReadyToInstall = false;
                    if (searchUpdateTask == null)
                        searchUpdateTask = myUpdateHandler.SearchAndDownloadUpdates();
                    if (searchUpdateTask.IsCompleted)
                        updatesReadyToInstall = searchUpdateTask.Result;

                    if (updatesReadyToInstall)
                    {
                        if (installUpdateTask == null)
                            installUpdateTask = myUpdateHandler.InstallUpdate();
                        if (installUpdateTask.IsCompleted)
                            if (installUpdateTask.Result)
                            {
                                shouldRun = false;
                                myLogWriter.LogWrite("Will now restart Intune Network Print Mapping to install updates.", 1);
                            }
                    }
                    currentDate = DateTime.Now;
                    elapsedTicks = currentDate.Ticks - dateLastUpdate.Ticks;
                    elapsedSpan = new TimeSpan(elapsedTicks);

                    if ((elapsedSpan.TotalSeconds >= updateInterval) && (updateInterval > 60))
                    {
                        dateLastUpdate = DateTime.Now;
                        installUpdateTask = null;
                        searchUpdateTask = null;
                    }

                }
                retryCount--;

            }
        }
    }
}
