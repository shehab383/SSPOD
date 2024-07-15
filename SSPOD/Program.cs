using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SSPOD
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);


            //Console.WriteLine("is connected:" + usb.IsConnected());
            //System.Threading.Thread.Sleep(1500);
            //Console.WriteLine("read line output: " + usb.ReadData());
            Form1 chart = new Form1();
            Usb usb = new Usb(chart);

            Application.Run(chart);
            

        }
    }
}
