using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;

namespace CocktailService.Services;

public class JwtServiceHandler : DelegatingHandler
{
    private readonly IConfiguration _cfg;
    private readonly ILogger<JwtServiceHandler> _logger;
    private string? _cached;
    private DateTime _expiresUtc;

    public JwtServiceHandler(IConfiguration cfg, ILogger<JwtServiceHandler> logger)
    {
        _cfg = cfg;
        _logger = logger;
        _logger.LogInformation("[CocktailService] 🛠️ JwtServiceHandler istanziato.");
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage req, CancellationToken ct)
    {
        if (_cached is null || DateTime.UtcNow >= _expiresUtc)
        {
            GenerateToken();
        }

        _logger.LogDebug("[CocktailService] 🔐 Token JWT attivo valido fino a: {ExpiryUtc}", _expiresUtc);

        req.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", _cached);

        return base.SendAsync(req, ct);
    }

    private void GenerateToken()
    {
        var key = new SymmetricSecurityKey(
            Encoding.ASCII.GetBytes(_cfg["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddHours(12);

        var jwt = new JwtSecurityToken(
            issuer: _cfg["Jwt:Issuer"],
            audience: _cfg["Jwt:Audience"],
            claims: new[] { new Claim(ClaimTypes.Role, "Service") },
            notBefore: DateTime.UtcNow,
            expires: expires,
            signingCredentials: creds);

        _cached = new JwtSecurityTokenHandler().WriteToken(jwt);
        _expiresUtc = expires;

        _logger.LogInformation("[CocktailService] ✅ JWT generato. Scadenza: {ExpiryUtc}", _expiresUtc);
    }
}
