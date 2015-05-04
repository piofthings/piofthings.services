using Microsoft.AspNet.SignalR.Client;
using Piofthings.Data;
using PiOfThings.GpioCore;
using PiOfThings.GpioUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piofthings.Services
{
    public class RelayClient
    {
        readonly GpioManager _manager = new GpioManager();

        private IHubProxy PiotHubProxy { get; set; }

        private HubConnection PiotHubConnection { get; set; }

        public RelayClient(HubConnection iotHubConnection, IHubProxy iotHubProxy)
        {
            PiotHubConnection = iotHubConnection;
            PiotHubProxy = iotHubProxy; //PioTHubConnection.CreateHubProxy("PiotHub");

            PiotHubProxy.On<GpioId>("SwitchOn", OnSwitchedOn);

            PiotHubProxy.On<GpioId>("SwitchOff", OnSwitchedOff);

            PiotHubProxy.On("StatusProbe", OnProbeRecieved);
        }

        private void OnSwitchedOn(GpioId gpioPinId)
        {
            Console.WriteLine("SWITCH ON RECIEVED " + gpioPinId);
            if (_manager.CurrentPin != gpioPinId)
            {
                _manager.SelectPin(gpioPinId);
            }
            _manager.WriteToPin(GpioPinState.Low);
        }

        private void OnSwitchedOff(GpioId gpioPinId)
        {
            Console.WriteLine("SWITCH OFF RECIEVED " + gpioPinId);

            if (_manager.CurrentPin != gpioPinId)
            {
                _manager.SelectPin(gpioPinId);
            }
            _manager.WriteToPin(GpioPinState.High);
        }

        private void OnProbeRecieved()
        {
            Console.WriteLine("StatusProbe RECIEVED ");
            try
            {
                GpioDeviceState data = new GpioDeviceState();
                for (int i = 1; i <= 40; i++)
                {
                    GpioId currentPinId = GpioPinMapping.GetGpioId(i);
                    if (currentPinId != GpioId.GPIOUnknown)
                    {
                        //_manager.SelectPin (GpioPinMapping.GetGPIOId (i));
                        GpioPinState state = _manager.ReadFromPin(currentPinId);
                        data.GpioPinStates.Add(currentPinId, state);
                        //_manager.ReleasePin (currentPinId);
                    }
                }
                data.TimeStamp = DateTime.UtcNow.Ticks;
                PiotHubProxy.Invoke<string>("CurrentStatus", data).ContinueWith(sendStatusTask =>
                {
                    if (sendStatusTask.IsFaulted)
                    {
                        Console.WriteLine("There was an error opening the connection:{0}",
                            sendStatusTask.Exception.GetBaseException());
                    }
                    else
                    {
                        Console.WriteLine(string.Format("Probe data sent: {0}", new DateTime(data.TimeStamp).ToLongDateString()));
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception : {0}" + ex.Message);
            }

        }

        public void StartConnection(Task task)
        {
            //Start connection
            DeviceDataContext context = new DeviceDataContext();
            DeviceData data = context.GetActiveDeviceData();
            try
            {
                if (task.IsFaulted)
                {
                    Console.WriteLine("There was an error opening the connection:{0}",
                        task.Exception.GetBaseException());
                }
                else
                {
                    Console.WriteLine("Connected");

                    PiotHubProxy.Invoke<string>("HandShake", data.DeviceId).ContinueWith(joinGroupTask =>
                    {
                        if (task.IsFaulted)
                        {
                            Console.WriteLine("There was an error calling send: {0}",
                                task.Exception.GetBaseException());
                        }
                        else
                        {
                            Console.WriteLine("Handshake successful - " + joinGroupTask.Result);
                        }
                    });
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        public void StopConnection()
        {
            _manager.ReleaseAll();
        }
    }
}
