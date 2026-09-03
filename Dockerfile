# Stage 1: Build React Frontend
FROM node:20-alpine AS frontend-build
WORKDIR /app/frontend
COPY frontend/ipo-forge-web/package*.json ./
RUN npm ci
COPY frontend/ipo-forge-web/ ./
RUN npm run build

# Stage 2: Build .NET Backend
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build
WORKDIR /app
COPY IPOForge.slnx ./
COPY src/IPOForge.Domain/IPOForge.Domain.csproj src/IPOForge.Domain/
COPY src/IPOForge.Contracts/IPOForge.Contracts.csproj src/IPOForge.Contracts/
COPY src/IPOForge.Application/IPOForge.Application.csproj src/IPOForge.Application/
COPY src/IPOForge.Infrastructure/IPOForge.Infrastructure.csproj src/IPOForge.Infrastructure/
COPY src/IPOForge.Api/IPOForge.Api.csproj src/IPOForge.Api/
COPY src/IPOForge.Worker/IPOForge.Worker.csproj src/IPOForge.Worker/
COPY tests/IPOForge.UnitTests/IPOForge.UnitTests.csproj tests/IPOForge.UnitTests/
COPY tests/IPOForge.IntegrationTests/IPOForge.IntegrationTests.csproj tests/IPOForge.IntegrationTests/
RUN dotnet restore IPOForge.slnx

COPY . ./
RUN dotnet publish src/IPOForge.Api/IPOForge.Api.csproj -c Release -o /out

# Stage 3: Runtime Container
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_ENVIRONMENT=Production
ENV PORT=5000

COPY --from=backend-build /out ./
COPY --from=frontend-build /app/frontend/dist ./wwwroot

ENTRYPOINT ["dotnet", "IPOForge.Api.dll"]
