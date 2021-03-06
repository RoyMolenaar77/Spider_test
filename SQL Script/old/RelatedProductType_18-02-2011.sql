CREATE TABLE dbo.RelatedProductType
	(
	RelatedProductTypeID int NOT NULL IDENTITY (1, 1) primary key,
	Type nvarchar(100) NOT NULL
	)  ON [PRIMARY]

alter table relatedproduct add RelatedProductTypeID int NULL

ALTER TABLE dbo.RelatedProduct ADD CONSTRAINT
	FK_RelatedProduct_RelatedProductType FOREIGN KEY
	(
	RelatedProductTypeID
	) REFERENCES dbo.RelatedProductType
	(
	RelatedProductTypeID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 