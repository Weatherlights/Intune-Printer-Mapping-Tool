using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntuneNetworkPrintMapping
{


    class PolicyRetrival
    {
        private string policyLocation = "Software\\Policies\\weatherlights.com\\NetworkPrintMapping";
        private RegistryKey policyStoreKeyUser = null;
        private RegistryKey policyStoreKeyMachine = null;

        public PolicyRetrival()
        {
            policyStoreKeyUser = Registry.CurrentUser.OpenSubKey(policyLocation, false);
            policyStoreKeyMachine = Registry.LocalMachine.OpenSubKey(policyLocation, false);
        }

        public List<NetworkPrintMappingPolicy> Policies
        {
            get
            {
                List<NetworkPrintMappingPolicy> policies = new List<NetworkPrintMappingPolicy>();
                if (this.retrivePolicyNames2(Registry.CurrentUser) != null) // This is the new configuration which is more flexible. If it is used it is prefered over the old configuration.
                {
                    foreach (string policyName in this.retrivePolicyNames2(Registry.CurrentUser))
                    {
                        policies.Add(this.GetPolicyByName2(policyName, Registry.CurrentUser));
                    }
                }

                if (this.retrivePolicyNames2(Registry.LocalMachine) != null) // This is the new configuration which is more flexible. If it is used it is prefered over the old configuration.
                {
                    foreach (string policyName in this.retrivePolicyNames2(Registry.LocalMachine))
                    {
                        policies.Add(this.GetPolicyByName2(policyName, Registry.LocalMachine));
                    }
                }
                /*               else if (this.retrivePolicyNames1() != null)
                               {
                                   foreach (string policyName in this.retrivePolicyNames1())
                                   {
                                       policies.Add(this.GetPolicyByName1(policyName));
                                   }
                               }*/
                return policies;
            }
        }

        private string[] retrivePolicyNames1(RegistryKey context)
        {
            RegistryKey myPolicyKey = Registry.CurrentUser.OpenSubKey(policyLocation + "\\Policies2");
            if (myPolicyKey != null)
            {
                return myPolicyKey.GetValueNames();
            }
            else
            {
                return null;
            }
        }

        private string[] retrivePolicyNames2(RegistryKey context)
        {
            RegistryKey myPolicyKey = context.OpenSubKey(policyLocation + "\\Policies");
            if (myPolicyKey != null)
            {
                return myPolicyKey.GetSubKeyNames();
            }
            else
            {
                return null;
            }
        }

/*        private NetworkPrintMappingPolicy GetPolicyByName1(string name)
        {
            NetworkPrintMappingPolicy policy = null;
            using (RegistryKey policyPolicyKey = Registry.CurrentUser.OpenSubKey(policyLocation + "\\Policies"))
            {
                policy = new NetworkPrintMappingPolicy();
                if (policyPolicyKey.GetValueNames().Contains(name))
                {
                    string myPolicyValue = (string)policyPolicyKey.GetValue(name);
                    string[] myPolicyValueArray = myPolicyValue.Split(';');
                    if (myPolicyValueArray.Length > 1)
                    {
                        policy.driveLetter = myPolicyValueArray[0];
                        string myPathWithVariables = myPolicyValueArray[1];
                        policy.uncPath = Environment.ExpandEnvironmentVariables(myPathWithVariables);
                        if (myPolicyValueArray.Length > 3)
                        {
                            policy.Username = myPolicyValueArray[2];
                            policy.Password = myPolicyValueArray[3];
                        }
                    }
                }
            }
            return policy;
        } */

        private NetworkPrintMappingPolicy GetPolicyByName2(string name, RegistryKey context)
        {
            NetworkPrintMappingPolicy policy = null;
            using (RegistryKey policyPolicyKey = context.OpenSubKey(policyLocation + "\\Policies\\" + name))
            {
                policy = new NetworkPrintMappingPolicy();
                if (policyPolicyKey.GetValueNames().Contains("DisplayName"))
                    policy.PrinterDisplayName = (string)policyPolicyKey.GetValue("DisplayName");
                string myPathWithVariables = (string)policyPolicyKey.GetValue("Path");
                if (policyPolicyKey.GetValueNames().Contains("Operation"))
                    policy.Operation = (string)policyPolicyKey.GetValue("Operation");

                policy.setDefault = false;
                if (policyPolicyKey.GetValueNames().Contains("SetDefault"))
                    if ((int)policyPolicyKey.GetValue("SetDefault") >= 1)
                        policy.setDefault = true;
                
                        policy.PrinterName = Environment.ExpandEnvironmentVariables(myPathWithVariables);
                if (policyPolicyKey.GetValueNames().Contains("Username") && policyPolicyKey.GetValueNames().Contains("Password"))
                {
                    if ((string)policyPolicyKey.GetValue("Username") != "")
                        policy.Username = (string)policyPolicyKey.GetValue("Username");
                    if ((string)policyPolicyKey.GetValue("Password") != "")
                        policy.Password = (string)policyPolicyKey.GetValue("Password");
                }
            }
            return policy;
        }

        private bool TestRegistryKeyValue(string AttributeName, RegistryKey policyStoreKey)
        {
            if (policyStoreKey != null)
                if (policyStoreKey.GetValueNames().Contains(AttributeName))
                    return true;
            return false;
        }


  


        public bool isEnabled()
        {

            try
            {
                if (TestRegistryKeyValue("Enabled", policyStoreKeyMachine))
                {
                    int value = (int)policyStoreKeyMachine.GetValue("Enabled");
                    if (value > 0)
                        return true;
                    else
                        return false;
                }
                else
                    return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }


        public int getRefreshInterval()
        {
            try
            {
                if (TestRegistryKeyValue("RefreshInterval", policyStoreKeyMachine))
                    return (int)policyStoreKeyMachine.GetValue("RefreshInterval");
                else
                    return 10000;
            }
            catch (Exception e)
            {
                return 10000;
            }
        }

        public int getRetryCount()
        {
            try
            {
                if (TestRegistryKeyValue("RetryCount", policyStoreKeyMachine))
                    return (int)policyStoreKeyMachine.GetValue("RetryCount");
                else
                    return 15;
            }
            catch (Exception e)
            {
                return 15;
            }
        }

        public int getUpdateInterval()
        {
            try
            {
                if (TestRegistryKeyValue("UpdateInterval", policyStoreKeyMachine))
                    return (int)policyStoreKeyMachine.GetValue("UpdateInterval");
                else
                    return 10800;
            }
            catch (Exception e)
            {
                return 10800;
            }
        }

        public bool isNetworkTestEnabled()
        {
            try
            {
                if (TestRegistryKeyValue("NetworkTestEnabled", policyStoreKeyMachine))
                {
                    int value = (int)policyStoreKeyMachine.GetValue("NetworkTestEnabled");
                    if (value > 0)
                        return true;
                    else
                        return false;
                }
                else
                    return true;
            }
            catch (Exception e)
            {
                return true;
            }
        }

    }
}