-- 1. جدول سجلات النسخ الاحتياطي
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BackupLogs')
BEGIN
    CREATE TABLE BackupLogs (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        FileName NVARCHAR(255),
        FilePath NVARCHAR(500),
        FileSizeMB FLOAT,
        BackupType NVARCHAR(50),
        CreatedBy NVARCHAR(100),
        CreatedDate DATETIME,
        Status NVARCHAR(50)
    );
END

-- 2. جدول إعدادات النسخ الاحتياطي التلقائي
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BackupSettings')
BEGIN
    CREATE TABLE BackupSettings (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        AutoBackupEnabled BIT,
        BackupIntervalHours INT,
        BackupTime TIME,
        BackupPath NVARCHAR(500)
    );
    
    -- إدراج القيم الافتراضية
    INSERT INTO BackupSettings (AutoBackupEnabled, BackupIntervalHours, BackupTime, BackupPath)
    VALUES (0, 24, '02:00:00', 'C:\SestamkBackups');
END
