IF DB_ID(N'DB_CryptoExchange_UP01') IS NULL
    CREATE DATABASE DB_Kolchin_Mikhailova_CryptoExchange;
GO
USE DB_Kolchin_Mikhailova_CryptoExchange;
GO

CREATE TABLE dbo.Users(
 UserID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
 Login NVARCHAR(50) NOT NULL CONSTRAINT UQ_Users_Login UNIQUE,
 PasswordHash NVARCHAR(300) NOT NULL,
 Role NVARCHAR(10) NOT NULL CONSTRAINT DF_Users_Role DEFAULT('User'),
 FullName NVARCHAR(150) NOT NULL,
 Email NVARCHAR(254) NOT NULL,
 IsActive BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT(1),
 CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Users_Created DEFAULT(SYSDATETIME()),
 UpdatedAt DATETIME2 NULL,
 LastLoginAt DATETIME2 NULL,
 CONSTRAINT CK_Users_Role CHECK(Role IN ('User','Admin'))
);
GO
CREATE TABLE dbo.Assets(
 AssetID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Assets PRIMARY KEY,
 Symbol NVARCHAR(12) NOT NULL CONSTRAINT UQ_Assets_Symbol UNIQUE,
 AssetName NVARCHAR(80) NOT NULL,
 AssetType NVARCHAR(20) NOT NULL,
 Precision TINYINT NOT NULL CONSTRAINT DF_Assets_Precision DEFAULT(8),
 IsActive BIT NOT NULL CONSTRAINT DF_Assets_Active DEFAULT(1),
 CONSTRAINT CK_Assets_Type CHECK(AssetType IN ('Cryptocurrency','Stablecoin','Token','Fiat')),
 CONSTRAINT CK_Assets_Precision CHECK(Precision BETWEEN 0 AND 18)
);
GO
CREATE TABLE dbo.ExchangeRates(
 RateID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ExchangeRates PRIMARY KEY,
 FromAssetID INT NOT NULL,
 ToAssetID INT NOT NULL,
 Rate DECIMAL(28,12) NOT NULL,
 UpdatedAt DATETIME2 NOT NULL CONSTRAINT DF_Rates_Updated DEFAULT(SYSDATETIME()),
 CONSTRAINT FK_Rates_From FOREIGN KEY(FromAssetID) REFERENCES dbo.Assets(AssetID) ON DELETE CASCADE,
 CONSTRAINT FK_Rates_To FOREIGN KEY(ToAssetID) REFERENCES dbo.Assets(AssetID) ON DELETE CASCADE,
 CONSTRAINT CK_Rates_Positive CHECK(Rate>0),
 CONSTRAINT UQ_Rates_Pair UNIQUE(FromAssetID,ToAssetID)
);
GO
CREATE TABLE dbo.Wallets(
 WalletID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Wallets PRIMARY KEY,
 UserID INT NOT NULL,
 AssetID INT NOT NULL,
 Address NVARCHAR(120) NOT NULL,
 Balance DECIMAL(28,12) NOT NULL CONSTRAINT DF_Wallets_Balance DEFAULT(0),
 CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Wallets_Created DEFAULT(SYSDATETIME()),
 CONSTRAINT FK_Wallets_User FOREIGN KEY(UserID) REFERENCES dbo.Users(UserID) ON DELETE CASCADE,
 CONSTRAINT FK_Wallets_Asset FOREIGN KEY(AssetID) REFERENCES dbo.Assets(AssetID),
 CONSTRAINT CK_Wallets_Balance CHECK(Balance>=0),
 CONSTRAINT UQ_Wallets_UserAsset UNIQUE(UserID,AssetID)
);
GO
CREATE TABLE dbo.ExchangeOrders(
 OrderID BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ExchangeOrders PRIMARY KEY,
 UserID INT NOT NULL,
 FromAssetID INT NOT NULL,
 ToAssetID INT NOT NULL,
 FromAmount DECIMAL(28,12) NOT NULL,
 ToAmount DECIMAL(28,12) NOT NULL,
 Rate DECIMAL(28,12) NOT NULL,
 Status NVARCHAR(20) NOT NULL CONSTRAINT DF_Orders_Status DEFAULT('Pending'),
 CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Orders_Created DEFAULT(SYSDATETIME()),
 CompletedAt DATETIME2 NULL,
 CONSTRAINT FK_Orders_User FOREIGN KEY(UserID) REFERENCES dbo.Users(UserID) ON DELETE CASCADE,
 CONSTRAINT FK_Orders_From FOREIGN KEY(FromAssetID) REFERENCES dbo.Assets(AssetID),
 CONSTRAINT FK_Orders_To FOREIGN KEY(ToAssetID) REFERENCES dbo.Assets(AssetID),
 CONSTRAINT CK_Orders_Amounts CHECK(FromAmount>0 AND ToAmount>0 AND Rate>0),
 CONSTRAINT CK_Orders_Status CHECK(Status IN ('Pending','Processing','Completed','Cancelled','Rejected')),
 CONSTRAINT CK_Orders_DifferentAssets CHECK(FromAssetID<>ToAssetID)
);
GO
CREATE TABLE dbo.Transactions(
 TransactionID BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Transactions PRIMARY KEY,
 OrderID BIGINT NOT NULL,
 UserID INT NOT NULL,
 TransactionType NVARCHAR(20) NOT NULL,
 Amount DECIMAL(28,12) NOT NULL,
 AssetID INT NOT NULL,
 TxHash NVARCHAR(128) NULL,
 TransactionDate DATETIME2 NOT NULL CONSTRAINT DF_Transactions_Date DEFAULT(SYSDATETIME()),
 Status NVARCHAR(20) NOT NULL CONSTRAINT DF_Transactions_Status DEFAULT('Created'),
 CONSTRAINT FK_Transactions_Order FOREIGN KEY(OrderID) REFERENCES dbo.ExchangeOrders(OrderID) ON DELETE CASCADE,
 CONSTRAINT FK_Transactions_User FOREIGN KEY(UserID) REFERENCES dbo.Users(UserID),
 CONSTRAINT FK_Transactions_Asset FOREIGN KEY(AssetID) REFERENCES dbo.Assets(AssetID),
 CONSTRAINT CK_Transactions_Amount CHECK(Amount>0),
 CONSTRAINT CK_Transactions_Type CHECK(TransactionType IN ('Debit','Credit','Exchange')),
 CONSTRAINT CK_Transactions_Status CHECK(Status IN ('Created','Confirmed','Failed'))
);
GO
CREATE TABLE dbo.WalletEntries(
 EntryID BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_WalletEntries PRIMARY KEY,
 WalletID INT NOT NULL,
 TransactionID BIGINT NULL,
 EntryType NVARCHAR(10) NOT NULL,
 Amount DECIMAL(28,12) NOT NULL,
 EntryDate DATETIME2 NOT NULL CONSTRAINT DF_Entries_Date DEFAULT(SYSDATETIME()),
 Description NVARCHAR(250) NULL,
 CONSTRAINT FK_Entries_Wallet FOREIGN KEY(WalletID) REFERENCES dbo.Wallets(WalletID) ON DELETE CASCADE,
 CONSTRAINT FK_Entries_Transaction FOREIGN KEY(TransactionID) REFERENCES dbo.Transactions(TransactionID) ON DELETE SET NULL,
 CONSTRAINT CK_Entries_Type CHECK(EntryType IN ('Credit','Debit')),
 CONSTRAINT CK_Entries_Amount CHECK(Amount>0)
);
GO
CREATE TABLE dbo.AuditLog(
 AuditID BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AuditLog PRIMARY KEY,
 UserID INT NULL,
 Action NVARCHAR(80) NOT NULL,
 EntityName NVARCHAR(80) NOT NULL,
 EntityID NVARCHAR(50) NULL,
 Details NVARCHAR(1000) NULL,
 CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Audit_Created DEFAULT(SYSDATETIME()),
 CONSTRAINT FK_Audit_User FOREIGN KEY(UserID) REFERENCES dbo.Users(UserID) ON DELETE SET NULL
);
GO

