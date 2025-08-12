using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntuneNetworkPrintMapping
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            LogWriter myLogWriter = new LogWriter("Program");
            myLogWriter.LogWrite("Intune Network Print Mapping has started.");
            PolicyRetrival myPolicyRetrival = new PolicyRetrival();
           if (!myPolicyRetrival.isEnabled() && args.Contains("Startmenu"))
            {
                myLogWriter.LogWrite("Has been started from start menu but configuration is missing. Will display user hint.", 2);
                UnconfiguredNoticeWindow myUnconfiguredNoticeWindow = new UnconfiguredNoticeWindow();
                myUnconfiguredNoticeWindow.ShowDialog();
            } 
            PrinterSettings printerSettings = new PrinterSettings();

            NetworkPrintMapping myNetworkPrintMapping = new NetworkPrintMapping(myPolicyRetrival);
            myNetworkPrintMapping.Execute();
            myLogWriter.LogWrite("Exiting Intune Network Print Mapping", 2);
            //PrinterSettings.AddPrinter("\\\\ambushvalley.weatherlights.com\\LocalToilet");
        }
    }
}
