FROM node:22-alpine AS build
WORKDIR /app

# Versão/commit do release, injetados pelo CI (release.yml). Default = dev.
ARG VERSION=0.0.0
ARG GIT_SHA=unknown

COPY package.json package-lock.json ./
RUN npm ci

COPY . .
RUN npm run build \
    && printf '{"app":"senior-portal","version":"%s","commit":"%s","buildTime":"%s"}\n' \
       "$VERSION" "$GIT_SHA" "$(date -u +%Y-%m-%dT%H:%M:%SZ)" > dist/version.json

FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

# §8.1/§4.1 — gera public-config.json a partir de PUBLIC_NAME antes do nginx
# subir, usando o mecanismo padrão de entrypoint da imagem oficial (roda
# qualquer script executável em /docker-entrypoint.d/).
COPY docker-entrypoint.d/10-public-config.sh /docker-entrypoint.d/10-public-config.sh

# Capacidade de bind em porta privilegiada via setcap, em vez de rodar o processo como
# root — master, workers E o script de entrypoint (roda como o usuário do container,
# não como root, já que USER nginx vale pro processo inteiro) ficam como usuário
# nginx. Por isso /usr/share/nginx/html — onde o script sobrescreve
# public-config.json — precisa estar no chown abaixo.
RUN apk add --no-cache libcap && \
    chmod +x /docker-entrypoint.d/10-public-config.sh && \
    setcap 'cap_net_bind_service=+ep' /usr/sbin/nginx && \
    chown -R nginx:nginx /var/cache/nginx /var/log/nginx /usr/share/nginx/html /etc/nginx/conf.d && \
    touch /run/nginx.pid && chown nginx:nginx /run/nginx.pid
USER nginx

EXPOSE 80

HEALTHCHECK --interval=30s --timeout=5s --retries=3 \
    CMD curl -f http://localhost/ || exit 1
