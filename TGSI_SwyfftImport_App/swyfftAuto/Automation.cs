﻿using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using swyfftAuto.com.salesforce.enterprise;
using swyfftAuto.Model;
using swyfftAuto.Model.Enums;
using swyfftAuto.Model.SalesForceModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading;

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
                var rows = table.FindElements(By.TagName("tr"));
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
                            string priorTerm = string.Empty, policyType = string.Empty, city = string.Empty, state = string.Empty, zipCode = string.Empty;
                            DateTime expirationDate = new DateTime();

                            var invoiceDetails = cust.InvoiceText.Split('\n').ToList();

                            GetInvoiceDetails(cust, ref policyType, ref city, ref zipCode, ref expirationDate, invoiceDetails);

                            if (cust.billing_frequency == "1-Pay")
                            {
                                term = "12 Month";
                            }
                            else if (cust.billing_frequency == "12-Pay")
                            {
                                term = "1 Month";
                            }

                            priorTerm = GetPriorTerm(cust);

                            //insert record in salesforce
                            if (policyType.Trim().ToLower() == PolicyType.New.ToString().ToLower())
                            {
                                GetSalesforceDetails(cust, term, priorTerm, expirationDate, city, state, zipCode);
                            }

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

        private static void GetInvoiceDetails(SwyfftRawData cust, ref string policyType, ref string city, ref string zipCode, ref DateTime expirationDate, List<string> invoiceDetails)
        {
            if (invoiceDetails != null)
            {
                var policyPeriodData = invoiceDetails.FirstOrDefault(x => x.Trim().ToLower().StartsWith("policy period"));

                if (policyPeriodData != null)
                {
                    var policyPeriod = policyPeriodData.Split(':')[1].Trim();

                    if (!string.IsNullOrEmpty(policyPeriod))
                    {
                        cust.EftDate = Convert.ToDateTime(policyPeriod.Split('-')[0].Trim());
                        expirationDate = Convert.ToDateTime(policyPeriod.Split('-')[1].Trim());
                    }
                }

                var policyTypeData = invoiceDetails.FirstOrDefault(x => x.Trim().ToLower().StartsWith("policy type"));

                if (policyTypeData != null)
                {
                    policyType = policyTypeData.Split(':')[1].Trim();
                }

                var locationData = invoiceDetails.FirstOrDefault(x => x.Trim().ToLower().StartsWith("location"));

                if (locationData != null)
                {
                    var locationDetails = locationData.Split(':');

                    if (locationDetails.Length >= 1)
                    {
                        city = locationDetails[1].Split(',')[1].Trim();
                    }

                    if (locationDetails.Length >= 2)
                    {
                        zipCode = locationDetails[1].Split(',')[2].Trim();
                    }
                }
            }
        }

        private static void GetSalesforceDetails(SwyfftRawData cust, string term, string priorTerm, DateTime expirationDate, string city, string state, string zipCode)
        {
            string salesForceUserName = ConfigurationManager.AppSettings["SalesForceUserName"].ToString();
            string salesForcePsw = ConfigurationManager.AppSettings["SalesForcePsw"].ToString();
            string salesForceToken = ConfigurationManager.AppSettings["SalesForceToken"].ToString();

            SforceService sfdcBinding = new SforceService();
            LoginResult currentLoginResult = null;
            currentLoginResult = sfdcBinding.login(salesForceUserName, salesForcePsw + salesForceToken);

            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            sfdcBinding.Url = currentLoginResult.serverUrl;
            sfdcBinding.SessionHeaderValue = new SessionHeader
            {
                sessionId = currentLoginResult.sessionId
            };

            var account = new com.salesforce.enterprise.Account
            {
                //Name
                Name = "SAP111111"
            };

            //Billing Address // Billing Country
            account.BillingAddress.city = "city";
            account.BillingAddress.country = "country";
            account.BillingAddress.countryCode = "countryCode";
            account.BillingAddress.postalCode = "";
            account.BillingAddress.state = "xxx";
            account.BillingAddress.stateCode = "xxx";
            account.BillingAddress.street = "xxx";
            account.RecordType.Name = "RecordType";

            var contact = new com.salesforce.enterprise.Contact
            {
                Name = "name"
            };
            contact.MailingAddress.city = "city";
            contact.MailingAddress.country = "country";
            contact.MailingAddress.countryCode = "countryCode";
            contact.MailingAddress.postalCode = "postalCode";
            contact.MailingAddress.state = "state";
            contact.MailingAddress.stateCode = "stateCode";
            contact.MailingAddress.street = "street";

            var tGS_Property__C = new TGS_Property__c
            {
                TGS_Account__r = account,
                TGS_Property_Address_2__c = "address",
                TGS_City__c = "City",
                TGS_State__c = "State",
                TGS_Zip_Code__c = "Zip_Code"
            };

            //Policy
            //Carrier
            var tGS_Carrier__C = new TGS_Carrier__c();

            //Carrier Product
            var tGS_CarrierProduct__C = new TGS_CarrierProduct__c();

            var tGS_Quote_Policy__C = new com.salesforce.enterprise.TGS_Quote_Policy__c
            {
                TGS_Account__r = account,
                TGS_Contact__r = contact,
                TGS_Property__r = tGS_Property__C,
                TGS_Carrier__r = tGS_Carrier__C,
                TGS_Carrier_Product__r = tGS_CarrierProduct__C
            };

            tGS_Quote_Policy__C.RecordType.Name = "RecorType name";
            tGS_Quote_Policy__C.TGS_Product_Type__c = "Product Type (Homeowners)";
            tGS_Quote_Policy__C.TGS_Policy_Number__c = "Policy_Number";
            tGS_Quote_Policy__C.TGS_Billing_Frequency__c = "Billing frequency";
            tGS_Quote_Policy__C.Type_of_Billing__c = "Type of Billing";
            tGS_Quote_Policy__C.TGS_Billing_Method__c = "Billing Method";

            //need to confirm which salesforce object need to be used for Premium
            tGS_Quote_Policy__C.TGS_Net_Premium__c = 1000.00; //Premium ?
            tGS_Quote_Policy__C.TGS_Total_Billable_Premium__c = 1000.00; //Premium ?
            tGS_Quote_Policy__C.TGS_Annualized_Premium__c = 1000.00; //Premium ?
            tGS_Quote_Policy__C.TGS_Total_Commissionable_Premium__c = 1000.00; //Premium ?

            tGS_Quote_Policy__C.TGS_Policy_Type__c = "Policy Type";
            tGS_Quote_Policy__C.TGS_Policy_Status__c = "Status";
            tGS_Quote_Policy__C.TGS_TGSI_Status__c = "TGSI Status";

            DateTime effectiveDate = new DateTime();
            DateTime.TryParse("27/02/2021", out effectiveDate);
            tGS_Quote_Policy__C.TGS_Effective_Date__c = effectiveDate;

            DateTime expDate = new DateTime();
            DateTime.TryParse("27/02/2021", out expDate);
            tGS_Quote_Policy__C.TGS_Expiration_Date__c = expDate;

            DateTime cancellationDate = new DateTime();
            DateTime.TryParse("27/02/2021", out cancellationDate);
            tGS_Quote_Policy__C.TGS_Cancellation_Date__c = cancellationDate;

            tGS_Quote_Policy__C.TGS_Next_Term__c = "Term"; ///// need to confirm the salesforce object name ?
            tGS_Quote_Policy__C.TGS_RenewalTerm__c = "Prior Term"; ///// need to confirm the salesforce object name ?

            tGS_Quote_Policy__C.TGS_Prior_PolicyNumber__c = "Prior Policy Number";
            tGS_Quote_Policy__C.TGS_Bundle_PolicyNumber__c = "Next Policy Number"; ///// need to confirm the salesforce object name ?

            SaveResult[] insertResults = sfdcBinding.create(new sObject[] { tGS_Quote_Policy__C });

            for (int i = 0; i < insertResults.Length; i++)
            {
                if (insertResults[i].success)
                {
                    Console.WriteLine(string.Concat("Successfully created ID: ", insertResults[i].id));
                }
                else
                {
                    Console.WriteLine(string.Concat("Error: could not create sobject for array element ", i, "."));
                    Console.WriteLine(string.Concat("The error reported was: ", insertResults[i].errors[0].message, "\n"));
                }
            }

            //Code to update the record by fetching the values from salesforce
            QueryResult queryResult = null;
            string SOQL = string.Format("select TGS_Billing_Frequency__c, Type_of_Billing__c from TGS_Quote_Policy__c WHERE TGS_Policy_Number__c = '{0}'", "Policy_Number");
            queryResult = sfdcBinding.query(SOQL);
            //update

            if (queryResult.size > 0)
            {
                for (int i = 0; i < queryResult.records.Length; i++)
                {
                    var tGS_Quote_Policy__c_1 = (com.salesforce.enterprise.TGS_Quote_Policy__c)queryResult.records[i];
                    tGS_Quote_Policy__c_1.TGS_Account__r = account;
                    tGS_Quote_Policy__c_1.TGS_Billing_Frequency__c = "";
                    tGS_Quote_Policy__c_1.Type_of_Billing__c = "";
                    SaveResult[] UpdateResults = sfdcBinding.update(new sObject[] { tGS_Quote_Policy__c_1 });

                    for (int j = 0; j < UpdateResults.Length; i++)
                    {
                        if (insertResults[i].success)
                        {
                            Console.WriteLine(string.Concat("Successfully Updated ID: ", UpdateResults[j].id));
                        }
                        else
                        {
                            Console.WriteLine(string.Concat("Error: could not create sobject for array element ", j, "."));
                            Console.WriteLine(string.Concat("The error reported was: ", UpdateResults[j].errors[0].message, "\n"));
                        }
                    }
                }
            }

            //Account account = new Account()
            //{
            //    Name = cust.CustName,
            //    BillingAddress = cust.Address,
            //    BillingCountry = null,
            //    RecordType = RecordType.Person.ToString()
            //};

            //Property property = new Property()
            //{
            //    Account = cust.CustName,
            //    Address = cust.Address,
            //    City = city,
            //    State = state,
            //    Zipcode = zipCode
            //};

            //Contact contact = new Contact()
            //{
            //    Name = cust.CustName,
            //    MailingAddress = null,
            //    MailingAddressWithCountry = null,
            //    RecordType = RecordType.Insured.ToString()
            //};

            //PolicyModel policy = new PolicyModel()
            //{
            //    Account = cust.CustName,
            //    Contact = null,
            //    Property = null,
            //    Carrier = "Swyfft",
            //    CarrierProduct = "Swyfft-Home",
            //    RecordType = RecordType.Policy.ToString(),
            //    ProductType = null,
            //    PolicyNumber = cust.PolicyNumber,
            //    BillingFrequency = cust.billing_frequency,
            //    TypeOfBilling = cust.TypeofBilling,
            //    BillingMethod = cust.BillingMethod,
            //    Premium = cust.Amount,
            //    PolicyType = PolicyType.New.ToString(),
            //    Status = Status.Issued.ToString(),  // to be confirmed
            //    TGSIStatus = TGSIStatus.Active.ToString(), // to be confirmed
            //    EffectiveDate = Convert.ToDateTime(cust.EftDate),
            //    ExpirationDate = expirationDate,
            //    CancellationDate = DateTime.Now, // to be implemented
            //    Term = term,
            //    PriorTerm = priorTerm,
            //    PriorPolicyNumber = cust.PolicyNumber, // to be confirmed
            //    NextPolicyNumber = null,
            //};
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
                var rows = table.FindElements(By.TagName("tr"));
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
                            cust.EftDate = Convert.ToDateTime(td.Text.ToString());
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
                            string term = string.Empty, priorTerm = string.Empty, policyType = string.Empty, city = string.Empty, state = string.Empty, zipCode = string.Empty;
                            DateTime expirationDate = new DateTime();

                            var invoiceDetails = cust.InvoiceText.Split('\n').ToList();

                            GetInvoiceDetails(cust, ref policyType, ref city, ref zipCode, ref expirationDate, invoiceDetails);

                            PolicyModel policy = new PolicyModel();

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

                            if (policyType.Trim().ToLower() == PolicyType.Renewal.ToString().ToLower())
                            {
                                if (cust.StatusText.ToLower() == Status.Cancelled.ToString().ToLower())
                                {
                                    policy.TGSIStatus = TGSIStatus.Cancel.ToString();
                                    policy.CancellationDate = DateTime.Now;
                                }
                                else
                                {
                                    var account = new Model.SalesForceModels.Account()
                                    {
                                        Name = cust.CustName,
                                        BillingAddress = cust.Address,
                                        BillingCountry = null,
                                        RecordType = Model.Enums.RecordType.Person.ToString()
                                    };

                                    Property property = new Property()
                                    {
                                        Account = cust.CustName,
                                        Address = cust.Address,
                                        City = city,
                                        State = state,
                                        Zipcode = zipCode
                                    };

                                    var contact = new Model.SalesForceModels.Contact()
                                    {
                                        Name = cust.CustName,
                                        MailingAddress = null,
                                        MailingAddressWithCountry = null,
                                        RecordType = Model.Enums.RecordType.Insured.ToString()
                                    };

                                    policy.Account = cust.CustName;
                                    policy.Contact = null;
                                    policy.Property = null;
                                    policy.Carrier = "Swyfft";
                                    policy.CarrierProduct = "Swyfft-Home";
                                    policy.RecordType = Model.Enums.RecordType.Policy.ToString();
                                    policy.ProductType = null;
                                    policy.PolicyNumber = cust.PolicyNumber;
                                    policy.BillingFrequency = cust.billing_frequency;
                                    policy.TypeOfBilling = cust.TypeofBilling;
                                    policy.BillingMethod = cust.BillingMethod;
                                    policy.Premium = cust.Amount;
                                    policy.PolicyType = PolicyType.Endorsement.ToString();
                                    policy.Status = Status.Issued.ToString();  // to be confirmed
                                    policy.TGSIStatus = TGSIStatus.Active.ToString(); // to be confirmed
                                    policy.EffectiveDate = Convert.ToDateTime(cust.EftDate);
                                    policy.ExpirationDate = expirationDate;
                                    policy.CancellationDate = DateTime.Now; // to be implemented
                                    policy.Term = term;
                                    policy.PriorTerm = priorTerm;
                                    policy.PriorPolicyNumber = cust.PolicyNumber; // to be confirmed
                                    policy.NextPolicyNumber = null;
                                }
                            }
                            //delete file from temporary folder
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
