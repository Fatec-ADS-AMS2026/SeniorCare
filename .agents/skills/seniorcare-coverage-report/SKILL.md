---
name: seniorcare-coverage-report
description: Generate a current code-coverage report for every SeniorCare application. Use when the user asks for test coverage, code coverage, or a consolidated SeniorCare coverage report.
---

# SeniorCare coverage report

Run this from the repository root:

```bash
make coverage
```

It executes the existing collectors for all four applications and writes `coverage-report.md`:

- Backend: `coverlet.collector` over unit and integration tests.
- Care web: Vitest V8 coverage.
- Stock web: Vitest V8 coverage.
- Senior portal: Vitest V8 coverage.

Read `coverage-report.md` and report the line-coverage percentage per component and the total. Do not claim quality from coverage alone; identify changed or critical uncovered behavior only when asked.

## Failure handling

- A test or collector failure means no report is current; fix or report that failure rather than reusing an older artifact.
- Missing or empty coverage artifacts fail the report generator deliberately.
- `coverage-report.md` is local and ignored by Git. CI publishes the raw component artifacts for 14 days.
