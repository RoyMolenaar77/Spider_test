SELECT MT.TypeID
, MT.Type
, MT2.TypeID AS RSOTypeID
, MT2.Type AS RSOType 

FROM {0}.[dbo].[MediaType] MT
LEFT JOIN {1}.[dbo].[MediaType] MT2 ON MT.Type = MT2.Type