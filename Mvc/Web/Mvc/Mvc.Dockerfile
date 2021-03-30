#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["Mvc.csproj", "./"]
RUN dotnet restore "./Mvc.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "Mvc.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Mvc.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS http://*:5000
ENTRYPOINT ["dotnet", "Mvc.dll"]