FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build-env
WORKDIR /app

COPY api/PasteMyst.Web/*.csproj .
RUN dotnet restore
COPY api/PasteMyst.Web/ .
RUN dotnet publish -c Release -o /publish

# Build guesslang-bun in its own build stage using oven/bun-alpine
FROM oven/bun:alpine AS guesslang-build
WORKDIR /guesslang-bun

# Install required dependencies (git is not included in oven/bun-alpine)
RUN apk add --no-cache git

# Clone the guesslang-bun repository source code
RUN git clone https://github.com/CodeMyst/guesslang-bun.git .

# Install dependencies and build the project
RUN bun install
RUN bun build index.ts --compile --outfile dist/index.js --target node

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /publish

# Copy published .NET application
COPY --from=build-env /publish .
COPY ../.git .git
EXPOSE 8080

# Copy guesslang-bun binary from build stage, and make it executable
COPY --from=guesslang-build /guesslang-bun/dist/index.js /bin/guesslang-bun
RUN chmod +x /bin/guesslang-bun

ENTRYPOINT [ "dotnet", "PasteMyst.Web.dll" ]
