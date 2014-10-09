alter table vendorprice add VendorPriceRuleID int null

alter table vendorprice add constraint FK_VendorPrice_VendorPriceRule foreign key (VendorPriceRuleID) references VendorPriceRule (VendorPriceRuleID)