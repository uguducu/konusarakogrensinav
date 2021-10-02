using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SinavOlusturma.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SinavOlusturma.Servis
{
    [Route("api/[controller]")]
    [ApiController]
    public class AciklamaGetirController : ControllerBase
    {
        [HttpPost]
        public AciklamaGetirModel Index()
        {
            AciklamaGetirModel model = new AciklamaGetirModel();
            string link = Request.Form["link"].ToString();

            Uri url = new Uri("https://www.wired.com" + link);
            WebClient client = new WebClient();
            string html = client.DownloadString(url);
            HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
            document.LoadHtml(html);
            HtmlNodeCollection aciklama = document.DocumentNode.SelectNodes("//div[contains(@class, 'grid--item body body__container article__body grid-layout__content')]");

            model.body = aciklama[0].InnerText;
            return model;
        }
    }
}
