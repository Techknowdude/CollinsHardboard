SELECT		DISTINCT 
			                                CAL.FullDate AS SalesOrderHeaderDate
			                                , S.DueDate AS DueDate
			                                , S.SalesOrder AS SalesOrderNumber
			                                , S.Invoice AS InvoiceNumber
			                                , (M.Pieces/S.Attribute2) AS Units
											, (M.Pieces - ((M.Pieces/S.Attribute2)*S.Attribute2)) AS Pieces
											, M.Pieces AS TotalPieces
			                                , P.Product AS ProductCode
			                                , P.Description
			                                , P.Descriptor1 AS Texture
			                                , P.Descriptor2 AS Thickness
			                                , P.Descriptor3 AS Grade
			                                , P.Descriptor4 AS Type
			                                , P.Descriptor5 AS Style
			                                , P.Descriptor6 AS WidthLabel
			                                , S.Attribute1 AS Width
			                                , S.Attribute2 AS PiecesPerPackage
			                                , S.Attribute3 AS Length
			                                , S.Attribute4 AS Certified
											, S.Shipment AS Ship


                                FROM        ltproddw.dbo.Measure M
                                INNER JOIN  ltproddw.dbo.Calendar CAL
                                ON M.CalendarKey = CAL.calendarkey
                                INNER JOIN  ltproddw.dbo.Sale S
                                ON M.SaleKey = S.SaleKey
                                INNER JOIN  ltproddw.dbo.Product P
                                ON M.ProductKey = P.ProductKey

                                WHERE P.ProductType = N'HB'-- HB = Hardboard
                                AND CAL.FullDate BETWEEN CAST(CONVERT(NVARCHAR(8), DATEADD(dd, -4, GETDATE()), 112) AS DATETIME) AND CAST(CONVERT(NVARCHAR(8), DATEADD(dd, 1, GETDATE()), 112) AS DATETIME)
                                ORDER BY S.Invoice, P.Product;
