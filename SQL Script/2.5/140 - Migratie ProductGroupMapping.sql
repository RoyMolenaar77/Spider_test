-- =============================================
-- Author:		Dashti
-- Create date: 29-9-2013
-- Description:	Migratie van ProductGroupMapping to MasterGroupMapping
-- -> @CopyProductGroupMappingToRoot: 1: Copy ProductGroupMappings to the root of MasterGroupMapping
--																		0: Copy ProductGroupMappings to a specific MasterGroupMapping, in this case should @RootMasterGroupMappingName not be empty
-- -> @ReverseScore: 1: To reverse the order of MasterGroupMapping after migration
--									 0: To use same score as ProductGroupMapping
-- -> @RootMasterGroupMappingName: if CopyProductGroupMappingToRoot is 0 then a new MasterGroupMapping created after that the current ProductGroupMappings will be placed here
-- -> @CopyFromConnectorID: Source ConnectorID of ProductGroupMapping 
-- -> @CopyToConnectorID: Destination ConnectorID of MasterGroupMapping. Null for MasterGroupMapping, 0..9 for ConnectorMapping
-- TODO: Copy MagentoSetting.
--			 Test Copy ProductGroupMapping to ConnectorMapping
-- =============================================
CREATE PROCEDURE [dbo].[MigrationProductGroupMapping]
  @CopyProductGroupMappingToRoot BIT = 0
