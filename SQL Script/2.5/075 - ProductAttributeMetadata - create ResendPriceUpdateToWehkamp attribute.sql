if not exists (select * from productattributemetadata where attributecode = 'ResendPriceUpdateToWehkamp')
	begin
	set identity_insert productattributemetadata on 
	insert into productattributemetadata (AttributeID, AttributeCode, ProductAttributeGroupID, isvisible, needsupdate, vendorid, issearchable, createdby, creationtime, [index])
	values (96, 'ResendPriceUpdateToWehkamp', 4, 0, 0, 48, 0,1,getdate(), 0)

	insert into productattributename (attributeid, languageid, name)
	values (96,1, 'ResendPriceUpdateToWehkamp')

	set identity_insert productattributemetadata off
end


if not exists (select * from productattributemetadata where attributecode = 'ResendProductInformationToWehkamp')
	begin
	set identity_insert productattributemetadata on 
	insert into productattributemetadata (AttributeID, AttributeCode, ProductAttributeGroupID, isvisible, needsupdate, vendorid, issearchable, createdby, creationtime, [index])
	values (97, 'ResendProductInformationToWehkamp', 4, 0, 0, 48, 0,1,getdate(), 0)

	insert into productattributename (attributeid, languageid, name)
	values (97,1, 'ResendProductInformationToWehkamp')

	set identity_insert productattributemetadata off
end
