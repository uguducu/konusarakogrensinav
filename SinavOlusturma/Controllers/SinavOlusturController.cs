using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HtmlAgilityPack;
using SinavOlusturma.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Text;

namespace SinavOlusturma.Controllers
{
    public class SinavOlusturController : Controller
    {
        public class BasliklarVeLinkler
        {
            public string baslik { get; set; }
            public string link { get; set; }
        }
        public IActionResult Index()
        {
            try
            {
                if (HttpContext.Session.GetString("OturumAnahtari") != null)
                {
                    string tarih = DecryptString(Environment.GetEnvironmentVariable("GUVEN_ANAHTAR"), HttpContext.Session.GetString("OturumAnahtari"));

                    if (Convert.ToDateTime(tarih) > DateTime.Now.AddHours(12))
                    {
                        return Redirect("/Login"); //sesison tarihinden 12 saat geçtiği için logine gönderiyorum
                    }
                }
                else
                    return Redirect("/Login"); //oturumAnahtari null geldiği için logine gönderiyorum
            }
            catch
            {
                return Redirect("/Login"); //bir hata oluştuğu için logine aktarıyroum
            }

            using (var db = new SqlliteDbContext())
            {
                ViewBag.kullanıcıAdı = db.Kullanicilar.Where(x => x.Eposta == HttpContext.Session.GetString("kullanıcıAdı")).FirstOrDefault().KullaniciAdSoyad.ToString();

                string basharf = "";
                var kullanici = ViewBag.kullanıcıAdı.Split(" ");
                for (int idx = 0; idx < kullanici.Length; idx++)
                {
                    basharf += kullanici[idx].Substring(0, 1);
                }

                ViewBag.kullanıcıAdıBasHarf = basharf;
            }


            Uri url = new Uri("https://www.wired.com/most-recent/");
            WebClient client = new WebClient();
            string html = client.DownloadString(url);
            HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
            document.LoadHtml(html);
            HtmlNodeCollection basliklar = document.DocumentNode.SelectNodes("//h2");

            List<BasliklarVeLinkler> list = new List<BasliklarVeLinkler>();

            foreach (var item in basliklar)
            {
                BasliklarVeLinkler model = new BasliklarVeLinkler();
                model.baslik = item.InnerText.ToString();
                var link = item.ParentNode.OuterHtml.ToString();
                string[] splitLink = link.Split("\"");
                model.link = splitLink[3].ToString();

                list.Add(model);
            }

            ViewBag.basliklar = list;

            return View();
        }

        [HttpPost]
        public KaydetModel Index(IFormCollection collection)
        {
            KaydetModel model = new KaydetModel();
            Sinavlar sinav = new Sinavlar();
            Cevaplar cevap = new Cevaplar();

            try
            {
                #region Kontroller
                model.mesaj = "";
                sinav.Baslik = collection["baslik"].ToString();
                if (sinav.Baslik == "")
                    model.mesaj += "<div class=\"alert alert-danger\" role=\"alert\"><div class=\"alert-text\">Sınav Başlığını Seçmelisiniz!</div></div>";
                sinav.Aciklama = collection["aciklama"].ToString();
                if (sinav.Aciklama == "")
                    model.mesaj += "<div class=\"alert alert-danger\" role=\"alert\"><div class=\"alert-text\">Sınav Açıklmasını Seçmelisiniz!</div></div>";

                for (int idx = 1; idx < 6; idx++)
                {
                    Sorular sorular = new Sorular();
                    sorular.Soru = collection["soru-" + idx].ToString();
                    if (sorular.Soru == "")
                        model.mesaj += "<div class=\"alert alert-danger\" role=\"alert\"><div class=\"alert-text\">Soru-" + idx + " Boş bırakılamaz!" + "</div></div>";

                    sorular.DogruCevapId = Convert.ToInt32(collection["soru-" + idx + "-dogrucevap"].ToString());
                    if (sorular.DogruCevapId == 0)
                        model.mesaj += "<div class=\"alert alert-danger\" role=\"alert\"><div class=\"alert-text\">Soru-" + idx + " için doğru bir cevap seçin!" + "</div></div>";

                    for (int iyx = 1; iyx < 5; iyx++)
                    {
                        cevap.Yanit = collection["soru-" + idx + "-cevap-" + iyx].ToString();
                        if (cevap.Yanit == "")
                            model.mesaj += "<div class=\"alert alert-danger\" role=\"alert\"><div class=\"alert-text\">Soru-" + idx + " için " + iyx + ".cevap boş bırakılamaz!" + "</div></div>";
                    }
                }
                #endregion

                #region DB

                if (model.mesaj == "")
                {
                    #region Sınavlar Tablosunu Doldurma
                    sinav.OlusturulmaZamani = DateTime.Now.ToString();
                    sinav.Olusturan = HttpContext.Session.GetString("kullanıcıAdı");
                    sinav.YanıtlayanKisiSayisi = 0;
                    using (var db = new SqlliteDbContext())
                    {
                        db.Sinavlar.Add(sinav);
                        db.SaveChanges();
                    }
                    #endregion

                    #region Sorular ve Cevaplar Tablosunu Doldurma
                    for (int idx = 1; idx < 6; idx++)
                    {
                        Sorular soru = new Sorular();
                        soru.Soru = collection["soru-" + idx].ToString();
                        using (var db = new SqlliteDbContext())
                        {
                            soru.SinavId = db.Sinavlar.OrderBy(x=> x.SinavId).LastOrDefault().SinavId;
                            soru.OlusturmaZamani = DateTime.Now.ToString();
                            soru.Olusturan = HttpContext.Session.GetString("kullanıcıAdı");

                            db.Sorular.Add(soru);
                            db.SaveChanges();

                            var DogruCevap = Convert.ToInt32(collection["soru-" + idx + "-dogrucevap"]);

                            SoruSecenekleri secenekler = new SoruSecenekleri();
                            for (int iyx = 1; iyx < 5; iyx++)
                            {
                                secenekler.SoruSecenekleriId = db.SoruSecenekleri.OrderBy(x => x.SoruSecenekleriId).LastOrDefault().SoruSecenekleriId + 1;
                                secenekler.SoruId = db.Sorular.OrderBy(x => x.SoruId).LastOrDefault().SoruId;
                                secenekler.Secenek = collection["soru-" + idx + "-cevap-" + iyx].ToString();
                                secenekler.OlusturmaZamani = DateTime.Now.ToString();
                                secenekler.Olusturan = HttpContext.Session.GetString("kullanıcıAdı");

                                db.SoruSecenekleri.Add(secenekler);
                                db.SaveChanges();

                                if (iyx == DogruCevap)
                                    DogruCevap = db.SoruSecenekleri.OrderBy(x => x.SoruSecenekleriId).LastOrDefault().SoruSecenekleriId;
                            }

                            int SoruId = db.Sorular.OrderBy(x => x.SoruId).LastOrDefault().SoruId;
                            Sorular UpdateSorular = db.Sorular.Where(s => s.SoruId == SoruId).FirstOrDefault();
                            if (UpdateSorular == null)
                            {
                                throw new Exception("");
                            }

                            UpdateSorular.DogruCevapId = DogruCevap;
                            db.SaveChanges();
                        }
                    }
                    #endregion
                }

                #endregion
            }
            catch (Exception ex)
            {
                model.durum = "Başarisiz";
                model.mesaj = "Sınav Kaydedilmedi.";
            }

            model.durum += "Basarili";
            model.mesaj += "Sınav Başarıyla Kaydedildi.";

            return model;
        }

        public static string EncryptString(string key, string plainText)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        public static string DecryptString(string key, string cipherText)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
