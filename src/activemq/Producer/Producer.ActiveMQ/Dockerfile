#Build
FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build
WORKDIR /source
COPY . .
RUN dotnet restore "./Producer.ActiveMQ/Producer.ActiveMQ.csproj" --disable-parallel
RUN dotnet publish "./Producer.ActiveMQ/Producer.ActiveMQ.csproj" -c release -o /app --no-restore

#Server
FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine
WORKDIR /app
COPY --from=build /app ./

ENTRYPOINT ["dotnet", "Producer.ActiveMQ.dll"]