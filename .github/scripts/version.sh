#!/usr/bin/env bash
# The csproj <Version> is set by hand and the release input is typed by hand. This is the check that they
# agree, because a tag that says one thing while the package says another is permanent once pushed.
set -euo pipefail

CSPROJ="${CSPROJ_FILE:-src/OpenApiExamples/OpenApiExamples.csproj}"

[ "${1:-}" = "check" ] && [ -n "${2:-}" ] || {
	echo "usage: version.sh check <version>" >&2
	exit 2
}

wanted="$2"
found="$(sed -n 's|.*<Version>\(.*\)</Version>.*|\1|p' "$CSPROJ" | head -1)"

[ -n "$found" ] || { echo "No <Version> in $CSPROJ." >&2; exit 1; }

if [ "$found" != "$wanted" ]; then
	echo "Releasing $wanted, but $CSPROJ says <Version>$found</Version>. Bump it and push before dispatching." >&2
	exit 1
fi

echo "$CSPROJ is $found."
