﻿using E_OneWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_OneWeb.DataAccess.Repository.IRepository
{
    public interface IFixedSchedulerRoomRepositoryAsync : IRepositoryAsync<FixedSchedulerRoom>
    {
        void Update(FixedSchedulerRoom entity);
    }
}
