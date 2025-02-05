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

# Build pastemyst-decryptor
FROM dlang2/dmd-ubuntu:2.096.1 AS pastemyst-decryptor-build

RUN apt-get update && apt-get install -y git libscrypt-dev

WORKDIR /pastemyst-decryptor

RUN git clone --recurse-submodules https://github.com/pastemyst/pastemyst-decryptor.git .

ENV DC=dmd

RUN dub build --build=release

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /publish

# Copy published .NET application
COPY --from=build-env /publish .
COPY ../.git .git
EXPOSE 8080

# Copy guesslang-bun binary from build stage, and make it executable
COPY --from=guesslang-build /guesslang-bun/dist/index.js /bin/guesslang-bun
RUN chmod +x /bin/guesslang-bun

# Copy pastemyst-decryptor binary from build stage, and make it executable
COPY --from=pastemyst-decryptor-build /pastemyst-decryptor/pastemyst-decryptor /bin/pastemyst-decryptor
RUN chmod +x /bin/pastemyst-decryptor

ENTRYPOINT [ "dotnet", "PasteMyst.Web.dll" ]
