namespace MangaBox.Auth;

public static class JwtKeyUtil
{
    public const string CONFIG_KEY = "OAuth";

    public static async Task EnsureKeyFile(string path)
    {
        if (string.IsNullOrEmpty(path))
            throw new ArgumentNullException(nameof(path), "File path is required for the RSA key");

        var dir = Path.GetDirectoryName(path);
        if (dir != null && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        if (Path.Exists(path))
            return;

        using var rsa = RSA.Create();
        var contents = rsa.ToXmlString(true);

        await File.WriteAllTextAsync(path, contents);
    }

    public static string GetRequiredConfigVar(IConfiguration config, string key)
    {
        return config[CONFIG_KEY + ":" + key]
            ?? throw new ArgumentNullException(CONFIG_KEY + ":" + key, $"OAuth {key} is required");
    }

    public static async Task<TokenValidationParameters> GetParameters(IConfiguration config)
    {
        var getVar = (string key) => GetRequiredConfigVar(config, key);

        return new TokenValidationParameters
        {
            IssuerSigningKey = await GetKey(getVar("KeyPath")),
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            ValidAudience = getVar("Audience"),
            ValidIssuer = getVar("Issuer"),
            RequireExpirationTime = true,
            ValidateLifetime = true,
        };
    }

    public static async Task<SecurityKey> GetKey(string path)
    {
        await EnsureKeyFile(path);

        using var rsa = RSA.Create();
        var key = await File.ReadAllTextAsync(path);
        rsa.FromXmlString(key);
        return new RsaSecurityKey(rsa.ExportParameters(true));
    }
}
