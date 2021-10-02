using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SinavOlusturma.Models
{
    public class Kullanicilar
    {
        public int KullaniciId { get; set; }
        public string Eposta { get; set; }
        public string Sifre { get; set; }
        public string KullaniciAdSoyad { get; set; }
        public int Aktifmi { get; set; }
    }
}
