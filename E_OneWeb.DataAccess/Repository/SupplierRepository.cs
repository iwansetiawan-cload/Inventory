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
    public class SupplierRepository : RepositoryAsync<Supplier>, ISupplierRepository
    {
        private readonly ApplicationDbContext _db;
        public SupplierRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(Supplier supplier)
        {
            var objFromDb = _db.Suppliers.FirstOrDefault(s => s.Id == supplier.Id);
            if (objFromDb != null)
            {
                objFromDb.Name = supplier.Name;
                objFromDb.Phone = supplier.Phone;
                objFromDb.Address = supplier.Address;
                _db.SaveChanges();
            }
        }
    }
}
