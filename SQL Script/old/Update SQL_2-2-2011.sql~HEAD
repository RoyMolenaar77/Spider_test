begin tran lala

select orderid,orderresponseid 
into #reponse
from OrderResponse

alter table orderresponse drop constraint FK_OrderResponse_Order
alter table orderresponse drop column orderid

insert into OrderResponseOrder (OrderReponseID, OrderID)
select orderresponseid, orderid
from #reponse

drop table #reponse

commit tran lala


alter table connectorrelation add FtpConnectionType int null
alter table connectorrelation alter column outboundmessagetype int null

insert into ManagementGroup ([Group])
values ('EDI')

insert into ManagementPage (Name,Description,RoleID,JSAction,Icon,GroupID,ID,isVisible,FunctionalityName)
values('Connector relations','Connector relations',1,'ConnectorRelations','link',9,'connector-rules',1,'GetConnectorRelations')

