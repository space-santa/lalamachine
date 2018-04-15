Name: lalamachine
Version: 3.6.1
Release: 1
License: GPL-3.0+
Summary: A music player
Url: https://github.com/space-santa/lalamachine
Source0: %{name}-%{version}.tar.gz
Requires: qt5-qtquickcontrols >= 5.4.1, qt5-qtmultimedia >= 5.4.1, taglib >= 1.11
BuildRequires: qt5-qtbase-devel >= 5.4.1, qt5-qtmultimedia-devel >= 5.4.1 taglib-devel >= 1.11

%description
Lalamachine is a music player.

%global debug_package %{nil}

%prep
%setup -q

%build
qmake-qt5 "target.path=%{buildroot}/usr/bin" \
      "icon.path=%{buildroot}/usr/share/icons/hicolor/64x64/apps" \
      "desktop.path=%{buildroot}/usr/share/applications" \
      lalamachine.pro
make %{?_smp_mflags}

%install
make install

%clean
rm -rf $RPM_BUILD_ROOT

%files
%defattr(-,root,root)
%dir /usr/share/icons/
%dir /usr/share/icons/hicolor/
%dir /usr/share/icons/hicolor/64x64/
%dir /usr/share/icons/hicolor/64x64/apps/
/usr/bin/lalamachine
/usr/share/icons/hicolor/64x64/apps/lalamachine-new.png
/usr/share/applications/lalamachine.desktop

%changelog
* Sat Apr 15 2018 space-santa 3.6.1-1
  * Bugfix: Scanning and rescanning the library works now reliably.
  * Bugfix: No more crashes on closing after scan.

* Fri Apr 13 2018 space-santa 3.6.0-1
  * Adding tracks to named playlists works now.
  * Behind the scenes refactoring to make everything better.

* Thu Apr 05 2018 space-santa 3.5.2-1
  * Remove SingleInstanceGuard to allow more than one instance of lalamachine.
  * Behind the scenes refactoring to make everything better.

* Mon Mar 26 2018 space-santa 3.5.1-1
  * Add SingleInstanceGuard to only allow one instance of lalamachine.

* Sun Mar 25 2018 space-santa 3.5.0-1
  * Add SingleInstanceGuard to only allow one instance of lalamachine.

* Sat Nov 25 2017 rmean 3.4-1
  * Consider discnumber when sorting tracks.

* Sat Sep 30 2017 rmean 3.3-1
  * Remove systray icon.
  * Update application icon.

* Fri Sep 29 2017 rmean 3.2-1
  * Make sure new settings are added and available in the dialog.
  * The settings dialog now resizes properly.
  * Remove disc burning functionality.
  * The background is now blue, the sliders are orange.
  * New application icon.

* Tue Sep 26 2017 rmean 3.1-1
  * Ported lalamachine to MacOS.
  * Added the discnumber to the library and playlist.

* Fri Jan 13 2017 rmean 3.0-1
  * Ported lalamachine to Windows.
  * Fixed a couple of utf8 and other crossplattform issues.

* Sat Sep 10 2016 rmean 2.2-1
  * Fixed the sound-off-when-new-track-starts bug.

* Mon Aug 15 2016 rmean 2.1-1
  * Fixed the mute-button.

* Sun Aug 14 2016 rmean 2.0-1
  * Now using libvlc as backend for way better performance.
  * This also fixes the seek bug.
  * Using the mouse-wheel ober the tray icon changes the volume.

* Sat Aug 06 2016 rmean 1.6.1-1
  * Fixed a bug that broke play next/previous.

* Fri Aug 05 2016 rmean 1.6.0-1
  * Replaced qml-multimedia with qtmultimedia as backend.

* Sat Jun 18 2016 rmean 1.5.1-3
  * Bugfix: CD burning works again.
  * Bugfix: Left click in playlist settings works again.

* Sat Jun 18 2016 rmean 1.5.1-2
  * Ported lalamachine to debian 8.
  * Bugfix: Fixed colours in various places.
  * Bugfix: Fixed a bug that made it impossible to get anything but the added
    date on first run

* Sun Jun 12 2016 r-mean 1.5.0-1: Release 1.5
  * Ported lalamachine to debian 8.
  * Bugfix: Fixed colours in various places.
  * Bugfix: Fixed a bug that made it impossible to get anything but the added
    date on first run.

* Sat Jun 04 2016 r-mean 1.4.3-1: Release 1.4.3
  * Bugfix: Fixed the colours of the table views to also work in the dark.

* Sun May 29 2016 r-mean 1.4.2-1: Release 1.4.2
  * Bugfix: Fixed path so taglib finds files to scan

* Thu Jan 21 2016 r-mean 1.4.1-1: Release 1.4.1
  * Bugfix: Significantly reduced the loading time of playlists.

* Thu Jan 21 2016 r-mean 1.4.0-1: Release 1.4
  * Bugfix: Artist no longer displayed in now playing panel.

