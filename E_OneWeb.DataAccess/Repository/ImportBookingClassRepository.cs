﻿using E_OneWeb.DataAccess.Data;
using E_OneWeb.DataAccess.Repository.IRepository;
using E_OneWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_OneWeb.DataAccess.Repository
{
    public class ImportBookingClassRepository : Repository<ImportBookingClass>, IImportBookingClassRepository
    {
        private readonly ApplicationDbContext _db;

        public ImportBookingClassRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
    }
}
