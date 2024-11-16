# 使用較小的基礎映像
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY Aron.NodePayMiner.csproj .
RUN dotnet restore "Aron.NodePayMiner.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet publish "Aron.NodePayMiner.csproj" -c $BUILD_CONFIGURATION -o /app/publish --self-contained -r linux-x64

# 使用較小的運行時基礎映像
FROM debian:bullseye-20240722-slim AS final
EXPOSE 50003/tcp
ENV ASPNETCORE_HTTP_PORTS 50003
ENV ASPNETCORE_URLS http://0.0.0.0:$ASPNETCORE_HTTP_PORTS
WORKDIR /app

RUN apt-get update -y
RUN apt-get install -y wget

#RUN wget https://dl.google.com/linux/direct/google-chrome-stable_current_amd64.deb -O browser.deb
#RUN apt-get install -y ./browser.deb
#RUN rm browser.deb

RUN apt-get install -y --no-install-recommends \
    curl \
    net-tools \
    iputils-ping \
    ca-certificates \
    chromium \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .
RUN chmod +x Aron.NodePayMiner

ENTRYPOINT ["./Aron.NodePayMiner"]
