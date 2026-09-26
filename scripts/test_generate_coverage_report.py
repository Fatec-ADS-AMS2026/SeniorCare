import subprocess
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parent.parent
SCRIPT = ROOT / "scripts" / "generate_coverage_report.py"


class GenerateCoverageReportTests(unittest.TestCase):
    def test_combines_backend_and_frontend_line_coverage(self) -> None:
        with tempfile.TemporaryDirectory() as temporary_directory:
            temporary_path = Path(temporary_directory)
            backend = temporary_path / "backend"
            care = temporary_path / "care"
            stock = temporary_path / "stock"
            portal = temporary_path / "portal"
            for directory in (backend, care, stock, portal):
                directory.mkdir()

            (backend / "coverage.cobertura.xml").write_text(
                """<coverage><packages><package><classes>
                <class filename="Api.cs"><lines>
                <line number="1" hits="1"/><line number="2" hits="1"/>
                </lines></class>
                <class filename="Other.cs"><lines>
                <line number="1" hits="1"/>
                </lines></class>
                </classes></package></packages></coverage>""",
                encoding="utf-8",
            )
            (care / "lcov.info").write_text("SF:Care.ts\nDA:1,1\nDA:2,1\nend_of_record\n", encoding="utf-8")
            (stock / "lcov.info").write_text("SF:Stock.ts\nDA:1,1\nend_of_record\n", encoding="utf-8")
            (portal / "lcov.info").write_text("SF:Portal.ts\nDA:1,1\nend_of_record\n", encoding="utf-8")
            report = temporary_path / "coverage-report.md"

            result = subprocess.run(
                [
                    sys.executable,
                    str(SCRIPT),
                    "--backend", str(backend),
                    "--care", str(care),
                    "--stock", str(stock),
                    "--portal", str(portal),
                    "--output", str(report),
                ],
                cwd=ROOT,
                capture_output=True,
                text=True,
            )

            self.assertEqual(result.returncode, 0, result.stderr)
            self.assertEqual(
                report.read_text(encoding="utf-8"),
                """# Relatório de cobertura\n\n| Componente | Linhas cobertas | Linhas instrumentadas | Cobertura |\n| --- | ---: | ---: | ---: |\n| Backend | 3 | 3 | 100.0% |\n| Care web | 2 | 2 | 100.0% |\n| Stock web | 1 | 1 | 100.0% |\n| Senior portal | 1 | 1 | 100.0% |\n| **Total** | **7** | **7** | **100.0%** |\n""",
            )

    def test_rejects_any_component_below_the_minimum_line_coverage(self) -> None:
        with tempfile.TemporaryDirectory() as temporary_directory:
            temporary_path = Path(temporary_directory)
            backend = temporary_path / "backend"
            care = temporary_path / "care"
            stock = temporary_path / "stock"
            portal = temporary_path / "portal"
            for directory in (backend, care, stock, portal):
                directory.mkdir()

            (backend / "coverage.cobertura.xml").write_text(
                """<coverage><packages><package><classes><class filename="Api.cs"><lines>
                <line number="1" hits="1"/>
                </lines></class></classes></package></packages></coverage>""",
                encoding="utf-8",
            )
            (care / "lcov.info").write_text("SF:Care.ts\nDA:1,1\nend_of_record\n", encoding="utf-8")
            (stock / "lcov.info").write_text("SF:Stock.ts\nDA:1,0\nDA:2,1\nend_of_record\n", encoding="utf-8")
            (portal / "lcov.info").write_text("SF:Portal.ts\nDA:1,1\nend_of_record\n", encoding="utf-8")

            result = subprocess.run(
                [
                    sys.executable,
                    str(SCRIPT),
                    "--backend", str(backend),
                    "--care", str(care),
                    "--stock", str(stock),
                    "--portal", str(portal),
                ],
                cwd=ROOT,
                capture_output=True,
                text=True,
            )

            self.assertNotEqual(result.returncode, 0)
            self.assertIn("Stock web (50.0%)", result.stderr)


if __name__ == "__main__":
    unittest.main()
