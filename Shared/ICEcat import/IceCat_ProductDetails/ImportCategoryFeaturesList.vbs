Set objBL = CreateObject("SQLXMLBulkLoad.SQLXMLBulkLoad")
objBL.ConnectionString = "provider=SQLOLEDB.10;data source=A0124;database=ICECAT;uid=sa;pwd=%kg77hB"
objBL.ErrorLogFile = "C:\userfolder\Importxml\error.log"
objBL.Execute "C:\userfolder\ICECAT\IceCat_ProductDetails\CategoryFeaturesListmap.xml", "C:\userfolder\Importxml\CategoryFeaturesList.xml"
Set objBL = Nothing