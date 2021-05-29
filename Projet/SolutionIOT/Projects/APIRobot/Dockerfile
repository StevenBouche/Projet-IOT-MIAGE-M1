#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Projects/APIRobot/APIRobot.csproj", "Projects/APIRobot/"]
COPY ["Libraries/MongoDBAccess/MongoDBAccess.csproj", "Libraries/MongoDBAccess/"]
COPY ["Libraries/ConfigPolicy/ConfigPolicy.csproj", "Libraries/ConfigPolicy/"]
COPY ["Projects/SharedModels/SharedModels.csproj", "Projects/SharedModels/"]
RUN dotnet restore "Projects/APIRobot/APIRobot.csproj"
COPY . .
WORKDIR "/src/Projects/APIRobot"
RUN dotnet build "APIRobot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "APIRobot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "APIRobot.dll"]