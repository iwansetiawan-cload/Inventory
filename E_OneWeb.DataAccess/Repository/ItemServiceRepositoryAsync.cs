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
    public class ItemServiceRepositoryAsync : RepositoryAsync<ItemService>, IItemServiceRepositoryAsync
    {
        private readonly ApplicationDbContext _db;

        public ItemServiceRepositoryAsync(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(ItemService itemservice)
        {
            var objFromDb = _db.ItemService.FirstOrDefault(s => s.Id == itemservice.Id);
            if (objFromDb != null)
            {
                objFromDb.Items = itemservice.Items;
                objFromDb.ServiceDate = itemservice.ServiceDate;
                objFromDb.ServiceEndDate = itemservice.ServiceEndDate;
                objFromDb.RepairDescription = itemservice.RepairDescription;
                objFromDb.Technician = itemservice.Technician;
                objFromDb.PhoneNumber = itemservice.PhoneNumber;
                objFromDb.Address = itemservice.Address;
                objFromDb.RequestBy = itemservice.RequestBy;
                objFromDb.CostOfRepair = itemservice.CostOfRepair;
                objFromDb.Status = itemservice.Status;
                _db.SaveChanges();
            }
        }

       
    }
}
