if not exists(
	select * from Portlet
	where Name = 'UrgentProducts' and Title = 'Urgent Products' and [description] = 'Products that have been sent to Wehkamp without enrichment'
	)
begin
	insert into Portlet (Name, Title, [Description])
	values('UrgentProducts', 'Urgent Products', 'Products that have been sent to Wehkamp without enrichment' )
end