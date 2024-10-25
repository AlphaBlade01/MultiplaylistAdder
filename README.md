# MultiplaylistAdder

This app will allow you to add a song into multiple playlists of your selection. Currently only works with music tracks (no podcast episodes).

## Usage
To effectively use the application, follow these steps:

1. **Authorisation:** Upon opening the app, you will be prompted to grant authorisation. Accept this prompt to allow the app to access your Spotify track and playlist data.
2. **Select Playlists:** Choose the playlists to which you would like to add your song.
3. **Update Current Song:** Click on the "Update Song" button to update the currently playing song.
4. **Add Songs:** Click on the "Add Song" button to add selected songs to the chosen playlists.

### Important: Ensure you have a spotify account active before using the app

## Setup with source code
1. Create an app on spotify
2. Create an `appsettings.json` file with the following format:
```
{
  "Spotify": {
    "ClientSecret": "INSERT_CLIENT_SECRET_HERE",
    "ClientId": "INSERT_CLIENT_ID_HERE"
  }
}
```
3. Build & Run!