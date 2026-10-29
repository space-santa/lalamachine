# lalamachine

## Intro

lalamachine is just another music player. It features:

- A library with nice filter and search functionality.
- Playlists which can be exported into a folder.

## Dependencies

Windows and the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

## Run it

Either use the installer from the release section, or clone the repo and

```bash
dotnet run --project Lalamachine.Wpf
```

## Test

```bash
dotnet test
```

## Release

`build-release.ps1` publishes the app to `Lalamachine.Wpf\bin\Release\net10.0-windows\win-x64\publish`.
`install-forge.ifp` turns that folder into the installer.

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
