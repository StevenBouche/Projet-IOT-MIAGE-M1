#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app
EXPOSE 1883

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["MQTTBroker/MQTTBroker.csproj", "MQTTBroker/"]
RUN dotnet restore "MQTTBroker/MQTTBroker.csproj"
COPY . .
WORKDIR "/src/MQTTBroker"
RUN dotnet build "MQTTBroker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MQTTBroker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MQTTBroker.dll"]
