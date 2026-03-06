namespace Materal.MergeBlock.Web;

/// <summary>
/// Kestrel 证书配置助手
/// </summary>
public static class KestrelCertificateHelper
{
    /// <summary>
    /// 证书目录
    /// </summary>
    public static string CertificateDirectory { get; set; } = Directory.GetCurrentDirectory();

    /// <summary>
    /// 配置 Kestrel 根据 URL 自动绑定证书
    /// </summary>
    public static void Configure(string[] args, KestrelServerOptions options)
    {
        string? urls = GetUrls(args);
        if (string.IsNullOrEmpty(urls)) return;

        string[] urlList = urls.Split(';', StringSplitOptions.RemoveEmptyEntries);
        foreach (var url in urlList)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) continue;

            if (uri.Scheme == "https")
            {
                string host = uri.Host;
                int port = uri.Port > 0 ? uri.Port : 443;
                options.ListenAnyIP(port, listenOptions =>
                {
                    X509Certificate2? certificate = FindCertificate(host);
                    if (certificate != null)
                    {
                        listenOptions.UseHttps(certificate);
                    }
                    else
                    {
                        listenOptions.UseHttps();
                    }
                });
            }
            else if (uri.Scheme == "http")
            {
                int port = uri.Port > 0 ? uri.Port : 80;
                options.ListenAnyIP(port);
            }
        }
    }

    /// <summary>
    /// 获取 URLs（从命令行参数或环境变量）
    /// </summary>
    private static string? GetUrls(string[] args)
    {
        // 从命令行参数获取 --urls
        foreach (var arg in args)
        {
            if (arg.StartsWith("--urls=", StringComparison.OrdinalIgnoreCase))
            {
                return arg[7..];
            }
        }
        // 从环境变量获取
        return Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
    }

    /// <summary>
    /// 查找证书（支持 .pfx、.pem + .key、.crt + .key）
    /// </summary>
    public static X509Certificate2? FindCertificate(string host)
    {
        string password = GetCertificatePassword(host);
        string pfxPath = Path.Combine(CertificateDirectory, $"{host}.pfx");
        if (File.Exists(pfxPath)) return LoadCertificate(pfxPath, password);
        return null;
    }

    /// <summary>
    /// 加载证书（兼容 .NET 8 和 .NET 9+）
    /// </summary>
    private static X509Certificate2 LoadCertificate(string certPath, string? password)
    {
#if NET9_0_OR_GREATER
        return X509CertificateLoader.LoadPkcs12FromFile(certPath, password);
#else
        return new X509Certificate2(certPath, password);
#endif
    }

    /// <summary>
    /// 获取证书密码
    /// </summary>
    private static string GetCertificatePassword(string host)
    {
        string certPasswordFile = Path.Combine(CertificateDirectory, $"{host}.password");
        if (File.Exists(certPasswordFile)) return File.ReadAllText(certPasswordFile).Trim();
        return string.Empty;
    }
}
