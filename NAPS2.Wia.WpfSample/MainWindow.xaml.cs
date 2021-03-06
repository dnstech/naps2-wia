﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace NAPS2.Wia.WpfSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private WiaDeviceInfo currentDevice;
        private string currentPaperSource;
        private bool hasLoadedDevices = false;
        private bool isScanning;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public ObservableCollection<WiaDeviceInfo> Devices { get; } = new ObservableCollection<WiaDeviceInfo>();

        public ObservableCollection<string> PaperSources { get; } = new ObservableCollection<string>();


        public ObservableCollection<ImageSource> Pages { get; } = new ObservableCollection<ImageSource>();

        public ObservableCollection<string> Logs { get; } = new ObservableCollection<string>();

        public WiaDeviceInfo CurrentDevice
        {
            get => this.currentDevice;
            set
            {
                if (this.currentDevice != value)
                {
                    this.currentDevice = value;
                    this.RaisePropertyChanged();

                    this.PaperSources.Clear();
                    foreach (var source in this.currentDevice.Sources)
                    {
                        this.PaperSources.Add(source);
                    }

                    this.CurrentPaperSource = this.PaperSources.FirstOrDefault();
                }
            }
        }

        /// <summary>
        /// Gets or sets what kind of scan we are going to perform, from a flatbed, 
        /// or a feeder and, whether it should be single or double sided (DUPLEX)
        /// </summary>
        public string CurrentPaperSource
        {
            get => this.currentPaperSource;
            set
            {
                if (this.currentPaperSource != value)
                {
                    this.currentPaperSource = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public bool IsScanning
        {
            get => this.isScanning;
            set
            {
                if (this.isScanning != value)
                {
                    this.isScanning = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public int ScanProgress { get; set; }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            if (!this.hasLoadedDevices)
            {
                this.hasLoadedDevices = true;
                this.RefreshDevices();
            }
        }

        private async void RefreshDevices()
        {
            this.Devices.Clear();
            var devices = await Task.Factory.StartNew(this.GetAllDevices, TaskCreationOptions.LongRunning);
            foreach (var device in devices)
            {
                this.Devices.Add(device);
            }

            this.CurrentDevice = this.Devices.FirstOrDefault();
        }

        private IReadOnlyList<WiaDeviceInfo> GetAllDevices()
        {
            var devices = new List<WiaDeviceInfo>();
            try
            {
                using (var deviceManager = new WiaDeviceManager(WiaVersion.Wia20))
                {
                    foreach (var device in deviceManager.GetDeviceInfos())
                    {
                        try
                        {
                            using (device)
                            {
                                var paperSources = new List<string>();
                                using (var openDevice = deviceManager.FindDevice(device.Id()))
                                {
                                    if (openDevice.SupportsFlatbed())
                                    {
                                        paperSources.Add("Flatbed");
                                    }

                                    if (openDevice.SupportsFeeder())
                                    {
                                        paperSources.Add("Feeder");
                                        if (openDevice.SupportsDuplex())
                                        {
                                            paperSources.Add("FeederDuplex");
                                        }
                                    }
                                }

                                var info = new WiaDeviceInfo(device.Id(), device.Name(), paperSources);
                                devices.Add(info);
                            }
                        }
                        catch (WiaException ex)
                        {
                            this.Log(ex.Message);
                        }
                    }
                }
            }
            catch (WiaException ex)
            {
                this.Log(ex.Message);
            }

            return devices;
        }

        private void ScanClicked(object sender, RoutedEventArgs e)
        {
            this.Pages.Clear();
            Task.Factory.StartNew(this.Scan, TaskCreationOptions.LongRunning);
        }

        private void Scan()
        {
            try
            {
                this.Log("Starting WIA Scan");
                using (var deviceManager = new WiaDeviceManager(WiaVersion.Wia20))
                {
                    using (var device = deviceManager.FindDevice(this.CurrentDevice.Id))
                    {
                        var subItem = this.CurrentPaperSource == "Flatbed" ? "Flatbed" : "Feeder";

                        // Select between Flatbed/Feeder
                        using (var item = device.FindSubItem(subItem))
                        {
                            if (this.CurrentPaperSource == "FeederDuplex")
                            {
                                // Enable duplex scanning
                                item.EnableDuplex();
                            }

                            if (subItem == "Feeder")
                            {
                                // Set WIA_IPS_PAGES to 0 to scan all pages.
                                item.SetPageCount(0);
                            }

                            item.SetColour(WiaColour.Colour);
                            var actualDpi = item.TrySetDpi(300);
                            item.SetAutoCrop(WiaAutoCrop.Single);
                            item.SetAutoDeskewEnabled(true);
                            ////item.SetPageSize(WiaPageSize.A4);

                            this.Log($"Scanning at {actualDpi}");

                            // Set up the scan
                            using (var transfer = item.StartTransfer())
                            {
                                try
                                {
                                    transfer.PageScanned += this.WiaTransferPageScanned;
                                    transfer.Progress += this.WiaTransferProgressChanged;
                                    transfer.TransferComplete += this.WiaTransferComplete;

                                    // Do the actual scan
                                    transfer.Download();
                                }
                                finally
                                {
                                    transfer.PageScanned -= this.WiaTransferPageScanned;
                                    transfer.Progress -= this.WiaTransferProgressChanged;
                                    transfer.TransferComplete -= this.WiaTransferComplete;
                                }
                            }
                        }
                    }
                }
            }
            catch (ArgumentException badProperty)
            {
                this.Log(badProperty.Message);
            }
            catch (WiaException error)
            {
                this.Log(error.Message);
            }
        }

        private void WiaTransferPageScanned(object sender, WiaTransfer.PageScannedEventArgs e)
        {
            using (e.Stream)
            {
                var bitmap = new Bitmap(e.Stream);
                this.AddPageFromBitmap(bitmap);
            }
        }

        private void WiaTransferProgressChanged(object sender, WiaTransfer.ProgressEventArgs e)
        {
            Dispatcher.BeginInvoke(
                  new Action<int?>(this.SetProgress),
                  (int?)e.Percent);
        }

        private void WiaTransferComplete(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(
                  new Action<int?>(this.SetProgress),
                  (int?)null);
        }

        private void WiaAcquisitionCompleted(object sender, EventArgs e)
        {
            this.Log("WiaAcquisitionCompleted");
        }

        private void WiaItemAcquired(object sender, EventArgs e)
        {
            this.Log("WiaItemAcquired:" + e.ToString());
        }

        public void SetProgress(int? progress)
        {
            if (progress.HasValue)
            {
                this.IsScanning = true;
                this.ScanProgress = progress.Value;
            }
            else
            {
                this.IsScanning = false;
            }
        }

        public void Log(string message)
        {
            Dispatcher.BeginInvoke(
                new Action<string>(this.Logs.Add),
                DispatcherPriority.Background,
                message);
        }

        public void AddPageFromBitmap(Bitmap bitmap)
        {
            Dispatcher.BeginInvoke(
                new Action<Bitmap>(
                    bmp =>
                    {
                        // To convert to WPF Image Source when adding from a Bitmap
                        var source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                            bmp.GetHbitmap(),
                            IntPtr.Zero,
                            Int32Rect.Empty,
                            BitmapSizeOptions.FromWidthAndHeight(bmp.Width, bmp.Height));
                        this.Log($"Page Added From Bitmap {bmp.Width}x{bmp.Height} ({bmp.RawFormat})");

                        // Example: Loading from a file, BitmapImage.UriSource must be in a BeginInit/EndInit block.
                        ////BitmapImage source = new BitmapImage();
                        ////source.BeginInit();
                        ////source.UriSource = new Uri(fileName, UriKind.RelativeOrAbsolute);
                        ////source.EndInit();
                        this.Pages.Add(source);
                    }),
                    DispatcherPriority.Background,
                    bitmap);
        }

        private void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Selection object for device id, name and what it can do.
        /// </summary>
        public sealed class WiaDeviceInfo
        {
            public WiaDeviceInfo(string id, string name, IEnumerable<string> supportedPaperSources)
            {
                this.Id = id;
                this.Name = name;
                foreach (var source in supportedPaperSources)
                {
                    this.Sources.Add(source);
                }
            }

            public string Id { get; }

            public string Name { get; }

            public List<string> Sources { get; } = new List<string>();
        }
    }
}
