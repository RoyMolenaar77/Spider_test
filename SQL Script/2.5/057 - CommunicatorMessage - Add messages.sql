if not exists ( select * from communicatormessage where type = 2)
begin
	insert into communicatormessage values (2, 'StockMutations', 'vrdmut', 0)
end

if not exists ( select * from communicatormessage where type = 3)
begin
	insert into communicatormessage values (3, 'TransferOrder\In', 'voormelding\in', 1)
end


if not exists ( select * from communicatormessage where type = 4)
begin
	insert into communicatormessage values (4, 'TransferOrder\Out', 'voormelding\out', 0)
end


if not exists (select * from vendorcommunicatormessage vcm inner join communicatormessage cm on cm.id = vcm.messageid where vcm.vendorid = 1 and cm.type = 2)
	begin
	insert into vendorcommunicatormessage (messageid, vendorid)
	select Id, 1 from communicatormessage 
	end

  if not exists (select * from vendorcommunicatormessage vcm inner join communicatormessage cm on cm.id = vcm.messageid where vcm.vendorid = 15 and cm.type = 2)
	begin
	insert into vendorcommunicatormessage (messageid, vendorid)
	select Id, 15 from communicatormessage 
	end

    if not exists (select * from vendorcommunicatormessage vcm inner join communicatormessage cm on cm.id = vcm.messageid where vcm.vendorid = 25 and cm.type = 2)
	begin
	insert into vendorcommunicatormessage (messageid, vendorid)
	select Id, 25 from communicatormessage 
	end