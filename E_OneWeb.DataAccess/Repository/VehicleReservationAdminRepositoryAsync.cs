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
    public class VehicleReservationAdminRepositoryAsync : RepositoryAsync<VehicleReservationAdmin>, IVehicleReservationAdminRepositoryAsync
    {
        private readonly ApplicationDbContext _db;

        public VehicleReservationAdminRepositoryAsync(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
		public void Update(VehicleReservationAdmin room)
		{
			var objFromDb = _db.VehicleReservationAdmin.FirstOrDefault(s => s.Id == room.Id);
			if (objFromDb != null)
			{
				objFromDb.StatusId = room.StatusId;
				objFromDb.Status = room.Status;
				objFromDb.Flag = room.Flag;
				objFromDb.BookingBy = room.BookingBy;
				objFromDb.BookingStartDate = room.BookingStartDate;
                objFromDb.BookingEndDate = room.BookingEndDate;
                objFromDb.BookingId = room.BookingId;
				_db.SaveChanges();
			}
		}
	}
}
