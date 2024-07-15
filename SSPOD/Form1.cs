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
        int i = 0;
        public Form1()
        {
            InitializeComponent();
            List<string> NEWLIST = new List<string>();
            NEWLIST.Add("1 , 2");
            NEWLIST.Add("2 , 4");
            NEWLIST.Add("4 , 6");
            NEWLIST.Add("6 , 7");
            NEWLIST.Add("20 , 30");
            //DrawFromList(NEWLIST);

            //AddTaskToChart(chart1, new Task("task1", 1, 4));
            //AddTaskToChart(chart1, new Task("task2", 4, 6));
            //AddTaskToChart(chart1, new Task("task3", 8, 9));
            //AddTaskToChart(chart1, new Task("task3", 9, 11));
            //AddTaskToChart(chart1, new Task("task3", 11, 14));
            //AddTaskToChart(chart1, new Task("task3", 15, 20));
            //AddTaskToChart(chart1, new Task("task3", 21, 25));
        }
        public void DrawFromList(List<string> list)
        {
           
            foreach (string data in list)
            {
                int[] numbers = GetNumbersAsArray(data);
                AddTaskToChart(new Task("task" + i++, numbers[0], numbers[1]));

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

    }


    public class Task
    {
        public string TaskName { get; set; }
        public double StartValue { get; set; }
        public double EndValue { get; set; }

        public Task(string taskName, double startValue, double endValue)
        {
            TaskName = taskName;
            StartValue = startValue;
            EndValue = endValue;
        }
    }
}
