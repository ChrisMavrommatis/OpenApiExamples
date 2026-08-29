#!/usr/bin/env bash
# Reads CHANGELOG.md. The release workflow uses check and extract in its gate, and date to stamp the
# release day. Version headings are matched as `## [1.2.3]`, with or without a trailing date.
set -euo pipefail

FILE="${CHANGELOG_FILE:-CHANGELOG.md}"

usage() {
	echo "usage: changelog.sh check|extract <version> | changelog.sh date <version> <YYYY-MM-DD>" >&2
	exit 2
}

# Dots are regex in awk and sed, and every version has them. 1.1.0 would otherwise match 1x1y0.
escape() { printf '%s' "$1" | sed 's/[.[\*^$]/\\&/g'; }

body() {
	local pattern
	pattern="$(escape "$1")"
	awk -v pat="^## \\\\[${pattern}\\\\]" '
		$0 ~ pat { inside = 1; next }
		inside && /^## \[/ { exit }
		inside { print }
	' "$FILE"
}

# Strips leading and trailing blank lines and keeps the ones in between, so an empty section is genuinely
# empty and the extracted body still has the blank line every markdown list needs before the next heading.
trim() {
	awk '
		/[^[:space:]]/ { while (pending-- > 0) print ""; pending = 0; found = 1; print; next }
		found { pending++ }
	'
}

command="${1:-}"
version="${2:-}"
[ -n "$command" ] && [ -n "$version" ] || usage

grep -q "^## \[$(escape "$version")\]" "$FILE" || {
	echo "$FILE has no section for $version. Add '## [$version]' before releasing." >&2
	exit 1
}

case "$command" in
	check)
		if [ -z "$(body "$version" | trim)" ]; then
			echo "The $version section in $FILE is empty. Write the notes before releasing." >&2
			exit 1
		fi
		echo "$FILE has a $version section and it is not empty."
		;;
	extract)
		body "$version" | trim
		;;
	date)
		day="${3:-}"
		[ -n "$day" ] || usage
		printf '%s' "$day" | grep -qE '^[0-9]{4}-[0-9]{2}-[0-9]{2}$' || {
			echo "'$day' is not a date. Write 2026-08-30." >&2
			exit 1
		}
		tmp="$(mktemp)"
		sed "s|^## \[$(escape "$version")\].*$|## [$version] - $day|" "$FILE" > "$tmp"
		mv "$tmp" "$FILE"
		echo "Stamped $version as $day."
		;;
	*)
		usage
		;;
esac