, @ReverseScore BIT
, @RootMasterGroupMappingName NVARCHAR(255) = NULL
, @CopyFromConnectorID INT
, @CopyToConnectorID INT = NULL
, @OnlyMigrateForVendor INT = NULL
AS 
  BEGIN

    PRINT 'Start migration';

    BEGIN TRY
      SET NOCOUNT ON;

      BEGIN TRANSACTION;

      IF @CopyProductGroupMappingToRoot = 0 
        BEGIN
          DECLARE @RootMasterGroupMappingID INT = NULL;
          DECLARE @ProductGroupID INT = NULL;

          IF @RootMasterGroupMappingName IS NOT NULL 
            BEGIN
              SELECT  @RootMasterGroupMappingID = m.MasterGroupMappingID
              FROM    dbo.MasterGroupMappingLanguage mgml
              INNER JOIN dbo.MasterGroupMapping m ON mgml.MasterGroupMappingID = m.MasterGroupMappingID
              WHERE   NAME = @RootMasterGroupMappingName
                      AND ParentMasterGroupMappingID IS NULL;

              IF @RootMasterGroupMappingID IS NULL 
                BEGIN
                  SELECT TOP 1
                          @ProductGroupID = ProductGroupID
                  FROM    dbo.ProductGroup pg

                  IF @ProductGroupID IS NULL 
                    BEGIN
                      INSERT  dbo.ProductGroup
                              ( Score, Searchable, ImagePath )
                      VALUES  ( 0, 0, NULL )

                      SET @ProductGroupID = SCOPE_IDENTITY()
                    END

                  BEGIN TRY
                    INSERT  dbo.MasterGroupMapping
                            (
                              ParentMasterGroupMappingID
                            , ProductGroupID
                            , Score
                            , ConnectorID
                            , SourceMasterGroupMappingID
                            , FlattenHierarchy
                            , FilterByParentGroup
                            , ExportID
                            , SourceProductGroupMappingID
                            , ImagePath
							              )
                    VALUES  (
                              NULL
                            , @ProductGroupID
                            , 0
                            , @CopyToConnectorID
                            , NULL
                            , 0
                            , 0
                            , NULL
                            , NULL
                            , NULL
							              )
                  END TRY

                  BEGIN CATCH
                    RAISERROR (
								'Failed to create a new MasterGroupMapping'
								,16
								,1
								);
                  END CATCH

                  SET @RootMasterGroupMappingID = SCOPE_IDENTITY();

                  IF @RootMasterGroupMappingID IS NULL 
                    BEGIN
                      RAISERROR (
								'MasterGroupMappingId %s is not correct.'
								,@RootMasterGroupMappingID
								,16
								,1
								);
                    END

                  INSERT  dbo.MasterGroupMappingLanguage
                          (
                            MasterGroupMappingID
                          , LanguageID
                          , NAME
						              )
                          SELECT  @RootMasterGroupMappingID
                          ,       LanguageID
                          ,       @RootMasterGroupMappingName
                          FROM    dbo.LANGUAGE l
                END
              ELSE 
                BEGIN
                  RAISERROR (
							'MasterGroupMapping %s is already exist'
							,@RootMasterGroupMappingName
							,16
							,1
							);
                END
            END
          ELSE 
            BEGIN
              RAISERROR (
						'If you want to copy ProductGroupMapping to a specific MasterGroupMapping then you should fill the name of MasterGroupMapping in @RootMasterGroupMappingName'
						,16
						,1
						);
            END
        END

      BEGIN TRY
        EXEC dbo.CopyProductGroupMappingToConnectorMappingPerConnector @SourceConnectorID = @CopyFromConnectorID, @RootID = @RootMasterGroupMappingID, @DestinationConnectorMappingID = @CopyToConnectorID
      END TRY

      BEGIN CATCH
        RAISERROR (
					'Failed to generate MasterGroupMappings'
					,16
					,1
					);
      END CATCH

      BEGIN TRY
        DECLARE @MasterGroupMappingProductGroupMapping TABLE
          (
            MasterGroupMappingID INT
          , ProductGroupVendorID INT
          );

        WITH  masterGroupMappingHierarchy
                AS ( SELECT m.MasterGroupMappingID
                     FROM   MasterGroupMapping m
                     WHERE  ISNULL(ParentMasterGroupMappingID, '') = ISNULL(@RootMasterGroupMappingID, '')
                     UNION ALL
                     SELECT m.MasterGroupMappingID
                     FROM   MasterGroupMapping m
                     INNER JOIN masterGroupMappingHierarchy mh ON m.ParentMasterGroupMappingID = mh.MasterGroupMappingID
                   )
          INSERT  @MasterGroupMappingProductGroupMapping
                  (
                    MasterGroupMappingID
                  , ProductGroupVendorID
				          )
                  SELECT  m.MasterGroupMappingID
                  ,       pgv.ProductGroupVendorID
                  FROM    MasterGroupMapping m
                  INNER JOIN masterGroupMappingHierarchy mh ON m.MasterGroupMappingID = mh.MasterGroupMappingID
                  INNER JOIN dbo.ProductGroupVendor pgv ON m.ProductGroupID = pgv.ProductGroupID and pgv.VendorID in ( CASE WHEN @OnlyMigrateForVendor is NOT NULL THEN (@OnlyMigrateForVendor) ELSE (select VendorID from Vendor) END )
				  

        MERGE MasterGroupMappingProductGroupVendor AS t
          USING 
            ( SELECT DISTINCT
                      MasterGroupMappingID
              ,       ProductGroupVendorID
              FROM    @MasterGroupMappingProductGroupMapping mgmpgm
            ) AS s
          ON s.MasterGroupMappingID = t.MasterGroupMappingID
            AND s.ProductGroupVendorID = t.ProductGroupVendorID
          WHEN NOT MATCHED 
            THEN
					INSERT  (
                    MastergroupMappingID
                  , ProductGroupVendorID
						      )
               VALUES
                  (
                    s.MasterGroupMappingID
                  , s.ProductGroupVendorID
						      );
      END TRY

      BEGIN CATCH
        RAISERROR (
					'Failed to insert coupling between MasterGroupMapping and ProductGroupVendor'
					,16
					,- 1
					);
      END CATCH

      IF @ReverseScore = 1 
        BEGIN
          DECLARE @maxScore INT;
          DECLARE @CountRows INT;

          SELECT  @maxScore = MAX(Score)
          FROM    dbo.MasterGroupMapping m

          SELECT  @CountRows = COUNT(1)
          FROM    dbo.MasterGroupMapping m

          PRINT @maxScore;
          PRINT @CountRows;

          IF @CountRows > @maxScore 
            SET @maxScore = @CountRows;

          WITH  masterGroupMappingHierarchy
                  AS ( SELECT m.MasterGroupMappingID
                       FROM   MasterGroupMapping m
                       WHERE  ISNULL(ParentMasterGroupMappingID, '') = ISNULL(@RootMasterGroupMappingID, '')
                       UNION ALL
                       SELECT m.MasterGroupMappingID
                       FROM   MasterGroupMapping m
                       INNER JOIN masterGroupMappingHierarchy mh ON m.ParentMasterGroupMappingID = mh.MasterGroupMappingID
                     )
            UPDATE  m
            SET     Score = @maxScore - Score
            FROM    MasterGroupMapping m
            INNER JOIN masterGroupMappingHierarchy mh ON m.MasterGroupMappingID = mh.MasterGroupMappingID
						WHERE ConnectorID = @CopyToConnectorID
        END

      COMMIT TRANSACTION;
    END TRY

    BEGIN CATCH
      IF @@TRANCOUNT <> 0 
        ROLLBACK TRANSACTION;

      SELECT  ERROR_LINE() AS [Error_Line]
      ,       ERROR_MESSAGE() AS [Error_Message]
      ,       ERROR_NUMBER() AS [Error_Number]
      ,       ERROR_SEVERITY() AS [Error_Severity]
      ,       ERROR_PROCEDURE() AS [Error_Procedure];

      RAISERROR (
					'Migration failed.'
					,16
					,1
					);
    END CATCH

    PRINT 'Finshed migration';

  END



GO


