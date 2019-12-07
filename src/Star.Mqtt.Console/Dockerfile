FROM mcr.microsoft.com/dotnet/core/runtime:3.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["Star.Mqtt.Console/Star.Mqtt.Console.csproj", "Star.Mqtt.Console/"]
COPY ["Star.Core/Star.Core.csproj", "Star.Core/"]
RUN dotnet restore "Star.Mqtt.Console/Star.Mqtt.Console.csproj"
COPY . .
WORKDIR "/src/Star.Mqtt.Console"
RUN dotnet build "Star.Mqtt.Console.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Star.Mqtt.Console.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Star.Mqtt.Console.dll"]