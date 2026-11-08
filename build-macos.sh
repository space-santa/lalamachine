#!/bin/bash
set -euo pipefail

# Publishes the app, wraps it in lalamachine.app and builds dist/lalamachine_<version>_<arch>.dmg.
# Usage: ./build-macos.sh [x64|arm64], defaults to the architecture of this Mac.
# Has to run on macOS: it signs the app and makes the disk image with macOS's tools.

root="$(cd "$(dirname "$0")" && pwd)"
project="$root/Lalamachine.Desktop/Lalamachine.Desktop.csproj"

if [ "$(uname)" != "Darwin" ]; then
  echo "Building the macOS app needs a Mac." >&2
  exit 1
fi

arch="${1:-}"
if [ -z "$arch" ]; then
  case "$(uname -m)" in
    arm64) arch=arm64 ;;
    *) arch=x64 ;;
  esac
fi
case "$arch" in
  x64|arm64) ;;
  *) echo "Unknown architecture '$arch', use x64 or arm64." >&2; exit 1 ;;
esac
rid="osx-$arch"

# The project file is the only place with the version.
version="$(sed -n 's:.*<Version>\(.*\)</Version>.*:\1:p' "$project" | head -n 1)"
if [ -z "$version" ]; then
  echo "No <Version> in $project" >&2
  exit 1
fi

build="$root/dist/macos-$arch"
publish="$build/publish"
app="$build/lalamachine.app"
dmg="$root/dist/lalamachine_${version}_$arch.dmg"

# Start clean, don't let an earlier build end up in the bundle.
rm -rf "$build"
mkdir -p "$build"

# Self-contained, so that the app doesn't need .NET installed.
dotnet publish "$project" -c Release -r "$rid" --self-contained true -o "$publish"

# Debug symbols are no use to anyone who installs the app. Skia's and HarfBuzz's alone are 100 MB.
find "$publish" -name '*.pdb' -delete

mkdir -p "$app/Contents/MacOS" "$app/Contents/Resources"
cp -R "$publish/" "$app/Contents/MacOS/"
cp "$root/assets/lala-icon.png.icns" "$app/Contents/Resources/lalamachine.icns"

cat > "$app/Contents/Info.plist" <<EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>CFBundleName</key><string>lalamachine</string>
  <key>CFBundleDisplayName</key><string>lalamachine</string>
  <key>CFBundleIdentifier</key><string>com.space-santa.lalamachine</string>
  <key>CFBundleVersion</key><string>$version</string>
  <key>CFBundleShortVersionString</key><string>$version</string>
  <key>CFBundlePackageType</key><string>APPL</string>
  <key>CFBundleExecutable</key><string>Lalamachine.Desktop</string>
  <key>CFBundleIconFile</key><string>lalamachine.icns</string>
  <key>NSHighResolutionCapable</key><true/>
  <key>LSMinimumSystemVersion</key><string>12.0</string>
</dict>
</plist>
EOF
plutil -lint "$app/Contents/Info.plist" > /dev/null

if [ ! -x "$app/Contents/MacOS/Lalamachine.Desktop" ]; then
  echo "The publish output has no Lalamachine.Desktop executable, Info.plist points at it." >&2
  exit 1
fi

# Ad-hoc signature. Enough to run on this Mac, a downloaded copy still needs a Developer ID and notarization.
codesign --force --deep --sign - "$app"

# The disk image shows the app next to a link to Applications, to drag it there.
staging="$build/dmg"
mkdir -p "$staging"
cp -R "$app" "$staging/"
ln -s /Applications "$staging/Applications"
rm -f "$dmg"
hdiutil create -volname "lalamachine $version" -srcfolder "$staging" -ov -format UDZO "$dmg"

echo "Disk image: $dmg"
