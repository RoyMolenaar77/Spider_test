/****** Object:  Table [dbo].[UserPortalPortlet]    Script Date: 01/28/2011 10:18:37 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[UserPortalPortlet](
      [UserID] [int] NOT NULL,
      [PortalID] [int] NOT NULL,
      [PortletID] [int] NOT NULL,
      [Column] [int] NULL,
      [Row] [int] NULL,
CONSTRAINT [PK_UserPortalPortlet_1] PRIMARY KEY CLUSTERED 
(
      [UserID] ASC,
      [PortalID] ASC,
      [PortletID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[UserPortalPortlet]  WITH CHECK ADD  CONSTRAINT [FK_UserPortalPortlet_Portlet] FOREIGN KEY([PortletID])
REFERENCES [dbo].[Portlet] ([PortletID])
GO

ALTER TABLE [dbo].[UserPortalPortlet] CHECK CONSTRAINT [FK_UserPortalPortlet_Portlet]
GO

ALTER TABLE [dbo].[UserPortalPortlet]  WITH CHECK ADD  CONSTRAINT [FK_UserPortalPortlet_UserPortal] FOREIGN KEY([PortalID], [UserID])
REFERENCES [dbo].[UserPortal] ([PortalID], [UserID])
GO

ALTER TABLE [dbo].[UserPortalPortlet] CHECK CONSTRAINT [FK_UserPortalPortlet_UserPortal]
GO

ALTER TABLE dbo.Language ALTER COLUMN DisplayCode NVARCHAR(50) NOT NULL 