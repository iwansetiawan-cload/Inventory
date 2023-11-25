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
    public class LocationRepository : Repository<Location>, ILocationRepository
    {
        private readonly ApplicationDbContext _db;

        public LocationRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(Location location)
        {
            var objFromDb = _db.Locations.FirstOrDefault(s => s.Id == location.Id);
            if (objFromDb != null)
            {
                objFromDb.Name = location.Name;
                objFromDb.Description = location.Description;
                _db.SaveChanges();
            }
        }
    }
}
