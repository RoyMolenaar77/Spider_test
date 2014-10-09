using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Concentrator.Plugins.Magento.Models;
using MySql.Data.MySqlClient;
using AutoMapper;

namespace Concentrator.Plugins.Magento.Helpers
{
  public class StockHelper : MagentoMySqlHelper
  {

    public StockHelper(string connectionString)
      : base(connectionString)
    {
      AutoMapper.Mapper.CreateMap<IDataReader, basstock>();
      AutoMapper.Mapper.CreateMap<IDataRecord, basstock>();

      AutoMapper.Mapper.CreateMap<IDataReader, basstore>();
      AutoMapper.Mapper.CreateMap<IDataRecord, basstore>();

      _tablesNames.Add("basstock", TableName("basstock"));
      _tablesNames.Add("basstore", TableName("basstore"));

    }


  }
}
