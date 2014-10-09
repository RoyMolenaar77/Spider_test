if not exists (select * from portlet where name = 'Refund queue')
begin
	insert into portlet values ('RefundQueue', 'Refund queue', 'Current state of refund queue')
end

if not exists(select * from managementpage where name = 'Refund queue')
begin
	insert into managementpage values ('Refund queue', 'Refund queue', 2, 'RefundQueue', 'node',6, 'refund-queue-item',1, 'ViewOrders')
end