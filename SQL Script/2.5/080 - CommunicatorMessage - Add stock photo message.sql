if not exists(select *from communicatormessage where [type] = 5)
begin
	insert into CommunicatorMessage values (5, 'StockPhoto', 'vrdfoto', 0)

end

if not exists(select * from vendorcommunicatormessage vcm inner join communicatormessage cm on cm.id= vcm.messageid where [type] = 5 and vendorid= 15)
begin 
	insert into VendorCommunicatorMessage (MessageID, VendorID)
	select ID, 15
  from Communicatormessage where [type] = 5
  
end

if not exists(select * from vendorcommunicatormessage vcm inner join communicatormessage cm on cm.id= vcm.messageid where [type] = 5 and vendorid= 25)
begin 
		insert into VendorCommunicatorMessage (MessageID, VendorID)
	select ID, 25
  from Communicatormessage where [type] = 5
end