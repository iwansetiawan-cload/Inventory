IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231123150622_AddDefaultIdentityMigration')
BEGIN
    CREATE TABLE [Article] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(max) NOT NULL,
        [Content] nvarchar(max) NOT NULL,
        [Point] int NULL,
        [PointDesc] float NULL,
        [PointExepiredDate] datetime2 NULL,
        [EntryBy] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [EntryDate] datetime2 NULL,
        [Flag] smallint NULL,
        CONSTRAINT [PK_Article] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231123150622_AddDefaultIdentityMigration')
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231123150622_AddDefaultIdentityMigration')
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [Discriminator] nvarchar(21) NOT NULL,
        [Name] nvarchar(max) NULL,
        [Gender] nvarchar(max) NULL,
        [CardNumber] nvarchar(max) NULL,
        [City] nvarchar(max) NULL,
        [PostalCode] nvarchar(max) NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231123150622_AddDefaultIdentityMigration')
BEGIN
    CREATE TABLE [Brands] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(50) NOT NULL,
        [Description] nvarchar(max) NULL,
        CONSTRAINT [PK_Brands] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231123150622_AddDefaultIdentityMigration')
BEGIN
    CREATE TABLE [Categories] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(50) NOT NULL,
        [Description] nvarchar(max) NULL,
        [Percent] int NULL,
        [Period] int NULL,
        CONSTRAINT [PK_Categories] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231123150622_AddDefaultIdentityMigration')
