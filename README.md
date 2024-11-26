![Total Visitors](https://komarev.com/ghpvc/?username=aron-666NodePayMiner&color=green)

[![en](https://img.shields.io/badge/lang-en-red.svg)](https://github.com/aron-666/Aron.NodePayMiner/blob/master/Readme.en.md)
[![中文](https://img.shields.io/badge/lang-中文-blue.svg)](https://github.com/aron-666/Aron.NodePayMiner)

# Aron.NodePayMiner 
使用.Net 8撰寫


## 好用請支持，使用我的推薦碼註冊: NwWtzg6qBuzUzXD
[立即註冊 app.nodepay.ai](https://app.nodepay.ai/register?ref=NwWtzg6qBuzUzXD)


## 執行畫面
1. 登入
![image](https://github.com/aron-666/Aron.NodePayMiner/blob/master/%E6%88%AA%E5%9C%96/%E5%BE%8C%E8%87%BA%E7%99%BB%E5%85%A5%E7%95%AB%E9%9D%A2.png?raw=true)

2. 挖礦資訊
![image](https://github.com/aron-666/Aron.NodePayMiner/blob/master/%E6%88%AA%E5%9C%96/%E6%8C%96%E7%A4%A6%E7%95%AB%E9%9D%A2.png?raw=true)

## 1. Docker 安裝
1. 安裝 Docker
   - Windows: [Docker Desktop](https://www.docker.com/products/docker-desktop/)
   - Linux: 你都會用Linux了還要我教？


2. 編輯 docker-compose.yml 
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

   - Port 5003 會在你電腦上開一個 Port，要讓區網連請開防火牆 Port 5003

3. 執行
   ```
   //cmd請先 cd 到資料夾目錄(docker-install)
   docker compose up -d
   或
   docker-compose up -d
   ```
   再來就可以用網址看後臺狀態了

   - 本機: [http://localhost:5003](http://localhost:5003)
   - 其他設備: 先開 cmd 打 `ipconfig`/`ifconfig` 找到你的區網 IP [http://IP:5003](http://IP:5003)
     - 關掉網頁還會繼續執行
     - Windows 要開機自動執行要去Docker Desktop設定改



# 支持此項目

如果您覺得這個自動挖礦機器人對您有所幫助，並希望支持我繼續開發，歡迎您：

☕ **請我喝杯咖啡！** ☕

您的支持就像一杯香醇的咖啡，讓我充滿能量繼續努力寫程式！

### 咖啡地址
- **BEP20（USDT/BNB 等）：** `0xd14ee77edb0a052eb955db6fcd2a1cdcafeef53e`
- **TRC20（USDT 等）：** `THrEH2tKHxCUiSiuFpGhU99Y4QdChW8dub`

感謝您的慷慨支持！ 🙌
