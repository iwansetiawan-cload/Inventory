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
    public class PurchaseOrderHeaderRepository : RepositoryAsync<PurchaseOrderHeader>, IPurchaseOrderHeaderRepository
    {
        private readonly ApplicationDbContext _db;

        public PurchaseOrderHeaderRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(PurchaseOrderHeader purchaseOrderHeader)
        {
            var objFromDb = _db.PurchaseOrderHeader.FirstOrDefault(s => s.Id == purchaseOrderHeader.Id);
            if (objFromDb != null)
            {
                //objFromDb.Name = items.Name;
                //objFromDb.Code = items.Code;
                //objFromDb.Description = items.Description;
                //objFromDb.CategoryId = items.CategoryId;
                //objFromDb.BrandsId = items.BrandsId;
                //objFromDb.BuyPrice = items.BuyPrice;
                //objFromDb.SellPrice = items.SellPrice;
                _db.SaveChanges();
            }
        }
    }
}
