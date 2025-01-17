using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Electron2D.Misc
{
    public static class Url
    {
        public static void Open(string _url)
        {
            try
            {
                Process.Start(_url);
            }
            catch
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    _url = _url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo(_url) { UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", _url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", _url);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
