using E_OneWeb.DataAccess.Data;
using E_OneWeb.DataAccess.Repository.IRepository;
using E_OneWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace E_OneWeb.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            Article = new ArticleRepository(_db);
            ApplicationUser = new ApplicationUserRepository(_db);
            Category = new CategoryRepository(_db);
            Brand = new BrandRepository(_db);
            Room = new RoomRepository(_db);
            Items = new ItemsRepository(_db);
            Supplier = new SupplierRepository(_db);
            PurchaseOrderHeader = new PurchaseOrderHeaderRepository(_db);
            ItemTransfer = new ItemTransferRepository(_db);
            Genmaster = new GenmasterRepository(_db);
            Location = new LocationRepository(_db);
            ItemService = new ItemServiceRepositoryAsync(_db);
            RequestItemHeader = new RequestItemHeaderRepositoryAsync(_db);
            RequestItemDetail = new RequestItemDetailRepositoryAsync(_db);
            RoomReservationUser = new RoomReservationUserRepositoryAsync(_db);
            RoomReservationAdmin = new RoomReservationAdminRepositoryAsync(_db);
            Personal = new PersonalRepositoryAsync(_db);
        }
        public IArticleRepository Article { get; private set; }
        public IApplicationUserRepository ApplicationUser { get; private set; }
        public ICategoryRepository Category { get; private set; }
        public IBrandRepository Brand { get; private set; }
        public IRoomRepository Room { get; private set; }
        public IItemsRepository Items { get; private set; }
        public ISupplierRepository Supplier { get; private set; }
        public IPurchaseOrderHeaderRepository PurchaseOrderHeader { get; set; }
        public IItemTransferRepository ItemTransfer { get; private set; }
        public IGenmasterRepository Genmaster { get; private set; }
        public ILocationRepository Location { get; private set; }
        public IItemServiceRepositoryAsync ItemService { get; private set; }
        public IRequestItemHeaderRepositoryAsync RequestItemHeader { get; private set; }
        public IRequestItemDetailRepositoryAsync RequestItemDetail { get; private set; }
        public IRoomReservationRepositoryUserAsync RoomReservationUser { get; private set; }
        public IRoomReservationRepositoryAdminAsync RoomReservationAdmin { get; private set; }
        public IPersonalRepositoryAsync Personal { get; private set; }
        public void Dispose()
        {
            _db.Dispose();
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
