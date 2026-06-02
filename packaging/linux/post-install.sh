#!/usr/bin/env bash
chmod +x /opt/wordsprinter/WordSprinter
if [ -f /usr/share/applications ]; then
    cp /opt/wordsprinter/wordsprinter.desktop /usr/share/applications/ 2>/dev/null || true
fi
