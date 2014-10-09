alter table portalnotification add 
IsResolved bit null 

update portalnotification set IsResolved = 0

alter table portalnofication alter column IsResolved bit not null