using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects;
using System.IO;
using System.Net;
using System.Xml.Linq;
using System.Xml;
using Concentrator.Objects.DataAccess.EntityFramework;
using Concentrator.Objects.Vendors.Base;

namespace Concentrator.Plugins.IceCat.Bulk
{
  public class AttributeMetaDataBulk : IceCatHTTPLoader<ConcentratorDataContext>
  {
    private const string _localFileName = "AttributeMetaDataIndex.xml";

    public const string AttributeMetaTable = "[DataStaging].[ICECAT_Attributes]";
    public const string AttributeGroupMetaTable = "[DataStaging].[ICECAT_AttributeGroups]";

    private string _attributeTableCreate = String.Format(@"CREATE TABLE {0} (
                                                              CatGroupID INT NOT NULL,
                                                              CatFeatureID INT NOT NULL,
                                                              FeatureID INT NOT NULL,
                                                              [Name] NVarChar(255) NOT NULL,
                                                              [LangID] INT NOT NULL,
                                                              [Index] INT NOT NULL,
                                                              [IsVisible] BIT NOT NULL,
                                                              [Sign] NVarChar(50) NOT NULL,
                                                              [Searchable] Bit NOT NULL,
                                                              [ConcentratorID] INT NULL
                                                          ) ON [Primary]
                                                          ", AttributeMetaTable);

    private string _attributeGroupTableCreate = String.Format(@"CREATE TABLE {0} (
                                                                 [GroupID] INT NOT NULL,
                                                                 [Name] NVarChar(500) NOT NULL,
                                                                 [Index] INT NOT NULL,
                                                                 [LangID] INT NOT NULL,
                                                                 [ConcentratorID] INT NULL
                                                                ) ON [Primary]
                                                              ", AttributeGroupMetaTable);

    string url = "/export/freexml.int/refs/CategoryFeaturesList.xml.gz";
    public AttributeMetaDataBulk(string baseUri, string localCache, NetworkCredential credentials, bool openIceCat)
      : base(baseUri, credentials)
    {
      LocalCacheLocation = Path.Combine(localCache, _localFileName);

      if (!openIceCat)
        url = "export/level4/refs/CategoryFeaturesList.xml.gz";
    }

    public string LocalCacheLocation { get; private set; }


