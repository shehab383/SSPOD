using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace SSPOD
{
    public partial class Form1 : Form
    {
        private List<string> receivedDataList = new List<string>();
        private List<Task> taskList = new List<Task>();
        int i = 0;
        public Form1()
        {
            InitializeComponent();
          

        }
        public void DrawFromList(List<Task> list)
        {  
           
            foreach (Task task in list)
            {
              
       

                   


                    taskList.Add(task);
                    AddTaskToChart(task);
                    // Update CPU usage at start
                    float cpuUsage = CalculateCpuUsage();
                    textBox1.Text = $"Average CPU Usage: {cpuUsage:F2}%";
                

            }

        }
        private int[] GetNumbersAsArray(string data)
        {
            // Split the string by the comma
            string[] parts = data.Split(',');

            // Check that the split resulted in exactly two parts
            if (parts.Length == 2)
            {
                // Trim the parts and convert them to integers
                if (int.TryParse(parts[0].Trim(), out int num1) && int.TryParse(parts[1].Trim(), out int num2))
                {
                    // Return the numbers as an array of two integers
                    return new int[] { num1, num2 };
                }
            }
            // Return null if the string could not be parsed correctly
            return null;
        }
  
        public void AddTaskToChart( Task task)
        {
            Chart chart = chart1;
            Series series = chart.Series.FindByName(task.TaskName);

            if (series == null)
            {
                series = new Series(task.TaskName)
                {
                    ChartType = SeriesChartType.RangeBar,
                    YValuesPerPoint = 2
                };
                chart.Series.Add(series);
            }

            DataPoint dataPoint = new DataPoint(1D, new double[] { task.StartValue, task.EndValue })
            {
                AxisLabel = "Gantt Chart"
            };

            series.Points.Add(dataPoint);
            this.Refresh();
        }

        private int CalculateTotalTime()
        {
            if (taskList.Count == 0)
                return 0;

            // Total time from the start of the first task to the end of the last task
            int totalTime = taskList.Last().EndValue;
            return totalTime;
        }

        private int CalculateIdleTime()
        {
            int idleTime = 0;

            // Ensure tasks are ordered by start time
            var orderedTasks = taskList.OrderBy(t => t.StartValue).ToList();

            // Calculate idle time between consecutive tasks
            for (int i = 1; i < orderedTasks.Count; i++)
            {
                idleTime += orderedTasks[i].StartValue - orderedTasks[i - 1].EndValue;
            }

            return idleTime;
        }

        private float CalculateCpuUsage()
        {
            int totalTime = CalculateTotalTime();
            if (totalTime == 0)
                return 0;

            int idleTime = CalculateIdleTime();
            int busyTime = totalTime - idleTime;
            return (busyTime / (float)totalTime) * 100;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }


    public class Task
    {
        public string TaskName { get; set; }
        public int StartValue { get; set; }
        public int EndValue { get; set; }

        public Task(string taskName, int startValue, int endValue)
        {
            TaskName = taskName;
            StartValue = startValue;
            EndValue = endValue;
        }
       public static Task GetTaskDetails(string task)
        {
            // Split the string by the comma
            string[] parts = task.Split(',');

            // Check that the split resulted in exactly three parts
            if (parts.Length == 3)
            {
                string taskName = parts[0].Trim();
                if (int.TryParse(parts[1].Trim(), out int start) && int.TryParse(parts[2].Trim(), out int end))
                {
                    // Return the parsed task details
                    return new Task(taskName, start, end);  
        
                }
            }

            // Return null if the string could not be parsed correctly
            return null;
        }
    }
}
