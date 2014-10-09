﻿--merge productdescription pd
--using 
--(
--	select pColor.productid as ColorProductID, pd.* from productdescription pd
--	inner join product p on p.productid = pd.productid
--	inner join product pColor on pColor.parentproductid = p.productid
--	where pd.vendorid = 48 and p.sourcevendorid in (1,13,14) and (p.isconfigurable = 1 and p.parentproductid is null)
--) src
--on src.ColorProductID = pd.productid and src.vendorid = pd.vendorid and pd.languageid = src.languageid
--when not matched by target 
--then 
--	insert (productid, languageid, vendorid, shortcontentdescription, longcontentdescription, shortsummarydescription, longsummarydescription, createdby)
--	values (src.ColorProductID, src.languageid, src.vendorid, src.shortcontentdescription, src.longcontentdescription, src.shortsummarydescription, src.longsummarydescription, src.createdby);