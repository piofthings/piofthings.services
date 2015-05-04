using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Piofthings.Services;
using Microsoft.AspNet.SignalR.Client;
using System.Diagnostics;

namespace Piofthings.Client
{
    class Program
    {
        static string url = "http://piofthings.dev.local/";

        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "-url")
            {
                if (args.Length >= 2)
                {
                    url = args[1];
                }
            }
            //var piotHubConnection = new HubConnection(url);
            //var piotHubProxy = piotHubConnection.CreateHubProxy("PiotHub");

            //RelayClient service = new RelayClient(piotHubConnection, piotHubProxy);

            //piotHubConnection.Start().ContinueWith(task =>
            //{
            //    service.StartConnection(task);
            //}).Wait();

            TemperatureSensorClient dht11 = new TemperatureSensorClient();
            dht11.Dht11ReadValue(); 
            Console.WriteLine("Press any key to stop service");
            Console.Read();

            //service.StopConnection();
            //piotHubConnection.Stop();

        }

        private static void PrintFrequency(double delayInMicroseconds)
        {
            double stepDuration = delayInMicroseconds; //100 microseconds
            //high speed timer
            Stopwatch clock = new Stopwatch();

            //gets clock rate, ticks per sec
            double freq = Stopwatch.Frequency * 0.000001;
            Console.WriteLine("Stopwatch Resolution in microseconds: {0}", freq);
        }
    }
}
