using System;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Linq;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Xml;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO.Ports;
using System.Management;
using System.Timers;
using System.Data;
using System.Threading;
using System.Windows.Forms.DataVisualization.Charting;

namespace SSPOD
{
    internal class Usb
    {
        private SerialPort serialPort;
        private StringBuilder receivedData ;
        private List<string> receivedDataList = new List<string>();
        private System.Timers.Timer readTimer;
        private Form1 ganttChart;

        public Usb(Form1 chart) {
             ganttChart = chart;

            // Initialize the timer
            readTimer = new System.Timers.Timer(1000); // Set the interval to 1000 ms (1 second)
            readTimer.Elapsed += OnTimedEvent;

            Connect();
            readTimer.Start();
        }
        // Function to connect to the USB port
        public bool Connect()
        {   
            
            //"USB Serial Device" is the name found in the windows device manager
            string  USBPort = GetPortWithName("USB Serial Device");
         



            try
            {
                 serialPort = new SerialPort(USBPort, 9600, Parity.None, 8, StopBits.One);
                
                if (!USBPort.Equals("not found") && !serialPort.IsOpen /*&& SPODUSB.*/)
                {
                    serialPort.Open();
                    if (serialPort.IsOpen) { Console.WriteLine("connected On: " + USBPort);
                        return true;
                    }
                    else { Console.WriteLine("Unable to Connect");
                        return false;
                    }
                   
                }
                else
                {
                    return false;
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error connecting to USB port: " + ex.Message);
                return false; // Connection failed
            }
        }

        // Function to check if the USB port is connected
        public bool IsConnected()
        {
            return serialPort != null && serialPort.IsOpen;
        }

        // Timer event handler to periodically check for data
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            if (serialPort != null && serialPort.IsOpen && serialPort.BytesToRead > 0)
            {
                string data = serialPort.ReadExisting();
                receivedDataList.Add(data);

                // Ensure that the UI update is performed on the UI thread
                ganttChart.BeginInvoke(new Action(() =>
                {
                    Console.WriteLine("Data sent: " + string.Join(", ", receivedDataList));
                    ganttChart.DrawFromList(receivedDataList);
                    receivedDataList.Clear();
                    Console.WriteLine("The current available data in the list: " + string.Join(", ", receivedDataList));
                }));


            }
        }

        // Function to read data from the USB port
        public String ReadData()
        { 
        byte[] readBuffer = new byte[200000];
            try
            {
                Console.WriteLine("bytes to read:" + serialPort.BytesToRead);
                if (serialPort.BytesToRead > 0)
                {
              
 
                    serialPort.Read(readBuffer, 0, 100000);
                    string data = Encoding.ASCII.GetString(readBuffer);
                    receivedData.Append(data);
                    return receivedData.ToString();
                }
                else
                {
                    // return "No data available.";
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading from USB port: " + ex.Message);
                //return "Read error.";
                return null;
            }
        }

        // Function to close the USB port connection
        public void Disconnect()
        {
            if (IsConnected())
            {
                serialPort.Close();
            }
        }


        private string GetPortWithName(string windowsPortName)
        {
            string pnpId = "";





            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity"))
            {


                ManagementObjectCollection collection = searcher.Get();
              
                if (collection.Count == 0)
                {
                   

                    return null;
                }

                foreach (ManagementObject obj in collection)
                {
                  

                    string portName = obj["DeviceID"]?.ToString();
                    string portDescription = obj["Description"]?.ToString();
                    string portManufacturer = obj["Manufacturer"]?.ToString();
               
                    if (portDescription != null && portDescription.ToString().Equals(windowsPortName))

                    {
                        pnpId = obj["PNPDeviceID"].ToString();

                    }

                }

            }

            string query = "SELECT * FROM Win32_SerialPort";

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    string deviceId = obj["PNPDeviceID"]?.ToString();
                    string name = obj["DeviceID"]?.ToString();

                    if (!string.IsNullOrEmpty(deviceId) && obj["PNPDeviceID"].ToString().Equals(pnpId))
                    {
                   

                        return name;
                    }
                }
            }


            return "not found";



        }
    }
}
