using PiOfThings.GpioCore;
using PiOfThings.GpioUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piofthings.Services
{
    public class TemperatureSensorClient
    {
        int[] dht11_val = { 0, 0, 0, 0, 0 };
        GpioManager _manager = new GpioManager();
        int MAX_TIME = 85;
        public void Dht11ReadValue()
        {
            PiOfThings.GpioUtils.GpioPinState lststate = GpioPinState.High;
            Int16 counter = 0;
            Int16 j = 0, i;
            float farenheit=0.0f;
            for (i = 0; i < 5; i++)
            {
                dht11_val[i] = 0;
            }
            //pinMode(DHT11PIN,OUTPUT);  
            _manager.SelectPin(GpioId.GPIO07);
            //digitalWrite(DHT11PIN,LOW);  
            _manager.WriteToPin(GpioPinState.Low);
            this.Delay(18000);
            //digitalWrite(DHT11PIN,HIGH);  
            _manager.WriteToPin(GpioPinState.High);
            //delayMicroseconds(40);  
            this.Delay(40);
            //pinMode(DHT11PIN,INPUT);  
            _manager.ReadFromPin(GpioId.GPIO07);
            for (i = 0; i < MAX_TIME; i++)
            {
                counter = 0;
                while (_manager.ReadFromPin(GpioId.GPIO07) == lststate)
                {
                    counter += 1;
                    this.Delay(1);
                    if (counter >= 255)
                        break;
                }
                lststate = _manager.ReadFromPin(GpioId.GPIO07);
                if (counter == 255)
                    break;
                // top 3 transistions are ignored  
                if ((i >= 4) && (i % 2 == 0))
                {
                    dht11_val[j / 8] <<= 1;
                    if (counter > 16)
                        dht11_val[j / 8] |= 1;
                    j++;
                }
            }
            //// verify cheksum and print the verified data  
            if ((j >= 40) && (dht11_val[4] == ((dht11_val[0] + dht11_val[1] + dht11_val[2] + dht11_val[3]) & 0xFF)))
            {
                
                //farenheit=dht11_val[2]*9./5.+32;  
                Console.WriteLine("Humidity = %d.%d %% Temperature = %d.%d *C (%.1f *F)\n", dht11_val[0], dht11_val[1], dht11_val[2], dht11_val[3], farenheit);
            }
            else
                Console.WriteLine(string.Format("Invalid Data!! {0}, {1}, {2}, {3}, {4}\n", dht11_val[4], dht11_val[0], dht11_val[1], dht11_val[2], dht11_val[3]));
        }
        //gets clock rate, ticks per sec
        long freq = Stopwatch.Frequency;
        Stopwatch clock = new Stopwatch();

        public void Delay(double microSecondDelay)
        {
            //define a wait duration
            double stepDuration = microSecondDelay * 0.000001;  //0.000001 = 1 microseconds
            //high speed timer

            //number of ticks to elapse before exiting loop
            double ticksPerStep = (double)freq * stepDuration;
            clock.Restart();//start clock
            while (clock.ElapsedTicks < ticksPerStep)
            {
                //wait------
            }
        }

        //int main()
        //{
        //    Console.WriteLine("Interfacing Temperature and Humidity Sensor (DHT11) With Raspberry Pi\n");
        //    if (wiringPiSetup() == -1)
        //        exit(1);
        //    while (1)
        //    {
        //        Dht11ReadValue();
        //        delay(3000);
        //    }
        //    return 0;
        //}
    }
}
