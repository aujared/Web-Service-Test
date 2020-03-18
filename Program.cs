using System;
using Serilog;
using System.Threading;
using System.Threading.Tasks;
using Websocket.Client;
using System.Net.WebSockets;
using Newtonsoft.Json.Linq;
using Destructurama;

namespace AmbientEdge
{
    class Program
    {
        static void Main(string[] args)
        {
            // Configure logging
            var logConfig = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Destructure.JsonNetTypes()
                .WriteTo.Console()
                .WriteTo.File("logs\\myapp.txt", rollingInterval: RollingInterval.Day);

            Log.Logger = logConfig.CreateLogger();
            Log.Information("Starting service");

            // Start program
            Program p = new Program();
            p.TestWSS();
        }
              
        void TestWSS()
        {
            var exitEvent = new ManualResetEvent(false);
            var url = new Uri("wss://ambient-lab-alexa-496.mybluemix.net/ws/edge/"); // Replace with your IBM url

            using (var client = new WebsocketClient(url))
            {
                client.ReconnectTimeout = TimeSpan.FromSeconds(30);
                client.ReconnectionHappened.Subscribe(info =>
                    Log.Information($"Reconnection happened, type: {info.Type}"));

                client.MessageReceived.Subscribe(HandleReceive);
                client.Start();
                Task.Run(() => client.Send("{ message }"));
                exitEvent.WaitOne();
            }
        }

        private void HandleReceive(ResponseMessage msg)
        {
            if (msg.MessageType == WebSocketMessageType.Text)
            {
                var text = msg.Text;
                Log.Information($"Message received: {text}");
                //dynamic m = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(text);
                dynamic m = JObject.Parse(text);
                var messagetype = m.response.msgtype;
                string returnstatement = "";
                var devicename = m.response.devicename;
                var servicename = m.response.servicename;
                Log.Information("Message is of type: {@msgtype}", m.response.msgtype);
                switch(messagetype){
                    case "StartSession":
                        returnstatement = "Hello, Welcome to the Ambient Lab!";
                        break;
                    case "DeviceActivation":
                        returnstatement = "$Activating service {servicename} on device {devicename}";
                        break;
                    case "ListDevices":
                        returnstatement = "The Available Devices are: TV1, TV2, Comp 1";
                        break;
                    case "EndSession":
                        returnstatement = "Thank you for visiting the Ambient Lab, Have a Nice Day";
                        break;
                }
                Log.Information($"Device Name is {devicename} Service Name is {servicename}");
                Log.Information($"Return Statement is: {returnstatement}" );
            }
        }
    }
}
