FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build-env
WORKDIR /app

COPY PasteMyst.Web.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine as runtime

WORKDIR /publish
COPY --from=build-env /publish .
EXPOSE 8080

# install guesslang-bun
RUN apk add --no-cache wget
RUN wget https://github.com/CodeMyst/guesslang-bun/releases/download/1.0.0/guesslang-bun -O /bin/guesslang-bun

ENTRYPOINT [ "dotnet", "pastemyst.dll" ]
