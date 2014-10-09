Set objBL = CreateObject("SQLXMLBulkLoad.SQLXMLBulkLoad")
objBL.ConnectionString = "provider=SQLOLEDB.10;data source=A0124;database=ICECAT;uid=sa;pwd=%kg77hB"
objBL.ErrorLogFile = "C:\userfolder\Importxml\error.log"
objBL.Execute "C:\userfolder\ICECAT\IceCat_ProductDetails\AlphaAssortmentImportMap.xml", "C:\userfolder\Importxml\Bas_Group_KERN_Prijslijst_Alpha.xml"
Set objBL = Nothing