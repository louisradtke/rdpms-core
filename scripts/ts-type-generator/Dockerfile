FROM node:20-slim

RUN npm install -g json-schema-to-typescript

RUN mkdir -p /app
RUN mkdir -p /work/{json,out}

COPY entrypoint.sh /app

ENTRYPOINT ["/app/entrypoint.sh"]
