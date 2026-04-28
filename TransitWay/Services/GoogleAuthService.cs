using Google.Apis.Auth;
namespace TransitWay.Services
{
    public class GoogleAuthService
    {
        public async Task<GoogleJsonWebSignature.Payload> VerifyToken(string token)
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings();
            return await GoogleJsonWebSignature.ValidateAsync(token, settings);
        }
    }
}