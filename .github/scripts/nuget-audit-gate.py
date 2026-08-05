#!/usr/bin/env python3
"""Gate fail-closed para o relatório JSON de `dotnet list package --vulnerable`.

O comando em si sempre sai com código 0, mesmo com pacote vulnerável — o gate real
precisa ler o JSON e decidir. Bloqueia no limiar de severidade configurado; severidades
abaixo do limiar são reportadas mas não travam o gate.
"""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

SEVERITY_ORDER = {"low": 0, "moderate": 1, "high": 2, "critical": 3}


def collect_findings(report: dict) -> list[dict]:
    findings = []
    for project in report.get("projects", []):
        path = project.get("path", "?")
        for framework in project.get("frameworks", []):
            for key in ("topLevelPackages", "transitivePackages"):
                for pkg in framework.get(key, []):
                    for vuln in pkg.get("vulnerabilities", []):
                        findings.append({
                            "project": path,
                            "package": pkg.get("id", "?"),
                            "resolved": pkg.get("resolvedVersion", "?"),
                            "severity": (vuln.get("severity") or "").lower(),
                            "url": vuln.get("advisoryurl", "?"),
                            "transitive": key == "transitivePackages",
                        })
    return findings


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("report", type=Path)
    parser.add_argument("--min-severity", default="high", choices=sorted(SEVERITY_ORDER))
    args = parser.parse_args(argv)

    try:
        report = json.loads(args.report.read_text(encoding="utf-8-sig"))
    except (OSError, json.JSONDecodeError) as exc:
        print(f"SCA FALHOU: relatório inválido: {exc}", file=sys.stderr)
        return 2

    findings = collect_findings(report)
    threshold = SEVERITY_ORDER[args.min_severity]
    blocking = [f for f in findings if SEVERITY_ORDER.get(f["severity"], -1) >= threshold]

    print(f"SCA: {len(findings)} pacote(s) vulnerável(is), {len(blocking)} bloqueante(s) "
          f"com severidade >= {args.min_severity}.")
    for finding in findings:
        marker = "[BLOQUEANTE]" if finding in blocking else "[reportado]"
        kind = "transitiva" if finding["transitive"] else "direta"
        print(f"{marker} {finding['package']} {finding['resolved']} ({kind}) "
              f"severity={finding['severity']} {finding['url']} — {finding['project']}")

    return 1 if blocking else 0


if __name__ == "__main__":
    sys.exit(main())