INSERT dbo.Assets(Symbol,AssetName,AssetType,Precision) VALUES
('BTC','Bitcoin','Cryptocurrency',8),('ETH','Ethereum','Cryptocurrency',8),
('USDT','Tether USD','Stablecoin',6),('USDC','USD Coin','Stablecoin',6),
('SOL','Solana','Cryptocurrency',8),('RUB','Российский рубль','Fiat',2);
GO
INSERT dbo.ExchangeRates(FromAssetID,ToAssetID,Rate)
SELECT a.AssetID,b.AssetID,v.Rate FROM (VALUES
 ('BTC','USDT',67500.000000000000),('ETH','USDT',3500.000000000000),
 ('SOL','USDT',145.000000000000),('USDT','USDC',1.000000000000),
 ('USDT','RUB',92.500000000000)) v(F,T,Rate)
JOIN dbo.Assets a ON a.Symbol=v.F JOIN dbo.Assets b ON b.Symbol=v.T;
GO
/* Демонстрационные пароли: admin / Admin123 и user / User123.
   Хеши PBKDF2-HMAC-SHA1, 120000 итераций, формат соответствует PasswordHasher.cs. */
INSERT dbo.Users(Login,PasswordHash,Role,FullName,Email) VALUES
('admin','PBKDF2$120000$YWRtaW5zZWVkMTIzNDU2Nw==$oKhbLT4gao8BPfp8BdSq+FI/Q022y2E4LpsZeHTREBk=','Admin','Администратор системы','admin@crypto.local'),
('user','PBKDF2$120000$dXNlcnNlZWQxMjM0NTY3OA==$NBeEQ2XcURPjTeSo2kK1P4UP6/Z2hW8VzredRevm+GA=','User','Тестовый Пользователь','user@crypto.local');
GO
INSERT dbo.Wallets(UserID,AssetID,Address,Balance)
SELECT u.UserID,a.AssetID,CONCAT('EDU-',u.Login,'-',a.Symbol),CASE a.Symbol WHEN 'BTC' THEN 0.125 WHEN 'ETH' THEN 2.5 WHEN 'USDT' THEN 1500 ELSE 0 END
FROM dbo.Users u CROSS JOIN dbo.Assets a WHERE u.Login='user' AND a.Symbol IN ('BTC','ETH','USDT');
GO
INSERT dbo.ExchangeOrders(UserID,FromAssetID,ToAssetID,FromAmount,ToAmount,Rate,Status)
SELECT u.UserID,a.AssetID,b.AssetID,0.01,675.0,67500,'Completed' FROM dbo.Users u CROSS JOIN dbo.Assets a CROSS JOIN dbo.Assets b
WHERE u.Login='user' AND a.Symbol='BTC' AND b.Symbol='USDT';
GO
INSERT dbo.Transactions(OrderID,UserID,TransactionType,Amount,AssetID,Status)
SELECT o.OrderID,o.UserID,'Exchange',o.FromAmount,o.FromAssetID,'Confirmed' FROM dbo.ExchangeOrders o;
GO
INSERT dbo.WalletEntries(WalletID,TransactionID,EntryType,Amount,Description)
SELECT w.WalletID,t.TransactionID,'Debit',0.01,'Демонстрационное списание по заявке'
FROM dbo.Wallets w CROSS JOIN dbo.Transactions t JOIN dbo.Users u ON u.UserID=w.UserID
WHERE u.Login='user' AND w.AssetID=(SELECT AssetID FROM dbo.Assets WHERE Symbol='BTC');
GO
INSERT dbo.AuditLog(UserID,Action,EntityName,EntityID,Details)
SELECT UserID,'SEED','Database','1','Созданы демонстрационные данные' FROM dbo.Users WHERE Login='admin';
GO
