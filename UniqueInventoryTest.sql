USE ltprdinvdw;
GO

DECLARE @SnapshotDate DATETIME = 
	CAST(CONVERT(NVARCHAR(8), DATEADD(dd, -1, GETDATE()), 112) AS DATETIME);

/* ------------------------------------------------------------------
Inventory information query
------------------------------------------------------------------ */

SELECT		Count(1) as Number
			,IR.[PROGRESS_RECID] AS InventoryDWInventoryRecordID
			, P.productkey AS InventoryDWProductKey
			, C.calendardate AS InventorySnapshotDate
			, UPPER(L.branch) AS Branch
			, L.location AS Location
			, P.producttype AS ProductType
			, P.product AS Product
			, IR.onhandvolume AS OnHandVolume
			, IR.onhandpieces AS OnHandPieces
			, HBP.hbgrade AS Grade
			, HBP.hbthickness AS Thickness
			, HBP.hbtexture AS Texture
			, HBP.hblength AS Length
			, HBP.hbwidth AS Width
			, HBP.hbpcspkg AS PcsPerUnit


FROM		ltprdinvdw.dbo.inventory_record IR -- main fact table within Inventory DW
INNER JOIN	ltprdinvdw.dbo.calendar C
ON			IR.calendarkey = C.calendarkey
INNER JOIN	ltprdinvdw.dbo.product P
ON			IR.productkey = P.productkey
INNER JOIN	ltprdinvdw.dbo.hb_product HBP -- hb_product table = hardboard type products only
ON			P.productkey = HBP.productkey
INNER JOIN	ltprdinvdw.dbo.location L
ON			IR.locationkey = L.locationkey

WHERE		C.calendardate = @SnapshotDate AND location = 'HB'

Group by IR.PROGRESS_RECID, P.productkey, calendardate, L.branch, L.location, P.producttype
,P.product,hbgrade,onhandvolume,onhandpieces,hbthickness,hbtexture,hblength,hbwidth,hbpcspkg
HAVING Count(1) > 1