

  --Begin Action
	IF EXISTS ( SELECT * FROM sys.objects WHERE NAME = 'ToUniversal' )
		drop function [dbo].[udf_ConvertfromCETtoUTC]
		go
      Create FUNCTION [dbo].[ToUniversal] (@CETDate AS DATETIME)
      RETURNS DATETIME
      AS
      BEGIN

      DECLARE @DstStart datetime
      DECLARE @DstEnd datetime
      DECLARE @UtcDate datetime

      SELECT @DstStart = DATEADD(hour, 2,DATEADD(day, DATEDIFF(day, 0, '31/Mar' + CAST(YEAR(@CETDate) AS varchar)) - 
		      (DATEDIFF(day, 6, '31/Mar' + CAST(YEAR(@CETDate) AS varchar)) % 7), 0)),
	      @DstEnd = DATEADD(hour, 3,DATEADD(day, DATEDIFF(day, 0, '31/Oct' + CAST(YEAR(@CETDate) AS varchar)) - 
		      (DATEDIFF(day, 6, '31/Oct' + CAST(YEAR(@CETDate) AS varchar)) % 7), 0))

      SELECT @UtcDate = CASE WHEN @CETDate <= @DstEnd AND @CETDate >= @DstStart
	      THEN DATEADD(hour, -2, @CETDate)
	      ELSE DATEADD(hour, -1, @CETDate) END

      RETURN @UtcDate
	  END

