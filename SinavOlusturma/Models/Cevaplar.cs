using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SinavOlusturma.Models
{
    public class Cevaplar
    {
        public int CevapId { get; set; }
        public int SinavId { get; set; }
        public int SoruId { get; set; }
        public string Yanit { get; set; }
        public string YanitTarihi { get; set; }
        public int OturumNo { get; set; }
    }
}
