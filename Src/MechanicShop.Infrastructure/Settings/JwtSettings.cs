public class JwtSettings
{
    public static readonly string Name = "JwtSettings";
    public string Issuer {get; set;} = default!;
    public string Audience { get; set; } = default!;
    public string SecretKey { get; set; } =default!;
    public int TokenExpirationInMinutes { get; set; }
}