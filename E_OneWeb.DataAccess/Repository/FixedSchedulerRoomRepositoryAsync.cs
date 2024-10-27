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
    public class FixedSchedulerRoomRepositoryAsync : RepositoryAsync<FixedSchedulerRoom>, IFixedSchedulerRoomRepositoryAsync
    {
        private readonly ApplicationDbContext _db;

        public FixedSchedulerRoomRepositoryAsync(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(FixedSchedulerRoom entity)
        {
            var objFromDb = _db.FixedSchedulerRoom.FirstOrDefault(s => s.Id == entity.Id);
            if (objFromDb != null)
            {
                objFromDb.RoomId = entity.RoomId;
                objFromDb.RoomName = entity.RoomName;
                objFromDb.LocationName = entity.LocationName;
                objFromDb.Room = entity.Room;
                objFromDb.Days = entity.Days;
                objFromDb.Start_Clock = entity.Start_Clock;
                objFromDb.End_Clock = entity.End_Clock;
                objFromDb.ValStart_Clock = entity.ValStart_Clock;
                objFromDb.ValEnd_Clock = entity.ValEnd_Clock;
                objFromDb.Prodi = entity.Prodi;
                objFromDb.Study = entity.Study;
                objFromDb.Semester = entity.Semester;
                objFromDb.Dosen = entity.Dosen;
                objFromDb.Flag = entity.Flag;
                _db.SaveChanges();
            }
        }
    }
}
