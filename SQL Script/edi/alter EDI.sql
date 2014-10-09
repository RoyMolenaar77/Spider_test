alter table customer add CompanyName nvarchar(150) Null

ALTER TABLE EdiVendor 
ADD Constraint FK_EdiValidate_EdiVendor
Foreign KEY (EdiVendorID) REFERENCES EdiValidate(EdiVendorID);

alter table orderline add EndCustomerOrderNr nvarchar(100) null
alter table orderline add ProductDescription nvarchar(255) null
alter table orderline add Currency nvarchar(10) null
alter table orderline add UnitOfMeasure nvarchar(10) null

alter table ediorder add OrderDate datetime null