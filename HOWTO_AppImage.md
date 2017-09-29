This is a rough description of what I did to make it work.
cd DEPLOYDIR
mkdir deploy-lala
cd deploy-lala
cp LALADIR/lalamachine LALADIR/lalamachine.desktop .
linuxdeployqt-continuous-x86_64.AppImage lalamachine.desktop -no-translations -appimage
# It may complain about .dyn stuff of libs
# Just ldd AppRun and check if it only links to files in ./lib.
cd ..
appimagetool-x86_64.AppImage -n deploy-lala/ lalamachine-x86_64.AppImage
# The -n flag make it ignore the xml metadat bollocks.
