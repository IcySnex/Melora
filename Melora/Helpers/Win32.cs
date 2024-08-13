#pragma warning disable CA1401 // P/Invokes should not be visible
#pragma warning disable SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
#pragma warning disable CA2211 // Non-constant fields should not be visible

using System.Runtime.InteropServices;

namespace Melora.Helpers;

public class Win32
{
    public delegate IntPtr WinProc(
        IntPtr hWnd,
        int msg,
        IntPtr wParam,
        IntPtr lParam);


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MINMAXINFO
    {
        public POINT ptReserved;
        public POINT ptMaxSize;
        public POINT ptMaxPosition;
        public POINT ptMinTrackSize;
        public POINT ptMaxTrackSize;
    }


    public static WinProc? NewWndProc { get; set; }
    public static IntPtr OldWndProc { get; set; } = IntPtr.Zero;

    public static int MinWidth = 0;
    public static int MinHeight = 0;


    [DllImport("user32.dll")]
    public static extern IntPtr GetDpiForWindow(
        IntPtr hwnd);

    [DllImport("user32.dll")]
    public static extern IntPtr CallWindowProc(
        IntPtr lpPrevWndFunc,
        IntPtr hWnd,
        int msg,
        IntPtr wParam,
        IntPtr lParam);

    [DllImport("user32")]
    public static extern IntPtr SetWindowLong(
        IntPtr hWnd,
        int nIndex,
        WinProc newProc);

    [DllImport("user32", SetLastError = true)]
    public static extern IntPtr SetWindowLongPtr(
        IntPtr hWnd,
        int nIndex,
        WinProc newProc);


    public static IntPtr NewWindowProc(
        IntPtr hWnd,
        int msg,
        IntPtr wParam,
        IntPtr lParam)
    {
        if (msg == 36)
        {
            MINMAXINFO minMaxInfo = Marshal.PtrToStructure<MINMAXINFO>(lParam);
            minMaxInfo.ptMinTrackSize.x = MinWidth;
            minMaxInfo.ptMinTrackSize.y = MinHeight;
            Marshal.StructureToPtr(minMaxInfo, lParam, true);

        }
        return CallWindowProc(OldWndProc, hWnd, msg, wParam, lParam);
    }
}