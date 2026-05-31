APP="CyberMaze.app"
EXE_SRC="Meatcorps.Game.CyberMaze"
EXE_DST="CyberMaze"

rm -rf "$APP"
mkdir -p "$APP/Contents/MacOS"
mkdir -p "$APP/Contents/Resources"
mkdir -p "$APP/Contents/Frameworks"

# Move the executable into Contents/MacOS (rename it to match CFBundleExecutable)
cp "./$EXE_SRC" "$APP/Contents/MacOS/$EXE_DST"
cp "./Asset.pak" "$APP/Contents/MacOS/Asset.pak"
cp ./*.dylib "$APP/Contents/Frameworks/"
cp ./*.dylib "$APP/Contents/MacOS/"
chmod +x "$APP/Contents/MacOS/$EXE_DST"

# Put everything else next to it so your relative file loads keep working
# (Asset.pak, configs, etc.)
# NOTE: this copies all files except the .app itself
# rsync -a --exclude="$APP" ./ "$APP/Contents/MacOS/"
cp "./Icon.icns" "$APP/Contents/Resources/CyberMaze.icns"
# Write Info.plist
cat > "$APP/Contents/Info.plist" <<'PLIST'
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>CFBundleDevelopmentRegion</key>
  <string>en</string>

  <key>CFBundleExecutable</key>
  <string>CyberMaze</string>

  <key>CFBundleIdentifier</key>
  <string>com.meatcorps.cybermaze</string>

  <key>CFBundleInfoDictionaryVersion</key>
  <string>6.0</string>

  <key>CFBundleName</key>
  <string>CyberMaze</string>

  <key>CFBundleDisplayName</key>
  <string>CyberMaze</string>

  <key>CFBundlePackageType</key>
  <string>APPL</string>

  <key>CFBundleShortVersionString</key>
  <string>1.0</string>

  <key>CFBundleVersion</key>
  <string>1</string>

  <key>LSMinimumSystemVersion</key>
  <string>12.0</string>

  <key>NSHighResolutionCapable</key>
  <true/>

  <!-- Important for games / real-time apps -->
  <key>LSBackgroundOnly</key>
  
  <key>CFBundleIconFile</key>
  <string>CyberMaze</string>
  <false/>
</dict>
</plist>
PLIST
