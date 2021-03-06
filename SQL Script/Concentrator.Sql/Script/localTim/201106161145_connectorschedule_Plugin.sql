/****** Object:  ForeignKey [FK_ConnectorSchedule_Connector]    Script Date: 06/16/2011 11:43:58 ******/
ALTER TABLE [dbo].[ConnectorSchedule] DROP CONSTRAINT [FK_ConnectorSchedule_Connector]
GO
/****** Object:  ForeignKey [FK_ConnectorSchedule_Plugin]    Script Date: 06/16/2011 11:43:58 ******/
ALTER TABLE [dbo].[ConnectorSchedule] DROP CONSTRAINT [FK_ConnectorSchedule_Plugin]
GO
/****** Object:  Table [dbo].[ConnectorSchedule]    Script Date: 06/16/2011 11:43:58 ******/
ALTER TABLE [dbo].[ConnectorSchedule] DROP CONSTRAINT [FK_ConnectorSchedule_Connector]
GO
ALTER TABLE [dbo].[ConnectorSchedule] DROP CONSTRAINT [FK_ConnectorSchedule_Plugin]
GO
DROP TABLE [dbo].[ConnectorSchedule]
GO
/****** Object:  Table [dbo].[Plugin]    Script Date: 06/16/2011 11:43:58 ******/
DROP TABLE [dbo].[Plugin]
GO
/****** Object:  Table [dbo].[Plugin]    Script Date: 06/16/2011 11:43:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Plugin](
	[PluginID] [int] IDENTITY(1,1) NOT NULL,
	[PluginName] [nvarchar](50) NOT NULL,
	[PluginType] [nvarchar](255) NOT NULL,
	[PluginGroup] [nvarchar](50) NOT NULL,
	[CronExpression] [nvarchar](50) NOT NULL,
	[ExecuteOnStartup] [bit] NOT NULL,
	[LastRun] [datetime] NULL,
	[NextRun] [datetime] NULL,
	[Duration] [nvarchar](50) NULL,
 CONSTRAINT [PK_Plugin] PRIMARY KEY CLUSTERED 
(
	[PluginID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ConnectorSchedule]    Script Date: 06/16/2011 11:43:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ConnectorSchedule](
	[ConnectorScheduleID] [int] IDENTITY(1,1) NOT NULL,
	[ConnectorID] [int] NOT NULL,
	[PluginID] [int] NOT NULL,
	[LastRun] [datetime] NULL,
	[Duration] [nvarchar](50) NULL,
	[ScheduledNextRun] [datetime] NULL,
	[ConnectorScheduleStatus] [int] NOT NULL,
	[ExecuteOnStartup] [bit] NULL,
	[CronExpression] [nvarchar](50) NULL,
 CONSTRAINT [PK_ConnectorSchedule] PRIMARY KEY CLUSTERED 
(
	[ConnectorScheduleID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  ForeignKey [FK_ConnectorSchedule_Connector]    Script Date: 06/16/2011 11:43:58 ******/
ALTER TABLE [dbo].[ConnectorSchedule]  WITH CHECK ADD  CONSTRAINT [FK_ConnectorSchedule_Connector] FOREIGN KEY([ConnectorID])
REFERENCES [dbo].[Connector] ([ConnectorID])
GO
ALTER TABLE [dbo].[ConnectorSchedule] CHECK CONSTRAINT [FK_ConnectorSchedule_Connector]
GO
/****** Object:  ForeignKey [FK_ConnectorSchedule_Plugin]    Script Date: 06/16/2011 11:43:58 ******/
ALTER TABLE [dbo].[ConnectorSchedule]  WITH CHECK ADD  CONSTRAINT [FK_ConnectorSchedule_Plugin] FOREIGN KEY([PluginID])
REFERENCES [dbo].[Plugin] ([PluginID])
GO
ALTER TABLE [dbo].[ConnectorSchedule] CHECK CONSTRAINT [FK_ConnectorSchedule_Plugin]
GO
