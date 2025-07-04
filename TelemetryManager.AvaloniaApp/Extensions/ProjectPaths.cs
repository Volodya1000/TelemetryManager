using System.IO;
using System;

namespace TelemetryManager.AvaloniaApp.Extensions;

public static class ProjectPaths
{
    public static string RootDirectory => Directory.GetParent(AppContext.BaseDirectory)!.Parent!.Parent!.Parent!.Parent!.FullName;
}