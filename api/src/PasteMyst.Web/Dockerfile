FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build-env
WORKDIR /app

COPY api/src/PasteMyst.Web/*.csproj .
RUN dotnet restore
COPY api/src/PasteMyst.Web/ .
RUN dotnet publish -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine as runtime

WORKDIR /publish
COPY --from=build-env /publish .
COPY ../.git .git
EXPOSE 8080

# install guesslang-bun
RUN apk add --no-cache wget
RUN wget https://github.com/CodeMyst/guesslang-bun/releases/download/1.0.0/guesslang-bun -O /bin/guesslang-bun

ENTRYPOINT [ "dotnet", "PasteMyst.Web.dll" ]
