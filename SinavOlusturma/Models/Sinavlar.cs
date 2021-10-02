using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SinavOlusturma.Models
{
    public class Sinavlar
    {
        public int SinavId { get; set; }
        public string Baslik { get; set; }
        public string Aciklama { get; set; }
        public int YanıtlayanKisiSayisi { get; set; }
        public string OlusturulmaZamani { get; set; }
        public string Olusturan { get; set; }
    }
}
