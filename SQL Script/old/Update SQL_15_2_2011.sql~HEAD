

/****** Object:  StoredProcedure [dbo].[sp_SearchDynamic]    Script Date: 02/16/2011 16:04:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Name
-- Create date: 
-- Description:	
-- =============================================
ALTER PROCEDURE [dbo].[sp_SearchDynamic] 
	-- Add the parameters for the stored procedure here
	@includeProductDescriptions bit = 0, 
	@includeProductIdentifiers bit = 0,
	@includeBrands bit = 0,
	@includeProductGroups bit = 0,
	@Query nvarchar(200),
	@LanguageID int = 1
	
	
	
	
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	Declare @results table(
		ProductID int		
	)

--		(SELECT top 1 MediaPath FROM ProductMedia where TypeID = 1 and productid = p.productid) as MediaPath  
	
	
	IF @includeBrands = 1
	BEGIN
	insert into @results
		select 
			p.ProductID
		
		from Product p
		inner join Brand b on p.BrandID = b.BrandID
		where b.Name like '%'+@Query+'%'
		
	
	END

	IF @includeProductDescriptions = 1
	BEGIN 
	
		insert into @results
		
		select p.ProductID
		
		from Product p
		inner join ProductDescription pd on p.ProductID = pd.ProductID

		where pd.ShortContentDescription like '%'+@Query+'%' or pd.ProductName like '%'+@Query+'%' 	
		
		insert into @results
		
		select p.ProductID
		
		from Product p
		inner join VendorAssortment pd on p.ProductID = pd.ProductID
		where pd.ShortDescription like '%'+@Query+'%' 
	END
	
	IF @includeProductDescriptions = 1
	BEGIN 
	
		insert into @results
		
		select p.ProductID
		
		from Product p
		inner join VendorAssortment pd on p.ProductID = pd.ProductID
		where pd.ShortDescription like '%'+@Query+'%' 
	
	END
	
	IF @includeProductIdentifiers = 1
	BEGIN 
	
		insert into @results
		
		select p.ProductID
		
		from Product p
		inner join VendorAssortment pd on p.ProductID = pd.ProductID
		where pd.CustomItemNumber = @Query or cast(p.ProductID as nvarchar) = @Query or p.VendorItemNumber like '%'+@Query+'%' 
	
	END
	
	
	IF @includeProductGroups = 1
	BEGIN 
		insert into @results
		
		select p.ProductID
		
		from Product p
		inner join ContentProductGroup cpg on p.ProductID = cpg.ProductID
		inner join ProductGroupMapping pgm on cpg.ProductGroupMappingID = pgm.ProductGroupMappingID
		inner join ProductGroupLanguage pgl on pgl.ProductGroupID = pgm.ProductGroupID
		where pgl.Name like '%'+@Query+'%'
		
		--where pd.CustomItemNumber = @Query or p.ProductID = @Query or p.VendorItemNumber like '%'+@Query+'%' 
	END
	
    -- Insert statements for procedure here
		select distinct
	p.ProductID,
	(SELECT top 1 MediaPath FROM ProductMedia where TypeID = 1 and productid = p.productid) as MediaPath , 
	 case when pd.ProductName is not null then pd.ProductName
	 else case when va.ShortDescription is not null then va.ShortDescription 
	 else case when pd.ShortContentDescription is not null then pd.ShortContentDescription
	 else case when prod.VendorItemNumber is not null then prod.VendorItemNumber
	 else cast(p.ProductID as nvarchar) end end end end as ProductName
	 from @results p
	 left join VendorAssortment va on va.ProductID = p.ProductID 
	 left join ProductDescription pd on pd.ProductID = p.ProductID and pd.LanguageID = @LanguageID
		left join Product prod on prod.ProductID = p.ProductID
		 
END
