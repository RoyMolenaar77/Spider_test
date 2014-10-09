MERGE [dbo].[Brand] AS T
USING #Brand AS S
ON    T.Name = S.Name
WHEN MATCHED AND S.Name COLLATE SQL_Latin1_General_CP1_CS_AS <> T.Name THEN
  UPDATE SET Name = S.Name
WHEN NOT MATCHED THEN
  INSERT (Name)
  VALUES (Name);