using System.Diagnostics;
using System.Text.Json;
using WisdomWarrior.Editor.FileSystem.Models;

namespace WisdomWarrior.Editor.FileSystem;

public class ProjectService
{
    public void CreateSolution(string projectPath, string projectName)
    {
        var name = projectName.Replace(" ", string.Empty);
        var rootPath = Path.Combine(projectPath, name);

        if (!Directory.Exists(rootPath))
            Directory.CreateDirectory(rootPath);

        var assetsPath = Path.Combine(rootPath, name);
        var gameLogicPath = Path.Combine(rootPath, $"{name}.Game");

        RunDotnetCommand(rootPath, $"new sln -n {name}");

        RunDotnetCommand(rootPath, $"new classlib -n {name} -o \"{assetsPath}\"");

        RunDotnetCommand(rootPath, $"new classlib -n {name}.Game -o \"{gameLogicPath}\"");

        RunDotnetCommand(gameLogicPath, $"add reference \"../{name}/{name}.csproj\"");

        RunDotnetCommand(rootPath, $"sln add \"{assetsPath}/{name}.csproj\"");
        RunDotnetCommand(rootPath, $"sln add \"{gameLogicPath}/{name}.Game.csproj\"");

        DeleteIfExists(Path.Combine(assetsPath, $"Class1.cs"));
        DeleteIfExists(Path.Combine(gameLogicPath, $"Class1.cs"));

        var manifest = new Manifest
        {
            GameProjectName = projectName,
            GameProjectNameStripped = name,
            GameProjectPath = gameLogicPath,
            Modules =
            [
                new ProjectModule
                {
                    Name = name,
                    Path = assetsPath
                }
            ]
        };

        var manifestPath = Path.Combine(rootPath, $"{name}.manifest.json");
        var json = JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(manifestPath, json);
    }

    private void RunDotnetCommand(string workingDirectory, string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = Process.Start(startInfo);
        process?.WaitForExit();
    }

    private void DeleteIfExists(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}