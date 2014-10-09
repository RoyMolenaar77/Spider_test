if not exists (select * from productattributemetadata where attributecode = 'ReadyForWehkamp')
	begin
	set identity_insert productattributemetadata on 
	insert into productattributemetadata (AttributeID, AttributeCode, ProductAttributeGroupID, isvisible, needsupdate, vendorid, issearchable, createdby, creationtime)
	values (87, 'ReadyForWehkamp', 4, 0, 0, 48, 0,1,getdate())

	insert into productattributename (attributeid, languageid, name)
	values (87,1, 'ReadyForWehkamp')

	set identity_insert productattributemetadata off
end


if not exists (select * from productattributemetadata where attributecode = 'SentToWehkamp')
	begin
	set identity_insert productattributemetadata on 
	insert into productattributemetadata (AttributeID, AttributeCode, ProductAttributeGroupID, isvisible, needsupdate, vendorid, issearchable, createdby, creationtime)
	values (88, 'SentToWehkamp', 4, 0, 0, 48, 0,1,getdate())

	insert into productattributename (attributeid, languageid, name)
	values (88,1, 'SentToWehkamp')

	set identity_insert productattributemetadata off
end


if not exists (select * from productattributemetadata where attributecode = 'Pijpwijdte')
	begin
	set identity_insert productattributemetadata on 
	insert into productattributemetadata (AttributeID, AttributeCode, ProductAttributeGroupID, isvisible, needsupdate, vendorid, issearchable, createdby, creationtime)
	values (89, 'Pijpwijdte', 4, 0, 0, 48, 0,1,getdate())

	insert into productattributename (attributeid, languageid, name)
	values (89,1, 'Pijpwijdte')

	set identity_insert productattributemetadata off
end


if not exists (select * from productattributemetadata where attributecode = 'Kraagvorm')
	begin
	set identity_insert productattributemetadata on 
	insert into productattributemetadata (AttributeID, AttributeCode, ProductAttributeGroupID, isvisible, needsupdate, vendorid, issearchable, createdby, creationtime)
	values (90, 'Kraagvorm', 4, 0, 0, 48, 0,1,getdate())

	insert into productattributename (attributeid, languageid, name)
	values (90,1, 'Kraagvorm')

	set identity_insert productattributemetadata off
end

if not exists (select * from productattributemetadata where attributecode = 'Dessin')
	begin
	set identity_insert productattributemetadata on 
	insert into productattributemetadata (AttributeID, AttributeCode, ProductAttributeGroupID, isvisible, needsupdate, vendorid, issearchable, createdby, creationtime)
	values (91, 'Dessin', 4, 0, 0, 48, 0,1,getdate())

	insert into productattributename (attributeid, languageid, name)
	values (91,1, 'Dessin')

	set identity_insert productattributemetadata off
end

if not exists (select * from productattributemetadata where attributecode = 'MaterialDescription')
	begin
	set identity_insert productattributemetadata on 
	insert into productattributemetadata (AttributeID, AttributeCode, ProductAttributeGroupID, isvisible, needsupdate, vendorid, issearchable, createdby, creationtime)
	values (92, 'MaterialDescription', 4, 0, 0, 48, 0,1,getdate())

	insert into productattributename (attributeid, languageid, name)
	values (92,1, 'MaterialDescription')

	set identity_insert productattributemetadata off
end

if not exists (select * from productattributemetadata where attributecode = 'WehkampProduct')
	begin
	set identity_insert productattributemetadata on 
	insert into productattributemetadata (AttributeID, AttributeCode, ProductAttributeGroupID, isvisible, needsupdate, vendorid, issearchable, createdby, creationtime)
	values (93, 'WehkampProduct', 4, 0, 1, 48, 0,1,getdate())

	insert into productattributename (attributeid, languageid, name)
	values (93,1, 'WehkampProduct')

	set identity_insert productattributemetadata off
end


if not exists (select * from productattributemetadata where attributecode = 'SentToWehkampAsDummy')
	begin
	set identity_insert productattributemetadata on 
	insert into productattributemetadata (AttributeID, AttributeCode, ProductAttributeGroupID, isvisible, needsupdate, vendorid, issearchable, createdby, creationtime)
	values (94, 'SentToWehkampAsDummy', 4, 0, 1, 48, 0,1,getdate())

	insert into productattributename (attributeid, languageid, name)
	values (94,1, 'SentToWehkampAsDummy')

	set identity_insert productattributemetadata off
end

if not exists (select * from productattributemetadata where attributecode = 'WehkampProductNumber')
	begin
	set identity_insert productattributemetadata on 
	insert into productattributemetadata (AttributeID, AttributeCode, ProductAttributeGroupID, isvisible, needsupdate, vendorid, issearchable, createdby, creationtime)
	values (95, 'WehkampProductNumber', 4, 0, 1, 48, 0,1,getdate())

	insert into productattributename (attributeid, languageid, name)
	values (95,1, 'WehkampProductNumber')

	set identity_insert productattributemetadata off
end