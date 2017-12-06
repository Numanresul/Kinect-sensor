using System;
using System.Collections.Generic;
using System.Linq;
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
using Microsoft.Kinect;

namespace Kinect104_Depth
{
   
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        // Her bir noktanın derinlik değerini tutacak short dizisi:
        private short[] depthPixelData;

        // Image elementinin kaynağı olarak atanacak görüntü:
        private WriteableBitmap outputImage;

     
        private DepthImageFormat lastDepthFormat = DepthImageFormat.Undefined;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Kinect bağlıysa, belirtilen parametrelere göre ilk Kinect'i yapılandır:
            if (KinectSensor.KinectSensors.Count > 0)
            {
                // Sisteme bağlı Kinect'lere, KinectSensors dizisinden ulaşıyoruz.
                // İlk Kinect'i başlat:
                KinectSensor.KinectSensors[0].Start();

                // Derinlik bilgisi akışını 640x480 30fps biçiminde başlat:
                KinectSensor.KinectSensors[0].DepthStream.Enable(
                    DepthImageFormat.Resolution640x480Fps30);

                // Yeni görüntü geldiğinde tetiklenecek event handler'ı oluştur:
                KinectSensor.KinectSensors[0].DepthFrameReady
                    += new EventHandler<DepthImageFrameReadyEventArgs>(Kinect_DepthFrameReady);
            }
        }

        private void Kinect_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            // Derinlik bilgisini açıp depthFrame'e aktar:
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                // Derinlik bilgisi boş değilse işle:
                if (depthFrame != null)
                {
                    // Önceki derinlik biçimi ile yeni gelen derinlik biçimi farklı mı?
                    bool haveNewFormat = lastDepthFormat != depthFrame.Format;

                    // Derinlik biçimi değiştiyse:
                    if (haveNewFormat)
                    {
                        // Yeni derinlik bilgisi boyutuna göre diziyi boyutlandır:
                        depthPixelData = new short[depthFrame.PixelDataLength];
                    }

                    // Derinlik bilgisini depthPixelData dizisine aktar:
                    depthFrame.CopyPixelDataTo(depthPixelData);

                    // Derinlik biçimi değiştiyse:
                    if (haveNewFormat)
                    {
                        // outputImage'ın özelliklerini değiştir:
                        outputImage = new WriteableBitmap(
                            depthFrame.Width, // Genişlik
                            depthFrame.Height, // Yükseklik
                            96, // Yatay DPI
                            96, // Dikey DPI
                            PixelFormats.Gray16, // Piksel biçimi
                            null); // Bitmap paleti

                        // Image elementinin kaynağını outputImage olarak belirle:
                        imgDepthFrame.Source = outputImage;
                    }

                    // outputImage'ın içeriğini güncelle:
                    outputImage.WritePixels(
                        new Int32Rect(0, 0, depthFrame.Width, depthFrame.Height),
                        depthPixelData,
                        depthFrame.Width * depthFrame.BytesPerPixel,
                        0);

                    // Bir sonraki derinlik biçimi karşılaştırması için, yeni derinlik
                    // biçimini kaydet:
                    lastDepthFormat = depthFrame.Format;
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Kinect(ler) sisteme bağlıysa:
            if (KinectSensor.KinectSensors.Count > 0)
            {
                // Tüm Kinect'leri durdur:
                foreach (KinectSensor sensor in KinectSensor.KinectSensors)
                    sensor.Stop();
            }
        }
    }
}
