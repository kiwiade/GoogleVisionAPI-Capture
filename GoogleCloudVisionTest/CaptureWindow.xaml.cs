using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using System.Drawing;
using System.Windows.Interop;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace GoogleCloudVisionTest
{
    /// <summary>
    /// CaptureWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CaptureWindow : Window
    {
        public CaptureWindow()
        {
            InitializeComponent();
        }
         
        // Grid에서 왼쪽클릭하면 window를 움직일수 있게 함
        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            this.MouseLeftButtonDown += delegate { DragMove(); };
        }

        // 닫기 버튼 누르면 창이닫힘
        private void close_button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        // 캡쳐버튼 누르면 CropCapture_Window의 사이즈만큼 bitmap을 만듬
        private double borderthickness = 5;
        private void capture_button_Click(object sender, RoutedEventArgs e)
        {
            // 듀얼 모니터까지 다합쳐서 길이
            //double screenLeft = SystemParameters.VirtualScreenLeft;
            //double screenTop = SystemParameters.VirtualScreenTop;
            //double screenWidth = SystemParameters.VirtualScreenWidth;
            //double screenHeight = SystemParameters.VirtualScreenHeight;

            // 현재 모니터의 크기
            //double screenWidth = SystemParameters.WorkArea.Width;
            //double screenHeight = SystemParameters.WorkArea.Height;
            double screenWidth = CropCapture_Window.ActualWidth;
            double screenHeight = CropCapture_Window.ActualHeight;

            // 시작점의 위치를 잡는다. (0,0)부터의 위치를 받아와서 두께만큼 더해줌.
            var startPoint = CropCapture_Window.PointToScreen(new System.Windows.Point(0, 0));
            startPoint.X += borderthickness;
            startPoint.Y += borderthickness;

            // Bitmap을 캡쳐창의 크기만큼 생성함(테두리 두께만큼 제외. 테두리가 찍히는것을 방지하기위해)
            using (Bitmap bmp = new Bitmap((int)(screenWidth - borderthickness * 2),
                (int)(screenHeight - borderthickness * 2)))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    String filename = "CropScreenCapture-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".png";
                    Opacity = .0;
                    //g.CopyFromScreen((int)screenLeft, (int)screenTop, 0, 0, bmp.Size);
                    // 시작점부터 bitmap의 (0,0)부터 그리기 시작함.
                    g.CopyFromScreen((int)startPoint.X, (int)startPoint.Y, 0, 0, bmp.Size);
                    bmp.Save("D:\\" + filename);
                    Opacity = 1;

                    MessageBox.Show("캡쳐가 저장되었습니다.\n" + "D:\\" + filename);
                }
            }
        }

        private void all_capture_button_Click(object sender, RoutedEventArgs e)
        {
            // 듀얼 모니터까지 다합쳐서 길이
            //double screenTop = SystemParameters.VirtualScreenTop;
            //double screenLeft = SystemParameters.VirtualScreenLeft;
            //double screenWidth = SystemParameters.VirtualScreenWidth;
            //double screenHeight = SystemParameters.VirtualScreenHeight;

            // 현재 모니터의 크기(workarea라서 작업표시줄 제외)
            //double screenWidth = SystemParameters.WorkArea.Width;
            //double screenHeight = SystemParameters.WorkArea.Height;
            //double screenTop = SystemParameters.WorkArea.Top;
            //double screenLeft = SystemParameters.WorkArea.Left;

            // 현재 모니터의 크기
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double screenTop = SystemParameters.WorkArea.Top;
            double screenLeft = SystemParameters.WorkArea.Left;

            // Bitmap을 캡쳐창의 크기만큼 생성함(테두리 두께만큼 제외. 테두리가 찍히는것을 방지하기위해)
            using (Bitmap bmp = new Bitmap((int)screenWidth,
                (int)screenHeight))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    String filename = "ScreenCapture-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".png";
                    Opacity = .0;
                    g.CopyFromScreen((int)screenLeft, (int)screenTop, 0, 0, bmp.Size);
                    // 시작점부터 bitmap의 (0,0)부터 그리기 시작함.
                    bmp.Save("D:\\" + filename);
                    Opacity = 1;

                    MessageBox.Show("캡쳐가 저장되었습니다.\n" + "D:\\" + filename);
                }
            }
        }

        /// ///////////////////////////////////////////////////////////////////////////

        static class NativeMethods
        {
            public const int DstInvert = 0x00550009;

            // 해당위치에 있는 윈도우를 IntPtr로 받아옴
            [DllImport("user32.dll")]
            public static extern IntPtr WindowFromPoint(POINT point);

            [DllImport("user32.dll")]
            public static extern IntPtr GetParent(IntPtr hWnd);

            // 커서위치를 받아오는 함수
            [DllImport("user32.dll")]
            public static extern bool GetCursorPos(ref POINT lpPoint);
            
            // 해당 윈도우의 이름을 받아온다.
            [DllImport("user32.dll")]
            public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

            // 포인터를 이용해 rect를 받아옴
            [DllImport("user32.dll")]
            public static extern bool GetWindowRect(IntPtr hwnd, out MyRect lpRect);

            // rect의 offset을 이동시킬때 사용. 
            [DllImport("user32.dll")]
            public static extern bool OffsetRect(ref MyRect lprc, int dx, int dy);

            // 포인터가 있는 윈도우의 dc(device context)를 받아옴. 그림을 그리기위해서
            [DllImport("user32.dll")]
            internal static extern IntPtr GetWindowDC(IntPtr ptr);

            // 가져온 DC를 해제한다
            [DllImport("user32.dll")]
            internal static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDc);

            // 지정된 사각형 영역을 선택한 브러시로 채움.
            [DllImport("gdi32.dll")]
            internal static extern bool PatBlt(IntPtr hdc, int nXLeft, int nYLeft, int nWidth, int nHeight, uint dwRop);

            // 윈도우의 일부분을 무효화 시켜주는 함수.
            [DllImport("user32.dll")]
            internal static extern bool InvalidateRect(IntPtr hWnd, IntPtr rect, bool bErase);

            // 윈도우의 무효화영역을 갱신함.
            [DllImport("user32.dll")]
            internal static extern bool UpdateWindow(IntPtr hWnd);

        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;

            public POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MyRect
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner

            public Int32Rect ToRectangle()
            {
                return new Int32Rect(Left, Top, Right - Left, Bottom - Top);
            }

            public System.Windows.Rect ToRect(double offset = 0, double scale = 1d)
            {
                return new System.Windows.Rect((Left - offset) / scale, (Top - offset) / scale, (Right - Left + offset * 2) / scale, (Bottom - Top + offset * 2) / scale);
            }
        }

        // 해당위치에 있는 윈도우를 받아옴
        public static IntPtr GetWindowFromPoint(System.Drawing.Point point)
        {
            var hwnd = NativeMethods.WindowFromPoint(new POINT((int)point.X, (int)point.Y));
            if (hwnd == IntPtr.Zero) return IntPtr.Zero;
            var p = NativeMethods.GetParent(hwnd);
            //while (p != IntPtr.Zero)
            //{
            //    hwnd = p;
            //    p = NativeMethods.GetParent(hwnd);
            //}
            return hwnd;
           
            //foreach (Window w in Application.Current.Windows)
            //{
            //    if (w.IsVisible)
            //    {
            //        var helper = new WindowInteropHelper(w);

            //        if (helper.Handle == hwnd) return w;
            //    }
            //}
            //return null;
        }

        // 커서위치를 받아와서 해당위치에있는 윈도우를 받아옴
        public static IntPtr GetWindowFromMousePosition()
        {
            POINT p = new POINT();
            NativeMethods.GetCursorPos(ref p);
            return GetWindowFromPoint(new System.Drawing.Point(p.x, p.y));
        }

        // 주변을 그려주는 함수
        public void DrawFrame(IntPtr hWnd, int frameWidth, double scale = 1d)
        {
            //TODO: Adjust for high DPI.
            if (hWnd == IntPtr.Zero)
                return;           

            // 그림을 그리기위해서 DC를 받아옴
            var hdc = NativeMethods.GetWindowDC(hWnd); //GetWindowDC((IntPtr) null); //

            // 현재 커서아래에 있는 포인터를 이용해 해당 rect를 받아옴
            NativeMethods.GetWindowRect(hWnd, out MyRect rect);

            //DwmGetWindowAttribute(hWnd, (int)DwmWindowAttribute.DwmwaExtendedFrameBounds, out rect, Marshal.SizeOf(typeof(Rect)));
            // rect의 offset을 이동시킬때 사용. 
            NativeMethods.OffsetRect(ref rect, -rect.Left, -rect.Top);

            // 지정된 사각형 영역을 선택한 브러시로 채움.(흰색으로 테두리를 만들어준다)
            NativeMethods.PatBlt(hdc, rect.Left, rect.Top, rect.Right - rect.Left, frameWidth, NativeMethods.DstInvert);

            NativeMethods.PatBlt(hdc, rect.Left, rect.Bottom - frameWidth, frameWidth, -(rect.Bottom - rect.Top - 2 * frameWidth), NativeMethods.DstInvert);

            NativeMethods.PatBlt(hdc, rect.Right - frameWidth, rect.Top + frameWidth, frameWidth, rect.Bottom - rect.Top - 2 * frameWidth, NativeMethods.DstInvert);

            NativeMethods.PatBlt(hdc, rect.Right, rect.Bottom - frameWidth, -(rect.Right - rect.Left), frameWidth, NativeMethods.DstInvert);

            // 받아온 DC를 풀어준다.
            NativeMethods.ReleaseDC(hWnd, hdc);
        }

        private readonly StringBuilder stringBuilder = new StringBuilder(100);
        private int maxCount = 100;
        // 찾기 마우스버튼을 떼면
        private void find_window_button_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            //Console.WriteLine("mouseUp");
            // 아래에 있는 윈도우의 포인터를 받아옴
            IntPtr under = GetWindowFromMousePosition();
            if (under != IntPtr.Zero)
            {
                // 해당 윈도우의 이름을 받아온다.
                int titleLength = NativeMethods.GetWindowText(under, stringBuilder, maxCount);
                //Console.WriteLine($"Title : {stringBuilder.ToString()}");
                
                // 아래에있는 윈도우의 rect를 받아옴
                if(NativeMethods.GetWindowRect(under, out MyRect myRect))
                {
                    //NativeMethods.OffsetRect(ref myRect, -myRect.Left, -myRect.Top);
                    var rect = myRect.ToRect();
                    //Console.WriteLine($"Width : {rect.Width}, Height: {rect.Height}");
                    //Console.WriteLine($"TopLeft : {rect.TopLeft}, BottomRight: {rect.BottomRight}");

                    double windowHeight = rect.Height + borderthickness * 2 + titleBar.Height + bottmBar.Height;
                    // 재설정하려는 높이가 현재화면보다 클경우 실행하지않음
                    if (windowHeight >= SystemParameters.PrimaryScreenHeight)
                        MessageBox.Show("높이가 너무 높습니다.\n전체화면 캡쳐를 이용해주세요.");
                    else
                    {
                        // rect의 위치에 맞춰 위치와 크기를 조정한다.
                        // 왼쪽은 테두리 하나있으므로 테두리 하나만큼빼준값으로 설정
                        this.Left = rect.Left - borderthickness;
                        // 위쪽은 테두리와 위쪽칸이 있으므로 그만큼 빼줌
                        this.Top = rect.Top - titleBar.Height - borderthickness;
                        // 너비는 양쪽 테두리 계산
                        this.Width = rect.Width + borderthickness * 2;
                        // 높이는 테두리*2 + 위 아래 박스
                        this.Height = windowHeight;
                    }
                }

                // 마우스를 놨으니 아래 윈도우를 다시 갱신함. 그려줬던걸 돌려놓기위해
                // 윈도우의 일부분을 무효화 시켜주는 함수.
                NativeMethods.InvalidateRect(under, IntPtr.Zero, true);
                // 윈도우의 무효화영역을 갱신함.
                NativeMethods.UpdateWindow(under);
            }

            // 버튼클릭 초기화, 마지막에 골랐던것도 초기화해줌.
            isButtonClicked = false;
            lastHandle = IntPtr.Zero;
        }

        private IntPtr handle = IntPtr.Zero;
        private IntPtr lastHandle = IntPtr.Zero;

        // 드래그를 위해 mouseMove에서 buttonClicked로 체크함
        private void find_window_button_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!isButtonClicked)
                return;

            handle = GetWindowFromMousePosition();
            if (handle != IntPtr.Zero && lastHandle != handle)
            {
                if (lastHandle != IntPtr.Zero)
                    DrawFrame(lastHandle, 3);

                lastHandle = handle;
                DrawFrame(handle, 3);
            }

        }

        private bool isButtonClicked = false;
        // 버튼을 클릭하면 변수를 true로
        private void find_window_button_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            isButtonClicked = true;
        }

        // 캡쳐창 크기 바뀌면 텍스트박스 갱신
        private void CropCapture_Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            width_textBox.Text = (CropCapture_Window.ActualWidth - borderthickness * 2).ToString();
            height_textBox.Text = (CropCapture_Window.ActualHeight - borderthickness * 2).ToString();
        }
    }
}
