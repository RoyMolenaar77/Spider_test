-- =============================================
-- Author:		Dashti
-- Create date: today :P, 09-02-2013
-- Description:	Copy product group mappings to master group mapping 
-- =============================================
CREATE PROCEDURE [dbo].[CopyProductGroupMappingToConnectorMappingPerConnector] 
	@SourceConnectorID INT,
  @DestinationConnectorMappingID int = NULL
	,@Root INT = NULL
	,@RootID INT = NULL
	,@MasterGroupMappingScore INT = 0
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @CurrentLevel TABLE (
		ProductGroupMappingID INT
		,ProductGroupID INT
		,ParentProductGroupMappingID INT
		,FlattenHierarchy BIT
		,FilterByParentGroup BIT
		,Lineage NVARCHAR(500)
		,Score INT
		,CustomProductGroupLabel NVARCHAR(255)
		,ProductGroupMappingPath NVARCHAR(255) NULL
		,MappingThumbnailImagePath NVARCHAR(1000) NULL
		)

	INSERT INTO @CurrentLevel
	SELECT ProductGroupMappingID
		,ProductGroupID
		,ParentProductGroupMappingID
		,FlattenHierarchy
		,FilterByParentGroup
		,Lineage
		,Score
		,CustomProductGroupLabel
		,ProductGroupMappingPath
		,MappingThumbnailImagePath
	FROM ProductGroupMapping PGM
	WHERE PGM.ConnectorID = @SourceConnectorID
		AND ISNULL(ParentProductGroupMappingID, - 1) = ISNULL(@Root, - 1)
	ORDER BY score DESC

	DECLARE @ProductGroupID INT
	DECLARE @ProductGroupMappingID INT
	DECLARE @ParentProductGroupMappingID INT
	DECLARE @FlattenHierarchy BIT
	DECLARE @FilterByParentGroup BIT
	DECLARE @Lineage NVARCHAR(500)
	DECLARE @NewRootID INT
	DECLARE @Score INT
	DECLARE @CustomProductGroupLabel NVARCHAR(255)
	DECLARE @ProductGroupIDs TABLE (ProductGroupID INT NULL)

    DECLARE @ProductGroupMappingPath NVARCHAR(255)
	DECLARE @MappingThumbnailImagePath NVARCHAR(1000)

	WHILE EXISTS (
			SELECT TOP 1 ProductGroupMappingID
			FROM @CurrentLevel
			ORDER BY ProductGroupMappingID DESC
			)
	BEGIN
		SELECT TOP 1 @ProductGroupID = ProductGroupID
			,@ProductGroupMappingID = ProductGroupMappingID
			,@Lineage = Lineage
			,@FlattenHierarchy = FlattenHierarchy
			,@FilterByParentGroup = FilterByParentGroup
			,@CustomProductGroupLabel = CustomProductGroupLabel
			,@Score = Score
			,@ProductGroupMappingPath = ProductGroupMappingPath
			,@MappingThumbnailImagePath = MappingThumbnailImagePath
		FROM @CurrentLevel
		ORDER BY ProductGroupMappingID DESC

		INSERT INTO MasterGroupMapping (
			ConnectorID
			,ParentMasterGroupMappingID
			,ProductGroupID
			,FlattenHierarchy
			,FilterByParentGroup
			,Score
			,ExportID
			,ImagePath
			,ThumbnailPath			
			)
		VALUES (
			@DestinationConnectorMappingID
			,@RootID
			,@ProductGroupID
			,@FlattenHierarchy
			,@FilterByParentGroup
			,@Score
			,CASE WHEN @SourceConnectorID is not null THEN @ProductGroupMappingID ELSE NULL END
			,CASE WHEN @ProductGroupMappingPath is not null THEN  @ProductGroupMappingPath ELSE NULL END
			,CASE WHEN @MappingThumbnailImagePath is not null THEN @MappingThumbnailImagePath ELSE NULL END
			)

		SET @MasterGroupMappingScore = @MasterGroupMappingScore + 1
		SET @NewRootID = SCOPE_IDENTITY()

		
		print @NewRootID;

		IF @DestinationConnectorMappingID is null
		BEGIN
		update ProductGroupMapping
		set MasterGroupMappingID = @NewRootID
		where ProductGroupMappingID = @ProductGroupMappingID
		END
		
		ELSE		
		BEGIN

		Declare @SourceMasterGroupMapping INT = (select MasterGroupMappingID from ProductGroupMapping where ProductGroupMappingID = @ProductGroupMappingID);
		
		UPDATE MasterGroupMapping set SourceMasterGroupMappingID = @SourceMasterGroupMapping
		WHERE MasterGroupMappingID = @NewRootID	

		insert into ProductGroupMappingConnectorNotActive(MasterGroupMappingID, ConnectorID)
		select @NewRootID as MasterGroupMappingID, ConnectorID From Connector 
		where ConnectorID = @DestinationConnectorMappingID or ParentConnectorID = @DestinationConnectorMappingID
		
		END

		DECLARE @LanguageID INT;

		IF isnull(@CustomProductGroupLabel, '') = ''
		BEGIN
			DECLARE @Name VARCHAR(255)

			DECLARE c CURSOR LOCAL STATIC READ_ONLY FORWARD_ONLY
			FOR
			SELECT pgl.LanguageID
				,pgl.NAME
			FROM ProductGroup pg
			INNER JOIN ProductGroupLanguage pgl ON pg.ProductGroupID = pgl.ProductGroupID
			WHERE pg.ProductGroupID = @ProductGroupID;

			OPEN c;

			FETCH NEXT
			FROM c
			INTO @LanguageID
				,@Name;

			WHILE @@FETCH_STATUS = 0
			BEGIN
				INSERT INTO MasterGroupMappingLanguage (
					MasterGroupMappingID
					,languageID
					,NAME
					)
				VALUES (
					@NewRootID
					,@LanguageID
					,@Name
					)

				FETCH NEXT
				FROM c
				INTO @LanguageID
					,@Name;
			END

			CLOSE c;

			DEALLOCATE c;
		END
		ELSE
		BEGIN
			DECLARE c CURSOR LOCAL STATIC READ_ONLY FORWARD_ONLY
			FOR
			SELECT LanguageID
			FROM LANGUAGE;

			OPEN c;

			FETCH NEXT
			FROM c
			INTO @LanguageID;

			WHILE @@FETCH_STATUS = 0
			BEGIN
				INSERT INTO MasterGroupMappingLanguage (
					MasterGroupMappingID
					,languageID
					,NAME
					)
				VALUES (
					@NewRootID
					,@LanguageID
					,@CustomProductGroupLabel
					)

				FETCH NEXT
				FROM c
				INTO @LanguageID;
			END

			CLOSE c;

			DEALLOCATE c;
		END

		IF (
				SELECT count(*)
				FROM @ProductGroupIDs
				WHERE ProductGroupID = @ProductGroupID
				) < 1
			INSERT INTO @ProductGroupIDs
			VALUES (@ProductGroupID)

		EXEC dbo.CopyProductGroupMappingToConnectorMappingPerConnector @SourceConnectorID
			,@DestinationConnectorMappingID
			,@ProductGroupMappingID
			,@NewRootID

		DELETE
		FROM @CurrentLevel
		WHERE ProductGroupMappingID = (
				SELECT TOP 1 ProductGroupMappingID
				FROM @CurrentLevel
				ORDER BY ProductGroupMappingID DESC
				)
	END
END






GO


