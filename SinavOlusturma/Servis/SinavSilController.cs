using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SinavOlusturma.Models;

namespace SinavOlusturma.Servis
{
    [Route("api/[controller]")]
    [ApiController]
    public class SinavSilController : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<bool>> SinavSil()
        {
            var SilId = Request.Form["SinavSSilId"].ToString();
            bool result = false;

            using (var db = new SqlliteDbContext())
            {
                var kontrol = db.Sinavlar.Where(x => x.SinavId == Convert.ToInt32(SilId)).SingleOrDefault();
                if (kontrol != null)
                {
                    db.Remove(db.Sinavlar.Where(x => x.SinavId == Convert.ToInt32(SilId)).SingleOrDefault());
                    db.SaveChanges();
                    result = true;
                }
            }

            return result;
        }
    }
}
