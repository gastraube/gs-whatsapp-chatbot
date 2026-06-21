namespace gschatbot.api.Configuration;

public class JwtOptions
{
    public const string Section = "Jwt";

    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "gschatbot";
    public string Audience { get; set; } = "gschatbot-web";
    public int ExpiracaoHoras { get; set; } = 8;
}
