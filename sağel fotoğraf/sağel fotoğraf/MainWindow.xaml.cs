using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Kinect;

namespace KinectFotograf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private Skeleton[] skeletons = new Skeleton[6];
        private Skeleton skeleton;
        private byte[] pixelData;
        private WriteableBitmap outputImage;
        private ColorImageFormat lastImageFormat = ColorImageFormat.Undefined;
        private bool savePictureFrame = false;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Sisteme Kinect bağlıysa, ilk Kinect'i yapılandır
            if (KinectSensor.KinectSensors.Count > 0)
            {
                KinectSensor.KinectSensors[0].Start();

                // Renkli görüntü
                KinectSensor.KinectSensors[0].ColorStream.Enable
                    (ColorImageFormat.RgbResolution1280x960Fps12);
                KinectSensor.KinectSensors[0].ColorFrameReady
                    += new EventHandler<ColorImageFrameReadyEventArgs>(Kinect_ColorFrameReady);

                // İskelet verisi
                TransformSmoothParameters tsp = new TransformSmoothParameters();
                tsp.Smoothing = 0.55f;
                tsp.Correction = 0.1f;
                tsp.Prediction = 0.1f;
                tsp.JitterRadius = 0.4f;
                tsp.MaxDeviationRadius = 0.4f;

                KinectSensor.KinectSensors[0].SkeletonStream.Enable(tsp);
                KinectSensor.KinectSensors[0].SkeletonFrameReady
                    += new EventHandler<SkeletonFrameReadyEventArgs>(Kinect_SkeletonFrameReady);
            }
        }

        private void Kinect_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            // Gelen görüntü bilgisini açıp imageFrame'e aktar
            using (ColorImageFrame imageFrame = e.OpenColorImageFrame())
            {
                // Görüntü varsa...
                if (imageFrame != null)
                {
                    // Önceki görüntü biçimi ile yeni gelen görüntü biçimi farklı mı?
                    bool haveNewFormat = lastImageFormat != imageFrame.Format;

                    // Görüntü biçimi değiştiyse...
                    if (haveNewFormat)
                    {
                        // Yeni görüntü boyutuna göre pixelData dizisini boyutlandır
                        pixelData = new byte[imageFrame.PixelDataLength];
                    }

                    // Görüntüyü pixelData dizisine aktar
                    imageFrame.CopyPixelDataTo(pixelData);

                    // Görüntü biçimi değiştiyse...
                    if (haveNewFormat)
                    {
                        // Image elementinin kaynağı olarak gösterilen outputImage'ın
                        // özelliklerini değiştir
                        outputImage = new WriteableBitmap(
                            imageFrame.Width,
                            imageFrame.Height,
                            96,
                            96,
                            PixelFormats.Bgr32,
                            null);

                        // Image elementinin kaynağını outputImage olarak belirle
                        imgColorFrame.Source = outputImage;
                    }

                    // outputImage'ın içeriğini güncelle
                    outputImage.WritePixels(
                        new Int32Rect(0, 0, imageFrame.Width, imageFrame.Height),
                        pixelData,
                        imageFrame.Width * 4,
                        0);

                    // (İstenmişse) görüntüyü kaydet...
                    if (savePictureFrame)
                    {
                        // Görüntüyü kaydet
                        SavePicture();

                        // Tetikleyici değişkeni pasifleştir
                        savePictureFrame = false;
                    }

                    // Bir sonraki görüntü biçimi karşılaştırması için, yeni görüntü
                    // biçimini kaydet
                    lastImageFormat = imageFrame.Format;
                }
            }
        }

        private void Kinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            try
            {
                // Gelen iskelet verisini skeletons dizisine kopyala
                if (e.OpenSkeletonFrame() != null) e.OpenSkeletonFrame().CopySkeletonDataTo(skeletons);

                // İlk algılanan iskeleti seç
                skeleton = (from s in skeletons
                            where s.TrackingState == SkeletonTrackingState.Tracked
                            select s).FirstOrDefault();
            }
            catch (Exception)
            {
                // Hata oluşursa skeleton'un parametrelerini temizle.
                // Bu frame'de veri işlemesi yapmayacağız.
                skeleton = null;
            }

            // İskelet algılanmışsa...
            if (skeleton != null)
            {
                // NUI Checkbox'ı seçiliyse ve
                // Sağ elin dikey pozisyonu, baş seviyesinden yukarıdaysa...
                if (chkNui.IsChecked == true &&
                    skeleton.Joints[JointType.HandRight].Position.Y >
                    skeleton.Joints[JointType.Head].Position.Y)
                {
                    // Tek fotoğraf çekilmesi için NUI Checkbox'ın seçili özelliğini kaldır
                    chkNui.IsChecked = false;

                    // Fotoğraf çekmek için tetikleyici değişkeni etkinleştir
                    savePictureFrame = true;
                }
            }
        }

        private void SavePicture()
        {
            // Görüntü dosyasını diske yazacak FileStream oluştur
            using (FileStream stream = new FileStream(@"D:\Temp\KinectPicture.bmp", FileMode.Create))
            {
                // Görüntüyü farklı biçimlerde kaydedebiliyoruz. (BMP, JPEG, PNG...)
                // Kaydedilmek istenen görüntü türüne göre Bitmap Encoder oluştur
                BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                //JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                //PngBitmapEncoder encoder = new PngBitmapEncoder();

                // outputImage'ı Bitmap olarak çöz ve encoder'a aktar
                encoder.Frames.Add(BitmapFrame.Create(outputImage));

                // Görüntü dosyasını kaydet
                encoder.Save(stream);

                // FileStream'i sonlandır
                stream.Close();
            }
        }

        private void btnSnapshot_Click(object sender, RoutedEventArgs e)
        {
            // Fotoğraf çekmek için tetikleyici değişkeni etkinleştir
            savePictureFrame = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Uygulama kapanırken tüm Kinect'leri durdur...
            if (KinectSensor.KinectSensors.Count > 0)
            {
                foreach (KinectSensor sensor in KinectSensor.KinectSensors)
                    sensor.Stop();
            }
        }
    }
}