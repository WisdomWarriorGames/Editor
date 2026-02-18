using System.Diagnostics;
using System.Text.Json;
using WisdomWarrior.Editor.Core;
using WisdomWarrior.Engine.Core;

namespace WisdomWarrior.Editor.FileSystem;

public class ProjectService
{
    public Manifest CreateSolution(string projectPath, string projectName)
    {
        var name = projectName.Replace(" ", string.Empty);
        var rootPath = GetRootPath(projectPath, projectName);

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

        SaveCurrentScene(gameLogicPath);
        var manifest = SaveManifest(projectName, name, rootPath);
        return manifest;
    }

    public string GetRootPath(string projectPath, string projectName)
    {
        var name = projectName.Replace(" ", string.Empty);
        var rootPath = Path.Combine(projectPath, name);

        return rootPath;
    }

    private Manifest SaveManifest(string projectName, string name, string rootPath)
    {
        var manifest = new Manifest
        {
            ProjectName = projectName,
            ProjectNameStripped = projectName.Replace(" ", ""),
            GameProjectPath = $"{projectName}.Game",
            ActiveScene = $"{projectName}.Game/CurrentScene.scene.json",
            Modules = new List<ProjectModule>
            {
                new() { Name = projectName, Path = projectName }
            }
        };

        var manifestPath = Path.Combine(rootPath, $"{name}.manifest.json");
        var json = JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(manifestPath, json);
        return manifest;
    }

    private string SaveCurrentScene(string gameLogicPath)
    {
        var scene = new Scene
        {
            Name = "Scene1",
            Entities = []
        };

        var scenePath = Path.Combine(gameLogicPath, $"CurrentScene.scene.json");
        var json = JsonSerializer.Serialize(scene, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(scenePath, json);

        return scenePath;
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