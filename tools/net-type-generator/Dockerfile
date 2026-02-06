FROM mcr.microsoft.com/dotnet/sdk:9.0

RUN apt-get update && apt-get install -y --no-install-recommends python3 \
    && rm -rf /var/lib/apt/lists/*

RUN dotnet tool install --global Corvus.Json.JsonSchema.TypeGeneratorTool \
    --version 4.5.2

RUN mkdir -p /app
RUN mkdir -p /work/{json,out}

COPY entrypoint.sh /app

ENTRYPOINT ["/app/entrypoint.sh"]
