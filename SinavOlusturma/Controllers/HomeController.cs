using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SinavOlusturma.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using System.Text;
using Aes = System.Security.Cryptography.Aes;
using System.IO;

namespace SinavOlusturma.Controllers
{
    public class HomeController : Controller
    {
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
                List<Sinavlar> ListeSinavlar = (from s in db.Sinavlar
                                                orderby s.SinavId descending
                                                select s).ToList();

                ViewBag.kullanıcıAdı = db.Kullanicilar.Where(x => x.Eposta == HttpContext.Session.GetString("kullanıcıAdı")).FirstOrDefault().KullaniciAdSoyad.ToString();

                string basharf = "";
                var kullanici = ViewBag.kullanıcıAdı.Split(" ");
                for (int idx = 0; idx < kullanici.Length; idx++)
                {
                    basharf += kullanici[idx].Substring(0, 1);
                }

                ViewBag.kullanıcıAdıBasHarf = basharf;
                ViewBag.ListeSinavlar = ListeSinavlar;
                ViewBag.SınavSayisi = ListeSinavlar.Count();
            }

            return View();
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
