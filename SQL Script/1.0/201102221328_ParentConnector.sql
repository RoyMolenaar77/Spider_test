ALTER TABLE dbo.Connector ADD ParentConnectorID INT NULL
ALTER TABLE dbo.Connector ADD CONSTRAINT Connector_ParentConnector FOREIGN KEY (ParentConnectorID) REFERENCES Connector (ConnectorID)