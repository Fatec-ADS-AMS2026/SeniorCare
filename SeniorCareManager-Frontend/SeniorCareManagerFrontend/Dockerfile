FROM node:22-alpine AS build
WORKDIR /app

# Versão/commit do release, injetados pelo CI (release.yml). Default = dev.
ARG VERSION=0.0.0
ARG GIT_SHA=unknown

COPY package.json package-lock.json ./
RUN npm ci

COPY . .
RUN npm run build \
    && printf '{"app":"care-web","version":"%s","commit":"%s","buildTime":"%s"}\n' \
       "$VERSION" "$GIT_SHA" "$(date -u +%Y-%m-%dT%H:%M:%SZ)" > dist/version.json

FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Capacidade de bind em porta privilegiada via setcap, em vez de rodar o processo como
# root — master e workers ficam como usuário nginx.
RUN apk add --no-cache libcap && \
    setcap 'cap_net_bind_service=+ep' /usr/sbin/nginx && \
    chown -R nginx:nginx /var/cache/nginx /var/log/nginx /usr/share/nginx/html /etc/nginx/conf.d && \
    touch /run/nginx.pid && chown nginx:nginx /run/nginx.pid
USER nginx

EXPOSE 80

HEALTHCHECK --interval=30s --timeout=5s --retries=3 \
    CMD curl -f http://localhost/ || exit 1
