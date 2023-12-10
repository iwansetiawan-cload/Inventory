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
    public class RoomReservationAdminRepositoryAsync : RepositoryAsync<RoomReservationAdmin>, IRoomReservationRepositoryAdminAsync
    {
        private readonly ApplicationDbContext _db;

        public RoomReservationAdminRepositoryAsync(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
    }
}
