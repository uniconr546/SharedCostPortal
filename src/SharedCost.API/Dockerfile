FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/SharedCost.Domain/SharedCost.Domain.csproj", "src/SharedCost.Domain/"]
COPY ["src/SharedCost.Application/SharedCost.Application.csproj", "src/SharedCost.Application/"]
COPY ["src/SharedCost.Infrastructure/SharedCost.Infrastructure.csproj", "src/SharedCost.Infrastructure/"]
COPY ["src/SharedCost.API/SharedCost.API.csproj", "src/SharedCost.API/"]
RUN dotnet restore "src/SharedCost.API/SharedCost.API.csproj"

COPY src/ src/
WORKDIR "/src/src/SharedCost.API"
RUN dotnet publish "SharedCost.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SharedCost.API.dll"]
