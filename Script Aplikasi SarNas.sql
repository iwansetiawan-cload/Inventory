select [Percent],Period, * from Categories

select * from Locations
select * from Rooms
select * from Items

IF NOT EXISTS (SELECT * FROM GENMASTER where GENFLAG = 1)
BEGIN
PRINT 'INSERT GENMASTER 1'
	Insert Into GENMASTER VALUES (0,'Baik',0,1,'SYSTEM',GETDATE())
	Insert Into GENMASTER VALUES (1,'Rusak Dapat Diperbaiki',1,1,'SYSTEM',GETDATE())
	Insert Into GENMASTER VALUES (2,'Rusak Berat',2,1,'SYSTEM',GETDATE())
END

IF NOT EXISTS (SELECT * FROM GENMASTER where GENFLAG = 2)
BEGIN
PRINT 'INSERT GENMASTER 2'
	Insert Into GENMASTER VALUES (0,'Aktif',0,2,'SYSTEM',GETDATE())
	Insert Into GENMASTER VALUES (1,'Dimusnahkan',1,2,'SYSTEM',GETDATE())
	Insert Into GENMASTER VALUES (2,'Hilang',2,2,'SYSTEM',GETDATE())
END

IF NOT EXISTS (SELECT * FROM GENMASTER where GENFLAG = 3)
BEGIN
PRINT 'INSERT GENMASTER 3'
	Insert Into GENMASTER VALUES (0,'Pemerintah',0,3,'SYSTEM',GETDATE())
	Insert Into GENMASTER VALUES (1,'Yayasan',1,3,'SYSTEM',GETDATE())
	Insert Into GENMASTER VALUES (2,'Hibah Alumni',2,3,'SYSTEM',GETDATE())
END

IF NOT EXISTS (SELECT * FROM GENMASTER where GENFLAG = 4)
BEGIN
PRINT 'INSERT GENMASTER 4'
	Insert Into GENMASTER VALUES (0,'Room Available',0,4,'SYSTEM',GETDATE())
	Insert Into GENMASTER VALUES (1,'Waiting Approval',1,4,'SYSTEM',GETDATE())
	Insert Into GENMASTER VALUES (2,'Approved',2,4,'SYSTEM',GETDATE())
	Insert Into GENMASTER VALUES (3,'Rejected',3,4,'SYSTEM',GETDATE())
END

IF NOT EXISTS (SELECT * FROM GENMASTER where GENFLAG = 5)
BEGIN
PRINT 'INSERT GENMASTER 5'
	Insert Into GENMASTER VALUES (0,'Tersedia',0,5,'SYSTEM',GETDATE())
	Insert Into GENMASTER VALUES (1,'Menunggu Persetujuan',1,5,'SYSTEM',GETDATE())
	Insert Into GENMASTER VALUES (2,'Siap Digunakan',2,5,'SYSTEM',GETDATE())
END

IF NOT EXISTS (SELECT * FROM GENMASTER where GENFLAG = 6)
BEGIN
PRINT 'INSERT GENMASTER 6'
	Insert Into GENMASTER VALUES (0,'Waiting approved',0,6,'SYSTEM',GETDATE())
	Insert Into GENMASTER VALUES (1,'Approved',1,6,'SYSTEM',GETDATE())
	Insert Into GENMASTER VALUES (2,'Rejected',2,6,'SYSTEM',GETDATE())
END

--create user [IIS APPPOOL\SarPrasApp] for login [IIS APPPOOL\SarPrasApp]
--go
--execute sp_addrolemember N'db_owner', N'IIS APPPOOL\SarPrasApp'
--go


ALTER PROCEDURE [dbo].[Sp_validationImportFixedSchedulerRoom] 
	-- Add the parameters for the stored procedure here
	@UserName varchar(100)
