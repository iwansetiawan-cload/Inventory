﻿using E_OneWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_OneWeb.DataAccess.Repository.IRepository
{
    public interface IRoomRepository : IRepository<Room>
    {
        void Update(Room room);
    }
}
