if not exists(select * from connectorsetting where connectorid = 13 and settingkey = 'DatcolShopNumber')
  begin 
    insert into connectorsetting values (13, 'DatcolShopNumber', '892')
    end

    if not exists(select * from connectorsetting where connectorid = 14 and settingkey = 'DatcolShopNumber')
  begin 
    insert into connectorsetting values (14, 'DatcolShopNumber', '803')
    end