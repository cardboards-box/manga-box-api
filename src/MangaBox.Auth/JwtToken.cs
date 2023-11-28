namespace MangaBox.Auth;

public class JwtToken
{
    private List<Claim> _claims = new List<Claim>();

    public string? this[string key]
    {
        get => _claims.Find(t => t.Type == key)?.Value;
        set
        {
            var claim = _claims.Find(t => t.Type == key);

            if (claim != null)
                _claims.Remove(claim);

            _claims.Add(new Claim(key, value ?? ""));
        }
    }

    public string? Email
    {
        get => this[ClaimTypes.Email];
        set => this[ClaimTypes.Email] = value;
    }

    public SecurityKey Key { get; }

    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
    public int ExpireyMinutes { get; set; } = 10080;
    public string SigningAlgorithm { get; set; } = SecurityAlgorithms.RsaSha512Signature;

    public JwtToken(SecurityKey key)
    {
        Key = key;
    }

    public JwtToken(TokenValidationParameters validators, string token)
    {
        Key = validators.IssuerSigningKey;
        Read(token, validators);
    }

    public JwtToken AddClaim(params Claim[] claims)
    {
        foreach (var claim in claims)
            _claims.Add(claim);
        return this;
    }
    public JwtToken AddClaim(string key, string value)
    {
        return AddClaim(new Claim(key, value));
    }
    public JwtToken AddClaim(params (string, string)[] claims)
    {
        foreach (var claim in claims)
            AddClaim(claim.Item1, claim.Item2);

        return this;
    }
    public JwtToken Expires(int minutes)
    {
        ExpireyMinutes = minutes;
        return this;
    }
    public JwtToken SetEmail(string email)
    {
        Email = email;
        return this;
    }
    public JwtToken SetIssuer(string issuer)
    {
        Issuer = issuer;
        return this;
    }
    public JwtToken SetAudience(string audience)
    {
        Audience = audience;
        return this;
    }

    public string Write()
    {
        this[JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString();

        var token = new JwtSecurityToken
        (
            issuer: Issuer,
            audience: Audience,
            claims: _claims,
            expires: DateTime.UtcNow.AddMinutes(ExpireyMinutes),
            signingCredentials: new SigningCredentials(Key, SigningAlgorithm)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private void Read(string token, TokenValidationParameters validations)
    {
        var handler = new JwtSecurityTokenHandler();

        _claims = handler.ValidateToken(token, validations, out SecurityToken ts).Claims.ToList();

        var t = (JwtSecurityToken)ts;
        Issuer = t.Issuer;
        Audience = t.Audiences.First();
        ExpireyMinutes = (t.ValidTo - DateTime.UtcNow).Minutes;
        SigningAlgorithm = t.SignatureAlgorithm;
    }
}