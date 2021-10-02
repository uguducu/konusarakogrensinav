using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SinavOlusturma.Models
{
    public class Sorular
    {
        public int SoruId { get; set; }
        public int SinavId { get; set; }
        public string Soru { get; set; }
        public int DogruCevapId { get; set; }
        public string OlusturmaZamani { get; set; }
        public string Olusturan { get; set; }
    }
}
