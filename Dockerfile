# Giai đoạn Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

# Cài đặt Node.js để build Tailwind CSS
RUN curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs

# Copy file dự án và restore các package .NET
COPY ["src/AquaCMS/AquaCMS.csproj", "src/AquaCMS/"]
RUN dotnet restore "src/AquaCMS/AquaCMS.csproj"

# Copy toàn bộ code và build
COPY . .
WORKDIR "/source/src/AquaCMS"
RUN npm install
RUN dotnet publish "AquaCMS.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Giai đoạn Chạy (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

# Cấu hình cổng chạy trong container
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "AquaCMS.dll"]
