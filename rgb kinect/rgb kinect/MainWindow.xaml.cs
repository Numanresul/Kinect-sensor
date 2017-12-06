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

namespace Kinect102_RGB
{
    
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        // Her bir noktanın renk değerlerini tutacak byte dizisi:
        private byte[] pixelData;

        // Image elementinin kaynağı olarak atanacak görüntü:
        private WriteableBitmap outputImage;

        // Görüntü biçimi değiştirilirse, pixelData ve outputImage boyutlarını da
        // değiştirmemiz gerekiyor. Performans açısından, her frame için boyutları
        // yeniden belirlemek yerine, son görüntü biçimiyle yeni görüntü biçimini
        // karşılaştırıyoruz. Yalnızca aralarında fark varsa boyutları değiştiriyoruz.

        // Karşılaştırmayı yapacağımız görüntü biçimi:
        private ColorImageFormat lastImageFormat = ColorImageFormat.Undefined;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            #region Kinect Init

            // Kinect bağlıysa, belirtilen parametrelere göre ilk Kinect'i yapılandır:
            if (KinectSensor.KinectSensors.Count > 0)
            {
                // Sisteme bağlı Kinect'lere, KinectSensors dizisinden ulaşıyoruz.
                // İlk Kinect'i başlat:
                KinectSensor.KinectSensors[0].Start();

                // Kinect'in renkli görüntü akışını RGB 640x480 30fps biçiminde başlat:
                KinectSensor.KinectSensors[0].ColorStream.Enable
                    (ColorImageFormat.RgbResolution640x480Fps30);
                // Görüntü biçimini değiştirmek için, Color Image Format parametresini
                // değiştirebilirsiniz.

                // Yeni görüntü geldiğinde tetiklenecek eventi oluştur:
                KinectSensor.KinectSensors[0].ColorFrameReady
                    += new EventHandler<ColorImageFrameReadyEventArgs>(Kinect_ColorFrameReady);
            }

            #endregion
        }

        private void Kinect_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            // Gelen görüntü bilgisini açıp imageFrame'e aktar:
            using (ColorImageFrame imageFrame = e.OpenColorImageFrame())
            {
                // Görüntü varsa:
                if (imageFrame != null)
                {
                    // Önceki görüntü biçimi ile yeni gelen görüntü biçimi farklı mı?
                    bool haveNewFormat = lastImageFormat != imageFrame.Format;

                    // Görüntü biçimi değiştiyse:
                    if (haveNewFormat)
                    {
                        // Yeni görüntü boyutuna göre pixelData dizisini boyutlandır:
                        pixelData = new byte[imageFrame.PixelDataLength];
                    }

                    // Görüntüyü pixelData dizisine aktar:
                    imageFrame.CopyPixelDataTo(pixelData);

                    // Görüntü biçimi değiştiyse:
                    if (haveNewFormat)
                    {
                        // Image elementinin kaynağı olarak gösterilen outputImage'ın
                        // özelliklerini değiştir:
                        outputImage = new WriteableBitmap(
                            imageFrame.Width,  // Genişlik
                            imageFrame.Height, // Yükseklik
                            96,  // Yatay DPI
                            96,  // Dikey DPI
                            PixelFormats.Bgr32, // Piksel biçimi
                            null); // Bitmap paleti

                        // Image elementinin kaynağını outputImage olarak belirle:
                        imgColorFrame.Source = outputImage;
                    }

                    // outputImage'ın içeriğini güncelle:
                    outputImage.WritePixels(
                        new Int32Rect(0, 0, imageFrame.Width, imageFrame.Height),
                        pixelData,
                        imageFrame.Width * 4,
                        0);

                    // Bir sonraki görüntü biçimi karşılaştırması için, yeni görüntü
                    // biçimini kaydet:
                    lastImageFormat = imageFrame.Format;
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
