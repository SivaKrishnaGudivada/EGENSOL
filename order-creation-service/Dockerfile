FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

RUN apt-get update
RUN apt-get install -y netcat
RUN apt-get install -y nano
RUN apt-get install iputils-ping
RUN apt-get install net-tools

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "order-creation-service.dll"]