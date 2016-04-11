USE ltprdinvdw;
GO

DECLARE @SnapshotDate DATETIME = 
	CAST(CONVERT(NVARCHAR(8), DATEADD(dd, -1, GETDATE()), 112) AS DATETIME);

/* ------------------------------------------------------------------
Products in Inventory master query
------------------------------------------------------------------ */

SELECT		DISTINCT HBP.productkey AS InventoryDWProductKey
			, HBP.producttype AS ProductType
			, HBP.product AS Product
			, HBP.[description] AS ProductDescription
			, HBP.hbtexture AS HBProductTexture
			, HBP.hbthickness AS HBProductThickness
			, HBP.hblength AS HBProductLength
			, HBP.hbwidth AS HBProductWidth -- actual width on individual inventory piece, i.e. not WidthLabel on product
			, HBP.hbpcspkg HBProductPiecesPerPackage
			, HBP.hbgrade AS HBProductGrade
			, HBP.hbtype AS HBProductType
			, HBP.hbstyle AS HBProductStyle
			, HBP.hbcertified AS HBProductCertified

FROM		ltprdinvdw.dbo.hb_product HBP; -- hb_product table = hardboard type products only

