# lalamachine

## Intro

lalamachine is just another music player. It features:

- A library with nice filter and search functionality.
- A queue for what to listen to next.
- Named playlists, and a heart on every track for the ones you like.
- Pushing a playlist to your Jellyfin server, with the same tracks in the same order.

## Dependencies

The [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0). The app runs on Windows and macOS, and on Linux
when VLC is installed.

## Run it

Either use the installer from the release section, or clone the repo and

```bash
dotnet run --project Lalamachine.Desktop
```

## Jellyfin

lalamachine can copy a playlist to a Jellyfin server. The server only needs an API key, no plugin.

1. Create an API key in Jellyfin's dashboard.
2. In lalamachine's settings, enter the server address and the key, press **Test connection**, choose your user and
   press **Save**.
3. Press **Check playlists** to see which of your tracks Jellyfin has. This changes nothing.
4. Select a playlist and press **Push to Jellyfin**. If you changed its order, save or discard that first, the button
   is hidden until then.

Good to know:

- A track is found when its path in Jellyfin ends with its path inside your library folder, so both need the same
  folders. Tracks Jellyfin doesn't have get listed, and you decide whether to push without them.
- A new playlist is private to the chosen user.
- Pushing a playlist again replaces its tracks in Jellyfin, so changes made there get lost. It asks first.
- The push is manual and one-way. Renaming a playlist in lalamachine creates a second one in Jellyfin on the next push,
  and deleting one doesn't reach Jellyfin.
- On Windows the API key is stored encrypted for your user. On macOS and Linux it's in a file only you can read, but
  it isn't encrypted.

## Test

```bash
dotnet test
```

## Release

Needs [InstallForge](https://installforge.net) 1.6 or later. Set `<Version>` in `Lalamachine.Desktop\Lalamachine.Desktop.csproj`,
then run `.\build-release.ps1`. It publishes the app and builds `dist\lalamachine_<version>.exe`.

`install-forge.ifp` uses paths relative to the repository root, and InstallForge resolves them against the current
directory. So build with the script, not from the InstallForge window. The window stores files you add there as
absolute paths, make them relative before committing.

## Contribute

If you'd like to contribute to lalamachine,

- Test. Either by using the app or by adding to the unit tests.
- Write code and create a pull request. [There is a list of issues that wants to be resolved.](https://github.com/space-santa/lalamachine/issues)
- Let me know how you like the app and what functionality is missing.

## Licence

Copyright 2015-2024 Claus Zirkel

Lalamachine is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Lalamachine is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with lalamachine. If not, see <http://www.gnu.org/licenses/>.
