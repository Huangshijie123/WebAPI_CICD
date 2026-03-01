# 基础运行镜像
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# 构建镜像
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 复制 csproj 文件并 restore
COPY ["WebAPI_CICD.csproj", "./"]
RUN dotnet restore "./WebAPI_CICD.csproj"

# 复制全部源码
COPY . .
RUN dotnet build "./WebAPI_CICD.csproj" -c $BUILD_CONFIGURATION -o /app/build

# 发布阶段
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./WebAPI_CICD.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# 运行镜像
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebAPI_CICD.dll"]