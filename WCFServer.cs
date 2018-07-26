using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace HookappServer
{
    public class WCFServer
    {
        public static Database Database;

        private WebServiceHost _host;
        private ServiceEndpoint _ep;
        private ServiceDebugBehavior _stp;
        public ILog Logger;

        public CommunicationState GetState()
        {
            return _host.State;
        }

        public void Start()
        {
            _host = new WebServiceHost(typeof(WCFEntryPoint), new Uri("http://localhost:9000"));
            _ep = _host.AddServiceEndpoint(typeof(IWCFEntryPoint), new WebHttpBinding(), "");
            _stp = _host.Description.Behaviors.Find<ServiceDebugBehavior>();
            _stp.HttpHelpPageEnabled = false;
            _host.Open();

            while (_host.State != CommunicationState.Opened)
            {
                Print.Info("Waiting for server to start up");
                Thread.Sleep(1000);
            }
        }

        public void EmptyDatabase()
        {
            //Database.EmptyDatabase();
        }

        public void Stop()
        {
            _host.Close();
        }

        public bool Setup()
        {
            Database = new Database();

            if (!Database.Connect())
            {
                Console.WriteLine("Database failed to start");
                return false;
            }
            else
                Console.WriteLine("Database up and running!");

            Console.WriteLine("Setting up Triwio");
            TwilioClient.Init(
                    Consts.TWILIO_ACCOUNT_SID,
                    Consts.TWILIO_AUTH_TOKEN);

            

            Console.WriteLine("Triwio up and running");

            return true;
        }
    }
}
