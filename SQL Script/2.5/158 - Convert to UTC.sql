begin tran orders
	
	--orders
	update [order] 
		set ReceivedDate = dbo.ToUniversal(ReceivedDate),
			DispatchToVendorDate = dbo.ToUniversal(DispatchToVendorDate)

	update [Event]
		set LastModificationTime = dbo.ToUniversal(LastModificationTime)

	update OrderLedger 
		set LedgerDate = dbo.ToUniversal(LedgerDate)

	update OrderResponse
		set OrderDate = dbo.ToUniversal(OrderDate),
			VendorDocumentDate = dbo.ToUniversal(VendorDocumentDate),
			ReqDeliveryDate = dbo.ToUniversal(ReqDeliveryDate),
			InvoiceDate = dbo.ToUniversal(InvoiceDate),
			ReceiveDate = dbo.ToUniversal(ReceiveDate)

	update OrderResponseLine 
		set DeliveryDate = dbo.ToUniversal(DeliveryDate),
			RequestDate = dbo.ToUniversal(RequestDate)

	update Outbound 
		set CreationTime = dbo.ToUniversal(CreationTime),
			ProcessDate  = dbo.ToUniversal(ProcessDate)

	update WehkampMessage
		set Received = dbo.ToUniversal(received),
			[sent] = dbo.ToUniversal([sent]),
			lastmodified = dbo.ToUniversal(lastModified)

	update DatcolLink 
		set DateCreated = dbo.ToUniversal(DateCreated)

commit tran orders