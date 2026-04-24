FROM mcr.microsoft.com/dotnet/sdk:10.0.203 AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src
COPY ["./backend/Backend.slnx", "./"]
COPY ["./backend/Backend.csproj", "./"]
RUN dotnet restore "Backend.slnx"

COPY ["./backend/", "./"]
RUN dotnet build "./Backend.slnx" --nologo --no-restore -c $BUILD_CONFIGURATION

FROM build AS publish
RUN dotnet publish "./Backend.csproj" \
    --nologo --no-restore --no-build -c $BUILD_CONFIGURATION \
    -o /app/publish /p:UseAppHost=false

FROM node:25.9.0-alpine AS node
WORKDIR /src
COPY ["./frontend/package.json", "./frontend/package-lock.json", "./"]
RUN npm ci
COPY ["./frontend/", "./"]
RUN npm run build

FROM mcr.microsoft.com/dotnet/aspnet:10.0.7 AS final
EXPOSE 80
HEALTHCHECK --interval=5s --timeout=5s CMD wget http://localhost/health -q -O - > /dev/null 2>&1

ENV ASPNETCORE_ENVIRONMENT="Production"
ENV ASPNETCORE_URLS="http://+:80"

RUN groupadd --system kb && useradd --system --no-create-home --gid kb kb
RUN mkdir /data && chown kb:kb /data

USER kb

WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=node /src/build/client ./wwwroot
ENTRYPOINT ["dotnet", "Backend.dll"]
