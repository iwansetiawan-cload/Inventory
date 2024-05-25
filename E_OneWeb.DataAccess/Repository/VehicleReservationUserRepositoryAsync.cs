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
    public class VehicleReservationUserRepositoryAsync : RepositoryAsync<VehicleReservationUser>, IVehicleReservationUserRepositoryAsync
    {
        private readonly ApplicationDbContext _db;

        public VehicleReservationUserRepositoryAsync(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(VehicleReservationUser vm)
        {
            var objFromDb = _db.VehicleReservationUser.FirstOrDefault(s => s.Id == vm.Id);
            if (objFromDb != null)
            {
                objFromDb.StatusId = vm.StatusId;
                objFromDb.Status = vm.Status;
                objFromDb.Flag = vm.Flag;
                objFromDb.NotesReject = vm.NotesReject;
                objFromDb.RejectedBy = vm.RejectedBy;
                objFromDb.RejectedDate = vm.RejectedDate;
                objFromDb.ApproveBy = vm.ApproveBy;
                objFromDb.ApproveDate = vm.ApproveDate;
                _db.SaveChanges();
            }
        }
    }
}
