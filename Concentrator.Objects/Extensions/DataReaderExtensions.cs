using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Globalization;

using Dapper;

namespace System.Data
{
  public static class DataReaderExtensions
  {
    public static TResult Get<TResult>(this IDataReader dataReader, Int32 index, CultureInfo cultureInfo = null)
    {
      var value = dataReader.GetValue(index);

      return value != DBNull.Value
        ? TypeConverterService.ConvertFrom<TResult>(value, cultureInfo)
        : TypeConverterService.ConvertFrom<TResult>(null, cultureInfo);
    }

    public static IEnumerable<TResult> Map<TResult>(this IDataReader dataReader)
    {
      var mappingFunction = SqlMapper.GetTypeDeserializer(typeof(TResult), dataReader);

      while (dataReader.Read())
      {
        yield return (TResult)mappingFunction(dataReader);
      }
    }
  }
}
