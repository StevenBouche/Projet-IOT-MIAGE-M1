#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Projects/MongoSeeding/MongoSeeding.csproj", "Projects/MongoSeeding/"]
COPY ["Libraries/MongoDBAccess/MongoDBAccess.csproj", "Libraries/MongoDBAccess/"]
COPY ["Projects/SharedModels/SharedModels.csproj", "Projects/SharedModels/"]
RUN dotnet restore "Projects/MongoSeeding/MongoSeeding.csproj"
COPY . .
WORKDIR "/src/Projects/MongoSeeding"
RUN dotnet build "MongoSeeding.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MongoSeeding.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MongoSeeding.dll"]