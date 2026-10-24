#!/usr/bin/env python3

import json
import shutil
import sys
from pathlib import Path

CURRENT_SCHEMA_VERSION = 2


def main(arguments: list[str]) -> int:
    path = Path(arguments[0] if arguments else "products.json")
    try:
        legacy = json.loads(path.read_text())
        entries = []

        for entry in legacy:
            left, right = entry["Name"].strip().split("-", 1)
            entries.append({"Id": entry["Id"], "Left": left, "Right": int(right)})

        backup = Path(f"{path}.v1.bak")
        shutil.copy2(path, backup)
        document = {"schemaVersion": CURRENT_SCHEMA_VERSION, "data": entries}
        path.write_text(json.dumps(document, indent=2) + "\n")
    except (OSError, KeyError, TypeError, ValueError) as error:
        print(f"Migration failed: {error}", file=sys.stderr)
        return 1

    print(f"Migrated {path} to schema version {CURRENT_SCHEMA_VERSION}.")
    print(f"Legacy backup: {backup}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[1:]))
