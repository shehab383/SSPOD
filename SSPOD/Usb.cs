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
using System.Threading.Tasks;

namespace SSPOD
{
    internal class Usb
    {
        private SerialPort serialPort;
        private StringBuilder receivedData ;
        private List<Task> receivedDataList = new List<Task>();
        private System.Timers.Timer readTimer;
        private Form1 ganttChart;
        private List<int[]> IdsList = new List<int[]>();
        private List<int[]>  TimestampsList = new List<int[]>();
        public Usb(Form1 chart) {
             ganttChart = chart;



            Connect();
            //while (!IsConnected()) { Connect(); }
            OnTimedEvent(null, null);
            // Initialize the timer
            readTimer = new System.Timers.Timer(1000); // Set the interval to 400 ms
            readTimer.Elapsed += OnTimedEvent;
            readTimer.Start();
        }
        // Function to connect to the USB port
        public void Connect()
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
                        return ;
                    }
                    else { Console.WriteLine("Unable to Connect");
                        Connect();
                    }
                   
                }
                else
                {
                    Connect();
                    return ;
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error connecting to USB port: " + ex.Message);
                return ; // Connection failed
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
                // Split the input string into lines
                string[] lines = data.Split('\n');
                Console.WriteLine(" recieved data: " + string.Join(", ", lines));
                for (int i = 0; i < lines.Length; i++) {
                    if (lines[i].Length <10)
                    {
                        continue;
                    }
                    var values = Array.ConvertAll(lines[i].Trim().Split('-'), int.Parse);

                    if (values.Length > 0 && values[0] == 0)
                    {
                        var ids = new int[values.Length - 1];
                        for (int j = 0; j < values.Length-1; j++)
                        {

                            ids[j] = values[j + 1];
                        }
        
                        IdsList.Add(ids);
                    }
                    else
                    {
                        // This is a timestamp line
                        TimestampsList.Add(values);
                    }
                }
                if(IdsList.Count > 0)
                {
                    foreach (var item in IdsList)
                    {
                        Console.WriteLine("current ids: " + string.Join(", ", item));
                    }
                    foreach (var item in TimestampsList)
                    {
                        Console.WriteLine("current timestamps: " + string.Join(", ", item));
                    }
                }
                else
                {
                    Console.WriteLine("No data available");
                }
                
                while(IdsList.Count > 0 && TimestampsList.Count > 0)
                {
                    int[] intIds = IdsList[0];
                    int[] intTimestamps = TimestampsList[0];
                    IdsList.RemoveAt(0);
                    TimestampsList.RemoveAt(0);
                    receivedDataList.AddRange(MatchTasksWithTimestamps(intIds, intTimestamps));
                }   
               

                //receivedDataList.Reverse();

                // Ensure that the UI update is performed on the UI thread
                ganttChart.BeginInvoke(new Action(() =>
                {
                    //Console.WriteLine("Data sent: " + string.Join(", ", receivedDataList));
                    if(receivedDataList.Count > 0)
                    ganttChart.DrawFromList(receivedDataList);
                    receivedDataList.Clear();
                    //Console.WriteLine("The current available data in the list: " + string.Join(", ", receivedDataList));
                }));


            }
        }

        static List<Task> MatchTasksWithTimestamps(int[] ids, int[] timestamps)
        {
            var results = new List<Task>();
            var taskStartTimes = new Dictionary<int, Queue<int>>();
            Task tempTask;
            for (int i = 0; i < ids.Length; i++)
            {
                int taskId = ids[i];
                int timestamp = timestamps[i];

                // If the task ID is seen for the first time, initialize its queue
                if (!taskStartTimes.ContainsKey(taskId))
                {
                    taskStartTimes[taskId] = new Queue<int>();
                }

                // If the queue is empty or has an even number of timestamps, add a start timestamp
                if (taskStartTimes[taskId].Count % 2 == 0)
                {
                    taskStartTimes[taskId].Enqueue(timestamp);
                }
                else
                {
                    // Get the start timestamp and remove it from the queue
                    int startTime = taskStartTimes[taskId].Dequeue();

                    // Add the task ID, start, and end times to the results
                    tempTask = new Task($"task{taskId}", startTime, timestamp);
                    results.Add(tempTask);

                    // If there are more start times in the queue, keep processing them
                    while (taskStartTimes[taskId].Count > 0)
                    {
                        startTime = taskStartTimes[taskId].Dequeue();
                        tempTask = new Task($"task{taskId}", startTime, timestamp);
                        results.Add(tempTask);
                    }
                }
            }

            return results;
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
