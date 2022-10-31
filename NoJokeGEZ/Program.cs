using NoJokeGEZ.GEZ;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NoJokeGEZ
{
    internal class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateFile(
            string lpFileName,
            int dwDesiredAccess,
            int dwShareMode,
            IntPtr lpSecurityAttributes,
            int dwCreationDisposition,
            int dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetCurrentConsoleFont(
            IntPtr hConsoleOutput,
            bool bMaximumWindow,
            [Out][MarshalAs(UnmanagedType.LPStruct)] ConsoleFontInfo lpConsoleCurrentFont);

        [StructLayout(LayoutKind.Sequential)]
        internal class ConsoleFontInfo
        {
            internal int nFont;
            internal Coord dwFontSize;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct Coord
        {
            [FieldOffset(0)]
            internal short X;
            [FieldOffset(2)]
            internal short Y;
        }

        private const int GENERIC_READ = unchecked((int)0x80000000);
        private const int GENERIC_WRITE = 0x40000000;
        private const int FILE_SHARE_READ = 1;
        private const int FILE_SHARE_WRITE = 2;
        private const int INVALID_HANDLE_VALUE = -1;
        private const int OPEN_EXISTING = 3;
        private static Size GetConsoleFontSize()
        {
            // getting the console out buffer handle
            IntPtr outHandle = CreateFile("CONOUT$", GENERIC_READ | GENERIC_WRITE,
                FILE_SHARE_READ | FILE_SHARE_WRITE,
                IntPtr.Zero,
                OPEN_EXISTING,
                0,
                IntPtr.Zero);
            int errorCode = Marshal.GetLastWin32Error();
            if (outHandle.ToInt32() == INVALID_HANDLE_VALUE)
            {
                throw new IOException("Unable to open CONOUT$", errorCode);
            }

            ConsoleFontInfo cfi = new ConsoleFontInfo();
            if (!GetCurrentConsoleFont(outHandle, false, cfi))
            {
                throw new InvalidOperationException("Unable to get font information.");
            }

            return new Size(cfi.dwFontSize.X, cfi.dwFontSize.Y);
        }

        private static int correct = 0;
        private static int wrong = 0;
        static void Main(string[] args)
        {
            Console.Title = "Fuck you GEZ";
            ContentCreator gz = new ContentCreator();
        HELLOWORLD:
            Console.Clear();
            Console.WriteLine("Erfolgreich angemeldet : " + correct.ToString());
            Console.WriteLine("Fehlerhafte anmeldungen: " + wrong.ToString());
            var SessionToken = GEZSession.BaseSessionToken();
            Console.WriteLine("Sessiontoken: " + SessionToken.GetSessionValue["JSESSIONID"]); 
            var Person = gz.CreateRandom();
            Console.WriteLine(Person.ToString());
            var f1Content = gz.CreatePostPersonData(SessionToken, Person);
            GEZSession.PostFirstFormular(SessionToken, f1Content);
            Console.WriteLine("Personendaten übetragen");
            SessionToken = GEZSession.GetSecondPage(ref SessionToken);
            var f2Content = gz.CreateWohnungData(SessionToken, Person);
            GEZSession.PostSecondFormular(SessionToken, f2Content);
            Console.WriteLine("Wohnungsdaten übertragen");
            SessionToken = GEZSession.GetThirdPage(ref SessionToken);
            var f3Content = gz.CreateZahlungsData(SessionToken, Person);
            GEZSession.PostThirdFormular(ref SessionToken, f3Content);
            Console.WriteLine("Zahlungsanweisung (Eigene Überweißung :) übertragen");
            var captcha = GEZSession.DownloadCaptcha(SessionToken);

            Point location = new Point(Console.CursorLeft, Console.CursorTop);
            Size imageSize = new Size(captcha.Width, captcha.Height);

            using (Graphics g = Graphics.FromHwnd(GetConsoleWindow()))
            {
                using (Image image = captcha)
                {
                    Size fontSize = GetConsoleFontSize();

                    // translating the character positions to pixels
                    Rectangle imageRect = new Rectangle(
                        location.X * fontSize.Width,
                        location.Y * fontSize.Height,
                        imageSize.Width ,
                        imageSize.Height );
                    g.DrawImage(image, imageRect);
                    Console.SetCursorPosition(0, Console.CursorTop + 5);
                }
            }
            captcha.Dispose();
            Console.Write("Bitte gebe den Captcha ein: ");
            string answer = Console.ReadLine();
            var f4Content = gz.CreateCaptchaAndSubmit(SessionToken, answer);
           if( GEZSession.PostLastPage(SessionToken, f4Content))
            {
                Console.WriteLine("Wohnung angemeldet");
                correct++;
                goto HELLOWORLD;
            }
            else
            {
                wrong++;
                Console.WriteLine("Captcha Error");
                goto HELLOWORLD;
            }

        }
    }
}
