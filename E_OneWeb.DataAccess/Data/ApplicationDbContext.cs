﻿using E_OneWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace E_OneWeb.DataAccess.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
           : base(options)
        {
        }
        public DbSet<Article> Article { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Items> Items { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<PurchaseOrderHeader> PurchaseOrderHeader { get; set; }
        public DbSet<PurchaseOrderDetail> PurchaseOrderDetail { get; set; }
        public DbSet<ItemTransfer> ItemTransfer { get; set; }
        public DbSet<GENMASTER> GENMASTER { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<ItemService> ItemService { get; set; }
        public DbSet<RequestItemHeader> RequestItemHeader { get; set; }
        public DbSet<RequestItemDetail> RequestItemDetail { get; set; }
        public DbSet<RoomReservationAdmin> RoomReservationAdmin { get; set; }
        public DbSet<RoomReservationUser> RoomReservationUser { get; set; }
        public DbSet<Personal> Personal { get; set; }
        public DbSet<ImportItems> ImportItems { get; set; }
        public DbSet<ImportBookingClass> ImportBookingClass { get; set; }
        public DbSet<FixedSchedulerRoom> FixedSchedulerRoom { get; set; }
        public DbSet<ImportFixedSchedulerRoom> ImportFixedSchedulerRoom { get; set; }
        public DbSet<VehicleReservationAdmin> VehicleReservationAdmin { get; set; }
        public DbSet<VehicleReservationUser> VehicleReservationUser { get; set; }		
		public DbSet<Drivers> Drivers { get; set; }
		protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }           
}
