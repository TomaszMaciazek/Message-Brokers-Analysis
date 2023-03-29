#Build
FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build
WORKDIR /source
COPY . .
RUN dotnet restore "./Producer.RabbitMQ/Producer.RabbitMQ.csproj" --disable-parallel
RUN dotnet publish "./Producer.RabbitMQ/Producer.RabbitMQ.csproj" -c release -o /app --no-restore

#Server
FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine
WORKDIR /app
COPY --from=build /app ./

ENTRYPOINT ["dotnet", "Producer.RabbitMQ.dll"]