using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace TrackerLibrary
{
    public class SMSLogic
    {
        public static void SendSMSMessge(string to, string textMessage)
        {
            string accountSid = GlobalConfig.AppKeyLookup("smsAccountSid");
            string authToken = GlobalConfig.AppKeyLookup("smsAuthToken");
            string fromPhoneNumber = GlobalConfig.AppKeyLookup("smsFromPhoneNumber");

            TwilioClient.Init(accountSid, authToken);

            var message = MessageResource.Create(
                to: new PhoneNumber(to),
                from: new PhoneNumber(fromPhoneNumber),
                body: textMessage
            );

            Console.WriteLine(message);
        }
    }

}
