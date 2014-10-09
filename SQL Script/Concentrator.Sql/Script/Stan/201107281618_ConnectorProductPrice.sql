create table ConnectorProductPrice (
	ProductID int not null ,
	ConnectorID int not null ,
	Price decimal(18,4) not null,
	ContentPriceRuleID int null,
	PriceRuleType int not null
	PRIMARY KEY (ProductID, ConnectorID, PriceRuleType)
)

alter table ConnectorProductPrice 
ADD CONSTRAINT FK_ConnectorProductPrice_Product FOREIGN KEY (ProductID) REFERENCES Product(ProductID)

alter table ConnectorProductPrice 
ADD CONSTRAINT FK_ConnectorProductPrice_Connector FOREIGN KEY (ConnectorID) REFERENCES Connector(ConnectorID)

alter table ConnectorProductPrice 
ADD CONSTRAINT FK_ConnectorProductPrice_ContentPrice FOREIGN KEY (ContentPriceRuleID) REFERENCES ContentPrice(ContentPriceRuleID)
