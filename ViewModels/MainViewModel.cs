using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MultiplaylistAdder.Logic;
using MultiplaylistAdder.Models;
using SpotifyAPI.Web;
using System.Collections.ObjectModel;

namespace MultiplaylistAdder.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private Spotify spotifyClient;
    private FullTrack? currentlyPlaying;

    [ObservableProperty]
    private ObservableCollection<Playlist> playlists = [];

    [ObservableProperty]
    private string currentlyPlayingName = "";

    [ObservableProperty]
    private string currentlyPlayingArtist = "";

    [ObservableProperty]
    private string status = "";

    public MainViewModel(Spotify spotifyClient)
    {
        this.spotifyClient = spotifyClient;
        _ = SyncPlaylists();
    }

    [RelayCommand]
    public async Task AddSong()
    {
        if (currentlyPlaying == null)
        {
            Status = "Update currently playing first.";
            return;
        }

        Status = "Adding...";

        List<Playlist> selectedPlaylists = Playlists.Where(p => p.Selected == true).ToList();
        bool success = await spotifyClient.AddSongToPlaylists(selectedPlaylists, currentlyPlaying);

        if (success) Status = "Success.";
        else Status = "Error in adding song.";
    }

    [RelayCommand]
    public async Task GetCurrentlyPlaying()
    {
        CurrentlyPlaying playing = await spotifyClient.GetCurrentlyListening();
        if (playing.CurrentlyPlayingType == ItemType.Episode.ToString()) return;

        currentlyPlaying = playing.Item as FullTrack;
        CurrentlyPlayingName = currentlyPlaying?.Name ?? "Not found";
        CurrentlyPlayingArtist = currentlyPlaying?.Artists.First().Name ?? "Not found";
    }

    private async Task SyncPlaylists()
    {
        List<Playlist> playlists = await spotifyClient.GetPlaylists();
        Playlists = new ObservableCollection<Playlist>(playlists);
    }
}
