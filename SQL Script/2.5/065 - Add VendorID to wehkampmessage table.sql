IF NOT EXISTS (
	SELECT * 
    FROM   sys.columns 
    WHERE  name = 'VendorID' 
    AND object_id = OBJECT_ID('WehkampMessage')) 

BEGIN
       ALTER TABLE WehkampMessage
       ADD VendorID int NOT NULL DEFAULT(1),
	   CONSTRAINT [FK_WehkampMessage_Vendor] FOREIGN KEY (VendorID) REFERENCES Vendor;
	   
END
GO