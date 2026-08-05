#!/usr/bin/env python3
"""Gate fail-closed para relatórios SARIF produzidos pelo CodeQL.

Bloqueia resultados de segurança no limiar configurado e falha quando o
relatório não existe ou não pode ser interpretado. Resultados de qualidade sem
classificação de segurança permanecem no artefato para triagem, sem serem
indevidamente promovidos a vulnerabilidades.
"""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path


def sarif_files(target: Path) -> list[Path]:
    if target.is_file():
        return [target] if target.suffix.lower() == ".sarif" else []
    if target.is_dir():
        return sorted(target.rglob("*.sarif"))
    return []


def security_severity(rule: dict) -> float | None:
    value = rule.get("properties", {}).get("security-severity")
    if value is None:
        return None
    try:
        return float(value)
    except (TypeError, ValueError):
        raise ValueError(f"security-severity inválida para regra {rule.get('id', '?')}: {value!r}")


def evaluate(report: dict, threshold: float) -> tuple[int, list[dict]]:
    total = 0
    blocking: list[dict] = []
    for run in report.get("runs", []):
        rules = run.get("tool", {}).get("driver", {}).get("rules", [])
        by_id = {rule.get("id"): rule for rule in rules if rule.get("id")}
        for result in run.get("results", []):
            total += 1
            rule = by_id.get(result.get("ruleId"), {})
            if not rule and isinstance(result.get("ruleIndex"), int):
                index = result["ruleIndex"]
                if 0 <= index < len(rules):
                    rule = rules[index]
            severity = security_severity(rule)
            if severity is not None and severity >= threshold:
                location = "?"
                locations = result.get("locations", [])
                if locations:
                    physical = locations[0].get("physicalLocation", {})
                    artifact = physical.get("artifactLocation", {}).get("uri", "?")
                    line = physical.get("region", {}).get("startLine")
                    location = f"{artifact}:{line}" if line else artifact
                blocking.append({
                    "rule": result.get("ruleId") or rule.get("id", "?"),
                    "severity": severity,
                    "location": location,
                    "message": result.get("message", {}).get("text", "sem descrição"),
                })
    return total, blocking


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("target", type=Path)
    parser.add_argument("--min-security-severity", type=float, default=7.0)
    args = parser.parse_args(argv)

    files = sarif_files(args.target)
    if not files:
        print(f"SAST FALHOU: nenhum SARIF encontrado em {args.target}", file=sys.stderr)
        return 2

    total = 0
    blocking: list[dict] = []
    try:
        for path in files:
            report = json.loads(path.read_text(encoding="utf-8-sig"))
            file_total, file_blocking = evaluate(report, args.min_security_severity)
            total += file_total
            blocking.extend(file_blocking)
    except (OSError, json.JSONDecodeError, ValueError) as exc:
        print(f"SAST FALHOU: relatório SARIF inválido: {exc}", file=sys.stderr)
        return 2

    print(f"SAST: {len(files)} relatório(s), {total} resultado(s), "
          f"{len(blocking)} bloqueante(s) com security-severity >= {args.min_security_severity:.1f}.")
    for finding in blocking:
        print(f"[BLOQUEANTE] {finding['rule']} CVSS={finding['severity']:.1f} "
              f"{finding['location']} — {finding['message']}")
    return 1 if blocking else 0


if __name__ == "__main__":
    sys.exit(main())
