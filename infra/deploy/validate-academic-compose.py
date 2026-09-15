#!/usr/bin/env python3
"""Valida o Compose acadêmico já renderizado em JSON."""
import json
import re
import sys

config = json.load(sys.stdin)
services = config.get("services", {})
errors: list[str] = []
required = {"postgres", "seniorcare-api", "care-web", "stock-web", "senior-portal", "mailpit", "caddy"}
missing = required - services.keys()
if missing:
    errors.append(f"serviços obrigatórios ausentes: {', '.join(sorted(missing))}")

for name in ("seniorcare-api", "care-web", "stock-web", "senior-portal"):
    image = services.get(name, {}).get("image", "")
    if not re.fullmatch(r"ghcr\.io/.+:[^@]+@sha256:[0-9a-f]{64}", image):
        errors.append(f"{name} deve usar imagem GHCR pinada por digest")
    if services.get(name, {}).get("ports"):
        errors.append(f"{name} não pode publicar porta no host")

if services.get("seniorcare-api", {}).get("environment", {}).get("Bootstrap__RunOnStartup") != "false":
    errors.append("API acadêmica deve desabilitar bootstrap no startup")

for name in required:
    if name not in services:
        continue
    if not services[name].get("healthcheck"):
        errors.append(f"{name} não tem healthcheck")
    labels = services[name].get("labels", {})
    if labels.get("com.seniorcare.environment") != "academic":
        errors.append(f"{name} não tem label de ambiente acadêmico")

mailpit = services.get("mailpit", {})
if not re.fullmatch(r"axllent/mailpit:v\d+\.\d+\.\d+", mailpit.get("image", "")):
    errors.append("Mailpit deve usar tag de versão pinada")
for port in mailpit.get("ports", []):
    if port.get("host_ip") != "127.0.0.1":
        errors.append("UI do Mailpit deve publicar somente em 127.0.0.1")

postgres = services.get("postgres", {})
for port in postgres.get("ports", []):
    if port.get("host_ip") != "127.0.0.1":
        errors.append("PostgreSQL deve publicar somente em 127.0.0.1")

caddy_ports = services.get("caddy", {}).get("ports", [])
if len(caddy_ports) != 1 or caddy_ports[0].get("target") != 443:
    errors.append("Caddy deve ser a única borda HTTPS publicada")

network = config.get("networks", {}).get("seniorcare-net", {})
if network.get("name") != "seniorcare-academico-net":
    errors.append("rede acadêmica deve ter nome estável e próprio")

if errors:
    print("Configuração acadêmica inválida:", *[f"- {error}" for error in errors], sep="\n", file=sys.stderr)
    raise SystemExit(1)

print("Configuração acadêmica válida.")
