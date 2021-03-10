using Salesforce.Common;
using Salesforce.Force;
using swyfftAuto.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace swyfftAuto
{
    class Program
    {

        /**
         * Notes -: 
         *      .Nuget Package for connecting to Salesforce in already installed. Name - DeveloperForce
         *      SampleSalesforceConnectionCode function demonstrates the use of developerForce package.
         * **/

        static void Main(string[] args)
        {
            try
            {
                Automation AT = new Automation();
                AT.ScanAddressInWeb();
            }
            catch (Exception ex)
            {
                Utility.SendMail("Swyfft Import Error  --> ", ex.Message);
                Log.Write("It is main program ---" + ex.Message + "  | " + ex.InnerException + " |  " + ex.StackTrace);
            }
        }


        public static void SampleSalesforceConnectionCode()
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;


                //set OAuth key and secret variables. You can find below two details after you create a Connected App in Salesforce
                string sfdcCustomentKey = "3MVG9KsVczVNcM8yQQIoTGjDxF1LKPGn58EA.GEiVfEob6ryRWMP3pn01Nm_hdC_uVafs5vnd03r5h5wZ0f_v";
                string sfdcCustomerSecret = "CC6D85F12E776F7A267110D052F3A8D89C4D6D27EE1DAD40918F50DB701F5312";

                //set to Force.com user account that has API access enabled
                string sfdcUserName = "**";
                string sfdcPassword = "**";
                string sfdcToken = "**";

                AuthenticationClient auth = new AuthenticationClient();
                string IsSandboxUser = "true";

                // Authenticate with Salesforce
                Console.WriteLine("Authenticating with Salesforce");
                var url = IsSandboxUser.Equals("true", StringComparison.CurrentCultureIgnoreCase)
                    ? "https://test.salesforce.com/services/oauth2/token"
                    : "https://login.salesforce.com/services/oauth2/token";

                AsyncHelper.RunSync(() => auth.UsernamePasswordAsync(sfdcCustomentKey, sfdcCustomerSecret, sfdcUserName, sfdcPassword + sfdcToken, url));                
                ForceClient client = new ForceClient(auth.InstanceUrl, auth.AccessToken, "v45.0");
                
                #region Query Record
                string qry = "Select Id, Name, TGS_Policy_Number__c " +
                             " from CanaryAMS__Policy__c ";
                var results = AsyncHelper.RunSync(() => client.QueryAsync<TGS_Quote_Policy__c>(qry));

                if (results.TotalSize > 0)
                {
                    List<TGS_Quote_Policy__c> policies = results.Records;
                }
                #endregion

                #region Insert Record
                TGS_Quote_Policy__c policyS = new TGS_Quote_Policy__c();
                policyS.Name = "Test Policy";
                policyS.TGS_Policy_Number__c = "12345";
                var policy = AsyncHelper.RunSync(() => client.CreateAsync("TGS_Quote_Policy__c", policyS));
                #endregion

                #region Update Record
                policyS.TGS_Policy_Number__c = "98765";
                var resultUpdate = AsyncHelper.RunSync(() => client.UpdateAsync("TGS_Quote_Policy__c", policy.Id, policyS));
                #endregion
            }
            catch (Exception ex)
            {

            }
        }

        public static class AsyncHelper
        {
            private static readonly TaskFactory _taskFactory = new
                TaskFactory(CancellationToken.None,
                            TaskCreationOptions.None,
                            TaskContinuationOptions.None,
                            TaskScheduler.Default);

            public static TResult RunSync<TResult>(Func<Task<TResult>> func)
                => _taskFactory
                    .StartNew(func)
                    .Unwrap()
                    .GetAwaiter()
                    .GetResult();

            public static void RunSync(Func<System.Threading.Tasks.Task> func)
                => _taskFactory
                    .StartNew(func)
                    .Unwrap()
                    .GetAwaiter()
                    .GetResult();
        }
    }
}
