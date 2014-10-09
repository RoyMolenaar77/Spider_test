  alter table dbo.UserDownload
  ADD 
	IsProduct bit not null,
	[MediaName] nvarchar(500) not null; 
	
  
  go