IF OBJECT_ID('[tmp].[{0}]') IS NOT NULL
  DROP TABLE [tmp].[{0}]

CREATE TABLE [tmp].[{0}]
(
  [ConnectorPublicationRuleID]  INT             NOT NULL,
  [ConnectorID]                 INT             NOT NULL,
  [ProductID]                   INT             NOT NULL,
  [ShortDescription]            NVARCHAR(2000)  NULL,
  [LongDescription]             NVARCHAR(MAX)   NULL
)

CREATE NONCLUSTERED INDEX CIN ON [tmp].[{0}] ([ConnectorID], [ProductID])