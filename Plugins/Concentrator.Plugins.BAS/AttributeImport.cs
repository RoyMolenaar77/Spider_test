using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.ConcentratorService;
using System.Data.SqlClient;
using System.Configuration;
using Concentrator.Objects.Models.Attributes;

namespace Concentrator.Plugins.BAS
{
  class AttributeImport : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "BAS insurance attribute import"; }
    }

    protected override void Process()
    {
      using (var unit = GetUnitOfWork())
      {
        var productattributeGroup = unit.Scope.Repository<ProductAttributeGroupMetaData>().GetSingle(x => x.VendorID == 1 && x.GroupCode == "Insurance");

        if(productattributeGroup == null){
          productattributeGroup = new ProductAttributeGroupMetaData(){
            Index = 0,
            GroupCode = "Insurance",
            VendorID = 1
          };

          unit.Scope.Repository<ProductAttributeGroupMetaData>().Add(productattributeGroup);
          
          var productGroupName = new ProductAttributeGroupName(){
            ProductAttributeGroupMetaData = productattributeGroup,
            LanguageID = 1,
            Name = "Insurance"
          };
          unit.Scope.Repository<ProductAttributeGroupName>().Add(productGroupName);

          var productGroupNameNL = new ProductAttributeGroupName(){
            ProductAttributeGroupMetaData = productattributeGroup,
            LanguageID = 2,
            Name = "Verzekeringen"
          };
          unit.Scope.Repository<ProductAttributeGroupName>().Add(productGroupNameNL);
          
          unit.Save();
        }

        var productattribute = unit.Scope.Repository<ProductAttributeMetaData>().GetSingle(x => x.VendorID == 1 && x.AttributeCode == "Risk");

        if(productattribute == null){
          productattribute = new ProductAttributeMetaData(){
            ProductAttributeGroupID = productattributeGroup.ProductAttributeGroupID,
            VendorID = 1,
            IsVisible = false,
            NeedsUpdate = true,
            IsSearchable = false,
            CreatedBy = 1,
            Mandatory = false,
            AttributeCode = "Risk"
          };
          unit.Scope.Repository<ProductAttributeMetaData>().Add(productattribute);

          var productAttributeName = new ProductAttributeName(){
            AttributeID = productattribute.AttributeID,
            LanguageID = 1,
            Name = "Risk"
          };
          unit.Scope.Repository<ProductAttributeName>().Add(productAttributeName);

          var productAttributeNameNL = new ProductAttributeName(){
            AttributeID = productattribute.AttributeID,
            LanguageID = 2,
            Name = "Risico"
          };
          unit.Scope.Repository<ProductAttributeName>().Add(productAttributeNameNL);

          unit.Save();
        }

        var insurances = GetInsurances(productattribute.AttributeID);

        using (var BASAttributeBulkImport = new BASAttributeBulkImport(insurances))
        {
          BASAttributeBulkImport.Init(unit.Context);
          BASAttributeBulkImport.Sync(unit.Context);
        }
      }
    }

    private List<ConnectFlowAdditionalInformation> GetInsurances(int attributeID)
    {
      List<ConnectFlowAdditionalInformation> adiList = new List<ConnectFlowAdditionalInformation>();

      using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["Xtract"].ConnectionString))
      {
        connection.Open();

        using (SqlCommand command = new SqlCommand(@"select 
imitm,
imprp8
from f4101
where imprp8 != ''", connection))
        {
          using (var reader = command.ExecuteReader())
          {
            while (reader.Read())
            {

              int jdeItem = int.Parse(reader["imitm"].ToString());

              int ins = 0;
              int.TryParse(reader["imprp8"].ToString(), out ins);

              var item = new ConnectFlowAdditionalInformation()
              {
                CustomItemNumber = jdeItem,
                Risk = ins,
                AttributeID = attributeID
              };
              adiList.Add(item);
            }
          }
        }
      }

      return adiList;
    }
  }
}
