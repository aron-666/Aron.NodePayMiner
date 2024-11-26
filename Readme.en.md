![Total Visitors](https://komarev.com/ghpvc/?username=aron-666NodePayMiner&color=green)

[![en](https://img.shields.io/badge/lang-en-red.svg)](https://github.com/aron-666/Aron.NodePayMiner/blob/master/README.en.md)
[![中文](https://img.shields.io/badge/lang-中文-blue.svg)](https://github.com/aron-666/Aron.NodePayMiner)

# Aron.NodePayMiner
Written in .Net 8

## If you find it useful, support me by using my referral code: NwWtzg6qBuzUzXD
[Register Now at app.nodepay.ai](https://app.nodepay.ai/register?ref=NwWtzg6qBuzUzXD)



## Execution Screenshots
1. Login
![image](https://github.com/aron-666/Aron.NodePayMiner/blob/master/%E6%88%AA%E5%9C%96/%E5%BE%8C%E8%87%BA%E7%99%BB%E5%85%A5%E7%95%AB%E9%9D%A2.png?raw=true)

2. Mining Information
![image](https://github.com/aron-666/Aron.NodePayMiner/blob/master/%E6%88%AA%E5%9C%96/%E6%8C%96%E7%A4%A6%E7%95%AB%E9%9D%A2.png?raw=true)

## 1. Docker Installation
1. Install Docker
   - Windows: [Docker Desktop](https://www.docker.com/products/docker-desktop/)
   - Linux: If you're using Linux, you probably know how to do this already.

2. Edit docker-compose.yml (In the docker-install folder of the source code)
   ```
   version: '1'
   services:
      nodepay:
         image: aron666/nodepay
         container_name: nodepay
         environment:
            - NP_TOKEN=token
            - ADMIN_USER=admin
            - ADMIN_PASS=admin
            - PROXY_ENABLE=false # true
            - PROXY_HOST=http(s)://host:port
            - PROXY_USER=user
            - PROXY_PASS=pass
         ports:
            - 5003:50003
   ```

   - Port 5003 will open a port on your computer. Open firewall port 5003 for LAN access.

3. Execute
   ```
   //cmd, navigate to the directory first (docker-install)
   docker compose up -d
   or
   docker-compose up -d
   ```
   Then, you can check the backend status using the following URLs:

   - Local: [http://localhost:5003](http://localhost:5003)
   - Other devices: Open cmd and type `ipconfig`/`ifconfig` to find your LAN IP, then access [http://IP:5003](http://IP:5003)
     - The process continues even if the webpage is closed.
     - For Windows auto-start, adjust settings in Docker Desktop.


# Support the Project

If you find this automated mining bot helpful and would like to support my ongoing development, feel free to:

☕ **Buy me a coffee!** ☕

Your support is like a freshly brewed cup of coffee that keeps me energized to keep coding!

### Donation Addresses
- **BEP20 (USDT/BNB, etc.):** `0xd14ee77edb0a052eb955db6fcd2a1cdcafeef53e`
- **TRC20 (USDT, etc.):** `THrEH2tKHxCUiSiuFpGhU99Y4QdChW8dub`

Thank you for your generosity! 🙌