# Giai đoạn Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

# Cài đặt Node.js
RUN curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs

# Copy file dự án và restore .NET
COPY ["src/AquaCMS/AquaCMS.csproj", "src/AquaCMS/"]
RUN dotnet restore "src/AquaCMS/AquaCMS.csproj"

# Copy code (đã có .dockerignore loại bỏ node_modules cũ)
COPY . .
WORKDIR "/source/src/AquaCMS"

# Cài đặt npm và sửa quyền thực thi cho các file trong node_modules/.bin
RUN npm install && \
    chmod -R +x node_modules/.bin && \
    npm run build:css

# Publish .NET
RUN dotnet publish "AquaCMS.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Giai đoạn Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "AquaCMS.dll"]
