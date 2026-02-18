using System.Diagnostics;

namespace ReData.ExprTree.Cli;

public static class GraphvizRenderer
{
    public static void RenderSvg(string dot, string outputPath)
    {
        var dotExe = ResolveDotExecutablePath();
        if (!File.Exists(dotExe))
        {
            throw new FileNotFoundException($"dot.exe not found at '{dotExe}'.");
        }

        var outputDirectory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrWhiteSpace(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        var inputPath = Path.Combine(Path.GetTempPath(), $"expr-tree-{Guid.NewGuid():N}.dot");
        File.WriteAllText(inputPath, dot);

        try
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = dotExe,
                Arguments = $"-Tsvg \"{inputPath}\" -o \"{outputPath}\"",
                WorkingDirectory = Path.GetDirectoryName(dotExe) ?? Environment.CurrentDirectory,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
            });

            if (process is null)
            {
                throw new InvalidOperationException("Unable to start dot.exe process.");
            }

            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                var error = process.StandardError.ReadToEnd();
                throw new InvalidOperationException($"dot.exe failed with code {process.ExitCode}. {error}");
            }
        }
        finally
        {
            if (File.Exists(inputPath))
            {
                File.Delete(inputPath);
            }
        }
    }

    public static void OpenFile(string path)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true,
        });
    }

    private static string ResolveDotExecutablePath()
    {
        var packagesRoot = Environment.GetEnvironmentVariable("NUGET_PACKAGES");
        if (string.IsNullOrWhiteSpace(packagesRoot))
        {
            packagesRoot = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".nuget",
                "packages");
        }

        var graphvizRoot = Path.Combine(packagesRoot, "graphviz");
        if (!Directory.Exists(graphvizRoot))
        {
            throw new DirectoryNotFoundException($"NuGet Graphviz package folder not found: {graphvizRoot}");
        }

        var versionDirs = Directory.GetDirectories(graphvizRoot)
            .OrderByDescending(ParseVersionOrDefault)
            .ToList();

        foreach (var versionDir in versionDirs)
        {
            var dotExePath = Path.Combine(versionDir, "dot.exe");
            if (File.Exists(dotExePath))
            {
                return dotExePath;
            }
        }

        throw new FileNotFoundException($"dot.exe not found under NuGet Graphviz folder: {graphvizRoot}");
    }

    private static Version ParseVersionOrDefault(string path)
    {
        var name = Path.GetFileName(path);
        return Version.TryParse(name, out var version) ? version : new Version(0, 0);
    }
}
