# Giai đoạn Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

# Cài đặt Node.js
RUN curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs

# Copy file dự án và restore .NET
COPY ["src/AquaCMS/AquaCMS.csproj", "src/AquaCMS/"]
RUN dotnet restore "src/AquaCMS/AquaCMS.csproj"

# Copy code
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

# CÀI ĐẶT MÚI GIỜ VIỆT NAM (Quan trọng)
USER root
RUN apt-get update && apt-get install -y tzdata && \
    ln -fs /usr/share/zoneinfo/Asia/Ho_Chi_Minh /etc/localtime && \
    dpkg-reconfigure --frontend noninteractive tzdata && \
    apt-get clean

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV TZ=Asia/Ho_Chi_Minh

ENTRYPOINT ["dotnet", "AquaCMS.dll"]
