# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 7003

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Passport/QuestFlag.Passport.WebApp/QuestFlag.Passport.WebApp/QuestFlag.Passport.WebApp.csproj", "Passport/QuestFlag.Passport.WebApp/QuestFlag.Passport.WebApp/"]
COPY ["Passport/QuestFlag.Passport.WebApp/QuestFlag.Passport.WebApp.Client/QuestFlag.Passport.WebApp.Client.csproj", "Passport/QuestFlag.Passport.WebApp/QuestFlag.Passport.WebApp.Client/"]
COPY ["Passport/QuestFlag.Passport.UserClient/QuestFlag.Passport.UserClient.csproj", "Passport/QuestFlag.Passport.UserClient/"]
RUN dotnet restore "./Passport/QuestFlag.Passport.WebApp/QuestFlag.Passport.WebApp/QuestFlag.Passport.WebApp.csproj"
COPY . .
WORKDIR "/src/Passport/QuestFlag.Passport.WebApp/QuestFlag.Passport.WebApp"
RUN dotnet build "./QuestFlag.Passport.WebApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./QuestFlag.Passport.WebApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "QuestFlag.Passport.WebApp.dll"]
