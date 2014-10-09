alter table orderresponse add DocumentName nvarchar(255) null
alter table productattributemetadata add Mandatory bit not null default 0
alter table edivendor add EdiVendorType nvarchar(255) not null
alter table ediorder add edirequestid int not null
alter table ediorder drop column document 

ALTER TABLE ediorder
ADD CONSTRAINT FK_EdiOrder_EdiOrderListener FOREIGN KEY 
(EdiRequestid)
REFERENCES ediorderlistener
(EdiRequestid)

alter table ediorder add status int not null
alter table ediorderlistener drop column status
alter table ediorderlistener drop column instanceid
alter table ediorderlistener drop column customerorderreference
alter table ediorderlistener drop column bSKIdentifier
alter table ediorderlistener drop column type

 create table EdiOrderType(
	EdiOrderTypeID int not null identity(1,1) primary key,
	Name nvarchar(100)
 )

alter table ediorder add EdiOrderTypeID int not null
 
 ALTER TABLE EdiOrder
ADD CONSTRAINT FK_EdiOrder_EdiOrderType FOREIGN KEY 
(EdiOrderTypeID)
REFERENCES EdiOrderType
(EdiOrderTypeID)