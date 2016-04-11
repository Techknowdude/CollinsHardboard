SELECT		DISTINCT IR.[PROGRESS_RECID] AS InventoryDWInventoryRecordID
			                                    , P.productkey AS InventoryDWProductKey
			                                    , C.calendardate AS InventorySnapshotDate
			                                    , UPPER(L.branch) AS Branch
			                                    , L.location AS Location
			                                    , P.producttype AS ProductType
			                                    , P.product AS Product
			                                    , IR.onhandpieces/HBP.hbpcspkg AS Units
			                                    , IR.onhandpieces - (IR.onhandpieces/HBP.hbpcspkg)*HBP.hbpcspkg AS Pieces
												, IR.onhandpieces AS TotalPieces
			                                    , HBP.hbpcspkg AS PcsPerPackage
			                                    , HBP.hbgrade AS Grade
			                                    , HBP.hbtype AS Type
			                                    , HBP.hbcertified AS Certified
			                                    , HBP.hbthickness AS Thickness
			                                    , HBP.hbtexture AS Texture
			                                    , HBP.hbstyle AS Style
			                                    , HBP.hblength AS Length
			                                    , HBP.hbwidth AS Width


                                    FROM		ltprdinvdw.dbo.inventory_record IR -- main fact table within Inventory DW
                                    INNER JOIN	ltprdinvdw.dbo.calendar C
                                    ON			IR.calendarkey = C.calendarkey
                                    INNER JOIN	ltprdinvdw.dbo.product P
                                    ON			IR.productkey = P.productkey
                                    INNER JOIN	ltprdinvdw.dbo.hb_product HBP -- hb_product table = hardboard type products only
                                    ON			P.productkey = HBP.productkey
                                    INNER JOIN	ltprdinvdw.dbo.location L
                                    ON			IR.locationkey = L.locationkey

                                    WHERE		C.calendardate = CAST(CONVERT(NVARCHAR(8), DATEADD(dd, -1, GETDATE()), 112) AS DATETIME)
                                                AND location = 'HB';