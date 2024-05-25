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
    public class DriversRepository : Repository<Drivers>, IDriversRepository
    {
        private readonly ApplicationDbContext _db;
        public DriversRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(Drivers driver)
        {
            var objFromDb = _db.Drivers.FirstOrDefault(s => s.Id == driver.Id);
            if (objFromDb != null)
            {
                objFromDb.Name = driver.Name;
                objFromDb.Address = driver.Address;
                objFromDb.PhoneNumber = driver.PhoneNumber;
                _db.SaveChanges();
            }
        }
    }
}
