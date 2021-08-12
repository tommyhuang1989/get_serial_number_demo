using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace get_serial_number_demo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btn_1_Click(object sender, RoutedEventArgs e)
        {
            List<HardDrive> list = get_serial_number();
            foreach (HardDrive hard in list)
            {
                list_number.Items.Add(hard.ToString());
            }
        }

        private void btn_2_Click(object sender, RoutedEventArgs e)
        {
            List<string> list = get_logical_serial_number();
            foreach (string hard in list)
            {
                list_number.Items.Add(hard.ToString());
            }
        }

        private void btn_3_Click(object sender, RoutedEventArgs e)
        {
           list_number.Items.Add(get_volume_serial_numer("C://"));
            list_number.Items.Add(get_volume_serial_numer("D://"));
        }

        private void btn_4_Click(object sender, RoutedEventArgs e)
        {
            //string cm1 = "wmic diskdrive get Name, Manufacturer, Model, InterfaceType, MediaType, SerialNumber";
            string cm1 = "wmic diskdrive get SerialNumber | more +1";
            string cm2 = "WMIC path win32_physicalmedia get serialnumber | more +1";
            //list_number.Items.Add(ExecuteCommandSync(cm1));
            //list_number.Items.Add(ExecuteCommandSync(cm2));
            //list_number.Items.Add(ExecuteCommand(cm1));
            //list_number.Items.Add(ExecuteCommand(cm2));
            //list_number.Items.Add(RunExternalExe("cmd.exe", cm1));
            //list_number.Items.Add(RunExternalExe("cmd.exe", cm2));

            List<string> list1 = ExecuteCommand(cm1); 
            List<string> list2 = ExecuteCommand(cm2);

            foreach (string hard in list1)
            {
                list_number.Items.Add(hard.ToString());
            }

            foreach (string hard in list2)
            {
                list_number.Items.Add(hard.ToString());
            }
        }

        private void btn_5_Click(object sender, RoutedEventArgs e)
        {
            List<string> list = get_physical_media_serial_number();
            foreach (string hard in list)
            {
                list_number.Items.Add(hard.ToString());
            }
        }

        public static List<HardDrive> get_serial_number()
        {
            //var hDid = string.Empty;
            List<HardDrive> list = new List<HardDrive>();

            ManagementObjectSearcher moSearcher = new
 ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");

            foreach (ManagementObject wmi_HD in moSearcher.Get())
            {
                HardDrive hd = new HardDrive();  // User Defined Class
                hd.Model = wmi_HD["Model"].ToString();  //Model Number
                hd.Type = wmi_HD["InterfaceType"].ToString();  //Interface Type
                hd.SerialNo = wmi_HD["SerialNumber"].ToString(); //Serial Number
                list.Add(hd);
            }

            return list;
        }

        public static List<string> get_logical_serial_number()
        {
            //var hDid = string.Empty;
            List<string> list = new List<string>();
            var mc = new ManagementClass("Win32_LogicalDisk");
            var moc = mc.GetInstances();
            foreach (var o in moc)
            {
                var mo = (ManagementObject)o;

                //hDid += (string)mo.Properties["VolumeSerialNumber"].Value + "|";
                list.Add((string)mo.Properties["VolumeSerialNumber"].Value);
            }

            return list;
        }

        public static List<string> get_physical_media_serial_number()
        {
            List<string> list = new List<string>();

            ManagementObjectSearcher searcher = new
                ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMedia");

            foreach (ManagementObject wmi_HD in searcher.Get())
            {
                // get the hardware serial no.
                if (wmi_HD["SerialNumber"] != null)
                {
                    list.Add(wmi_HD["SerialNumber"].ToString());
                }
            }

            return list;
        }

        [DllImport("kernel32.dll")]
        private static extern int GetVolumeInformation(
            string lpRootPathName,
            string lpVolumeNameBuffer,
            int nVolumeNameSize,
            ref int lpVolumeSerialNumber,
            int lpMaximumComponentLength,
            int lpFileSystemFlags,
            string lpFileSystemNameBuffer,
            int nFileSystemNameSize
        );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="root_path">""D://</param>
        /// <returns></returns>
        public static string get_volume_serial_numer(string root_path)
        {
            const int MAX_FILENAME_LEN = 256;
            int retVal = 0;
            int a = 0;
            int b = 0;
            string str1 = null;
            string str2 = null;
            int i = GetVolumeInformation(
            root_path,
            str1,
            MAX_FILENAME_LEN,
            ref retVal,
            a,
            b,
            str2,
            MAX_FILENAME_LEN
            );
            //Console.WriteLine(retVal.ToString("x"));
            //Console.ReadKey();
            return retVal.ToString("x");
        }

        public class HardDrive
        {
            private string model = null;
            private string type = null;
            private string serialNo = null;
            public string Model
            {
                get { return model; }
                set { model = value; }
            }
            public string Type
            {
                get { return type; }
                set { type = value; }
            }
            public string SerialNo
            {
                get { return serialNo; }
                set { serialNo = value; }
            }

            public override string ToString()
            {
                return string.Format("model: {0}, type: {1}, serial: {2}", Model, Type, SerialNo);
            }
        }

        public static string ExecuteCommandSync(object command)
        {
            try
            {
                // create the ProcessStartInfo using "cmd" as the program to be run,
                // and "/c " as the parameters.
                // Incidentally, /c tells cmd that we want it to execute the command that follows,
                // and then exit.
                System.Diagnostics.ProcessStartInfo procStartInfo =
                    new System.Diagnostics.ProcessStartInfo("cmd", "/c " + command);

                // The following commands are needed to redirect the standard output.
                // This means that it will be redirected to the Process.StandardOutput StreamReader.
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                // Do not create the black window.
                procStartInfo.CreateNoWindow = true;
                // Now we create a process, assign its ProcessStartInfo and start it
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = procStartInfo;
                proc.Start();
                // Get the output into a string
                string result = proc.StandardOutput.ReadToEnd();
                // Display the command output.
                return result;
            }
            catch (Exception)
            {
                // Log the exception
                return null;
            }
        }

        public static List<string> ExecuteCommand(object command)
        {
            try
            {
                List<string> list = new List<string>();
                // create the ProcessStartInfo using "cmd" as the program to be run,
                // and "/c " as the parameters.
                // Incidentally, /c tells cmd that we want it to execute the command that follows,
                // and then exit.
                System.Diagnostics.ProcessStartInfo procStartInfo =
                    new System.Diagnostics.ProcessStartInfo("cmd", "/c " + command);

                // The following commands are needed to redirect the standard output.
                // This means that it will be redirected to the Process.StandardOutput StreamReader.
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                // Do not create the black window.
                procStartInfo.CreateNoWindow = true;
                // Now we create a process, assign its ProcessStartInfo and start it
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = procStartInfo;

                proc.Start();
                // Get the output into a string
                //string result = proc.StandardOutput.ReadToEnd();
                StreamReader sr = proc.StandardOutput;//获取返回值 
                string line = "";
                int num = 1;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line != "")
                    {
                        //Console.WriteLine(line + " " + num++);
                        list.Add(line.Trim());
                    }
                }
                //return result;
                return list;
            }
            catch (Exception)
            {
                // Log the exception
                return null;
            }
        }

        public static string RunExternalExe(string filename, string arguments = null)
        {
            var process = new Process();

            process.StartInfo.FileName = filename;
            if (!string.IsNullOrEmpty(arguments))
            {
                process.StartInfo.Arguments = arguments;
            }

            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.UseShellExecute = false;

            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            var stdOutput = new StringBuilder();
            process.OutputDataReceived += (sender, args) => stdOutput.AppendLine(args.Data); // Use AppendLine rather than Append since args.Data is one line of output, not including the newline character.

            string stdError = null;
            try
            {
                process.Start();
                process.BeginOutputReadLine();
                stdError = process.StandardError.ReadToEnd();
                process.WaitForExit();
            }
            catch (Exception e)
            {
                throw new Exception("OS error while executing " + Format(filename, arguments) + ": " + e.Message, e);
            }

            if (process.ExitCode == 0)
            {
                return stdOutput.ToString();
            }
            else
            {
                var message = new StringBuilder();

                if (!string.IsNullOrEmpty(stdError))
                {
                    message.AppendLine(stdError);
                }

                if (stdOutput.Length != 0)
                {
                    message.AppendLine("Std output:");
                    message.AppendLine(stdOutput.ToString());
                }

                throw new Exception(Format(filename, arguments) + " finished with exit code = " + process.ExitCode + ": " + message);
            }
        }

        private static string Format(string filename, string arguments)
        {
            return "'" + filename +
                ((string.IsNullOrEmpty(arguments)) ? string.Empty : " " + arguments) +
                "'";
        }
    }
}
