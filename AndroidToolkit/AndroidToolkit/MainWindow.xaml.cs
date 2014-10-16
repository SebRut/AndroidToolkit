using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AndroidDeviceConfig;
using de.sebastianrutofski.AndroidToolkit.Models;
using Ionic.Zip;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using RegawMOD.Android;
using Action = AndroidDeviceConfig.Action;

namespace de.sebastianrutofski.AndroidToolkit
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private readonly List<DeviceConfig> _Devices = new List<DeviceConfig>();

        private DeviceModel _DeviceModel = new DeviceModel();
        private LogModel _Log = new LogModel();
        private bool _OperationRunning;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = DeviceModel;
            logBox.DataContext = Log;
        }

        public DeviceModel DeviceModel
        {
            get { return _DeviceModel; }
            set { _DeviceModel = value; }
        }

        public LogModel Log
        {
            get { return _Log; }
            set { _Log = value; }
        }

        private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ProgressDialogController controller = await this.ShowProgressAsync("Please wait...", String.Empty);
            controller.SetCancelable(false);

            controller.SetMessage("Initializing...");
            LogBaseSystemInfos();
            controller.SetProgress(0.16);

            controller.SetMessage("Preparing file system...");
            PrepareFileSystem();
            controller.SetProgress(0.3);

            controller.SetMessage("Loading device configs...");
            LoadDeviceConfig();
            controller.SetProgress(0.6);

            controller.SetMessage("Searching your device...");
            controller.SetCancelable(true);
            RecognizeDevice();
            controller.SetProgress(1);

            await controller.CloseAsync();

            if (Directory.GetFiles("Data/Devices").Length == 0)
            {
                await
                    this.ShowMessageAsync("No configs found",
                        "It appears that you haven't downloaded any device configs yet. Please download the configs you need from this site, put them in \"Data/Devices\" and restart he toolkit.");
                Process.Start("https://github.com/SebRut/AndroidDeviceConfig-Device-Configs");
            }
        }

        private void LogBaseSystemInfos()
        {
            Log.AddLogItem(Environment.OSVersion + " " + (Environment.Is64BitOperatingSystem ? "64Bit" : "32bit"),
                "INFO");
            Log.AddLogItem(
                Assembly.GetExecutingAssembly().GetName().Name + " " + Assembly.GetExecutingAssembly().GetName().Version,
                "INFO");
            foreach (AssemblyName referencedAssembly in Assembly.GetExecutingAssembly().GetReferencedAssemblies())
            {
                Log.AddLogItem(referencedAssembly.Name + " " + referencedAssembly.Version, "INFO");
            }
        }

        private async Task DownloadFile(string url, string path)
        {
            ProgressDialogController controller =
                await this.ShowProgressAsync("Downloading...", "URL:" + url + "\nDestination:" + path);
            controller.SetCancelable(false);

            var client = new WebClient();
            client.DownloadProgressChanged += (s, e) =>
            {
                controller.SetMessage(e.BytesReceived + "B/" + e.TotalBytesToReceive + "B");
                double progress = 0;
                if (e.ProgressPercentage > progress)
                {
                    controller.SetProgress(e.ProgressPercentage / 100.0d);
                    progress = e.ProgressPercentage;
                }
            };
            client.DownloadFileCompleted += (s, e) => controller.CloseAsync();
            Log.AddLogItem("Download of " + url + "started!", "DOWNLOAD");
            await client.DownloadFileTaskAsync(new Uri(url), path);
        }

        private void RecognizeDevice()
        {
            _OperationRunning = true;
            AndroidController android = AndroidController.Instance;
            try
            {
                android.UpdateDeviceList();
                if (android.HasConnectedDevices)
                {
                    DeviceModel.Device = android.GetConnectedDevice(android.ConnectedDevices[0]);
                    if (DeviceModel.Device == null)
                    {
                    }
                    else
                    {
                        switch (DeviceModel.Device.State)
                        {
                            case DeviceState.ONLINE:
                                if (DeviceModel.Device.BuildProp.GetProp("ro.product.model") == null)
                                {
                                }
                                else
                                {
                                    foreach (DeviceConfig deviceConfig in _Devices)
                                    {
                                        bool found = false;
                                        foreach (DeviceVersion deviceVersion in deviceConfig.Versions)
                                        {
                                            bool matching = true;

                                            foreach (DeviceIdentifier deviceIdentifier in deviceVersion.Identifiers)
                                            {
                                                switch (deviceIdentifier.Type)
                                                {
                                                    case IdentifierType.Name:
                                                        matching =
                                                            DeviceModel.Device.BuildProp.GetProp("ro.product.name") ==
                                                            deviceIdentifier.AdditionalArgs[0];
                                                        break;

                                                    case IdentifierType.AndroidVersion:
                                                        matching =
                                                            DeviceModel.Device.BuildProp.GetProp(
                                                                "ro.build.version.release") ==
                                                            deviceIdentifier.AdditionalArgs[0];
                                                        break;
                                                    case IdentifierType.ProductDevice:
                                                        matching =
                                                            DeviceModel.Device.BuildProp.GetProp("ro.product.device") ==
                                                            deviceIdentifier.AdditionalArgs[0] ||
                                                            DeviceModel.Device.BuildProp.GetProp("ro.build.product") ==
                                                            deviceIdentifier.AdditionalArgs[0];
                                                        break;
                                                }
                                            }

                                            if (!matching) continue;
                                            DeviceModel.Config = new ConfigModel(deviceConfig);
                                            DeviceModel.Version = new VersionModel(deviceVersion);

                                            Log.AddLogItem(
                                                deviceConfig.Vendor + " " + deviceConfig.Name + " detected!", "DEVICE");

                                            found = true;
                                            break;
                                        }

                                        if (found) break;
                                    }
                                }
                                break;
                        }
                    }
                }
            }
            catch
            {
            }
            _OperationRunning = false;
        }

        private void LoadDeviceConfig()
        {
            foreach (string file in Directory.GetFiles("Data/Devices"))
            {
                _Devices.Add(DeviceConfig.LoadConfig(file));
                Log.AddLogItem("config file \"" + file + "\" loaded!", "CONFIG");
            }
        }

        private void PrepareFileSystem()
        {
            string[] neededDirectories =
            {
                "Data/", "Data/Backups", "Data/Installers", "Data/Logcats", "Data/Logs",
                "Data/Recoveries", "Data/Devices", "Data/Temp"
            };

            foreach (string dir in neededDirectories)
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                    Log.AddLogItem(dir + " created!", "FILESYSTEM");
                }
            }
        }

        private async Task PrepareFastboot()
        {
            if (DeviceModel.Device.State != DeviceState.FASTBOOT)
            {
                DeviceModel.Device.RebootBootloader();
                await this.ShowMessageAsync(String.Empty,
                    "Your device will boot to bootloader now. Please click \"Ok\" when your device has booted to bootloader.");
            }
        }

        private async Task PrepareSystem()
        {
            if (DeviceModel.Device.State != DeviceState.ONLINE)
            {
                DeviceModel.Device.Reboot();
                await
                    this.ShowMessageAsync(String.Empty,
                        "Your device will boot to system now. Please click \"Ok\" when your device has booted to system.");
            }
        }

        private void OpenLinkCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var url = e.Parameter as string;
            if (url != null) Process.Start(url);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            _OperationRunning = true;

            ProgressDialogController controller = await this.ShowProgressAsync("Performing Actions..", String.Empty);
            controller.SetCancelable(false);

            ActionSetModel actionSetModel = ((ActionSetModel)actionsList.SelectedItem);
            string actionSetDirectory = "Data/Temp/" + actionSetModel.GetHashCode();
            for (int i = 0; i < actionSetModel.Actions.Count; i++)
            {
                Action action = actionSetModel.Actions[i];
                controller.SetMessage(String.Format("Action {0} of {1}",
                    i + 1, actionSetModel.Actions.Count));

                switch (action.Type)
                {
                    case ActionType.ExecuteFastbootCommand:
                        {
                            await PerformFastbootCommand(StringMethods.StringEnumToString(action.AdditionalInfos));
                            break;
                        }
                    case ActionType.ExecuteAdbCommand:
                        {
                            await PrepareSystem();

                            //TODO convert to "PerformAdbCommand"
                            AdbCommand command = Adb.FormAdbCommand(StringMethods.StringEnumToString(action.AdditionalInfos));
                            Log.AddLogItem(String.Format("command \"adb {0}\" exited with code {1}", command.ToString(), Adb.ExecuteAdbCommandReturnExitCode(command)), "ADB");

                            break;
                        }
                    case ActionType.ExtractZip:
                        using (ZipFile zip = ZipFile.Read(action.AdditionalInfos[0]))
                        {

                            zip.ExtractAll(actionSetDirectory + "/" + action.AdditionalInfos[0]);
                        }
                        break;
                    case ActionType.DownloadFile:
                        await DownloadFile(action.AdditionalInfos[0], actionSetDirectory + "/" + action.AdditionalInfos[1]);
                        break;
                }

                controller.SetProgress(actionSetModel.Actions.Count * (i + 1) / 1);
            }

            _OperationRunning = false;
        }

        private async Task PerformFastbootCommand(string command)
        {
            await PrepareFastboot();

            FastbootCommand fCommand =
                Fastboot.FormFastbootCommand(DeviceModel.Device, command);
            Log.AddLogItem(
                String.Format("command \"{0}\" responded with: {1}", fCommand.ToString(),
                    Fastboot.ExecuteFastbootCommand(fCommand)), "FASTBOOT");
        }

        private async void FlashRecoveryButton_Click(object sender, RoutedEventArgs e)
        {
            _OperationRunning = true;
            if (recoveriesList.SelectedItem != null)
            {

                var recovery = ((RecoveryModel)recoveriesList.SelectedItem);
                if (!File.Exists("Data/Recoveries/" + recovery.Name + "_" + DeviceModel.GetHashCode() + ".img"))
                {
                    await DownloadFile(recovery.DownloadUrl,
                        "Data/Recoveries/" + recovery.Name + "_" + DeviceModel.GetHashCode() + ".img");
                }
                await PerformFastbootCommand("flash " + "recovery " + "Data/Recoveries/" + recovery.Name + "_" +
                                             DeviceModel.GetHashCode() + ".img");
            }
            _OperationRunning = false;
        }

        private void ToggleLogCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ((Flyout)Flyouts.Items[0]).IsOpen = !((Flyout)Flyouts.Items[0]).IsOpen;
        }

        private async void ReloadDeviceCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ProgressDialogController controller =
                await this.ShowProgressAsync("Please wait...", "Searching your device...");
            controller.SetCancelable(false);
            RecognizeDevice();
            await controller.CloseAsync();
        }

        private void ReloadDeviceCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !_OperationRunning;
        }

        private async void GetHtcUnlockTokenButton_Click(object sender, RoutedEventArgs e)
        {
            _OperationRunning = true;
            Log.AddLogItem("Trying to fetch unlock token", "FASTBOOT");
            string rawReturn = Fastboot.ExecuteFastbootCommand(Fastboot.FormFastbootCommand(DeviceModel.Device, "oem get_identifier_token"));
            string rawToken = StringMethods.GetStringBetween(rawReturn, "< Please cut following message >\r\n", "\r\nOKAY");
            string cleanedToken = rawToken.Replace("(bootloader) ", "");
            Thread.CurrentThread.SetApartmentState(ApartmentState.STA);
            Clipboard.SetText(cleanedToken);
            await this.ShowMessageAsync("Success!", "The unlock token has been copied to your clipboard. Follow the instructions on the htc dev page, that will open when you click ok, to obtain your unlock bin.");
            Process.Start("http://www.htcdev.com/bootloader/unlock-instructions/page-3");

            _OperationRunning = false;
        }

        private async void HtcUnlockButton_Click(object sender, RoutedEventArgs e)
        {
            _OperationRunning = true;

            OpenFileDialog ofd = new OpenFileDialog { CheckFileExists = true, Filter = "*.bin", Multiselect = false };
            ofd.ShowDialog();

            if (File.Exists(ofd.FileName))
            {
                await PerformFastbootCommand("oem unlock " + ofd.FileName);
            }
            _OperationRunning = false;
        }
    }
}