BEGIN
    CREATE TABLE [GENMASTER] (
        [IDGEN] int NOT NULL IDENTITY,
        [GENCODE] nvarchar(255) NULL,
        [GENNAME] nvarchar(255) NULL,
        [GENVALUE] float NULL,
        [GENFLAG] int NOT NULL,
        [ENTRYBY] nvarchar(max) NULL,
        [ENTRYDATE] datetime2 NULL,
        CONSTRAINT [PK_GENMASTER] PRIMARY KEY ([IDGEN])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231123150622_AddDefaultIdentityMigration')
BEGIN
    CREATE TABLE [Locations] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(225) NOT NULL,
        [Description] nvarchar(max) NULL,
        CONSTRAINT [PK_Locations] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231123150622_AddDefaultIdentityMigration')
BEGIN
    CREATE TABLE [Suppliers] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(max) NULL,
        [Name] nvarchar(225) NOT NULL,
        [Address] nvarchar(max) NULL,
        [Phone] nvarchar(25) NULL,
        CONSTRAINT [PK_Suppliers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231123150622_AddDefaultIdentityMigration')
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231123150622_AddDefaultIdentityMigration')
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231123150622_AddDefaultIdentityMigration')
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231123150622_AddDefaultIdentityMigration')
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231123150622_AddDefaultIdentityMigration')
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231123150622_AddDefaultIdentityMigration')
BEGIN
    CREATE TABLE [Rooms] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(225) NOT NULL,
        [Description] nvarchar(max) NULL,
        [IDLocation] int NOT NULL,
        CONSTRAINT [PK_Rooms] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Rooms_Locations_IDLocation] FOREIGN KEY ([IDLocation]) REFERENCES [Locations] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231123150622_AddDefaultIdentityMigration')
BEGIN
    CREATE TABLE [PurchaseOrderHeader] (
        [Id] bigint NOT NULL IDENTITY,
        [SupplierId] int NOT NULL,
        [PurchaseOrderNo] nvarchar(20) NULL,
        [TransactionDate] datetime2 NOT NULL,
        [Requester] nvarchar(255) NOT NULL,
        [Notes] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        [TaxId] nvarchar(50) NULL,
        [TaxAmount] float NULL,
        [ShipmentFee] float NULL,
        [HandlingFee] float NULL,
        [DownPayment] float NULL,
        [DownPaymentPaid] float NULL,
        [PercentageDiscount] int NULL,
        [FinalDiscount] float NULL,
        [SubTotal] float NULL,
        [Total] float NULL,
        [DPP] float NULL,
        [GrandTotal] float NULL,
        [ApprovedBy] nvarchar(max) NULL,
        [DeletedDate] datetime2 NULL,
        [DeletedBy] nvarchar(50) NULL,
        [StatusTerima] nvarchar(50) NULL,
        [EntryBy] nvarchar(255) NULL,
        [EntryDate] datetime2 NULL,
        CONSTRAINT [PK_PurchaseOrderHeader] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PurchaseOrderHeader_Suppliers_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Suppliers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231123150622_AddDefaultIdentityMigration')
BEGIN
    CREATE TABLE [Items] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(50) NULL,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(max) NULL,
        [StartDate] datetime2 NULL,
        [Price] float NULL,
        [Qty] int NULL,
        [TotalAmount] float NULL,
        [Percent] int NULL,
        [Period] int NULL,
        [DepreciationExpense] float NULL,
        [Status] int NULL,
        [OriginOfGoods] nvarchar(255) NULL,
        [RoomId] int NOT NULL,
        [CategoryId] int NOT NULL,
        [EntryBy] nvarchar(255) NULL,
        [EntryDate] datetime2 NULL,
        [Condition] int NULL,
        CONSTRAINT [PK_Items] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Items_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Items_Rooms_RoomId] FOREIGN KEY ([RoomId]) REFERENCES [Rooms] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231123150622_AddDefaultIdentityMigration')
BEGIN
    CREATE TABLE [ItemTransfer] (
        [Id] int NOT NULL IDENTITY,
        [ItemId] int NOT NULL,
        [Description] nvarchar(max) NULL,
        [TransferDate] datetime2 NULL,
        [PreviousLocationId] int NOT NULL,
        [PreviousLocation] nvarchar(max) NOT NULL,
        [CurrentLocationId] int NOT NULL,
        [CurrentLocation] nvarchar(max) NOT NULL,
        [EntryBy] nvarchar(255) NULL,
        [EntryDate] datetime2 NULL,
        CONSTRAINT [PK_ItemTransfer] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ItemTransfer_Items_ItemId] FOREIGN KEY ([ItemId]) REFERENCES [Items] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231123150622_AddDefaultIdentityMigration')
BEGIN
    CREATE TABLE [PurchaseOrderDetail] (
        [Id] bigint NOT NULL IDENTITY,
        [PurchaseOrderNo] nvarchar(20) NULL,
        [ItemsId] int NOT NULL,
        [HeaderId] bigint NOT NULL,
        [ItemName] nvarchar(100) NOT NULL,
        [Quantity] float NOT NULL,
        [ItemCost] float NULL,
        [ItemDiscount] float NULL,
        [DiscountAmount] float NULL,
        [NettPrice] float NULL,
        [Total] float NOT NULL,
        [TaxAmount] float NULL,
        [DPP] float NULL,
        [BuyPrice] float NULL,
        CONSTRAINT [PK_PurchaseOrderDetail] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PurchaseOrderDetail_Items_ItemsId] FOREIGN KEY ([ItemsId]) REFERENCES [Items] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_PurchaseOrderDetail_PurchaseOrderHeader_HeaderId] FOREIGN KEY ([HeaderId]) REFERENCES [PurchaseOrderHeader] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231123150622_AddDefaultIdentityMigration')
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231123150622_AddDefaultIdentityMigration')
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231123150622_AddDefaultIdentityMigration')
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231123150622_AddDefaultIdentityMigration')
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231123150622_AddDefaultIdentityMigration')
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231123150622_AddDefaultIdentityMigration')
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231123150622_AddDefaultIdentityMigration')
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231123150622_AddDefaultIdentityMigration')
BEGIN
    CREATE INDEX [IX_Items_CategoryId] ON [Items] ([CategoryId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231123150622_AddDefaultIdentityMigration')
BEGIN
    CREATE INDEX [IX_Items_RoomId] ON [Items] ([RoomId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231123150622_AddDefaultIdentityMigration')
BEGIN
    CREATE INDEX [IX_ItemTransfer_ItemId] ON [ItemTransfer] ([ItemId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231123150622_AddDefaultIdentityMigration')
BEGIN
    CREATE INDEX [IX_PurchaseOrderDetail_HeaderId] ON [PurchaseOrderDetail] ([HeaderId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231123150622_AddDefaultIdentityMigration')
BEGIN
    CREATE INDEX [IX_PurchaseOrderDetail_ItemsId] ON [PurchaseOrderDetail] ([ItemsId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231123150622_AddDefaultIdentityMigration')
BEGIN
    CREATE INDEX [IX_PurchaseOrderHeader_SupplierId] ON [PurchaseOrderHeader] ([SupplierId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231123150622_AddDefaultIdentityMigration')
BEGIN
    CREATE INDEX [IX_Rooms_IDLocation] ON [Rooms] ([IDLocation]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231123150622_AddDefaultIdentityMigration')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20231123150622_AddDefaultIdentityMigration', N'8.0.0-preview.3.23174.2');
END;
GO

COMMIT;
GO

