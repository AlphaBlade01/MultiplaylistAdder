using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MultiplaylistAdder.Models;
using SpotifyAPI.Web;
using System.Collections.ObjectModel;

namespace MultiplaylistAdder.ViewModels;

public partial class MainViewModel(SpotifyClient spotifyClient) : ObservableObject
{
    private SpotifyClient spotifyClient = spotifyClient;

    [ObservableProperty]
    private ObservableCollection<Playlist> playlists = new();

    [ObservableProperty]
    private string currentlyPlaying = "";

    [ObservableProperty]
    private string status = "";

    [RelayCommand]
    public void AddSong()
    {

    }
}
