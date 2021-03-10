using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Salesforce.Common;
using Salesforce.Common.Models.Json;
using Salesforce.Force;
//using swyfftAuto.com.salesforce.enterprise;
using swyfftAuto.Model;
using swyfftAuto.Model.Enums;
using swyfftAuto.Model.SalesForceModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using static swyfftAuto.Program;

namespace swyfftAuto
{
    public class Automation
    {
        IWebDriver driver = null;
        IWebElement txtUserid, txtPassword, frmLogin;
        public Automation()
        {

        }

        #region Function runs automation grabs data for new policies and saves the data in SQL. We need to replace the SQL part with Salesforce
        /** For the automation to run add download Chrome driver from 
         * https://chromedriver.chromium.org/downloads and updare the path of the driver in App config appsettings "ChromeDriver"
         *  **/
        public void ScanAddressInWeb()
        {
            try
            {
                string pathCD = ConfigurationManager.AppSettings["ChromeDriver"].ToString();
                string url = "https://www.swyfft.com/Login";
                string username = ConfigurationManager.AppSettings["username"].ToString();
                string password = ConfigurationManager.AppSettings["password"].ToString();
                ChromeOptions o = new ChromeOptions();
                o.AddArguments("--disable-extensions");
                o.AddArguments("--start-maximized");
                o.AddAdditionalCapability("useAutomationExtension", false);
                driver = new ChromeDriver(pathCD, o);
                driver.Navigate().GoToUrl(url);
                txtUserid = driver.FindElement(By.Id("login-AgentID"));
                txtPassword = driver.FindElement(By.Id("login-Password"));
                frmLogin = driver.FindElement(By.Id("login-button"));
                txtUserid.SendKeys(username);
                txtPassword.SendKeys(password);

                // Now submit the form. WebDriver will find the form for us from the element
                frmLogin.Submit();
                Thread.Sleep(5000);

                new WebDriverWait(driver, TimeSpan.FromSeconds(3000)).Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.Id("history-navbar-link")));

                try
                {
                    IWebElement historyTab = driver.FindElement(By.Id("history-navbar-link"));

                    if (historyTab != null)
                    {
                        Thread.Sleep(5000);
                        historyTab.Click();
                    }
                }
                catch (Exception ex)
                {
                    Thread.Sleep(2500);
                    IWebElement historyTab = driver.FindElement(By.Id("history-navbar-link"));

                    if (historyTab != null)
                    {
                        Thread.Sleep(5000);
                        historyTab.Click();
                    }
                }

                ///history?homeowners
                ///
                new WebDriverWait(driver, TimeSpan.FromSeconds(3000)).Until(ExpectedConditions.UrlContains("history?homeowners"));
                Thread.Sleep(3000);

                IJavaScriptExecutor js11 = (IJavaScriptExecutor)driver;
                js11.ExecuteScript("arguments[0].click()", driver.FindElement(By.CssSelector(".glyphicon.glyphicon-ok")));

                Thread.Sleep(5000);

                //Log.Write("It is main program ---" + adr.RecID);
                var table = driver.FindElement(By.TagName("table"));
                Thread.Sleep(1000);
                var rowsFull = table.FindElements(By.TagName("tr"));
                var rows = rowsFull.Where(s => s.Text.Contains(string.Concat(DateTime.Now.Month, "/", DateTime.Now.Day, "/", DateTime.Now.Year)));
                //var rows = rowsFull.Where(s => s.Text.Contains("3/8/2021"));

                if (rows.Count() < 1)
                {
                    string strDate = DateTime.Now.Month < 10 ? "0" + DateTime.Now.Month.ToString() : DateTime.Now.Month.ToString();
                    strDate += DateTime.Now.Day < 10 ? "/0" + DateTime.Now.Day.ToString() : "/" + DateTime.Now.Day.ToString();
                    strDate += DateTime.Now.Year.ToString();

                    rows = rowsFull.Where(s => s.Text.Contains(strDate));
                }

                //Thread.Sleep(2000);
                Iswyfft si = new Policy();

