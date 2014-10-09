using System;
using System.Collections.Generic;
using System.Linq;
using Concentrator.Objects.DataAccess.Repository;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.Magento;
using Concentrator.Objects.Models.Products;
using Concentrator.Plugins.PFA.ConcentratorRepos;
using Concentrator.Plugins.PFA.ConcentratorRepos.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace Concentrator.Plugins.PFA.Tests
{
  [TestClass]
  public class GetTheLookTests
  {
    //private GetTheLookProcessor _processor;
    //private IRepository<ProductGroupMapping> _repoProductGroupMapping;
    //private IRepository<ContentProductGroup> _repoContentProductGroup;
    //private IRepository<Content> _repoContent;
    //private IRepository<MagentoProductGroupSetting> _repoMagento;
    //private IGetTheLookRepository _repoGetTheLook;
    //private Connector _connector;

    //[TestInitialize]
    //public void Setup()
    //{
    //  _repoProductGroupMapping = MockRepository.GenerateStub<IRepository<ProductGroupMapping>>();
    //  _repoContent = MockRepository.GenerateStub<IRepository<Content>>();
    //  _repoContentProductGroup = MockRepository.GenerateStub<IRepository<ContentProductGroup>>();
    //  _repoMagento = MockRepository.GenerateStub<IRepository<MagentoProductGroupSetting>>();
    //  _repoGetTheLook = MockRepository.GenerateStub<IGetTheLookRepository>();
    //  _connector = new Connector()
    //  {
    //    ConnectorID = 5
    //  };

    //  _processor = new GetTheLookProcessor(
    //      _repoContentProductGroup,
    //      _repoProductGroupMapping,
    //      _repoContent,
    //      _repoMagento,
    //      _repoGetTheLook,
    //      _connector,
    //      1, 1, 48, 1);
    //}

    //private void StubGetTheLookRepoWithNrMatches(int groupMatches, int productMatches, IGetTheLookRepository repo)
    //{
    //  List<LookGroup> list = new List<LookGroup>();

    //  for (int i = 0; i < groupMatches; i++)
    //  {
    //    list.Add(new LookGroup()
    //    {
    //      Products = Enumerable.Range(0, productMatches).ToList(),
    //      Season = string.Format("W{0}", i),
    //      InputCode = string.Format("InputCode{0}", i),
    //      TargetGroup = string.Format("TargetGroup{0}", i),
    //    });
    //  }

    //  _repoGetTheLook.Stub(c => c.GetMatchedLookGroups(1, 1, 1, 1)).Return(list);
    //}

    //private string FormatBackendLabel(int seasonPosition = 0, int inputCodePosition = 0, int targetGroupPosition = 0)
    //{
    //  return string.Format("InputCode{0} - W{1} - TargetGroup{2}", inputCodePosition, seasonPosition, targetGroupPosition);
    //}


    //[TestMethod]
    //public void Get_The_Look_Should_Create_A_ProductGroupMapping_When_One_Doesnt_Exist()
    //{
    //  StubGetTheLookRepoWithNrMatches(1, 1, _repoGetTheLook);

    //  var label = FormatBackendLabel();
    //  _repoProductGroupMapping.Stub(s => s.GetSingle(c => c.ProductGroupID == 1 && c.BackendMatchingLabel == label && c.ParentProductGroupMappingID == 1)).Return(null);

    //  _repoProductGroupMapping.Expect(e => e.Add(new ProductGroupMapping()
    //  {
    //    ProductGroupID = 1,
    //    ParentProductGroupMappingID = 1,
    //    BackendMatchingLabel = label,
    //    Score = 0,
    //    CustomProductGroupLabel = label
    //  }));

    //  _processor.Process();
      
    //  _repoProductGroupMapping.VerifyAllExpectations();      
    //}
  }
}