    public override void Init(ConcentratorDataContext context)
    {
      base.Init(context);

      context.ExecuteStoreCommand(_attributeGroupTableCreate);
      context.ExecuteStoreCommand(_attributeTableCreate);

      DownloadFile(url, LocalCacheLocation + ".gz");
      BasicUnzip.Unzip(LocalCacheLocation + ".gz", LocalCacheLocation);

 
        using (var file = File.OpenRead(LocalCacheLocation))
        {
          var reader = new XmlTextReader(file);
          var aR = new AttributeGroupReader();
          aR.Load(reader);
          BulkLoad(AttributeGroupMetaTable, 1000, aR);
        }

        using (var file = File.OpenRead(LocalCacheLocation))
        {
          var reader = new XmlTextReader(file);
          var aR = new AttributeReader();
          aR.Load(reader);
          BulkLoad(AttributeMetaTable, 10000, aR);
        }


      // Deletes all attributes we do not have values for
      context.ExecuteStoreCommand(String.Format(@"DELETE A 
                                             FROM {0} A 
                                             WHERE (	SELECT COUNT(*)
		                                                  FROM {1} S
		                                                  WHERE S.CatFeatureID = A.CatFeatureID
                                              ) <= 0", AttributeMetaTable, ProductBulk.ProductSpecTable));

      // Delete all attributegroups for which we do not have attributes
      context.ExecuteStoreCommand(String.Format(@"DELETE G
                                              FROM {0} G
                                              WHERE G.GroupID NOT IN (
	                                              SELECT  S.CatGroupID
	                                              FROM {1} S
                                              )", AttributeGroupMetaTable, AttributeMetaTable));
    }

    public override void Sync(ConcentratorDataContext context)
    {
      #region Sync groups
      // Insert new groups into ProductAttributeGroupMetaData
      context.ExecuteStoreCommand(String.Format(@"INSERT INTO ProductAttributeGroupMetaData ([Index], GroupCode, VendorID)
                                             SELECT A.[Index], A.GroupID, {1}
                                             FROM {0} A
	                                             LEFT JOIN ProductAttributeGroupMetaData I ON (VendorID = {1} AND I.GroupCode = A.GroupID)
                                             WHERE I.ProductAttributeGroupID IS NULL
                                             GROUP BY A.[Index], A.[GroupID]", AttributeGroupMetaTable, DataImport.VendorID));

      // Syncing ProductAttributeGroupMetaData concentrator id's back to group staging table so we can easily join later on
      context.ExecuteStoreCommand(String.Format(@"UPDATE A 
                                             SET A.ConcentratorID = I.ProductAttributeGroupID
                                             FROM {0} A
	                                              LEFT JOIN ProductAttributeGroupMetaData I ON (I.GroupCode = A.GroupID AND VendorID = {1})
                                             WHERE I.ProductAttributeGroupID IS NOT NULL", AttributeGroupMetaTable, DataImport.VendorID));

      // Set the index on concentrator att groups
      context.ExecuteStoreCommand(String.Format(@"UPDATE PA 
                                             SET PA.[Index] = IA.[Index]
                                             FROM ProductAttributeGroupMetaData PA
	                                             LEFT JOIN {0} IA ON (IA.ConcentratorID = PA.ProductAttributeGroupID)
                                             WHERE PA.VendorID = {1}
                                             AND IA.ConcentratorID IS NOT NULL", AttributeGroupMetaTable, DataImport.VendorID));

      // Merge attribute group names
      context.ExecuteStoreCommand(String.Format(@"MERGE 
                                              ProductAttributeGroupName as CN
                                            USING 
                                              {0} as IG
                                            ON
                                              (CN.ProductAttributeGroupID = IG.ConcentratorID AND CN.LanguageID = IG.[LangID])
                                            WHEN NOT MATCHED BY TARGET
                                              THEN
	                                              INSERT (Name, ProductAttributeGroupID, LanguageID)
	                                              VALUES([IG].Name, [IG].ConcentratorID, [IG].[LangID])
                                            WHEN MATCHED
                                              THEN 
	                                              UPDATE SET
		                                              [CN].Name = [IG].Name;", AttributeGroupMetaTable));

      #endregion Sync groups

      #region Sync attribute metadata

      // Insert new attributes into ProductAttributeMetaData
      context.ExecuteStoreCommand(String.Format(@"INSERT INTO ProductAttributeMetaData (AttributeCode, ProductAttributeGroupID, [Index], IsVisible, NeedsUpdate, VendorID, IsSearchable, [Sign], CreatedBy, CreationTime) 
                                              SELECT IC.CatFeatureID, IG.ConcentratorID, IC.[Index], IC.IsVisible, 1, {0}, IC.Searchable, IC.[Sign], 1, GETDATE()
                                              FROM {1} IC
	                                              LEFT JOIN {2} IG ON IG.GroupID = IC.CatGroupID
	                                              LEFT JOIN ProductAttributeMetaData PM ON (PM.VendorID = {0} AND PM.AttributeCode = IC.CatFeatureID)
                                              WHERE PM.AttributeID IS NULL and ig.concentratorid IS NOT NULL
                                              GROUP BY IC.CatFeatureID, IG.ConcentratorID, IC.[Index], IC.IsVisible, IC.Searchable, IC.[Sign]
                                  ", DataImport.VendorID, AttributeMetaTable, AttributeGroupMetaTable));

      // Sync back the concentrator keys to the staging table for easy future joining
      context.ExecuteStoreCommand(String.Format(@"UPDATE A
                                             SET A.ConcentratorID = I.AttributeID
                                             FROM {0} A
	                                             LEFT JOIN ProductAttributeMetaData I ON (I.VendorID = {1} AND I.AttributeCode = A.CatFeatureID)
                                             WHERE I.AttributeID IS NOT NULL", AttributeMetaTable, DataImport.VendorID));


      // Update the info on existing attributes
      context.ExecuteStoreCommand(String.Format(@"UPDATE A
                                             SET A.ProductAttributeGroupID = IG.ConcentratorID, 
	                                               A.[Index] = IA.[Index], 
	                                               A.IsVisible = [IA].[IsVisible], 
	                                               A.NeedsUpdate = 1, 
	                                               A.IsSearchable = [IA].[Searchable], 
	                                               A.[Sign] = [IA].[Sign]
                                            FROM ProductAttributeMetaData A
	                                            LEFT JOIN {0} IA ON (A.AttributeID = IA.ConcentratorID)
	                                            LEFT JOIN {1} IG ON (IG.GroupID = IA.CatGroupID)
                                            WHERE A.VendorID = {2}
                                            AND IA.ConcentratorID IS NOT NULL AND IG.ConcentratorID IS NOT NULL", AttributeMetaTable, AttributeGroupMetaTable, DataImport.VendorID));

      // Merge attribute names
      context.ExecuteStoreCommand(String.Format(@"MERGE 
	                                              ProductAttributeName as CN
                                              USING
	                                              (select * from {0} where concentratorid IS NOT NULL) IA
                                              ON
	                                              ([CN].[AttributeID] = [IA].[ConcentratorID] AND [CN].[LanguageID] = [IA].[LangID])
                                              WHEN NOT MATCHED BY TARGET
	                                              THEN
		                                              INSERT (AttributeID, LanguageID, [Name])
		                                              VALUES ([IA].ConcentratorID, [IA].[LangID], [IA].[Name])
                                              WHEN MATCHED
	                                              THEN
		                                              UPDATE SET
			                                              [CN].[Name] = [IA].[Name];", AttributeMetaTable));

      #endregion Sync attribute metadata
    }

    protected override void TearDown(ConcentratorDataContext context)
    {
      context.ExecuteStoreCommand(String.Format("DROP TABLE {0}", AttributeGroupMetaTable));
      context.ExecuteStoreCommand(String.Format("DROP TABLE {0}", AttributeMetaTable));

      File.Delete(LocalCacheLocation);
      File.Delete(LocalCacheLocation + ".gz");
    }

    private class AttributeReader : XmlDataReader
    {
      private const int _fieldCount = 10;

      public override int FieldCount
      {
        get { return _fieldCount; }
      }

      protected override IEnumerable<object[]> Enumerate(XmlReader reader)
      {
        while (reader.Read())
        {
          if (reader.NodeType == XmlNodeType.Element && reader.Name == "Feature")
          {
            var element = XElement.ReadFrom(reader) as XElement;
            if (element.IsEmpty)
              continue;

            var catGroupID = (int)element.Attribute("CategoryFeatureGroup_ID");
            var catFeatureID = (int)element.Attribute("CategoryFeature_ID");
            var featureID = (int)element.Attribute("ID");
            int no = 0;
            int.TryParse(element.Attribute("No").Value, out no);              

            var cla = (int)element.Attribute("Class") == 0 ? 1 : 0;
            var sign = element.Element("Measure").Attribute("Sign").Value;
            var searchable = (int)element.Attribute("Searchable");

            var names = from n in element.Elements("Name")
                        let lang = (int)n.Attribute("langid")
                        where DataImport.Languages.ContainsKey(lang)
                        select new
                        {
                          Value = n.Attribute("Value").Value,
                          Lang = lang
                        };

            foreach (var name in names)
            {
              yield return new object[] { catGroupID, catFeatureID, featureID, name.Value, name.Lang, no, cla, sign, searchable, null };
            }
          }
        }
      }
    }

    private class AttributeGroupReader : XmlDataReader
    {
      private const int _fieldCount = 5;

      public override int FieldCount
      {
        get { return _fieldCount; }
      }

      protected override IEnumerable<object[]> Enumerate(System.Xml.XmlReader reader)
      {
        while (reader.Read())
        {
          if (reader.NodeType == XmlNodeType.Element && reader.Name == "CategoryFeatureGroup")
          {
            var featureElement = XElement.ReadFrom(reader) as XElement;
            if (featureElement.IsEmpty)
              continue;

            var grpID = (int)featureElement.Attribute("ID");
            var no = (int)featureElement.Attribute("No");

            var names = from n in featureElement.Element("FeatureGroup").Elements("Name")
                        let langID = (int)n.Attribute("langid")
                        where DataImport.Languages.ContainsKey(langID)
                        select new
                        {
                          Value = n.Attribute("Value").Value,
                          LangID = langID
                        };

            foreach (var name in names)
            {
              yield return new object[] { grpID, name.Value, no, name.LangID, null };
            }
          }
        }
      }
    }
  }
}
