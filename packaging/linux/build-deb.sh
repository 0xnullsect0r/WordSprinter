#!/usr/bin/env bash
set -euo pipefail
VERSION="${1:-1.0.0}"

fpm -s dir -t deb \
    -n wordsprinter \
    -v "${VERSION}" \
    --description "WordSprinter — RSVP Ebook Reader" \
    --url "https://github.com/0xnullsect0r/WordSprinter" \
    --license "GPL-3.0" \
    --maintainer "WordSprinter" \
    --prefix /opt/wordsprinter \
    --after-install packaging/linux/post-install.sh \
    --after-remove packaging/linux/post-remove.sh \
    publish/linux-x64/=.

echo "DEB built: wordsprinter_${VERSION}_amd64.deb"
