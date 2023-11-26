namespace MangaBox.Core.Migrations;

public interface IDatabaseDeploy
{
    Task ExecuteScripts();
}

public class DatabaseDeploy : IDatabaseDeploy
{
    private const string MANIFEST_EXTENSION = ".manifest.json";
    private const string SQL_EXTENSION = ".sql";
    private const string ONETIME_EXTENSION = ".onetime.sql";

    private string? _actualWorkingDir;

    private readonly string? _workingDirectory;
    private readonly IDbConnection _connection;

    public string WorkingDirectory => DetermineWorkingDirectory();

    public DatabaseDeploy(
        IDbConnection connection,
        string? workingDirectory = null)
    {
        _workingDirectory = workingDirectory;
        _connection = connection;
    }

    public static string CurrentOsPath(string path)
    {
        return Path.Combine(path.Split('\\', '/'));
    }

    public static string MarshalPath(string target, string root)
    {
        target = CurrentOsPath(target);
        root = CurrentOsPath(root);

        return Path.IsPathRooted(target) && Path.IsPathFullyQualified(target)
             ? target
             : Path.Combine(root, target);
    }

    public string DetermineWorkingDirectory()
    {
        if (!string.IsNullOrEmpty(_actualWorkingDir)) return _actualWorkingDir;

        var roots = new[]
        {
            AppDomain.CurrentDomain.BaseDirectory,
            AppDomain.CurrentDomain.RelativeSearchPath,
            AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
            "./"
        }.Select(root =>
        {
            if (string.IsNullOrEmpty(root)) return null;
            var path = root;
            if (!string.IsNullOrEmpty(_workingDirectory))
                path = MarshalPath(_workingDirectory, root);

            return path;
        }).Where(t => !string.IsNullOrEmpty(t))
        .ToArray();

        foreach (var root in roots)
            if (Directory.Exists(root))
                return _actualWorkingDir = root;

        throw new DirectoryNotFoundException($"Could not find working directory `{_workingDirectory}` in any of: {string.Join("\r\n", roots)}");
    }

    public string[] GetManifestFiles()
    {
        return Directory.Exists(WorkingDirectory)
            ? Directory.GetFiles(WorkingDirectory, "*" + MANIFEST_EXTENSION, SearchOption.AllDirectories)
                .OrderBy(t => t)
                .ToArray()
            : Array.Empty<string>();
    }

    public async Task<Manifest[]> GetManifiests()
    {
        var paths = GetManifestFiles();
        if (paths.Length <= 0) return Array.Empty<Manifest>();

        var manifests = new List<Manifest>();

        foreach (var path in paths)
        {
            using var io = File.Open(path, FileMode.Open);
            var manifest = await JsonSerializer.DeserializeAsync<Manifest>(io) ?? new();
            manifest.WorkDir = path;
            manifests.Add(manifest);
        }

        return manifests.ToArray();
    }

    public IEnumerable<string> GetSqlScripts(Manifest manifest)
    {
        static bool Valid(string path) => !path.ToLower().EndsWith(ONETIME_EXTENSION);

        if (manifest.Paths is null || manifest.Paths.Length <= 0) yield break;

        var wrk = Path.GetDirectoryName(manifest.WorkDir) ?? WorkingDirectory;
        foreach (var path in manifest.Paths)
        {
            var fullPath = MarshalPath(path, wrk);

            if (Directory.Exists(fullPath))
            {
                var files = Directory.GetFiles(fullPath, "*" + SQL_EXTENSION, SearchOption.AllDirectories);
                foreach (var file in files.OrderBy(t => t))
                    if (Valid(file))
                        yield return file;

                continue;
            }

            if (!File.Exists(fullPath) ||
                !fullPath.ToLower().EndsWith(SQL_EXTENSION) ||
                !Valid(fullPath))
                continue;

            yield return fullPath;
        }
    }

    public async Task ExecuteScripts()
    {
        var manifests = await GetManifiests();
        if (manifests.Length <= 0) return;

        foreach (var man in manifests)
        {
            var scripts = GetSqlScripts(man);
            foreach (var script in scripts)
            {
                try
                {
                    var context = await File.ReadAllTextAsync(script);
                    await _connection.ExecuteAsync(context);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to execute script: {script}\r\n{ex}");
                    throw;
                }
            }
        }
    }
}
