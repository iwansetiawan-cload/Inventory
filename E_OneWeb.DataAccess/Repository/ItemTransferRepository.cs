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
    public class ItemTransferRepository : RepositoryAsync<ItemTransfer>, IItemTransferRepository
    {
        private readonly ApplicationDbContext _db;

        public ItemTransferRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(ItemTransfer items)
        {
            var objFromDb = _db.ItemTransfer.FirstOrDefault(s => s.Id == items.Id);
            if (objFromDb != null)
            {
                objFromDb.ItemId = items.ItemId;
                objFromDb.Description = items.Description;              
                objFromDb.TransferDate = items.TransferDate;
                objFromDb.EntryBy = items.EntryBy;
                objFromDb.EntryDate = items.EntryDate;
                objFromDb.PreviousLocation = items.PreviousLocation;
                objFromDb.CurrentLocation = items.CurrentLocation;
                objFromDb.PreviousLocationId = items.PreviousLocationId;
                objFromDb.CurrentLocationId = items.CurrentLocationId;
                objFromDb.PreviousRoom = items.PreviousRoom;
                objFromDb.CurrentRoom = items.CurrentRoom;

                _db.SaveChanges();
            }
        }
    }
}
