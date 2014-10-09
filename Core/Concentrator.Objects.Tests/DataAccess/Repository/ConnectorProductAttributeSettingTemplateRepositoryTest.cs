using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;

using Xunit;

namespace Concentrator.Objects.DataAccess.Repository.Tests
{
  using Models;

  public class ConnectorProductAttributeSettingTemplateRepositoryTest : RepositoryBaseTest
  {
    private const string DisplayName = "Connector Product Attribute Setting Template Repository: ";

    [Fact(DisplayName = DisplayName + "Initialization with valid connection parameter")]
    public void InitializeWithValidConnection()
    {
      new ConnectorProductAttributeSettingTemplateRepository(Connection);
    }

    [Fact(DisplayName = DisplayName + "Initialization with invalid connection parameter")]
    public void InitializeWithInvalidConnection()
    {
      Assert.Throws<ArgumentNullException>(delegate
      {
        new ConnectorProductAttributeSettingTemplateRepository(null);
      });
    }

    private const String ConnectorSystemName = "Default";
    private const String ConnectorSystemTableName = "ConnectorSystem";

    private Int32 GetConnectorSystemID()
    {
      using (var command = CreateCommand("SELECT [ConnectorSystemID] FROM {0} WHERE [Name] = '{1}'", ConnectorSystemTableName, ConnectorSystemName))
      {
        var commandSystemID = (Int32?)command.ExecuteScalar();

        Assert.Equal(commandSystemID.HasValue, true);

        return commandSystemID.GetValueOrDefault();
      }
    }

    private const String ConnectorProductAttributeSettingTemplateTableName = "[dbo].[ConnectorProductAttributeSettingTemplate]";

    [Fact(DisplayName = DisplayName + "Get single")]
    public void GetSingle()
    {
      var repository = new ConnectorProductAttributeSettingTemplateRepository(Connection);

      var connectorSystemID = GetConnectorSystemID();
      var settingTemplate = default(ConnectorProductAttributeSettingTemplate);
      var settingTemplateCode = "UNIT_TEST_GET_SINGLE";
      var settingTemplateType = typeof(Guid).FullName;
      var settingTemplateValue = Guid.NewGuid().ToString();

      ExecuteCommand("INSERT INTO {0} ([ConnectorSystemID], [Code], [Type], [Value]) VALUES ({1}, '{2}', '{3}', '{4}')"
        , ConnectorProductAttributeSettingTemplateTableName
        , connectorSystemID
        , settingTemplateCode
        , settingTemplateType
        , settingTemplateValue);

      try
      {
        settingTemplate = repository.GetSingle(connectorSystemID, settingTemplateCode);

        Assert.NotNull(settingTemplate);
        Assert.Equal(settingTemplate.ConnectorSystemID, connectorSystemID);
        Assert.Equal(settingTemplate.Code, settingTemplateCode);
        Assert.Equal(settingTemplate.Type, settingTemplateType);
        Assert.Equal(settingTemplate.Value, settingTemplateValue);
      }
      finally
      {
        ExecuteCommand("DELETE FROM {0} WHERE [ConnectorSystemID] = {1} AND [Code] = '{2}'"
          , ConnectorProductAttributeSettingTemplateTableName
          , connectorSystemID
          , settingTemplateCode);
      }

      settingTemplate = repository.GetSingle(connectorSystemID, settingTemplateCode);

      Assert.Null(settingTemplate);
    }
  }
}
