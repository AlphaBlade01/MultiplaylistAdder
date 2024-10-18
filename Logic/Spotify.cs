using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SpotifyAPI.Web;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace MultiplaylistAdder.Logic;

public class Spotify
{
    private const string ACCESS_TOKEN_URL = "https://accounts.spotify.com/api/token?grant_type=client_credentials&client_id={0}&client_secret={1}";
    private const string ACCESS_TOKEN_PATTERN = @"/""access_token"":\s*""(\S*)""/gm";

    private readonly JsonSerializer jsonSerializer = new();
    private readonly HttpClient _httpClient = new();
    private readonly SpotifyClient _client;
    private readonly IConfiguration _configuration;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private string _accessToken;
    
    public Spotify(IConfiguration configuration)
    {
        _configuration = configuration;
        _clientId = configuration["Spotify:ClientId"];
        _clientSecret = configuration["Spotify:ClientSecret"];
        // Instantiate client
    }

    public void GetCurrentlyListening()
    {

    }

    private async Task<string?> GetAccessToken(string clientSecret)
    {
        string url = String.Format(ACCESS_TOKEN_URL, _clientId, _clientSecret);
        HttpContent content = JsonContent.Create(new Dictionary<string, string> { { "Content-Type", "application/x-www-form-urlencoded" } });

        try
        {
            HttpResponseMessage response = await _httpClient.PostAsync(url, content);

            using Stream stream = await response.Content.ReadAsStreamAsync();
            using StreamReader reader = new(stream);
            string body = reader.ReadToEnd();

            Regex pattern = new(ACCESS_TOKEN_PATTERN);
            string token = pattern.Matches(body)[0].Value;

            return token;
        }
        catch (Exception ex)
        {   
            Debug.WriteLine(ex);
            return "";
        }
    }
}
