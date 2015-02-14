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
Version: 0.2.4
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
qmake-qt5 "target.path=%{buildroot}/%{_rmeandir}/bin" \
      "icon.path=%{buildroot}/%{_rmeandir}/icons" \
      "desktop.path=%{buildroot}/usr/share/applications" \
      lalamachine.pro
make %{?_smp_mflags}

%install
make install

%post
rm -f /usr/local/bin/lalamachine
ln -s %{_rmeandir}/bin/lalamachine /usr/local/bin/lalamachine

%preun
if [$1 -lt 1]; then
    rm -f /usr/local/bin/lalamachine
fi

%clean
rm -rf $RPM_BUILD_ROOT

%files
%defattr(-,root,root)
%{_rmeandir}/bin/lalamachine
%{_rmeandir}/icons/lalamachine.png
/usr/share/applications/lalamachine.desktop

%changelog
* Sat Feb 07 2015 %packager
  0.2.4-1: Testrelease to see if the preun scriptlet now works.

* Sat Feb 07 2015 %packager
  0.2.3-1: Fixed the condition done should be fi.

* Sat Feb 07 2015 %packager
  0.2.2-1: Changed the removal of the sumlink from postun to preun.

* Sat Feb 07 2015 %packager
  0.2.1-1: Fixed the postun scriptlet in the spec file to not remove the
  symlink on upgrade.

* Sat Feb 07 2015 %packager
  0.2.0-1: Release 0.2
  * Added keyboard control.
  * Playlists can now be saved and loaded.
  * The total playtime of the playlist is now displayed.
  * App settings can now be stored in a JSON file.
  * The last volume is remembered on app start.

* Sat Feb 07 2015 %packager
  0.1.0-1: Release 0.1 - A useable player.

* Sat Feb 07 2015 %packager
  0.0.9-1: Initial package.
