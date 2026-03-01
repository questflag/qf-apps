# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Passport/QuestFlag.Passport.Services/QuestFlag.Passport.Services.csproj", "Passport/QuestFlag.Passport.Services/"]
COPY ["Passport/QuestFlag.Passport.ApiCore/QuestFlag.Passport.ApiCore.csproj", "Passport/QuestFlag.Passport.ApiCore/"]
COPY ["Passport/QuestFlag.Passport.Application/QuestFlag.Passport.Application.csproj", "Passport/QuestFlag.Passport.Application/"]
COPY ["Passport/QuestFlag.Passport.Domain/QuestFlag.Passport.Domain.csproj", "Passport/QuestFlag.Passport.Domain/"]
COPY ["Passport/QuestFlag.Passport.Core/QuestFlag.Passport.Core.csproj", "Passport/QuestFlag.Passport.Core/"]
RUN dotnet restore "./Passport/QuestFlag.Passport.Services/QuestFlag.Passport.Services.csproj"
COPY . .
WORKDIR "/src/Passport/QuestFlag.Passport.Services"
RUN dotnet build "./QuestFlag.Passport.Services.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./QuestFlag.Passport.Services.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "QuestFlag.Passport.Services.dll"]