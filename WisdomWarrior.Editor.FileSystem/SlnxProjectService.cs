using System.Diagnostics;

namespace WisdomWarrior.Editor.FileSystem;

public class SlnxProjectService
{
    private readonly EditorManifestService _manifestService;

    public SlnxProjectService() : this(new EditorManifestService())
    {
    }

    public SlnxProjectService(EditorManifestService manifestService)
    {
        _manifestService = manifestService;
    }

    public SlnxWorkspaceDescriptor CreateSolution(string projectPath, string projectName)
    {
        var name = projectName.Replace(" ", string.Empty);
        var rootPath = GetRootPath(projectPath, projectName);

        if (!Directory.Exists(rootPath))
            Directory.CreateDirectory(rootPath);

        var solutionFile = Path.Combine(rootPath, $"{name}.slnx");
        var defaultProjectPath = Path.Combine(rootPath, name);
        var defaultProjectFile = Path.Combine(defaultProjectPath, $"{name}.csproj");

        RunDotnetCommand(rootPath, $"new sln --format slnx -n \"{name}\"");
        RunDotnetCommand(rootPath, $"new classlib -n \"{name}\" -o \"{defaultProjectPath}\"");
        RunDotnetCommand(rootPath, $"sln \"{solutionFile}\" add \"{defaultProjectFile}\"");

        DeleteIfExists(Path.Combine(defaultProjectPath, "Class1.cs"));
        _manifestService.Save(rootPath, _manifestService.CreateDefault(name));

        return new SlnxWorkspaceDescriptor
        {
            RootPath = rootPath,
            SolutionFilePath = Path.Combine(rootPath, $"{name}.slnx"),
            DefaultProjectPath = name,
            ActiveScenePath = string.Empty
        };
    }

    public string GetRootPath(string projectPath, string projectName)
    {
        var name = projectName.Replace(" ", string.Empty);
        return Path.Combine(projectPath, name);
    }

    private static void RunDotnetCommand(string workingDirectory, string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        if (process == null)
            throw new InvalidOperationException("Failed to start dotnet process.");

        var standardOutput = process.StandardOutput.ReadToEnd();
        var standardError = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode == 0)
            return;

        var output = string.Join(Environment.NewLine, new[] { standardOutput, standardError }
            .Where(x => !string.IsNullOrWhiteSpace(x)));
        throw new InvalidOperationException($"dotnet {arguments} failed with exit code {process.ExitCode}.{Environment.NewLine}{output}");
    }

    private static void DeleteIfExists(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
