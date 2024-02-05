using System.Security.Authentication;
using Externalities.QueryModels;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Newtonsoft.Json;
using Serilog;

namespace Api;

public class TokenService
{
    public string IssueJwt(EndUser user)
    {
        try
        {
            IJwtAlgorithm algorithm = new HMACSHA512Algorithm();
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);
            return encoder.Encode(user, Environment.GetEnvironmentVariable("FULLSTACK_JWT_PRIVATE_KEY"));
        }
        catch (Exception e)
        {
            Log.Error(e, "IssueJWT");
            throw new InvalidOperationException("User authentication succeeded, but could not create token");
        }
    }
    
    public Dictionary<string, string> ValidateJwtAndReturnClaims(string jwt)
    {
        try
        {
            IJsonSerializer serializer = new JsonNetSerializer();
            var provider = new UtcDateTimeProvider();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtValidator validator = new JwtValidator(serializer, provider);
            IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder, new HMACSHA512Algorithm());
            var json = decoder.Decode(jwt, Environment.GetEnvironmentVariable("FULLSTACK_JWT_PRIVATE_KEY"));
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(json)!;
        }
        catch (Exception e)
        {
            Log.Error(e, "ValidateJwtAndReturnClaims");
            throw new AuthenticationException("Authentication failed.");
        }
    }
}