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
    public class RoomRepository : Repository<Room>, IRoomRepository
    {
        private readonly ApplicationDbContext _db;
        public RoomRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(Room room)
        {
            var objFromDb = _db.Rooms.FirstOrDefault(s => s.Id == room.Id);
            if (objFromDb != null)
            {
                objFromDb.Name = room.Name;
                objFromDb.Description = room.Description;
                _db.SaveChanges();
            }
        }
    }
}
