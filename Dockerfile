#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
RUN apt-get update
RUN apt-get install -y default-jre
RUN apt-get install -y xvfb
EXPOSE 8080
EXPOSE 8081

# Set environment variables for Xvfb
ENV DISPLAY=:99

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["FileValidation.API.csproj", "."]
RUN dotnet restore "./FileValidation.API.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./FileValidation.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FileValidation.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileValidation.API.dll"]