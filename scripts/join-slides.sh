#!/bin/sh
set -eu

output=$1
shift

{
	printf '%s\n' '---' 'marp: true' 'theme: todo' '---'
	first=1
	for slide do
		if [ "$first" -eq 0 ]; then
			printf '\n---\n\n'
		else
			printf '\n'
			first=0
		fi
		cat "$slide"
	done
} > "$output"
