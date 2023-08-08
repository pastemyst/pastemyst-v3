FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /app

COPY api/*.csproj .
RUN dotnet restore
COPY api/ .
RUN dotnet publish -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:7.0 as runtime
WORKDIR /publish
COPY --from=build-env /publish .
COPY ../.git .git
EXPOSE 80

ENTRYPOINT [ "dotnet", "pastemyst.dll" ]