                foreach (var row in rows)
                {
                    var rowTds = row.FindElements(By.TagName("td"));
                    Thread.Sleep(1000);
                    bool flag = false;
                    int n = 0;
                    var ts = string.Empty;
                    SwyfftRawData cust = new SwyfftRawData();

                    foreach (var td in rowTds)
                    {
                        switch (n)
                        {
                            case 0:
                                var status = td.FindElement(By.CssSelector(".glyphicon"));
                                ts = status.GetAttribute("title");
                                cust.Address = td.Text.ToString();
                                cust.Address = cust.Address.Replace("\r\n", " ");
                                break;

                            case 1:
                                cust.Amount = Convert.ToDecimal(td.Text.ToString().Replace("$", ""));
                                break;

                            case 2:
                                var paymentType = td.FindElement(By.TagName("a")).Text.ToString().Trim();

                                if (paymentType.ToLower().Contains("monthly"))
                                {
                                    cust.billing_frequency = "12-Pay";
                                }

                                if (paymentType.ToLower().Contains("annual"))
                                {
                                    cust.billing_frequency = "1-Pay";
                                }

                                if (paymentType.ToLower().Contains("echeck"))
                                {
                                    cust.BillingMethod = "Electronic Funds Transfer (EFT)";
                                }

                                if (paymentType.ToLower().Contains("credit card"))
                                {
                                    cust.BillingMethod = "Credit Card (CC)";
                                }

                                if (paymentType.ToLower().Contains("escrow"))
                                {
                                    cust.billing_frequency = "1-Pay";
                                    cust.BillingMethod = "Mortgagee Bill";
                                }

                                cust.TypeofBilling = "Direct Bill - Automatic";

                                if (cust.billing_frequency == null || cust.BillingMethod == null || cust.TypeofBilling == null)
                                {
                                    Utility.SendMail("Swyfft Import Error", cust.CustName + "\n --> New Payment Type - " + paymentType + "");
                                    break;
                                }
                                break;
                            case 3:
                                cust.CustName = td.Text.ToString();
                                break;

                            case 4:
                                cust.EftDate = Convert.ToDateTime(td.Text.ToString());
                                break;

                            case 5:
                                cust.PolicyNumber = td.Text.ToString();
                                break;

                            case 6:
                                cust.SalesAgent = td.Text.ToString();
                                break;

                            case 7:
                                //check pdf to delete if pdf file still exist
                                si.deletefiles();
                                var aval = td.FindElements(By.CssSelector(".action-wrapper-bs.file-available-bs"));
                                var aval1 = td.FindElements(By.CssSelector(".action-wrapper-bs"));
                                var a = td.FindElements(By.TagName("a"));

                                if (a.Count == 1)
                                {
                                    foreach (var suba in a)
                                    {
                                        Thread.Sleep(1000);
                                        suba.Click();
                                        Thread.Sleep(3000);

                                        try
                                        {
                                            IWebElement modal;
                                            try
                                            {
                                                modal = driver.FindElement(By.XPath("/html/body/div[112]/div[2]"));
                                            }
                                            catch (Exception e)
                                            {
                                                try
                                                {
                                                    modal = driver.FindElement(By.XPath("html/body/div[114]/div[2]")); /// html / body / div[108] / div[2]
                                                }
                                                catch (Exception ex)
                                                {
                                                    try
                                                    {
                                                        modal = driver.FindElement(By.XPath("html/body/div[115]/div[2]")); // 

                                                    }
                                                    catch (Exception ex1)
                                                    {
                                                        try
                                                        {
                                                            modal = driver.FindElement(By.XPath("html/body/div[108]/div[2]"));
                                                        }
                                                        catch (Exception ex2)
                                                        {
                                                            try
                                                            {
                                                                modal = driver.FindElement(By.XPath("html/body/div[109]/div[2]"));
                                                            }
                                                            catch (Exception ex3)
                                                            {
                                                                modal = driver.FindElement(By.ClassName("modal"));
                                                            }

                                                        }
                                                    }
                                                }
                                            }

                                            if (modal.GetCssValue("display") == "block")
                                            {
                                                var policyOverview = modal.FindElements(By.ClassName("modal-body"));

                                                if (policyOverview.Count > 2)
                                                {
                                                    var pdf = policyOverview[2].FindElements(By.ClassName("policy-download-link-bs"));

                                                    if (pdf.Count > 0)
                                                    {
                                                        foreach (var link in pdf)
                                                        {
                                                            try
                                                            {
                                                                if (link.Text.Contains("Download Policy") || link.Text.Contains("Download Invoice") || link.Text.Contains("Download Receipt"))
                                                                {
                                                                    link.Click();
                                                                    flag = true;
                                                                    Thread.Sleep(1000);
                                                                }
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                Utility.SendMail("Swyfft Import Error", cust.CustName + " --> " + ex + "");
                                                            }
                                                        }
                                                    }
                                                    modal.FindElement(By.ClassName("x")).Click();
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Utility.SendMail("Swyfft Import Error", cust.CustName + " --> " + ex + "");
                                        }
                                    }
                                }
                                break;
                        }
                        n++;
                    }

                    if (flag)
                    {
                        var downloadedFiles = si.readfiles();

                        foreach (var item in downloadedFiles)
                        {
                            if (item.IndexOf("Invoice") >= 0)
                            {
                                string pathfile = ConfigurationManager.AppSettings.Get("sourcePath").ToString() + "\\" + item;
                                cust.Invoice = item;
                                cust.InvoiceText = PdfParse.ExtractTextFromPdf(pathfile);
                            }
                            if (item.IndexOf("Payment") >= 0)
                            {
                                cust.Payment = item;
                                string pathfile = ConfigurationManager.AppSettings.Get("sourcePath").ToString() + "\\" + item;
                                cust.StatusText = PdfParse.ExtractTextFromPdf(pathfile);
                            }
                            if (item.IndexOf("Policy") > 0)
                            {
                                cust.Policy = item;
                            }
                        }

                        if (string.IsNullOrEmpty(cust.Payment) || string.IsNullOrEmpty(cust.Invoice) || string.IsNullOrEmpty(cust.Policy))
                        {
                            string check = string.Empty;
                        }
                        else
                        {
                            string term = string.Empty;
                            string priorTerm = string.Empty, policyType = string.Empty, city = string.Empty, state = string.Empty, street = string.Empty, zipCode = string.Empty;
                            DateTime expirationDate = new DateTime();

                            var invoiceDetails = cust.InvoiceText.Split('\n').ToList();

                            GetInvoiceDetails(cust, ref policyType, ref city, ref street, ref zipCode, ref expirationDate, invoiceDetails);

                            if (cust.billing_frequency == "1-Pay")
                            {
                                term = "12 Month";
                            }
                            else if (cust.billing_frequency == "12-Pay")
                            {
                                term = "1 Month";
                            }

                            priorTerm = GetPriorTerm(cust);

                            var policyStatus = cust.StatusText.ToLower().Trim().Contains("issued") ? Status.Issued.ToString() : "";
                            policyStatus = cust.StatusText.ToLower().Trim().Contains("cancelled") ? Status.Cancelled.ToString() : "";

                            var policy = GetPolicyDetails(cust, term, priorTerm, policyType, city, state, street, zipCode, expirationDate, policyStatus);

                            ConnectSalesforceDetails(policy);
                            
                            //delete downloaded files after saving
                            si.deletefiles();
                        }
                    }

                    flag = false;
                }
            }
            catch (Exception ex)
            {
                Utility.SendMail("Swyfft Import Error", ex + "");
                Log.Write("It is Error as ---" + ex.Message + "  | " + ex.InnerException + " |  " + ex.StackTrace + " |  " + ex);
            }
            finally
            {
                ScanEndoredTab(driver);
                driver.Close();
                driver.Quit();
            }
        }

