FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY AiNotes.Api/AiNotes.Api.csproj AiNotes.Api/
RUN dotnet restore AiNotes.Api/AiNotes.Api.csproj

COPY AiNotes.Api/ AiNotes.Api/
WORKDIR /src/AiNotes.Api
RUN dotnet publish AiNotes.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AiNotes.Api.dll"]
