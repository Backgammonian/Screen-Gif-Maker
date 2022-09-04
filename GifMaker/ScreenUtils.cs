using System;
using System.Runtime.InteropServices;
using System.Drawing;

namespace GifMaker
{
    public static class ScreenUtils
    {
        [DllImport("User32.dll")]
        private static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("User32.dll")]
        private static extern int ReleaseDC(IntPtr hwnd, IntPtr dc);

        [DllImport("gdi32.dll")]
        private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        public static Size GetScreenSize()
        {
            try
            {
                IntPtr primary = GetDC(IntPtr.Zero);
                int DESKTOPVERTRES = 117;
                int DESKTOPHORZRES = 118;
                int actualPixelsX = GetDeviceCaps(primary, DESKTOPHORZRES);
                int actualPixelsY = GetDeviceCaps(primary, DESKTOPVERTRES);
                ReleaseDC(IntPtr.Zero, primary);

                return new Size(actualPixelsX, actualPixelsY);
            }
            catch (Exception)
            {
                return new Size(800, 600);
            }
        }
    }
}
