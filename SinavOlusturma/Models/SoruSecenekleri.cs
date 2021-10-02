using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SinavOlusturma.Models
{
    public class SoruSecenekleri
    {
        public int SoruSecenekleriId { get; set; }
        public int SoruId { get; set; }
        public string Secenek { get; set; }
        public string OlusturmaZamani { get; set; }
        public string Olusturan { get; set; }
    }
}
