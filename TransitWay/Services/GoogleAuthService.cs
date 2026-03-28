using Google.Apis.Auth;

namespace TransitWay.Services
{
    public class GoogleAuthService
    {
        public async Task<GoogleJsonWebSignature.Payload> VerifyToken(string token)
        {
            return await GoogleJsonWebSignature.ValidateAsync(token);
        }
    }
}