* Sat Jan 16 2016 r-mean 1.3.3-1: Release 1.3.3
  * Fixed a bug that made drag and move fail for ling playlists.

* Fri Jan 15 2016 r-mean 1.3.2-1: Release 1.3.2
  * Player controls are now also visible in library view.

* Fri Jan 01 2016 r-mean 1.3.1-1: Release 1.3.1
  * Fixed segfault on openSUSE 42.1.

* Thu Aug 20 2015 r-mean 1.3.0-1: Release 1.3
  * Kiosk mode.
  * Mousewheel on LalaTray changes the volume and shows message with value.
  * Hovering over LalaTray shows a tooltip with the currently playhing title.
  * The LalaTray context menu can now play/pause, next and previous.
    Also the quitAction got an icon.
  * If the GUI is visible but behind another window, clicking on LalaTray
    bring the GUI to the front.
  * Fixed a bug that made it impossible to set the volume to 100% using
    the volume up shortcut.

* Sat Aug 08 2015 r-mean 1.2-1: Release 1.2
  * Fixed a bug that would move tracks when clicking in the playlist.
  * Playlist and musivlib are now in tabs and no longer visible at once.
  * The app has now a system tray icon.
  * The xontainers of the musiclib tracklist and filter are now resizable.

* Sun Aug 02 2015 r-mean
  1.1.1-1: Release 1.1.1
  * Fixed sorting the length.
  * Minor formatting changes to text.
  * Changed the text colour of the selected row to be visible on dark bg.

* Sun Aug 02 2015 r-mean
  1.1.0-1: Release 1.1.0
  * Each track now has a tag "date added". This is only inside lalamachine and
    is not an actual tag that is written to the file.
  * Fixed a bug that made sorting not work properly.
  * Playlists are now exported into a new folder.

* Sun Jul 19 2015 r-mean
  1.0.10-1: Release 1.0.10
  * Tracks in a playlist can now be moved with mouse drag'n'move.
  * Creating a new playlist has now a shortcut (ctrl+n)
    and a right click menu item.
  * The playlist button bar is now redundant.

* Sun Jul 19 2015 r-mean
  1.0.9-1: Release 1.0.9
  * Fixed a bug that would not preserve the dateAdded tag for tracks with a
    singlequote in their mrl.

* Sat Jul 18 2015 r-mean
  1.0.8-1: Release 1.0.8
  * Added custom tag 'dateAdded'. Lists can be sorted by that.

* Sat Jul 18 2015 r-mean
  1.0.7-1: Release 1.0.7
  * Exporting a playlist now creates a folder named after the playlist.

* Sun Jul 12 2015 r-mean
  1.0.6-1: Release 1.0.6
  * Changes to named playlists are now always saved.

* Sun Jul 12 2015 r-mean
  1.0.5-1: Release 1.0.5
  * Added action/shortcut ctrl+enter to play currently highlighted track.
  * Playlists can now also be sorted by album.

* Sat Jul 11 2015 r-mean
  1.0.4-1: Release 1.0.4
  * Improved the user experience with keyboard shortcuts.
    E.g. the keyboard shortcuts still work while the filtertext has focus.

* Thu Jul 09 2015 r-mean
  1.0.3-1: Release 1.0.3
  * Fixed a bug that would screw up the track progress slider update when used
    with the mouse wheel.

* Thu Jul 09 2015 r-mean
  1.0.2-1: Release 1.0.2
  * Fixed a bug that could cause the app to crash when a playlist is deleted.

* Tue Jun 23 2015 r-mean
  1.0.1-1: Release 1.0.1
  * Musiclib scanning now uses a separate dbase connection.

* Sat Jun 20 2015 r-mean
  1.0-1: Release 1.0
  * Code cleanup.
  * Shift/ctrl + left click now selects multiple tracks as expected.

* Sat Jun 20 2015 r-mean
  0.99.2-1: Release 0.99.2
  * Library scan is now significantly faster.
  * If on startup there is no config.json, the volume is 0.4.
  * Resetting the filter no longer blocks the UI.

* Sun Jun 14 2015 r-mean
  0.99.1-1: Release 0.99.1
  * Fixed a bug that would overwrite a named playlist when adding an album to a
    new playlist.

* Sat May 30 2015 r-mean
  0.99.0-1: Release 0.99
  * Right click in the album list can add that album to a playlist.
  * The window size and position is remembered between sessions.

* Sat May 16 2015 r-mean
  0.20.0-1: Release 0.20
  * Showing an info dialog when the player has an error.
  * Auto playlists are now sortet like the library by default.

* Fri May 15 2015 r-mean
  0.19.0-1: Release 0.19
  * Deleting the old musiclib before doing a rescan.
  * Frequently updating the displaylibs while scanning the lib.
  * Updated the auto playlist dialog.
  * Fixed a bug that would not show the correct columns in the playlists.

* Sat May 09 2015 r-mean
  0.18.0-1: Release 0.18
  * The Help and Report Bug menu items now open the appropriate bitbucket-url
    in a browser.
  * Clicking the speaker image in the volume control now mutes/unmutes.

* Fri May 08 2015 r-mean
  0.17.0-1: Release 0.17
  * There is now a splash screen altough the app loads too fast to see it.
  * Increased the speed with which the library view is updated.
    This especially matters when clearing the filter box.
  * In the filter lists, the all filter is now highlighted when appropriate.
  * When moving a track in the playlist up/down it stays highlighted/selected.

* Wed May 06 2015 r-mean
  0.16.0-1: Release 0.16
  * The columns of the playlist are now configurable.
  * Resetting the filterbox now also resets the sorting.

* Sat May 02 2015 r-mean
  0.15.0-1: Release 0.15
  * The titles in the library view are now better sorted.
  * Added a filter box which is filtering genre/artist/album and titles while
    typing in it.

* Sun Apr 26 2015 r-mean
  0.14.0-1: Release 0.14
  * Can export the files of a playlist to a folder..

* Sat Apr 25 2015 r-mean
  0.13.2-1: Release 0.13.2
  * One track can now be repeated.
  * Random tracks can be played.
  * Auto-playlists can be edited.

* Sun Apr 19 2015 r-mean
  0.13.1-1: Release 0.13.1
  * Using standard installation paths now.

* Sun Apr 19 2015 r-mean
  0.12.0-1: Release 0.12.1
  * Automatic Playlists.
  * The total playtime is always calculated, also for the library.

* Sat Apr 04 2015 r-mean
  0.11.0-1: Release 0.11.1
  * Playlists now have a name when loaded.
  * Changing a playlist will automatically save the changes.
  * The app remembers which playlist was loaded between sessions.
  * Playlists can now be named (save as) in the menu.
  * Removed the save button from the button bar.

* Fri Apr 03 2015 r-mean
  0.10.1-1: Release 0.10.1
  * Fixed bug that could crash the application while scanning the database.
  * Added Info dialog.
  * Double clicking a library track is now playing it instead o adding it to
    the current playlist.

* Wed Apr 01 2015 r-mean
  0.10.0-1: Release 0.10
  * The musiclib is now a sqlite database.

* Sun Mar 29 2015 r-mean
  0.9.0-1: Release 0.9
  * If a comment starts with a camelot key it is sorted from 1-12.
  * Track numbers are now sorted correctly again.
  * Added a right click menu to add or remove tracks from playlists.
  * Moved playlist open, delete and burn into the menu bar of the main window.

* Sun Mar 15 2015 r-mean
  0.8.0-1: Release 0.8
  * Sorting a playlist is now significantly faster. The whole library can now
    be sorted.
  * Each table displays now how many entries it has.

* Sun Mar 15 2015 r-mean
  0.7.0-1: Release 0.7
  * The library can now be rescanned.
  * The library path is now configurable.
  * Fixed a bug that would not update the artist and album list after the
    library has changed.

* Sat Mar 14 2015 r-mean
  0.6.0-1: Release 0.6
  * Fixed a bug that would play the wrong next track if the playlist was
    reordered.
  * Fixed a bug that would not save the musiclib in UTF8.
  * The misc playlist is no longer visible/selectable in the playlistlist.

* Sun Mar 08 2015 r-mean
  0.5.0-1: Release 0.5
  * The window tile now contains the title of the currently playing track.
  * Playlists can now be deleted.
  * Playlists can now be burned as audio cd with K3b.

* Wed Mar 04 2015 r-mean
  0.4.0-1: Release 0.4
  * Limiting the size of the playlist for sorting.
  * The filter lists are alphabetically sorted and can no longer be sorted
    manually.
  * Fixed a bug that would highlight the wrong track when the same track is in
    in the list more than once. (UUID for each playlist_model entry)

* Sun Mar 01 2015 r-mean
  0.3.0-1: Added a rhythmbox like music library.

* Sat Feb 07 2015 r-mean
  0.2.4-1: Testrelease to see if the preun scriptlet now works.

* Sat Feb 07 2015 r-mean
  0.2.3-1: Fixed the condition done should be fi.

* Sat Feb 07 2015 r-mean
  0.2.2-1: Changed the removal of the sumlink from postun to preun.

* Sat Feb 07 2015 r-mean
  0.2.1-1: Fixed the postun scriptlet in the spec file to not remove the
  symlink on upgrade.

* Sat Feb 07 2015 r-mean
  0.2.0-1: Release 0.2
  * Added keyboard control.
  * Playlists can now be saved and loaded.
  * The total playtime of the playlist is now displayed.
  * App settings can now be stored in a JSON file.
  * The last volume is remembered on app start.

* Sat Feb 07 2015 r-mean
  0.1.0-1: Release 0.1 - A useable player.

* Sat Feb 07 2015 r-mean
  0.0.9-1: Initial package.
