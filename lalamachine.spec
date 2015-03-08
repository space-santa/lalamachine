#
# spec file for package lalamachine
#
# Copyright (c) 2015 SUSE LINUX Products GmbH, Nuernberg, Germany.
#
# All modifications and additions to the file contributed by third parties
# remain the property of their copyright owners, unless otherwise agreed
# upon. The license for this file, and modifications and additions to the
# file, is the same license as for the pristine package itself (unless the
# license for the pristine package is not an Open Source License, in which
# case the license is the MIT License). An "Open Source License" is a
# license that conforms to the Open Source Definition (Version 1.9)
# published by the Open Source Initiative.

# Please submit bugfixes or comments via http://bugs.opensuse.org/
#

Name: lalamachine
Version: 0.5.0
Release: 1
License: GPLv3
Summary: A music player that is awesome.
Url: https://bitbucket.org/r-mean/lalamachine
Group: Applications/Multimedia
Source: %_sourcedir/%{name}-%{version}.tar.gz
Requires: libqt5-qtquickcontrols libQt5Multimedia5 taglib
BuildRequires: libqt5-qtbase-devel libqt5-qtmultimedia-devel libtag-devel
BuildRoot:      %{_tmppath}/%{name}-%{version}-build

%description

%prep
%setup -q

%build
qmake-qt5 "target.path=%{buildroot}/opt/rmean/bin" \
      "icon.path=%{buildroot}/opt/rmean/icons" \
      "desktop.path=%{buildroot}/usr/share/applications" \
      lalamachine.pro
make %{?_smp_mflags}

%install
make install

%post
rm -f /usr/local/bin/lalamachine
ln -s /opt/rmean/bin/lalamachine /usr/local/bin/lalamachine

%preun
if [$1 -lt 1]; then
    rm -f /usr/local/bin/lalamachine
fi

%clean
rm -rf $RPM_BUILD_ROOT

%files
%dir /opt/rmean
%dir /opt/rmean/bin
%dir /opt/rmean/icons
%defattr(-,root,root)
/opt/rmean/bin/lalamachine
/opt/rmean/icons/lalamachine.png
/usr/share/applications/lalamachine.desktop

%changelog
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
