# Etapa de compilación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["LogiCore.Api/LogiCore.Api.csproj", "LogiCore.Api/"]
COPY ["LogiCore.Application/LogiCore.Application.csproj", "LogiCore.Application/"]
COPY ["LogiCore.Infrastructure/LogiCore.Infrastructure.csproj", "LogiCore.Infrastructure/"]
COPY ["LogiCore.Domain/LogiCore.Domain.csproj", "LogiCore.Domain/"]

RUN dotnet restore "LogiCore.Api/LogiCore.Api.csproj"

# Copy the rest of the code and compile
COPY . .
WORKDIR "/src/LogiCore.Api"
RUN dotnet build "LogiCore.Api.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "LogiCore.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# final stage: runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Listen on port 8080 by default (Railway usually uses the PORT variable; adjust if necessary)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LogiCore.Api.dll"]
