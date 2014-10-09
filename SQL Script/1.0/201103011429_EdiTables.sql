/****** Object:  Table [dbo].[EdiOrderResponse]    Script Date: 03/01/2011 14:17:13 ******/
CREATE TABLE [dbo].[EdiVendor](
	[EdiVendorID] [int] IDENTITY(1,1) NOT NULL primary key,
	Name nvarchar(255) not null
)

CREATE TABLE [dbo].[EdiFieldMapping](
	[EdiMappingID] [int] IDENTITY(1,1) NOT NULL primary key,
	TableName nvarchar(50) null,
	FieldName nvarchar(50) null,
	EdiVendorID int not null,
	VendorFieldName nvarchar(50) not null,
	VendorTableName nvarchar(50) not null,
	VendorFieldLength nvarchar(50) null,
	VendorDefaultValue nvarchar(50) null,
	EdiType int not null
)

CREATE TABLE [dbo].[EdiValidate](
	[EdiValidateID] [int] IDENTITY(1,1) NOT NULL primary key,
	TableName nvarchar(50) null,
	FieldName nvarchar(50) null,
	EdiVendorID int not null,
	MaxLength nvarchar(50) null,
	Type nvarchar(50) null,
	Value nvarchar(50) null,
	IsActive bit not null default 1,
	EdiType int not null
)

