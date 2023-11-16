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
    public class ItemsRepository : RepositoryAsync<Items>, IItemsRepository
    {
        private readonly ApplicationDbContext _db;

        public ItemsRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Items items)
        {
            var objFromDb = _db.Items.FirstOrDefault(s => s.Id == items.Id);
            if (objFromDb != null)
            {
                objFromDb.Name = items.Name;
                objFromDb.Code = items.Code;
                objFromDb.Description = items.Description;
                objFromDb.CategoryId = items.CategoryId;
                objFromDb.RoomId = items.RoomId;
                objFromDb.StartDate = items.StartDate;
                objFromDb.Price = items.Price;
                objFromDb.Qty = items.Qty;
                objFromDb.TotalAmount = items.TotalAmount;
                objFromDb.Percent = items.Percent;
                objFromDb.Period = items.Period;
                objFromDb.DepreciationExpense = items.DepreciationExpense;
                objFromDb.OriginOfGoods = items.OriginOfGoods;
                objFromDb.Status = items.Status;
                objFromDb.EntryBy = items.EntryBy;
                objFromDb.EntryDate = items.EntryDate;

                _db.SaveChanges();
            }
        }
    }
}
