
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Net35Fixer
{
    public partial class Form1 : Form
    {
        private const int NormalCommandTimeout = 60 * 60 * 1000;
        private const int DetectionCommandTimeout = 60 * 1000;

        private sealed class CommandResult
        {
            public string Output { get; set; }
            public string Error { get; set; }
            public int ExitCode { get; set; }
            public bool TimedOut { get; set; }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private async void btnFixAndInstall_Click(object sender, EventArgs e)
        {
            btnFixAndInstall.Enabled = false;
            txtLog.Clear();

            try
            {
                Log("Starting...");

                lblStatus.Text = "Checking DotNet 3.5...";
                progressBar.Value = Math.Min(
                    progressBar.Maximum,
                    Math.Max(progressBar.Minimum, 5));

                if (await IsNetFx35InstalledAsync())
                {
                    Log("DotNet 3.5 already installed");

                    lblStatus.Text = "DotNet 3.5 already installed";
                    progressBar.Value = progressBar.Maximum;

                    MessageBox.Show(
                        "DotNet 3.5 already installed",
                        "Net Fx",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    Close();
                    return;
                }

                UpdateStatus("Checking rights...", 10);

                if (!IsUserAnAdmin())
                {
                    Log("[ERROR] Util requires admin rights.");

                    MessageBox.Show(
                        "Run util with admin rights!",
                        "Critical Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                UpdateStatus("Fx working on fixing...", 25);
                FixRegistryPolicies();

                UpdateStatus("Fx working on it...", 40);

                await RunCommandAsync("net", "stop wuauserv");
                await RunCommandAsync("net", "stop bits");
                await RunCommandAsync("net", "start wuauserv");
                await RunCommandAsync("net", "start bits");

                UpdateStatus("Fx working on WinSxS...", 60);

                Log("Running DISM...");

                await RunCommandAsync(
                    "dism.exe",
                    "/Online /Cleanup-Image /StartComponentCleanup");

                await RunCommandAsync(
                    "dism.exe",
                    "/Online /Cleanup-Image /RestoreHealth");

                UpdateStatus("Fx working on DotNET...", 85);

                Log("Integrating NetFx3...");

                string dismResult = await RunCommandAsync(
                    "dism.exe",
                    "/Online /Enable-Feature /FeatureName:NetFx3 /All /NoRestart");

                bool featureEnabled = await IsNetFx35InstalledAsync();
                bool reportedSuccess = IsDismSuccess(dismResult);

                if (!featureEnabled && !reportedSuccess)
                {
                    string localSource = FindLocalSxsSource();

                    if (!string.IsNullOrEmpty(localSource))
                    {
                        Log("[INFO] Retrying NetFx3 installation from local source.");
                        Log("[INFO] SxS source: " + localSource);

                        UpdateStatus("Fx working on local source...", 93);

                        string sourceArguments =
                            "/Online /Enable-Feature /FeatureName:NetFx3 " +
                            "/All /Source:\"" + localSource + "\" " +
                            "/LimitAccess /NoRestart";

                        dismResult = await RunCommandAsync(
                            "dism.exe",
                            sourceArguments);

                        featureEnabled = await IsNetFx35InstalledAsync();
                        reportedSuccess = IsDismSuccess(dismResult);
                    }
                    else
                    {
                        Log("[INFO] Local sources\\sxs not found.");
                    }
                }

                if (featureEnabled || reportedSuccess)
                {
                    UpdateStatus("Sucess.", 100);

                    Log("[SUCESS] (recommend) Restart PC.");

                    MessageBox.Show(
                        ".NET Framework 3.5 sucessfully installed!",
                        "Sucess",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    UpdateStatus("Fx error", 100);

                    Log("[WARNING] Online method not work.");

                    MessageBox.Show(
                        "Online method not work.",
                        "Warning",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                Log($"[CRITICAL ERROR] {ex.Message}");
            }
            finally
            {
                if (!IsDisposed && !Disposing)
                {
                    btnFixAndInstall.Enabled = true;
                }
            }
        }

        private async Task<bool> IsNetFx35InstalledAsync()
        {
            try
            {
                if (IsNetFx35InstalledInRegistry())
                {
                    return true;
                }

                CommandResult result = await RunCommandCaptureAsync(
                    "dism.exe",
                    "/Online /Get-FeatureInfo /FeatureName:NetFx3 /English",
                    DetectionCommandTimeout);

                if (result == null || result.TimedOut || result.ExitCode != 0)
                {
                    return false;
                }

                string output = (result.Output ?? string.Empty) +
                                Environment.NewLine +
                                (result.Error ?? string.Empty);

                bool enabled =
                    Regex.IsMatch(
                        output,
                        @"(?im)^\s*State\s*:\s*Enabled\s*$") ||
                    Regex.IsMatch(
                        output,
                        @"(?im)^\s*Состояние\s*:\s*Включено\s*$");

                return enabled;
            }
            catch
            {
                return false;
            }
        }

        // HKLM\SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.5
        private bool IsNetFx35InstalledInRegistry()
        {
            RegistryView[] views = Environment.Is64BitOperatingSystem
                ? new RegistryView[]
                {
                    RegistryView.Registry64,
                    RegistryView.Registry32
                }
                : new RegistryView[]
                {
                    RegistryView.Registry32
                };

            const string keyPath =
                @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.5";

            foreach (RegistryView view in views)
            {
                try
                {
                    using (RegistryKey baseKey =
                        RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
                    using (RegistryKey key = baseKey.OpenSubKey(keyPath))
                    {
                        if (key == null)
                        {
                            continue;
                        }

                        object installValue = key.GetValue("Install");

                        if (installValue != null &&
                            Convert.ToInt32(installValue) == 1)
                        {
                            return true;
                        }
                    }
                }
                catch
                {
                    // dont ruins the check if registry accss fails
                }
            }

            return false;
        }

        private bool IsDismSuccess(string result)
        {
            if (string.IsNullOrWhiteSpace(result))
            {
                return false;
            }

            return result.IndexOf(
                       "успешно",
                       StringComparison.OrdinalIgnoreCase) >= 0 ||
                   result.IndexOf(
                       "successfully",
                       StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private string FindLocalSxsSource()
        {
            DriveInfo[] drives;

            try
            {
                drives = DriveInfo.GetDrives();
            }
            catch
            {
                return null;
            }

            foreach (DriveInfo drive in drives)
            {
                try
                {
                    if (!drive.IsReady)
                    {
                        continue;
                    }

                    string sourcePath = Path.Combine(
                        drive.RootDirectory.FullName,
                        @"sources\sxs");

                    if (!Directory.Exists(sourcePath))
                    {
                        continue;
                    }

                    string[] packages = Directory.GetFiles(
                        sourcePath,
                        "*netfx3*",
                        SearchOption.TopDirectoryOnly);

                    if (packages.Length > 0)
                    {
                        return sourcePath;
                    }
                }
                catch
                {
                    // skip
                }
            }

            return null;
        }

        private async Task<string> RunCommandAsync(
            string fileName,
            string arguments)
        {
            CommandResult result = await RunCommandCaptureAsync(
                fileName,
                arguments,
                NormalCommandTimeout);

            if (!string.IsNullOrEmpty(result.Output))
            {
                Log(result.Output.Trim());
            }

            if (!string.IsNullOrEmpty(result.Error))
            {
                Log("[LOG ERROR] " + result.Error.Trim());
            }

            if (result.TimedOut)
            {
                string timeoutMessage =
                    "Command timed out: " + fileName;

                Log("[LOG ERROR] " + timeoutMessage);

                return (result.Output ?? string.Empty) +
                       (result.Error ?? string.Empty) +
                       Environment.NewLine +
                       timeoutMessage;
            }

            return (result.Output ?? string.Empty) +
                   (result.Error ?? string.Empty);
        }

        [DllImport("kernel32.dll")]
        private static extern uint GetOEMCP();

        private static Encoding GetCommandOutputEncoding()
        {
            try
            {
                uint codePage = GetOEMCP();
                if (codePage != 0)
                {
                    return Encoding.GetEncoding((int)codePage);
                }
            }
            catch
            {
                // 
            }

            return Encoding.Default;
        }

        private static Task WaitForExitAsyncCompat(Process process)
        {
            TaskCompletionSource<bool> completion =
                new TaskCompletionSource<bool>();

            EventHandler exitedHandler = null;
            exitedHandler = (sender, args) =>
            {
                process.Exited -= exitedHandler;
                completion.TrySetResult(true);
            };

            process.Exited += exitedHandler;

            try
            {
                if (process.HasExited)
                {
                    process.Exited -= exitedHandler;
                    completion.TrySetResult(true);
                }
            }
            catch (Exception ex)
            {
                process.Exited -= exitedHandler;
                completion.TrySetException(ex);
            }

            return completion.Task;
        }

        private static string GetCompletedTaskResult(Task<string> task)
        {
            if (task == null || task.Status != TaskStatus.RanToCompletion)
            {
                return string.Empty;
            }

            return task.Result ?? string.Empty;
        }

        private async Task<CommandResult> RunCommandCaptureAsync(
            string fileName,
            string arguments,
            int timeoutMilliseconds)
        {
            try
            {
                Encoding commandEncoding = GetCommandOutputEncoding();

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = commandEncoding,
                    StandardErrorEncoding = commandEncoding
                };

                using (Process proc = new Process())
                {
                    proc.StartInfo = psi;
                    proc.EnableRaisingEvents = true;

                    if (!proc.Start())
                    {
                        return new CommandResult
                        {
                            Output = string.Empty,
                            Error = "Unable to start process: " + fileName,
                            ExitCode = -1,
                            TimedOut = false
                        };
                    }

                    Task<string> outputTask =
                        proc.StandardOutput.ReadToEndAsync();

                    Task<string> errorTask =
                        proc.StandardError.ReadToEndAsync();

                    Task exitTask = WaitForExitAsyncCompat(proc);
                    Task timeoutTask = Task.Delay(timeoutMilliseconds);

                    Task completedTask = await Task.WhenAny(exitTask, timeoutTask);
                    bool exited = completedTask == exitTask || exitTask.IsCompleted;
                    bool timedOut = !exited;

                    if (timedOut)
                    {
                        try
                        {
                            if (!proc.HasExited)
                            {
                                proc.Kill();
                            }
                        }
                        catch
                        {
                            // 
                        }

                        await Task.WhenAny(exitTask, Task.Delay(5000));
                    }

                    bool processStopped = exited || exitTask.IsCompleted;
                    if (!processStopped)
                    {
                        try
                        {
                            processStopped = proc.HasExited;
                        }
                        catch
                        {
                            processStopped = false;
                        }
                    }

                    Task readTasks = Task.WhenAll(outputTask, errorTask);
                    if (processStopped)
                    {
                        try
                        {
                            await readTasks;
                        }
                        catch
                        {
                            // 
                        }
                    }
                    else
                    {
                        // 
                        await Task.WhenAny(readTasks, Task.Delay(1000));
                    }

                    string output = GetCompletedTaskResult(outputTask);
                    string error = GetCompletedTaskResult(errorTask);

                    int exitCode = -1;
                    if (!timedOut && processStopped)
                    {
                        try
                        {
                            exitCode = proc.ExitCode;
                        }
                        catch
                        {
                            exitCode = -1;
                        }
                    }

                    return new CommandResult
                    {
                        Output = output,
                        Error = error,
                        ExitCode = exitCode,
                        TimedOut = timedOut
                    };
                }
            }
            catch (Exception ex)
            {
                return new CommandResult
                {
                    Output = string.Empty,
                    Error = ex.Message,
                    ExitCode = -1,
                    TimedOut = false
                };
            }
        }

        private void LogPreviousRegistryValue(string keyPath, string valueName)
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath, false))
                {
                    object previousValue = key == null
                        ? null
                        : key.GetValue(valueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames);

                    if (previousValue == null)
                    {
                        Log("[REG] Previous " + valueName + " value: <not set>.");
                    }
                    else
                    {
                        Log("[REG] Previous " + valueName + " value: " +
                            Convert.ToString(previousValue) + ".");
                    }
                }
            }
            catch (Exception ex)
            {
                Log("[REG ERROR] Unable to read previous " + valueName + " value: " + ex.Message);
            }
        }

        private void FixRegistryPolicies()
        {
            try
            {
                string keyPath =
                    @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU";

                LogPreviousRegistryValue(keyPath, "UseWUServer");

                using (RegistryKey key =
                    Registry.LocalMachine.CreateSubKey(keyPath))
                {
                    if (key != null)
                    {
                        key.SetValue(
                            "UseWUServer",
                            0,
                            RegistryValueKind.DWord);

                        Log("[REG] UseWUServer disabled (0).");
                    }
                }

                string compPath =
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Servicing";

                LogPreviousRegistryValue(compPath, "RepairContentServerSource");

                using (RegistryKey key =
                    Registry.LocalMachine.CreateSubKey(compPath))
                {
                    if (key != null)
                    {
                        key.SetValue(
                            "RepairContentServerSource",
                            2,
                            RegistryValueKind.DWord);

                        Log("[REG] Fx RepairSource now on Microsoft.");
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"[REG ERROR] Key error: {ex.Message}");
            }
        }

        private bool IsUserAnAdmin()
        {
            try
            {
                System.Security.Principal.WindowsIdentity user =
                    System.Security.Principal.WindowsIdentity.GetCurrent();

                System.Security.Principal.WindowsPrincipal principal =
                    new System.Security.Principal.WindowsPrincipal(user);

                return principal.IsInRole(
                    System.Security.Principal.WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        private void Log(string text)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action(() => Log(text)));
            }
            else
            {
                txtLog.AppendText(
                    $"[{DateTime.Now.ToLongTimeString()}] {text}" +
                    Environment.NewLine);
            }
        }

        private void UpdateStatus(string message, int progressValue)
        {
            lblStatus.Text = message;

            progressBar.Value = Math.Max(
                progressBar.Minimum,
                Math.Min(progressBar.Maximum, progressValue));

            Log($"--- Step: {message} ---");
        }
    }
}
