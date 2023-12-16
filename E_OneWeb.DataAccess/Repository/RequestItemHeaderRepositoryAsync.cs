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
    public class RequestItemHeaderRepositoryAsync : RepositoryAsync<RequestItemHeader>, IRequestItemHeaderRepositoryAsync
    {
        private readonly ApplicationDbContext _db;

        public RequestItemHeaderRepositoryAsync(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(RequestItemHeader requestheader)
        {
            var objFromDb = _db.RequestItemHeader.FirstOrDefault(s => s.Id == requestheader.Id);
            if (objFromDb != null)
            {
                objFromDb.ReqNumber = requestheader.ReqNumber;                
                objFromDb.Notes = requestheader.Notes;
                objFromDb.Status = requestheader.Status;
                objFromDb.StatusId = requestheader.StatusId;
                objFromDb.Requester = requestheader.Requester;
                objFromDb.RequestDate = requestheader.RequestDate;               
                objFromDb.TotalAmount = requestheader.TotalAmount;
                objFromDb.RefFile = requestheader.RefFile;

                _db.SaveChanges();
            }
        }
    }
}
