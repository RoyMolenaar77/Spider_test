using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace System.Data
{
  using SqlClient;

  public static class DataTableExtensions
  {
    /// <summary>
    /// Returns the specified data table as a table-valued parameter.
    /// </summary>
    public static SqlParameter AsParameter(this DataTable dataTable, String typeName, String parameterName = null)
    {
      return new SqlParameter
      {
        ParameterName = parameterName,
        SqlDbType     = SqlDbType.Structured,
        TypeName      = typeName,
        Value         = dataTable
      };
    }

    ///// <summary>
    ///// Splits the data table up in one or more data tables based on the specified indices. The last table will contain the remainder of the data rows.
    ///// </summary>
    //public static IEnumerable<DataTable> Split(this DataTable dataTable, params Int32[] indices)
    //{
    //  return Split(dataTable, indices);
    //}

    ///// <summary>
    ///// Splits the data table up in one or more data tables based on the specified indices.
    ///// The last table to be returned will contain the remainder of the data rows if there are any.
    ///// </summary>
    //public static IEnumerable<DataTable> Split(this DataTable dataTable, IEnumerable<Int32> indices)
    //{
    //  if (dataTable == null)
    //  {
    //    throw new NullReferenceException("dataTable");
    //  }

    //  if (dataTable.Rows.Count > 0)
    //  {
    //    var indexList = new LinkedList<Int32>(indices
    //      .OrderBy(index => index)
    //      .Where(index => index > -1 && index <= dataTable.Rows.Count)
    //      .Distinct());

    //    if (indexList.First == null || indexList.First.Value != 0)
    //    {
    //      indexList.AddFirst(0);
    //    }

    //    if (indexList.Last.Value < dataTable.Rows.Count)
    //    {
    //      indexList.AddLast(dataTable.Rows.Count);
    //    }

    //    for (var currentNode = indexList.First; currentNode.Next != null; currentNode = currentNode.Next)
    //    {
    //      var newDataTable = dataTable.Clone();

    //      foreach (var rowIndex in Enumerable.Range(currentNode.Value, currentNode.Next.Value - currentNode.Value))
    //      {
    //        newDataTable.Rows.Add(dataTable.Rows[rowIndex].ItemArray);
    //      }

    //      yield return newDataTable;
    //    }
    //  }
    //}

    // //<summary>
    // //Reads CSV data into the <see cref="DataTable"/> from the specified stream. 
    // //</summary>
    // //<param name="fieldConverter">
    // //Optional: Provide a function that converts the string-value to the type of specific data colomn.
    // //</param>
    //[DebuggerStepThrough]
    //public static void ReadCsv(this DataTable dataTable, Stream stream, Func<DataColumn, String, Object> fieldConverter, Char separator = ';', Boolean isFirstLineHeader = false)
    //{
    //  ReadCsv(dataTable, stream, separator, isFirstLineHeader, fieldConverter);
    //}

    // //<summary>
    // //Reads CSV data into the <see cref="DataTable"/> from the specified stream. 
    // //</summary>
    //[DebuggerStepThrough]
    //public static void ReadCsv(this DataTable dataTable, Stream stream, Char separator = ';', Boolean isFirstLineHeader = false, Func<DataColumn, String, Object> fieldConverter = null)
    //{
    //  dataTable.ThrowIfNull("dataTable");
    //  stream.ThrowArgNull("stream");
      
    //  using (var memoryStream = new MemoryStream())
    //  {
    //    stream.CopyTo(memoryStream);

    //    using (var csvReader = new CsvReader(memoryStream.Reset(), separator))
    //    {
    //      for (Int32 index = 0; csvReader.Read(); index++)
    //      {
    //        if (index == 0)
    //        {
    //          if (dataTable.Columns.Count == 0)
    //          {
    //            foreach (var header in csvReader)
    //            {
    //              dataTable.Columns.Add(new DataColumn(isFirstLineHeader ? header : null));
    //            }
    //          }

    //          if (isFirstLineHeader)
    //          {
    //            continue;
    //          }
    //        }

    //        if (csvReader.CurrentFields.Length != dataTable.Columns.Count)
    //        {
    //          throw new DataException(String.Format("Data mismatch; The DataTable instance was initialized with {0} columns, but '{1}' contains {2} values."
    //            , dataTable.Columns.Count
    //            , csvReader.CurrentLine
    //            , csvReader.CurrentFields.Length));
    //        }

    //        if (fieldConverter != null)
    //        {
    //          dataTable.LoadDataRow(Enumerable
    //            .Range(0, dataTable.Columns.Count)
    //            .Select(i => fieldConverter(dataTable.Columns[i], csvReader.CurrentFields[i]))
    //            .ToArray(), true);
    //        }
    //        else
    //        {
    //          dataTable.LoadDataRow(csvReader.CurrentFields, true);
    //        }
    //      }
    //    }
    //  }
    //}

    // //<summary>
    // //Reads CSV data into the <see cref="DataTable"/> from the specified file. 
    // //</summary>
    //[DebuggerStepThrough]
    //public static void ReadCsv(this DataTable dataTable, String filename, Char separator = ';', Boolean isFirstLineHeader = false)
    //{
    //  using (var fileStream = File.OpenRead(filename))
    //  {
    //    ReadCsv(dataTable, fileStream, separator, isFirstLineHeader);
    //  }
    //}

    // //<summary>
    // //Write CSV data from the <see cref="DataTable"/> from the specified stream. 
    // //</summary>
    //[DebuggerStepThrough]
    //public static void WriteCsv(this DataTable dataTable, Stream stream, Char separator = ';', Boolean includeHeader = false)
    //{
    //  dataTable.ThrowIfNull("dataTable");
    //  stream.ThrowArgNull("stream");
    
    //  using (var streamWriter = new StreamWriter(stream, System.Text.Encoding.UTF8))
    //  {
    //    if (includeHeader)
    //    {
    //      var headers = String.Join(separator.ToString(), dataTable.Columns.Cast<DataColumn>().Select(x => x.Caption ?? x.ColumnName).ToArray<String>());
    
    //      streamWriter.WriteLine(headers);
    //    }
    
    //    foreach (var dataRow in dataTable.Rows.Cast<DataRow>())
    //    {
    //      var values = String.Join(separator.ToString(), dataRow.ItemArray.Select(x => x.ToString()));
    
    //      streamWriter.WriteLine(values);
    //    }
    
    //    streamWriter.Flush();
    //  }
    //}

    // //<summary>
    // //Write CSV data from the <see cref="DataTable"/> from the specified file. 
    // //</summary>
    //[DebuggerStepThrough]
    //public static void WriteCsv(this DataTable dataTable, String filename, Char separator = ';', Boolean includeHeader = false)
    //{
    //  using (var fileStream = File.OpenWrite(filename))
    //  {
    //    WriteCsv(dataTable, fileStream, separator, includeHeader);
    //  }
    //}
  }
}
