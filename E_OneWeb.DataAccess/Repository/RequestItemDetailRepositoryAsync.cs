using E_OneWeb.DataAccess.Data;
using E_OneWeb.DataAccess.Repository.IRepository;
using E_OneWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_OneWeb.DataAccess.Repository
{
    public class RequestItemDetailRepositoryAsync : RepositoryAsync<RequestItemDetail>, IRequestItemDetailRepositoryAsync
    {
        private readonly ApplicationDbContext _db;

        public RequestItemDetailRepositoryAsync(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(RequestItemDetail requestdetail)
        {
            var objFromDb = _db.RequestItemDetail.FirstOrDefault(s => s.Id == requestdetail.Id);
            if (objFromDb != null)
            {
                objFromDb.Name = requestdetail.Name;
                objFromDb.Status = requestdetail.Status;
                objFromDb.Notes = requestdetail.Notes;
                objFromDb.StatusId = requestdetail.StatusId;
                _db.SaveChanges();
            }
        }
    }
}
