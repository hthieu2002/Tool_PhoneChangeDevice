using Newtonsoft.Json;

namespace POCO.ResponseModels

{
    public class GetGoogleDriveAccessTokenResponse
    {
        public GetGoogleDriveAccessToken GetGoogleDriveAccessToken { get; set; }
    }
    public class GetGoogleDriveAccessToken
    {
        public string Access_token;
    }
}
