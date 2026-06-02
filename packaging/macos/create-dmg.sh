#!/usr/bin/env bash
set -euo pipefail

VERSION="${1:-1.0.0}"
APP_NAME="WordSprinter"
DMG_NAME="${APP_NAME}-${VERSION}-macos-x64.dmg"

# Build .app bundle
rm -rf "${APP_NAME}.app"
mkdir -p "${APP_NAME}.app/Contents/MacOS"
mkdir -p "${APP_NAME}.app/Contents/Resources"

cp -r publish/osx-x64/* "${APP_NAME}.app/Contents/MacOS/"
cp packaging/macos/Info.plist "${APP_NAME}.app/Contents/"

# Update version in plist
/usr/libexec/PlistBuddy -c "Set :CFBundleVersion ${VERSION}" "${APP_NAME}.app/Contents/Info.plist"
/usr/libexec/PlistBuddy -c "Set :CFBundleShortVersionString ${VERSION}" "${APP_NAME}.app/Contents/Info.plist"

chmod +x "${APP_NAME}.app/Contents/MacOS/WordSprinter"

# Create DMG
create-dmg \
    --volname "${APP_NAME}" \
    --window-pos 200 120 \
    --window-size 600 400 \
    --icon-size 100 \
    --app-drop-link 450 185 \
    "${DMG_NAME}" \
    "${APP_NAME}.app"

echo "Created: ${DMG_NAME}"
