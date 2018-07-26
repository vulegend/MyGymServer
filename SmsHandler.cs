using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace HookappServer
{
    public class SmsHandler
    {
        private static SmsHandler _instance;
        public static SmsHandler Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SmsHandler();

                return _instance;
            }
        }

        public void SendRegistryConfirmation(string code, string mobileNumber, long userID)
        {
            DateTime validUntil = DateTime.Now.AddMinutes(15);

            //Add to unconfirmed pool before sending an sms
            if(!WCFServer.Database.AddToUnconfirmedPool(code,mobileNumber,validUntil,userID))
            {
                Console.WriteLine("Failed to add to unconfirmed pool, try again!");
                return;
            }

            Console.WriteLine("Added to the unconfirmed pool, sending sms");

            MessageResource.Create(
                to: new PhoneNumber(mobileNumber),
                from: new PhoneNumber(Consts.TWILIO_MOBILE_NUMBER),
                body: code);

          
            Console.WriteLine("Sms sent!");
        }
    }
}
