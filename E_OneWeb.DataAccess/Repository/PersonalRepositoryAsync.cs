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
    public class PersonalRepositoryAsync : Repository<Personal>, IPersonalRepositoryAsync
    {
        private readonly ApplicationDbContext _db;

        public PersonalRepositoryAsync(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(Personal personal)
        {
            var objFromDb = _db.Personal.FirstOrDefault(s => s.Id == personal.Id);
            if (objFromDb != null)
            {
                objFromDb.UserName = personal.UserName;                
                objFromDb.PhoneNumber = personal.PhoneNumber;
                objFromDb.NIM = personal.NIM;
                objFromDb.Prodi = personal.Prodi;
                objFromDb.Fakultas = personal.Fakultas;
                objFromDb.Photo = personal.Photo;   

                _db.SaveChanges();
            }
        }
    }
}
