﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using swyfftAuto.com.salesforce.enterprise;

namespace swyfftAuto.Model
{
    class SalesforceModel
    {
        
    }
    //public class Account
    //{
    //    public string Id { get; set; }
    //    public string Name { get; set; }
    //    public string BillingStreet { get; set; }
    //    public string BillingCity { get; set; }
    //    public string BillingState { get; set; }
    //}
    public class TGS_Quote_Policy__c
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string TGS_Policy_Number__c { get; set; }
        public Account TGS_Account__r { get; internal set; }
        public Contact TGS_Contact__r { get; internal set; }
        public TGS_Property__c TGS_Property__r { get; internal set; }
        public RecordType RecordType { get; internal set; }
        public string TGS_Product_Type__c { get; internal set; }
        public string TGS_Billing_Frequency__c { get; internal set; }
        public string Type_of_Billing__c { get; internal set; }
        public string TGS_Billing_Method__c { get; internal set; }
        public double TGS_Net_Premium__c { get; internal set; }
        public double TGS_Total_Billable_Premium__c { get; internal set; }
        public double TGS_Annualized_Premium__c { get; internal set; }
        public double TGS_Total_Commissionable_Premium__c { get; internal set; }
        public string TGS_Policy_Type__c { get; internal set; }
        public string TGS_Policy_Status__c { get; internal set; }
        public string TGS_TGSI_Status__c { get; internal set; }
        public DateTime TGS_Effective_Date__c { get; internal set; }
        public DateTime TGS_Expiration_Date__c { get; internal set; }
        public DateTime TGS_Cancellation_Date__c { get; internal set; }
        public string TGS_Next_Term__c { get; internal set; }
        public string TGS_RenewalTerm__c { get; internal set; }
        public string TGS_Prior_PolicyNumber__c { get; internal set; }
        public string TGS_Bundle_PolicyNumber__c { get; internal set; }
    }
}
