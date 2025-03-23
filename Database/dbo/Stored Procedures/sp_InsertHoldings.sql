CREATE   PROCEDURE sp_InsertHoldings
    @HoldingSummaryId INT,
    @SchemeName NVARCHAR(100) NULL,
	@AMC NVARCHAR(100) NULL,
	@Category NVARCHAR(50) NULL,
	@SubCategory NVARCHAR(50) NULL,
    @FolioNo BIGINT NULL,
	@Source NVARCHAR(20) NULL,
	@Unit DECIMAL(18,2) NULL,
	@InvestedValue DECIMAL(18,2) NULL,
	@CurrentValue DECIMAL(18,2) NULL,
	@Returns DECIMAL(18,2) NULL,
	@XIRR DECIMAL(18,2) NULL
	AS
	BEGIN

	INSERT INTO Holdings (HoldingSummaryId,SchemeName, AMC, Category, SubCategory, 
	FolioNo, Source, Unit, InvestedValue, CurrentValue, Returns,XIRR)
    VALUES (@HoldingSummaryId,@SchemeName, @AMC, @Category, @SubCategory, 
	@FolioNo, @Source, @Unit, @InvestedValue, @CurrentValue, @Returns,@XIRR);

	END