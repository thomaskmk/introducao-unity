#!/usr/bin/env bash
set -euo pipefail
project_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
output_zip="${1:-$project_dir/Builds/percurso-webgl.zip}"
python3 - "$project_dir/Builds/WebGL" "$output_zip" <<'PYTHON'
from pathlib import Path
from zipfile import ZipFile, ZIP_DEFLATED
import sys
source = Path(sys.argv[1])
output = Path(sys.argv[2]).resolve()
if not (source / 'index.html').is_file():
    raise SystemExit('Gere o WebGL antes de compactar.')
if source.resolve() in output.parents:
    raise SystemExit('Salve o ZIP fora da pasta WebGL para evitar incluir o próprio ZIP.')
output.parent.mkdir(parents=True, exist_ok=True)
with ZipFile(output, 'w', ZIP_DEFLATED) as archive:
    for path in sorted(source.rglob('*')):
        if path.is_file() and '.DS_Store' not in path.parts:
            archive.write(path, path.relative_to(source))
print(f'ZIP pronto para itch.io: {output}')
PYTHON
