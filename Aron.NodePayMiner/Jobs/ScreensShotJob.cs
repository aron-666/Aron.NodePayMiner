﻿using Aron.NodePayMiner.Models;
using Aron.NodePayMiner.Services;
using OpenQA.Selenium;
using Quartz;
using System.Drawing;
using System.Net;
using System.Xml.Linq;

namespace Aron.NodePayMiner.Jobs
{
    public class ScreensShotJob(MinerRecord _minerRecord, IMinerService minerService) : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            try
            {
                if (minerService.driver == null)
                {
                    return Task.CompletedTask;
                }

                // 設置瀏覽器窗口大小以包含整個網頁
                minerService.driver.Manage().Window.Size = new Size(1024, 768);

                // 等待窗口調整完成
                Thread.Sleep(1000);

                // 截圖
                Screenshot screenshot = ((ITakesScreenshot)minerService.driver).GetScreenshot();
                _minerRecord.Base64Image = "data:image/png;base64," + screenshot.AsBase64EncodedString;

            }
            catch (Exception e)
            {
            }
            return Task.CompletedTask;

        }




    }
}
