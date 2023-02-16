CREATE DATABASE [SbusTestBusinessData]
GO

ALTER DATABASE [SbusTestBusinessData] SET COMPATIBILITY_LEVEL = 150
GO

USE [SbusTestBusinessData]
GO

CREATE TABLE [dbo].[MainTable](
                                  [Id] [int] IDENTITY(1,1) NOT NULL,
                                  [Data] [nvarchar](max) NOT NULL,
                                  [UpdatedDate] [datetime] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO






