Imports RMA.OpenNURBS
Imports InsoleDesigner.bll.IdAlias
Imports InsoleDesigner.bll.IdLanguageManager
Imports InsoleDesigner.bll.IdElement3dManager
Imports InsoleDesigner.bll.IdGeometryUtils
Imports InsoleDesigner.bll.AbstractCutoutCommons
Imports RhinoUtils
Imports RhinoUtils.RhDocument
Imports RhinoUtils.RhGeometry
Imports RMA.Rhino


Public Class IdCutoutPartialAddiction
    Inherits IdCutoutToTalAddiction
    Implements IPartialCutoutAddiction


#Region " Constant "

    Public Const OFFSET_PARTIAL_CUTOUT As Double = 7.0
    Public Const POLAR_CURVE_PER_PIGRECO As Integer = 20

#End Region


#Region " FIELD "


    Public Property Depth As Double Implements IPartialCutoutAddiction.Depth
    Public Property InsoleTopSrfCopyId As Guid Implements IPartialCutoutAddiction.InsoleTopSrfCopyId


#End Region


#Region " Constructor "


    Public Sub New(ByVal side As eSide, ByVal direction As eCutoutDirection)
        MyBase.New(side, direction)
    End Sub

    Protected Overrides Sub SetCutout()
        Me.Model = eAddictionModel.cutoutPartial
        Depth = 2
    End Sub


#End Region


#Region " Property "

    ''' <summary>
    ''' Ridefinito per comodità
    ''' </summary>
    Public Overloads Property Side() As eSide Implements IPartialCutoutAddiction.Side
        Get
            Return MyBase.Side
        End Get
        Set(value As eSide)
            MyBase.Side = value
        End Set
    End Property


#End Region


#Region " CAD Method "


    Public Shared Function SetMaxCutoutDepth(ByVal lataralSrfId As Guid, ByVal topSrfId As Guid, ByVal bottomSrfId As Guid) As Decimal
        Dim result As Decimal = 0
        'SICURAMENTE SARA' MINORE DEL MASSIMO DELLA BBOX DELLA SUPERFICIE LATERALE - calcolo in caso l'algoritmo fallisse
        Dim surfaceObjRef As New MRhinoObjRef(lataralSrfId)
        If surfaceObjRef Is Nothing Then Return result
        Dim lateralSrfObj As IRhinoObject = surfaceObjRef.Object()
        Dim bbox As OnBoundingBox = lateralSrfObj.BoundingBox
        Dim minHeight As Double = bbox.m_max.z - bbox.m_min.z
        result = Convert.ToDecimal(minHeight)
        bbox.Dispose()
        surfaceObjRef.Dispose()
        'Considerando che il cutout va a tagliare in punta dove lo spessore è minimo, 
        'impongo che la profondità sia minore della distanza tra i punti a X massima delle superfici superiore e inferiore           
        Dim curveTop As OnCurve = RhGeometry.CurvaDiBordoUnicaSrfTrimmata(topSrfId)
        If curveTop Is Nothing Then Return result
        Dim curveBottom As OnCurve = RhGeometry.CurvaDiBordoUnicaSrfTrimmata(bottomSrfId)
        If curveBottom Is Nothing Then Return result
#If DEBUG Then
        'AddDocumentToDebug(curveTop, "curveTop")
        'AddDocumentToDebug(curveBottom, "curveBottom")
#End If
        'Trovo i rispettivi punti con X massima
        Dim maxX As New On3dPoint
        Dim minX As New On3dPoint
        Dim minY As New On3dPoint
        Dim maxY As New On3dPoint
        RhGeometry.CurveFindExtremePoints(curveTop, minX, maxX, minY, maxY)
        Dim testPoint As New On3dPoint(maxX)
        RhGeometry.CurveFindExtremePoints(curveBottom, minX, maxX, minY, maxY)
#If DEBUG Then
        'AddDocumentToDebug(testPoint, "testPoint")
        'AddDocumentToDebug(maxX, "maxX")
#End If
        result = Convert.ToDecimal(testPoint.DistanceTo(maxX))
        maxX.Dispose()
        minX.Dispose()
        minY.Dispose()
        maxY.Dispose()
        testPoint.Dispose()
        curveTop.Dispose()
        curveBottom.Dispose()
        Return result
    End Function


    ''' <summary>
    ''' Esegue lo split delle superfici superiori della suola(originale e la copia) con l'estrusione della curva del cutout
    ''' </summary>
    ''' <param name="topSurface"></param>
    ''' <param name="insoleSrfRefId"></param>
    ''' <param name="cuttingSrfRefId"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetTrimmedTopSurface(ByVal topSurface As eTopSurface, ByVal insoleSrfRefId As Guid, ByVal cuttingSrfRefId As Guid) As Guid Implements IPartialCutoutAddiction.GetTrimmedTopSurface
        Dim getObjects As New MRhinoGetObject
        Dim sortedOgjList As New SortedList(Of Double, MRhinoObjRef)
        RhUtil.RhinoApp.RunScript("_SelNone", 0)
        Dim insoleId As String = insoleSrfRefId.ToString()
        Dim cuttingSrfID As String = cuttingSrfRefId.ToString()
        Dim splitCommand As String = "-_Split _SelID " & insoleId & " _Enter _SelId " & cuttingSrfID & " _Enter"
        RhUtil.RhinoApp().RunScript(splitCommand, 0)
        RhUtil.RhinoApp.RunScript("_SelNone", 0)
        RhUtil.RhinoApp().RunScript("_SelLast", 0)
        'Prendo i due oggetti risultanti dallo split            
        getObjects.SetGeometryFilter(IRhinoGetObject.GEOMETRY_TYPE_FILTER.surface_object Or IRhinoGetObject.GEOMETRY_TYPE_FILTER.polysrf_object)
        getObjects.GetObjects(0, Integer.MaxValue)
        If getObjects.ObjectCount <> 2 Then Return Nothing
        Dim bbox As New OnBoundingBox
        getObjects.Object(0).Object.GetTightBoundingBox(bbox)
        sortedOgjList.Add(bbox.Volume(), getObjects.Object(0))
        getObjects.Object(1).Object.GetTightBoundingBox(bbox)
        sortedOgjList.Add(bbox.Volume(), getObjects.Object(1))
        getObjects.Dispose()
        bbox.Dispose()
        RhUtil.RhinoApp.RunScript("_SelNone", 0)
        If topSurface = eTopSurface.original Then
            RhUtil.RhinoApp.ActiveDoc.DeleteObject(sortedOgjList.Values.Item(0))
            Return sortedOgjList.Values.Item(1).m_uuid
        Else
            RhUtil.RhinoApp.ActiveDoc.DeleteObject(sortedOgjList.Values.Item(1))
            Return sortedOgjList.Values.Item(0).m_uuid
        End If
    End Function



    ''' <summary>
    ''' A partire dalla superficie estrusa ritorna le curve L1 e L2 da cui è stato generato
    ''' </summary>
    ''' <param name="curveL1"></param>
    ''' <param name="curveL2"></param>
    ''' <returns></returns>
    ''' <remarks>Non posso salvarmi le curve prima perchè potrebbero essere state spostate dall'utente</remarks>
    Public Function GetTrimmedL1_L2FromSurface(ByRef curveL1 As OnCurve, ByRef curveL2 As OnCurve) As Boolean
        Dim cutoutSrfRef As New MRhinoObjRef(SurfaceID)
        If cutoutSrfRef Is Nothing Then Throw New Exception(LanguageManager.Message(358))
        curveL1 = Nothing
        curveL2 = Nothing
        'Estraggo la curva di bordo più vicino al massimo della bbox della superficie
        Dim maxZ As Double = cutoutSrfRef.Object.BoundingBox.m_max.z
        Dim borderCurves As New List(Of OnCurve)
        For Each edge As OnBrepEdge In cutoutSrfRef.Object.Geometry.BrepForm.m_E
            If edge.BoundingBox.m_min.z + 1 > maxZ Then borderCurves.Add(edge.NurbsCurve.DuplicateCurve())
        Next
        If borderCurves.Count <> 1 Then Throw New Exception(LanguageManager.Message(359))
        ''CREO LE DUE SEZIONI DI CURVA A PARTIRE DAI PRIMI DUE PUNTI DI CONROLLO DI OGNI ESTREMO
        Dim joinedCurve As OnCurve = borderCurves(0)
        Dim point1 As New On3dPoint
        Dim point2 As New On3dPoint
        joinedCurve.NurbsCurve.GetCV(0, point1)
        joinedCurve.NurbsCurve.GetCV(1, point2)
        Dim line1 As New OnLineCurve(point1, point2)
        joinedCurve.NurbsCurve.GetCV(joinedCurve.NurbsCurve.CVCount - 1, point1)
        joinedCurve.NurbsCurve.GetCV(joinedCurve.NurbsCurve.CVCount - 2, point2)
        Dim line2 As New OnLineCurve(point1, point2)
        curveL1 = line1.DuplicateCurve()
        curveL2 = line2.DuplicateCurve()
        If curveL1 Is Nothing Then Throw New Exception(LanguageManager.Message(360))
        If curveL2 Is Nothing Then Throw New Exception(LanguageManager.Message(361))
        cutoutSrfRef.Dispose()
        point1.Dispose()
        point2.Dispose()
        line1.Dispose()
        line2.Dispose()
        Return True
    End Function


    Public Function GetL1AndCenterFromTrimmedCurves(ByVal trimmedCurveL1 As OnCurve, ByVal trimmedCurveL2 As OnCurve, ByVal extensionLenght As Double, ByVal curveInsoleDistance As Double, ByRef centerRotation As On3dPoint) As OnCurve
        'ESTENDO LE CURVE L1 e L2
        Dim extendedCurveL1 As OnCurve = trimmedCurveL1.DuplicateCurve()
        Dim extendedCurveL2 As OnCurve = trimmedCurveL2.DuplicateCurve()
        RhUtil.RhinoExtendCurve(extendedCurveL1, IRhinoExtend.Type.Line, 0, extensionLenght)
        RhUtil.RhinoExtendCurve(extendedCurveL1, IRhinoExtend.Type.Line, 1, extensionLenght)
        RhUtil.RhinoExtendCurve(extendedCurveL2, IRhinoExtend.Type.Line, 0, extensionLenght)
        RhUtil.RhinoExtendCurve(extendedCurveL2, IRhinoExtend.Type.Line, 1, extensionLenght)
#If DEBUG Then
        'AddDocumentToDebug(extendedCurveL1, "curveL1 post-extend")
        'AddDocumentToDebug(extendedCurveL2, "curveL2 post-extend")
#End If
        'ESTRUDO SUPERFICI        
        Dim extrusionVector As New On3dVector(0, 0, -curveInsoleDistance * 2)
        Dim extrudedSrfL1 As OnSurface = RhUtil.RhinoExtrudeCurveStraight(extendedCurveL1, extrusionVector)
        Dim extrudedSrfL2 As OnSurface = RhUtil.RhinoExtrudeCurveStraight(extendedCurveL2, extrusionVector)
#If DEBUG Then
        'AddDocumentToDebug(extrudedSrfL1, "extrudedSrfL1")
        'AddDocumentToDebug(extrudedSrfL2, "extrudedSrfL2")
#End If
        'TROVO CENTRO DI ROTAZIONE DALL'INTERSEZIONE
        Dim curves() As OnCurve = {}
        Dim points As New On3dPointArray
        If Not RhUtil.RhinoIntersectSurfaces(extrudedSrfL1, extrudedSrfL2, 0.1, curves, points) Then Throw New Exception(LanguageManager.Message(363))
        If curves.Length <> 1 Then Throw New Exception(LanguageManager.Message(362))
#If DEBUG Then
        'AddDocumentToDebug(curves, "intersezioe L1-L2 per centro rotazione")
#End If
        centerRotation = New On3dPoint(curves(0).PointAtStart.x, curves(0).PointAtStart.y, extendedCurveL1.PointAtStart.z)
#If DEBUG Then
        'AddDocumentToDebug(centerRotation, "centerRotation")
#End If
        'TAGLIO LE CURVE ESTESE COL CENTRO DI ROTAZIONE
        Dim splittedPoints As New On3dPointArray
        splittedPoints.Append(centerRotation)
        Dim splittedL1 As OnCurveArray = RhGeometry.SplitCurve(extendedCurveL1, splittedPoints)
#If DEBUG Then
        'AddDocumentToDebug(splittedL1, "splittedL1")
