using Concentrator.Plugins.Axapta.Enum;
using Concentrator.Plugins.Axapta.Helpers;
using Concentrator.Plugins.Axapta.Repositories;
using Concentrator.Plugins.Axapta.Models;
using Concentrator.Objects.Ftp;
using Excel;
using System.Data;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using FileHelpers;

namespace Concentrator.Plugins.Axapta.Services
{
  public class CustomerInformationService : ICustomerInformationService
  {
    private readonly FtpSetting _ftpSetting = new FtpSetting();
    private readonly List<DatColCustomerInformation> _listOfCorruptCustomerInformation = new List<DatColCustomerInformation>();

    private const int SapphVendorID = 50;

    private const string FtpAddressSettingKey = "FtpAddress";
    private const string FtpUsernameSettingKey = "FtpUsername";
    private const string FtpPasswordSettingKey = "FtpPassword";

    private const string FtpUriCustomerInformationSettingKey = "TNTDestinationURI";
    private const string CustomerInformationDirectorySettingKey = "Ax Ftp Dir CustomerInformation";
    private const string CorruptCustomerInformationDirectorySettingKey = "Ax Ftp Dir Corrupt CustomerInformation";

    private string CustomerInformationFileName
    {
      get
      {
        return String.Format("CustomerInformation_{0:yyyy-MM-dd_Hmmss}.xml", DateTime.Now);
      }
    }
    private string CorruptCustomerInformationFileName
    {
      get
      {
        return String.Format("Failed_CustomerInformation_{0:yyyy-MM-dd_Hmmss}.csv", DateTime.Now);
      }
    }

    const int ColumnCustomerNumber = 0;
    const int ColumnSupplierName = 5;
    const int ColumnSupplierAdress = 7;
    const int ColumnExpeditionCity = 8;
    const int ColumnExpeditionZip = 9;
    const int ColumnExpeditionCountry = 10;
    //const int ColumnInventLocation = 1;
    //const int ColumnLanguageID = 2;
    //const int ColumnRecID = 3;
    //const int ColumnKind = 4;
    //const int ColumnAdressTotal = 6;
    //const int ColumnBlocked = 11;
    //const int ColumnILN = 12;

    protected virtual Regex FileNameRegex
    {
      get
      {
        return new Regex(".*\\.xlsx$", RegexOptions.IgnoreCase);
      }
    }

    private readonly ILogger _log;
    private readonly IArchiveService _archiveService;
    private readonly IVendorSettingRepository _vendorSettingRepo;
    public CustomerInformationService(ILogger log, IVendorSettingRepository vendorSettingRepo, IArchiveService archiveService)
    {
      _log = log;
      _archiveService = archiveService;
      _vendorSettingRepo = vendorSettingRepo;
    }

    public void Process()
    {
      FillSettings();
      ReadCustomerInformation();
    }

    private void FillSettings()
    {
      _ftpSetting.FtpAddress = _vendorSettingRepo.GetVendorSetting(SapphVendorID, FtpAddressSettingKey);
      _ftpSetting.FtpUsername = _vendorSettingRepo.GetVendorSetting(SapphVendorID, FtpUsernameSettingKey);
      _ftpSetting.FtpPassword = _vendorSettingRepo.GetVendorSetting(SapphVendorID, FtpPasswordSettingKey);
    }
    private void ReadCustomerInformation()
    {
      _ftpSetting.Path = _vendorSettingRepo.GetVendorSetting(SapphVendorID, CustomerInformationDirectorySettingKey);
      var ftpManager = new FtpManager(_ftpSetting.FtpUri, null, false, usePassive: true);

      var regex = FileNameRegex;
      foreach (var fileName in ftpManager.GetFiles())
      {
        if (!regex.IsMatch(fileName)) continue;
        try
        {
          _log.Info(string.Format("Processing file: {0}", fileName));

          using (var stream = ftpManager.Download(Path.GetFileName(fileName)))
          {
            using (var excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream))
            {
              excelReader.IsFirstRowAsColumnNames = true;
              var result = excelReader.AsDataSet();
              var dt = result.Tables[0];

              var customerInformationList = (
                from ci in dt.AsEnumerable()
                select new DatColCustomerInformation
                {
                  CustomerNumber = ci.Field<string>(ColumnCustomerNumber),
                  SupplierName = ci.Field<string>(ColumnSupplierName),
                  SupplierAdress = ci.Field<string>(ColumnSupplierAdress),
                  ExpeditionCity = ci.Field<string>(ColumnExpeditionCity),
                  ExpeditionPc = ci.Field<string>(ColumnExpeditionZip),
                  ExpeditionCountry = ci.Field<string>(ColumnExpeditionCountry)
                })
                .ToList();
              UploadCustomerInformationToTnt(customerInformationList);
            }
          }

          _archiveService.CopyToArchive(ftpManager.BaseUri.AbsoluteUri, SaveTo.CustomerInformationDirectory, fileName);
          ftpManager.Delete(Path.GetFileName(fileName));
        }
        catch (Exception e)
        {
          _log.Info(string.Format("Failed to process file {0}. Error: {1}", fileName, e.Message));
        }
      }
    }
    private void UploadCustomerInformationToTnt(IEnumerable<DatColCustomerInformation> customerInformation)
    {
      var listOfCorruptCustomerInformation = new List<DatColCustomerInformation>();

      var element = new XElement("CustomerInformation");
      foreach (var customer in customerInformation)
      {
        if (customer.IsValidCustomer)
        {
          element.Add(new XElement("Customer",
                                   new XElement("ID", customer.CustomerNumber),
                                   new XElement("Name", customer.SupplierName),
                                   new XElement("Address", customer.SupplierAdress),
                                   new XElement("City", customer.ExpeditionCity),
                                   new XElement("Zip", customer.ExpeditionPc),
                                   new XElement("Country", customer.ExpeditionCountry),
                                   new XElement("Telephone", string.Empty),
                                   new XElement("Currency", string.Empty),
                                   new XElement("BTWNumber", string.Empty)
                        ));
          continue;
        }
        listOfCorruptCustomerInformation.Add(customer);
      }

      using (Stream xmlStream = new MemoryStream())
      {
        element.Save(xmlStream);

        xmlStream.Flush();
        xmlStream.Position = 0;

        var ftpManager = new FtpManager(_vendorSettingRepo.GetVendorSetting(SapphVendorID, FtpUriCustomerInformationSettingKey), null, false, true);

        ftpManager.Upload(xmlStream, CustomerInformationFileName);
      }

      _archiveService.ExportToAxapta(listOfCorruptCustomerInformation, SaveTo.CorruptCustomerInformationDirectory, CorruptCustomerInformationFileName);
    }
  }
}
