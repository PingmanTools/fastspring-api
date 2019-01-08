using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Serialization;

namespace FastSpringApi.Models
{
   [XmlRoot("subscription")]
   public class Subscription : FastSpringObject
   {
      [XmlElement("status")]
      public SubscriptionStatus Status { get; set; }

      [XmlElement("statusChanged")]
      public DateTime StatusChanged { get; set; }

      [XmlElement("statusReason")]
      public SubscriptionStatusReason? StatusReason { get; set; }

      [XmlElement("cancelable")]
      public bool IsCancelable { get; set; }

      [XmlElement("reference")]
      public string Reference { get; set; }

      [XmlElement("test")]
      public bool IsTest { get; set; }

      [XmlElement("referrer")]
      public string Referer { get; set; }

      [XmlElement("sourceName")]
      public string SourceName { get; set; }

      [XmlElement("sourceKey")]
      public string SourceKey { get; set; }

      [XmlElement("sourceCampaign")]
      public string SourceCampaign { get; set; }

      [XmlElement("customer")]
      public Customer Customer { get; set; }

      [XmlElement("customerUrl")]
      public string CustomerUrl { get; set; }

      [XmlElement("productName")]
      public string ProductName { get; set; }

      [XmlElement("tags")]
      public TagCollection Tags { get; set; }

      [XmlElement("quantity")]
      public int Quantity { get; set; }

      [XmlElement("nextPeriodDate")]
      public DateTime NextPeriodDate { get; set; }

      [XmlElement("end")]
      public DateTime? End { get; set; }

      /// <summary>
      /// Cancels a subscription at the next period.
      /// </summary>
      public void Cancel()
      {
         Client.Delete("subscription/" + Reference);
      }

      /// <summary>
      /// Reactivates a canceled subscription which has not yet been deactivated.
      /// </summary>
      public void Reactivate()
      {
         Client.Put("subscription/" + Reference, "<subscription><no-end-date/></subscription>");
      }

      /// <summary>
      /// Updates the data for this subscription.
      /// </summary>
      /// <param name="updateData">The data to be submitted</param>
      public void Update(SubscriptionUpdate updateData)
      {
         if (updateData == null)
         {
            throw new ArgumentNullException("updateData");
         }

         var sb = new StringBuilder();

         XmlAttributeOverrides overrides = new XmlAttributeOverrides();
         XmlAttributes attribs = new XmlAttributes();
         attribs.XmlIgnore = true;
         if (!updateData.NoEndDate)
         {
            attribs.XmlElements.Add(new XmlElementAttribute("NoEndDate"));
            attribs.XmlElements.Add(new XmlElementAttribute("no-end-date"));
            overrides.Add(typeof(SubscriptionUpdate), "NoEndDate", attribs);
         }
         var serializer = new XmlSerializer(typeof(SubscriptionUpdate), overrides);
         serializer.Serialize(new StringWriter(sb), updateData);

         Client.Put("subscription/" + Reference, FastSpringClient.CleanUpXml(sb.ToString()));
      }

      /// <summary>
      /// Only for On-Demand subscriptions: Renews the subscription according to the predefined conditions.
      /// </summary>
      public RenewResult Renew()
      {
         try
         {
            Client.Post("subscription/" + Reference + "/renew");
         }
         catch (WebException e)
         {
            int? code = null;

            if (e.Response != null & e.Response is HttpWebResponse)
            {
               code = (int)((HttpWebResponse)e.Response).StatusCode;
            }

            if (code == 503)
            {
               return RenewResult.Unavailable;
            }

            if (code == 422)
            {
               //var s = ((HttpWebResponse) e.Response).StatusDescription;
               //TODO: parse enum value from status description
               return RenewResult.Unknown;
            }
         }

         return RenewResult.Ok;
      }
   }
}