        private void ConnectSalesforceDetails(PolicyModel policy)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;

                //set OAuth key and secret variables. You can find below two details after you create a Connected App in Salesforce
                string sfdcCustomentKey = "3MVG9KsVczVNcM8yQQIoTGjDxF1LKPGn58EA.GEiVfEob6ryRWMP3pn01Nm_hdC_uVafs5vnd03r5h5wZ0f_v";
                string sfdcCustomerSecret = "CC6D85F12E776F7A267110D052F3A8D89C4D6D27EE1DAD40918F50DB701F5312";

                //set to Force.com user account that has API access enabled
                string sfdcUserName = ConfigurationManager.AppSettings["SalesForceUserName"].ToString();
                string sfdcPassword = ConfigurationManager.AppSettings["SalesForcePsw"].ToString();
                string sfdcToken = ConfigurationManager.AppSettings["SalesForceToken"].ToString();

                AuthenticationClient auth = new AuthenticationClient();
                string IsSandboxUser = "true";

                // Authenticate with Salesforce
                Console.WriteLine("Authenticating with Salesforce");
                var url = IsSandboxUser.Equals("true", StringComparison.CurrentCultureIgnoreCase)
                    ? "https://test.salesforce.com/services/oauth2/token"
                    : "https://login.salesforce.com/services/oauth2/token";

                AsyncHelper.RunSync(() => auth.UsernamePasswordAsync(sfdcCustomentKey, sfdcCustomerSecret, sfdcUserName, sfdcPassword + sfdcToken, url));
                ForceClient client = new ForceClient(auth.InstanceUrl, auth.AccessToken, "v45.0");

