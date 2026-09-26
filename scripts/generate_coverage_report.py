#!/usr/bin/env python3
"""Generate one Markdown line-coverage report from SeniorCare test artifacts."""

import argparse
import xml.etree.ElementTree as element_tree
from collections.abc import Iterable
from pathlib import Path


def line_coverage_from_lcov(files: Iterable[Path]) -> tuple[int, int]:
    lines: dict[tuple[str, int], int] = {}
    for file in files:
        source_file = ""
        for record in file.read_text(encoding="utf-8").splitlines():
            if record.startswith("SF:"):
                source_file = record[3:]
            elif record.startswith("DA:"):
                line_number, hits = record[3:].split(",", maxsplit=1)
                key = (source_file, int(line_number))
                lines[key] = max(lines.get(key, 0), int(hits))
    return sum(hits > 0 for hits in lines.values()), len(lines)


def line_coverage_from_cobertura(files: Iterable[Path]) -> tuple[int, int]:
    lines: dict[tuple[str, int], int] = {}
    for file in files:
        root = element_tree.parse(file).getroot()
        for class_element in root.findall(".//class"):
            filename = class_element.attrib["filename"]
            for line in class_element.findall("./lines/line"):
                key = (filename, int(line.attrib["number"]))
                lines[key] = max(lines.get(key, 0), int(line.attrib["hits"]))
    return sum(hits > 0 for hits in lines.values()), len(lines)


def format_row(name: str, covered: int, total: int, bold: bool = False) -> str:
    coverage = covered / total * 100 if total else 0
    values = (name, str(covered), str(total), f"{coverage:.1f}%")
    if bold:
        values = tuple(f"**{value}**" for value in values)
    return f"| {values[0]} | {values[1]} | {values[2]} | {values[3]} |"


def generate_report(backend: Path, care: Path, stock: Path, portal: Path) -> str:
    components = (
        ("Backend", line_coverage_from_cobertura(backend.rglob("coverage.cobertura.xml"))),
        ("Care web", line_coverage_from_lcov(care.rglob("lcov.info"))),
        ("Stock web", line_coverage_from_lcov(stock.rglob("lcov.info"))),
        ("Senior portal", line_coverage_from_lcov(portal.rglob("lcov.info"))),
    )
    missing = [name for name, (_, total) in components if total == 0]
    if missing:
        raise ValueError(f"Artefato de cobertura ausente ou vazio: {', '.join(missing)}")

    below_minimum = [
        f"{name} ({covered / total * 100:.1f}%)"
        for name, (covered, total) in components
        if covered / total * 100 < 80
    ]
    if below_minimum:
        raise ValueError(
            f"Cobertura mínima de linhas de 80.0% não atingida: {', '.join(below_minimum)}"
        )

    covered = sum(value[0] for _, value in components)
    total = sum(value[1] for _, value in components)
    rows = [format_row(name, *value) for name, value in components]
    rows.append(format_row("Total", covered, total, bold=True))
    return "\n".join((
        "# Relatório de cobertura",
        "",
        "| Componente | Linhas cobertas | Linhas instrumentadas | Cobertura |",
        "| --- | ---: | ---: | ---: |",
        *rows,
        "",
    ))


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--backend", type=Path, default=Path("SeniorCareManager-Backend/TestResults"))
    parser.add_argument("--care", type=Path, default=Path("SeniorCareManager-Frontend/SeniorCareManagerFrontend/coverage"))
    parser.add_argument("--stock", type=Path, default=Path("SeniorStockManager-Frontend/SeniorStockManagerFrontend/coverage"))
    parser.add_argument("--portal", type=Path, default=Path("SeniorPortal-Frontend/SeniorPortalFrontend/coverage"))
    parser.add_argument("--output", type=Path, default=Path("coverage-report.md"))
    arguments = parser.parse_args()

    try:
        report = generate_report(arguments.backend, arguments.care, arguments.stock, arguments.portal)
    except (element_tree.ParseError, OSError, ValueError) as error:
        parser.error(str(error))
    arguments.output.write_text(report, encoding="utf-8")
    print(arguments.output)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
