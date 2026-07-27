FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env

WORKDIR /app
RUN mkdir EventBus

WORKDIR /app/EventBus
RUN mkdir EventBus
RUN mkdir EventBusRabbitMQ
RUN mkdir IntegrationEventLogEF

WORKDIR /app/src
RUN mkdir Application
RUN mkdir Domain
RUN mkdir Infrastructure


WORKDIR /app/src
COPY ./Backend/src/Application/*.csproj ./Application
COPY ./Backend/src/Domain/*.csproj ./Domain
COPY ./Backend/src/Infrastructure/*.csproj ./Infrastructure

WORKDIR /app/EventBus
COPY ./Backend/EventBus/EventBus/*.csproj ./EventBus
COPY ./Backend/EventBus/IntegrationEventLogEF/*.csproj ./IntegrationEventLogEF
COPY ./Backend/EventBus/EventBusRabbitMQ/*.csproj ./EventBusRabbitMQ

WORKDIR /app/src/Application
RUN dotnet restore



# Copy everything else and build
WORKDIR /app
COPY ./Backend/. ./

WORKDIR /app/src/Application
RUN dotnet publish -c Release -o out

# Build runtime image
FROM  mcr.microsoft.com/dotnet/sdk:7.0
WORKDIR /app
COPY --from=build-env /app/src/Application/out .
ENTRYPOINT ["dotnet", "Application.dll"]