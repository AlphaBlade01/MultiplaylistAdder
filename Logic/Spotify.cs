using Microsoft.Extensions.Configuration;
using MultiplaylistAdder.Models;
using Newtonsoft.Json;
using SpotifyAPI.Web;
using System.Diagnostics;
using System.Net;

namespace MultiplaylistAdder.Logic;

public class Spotify
{
    private const string ACCESS_TOKEN_URL = "https://accounts.spotify.com/api/token?grant_type=client_credentials&client_id={0}&client_secret={1}";
    private const string ACCESS_TOKEN_PATTERN = @"/""access_token"":\s*""(\S*)""/gm";
    private const string REDIRECT_BASE_URL = "http://localhost:3000/callback/";

    private static readonly JsonSerializer jsonSerializer = new();
    private static readonly HttpClient _httpClient = new();
    private readonly IConfiguration _configuration;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private SpotifyClient _client;
    private IUserProfileClient _userProfile;


    public Spotify(IConfiguration configuration, SpotifyClient client)
    {
        _configuration = configuration;
        _clientId = configuration["Spotify:ClientId"];
        _clientSecret = configuration["Spotify:ClientSecret"];
        _client = client;
        _userProfile = client.UserProfile;
    }

    public static Spotify BuildClient(IConfiguration configuration)
    {
        return Task.Run(async () =>
        {
            string? clientId = configuration["Spotify:ClientId"];
            string? clientSecret = configuration["Spotify:ClientSecret"];

            if (clientId is null || clientSecret is null) throw new Exception("ClientId or secret is null");

            HttpListener loginCallbackServer = CreateLoginCallbackServer();
            string codeVerifier = LoginUser(clientId);
            string? code = await ListenOnLoginCallbackServer(loginCallbackServer);

            if (code != null)
            {
                SpotifyClient client = await InitialiseClient(code, clientId, codeVerifier);
                return new Spotify(configuration, client);
            }

            throw new Exception("Code received from spotify was null");
        }).GetAwaiter().GetResult();
    }

    public async Task<CurrentlyPlaying> GetCurrentlyListening()
    {
        PlayerCurrentlyPlayingRequest request = new();
        CurrentlyPlaying? currentlyListening = await _client.Player.GetCurrentlyPlaying(request);
        return currentlyListening;
    }

    public async Task<List<Playlist>> GetPlaylists()
    {
        var playlists = await _client.PaginateAll(await _client.Playlists.CurrentUsers());
        List<Playlist> result = [];

        if (playlists is null) return result;

        foreach (var playlist in playlists)
        {
            if (playlist is null || playlist.Owner?.Id != (await _client.UserProfile.Current()).Id) continue;

            var newPlaylist = new Playlist
            {
                Name = playlist.Name ?? "",
                Id = playlist.Id ?? "",
                Selected = false
            };
            result.Add(newPlaylist);
        }
        return result;
    }

    public async Task<bool> AddSongToPlaylists(List<Playlist> playlists, FullTrack song)
    {
        try
        {
            foreach (var playlist in playlists)
            {
                var request = new PlaylistAddItemsRequest([song.Uri]);
                await _client.Playlists.AddItems(playlist.Id, request);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }
        return true;
    }

    private static async Task<SpotifyClient> InitialiseClient(string code, string clientId, string codeVerifier)
    {
        var tokenResponse = await new OAuthClient().RequestToken(
            new PKCETokenRequest(clientId, code, new Uri(REDIRECT_BASE_URL), codeVerifier)
        );
        var authenticator = new PKCEAuthenticator(clientId, tokenResponse);
        var config = SpotifyClientConfig.CreateDefault().WithAuthenticator(authenticator);

        return new SpotifyClient(config);
    }

    private static HttpListener CreateLoginCallbackServer()
    {
        HttpListener listener = new();
        listener.Prefixes.Add(REDIRECT_BASE_URL);
        listener.Start();
        return listener;
    }

    private static async Task<string?> ListenOnLoginCallbackServer(HttpListener listener)
    {
        HttpListenerContext context = await listener.GetContextAsync();
        string? code = context.Request.QueryString["code"];
        context.Response.StatusCode = code == null ? 400 : 200;
        return code;
    }

    private static string LoginUser(string clientId)
    {
        string codeVerifier, codeChallenge;
        (codeVerifier, codeChallenge) = PKCEUtil.GenerateCodes();

        LoginRequest loginRequest = new(
            new Uri(REDIRECT_BASE_URL),
            clientId,
            LoginRequest.ResponseType.Code
        )
        {
            CodeChallengeMethod = "S256",
            CodeChallenge = codeChallenge,
            Scope = [
                Scopes.PlaylistReadPrivate,
                Scopes.PlaylistReadCollaborative,
                Scopes.UserReadCurrentlyPlaying,
                Scopes.PlaylistModifyPrivate,
                Scopes.PlaylistModifyPublic
            ]
        };

        var uri = loginRequest.ToUri();
        try
        {
            var processStartInfo = new ProcessStartInfo()
            {
                FileName = uri.AbsoluteUri,
                UseShellExecute = true
            };
            _ = Process.Start(processStartInfo);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        return codeVerifier;
    }
}
