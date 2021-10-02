using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SinavOlusturma.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using System.Text;
using Aes = System.Security.Cryptography.Aes;

namespace SinavOlusturma.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            try
            {
                if (HttpContext.Session.GetString("OturumAnahtari") != null)
                {
                    string tarih = DecryptString(Environment.GetEnvironmentVariable("GUVEN_ANAHTAR"), HttpContext.Session.GetString("OturumAnahtari"));

                    if (Convert.ToDateTime(tarih) < DateTime.Now.AddHours(12))
                    {
                        return Redirect("/Home");
                    }
                }
            }
            catch
            {

            }

            return View();
        }

        [HttpPost]
        public KaydetModel Index(IFormCollection collection)
        {
            KaydetModel cevap = new KaydetModel();

            if (collection.Keys.Count() == 3)
            {
                string kullanici = collection["email"].ToString();
                string sifre = collection["password"].ToString();

                try
                {
                    IEnumerable<Kullanicilar> sonuc = null;
                    using (var db = new SqlliteDbContext())
                    {
                        sonuc = db.Kullanicilar.Where(x => x.Eposta == kullanici).ToList();
                    }

                    IEnumerable<Kullanicilar> aktif = null;
                    using (var db = new SqlliteDbContext())
                    {
                        aktif = db.Kullanicilar.Where(x => x.Eposta == kullanici && x.Aktifmi == 0).ToList();
                    }

                    if (sonuc.Count() == 0)
                    {
                        cevap.durum = "Hatali";
                        cevap.mesaj = "Hesabınız bulunamadı. ";

                        return cevap;
                    }

                    else
                    {

                        if (aktif.Count() != 0)
                        {
                            cevap.durum = "Hatali";
                            cevap.mesaj = "Hesabınız aktif değil.";
                        }

                        else
                        {
                            if (GirisBilgileriniDogrula(kullanici, sifre))
                            {
                                HttpContext.Session.SetString("OturumAnahtari", EncryptString(Environment.GetEnvironmentVariable("GUVEN_ANAHTAR"), DateTime.Now.ToString()));
                                HttpContext.Session.SetString("kullanıcıAdı", kullanici);
                                cevap.durum = "Basarili";
                            }
                            else
                            {
                                cevap.durum = "Hatali";
                                cevap.mesaj = "Parola yanlış. Lütfen tekrar deneyin.";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    var hata = ex.Message.ToString();
                    cevap.durum = "Hatali";
                    cevap.mesaj = "Bir şeyler ters gitti. Lütfen tekrar deneyiniz.";

                    return cevap;
                }
            }

            else if (collection.Keys.Count() == 5)
            {

                try
                {
                    Kullanicilar kullanici = new Kullanicilar();
                    kullanici.KullaniciAdSoyad = collection["fullname"].ToString();
                    kullanici.Eposta = collection["email"].ToString();
                    kullanici.Aktifmi = 1;
                    kullanici.Sifre = collection["password"].ToString();
                    if (kullanici.Sifre != collection["rpassword"].ToString())
                    {
                        cevap.durum = "Hatali";
                        cevap.mesaj = "Şifreler birbirinden farklı olamaz!";
                    }
                    else
                    {
                        IEnumerable<Kullanicilar> sonuc = null;
                        using (var db = new SqlliteDbContext())
                        {
                            sonuc = db.Kullanicilar.Where(x => x.Eposta == kullanici.Eposta).ToList();
                        }

                        if (sonuc.Count() > 0)
                        {
                            cevap.durum = "Hatali";
                            cevap.mesaj = "Bu e-posta adresi ile daha önce kayıt yapılmış.";
                        }

                        else
                        {
                            try
                            {
                                using (var db = new SqlliteDbContext())
                                {
                                    db.Kullanicilar.Add(kullanici);
                                    db.SaveChanges();
                                }

                                cevap.durum += "Basarili";
                            }
                            catch (Exception)
                            {
                                cevap.durum = "Hatali";
                                cevap.mesaj = "Tekrar deneyiniz sorun devam ederse bize ulaşınız lütfen.!";
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }


            return cevap;
        }

        private bool GirisBilgileriniDogrula(string kullanici, string sifre)
        {
            bool sonuc = false;

            using (var db = new SqlliteDbContext())
            {
                List<Kullanicilar> kontrol = new List<Kullanicilar>();
                kontrol = db.Kullanicilar.Where(x => x.Sifre == sifre).ToList();

                if (kontrol.Count() > 0)
                {
                    sonuc = true;
                }
            }

            return sonuc;
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