AS
BEGIN


	UPDATE ImportFixedSchedulerRoom SET
			ImportRemark = CASE WHEN ImportRemark IS NOT NULL THEN ImportRemark + ', nama gedung harus diisi' ELSE 'nama gedung harus diisi' END,
			ImportStatus = 'Invalid'
			WHERE EntryBy = @UserName and (LocationName = null or LocationName = '')

	UPDATE ImportFixedSchedulerRoom SET
			ImportRemark = CASE WHEN ImportRemark IS NOT NULL THEN ImportRemark + ', nama ruangan harus diisi' ELSE 'nama ruangan harus diisi' END,
			ImportStatus = 'Invalid'
			WHERE EntryBy = @UserName and (RoomName = null or RoomName = '')

	UPDATE IM SET
			ImportRemark = CASE WHEN ImportRemark IS NOT NULL THEN ImportRemark + ', ruangan dan nama gedung tidak ditemukan' ELSE 'ruangan dan nama gedung tidak ditemukan' END,
			ImportStatus = 'Invalid'
			FROM ImportFixedSchedulerRoom IM
			left join Rooms rom on ltrim(rtrim(IM.RoomName)) = ltrim(rtrim(rom.Name))
			left join Locations loc on ltrim(rtrim(IM.LocationName)) = ltrim(rtrim(loc.Name))
			WHERE IM.EntryBy = @UserName and rom.Id is null

	UPDATE ImportFixedSchedulerRoom SET
			ImportRemark = CASE WHEN ImportRemark IS NOT NULL THEN ImportRemark + ', hari harus diisi' ELSE 'hari harus diisi' END,
			ImportStatus = 'Invalid'
			WHERE EntryBy = @UserName and ([Days] is null or [Days] = '')

	UPDATE ImportFixedSchedulerRoom SET
			ImportRemark = CASE WHEN ImportRemark IS NOT NULL THEN ImportRemark + ', nama hari tidak sesuai' ELSE 'nama hari tidak sesuai' END,
			ImportStatus = 'Invalid'
			WHERE EntryBy = @UserName and ([Days] is not null or [Days] != '') and [Days] not in ('senin','selasa','rabu','kamis','jumat','sabtu','minggu')

	UPDATE ImportFixedSchedulerRoom SET
			ImportRemark = CASE WHEN ImportRemark IS NOT NULL THEN ImportRemark + ', mulai jam harus diisi' ELSE 'mulai jam harus diisi' END,
			ImportStatus = 'Invalid'
			WHERE EntryBy = @UserName and ([Start_Clock] = null or [Start_Clock] = '')

	UPDATE ImportFixedSchedulerRoom SET
			ImportRemark = CASE WHEN ImportRemark IS NOT NULL THEN ImportRemark + ', sampai jam harus diisi' ELSE 'sampai jam harus diisi' END,
			ImportStatus = 'Invalid'
			WHERE EntryBy = @UserName and ([End_Clock] = null or [End_Clock] = '')

	declare @YEAR varchar(50), 
			@MONTH varchar(50),
			@DATEEXEC DATETIME
		SET @YEAR = YEAR(GETDATE()) ;
		SET @MONTH = MONTH(GETDATE());
		SET @DATEEXEC = @YEAR+'-'+@MONTH+'-01';	

	UPDATE UP SET
		RoomId = 
		(SELECT TOP 1 Id FROM Rooms WHERE Name=LTRIM(RTRIM(UP.RoomName))
		AND IDLocation = (SELECT TOP 1 Id FROM Locations WHERE Name=LTRIM(RTRIM(UP.LocationName)))),
		ValDays = [Days],
		Start_Clock = FORMAT(CONVERT(DATETIME, Start_Clock,103),'HH:mm'),
		End_Clock = FORMAT(CONVERT(DATETIME, End_Clock,103),'HH:mm'),
		ValStart_Clock = @DATEEXEC + FORMAT(CONVERT(DATETIME, Start_Clock,103),'HH:mm'),
		ValEnd_Clock = @DATEEXEC + FORMAT(CONVERT(DATETIME, End_Clock,103),'HH:mm'),
		Flag = case when ltrim(rtrim([Days])) = 'senin' then 1 when  ltrim(RTRIM([Days])) = 'selasa' then 2 when  ltrim(RTRIM([Days])) = 'rabu' then 3
			   when  ltrim(RTRIM([Days])) = 'kamis' then 4 when ltrim(RTRIM([Days])) = 'jumat' then 5 when  ltrim(RTRIM([Days])) = 'sabtu' then 6 when  ltrim(RTRIM([Days])) = 'minggu' then 7  end
		FROM ImportFixedSchedulerRoom UP
		WHERE UP.EntryBy = @UserName 
		and UP.ImportStatus = 'Valid'
END

ALTER PROCEDURE [dbo].[Sp_processImportFixedSchedulerRoom] 
	-- Add the parameters for the stored procedure here
	@UserName varchar(100)
AS
BEGIN

	insert FixedSchedulerRoom (RoomId,RoomName,LocationName,[Days],Start_Clock,ValStart_Clock,End_Clock,ValEnd_Clock,Study,Dosen,Flag,EntryBy,EntryDate)
	select RoomId,RoomName,LocationName,ValDays,Start_Clock,ValStart_Clock,End_Clock,ValEnd_Clock,Study,Dosen,
	Flag,EntryBy,EntryDate
	from ImportFixedSchedulerRoom
	WHERE EntryBy = @UserName AND ImportStatus='Valid'
	
END