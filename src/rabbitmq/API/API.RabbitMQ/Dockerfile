#Build
FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build
WORKDIR /source
COPY . .
RUN dotnet restore "./API.RabbitMQ/API.RabbitMQ.csproj" --disable-parallel
RUN dotnet publish "./API.RabbitMQ/API.RabbitMQ.csproj" -c release -o /app --no-restore

#Server
FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine
WORKDIR /app
COPY --from=build /app ./

EXPOSE 5000
ENTRYPOINT ["dotnet", "API.RabbitMQ.dll"]