#End If
        If splittedL1.Count <> 2 Then Throw New Exception(LanguageManager.Message(364))
        'Ritrovo la curva origiale L1(ma estesa) prima del raccordo e relativo Trim
        Dim curveL1 As OnCurve
        Dim testPointL1 As On3dPoint = trimmedCurveL1.BoundingBox.Center
        If splittedL1.Item(0).BoundingBox.Center.DistanceTo(testPointL1) < splittedL1.Item(1).BoundingBox.Center.DistanceTo(testPointL1) Then
            curveL1 = splittedL1.Item(0).DuplicateCurve()
        Else
            curveL1 = splittedL1.Item(1).DuplicateCurve()
        End If
        'Dispose
        extendedCurveL1.Dispose()
        extendedCurveL2.Dispose()
        extrusionVector.Dispose()
        extrudedSrfL1.Dispose()
        extrudedSrfL2.Dispose()
        splittedPoints.Dispose()
        splittedL1.Dispose()
        testPointL1.Dispose()
        Return curveL1
    End Function


    Public Overrides Sub DeleteSrf()
        MyBase.DeleteSrf()
        If IsSurfaceInDocument() Then RhUtil.RhinoApp.ActiveDoc.DeleteObject(New MRhinoObjRef(SurfaceID))
        If RhUtil.RhinoApp.ActiveDoc.LookupObject(InsoleTopSrfCopyId) IsNot Nothing Then
            RhUtil.RhinoApp.ActiveDoc.DeleteObject(New MRhinoObjRef(InsoleTopSrfCopyId))
        End If
    End Sub


    Public Sub CheckBlendSurfaceDirection(ByRef objRef As MRhinoObjRef)
        If Me.Side = eSide.right And Me.CutoutDirection = eCutoutDirection.external Or Me.Side = eSide.left And Me.CutoutDirection = eCutoutDirection.internal Then
            CheckSurfaceDirection(objRef, eDirectionCheck.cutoutBlendSrfNegY)
        Else
            CheckSurfaceDirection(objRef, eDirectionCheck.cutoutBlendSrfPosY)
        End If
    End Sub


    Function GetSweepSurface(ByRef extrusionCurveCutoutSrfRef As MRhinoObjRef, ByRef trimmedInsoleTopCopy As MRhinoObjRef,
                             ByRef borderCurve As OnCurve, ByRef offsetCurve As OnCurve) As OnSurface Implements IPartialCutoutAddiction.GetSweepSurface

        Dim insoleSrfRefLateral As MRhinoObjRef = Element3dManager.GetRhinoObjRef(eReferences.insoleLateralSurface, Me.Side)
        Dim insoleSrfRefBottom As MRhinoObjRef = Element3dManager.GetRhinoObjRef(eReferences.insoleBottomSurface, Me.Side)
        Dim intersectionCurves() As OnCurve = {}
        Dim intersectionPoints As New On3dPointArray
        Dim xform As New OnXform
        Dim getObjects As New MRhinoGetObject
        'Controllo che non ci sia intersezione tra la srf copiata tagliata e quella inferiore 
        Dim borderTrimmedInsoleTopCopy As OnCurve = RhGeometry.CurvaDiBordoUnicaSrfTrimmata(trimmedInsoleTopCopy.m_uuid)
        RhUtil.RhinoCurveBrepIntersect(borderTrimmedInsoleTopCopy, insoleSrfRefBottom.Surface.BrepForm, RhUtil.RhinoApp.ActiveDoc.AbsoluteTolerance, intersectionCurves, intersectionPoints)
        If intersectionCurves.Length > 0 Or intersectionPoints.Count > 0 Then
            Throw New Exception(LanguageManager.Message(365))
        End If
        'RICAVO UNA SEZIONE DELLE CURVE L1 E L2
        Dim trimmedCurveL1 As OnCurve = Nothing
        Dim trimmedCurveL2 As OnCurve = Nothing
        If Not Me.GetTrimmedL1_L2FromSurface(trimmedCurveL1, trimmedCurveL2) Then Throw New Exception(LanguageManager.Message(366))
        '#If DEBUG Then
        '        AddDocumentToDebug(trimmedCurveL1, "curveL1 per-extend")
        '        AddDocumentToDebug(trimmedCurveL2, "curveL2 per-extend")
        '#End If
        ''RECUPERO LA CURVA ESTESA CON UN ESTREMO NEL CENTRO DI ROTAZIONE
        Dim extensionLenght As Double = insoleSrfRefLateral.Object.BoundingBox.Diagonal.Length
        Dim curveInsoleDistance As Double = trimmedCurveL1.BoundingBox.m_max.z - trimmedInsoleTopCopy.Object.BoundingBox.m_min.z
        Dim centerSerie As On3dPoint = Nothing
        Dim curveL1 As OnCurve = Me.GetL1AndCenterFromTrimmedCurves(trimmedCurveL1, trimmedCurveL2, extensionLenght, curveInsoleDistance, centerSerie)
        '#If DEBUG Then
        '        AddDocumentToDebug(curveL1, "curveL1")
        '        AddDocumentToDebug(centerSerie, "centerRotation")
        '#End If
        intersectionCurves = {}
        intersectionPoints.Empty()
        ''CALCOLO LA CURVA DI INTERSEZIONE RELATIVA AL BORDO
        Dim intersectionResult As Boolean = RhUtil.RhinoIntersectSurfaces(trimmedInsoleTopCopy.Surface, extrusionCurveCutoutSrfRef.Surface, 0.1, intersectionCurves, intersectionPoints)
        If Not intersectionResult OrElse intersectionCurves.Length = 0 Then
            Throw New Exception(LanguageManager.Message(367))
        End If
        Dim mergedCurves() As OnCurve = {}
        If intersectionCurves.Length <> 1 Then
            RhUtil.RhinoMergeCurves(intersectionCurves, mergedCurves)
        Else
            ReDim mergedCurves(0)
            mergedCurves(0) = intersectionCurves(0)
        End If
        If mergedCurves.Length <> 1 Then Throw New Exception(LanguageManager.Message(368))
        Dim externalProjCrvObj As MRhinoCurveObject = RhUtil.RhinoApp.ActiveDoc.AddCurveObject(mergedCurves(0))
        ''CREO LA SERIE POLARE DI CURVE E LE PROIETTO                   
        RhViewport.SetNewConstructionPlane(OnUtil.On_xy_plane)
        Dim rotationAngle As Double = Math.PI / POLAR_CURVE_PER_PIGRECO
        If Me.Side = eSide.right And Me.CutoutDirection = eCutoutDirection.internal Or Me.Side = eSide.left And Me.CutoutDirection = eCutoutDirection.external Then
            rotationAngle = -rotationAngle
        End If
        Dim projectedCurves As New List(Of Guid)
        For i As Integer = 1 To POLAR_CURVE_PER_PIGRECO
            RhUtil.RhinoApp.RunScript("_SelNone", 0)
            xform.Identity()
            xform.Rotation(rotationAngle * i, New On3dVector(0, 0, -1), centerSerie)
            Dim curve As OnCurve = curveL1.DuplicateCurve()
            curve.Transform(xform)
            getObjects.ClearObjects()
            getObjects.SetGeometryFilter(IRhinoGetObject.GEOMETRY_TYPE_FILTER.curve_object)
            ''PROIETTO LE CURVE SULLA COPIA DELLA SUPERFICIE SUPERIORE DEL PLANATER
            Dim curveObj As MRhinoObject = RhUtil.RhinoApp.ActiveDoc.AddCurveObject(curve)
            RhUtil.RhinoApp.RunScript("_SelNone", 0)
            curveObj.Select(True, True)
            If RhinoLanguageSetting() = elanguage.English Then
                RhUtil.RhinoApp.RunScript("-_Project _SelID " & trimmedInsoleTopCopy.m_uuid.ToString() & " _L=N _D=N O=Current _Enter", 0)
            Else
                RhUtil.RhinoApp.RunScript("-_Project _SelID " & trimmedInsoleTopCopy.m_uuid.ToString() & " _A=N _C=N O=Corrente _Enter", 0)
            End If
            RhUtil.RhinoApp.RunScript("_SelNone", 0)
            RhUtil.RhinoApp.RunScript("_SelLast", 0)
            getObjects.GetObjects(0, Integer.MaxValue)
            If getObjects.ObjectCount = 1 Then
                Dim projectedCrvRef As MRhinoObjRef = getObjects.Object(0)
                'Evito di aggiungere doppioni a causa delle curve che vanno fuori e il SelLast ritorna la precedente
                If Not projectedCrvRef.m_uuid = curveObj.Attributes.m_uuid Then
                    'Controllo che la curva proiettata intersechi quella di bordo in due punti
                    RhUtil.RhinoApp.RunScript("_SelNone", 0)
                    Dim curveToCheck As IOnCurve = projectedCrvRef.Curve
                    If curveToCheck Is Nothing Then Continue For
                    Dim checkSrf As OnSurface = RhUtil.RhinoExtrudeCurveStraight(curveToCheck, New On3dVector(0, 0, -curveInsoleDistance * 2))
                    xform.Identity()
                    xform.Translation(0, 0, curveInsoleDistance)
                    checkSrf.Transform(xform)
                    intersectionPoints.Empty()
                    '#If DEBUG Then
                    '                        AddDocumentToDebug(borderTrimmedInsoleTopCopy, "borderTrimmedInsoleTopCopy")
                    '                        AddDocumentToDebug(checkSrf, "checkSrf")
                    '#End If
                    intersectionPoints = RhGeometry.IntersecaCurvaConSuperfice(borderTrimmedInsoleTopCopy, checkSrf)
                    If intersectionPoints.Count = 2 Then
                        projectedCurves.Add(projectedCrvRef.m_uuid)
                    Else
                        RhUtil.RhinoApp.ActiveDoc.DeleteObject(projectedCrvRef)
                    End If
                    checkSrf.Dispose()
                End If
                projectedCrvRef.Dispose()
            End If
            '#If Not Debug Then
            RhUtil.RhinoApp.ActiveDoc.PurgeObject(curveObj)
            '#End If
            RhUtil.RhinoApp.RunScript("_SelNone", 0)
        Next
        'ESTENDO LE CURVE
        Dim externalProjCurve As OnCurve = externalProjCrvObj.Curve.DuplicateCurve()
        'Estendo di molto perchè in casi di particolare pendenza potrebbe dare problemi
        extensionLenght = OFFSET_PARTIAL_CUTOUT * 10
        RhUtil.RhinoExtendCurve(externalProjCurve, IRhinoExtend.Type.Line, 0, extensionLenght)
        RhUtil.RhinoExtendCurve(externalProjCurve, IRhinoExtend.Type.Line, 1, extensionLenght)
        RhUtil.RhinoApp.ActiveDoc.DeleteObject(New MRhinoObjRef(externalProjCrvObj.Attributes.m_uuid))
        externalProjCrvObj = RhUtil.RhinoApp.ActiveDoc.AddCurveObject(externalProjCurve)
        Dim extendedProjCurve As New List(Of MRhinoObject)
        For Each uuid As Guid In projectedCurves
            Dim objref As New MRhinoObjRef(uuid)
            If objref Is Nothing Then Continue For
            Dim crv As IOnCurve = objref.Curve
            If crv Is Nothing Then Continue For
            Dim curve As OnCurve = crv.DuplicateCurve()
            Dim centerSerieProjected As New On3dPoint(centerSerie)
            centerSerieProjected.z = curve.BoundingBox.Center.z
            '#If DEBUG Then
            '            AddDocumentToDebug(curve, "pre-extended")
            '            AddDocumentToDebug(centerSerieProjected, "centerSerieProjected")
            '#End If
            'Controllo quale estremo va esteso
            If curve.PointAtStart.DistanceTo(centerSerieProjected) > curve.PointAtEnd.DistanceTo(centerSerieProjected) Then
                RhUtil.RhinoExtendCurve(curve, IRhinoExtend.Type.Line, 0, extensionLenght)
            Else
                RhUtil.RhinoExtendCurve(curve, IRhinoExtend.Type.Line, 1, extensionLenght)
            End If
            '#If DEBUG Then
            '            AddDocumentToDebug(curve, "post-extended")
            '#End If
            extendedProjCurve.Add(RhUtil.RhinoApp.ActiveDoc.AddCurveObject(curve))
            RhUtil.RhinoApp.ActiveDoc.DeleteObject(objref)
            objref.Dispose()
            curve.Dispose()
            centerSerieProjected.Dispose()
        Next
        'ESTRUDO CURVA DI BORDO E DI OFFSET
        Dim offsetSrf As OnSurface = RhUtil.RhinoExtrudeCurveStraight(offsetCurve, New On3dVector(0, 0, -curveInsoleDistance * 2))
        Dim borderSrf As OnSurface = RhUtil.RhinoExtrudeCurveStraight(borderCurve, New On3dVector(0, 0, -curveInsoleDistance * 2))
        xform.Identity()
        xform.Translation(0, 0, curveInsoleDistance)
        offsetSrf.Transform(xform)
        borderSrf.Transform(xform)
        Dim offsetSrfObj As MRhinoObject = RhUtil.RhinoApp.ActiveDoc.AddSurfaceObject(offsetSrf)
        Dim borderSrfObj As MRhinoObject = RhUtil.RhinoApp.ActiveDoc.AddSurfaceObject(borderSrf)
        ''DOPPIO TAGLIO DELLE CURVE
        Dim sweepSections As New List(Of Guid)
        Dim sortedObj As New SortedDictionary(Of Double, MRhinoObjRef)
        Dim offsetSrfObjId As String = offsetSrfObj.Attributes.m_uuid.ToString()
        Dim borderSrfObjId As String = borderSrfObj.Attributes.m_uuid.ToString()
        Dim splitCmd As String = "-_Split _SelID " & offsetSrfObjId & " _SelID " & borderSrfObjId & " _Enter"
        Dim testpoint As On3dPoint = Nothing
        For Each rhinoObj As MRhinoObject In extendedProjCurve
            RhUtil.RhinoApp.RunScript("_SelNone", 0)
            getObjects.ClearObjects()
            rhinoObj.Select(True, True)
            RhUtil.RhinoApp.RunScript(splitCmd, 0)
            RhUtil.RhinoApp.RunScript("_SelNone", 0)
            RhUtil.RhinoApp.RunScript("_SelLast", 0)
            getObjects.SetGeometryFilter(IRhinoGetObject.GEOMETRY_TYPE_FILTER.curve_object)
            getObjects.GetObjects(0, Integer.MaxValue)
            If getObjects.ObjectCount <> 3 Then Continue For
            'Ordino in base alla distanza dal centro di rotazione e prendo il medio
            sortedObj.Clear()
            testpoint = New On3dPoint(getObjects.Object(0).Object.BoundingBox.Center)
            testpoint.z = centerSerie.z
            sortedObj.Add(testpoint.DistanceTo(centerSerie), getObjects.Object(0))
            testpoint = New On3dPoint(getObjects.Object(1).Object.BoundingBox.Center)
            testpoint.z = centerSerie.z
            sortedObj.Add(testpoint.DistanceTo(centerSerie), getObjects.Object(1))
            testpoint = New On3dPoint(getObjects.Object(2).Object.BoundingBox.Center)
            testpoint.z = centerSerie.z
            sortedObj.Add(testpoint.DistanceTo(centerSerie), getObjects.Object(2))
            sweepSections.Add(sortedObj.Values(1).m_uuid)
            RhUtil.RhinoApp.ActiveDoc.DeleteObject(sortedObj.Values(0))
            RhUtil.RhinoApp.ActiveDoc.DeleteObject(sortedObj.Values(2))
            RhUtil.RhinoApp.RunScript("_SelNone", 0)
        Next
        'TRIPLO TAGLIO DELLA CURVA DI BORDO
        Dim borderSweepSection1 As OnCurve = Nothing
        Dim borderSweepSection2 As OnCurve = Nothing
        RhUtil.RhinoApp.RunScript("_SelNone", 0)
        getObjects.ClearObjects()
        sortedObj.Clear()
        externalProjCrvObj.Select(True, True)
        RhUtil.RhinoApp.RunScript("-_Split _SelID " & borderSrfObjId & " _Enter", 0)
        RhUtil.RhinoApp.RunScript("_SelNone", 0)
        RhUtil.RhinoApp.RunScript("_SelLast", 0)
        getObjects.SetGeometryFilter(IRhinoGetObject.GEOMETRY_TYPE_FILTER.curve_object)
        getObjects.GetObjects(0, Integer.MaxValue)
        If getObjects.ObjectCount <> 3 Then Throw New Exception(LanguageManager.Message(369))
        'Filtro i 3 pezzi in base alla massima X della bbox
        sortedObj.Add(getObjects.Object(0).Object.BoundingBox.m_max.x, getObjects.Object(0))
        sortedObj.Add(getObjects.Object(1).Object.BoundingBox.m_max.x, getObjects.Object(1))
        sortedObj.Add(getObjects.Object(2).Object.BoundingBox.m_max.x, getObjects.Object(2))
        RhUtil.RhinoApp.ActiveDoc.DeleteObject(sortedObj.Values(1))
        Dim segmentCrv1 As MRhinoObjRef = sortedObj.Values(0)
        Dim segmentCrv2 As MRhinoObjRef = sortedObj.Values(2)
        'SECONDO TAGLIO
        RhUtil.RhinoApp.RunScript("_SelNone", 0)
        getObjects.ClearObjects()
        sortedObj.Clear()
        segmentCrv1.Object.Select(True, True)
        RhUtil.RhinoApp.RunScript("-_Split _SelID " & offsetSrfObjId & " _Enter", 0)
        RhUtil.RhinoApp.RunScript("_SelNone", 0)
        RhUtil.RhinoApp.RunScript("_SelLast", 0)
        getObjects.SetGeometryFilter(IRhinoGetObject.GEOMETRY_TYPE_FILTER.curve_object)
        getObjects.GetObjects(0, Integer.MaxValue)
        If getObjects.ObjectCount <> 2 Then Throw New Exception(LanguageManager.Message(370))
        testpoint = New On3dPoint(getObjects.Object(0).Object.BoundingBox.Center)
        testpoint.z = centerSerie.z
        sortedObj.Add(testpoint.DistanceTo(centerSerie), getObjects.Object(0))
        testpoint = New On3dPoint(getObjects.Object(1).Object.BoundingBox.Center)
        testpoint.z = centerSerie.z
        sortedObj.Add(testpoint.DistanceTo(centerSerie), getObjects.Object(1))
        RhUtil.RhinoApp.ActiveDoc.DeleteObject(sortedObj.Values(1))
        sweepSections.Add(sortedObj.Values(0).m_uuid)
        borderSweepSection1 = sortedObj.Values(0).Curve.DuplicateCurve
        'TERZO TAGLIO
        RhUtil.RhinoApp.RunScript("_SelNone", 0)
        getObjects.ClearObjects()
        sortedObj.Clear()
        segmentCrv2.Object.Select(True, True)
        RhUtil.RhinoApp.RunScript("-_Split _SelID " & offsetSrfObjId & " _Enter", 0)
        RhUtil.RhinoApp.RunScript("_SelNone", 0)
        RhUtil.RhinoApp.RunScript("_SelLast", 0)
        getObjects.SetGeometryFilter(IRhinoGetObject.GEOMETRY_TYPE_FILTER.curve_object)
        getObjects.GetObjects(0, Integer.MaxValue)
        If getObjects.ObjectCount <> 2 Then Throw New Exception(LanguageManager.Message(371))
        testpoint = New On3dPoint(getObjects.Object(0).Object.BoundingBox.Center)
        testpoint.z = centerSerie.z
        sortedObj.Add(testpoint.DistanceTo(centerSerie), getObjects.Object(0))
        testpoint = New On3dPoint(getObjects.Object(1).Object.BoundingBox.Center)
        testpoint.z = centerSerie.z
        sortedObj.Add(testpoint.DistanceTo(centerSerie), getObjects.Object(1))
        sweepSections.Add(sortedObj.Values(0).m_uuid)
        borderSweepSection2 = sortedObj.Values(0).Curve.DuplicateCurve
        RhUtil.RhinoApp.ActiveDoc.DeleteObject(sortedObj.Values(1))
        RhUtil.RhinoApp.RunScript("_SelNone", 0)
        ''SPLIT PER OTTENERE IL BINARIO INTERNO DELLO SWEEP1
        Dim sweepRail1 As OnCurve = Nothing
        Dim minLenght As Double = Double.MaxValue
        For Each curve As OnCurve In RhGeometry.SplitCurveBySurface(borderCurve, extrusionCurveCutoutSrfRef.Surface, New On3dPointArray())
            Dim lenght As Double = 0
            curve.GetLength(lenght)
            If lenght < minLenght Then
                minLenght = lenght
                sweepRail1 = curve
            End If
        Next
        If sweepRail1 Is Nothing Then Throw New Exception(LanguageManager.Message(372))
        RhUtil.RhinoApp.ActiveDoc.PurgeObject(offsetSrfObj)
        RhUtil.RhinoApp.ActiveDoc.PurgeObject(borderSrfObj)
        Dim sweep1RailObj As MRhinoObject = RhUtil.RhinoApp.ActiveDoc.AddCurveObject(sweepRail1)
        RhUtil.RhinoApp.RunScript("_SelNone", 0)
        ''SWEEP1 PER OTTENERE LA SUPERFICIE AGGIUNTIVA PER IL TAGLIO DI QUELLA LATERALE DEL PLANTARE
        Dim stringaSweep As String = "-_Sweep1"
        For Each stringID As Guid In sweepSections
            stringaSweep &= " _SelID " & stringID.ToString()
        Next
        If RhDocument.RhinoMajorRelease() = 5 Then stringaSweep &= " _Enter"
        If RhinoLanguageSetting() = elanguage.English Then
            'Con _U=Yes obbligo a farla unita (in teoria è _C=Yes)
            stringaSweep &= " _Enter _S=Freeform _I=None _C=Yes _U=Yes _Enter"
        Else
            stringaSweep &= " _Enter _S=TorsioneLibera _E=Nessuno _C=Sì _U=Sì _Enter"
        End If
        sweep1RailObj.Select(True, True)
        RhUtil.RhinoApp.RunScript(stringaSweep, 0)
        getObjects.ClearObjects()
        Dim filter() As IRhinoGetObject.GEOMETRY_TYPE_FILTER = {IRhinoGetObject.GEOMETRY_TYPE_FILTER.surface_object, IRhinoGetObject.GEOMETRY_TYPE_FILTER.polysrf_object}
        getObjects.SetGeometryFilter(filter)
        RhUtil.RhinoApp.RunScript("_SelLast", 0)
        getObjects.GetObjects(0, Integer.MaxValue)
        If getObjects.ObjectCount <> 1 Then Throw New Exception(LanguageManager.Message(373))
        Dim sweepSurfaceRef As MRhinoObjRef = getObjects.Object(0)
        'ESTENDO SUPERFICIE PER SUCCESSIVO USO COME GUIDA DELLA NUOVA SUPERFICIE
        Dim sweepSurface As OnSurface = sweepSurfaceRef.Surface().DuplicateSurface
        Dim edgeIndexes() As Integer = GetSweepEdgeIndexToExtend(sweepSurface, borderSweepSection1, borderSweepSection2)
        For i As Integer = 3 To 6            'W_iso = 3, S_iso = 4, E_iso = 5, N_iso = 6
            Dim edgeIndex As IOnSurface.ISO = CType(i, IOnSurface.ISO)
            If edgeIndexes.Contains(i) Then
                RhUtil.RhinoExtendSurface(sweepSurface, edgeIndex, 20, False)
            Else
                RhUtil.RhinoExtendSurface(sweepSurface, edgeIndex, 2, False)
            End If
        Next
        '#If DEBUG Then
        '            AddDocumentToDebug(sweepSurface, "sweepSurface estesa")
        '#End If
        'Pulisco DOC
        RhUtil.RhinoApp.ActiveDoc.PurgeObject(sweep1RailObj)
        sweep1RailObj.Dispose()
        For Each uuid As Guid In sweepSections
            RhUtil.RhinoApp.ActiveDoc.DeleteObject(New MRhinoObjRef(uuid))
        Next
        RhUtil.RhinoApp.ActiveDoc.DeleteObject(sweepSurfaceRef)
        'Dispose
        trimmedCurveL1.Dispose()
        trimmedCurveL2.Dispose()
        offsetSrf.Dispose()
        borderSrf.Dispose()
        offsetSrfObj.Dispose()
        borderSrfObj.Dispose()
        externalProjCrvObj.Dispose()
        externalProjCurve.Dispose()
        borderTrimmedInsoleTopCopy.Dispose()

        Return sweepSurface
    End Function



