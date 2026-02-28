# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 7004

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Passport/QuestFlag.Passport.AdminWebApp/QuestFlag.Passport.AdminWebApp/QuestFlag.Passport.AdminWebApp.csproj", "Passport/QuestFlag.Passport.AdminWebApp/QuestFlag.Passport.AdminWebApp/"]
COPY ["Passport/QuestFlag.Passport.AdminWebApp/QuestFlag.Passport.AdminWebApp.Client/QuestFlag.Passport.AdminWebApp.Client.csproj", "Passport/QuestFlag.Passport.AdminWebApp/QuestFlag.Passport.AdminWebApp.Client/"]
COPY ["Passport/QuestFlag.Passport.AdminClient/QuestFlag.Passport.AdminClient.csproj", "Passport/QuestFlag.Passport.AdminClient/"]
RUN dotnet restore "./Passport/QuestFlag.Passport.AdminWebApp/QuestFlag.Passport.AdminWebApp/QuestFlag.Passport.AdminWebApp.csproj"
COPY . .
WORKDIR "/src/Passport/QuestFlag.Passport.AdminWebApp/QuestFlag.Passport.AdminWebApp"
RUN dotnet build "./QuestFlag.Passport.AdminWebApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./QuestFlag.Passport.AdminWebApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "QuestFlag.Passport.AdminWebApp.dll"]
