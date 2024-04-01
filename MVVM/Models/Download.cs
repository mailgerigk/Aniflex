using System.Diagnostics;

namespace Aniflex.MVVM.Models;

public sealed class Download
{
    public Magnet? Magnet { get; set; }
    public Process? Process { get; set; }
    public DownloadProgress? Progress { get; set; }
}
