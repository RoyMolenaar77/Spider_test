IF EXISTS(SELECT * FROM sys.objects WHERE type = 'P' AND name = 'GetUserPlugins')
	DROP PROCEDURE GetUserPlugins
GO
-- =============================================
-- Author:		Stan Todorov
-- Create date: DateTime.Now :P
-- Description:	Retrieves the plugins to which is registerred for notification
-- =============================================
CREATE PROCEDURE GetUserPlugins
	-- Add the parameters for the stored procedure here
	@UserID int = 0,
	@Start int = 0,
	@Take int = 50, 
	@PluginFilter nvarchar(1000) = '',
	@TypeFilter nvarchar(1000) = ''

AS
BEGIN
		select
		
				ROW_NUMBER() over (order by up.PluginID) as RowNum,
				up.UserID,
				up.PluginID, 
				p.PluginName,
				et.TypeID,
				et.[Type]
		into #TempUserPlugin
		From UserPlugin up
		inner join Plugin p on p.PluginID = up.pluginid
		inner join EventType et on et.TypeID = up.TypeID
		Where up.UserID = @UserID
		and 
			(isnull(@PluginFilter, '') = '' or (p.PluginName like '%' + @PluginFilter + '%')) and
			(isnull(@TypeFilter, '') = '' or (et.[Type] like '%' + @TypeFilter + '%'))


	
	select * from #TempUserPlugin where RowNum > 0 and RowNum <= 50


	select count(*) from #TempUserPlugin

END

GO

