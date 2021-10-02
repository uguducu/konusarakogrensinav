using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SinavOlusturma.Models;
namespace SinavOlusturma.Controllers
{
    public class SinavCevaplaController : Controller
    {
        [HttpPost]
        public IActionResult Index(IFormCollection collection)
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

            int GelenSınavId = Convert.ToInt32(collection["sınavıdkontrol"]);
            using (var db = new SqlliteDbContext())
            {
                List<Sinavlar> SinavBilgilerim = db.Sinavlar.Where(s => s.SinavId == GelenSınavId).ToList();

                List<Sorular> SoruBilgilerim = (from s in db.Sorular
                                                where s.SinavId == GelenSınavId
                                                orderby s.SoruId ascending
                                                select s).ToList();

                List<SoruSecenekleri> SoruSecenekBilgilerim = (from sc in db.SoruSecenekleri
                                                               join s in db.Sorular
                                                               on sc.SoruId equals s.SoruId
                                                               orderby sc.SoruSecenekleriId
                                                               select sc).ToList();

                ViewBag.SinavBilgilerim = SinavBilgilerim;
                ViewBag.SoruBilgilerim = SoruBilgilerim;
                ViewBag.SoruSecenekBilgilerim = SoruSecenekBilgilerim;
            }

            return View();
        }

        [HttpPost]
        public KaydetModel SinavCevapKaydet(IFormCollection collection)
        {
            KaydetModel model = new KaydetModel();
            int SinavId;

            try
            {
                long OturumId = 1;

                foreach (byte b in Guid.NewGuid().ToByteArray())
                {
                    OturumId *= ((int)b + 1);
                }

                string oturumNo = String.Format("{0:d9}", (DateTime.Now.Ticks / 10) % 1000000000);
                int _SınavId = Convert.ToInt32(collection["SınavIdKontrol"].ToString());

                for (int idx = 0; idx < 5; idx++)
                {
                    Cevaplar cevaplar = new Cevaplar();
                    int SoruId = Convert.ToInt32(collection["soru-" + idx + "-id"]);
                    SinavId = _SınavId;

                    cevaplar.SoruId = SoruId;
                    cevaplar.SinavId = SinavId;

                    cevaplar.Yanit = collection["soru_" + idx + "_cevap_0"].ToString();
                    cevaplar.YanitTarihi = DateTime.Now.ToString();
                    cevaplar.OturumNo = Convert.ToInt32(oturumNo);

                    using (var db = new SqlliteDbContext())
                    {
                        db.Cevaplar.Add(cevaplar);
                        db.SaveChanges();
                    }
                }

                #region Yanıtlayan kişi sayısı +1
                using (var db = new SqlliteDbContext())
                {
                    int UpdateSinavId = Convert.ToInt32(collection["SınavIdKontrol"].ToString());
                    Sinavlar UpdateSorular = db.Sinavlar.Where(s => s.SinavId == UpdateSinavId).FirstOrDefault();
                    int sonYanıt = UpdateSorular.YanıtlayanKisiSayisi;
                    if (UpdateSorular == null)
                    {
                        throw new Exception("");
                    }

                    UpdateSorular.YanıtlayanKisiSayisi = sonYanıt + 1;
                    db.SaveChanges();
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
