FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Emailing.Service/Emailing.Service/Emailing.Service.sln", "Emailing.Service/Emailing.Service/"]
COPY ["Emailing.Service/Emailing.Service/Emailing.Service.csproj", "Emailing.Service/Emailing.Service/"]
COPY ["Emailing.Service/Emailing.Service.Data/Emailing.Service.Data.csproj", "Emailing.Service/Emailing.Service.Data/"]

RUN dotnet restore "Emailing.Service/Emailing.Service/Emailing.Service.sln"

COPY . .

WORKDIR "/src/Emailing.Service/Emailing.Service"

RUN dotnet build "Emailing.Service.sln" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Emailing.Service.sln" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Emailing.Service.dll"]
