using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace eratter
{
    class Console : Form
    {
        private Graphics graphics;
        private IntPtr hDC;

        [DllImport("gdi32.dll", EntryPoint = "TextOut")]
        private static extern bool TextOut(IntPtr hdc, int nXStart, int nYStart, string lpString, int cbString);

        [DllImport("gdi32.dll", EntryPoint = "SetTextColor")]
        private static extern uint SetTextColor(IntPtr hdc, int crColor);

        [DllImport("gdi32.dll", EntryPoint = "SetBkColor")]
        private static extern int SetBkColor(IntPtr hdc, int crColor);

        [DllImport("gdi32.dll", EntryPoint = "SelectObject")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteObject(IntPtr hObject);

        public Console()
        {
            BackColor = Color.Black;
            graphics = CreateGraphics();
            hDC = graphics.GetHdc();
            Font = new Font("ＭＳ ゴシック", 12);

            SetTextColor(hDC, ColorTranslator.ToWin32(Color.White));
            SetBkColor(hDC, ColorTranslator.ToWin32(Color.Black));

            MessageOut("これはテストです。\n");
            Click += new EventHandler(ConsoleClickEx);
        }

        ~Console()
        {
            graphics.ReleaseHdc(hDC);
            graphics.Dispose();
        }

        public void MessageOut(string message)
        {
            IntPtr hFont = Font.ToHfont();
            
            IntPtr hOldFont = SelectObject(hDC, hFont);

            TextOut(hDC, testNum * 16, 0, message, message.Length);

            DeleteObject(SelectObject(hDC, hOldFont));
        }

        private int testNum = 0;
        private void ConsoleClickEx(object sender, EventArgs e)
        {
            MessageOut("テスト" + testNum);
            ++testNum;
        }
    }
}
