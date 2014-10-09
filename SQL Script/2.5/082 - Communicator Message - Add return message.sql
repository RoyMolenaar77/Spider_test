if not exists(select *from communicatormessage where [type] = 6)
begin
	insert into CommunicatorMessage values (6, 'Return', 'returns', 0)
end

if not exists(select * from vendorcommunicatormessage vcm inner join communicatormessage cm on cm.id= vcm.messageid where [type] = 6 and vendorid= 15)
begin 
	insert into VendorCommunicatorMessage (MessageID, VendorID)
	select ID, 15
  from Communicatormessage where [type] = 6
  
end

if not exists(select * from vendorcommunicatormessage vcm inner join communicatormessage cm on cm.id= vcm.messageid where [type] = 6 and vendorid= 25)
begin 
		insert into VendorCommunicatorMessage (MessageID, VendorID)
	select ID, 25
  from Communicatormessage where [type] = 6
end