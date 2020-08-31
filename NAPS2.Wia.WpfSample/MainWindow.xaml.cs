using System;
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
        private string currentDevice;
        private string currentPaperSource;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            this.Devices.Clear();

            using (var deviceManager = new WiaDeviceManager())
            {
                foreach (var device in deviceManager.GetDeviceInfos())
                {
                    using (device)
                    {
                        this.Devices.Add(device.Id());
                    }
                }
            }

            this.CurrentDevice = this.Devices.FirstOrDefault();
        }

        public ObservableCollection<string> Devices { get; } = new ObservableCollection<string>();

        public ObservableCollection<string> PaperSources { get; } = new ObservableCollection<string>();

        public ObservableCollection<ImageSource> Pages { get; } = new ObservableCollection<ImageSource>();

        public string CurrentDevice
        {
            get => this.currentDevice;
            set
            {
                if (this.currentDevice != value)
                {
                    this.currentDevice = value;
                    this.RefreshDeviceCapabilities(value);
                    this.RaisePropertyChanged();
                }
            }
        }

        private void RefreshDeviceCapabilities(string deviceId)
        {
            this.PaperSources.Clear();
            using (var deviceManager = new WiaDeviceManager())
            {
                using (var device = deviceManager.FindDevice(deviceId))
                {
                    if (device.SupportsFlatbed())
                    {
                        this.PaperSources.Add("Flatbed");
                    }

                    if (device.SupportsFeeder())
                    {
                        this.PaperSources.Add("Feeder");
                        if (device.SupportsDuplex())
                        {
                            this.PaperSources.Add("FeederDuplex");
                        }
                    }
                }
            }
        }

        public string CurrentPaperSource
        {
            get => this.currentPaperSource;
            set
            {
                if (this.currentPaperSource != value)
                {
                    this.currentPaperSource = value;
                }
            }
        }

        public ObservableCollection<string> Logs { get; } = new ObservableCollection<string>();

        private void ScanClicked(object sender, RoutedEventArgs e)
        {
            this.Pages.Clear();
            this.Log("Starting WIA Scan");
            Task.Run(() =>
            {
                try
                {
                    using (var deviceManager = new WiaDeviceManager())
                    {
                        using (var device = deviceManager.FindDevice(this.CurrentDevice))
                        {
                            var subItem = this.CurrentPaperSource == "Flatbed" ? "Flatbed" : "Feeder";

                            // Select between Flatbed/Feeder
                            using (var item = device.FindSubItem(subItem)) {

                                if (this.CurrentPaperSource == "FeederDuplex")
                                {
                                    // Enable duplex scanning
                                    item.SetProperty(WiaPropertyId.IPS_DOCUMENT_HANDLING_SELECT,
                                                     WiaPropertyValue.DUPLEX | WiaPropertyValue.FRONT_FIRST);
                                }

                                // Set up the scan
                                using (var transfer = item.StartTransfer())
                                {
                                    transfer.PageScanned += (s, args) =>
                                    {
                                        using (args.Stream)
                                        {
                                            var bitmap = new Bitmap(args.Stream);
                                            this.AddPageFromBitmap(bitmap);
                                        }
                                    };

                                    // Do the actual scan
                                    transfer.Download();
                                }
                            }
                        }
                    }
                }
                catch (Exception error)
                {
                    this.Log(error.Message);
                }
            });
        }

        private void WiaAcquisitionCompleted(object sender, EventArgs e)
        {
            this.Log("WiaAcquisitionCompleted");
        }

        private void WiaItemAcquired(object sender, EventArgs e)
        {
            this.Log("WiaItemAcquired:" + e.ToString());
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
    }
}