                InsertSalesforceDetails(policy, client);
                
            }
            catch (Exception ex)
            {
                Utility.SendMail("Error occurred while connecting to SalesForce -> ", ex.Message);
                Console.WriteLine(ex.Message);
            }

            
        }

        private void InsertSalesforceDetails(PolicyModel policy, ForceClient client)
        {

            try
            {
  
                //To fetch the record Type
                string qry = string.Format("Select Id from RecordType where Name = '{0}'", policy.RecordType);

                var resultsRecordType = AsyncHelper.RunSync(() => client.QueryAsync<RecordType_c>(qry));
                string recordType = resultsRecordType.Records[0].ID;


                qry = string.Format("Select Id from Account where Name = '{0}'", policy.Account.Name);

                var resultAccount = AsyncHelper.RunSync(() => client.QueryAsync<Account_s>(qry));

                var account = new Account();

                account = policy.Account;

                string accountRecordID = string.Empty;

                if (resultAccount.Records.Count() > 0)
                {
                    accountRecordID = resultAccount.Records[0].Id;
                    var resultQry = AsyncHelper.RunSync(() => client.UpdateAsync("Account", accountRecordID, account));
                }
                else
                {
                    var resultQry = AsyncHelper.RunSync(() => client.CreateAsync("Account", account));
                    if (resultQry.Success)
                    {
                        accountRecordID = resultQry.Id;
                    }
                }

                qry = string.Format("Select Id from Contact where FirstName = '{0}' AND LastName = '{1}'", policy.Contact.FirstName, policy.Contact.LastName);

                var resultContact = AsyncHelper.RunSync(() => client.QueryAsync<Contact>(qry));

                string contactRecordID = string.Empty;

                var contact = new Contact();
                contact = policy.Contact;

                if (resultContact.Records.Count() > 0)
                {
                    contactRecordID = resultContact.Records[0].Id;
                    var resultQry = AsyncHelper.RunSync(() => client.UpdateAsync("Contact", contactRecordID, contact));
                }
                else
                {
                    var resultQry = AsyncHelper.RunSync(() => client.CreateAsync("Contact", contact));
                    if (resultQry.Success)
                    {
                        contactRecordID = resultQry.Id;
                    }
                }

                var property = new Property();

                property = policy.Property;
                property.TGS_Account__c = accountRecordID;

                qry = string.Format("Select Id from TGS_Property__c where Name = '{0}'", policy.Property.Name);

                var resultProperty = AsyncHelper.RunSync(() => client.QueryAsync<Account_s>(qry));

                string propertyRecordID = string.Empty;

                if (resultProperty.Records.Count>0)
                {
                    propertyRecordID = resultProperty.Records[0].Id;
                    var resultQry = AsyncHelper.RunSync(() => client.UpdateAsync("TGS_Property__c", propertyRecordID, property));
                }
                else
                {
                    var resultQry = AsyncHelper.RunSync(() => client.CreateAsync("TGS_Property__c", property));
                    if (resultQry.Success)
                    {
                        propertyRecordID = resultQry.Id;
                    }
                }

                var tGS_Quote_Policy__C = new TGS_Quote_Policy__c
                {
                        TGS_Account__c  = accountRecordID,
                        TGS_Contact__c = contactRecordID,
                        TGS_Property__c = propertyRecordID,
                        RecordTypeId = recordType,
                        TGS_Product_Type__c = policy.ProductType,
                        TGS_Policy_Number__c = policy.PolicyNumber,
                        TGS_Billing_Frequency__c = policy.BillingFrequency,
                        Type_of_Billing__c = policy.TypeOfBilling,
                        TGS_Billing_Method__c = policy.BillingMethod,
                        TGS_Net_Premium__c = Convert.ToDouble(policy.Premium),
                        TGS_Policy_Type__c = policy.PolicyType,
                        TGS_Policy_Status__c = policy.Status,
                        TGS_TGSI_Status__c = policy.TGSIStatus,
                        TGS_Effective_Date__c = policy.EffectiveDate,
                        TGS_Expiration_Date__c = policy.ExpirationDate,
                        TGS_Cancellation_Date__c = policy.CancellationDate,

                        //TGS_Next_Term__c = policy.Term, ///// need to confirm the salesforce object name ?
                        TGS_RenewalTerm__c = policy.Term, ///// need to confirm the salesforce object name ?

                };

                string[] policyNumber = policy.PolicyNumber.Split('-');
                int priorPolicyNumber = 0;

                if (int.TryParse(policyNumber[policyNumber.Length - 1], out priorPolicyNumber))
                {
                    if (priorPolicyNumber == 0)
                    {
                        var result = AsyncHelper.RunSync(() => client.CreateAsync("TGS_Quote_Policy__c", tGS_Quote_Policy__C));
                        Console.WriteLine("Quote Policy Inserted : " + result.Id + ", Policy Number" + policy.PolicyNumber);
                    }
                    else
                    {
                        priorPolicyNumber = priorPolicyNumber - 1;
                        policyNumber[policyNumber.Length - 1] = priorPolicyNumber < 10 ? string.Concat("0", priorPolicyNumber.ToString()) : priorPolicyNumber.ToString();
                        tGS_Quote_Policy__C.TGS_Prior_PolicyNumber__c = string.Join("-", policyNumber);

                        qry = string.Format("Select Id from TGS_Quote_Policy__c where TGS_Policy_Number__c = '{0}'", policy.PolicyNumber);

                        var resultQuotePolicy = AsyncHelper.RunSync(() => client.QueryAsync<Account_s>(qry));

                        string quotePolicyRecordId = string.Empty;

                        if (resultQuotePolicy.Records.Count > 0)
                        {
                            quotePolicyRecordId = resultQuotePolicy.Records[0].Id;
                            var resultQry = AsyncHelper.RunSync(() => client.UpdateAsync("TGS_Quote_Policy__c", quotePolicyRecordId, tGS_Quote_Policy__C));
                            Console.WriteLine("Quote Policy Updated : " + quotePolicyRecordId + ", Policy Number" + policy.PolicyNumber);
                        }
                        else
                        {
                            var result = AsyncHelper.RunSync(() => client.CreateAsync("TGS_Quote_Policy__c", tGS_Quote_Policy__C));
                            Console.WriteLine("Quote Policy Inserted : " + result.Id + ", Policy Number" + policy.PolicyNumber);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utility.SendMail("Error occurred Inserting / Updating to SalesForce -> ", ex.Message);
                Console.WriteLine(ex.Message);
            }
        }

        private static PolicyModel GetPolicyDetails(SwyfftRawData cust, string term, string priorTerm, string policyType, string city, string state, string street, string zipCode, DateTime expirationDate, string policyStatus)
        {
            string Name = cust.CustName;
            string FirstName = Name.Split(' ')[0];
            string LastName = Name.Split(' ').Length > 1 ? Name.Split(' ')[1] : string.Empty;

            var policy = new PolicyModel()
            {
                Account = new Account()
                {
                    Name = Name,
                    BillingCity = city,
                    BillingState = state,
                    BillingPostalCode = zipCode,
                    BillingStreet = street
                },
                Contact = new Contact()
                {
                    FirstName = FirstName,
                    LastName = LastName,
                    MailingCity = city,
                    MailingCountry = null,
                    MailingPostalCode = zipCode,
                    MailingState = state
                },
                Property = new Property()
                {
                    Name = Name,
                    TGS_City__c = city,
                    TGS_State__c = state,
                    TGS_Zip_Code__c = zipCode
                },
                Carrier = "Swyfft",
                CarrierProduct = "Swyfft-Home",
                RecordType = Model.Enums.RecordType.Policy.ToString(),
                ProductType = null,
                PolicyNumber = cust.PolicyNumber,
                BillingFrequency = cust.billing_frequency,
                TypeOfBilling = cust.TypeofBilling,
                BillingMethod = cust.BillingMethod,
                Premium = cust.Amount,
                PolicyType = policyType,
                Status = policyStatus,
                TGSIStatus = TGSIStatus.Active.ToString(), // to be confirmed
                EffectiveDate = Convert.ToDateTime(cust.EftDate),
                ExpirationDate = expirationDate,
                CancellationDate = DateTime.Now, // to be implemented
                Term = term,
                PriorTerm = priorTerm,
                PriorPolicyNumber = "",
                NextPolicyNumber = "",
            };
            return policy;
        }

        private static string GetPriorTerm(SwyfftRawData cust)
        {
            string priorTerm;
            // to check whether policy number ends with 00, 01, 02 or 03
            if (cust.PolicyNumber.EndsWith("00"))
            {
                priorTerm = string.Empty;
            }
            else if (cust.PolicyNumber.EndsWith("01"))
            {
                priorTerm = PolicyTerm.Policy_0.ToString().Replace('_', ' ');

            }
            else if (cust.PolicyNumber.EndsWith("02"))
            {
                priorTerm = PolicyTerm.Policy_1.ToString().Replace('_', ' ');
            }
            else
            {
                priorTerm = PolicyTerm.Policy_2.ToString().Replace('_', ' ');
            }

            return priorTerm;
        }

        private static void GetInvoiceDetails(SwyfftRawData cust, ref string policyType, ref string city, ref string street, ref string zipCode, ref DateTime expirationDate, List<string> invoiceDetails)
        {
            if (invoiceDetails != null)
            {
                var policyPeriodData = invoiceDetails.FirstOrDefault(x => x.Trim().ToLower().StartsWith("policy period"));

                if (policyPeriodData != null)
                {
                    var policyPeriod = policyPeriodData.Split(':')[1].Trim();

                    if (!string.IsNullOrEmpty(policyPeriod))
                    {
                        try
                        {
                            cust.EftDate = Convert.ToDateTime(policyPeriod.Split('-')[0].Trim());
                            expirationDate = Convert.ToDateTime(policyPeriod.Split('-')[1].Trim());
                        }
                        catch (Exception)
                        {
                            cust.EftDate = ConvertDate(policyPeriod.Split('-')[0].Trim());
                            expirationDate = ConvertDate(policyPeriod.Split('-')[1].Trim());
                        }                        
                    }
                }

                var policyTypeData = invoiceDetails.FirstOrDefault(x => x.Trim().ToLower().StartsWith("policy type"));

                if (policyTypeData != null)
                {
                    policyType = policyTypeData.Split(':')[1].Trim();
                    if (policyType.ToLower().Equals("new"))
                    {
                        policyType = "New Business";
                    }
                }

                var locationData = invoiceDetails.FirstOrDefault(x => x.Trim().ToLower().StartsWith("location"));

                if (locationData != null)
                {
                    var locationDetails = locationData.Split(':');

                    if (locationDetails.Length >= 1)
                    {
                        street = locationDetails[1].Split(',')[0].Trim();
                        city = locationDetails[1].Split(',')[1].Trim();
                    }

                    if (locationDetails.Length >= 2)
                    {
                        zipCode = locationDetails[1].Split(',')[2].Trim();
                    }
                }
            }
        }

        private static DateTime ConvertDate(string strDate)
        {
            string[] lstDate = strDate.Split('/');
            if (lstDate.Count() < 3)
            {
                lstDate = strDate.Split('-');
            }
            return DateTime.Parse(string.Concat(lstDate[1],"/",lstDate[0],"/" ,lstDate[2]));
        }

        //Code to connect to sales force via WSDl
        private static void SaveSalesforceDetails(PolicyModel policy)
        {
            //string salesForceUserName = ConfigurationManager.AppSettings["SalesForceUserName"].ToString();
            //string salesForcePsw = ConfigurationManager.AppSettings["SalesForcePsw"].ToString();
            //string salesForceToken = ConfigurationManager.AppSettings["SalesForceToken"].ToString();

            //SforceService sfdcBinding = new SforceService();
            //LoginResult currentLoginResult = null;
            //currentLoginResult = sfdcBinding.login(salesForceUserName, salesForcePsw + salesForceToken);

            //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            //sfdcBinding.Url = currentLoginResult.serverUrl;
            //sfdcBinding.SessionHeaderValue = new SessionHeader
            //{
            //    sessionId = currentLoginResult.sessionId
            //};

            //var account = new com.salesforce.enterprise.Account
            //{
            //    //Name
            //    Name = policy.Account.Name
            //};

            ////Billing Address // Billing Country
            //account.BillingAddress.city = policy.Property.City;
            //account.BillingAddress.country = "country"; // Not given in the PDF
            //account.BillingAddress.postalCode = policy.Property.Zipcode;
            //account.BillingAddress.state = policy.Property.State;
            //account.RecordType.Name = policy.RecordType; // To be confirm

            //var contact = new com.salesforce.enterprise.Contact
            //{
            //    Name = policy.Contact.Name
            //};
            //contact.MailingAddress.city = policy.Property.City;
            ////contact.MailingAddress.country = "country"; // Not given in the PDF
            //contact.MailingAddress.postalCode = policy.Property.Zipcode;
            //contact.MailingAddress.state = policy.Property.State;

            //var tGS_Property__C = new TGS_Property__c
            //{
            //    TGS_Account__r = account,
            //    TGS_Property_Address_2__c = account.BillingAddress.ToString(),
            //    TGS_City__c = policy.Property.City,
            //    TGS_State__c = policy.Property.State,
            //    TGS_Zip_Code__c = policy.Property.Zipcode
            //};

            ////Policy
            ////Carrier
            //var tGS_Carrier__C = new TGS_Carrier__c();

            ////Carrier Product
            //var tGS_CarrierProduct__C = new TGS_CarrierProduct__c();

            //var tGS_Quote_Policy__C = new com.salesforce.enterprise.TGS_Quote_Policy__c
            //{
            //    TGS_Account__r = account,
            //    TGS_Contact__r = contact,
            //    TGS_Property__r = tGS_Property__C,
            //    TGS_Carrier__r = tGS_Carrier__C,
            //    TGS_Carrier_Product__r = tGS_CarrierProduct__C
            //};

            //tGS_Quote_Policy__C.RecordType.Name = policy.RecordType;
            //tGS_Quote_Policy__C.TGS_Product_Type__c = "Product Type (Homeowners)";
            //tGS_Quote_Policy__C.TGS_Policy_Number__c = policy.PolicyNumber;
            //tGS_Quote_Policy__C.TGS_Billing_Frequency__c = policy.BillingFrequency;
            //tGS_Quote_Policy__C.Type_of_Billing__c = policy.TypeOfBilling;
            //tGS_Quote_Policy__C.TGS_Billing_Method__c = policy.BillingMethod;

            ////need to confirm which salesforce object need to be used for Premium
            //tGS_Quote_Policy__C.TGS_Net_Premium__c = Convert.ToDouble(policy.Premium); //Premium ?
            //tGS_Quote_Policy__C.TGS_Total_Billable_Premium__c = Convert.ToDouble(policy.Premium); //Premium ?
            //tGS_Quote_Policy__C.TGS_Annualized_Premium__c = Convert.ToDouble(policy.Premium); //Premium ?
            //tGS_Quote_Policy__C.TGS_Total_Commissionable_Premium__c = Convert.ToDouble(policy.Premium); //Premium ?

            //tGS_Quote_Policy__C.TGS_Policy_Type__c = policy.PolicyType;
            //tGS_Quote_Policy__C.TGS_Policy_Status__c = policy.Status;
            //tGS_Quote_Policy__C.TGS_TGSI_Status__c = policy.TGSIStatus;

            
            //tGS_Quote_Policy__C.TGS_Effective_Date__c = policy.EffectiveDate;

            //tGS_Quote_Policy__C.TGS_Expiration_Date__c = policy.ExpirationDate;

            //tGS_Quote_Policy__C.TGS_Cancellation_Date__c = policy.CancellationDate;

            //tGS_Quote_Policy__C.TGS_Next_Term__c = policy.Term; ///// need to confirm the salesforce object name ?
            //tGS_Quote_Policy__C.TGS_RenewalTerm__c = policy.Term; ///// need to confirm the salesforce object name ?

            //tGS_Quote_Policy__C.TGS_Prior_PolicyNumber__c = "Prior Policy Number";
            //tGS_Quote_Policy__C.TGS_Bundle_PolicyNumber__c = "Next Policy Number"; ///// need to confirm the salesforce object name ?

            //SaveResult[] insertResults = sfdcBinding.create(new sObject[] { tGS_Quote_Policy__C });

            //for (int i = 0; i < insertResults.Length; i++)
            //{
            //    if (insertResults[i].success)
            //    {
            //        Console.WriteLine(string.Concat("Successfully created ID: ", insertResults[i].id));
            //    }
            //    else
            //    {
            //        Console.WriteLine(string.Concat("Error: could not create sobject for array element ", i, "."));
            //        Console.WriteLine(string.Concat("The error reported was: ", insertResults[i].errors[0].message, "\n"));
            //    }
            //}

            ////Code to update the record by fetching the values from salesforce
            //QueryResult queryResult = null;
            //string SOQL = string.Format("select TGS_Billing_Frequency__c, Type_of_Billing__c from TGS_Quote_Policy__c WHERE TGS_Policy_Number__c = '{0}'", "Policy_Number");
            //queryResult = sfdcBinding.query(SOQL);
            ////update

            //if (queryResult.size > 0)
            //{
            //    for (int i = 0; i < queryResult.records.Length; i++)
            //    {
            //        var tGS_Quote_Policy__c_1 = (com.salesforce.enterprise.TGS_Quote_Policy__c)queryResult.records[i];
            //        tGS_Quote_Policy__c_1.TGS_Account__r = account;
            //        tGS_Quote_Policy__c_1.TGS_Billing_Frequency__c = "";
            //        tGS_Quote_Policy__c_1.Type_of_Billing__c = "";
            //        SaveResult[] UpdateResults = sfdcBinding.update(new sObject[] { tGS_Quote_Policy__c_1 });

            //        for (int j = 0; j < UpdateResults.Length; i++)
            //        {
            //            if (insertResults[i].success)
            //            {
            //                Console.WriteLine(string.Concat("Successfully Updated ID: ", UpdateResults[j].id));
            //            }
            //            else
            //            {
            //                Console.WriteLine(string.Concat("Error: could not create sobject for array element ", j, "."));
            //                Console.WriteLine(string.Concat("The error reported was: ", UpdateResults[j].errors[0].message, "\n"));
            //            }
            //        }
            //    }
            //}
        }

        #endregion

        #region This function is used to updating an Existing Policy.
        public void ScanEndoredTab(IWebDriver driver)
        {
            try
            {
                Thread.Sleep(3000);

                IJavaScriptExecutor js11 = (IJavaScriptExecutor)driver;
                js11.ExecuteScript("arguments[0].click()", driver.FindElement(By.XPath("/html/body/div[2]/section/main/div/div[1]/div[2]/div[3]/ul/li[3]/a")));

                Thread.Sleep(5000);

                //Log.Write("It is main program ---" + adr.RecID);
                var table = driver.FindElement(By.TagName("table"));
                Thread.Sleep(1000);
                
                var rowsFull = table.FindElements(By.TagName("tr"));
                var rows = rowsFull.Where(s => s.Text.Contains(string.Concat(DateTime.Now.Month, "/", DateTime.Now.Day, "/", DateTime.Now.Year)));
                //var rows = rowsFull.Where(s => s.Text.Contains("2/15/2021"));
                if (rows.Count() < 1)
                {
                    string strDate = DateTime.Now.Month < 10 ? "0" + DateTime.Now.Month.ToString() : DateTime.Now.Month.ToString();
                    strDate += DateTime.Now.Day < 10 ? "/0" + DateTime.Now.Day.ToString() : "/" + DateTime.Now.Day.ToString();
                    strDate += DateTime.Now.Year.ToString();

                    rows = rowsFull.Where(s => s.Text.Contains(strDate));
                }

                //Thread.Sleep(2000);
                Iswyfft si = new Policy();

                foreach (var row in rows)
                {
                    var rowTds = row.FindElements(By.TagName("td"));
                    Thread.Sleep(1000);
                    bool flag = false;
                    int n = 0;
                    var ts = "";
                    SwyfftRawData cust = new SwyfftRawData();

                    foreach (var td in rowTds)
                    {
                        if (n == 0)
                        {
                            var status = td.FindElement(By.CssSelector(".glyphicon"));
                            ts = status.GetAttribute("title");
                            cust.Address = td.Text.ToString();
                            cust.Address = cust.Address.Replace("\r\n", " ");
                        }
                        if (n == 1)
                        {
                            cust.Amount = Convert.ToDecimal(td.Text.ToString().Replace("$", ""));
                        }
                        if (n == 2)
                        {
                            var paymentType = td.FindElement(By.TagName("a")).Text.ToString().Trim();

                            if (paymentType.ToLower().Contains("monthly"))
                            {
                                cust.billing_frequency = "12-Pay";
                            }

                            if (paymentType.ToLower().Contains("annual"))
                            {
                                cust.billing_frequency = "1-Pay";
                            }

                            if (paymentType.ToLower().Contains("echeck"))
                            {
                                cust.BillingMethod = "Electronic Funds Transfer (EFT)";
                            }

                            if (paymentType.ToLower().Contains("credit card"))
                            {
                                cust.BillingMethod = "Credit Card (CC)";
                            }

                            if (paymentType.ToLower().Contains("escrow"))
                            {
                                cust.billing_frequency = "1-Pay";
                                cust.BillingMethod = "Mortgagee Bill";
                            }

                            cust.TypeofBilling = "Direct Bill - Automatic";

                            if (cust.billing_frequency == null || cust.BillingMethod == null || cust.TypeofBilling == null)
                            {
                                Utility.SendMail("Swyfft Import Error", cust.CustName + "\n --> New Payment Type - " + paymentType + "");
                                break;
                            }
                        }

                        if (n == 3)
                        {
                            cust.CustName = td.Text.ToString();
                        }
                        if (n == 4)
                        {

                            DateTime dateValue;

                            CultureInfo enUS = new CultureInfo("en-US");

                            string[] strDate = td.Text.ToString().Split('/');

                            DateTime.TryParse(string.Concat(strDate[1], "/", strDate[0], "/", strDate[2]), out dateValue);
                                
                            cust.EftDate = dateValue;
                        }
                        if (n == 5)
                        {
                            cust.PolicyNumber = td.Text.ToString();
                        }
                        if (n == 7)
                        {
                            //check pdf to delete if pdf file still exist
                            si.deletefiles();
                            var aval = td.FindElements(By.CssSelector(".action-wrapper-bs.file-available-bs"));
                            var aval1 = td.FindElements(By.CssSelector(".action-wrapper-bs"));
                            var a = td.FindElements(By.TagName("a"));
                            if (a.Count == 1)
                            {
                                foreach (var suba in a)
                                {
                                    Thread.Sleep(1000);
                                    suba.Click();
                                    Thread.Sleep(3000);

                                    try
                                    {

                                        IWebElement modal;
                                        try
                                        {
                                            modal = driver.FindElement(By.XPath("/html/body/div[112]/div[2]"));
                                        }
                                        catch (Exception e)
                                        {
                                            try
                                            {
                                                modal = driver.FindElement(By.XPath("html/body/div[114]/div[2]")); /// html / body / div[108] / div[2]
                                            }
                                            catch (Exception ex)
                                            {
                                                try
                                                {
                                                    modal = driver.FindElement(By.XPath("html/body/div[115]/div[2]")); // 

                                                }
                                                catch (Exception ex1)
                                                {
                                                    try
                                                    {
                                                        modal = driver.FindElement(By.XPath("html/body/div[108]/div[2]"));
                                                    }
                                                    catch (Exception ex2)
                                                    {
                                                        try
                                                        {
                                                            modal = driver.FindElement(By.XPath("html/body/div[109]/div[2]"));
                                                        }
                                                        catch (Exception ex3)
                                                        {
                                                            modal = driver.FindElement(By.ClassName("modal"));
                                                        }

                                                    }
                                                }
                                            }
                                        }

                                        if (modal.GetCssValue("display") == "block")
                                        {
                                            var policyOverview = modal.FindElements(By.ClassName("modal-body"));
                                            if (policyOverview.Count > 2)
                                            {
                                                var pdf = policyOverview[2].FindElements(By.ClassName("policy-download-link-bs"));

                                                if (pdf.Count > 0)
                                                {

                                                    foreach (var link in pdf)
                                                    {
                                                        try
                                                        {
                                                            if (link.Text.Contains("Download Receipt") || link.Text.Contains("Download Invoice") || link.Text.Contains("Download Receipt"))
                                                            {
                                                                link.Click();
                                                                flag = true;
                                                                Thread.Sleep(3000);
                                                            }
                                                        }
                                                        catch (Exception ex)
                                                        {

                                                        }

                                                    }
                                                }
                                                modal.FindElement(By.ClassName("x")).Click();
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Utility.SendMail("Swyfft Import Error", cust.CustName + " --> " + ex + "");
                                    }
                                }
                            }
                        }
                        n++;
                    }

                    if (flag)
                    {

                        var lst = si.readfiles();

                        foreach (var item in lst)
                        {
                            if (item.IndexOf("Invoice") >= 0)
                            {
                                string pathfile = ConfigurationManager.AppSettings.Get("sourcePath").ToString() + "\\" + item;
                                cust.Invoice = item;
                                cust.InvoiceText = PdfParse.ExtractTextFromPdf(pathfile);
                            }

                            if (item.IndexOf("Payment") >= 0)
                            {
                                cust.Payment = item.Replace(".pdf", "-SU.pdf");
                                string pathfile = ConfigurationManager.AppSettings.Get("sourcePath").ToString() + "\\" + item;

                                string newStatusText = PdfParse.ExtractTextFromPdf(pathfile);

                                string[] newPolicyStatus = newStatusText.Split('\n');

                                var details = newPolicyStatus[0].Trim('{', '}')
                                     .Split(',')
                                     .Select(s => s.Trim().Split(':'))
                                     .ToDictionary(a => a[0], a => a[1]);

                                cust.StatusText = details["Policy Status"];

                                if (!cust.StatusText.Trim().ToLower().Contains(newPolicyStatus[0].Trim().ToLower()))
                                {
                                    cust.StatusText = newStatusText.Trim();
                                    cust.StatusModifiedDate = DateTime.Now;
                                }
                            }
                        }

                        if (cust.UpdatedStatusText == null)
                        {
                            cust.UpdatedStatusText = " ";
                        }

                        if (cust.StatusModifiedDate.Value.Date == DateTime.Now.Date)
                        {
                            //insert data in salesforce
                            string term = string.Empty, priorTerm = string.Empty, policyType = string.Empty, city = string.Empty, state = string.Empty, street = string.Empty, zipCode = string.Empty;
                            DateTime expirationDate = new DateTime();

                            var invoiceDetails = cust.InvoiceText.Split('\n').ToList();

                            GetInvoiceDetails(cust, ref policyType, ref city,ref street, ref zipCode, ref expirationDate, invoiceDetails);

                            // to be confirmed
                            if (cust.billing_frequency == "1-Pay")
                            {
                                term = "12 Month";
                            }
                            else if (cust.billing_frequency == "12-Pay")
                            {
                                term = "1 Month";
                            }

                            priorTerm = GetPriorTerm(cust);

                            var policyStatus = cust.StatusText.ToLower().Trim().Contains("issued") ? Status.Issued.ToString() : "";
                            policyStatus = cust.StatusText.ToLower().Trim().Contains("cancelled") ? Status.Cancelled.ToString() : "";
                            

                            var policy = GetPolicyDetails(cust, term, priorTerm, policyType, city, state, street, zipCode, expirationDate, policyStatus);

                            // Update record
                            if (policyStatus == Status.Cancelled.ToString())
                            {
                                policy.TGSIStatus = TGSIStatus.Cancel.ToString();
                                policy.CancellationDate = DateTime.Now;
                            }

                            // TGSIStatus TO DO (Cancel)

                            ConnectSalesforceDetails(policy);

                            //delete file from temporary folder
                            si.deletefiles();
                        }
                    }

                    flag = false;
                }
            }
            catch (Exception ex)
            {
                Utility.SendMail("Swyfft Import Error ScanEndoredTab", ex + "");
                Log.Write("It is Error as ---" + ex.Message + "  | " + ex.InnerException + " |  " + ex.StackTrace + " |  " + ex);
            }
        }
        #endregion
    }
}

//switch (paymentType.ToLower().Trim())
//{    
//    case "escrow":
//        cust.billing_frequency = "1-Pay";
//        cust.BillingMethod = "Mortgagee Bill";
//        cust.TypeofBilling = "Direct Bill - Automatic";
//        break;
//    case "monthly echeck":
//        cust.billing_frequency = "12-Pay";
//        cust.BillingMethod = "Electronic Funds Transfer (EFT)";
//        cust.TypeofBilling = "Direct Bill - Automatic";
//        break;
//    case "annual credit card":
//        cust.billing_frequency = "1-Pay";
//        cust.BillingMethod = "Credit Card (CC)";
//        cust.TypeofBilling = "Direct Bill - Automatic";
//        break;
//    case "monthly credit card":
//        cust.billing_frequency = "12-Pay";
//        cust.BillingMethod = "Credit Card (CC)";
//        cust.TypeofBilling = "Direct Bill - Automatic";
//        break;
//    case "annual echeck":
//        cust.billing_frequency = "1-Pay";
//        cust.BillingMethod = "Electronic Funds Transfer (EFT)";
//        cust.TypeofBilling = "Direct Bill - Automatic";
//        break;
//    case "monthly\r\necheck":
//        cust.billing_frequency = "12-Pay";
//        cust.BillingMethod = "Electronic Funds Transfer (EFT)";
//        cust.TypeofBilling = "Direct Bill - Automatic";
//        break;
//    case "annual\r\ncredit card":
//        cust.billing_frequency = "1-Pay";
//        cust.BillingMethod = "Credit Card (CC)";
//        cust.TypeofBilling = "Direct Bill - Automatic";
//        break;
//    case "monthly\r\ncredit card":
//        cust.billing_frequency = "12-Pay";
//        cust.BillingMethod = "Credit Card (CC)";
//        cust.TypeofBilling = "Direct Bill - Automatic";
//        break;
//    case "annual\r\necheck":
//        cust.billing_frequency = "1-Pay";
//        cust.BillingMethod = "Electronic Funds Transfer (EFT)";
//        cust.TypeofBilling = "Direct Bill - Automatic";
//        break;
//    case "escrow - renewal\r\ncredit card":
//        cust.billing_frequency = "1-Pay";
//        cust.BillingMethod = "Electronic Funds Transfer (EFT)";
//        cust.TypeofBilling = "Direct Bill - Automatic";
//        break;
//    case "monthly - renewal\r\necheck":
//        cust.billing_frequency = "1-Pay";
//        cust.BillingMethod = "Electronic Funds Transfer (EFT)";
//        cust.TypeofBilling = "Direct Bill - Automatic";
//        break;
//    case "monthly - renewal\r\ncredit card":
//        cust.billing_frequency = "1-Pay";
//        cust.BillingMethod = "Electronic Funds Transfer (EFT)";
//        cust.TypeofBilling = "Direct Bill - Automatic";
//        break;
//}
