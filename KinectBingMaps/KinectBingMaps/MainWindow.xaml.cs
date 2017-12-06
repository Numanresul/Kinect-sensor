using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Kinect;
using Microsoft.Maps.MapControl.WPF;

namespace KinectBingMaps
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

        KinectSensor kinect;
        Skeleton[] skeletons = new Skeleton[6];

        ImlecKontrol imlec = new ImlecKontrol();

        int ekranGenisligi = (int)System.Windows.SystemParameters.PrimaryScreenWidth;
        int ekranYuksekligi = (int)System.Windows.SystemParameters.PrimaryScreenHeight;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            harita.Mode = new AerialMode();

            // Kinect bağlıysa, belirtilen parametrelere göre başlat
            if (KinectSensor.KinectSensors.Count > 0)
            {
                kinect = KinectSensor.KinectSensors[0];

                // Hareket yumuşatma parametreleri
                TransformSmoothParameters tsp = new TransformSmoothParameters();
                tsp.Smoothing = 0.55f;
                tsp.Correction = 0.1f;
                tsp.Prediction = 0.1f;
                tsp.JitterRadius = 0.4f;
                tsp.MaxDeviationRadius = 0.4f;

                // Sisteme bağlı ilk Kinect'i başlat
                kinect.Start();

                // Derinlik algılamasında yakın modu etkinleştir
                kinect.DepthStream.Range = DepthRange.Near;

                // Derinlik algılaması yakın moddayken iskelet algılamasını etkinleştir
                kinect.SkeletonStream.EnableTrackingInNearRange = true;

                // İskelet algılamasında oturma modunu kullan
                kinect.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;

                // Derinlik akışını başlat (Yakın modun aktif olabilmesi için gerekli)
              //  kinect.DepthStream.Enable();

                // İskelet verisi akışını belirtilen hareket yumuşatma parametrelerine göre başlat
                kinect.SkeletonStream.Enable(tsp);

                // İskelet verisi güncellendiğinde tetiklenecek event handler'ı oluştur
                kinect.SkeletonFrameReady +=
                    new EventHandler<SkeletonFrameReadyEventArgs>(Kinect_SkeletonFrameReady);
            }
        }

        private void Kinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton iskelet;

            try
            {
                // İskelet verisini al
                if (e.OpenSkeletonFrame() != null)
                    e.OpenSkeletonFrame().CopySkeletonDataTo(skeletons);

                // İlk algılanan iskeleti seç
                iskelet = (from s in skeletons
                           where s.TrackingState == SkeletonTrackingState.Tracked
                           select s).FirstOrDefault();
            }
            catch (Exception)
            {
                iskelet = null;
            }

            // İskelet algılanmışsa veriyi işle
            if (iskelet != null)
            {
                // Kullanılacak eklem bilgilerini, HaritaKontrol metoduna aktar
                HaritaKontrol(iskelet.Joints[JointType.HandRight],
                    iskelet.Joints[JointType.HandLeft],
                    iskelet.Joints[JointType.ShoulderCenter]);
            }
        }

        private void HaritaKontrol(Joint sagEl, Joint solEl, Joint boyun)
        {
            // Mod seçimi:
            // Her iki el de ilerideyse, zoom moduna gir
            // Boyun ile elin Z pozisyonu farkı, o elin kaç cm uzatıldığını verir.
            // 0.45 = 45cm değerini değiştirerek zoom modu uzaklığını ayarlayabilirsiniz.
            if (boyun.Position.Z - sagEl.Position.Z > 0.45 &&
                boyun.Position.Z - solEl.Position.Z > 0.45)
            {
                // Eller arası yatay farka göre zoom oranını belirle
                double zoom = (sagEl.Position.X - solEl.Position.X) * 15;

                // En yüksek ve düşük zoom sınırlaması
                if (zoom > 15) zoom = 15;
                else if (zoom < 5) zoom = 5;

                // Zoom seviyesini haritaya gönder
                harita.ZoomLevel = zoom;
            }

            // Her iki el ileride değilse, imleç kontrol moduna gir
            else
            {
                // İmleç pozisyonunu boyun ile eller arasındaki mesafeye göre belirle
                int imlecX = (int)((sagEl.Position.X - boyun.Position.X) * 3000);
                int imlecY = (int)((boyun.Position.Y - sagEl.Position.Y) * 3000);

                // İmlecin ekran çözünürlüğü dışına çıkmaması için sınırlama
                if (imlecX < 0) imlecX = 0;
                else if (imlecX > ekranGenisligi) imlecX = ekranGenisligi;
                if (imlecY < 0) imlecY = 0;
                else if (imlecY > ekranYuksekligi) imlecY = ekranYuksekligi;

                // İmleci pozisyonlandır
                imlec.Git(imlecX, imlecY);

                // İmleç tıklama ve bırakma seçimi:
                // Sağ el ilerideyse, haritayı oynatmak için tıkla ve imleci basılı tut.
                // Kaydırma için el uzaklığı mesafesini 0.45'i değiştirerek ayarlayabilirsiniz.
                if (boyun.Position.Z - sagEl.Position.Z > 0.45)
                {
                    imlec.SolBas();
                }

                // Sağ el normal uzaklıktaysa tıklamayı serbest bırak.
                // Bu durumda, imleç serbest hareket eder.
                else
                {
                    imlec.SolBirak();
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Uygulama kapanırken, Kinect çalışıyorsa Kinect'i durdur
            if (kinect.IsRunning)
                kinect.Stop();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Escape tuşuna basıldığında uygulamayı sonlandır
            if (e.Key == Key.Escape) this.Close();
        }
    }
}