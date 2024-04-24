FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build-env
WORKDIR /app

COPY api/*.csproj .
RUN dotnet restore
COPY api/ .
RUN dotnet publish -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine as runtime
WORKDIR /publish
COPY --from=build-env /publish .
COPY ../.git .git
EXPOSE 8080

ENTRYPOINT [ "dotnet", "pastemyst.dll" ]