#End Region


#Region " SERIALIZZAZIONE / DESERIALIZZAZIONE "


    Public Overrides Function Serialize(ByRef archive As OnBinaryArchive) As Boolean
        If Not MyBase.CommonSerialize(archive) Then Return False

        'DOUBLE
        If Not archive.WriteDouble(Depth) Then Return False
        'BREP
        If Not archive.WriteObject(Me.BackupInsoleSurface(eAddictionBkSrf.lateral)) Then Return False
    
        Return True
    End Function

    Public Overrides Function Deserialize(ByRef archive As OnBinaryArchive) As Boolean
        If Not archive.ReadDouble(Depth) Then Return False

        Dim onobj As OnObject = New OnBrep()
        If Not CBool(archive.ReadObject(onobj)) Then Return False
        Me.BackupInsoleSurface(eAddictionBkSrf.lateral) = OnBrep.Cast(onobj).Duplicate
        onobj.Dispose()

        Return True
    End Function


#End Region


#Region " IClonable "

    Public Overrides Function Clone() As Object
        Dim plainObject As IdAddiction = IdAddictionFactory.Create(Me.Side, Me.Type, Me.Model, Me.Size, Me.CutoutDirection)
        MyBase.CloneCommonField(plainObject)
        Dim plainCutout As IdCutoutToTalAddiction = DirectCast(plainObject, IdCutoutToTalAddiction)
        'Campi specifici IdCutoutToTalAddiction
        MyBase.CloneCommonCutoutField(plainCutout)
        Dim result As IdCutoutPartialAddiction = DirectCast(plainCutout, IdCutoutPartialAddiction)
        'Campi specifici IdCutoutPartialAddiction
        result.Depth = Me.Depth
        result.BlendSurfaceID = New Guid(Me.BlendSurfaceID.ToString)
        result.InsoleTopSrfCopyId = New Guid(Me.InsoleTopSrfCopyId.ToString)
        Return (result)
    End Function

#End Region



End Class
