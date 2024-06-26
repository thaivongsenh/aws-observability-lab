#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["VotingData/VotingData.csproj", "VotingData/"]
RUN dotnet restore "VotingData/VotingData.csproj"
COPY . .
WORKDIR "/src/VotingData"
RUN dotnet build "VotingData.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VotingData.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VotingData.dll"]