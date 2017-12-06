using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;

namespace KinectBingMaps
{
    class ImlecKontrol
    {
        private bool solButonDurumu = false;

        [DllImport("user32.dll")]
        private static extern void mouse_event(
            UInt32 dwFlags, UInt32 dx, UInt32 dy, UInt32 dwData, IntPtr dwExtraInfo);
        private const UInt32 MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const UInt32 MOUSEEVENTF_LEFTUP = 0x0004;
        private const UInt32 MOUSEEVENTF_WHEEL = 0x0800;

        private void LeftButtonUp()
        {
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, new System.IntPtr());
        }

        private void LeftButtonDown()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, new System.IntPtr());
        }

        private void TurnMouseWheel(UInt32 amount)
        {
            mouse_event(MOUSEEVENTF_WHEEL, 0, 0, amount, new System.IntPtr());
        }

        /// <summary>
        /// İmleci belirtilen pozisyona götürür.
        /// </summary>
        /// <param name="x">X Pozisyonu</param>
        /// <param name="y">Y Pozisyonu</param>
        public void Git(int x, int y)
        {
            Cursor.Position = new Point(x, y);
        }

        /// <summary>
        /// İmleci belirtilen pozisyona götürür.
        /// </summary>
        /// <param name="p">Pozisyon</param>
        public void Git(Point p)
        {
            Cursor.Position = p;
        }

        /// <summary>
        /// İmlecin bulunduğu noktaya sol tıklar ve bırakır.
        /// </summary>
        public void SolTikla()
        {
            LeftButtonDown();
            LeftButtonUp();
        }

        /// <summary>
        /// İmlecin bulunduğu noktaya çift tıklar.
        /// </summary>
        public void CiftTikla()
        {
            SolTikla();
            SolTikla();
        }

        /// <summary>
        /// İmlecin bulunduğu noktaya sol tıklar ve basılı bırakır.
        /// </summary>
        public void SolBas()
        {
            if (!solButonDurumu)
            {
                solButonDurumu = true;
                LeftButtonDown();
            }
        }

        /// <summary>
        /// Sol butonu serbest bırakır.
        /// </summary>
        public void SolBirak()
        {
            if (solButonDurumu)
            {
                solButonDurumu = false;
                LeftButtonUp();
            }
        }

        /// <summary>
        /// Fare tekerleğinin kaydırma işlemini gerçekleştirir.
        /// </summary>
        /// <param name="miktar">Dönüş miktarı</param>
        public void Kaydir(UInt32 miktar)
        {
            TurnMouseWheel(miktar);
        }
    }
}