#!/usr/bin/env python3
"""Validate the checked-in Unity WebGL static demo path."""

from __future__ import annotations

import re
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
DOCS = ROOT / "docs"


def require(condition: bool, message: str) -> None:
    if not condition:
        raise SystemExit(f"FAIL: {message}")


def main() -> None:
    index = DOCS / "index.html"
    require(index.exists(), "docs/index.html missing")
    html = index.read_text(encoding="utf-8")
    require("createUnityInstance" in html, "Unity loader bootstrap missing")
    require("Chicken Coop" in html, "build title missing")

    build_url = re.search(r'var\s+buildUrl\s*=\s*"([^"]+)"', html)
    require(build_url is not None, "Unity buildUrl declaration missing")
    build_dir = build_url.group(1)
    refs = [
        f"{build_dir}/build.loader.js",
        f"{build_dir}/build.data",
        f"{build_dir}/build.framework.js",
        f"{build_dir}/build.wasm",
    ]
    require(refs, "docs/index.html does not reference Build assets")
    missing = [ref for ref in refs if not (DOCS / ref).exists()]
    require(not missing, "missing WebGL build files: " + ", ".join(missing))

    template_refs = ["TemplateData/favicon.ico", "TemplateData/style.css", "TemplateData/fullscreen-button.png"]
    missing_templates = [ref for ref in template_refs if not (DOCS / ref).exists()]
    require(not missing_templates, "missing TemplateData files: " + ", ".join(missing_templates))

    total_size = sum((DOCS / ref).stat().st_size for ref in refs)
    print("WEBGL DEMO OK")
    print(f"build_files={len(refs)} build_bytes={total_size}")
    print("demo_path=python3 -m http.server 8000 -d docs -> http://localhost:8000/")


if __name__ == "__main__":
    main()
