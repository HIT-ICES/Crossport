#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Crossport.csproj", "."]
RUN dotnet restore "./Crossport.csproj" -s https://api.nuget.org/v3/index.json -s "http://10.123.3.1:30888/" 
COPY . .
WORKDIR "/src/."
RUN dotnet build "Crossport.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Crossport.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Crossport.dll"]