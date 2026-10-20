#!/usr/bin/env python3
"""Import the author's free ZIP, without redistributing the licensed source art."""
import argparse
from pathlib import Path
from zipfile import ZipFile


def main():
    parser = argparse.ArgumentParser(description="Importa Modern Interiors Free v2.2.")
    parser.add_argument("zip", type=Path, help="ZIP baixado na página oficial da LimeZu")
    args = parser.parse_args()
    target = Path(__file__).resolve().parents[1] / "Assets/Art/Licensed/ModernInteriors"
    prefix = "Modern tiles_Free/"
    files = {
        prefix + "Interiors_free/16x16/Room_Builder_free_16x16.png": "RoomBuilder.png",
        prefix + "LICENSE.txt": "LICENSE.txt",
    }
    with ZipFile(args.zip) as archive:
        missing = set(files) - set(archive.namelist())
        if missing:
            parser.error("ZIP incompatível. Baixe Modern_Interiors_Free_v2.2.zip na página oficial.")
        target.mkdir(parents=True, exist_ok=True)
        for source, name in files.items():
            (target / name).write_bytes(archive.read(source))
    print("Artes importadas. No Unity, abra Percurso.unity ou reconstrua pelo menu Percurso.")


if __name__ == "__main__":
    main()
