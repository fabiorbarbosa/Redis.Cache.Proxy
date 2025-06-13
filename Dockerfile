FROM redis:alpine

# Atualiza os pacotes para corrigir vulnerabilidades
RUN apk update && apk upgrade --no-cache

# Expõe a porta padrão do Redis
EXPOSE 6379

# Comando padrão para iniciar o Redis
CMD ["redis-server", "--appendonly", "yes"]