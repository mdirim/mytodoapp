# Build aşaması
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Proje dosyasını kopyala ve restore yap
COPY ["MyTodoApp.csproj", "./"]
RUN dotnet restore "MyTodoApp.csproj"

# Tüm dosyaları kopyala
COPY . .

# Release build
RUN dotnet publish "MyTodoApp.csproj" -c Release -o /app/publish

# Runtime aşaması
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render.com 10000 portunu bekler
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

# Health check için
HEALTHCHECK --interval=30s --timeout=5s --start-period=60s --retries=3 \
  CMD wget --no-verbose --tries=1 --spider http://localhost:10000/ || exit 1

ENTRYPOINT ["dotnet", "MyTodoApp.dll"]
