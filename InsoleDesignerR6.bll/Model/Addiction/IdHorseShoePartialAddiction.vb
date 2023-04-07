Imports RMA.OpenNURBS
Imports InsoleDesigner.bll.IdAlias
Imports InsoleDesigner.bll.IdLanguageManager
Imports InsoleDesigner.bll.IdElement3dManager
Imports GeometryUtils = InsoleDesigner.bll.IdGeometryUtils
Imports InsoleDesigner.bll.AbstractCutoutCommons
Imports InsoleDesigner.bll.IdCutoutPartialAddiction
Imports RhinoUtils
Imports RhinoUtils.RhDocument
Imports RMA.Rhino


Public Class IdHorseShoePartialAddiction
    Inherits IdHorseShoeTotalAddiction
    Implements IPartialCutoutAddiction




#Region " FIELD "


    Public Property Depth As Double Implements IPartialCutoutAddiction.Depth
    Public Property InsoleTopSrfCopyId As Guid Implements IPartialCutoutAddiction.InsoleTopSrfCopyId


#End Region


#Region " Constructor "


    Public Sub New(ByVal side As IdElement3dManager.eSide)
        MyBase.New(side)
    End Sub

    Protected Overrides Sub SetModel()
        Me.Model = eAddictionModel.horseShoePartial
        Depth = 2
    End Sub


#End Region


#Region " Property "


    ''' <summary>
    ''' Ridefinito per comodità
    ''' </summary>
    Public Overloads Property Side() As IdElement3dManager.eSide Implements IPartialCutoutAddiction.Side
        Get
            Return MyBase.Side
        End Get
        Set(value As IdElement3dManager.eSide)
            MyBase.Side = value
        End Set
    End Property


#End Region


#Region " CAD Method "


    ''' <summary>
    ''' Ordino gli estremi delle due curve in base alla distanza dal centro e creo una curca unica con gli estremi
    ''' </summary> 
    ''' <remarks>OBSOLETO</remarks>
    Public Shared Function JoinProjectedCurve(ByVal centerSerie As On3dPoint, ByVal curve1 As IOnCurve, ByVal curve2 As IOnCurve) As OnCurve
        Dim points As New SortedList(Of Double, On3dPoint)
        points.Add(curve1.PointAtStart.DistanceTo(centerSerie), curve1.PointAtStart)
        points.Add(curve1.PointAtEnd.DistanceTo(centerSerie), curve1.PointAtEnd)
        points.Add(curve2.PointAtStart.DistanceTo(centerSerie), curve2.PointAtStart)
        points.Add(curve2.PointAtEnd.DistanceTo(centerSerie), curve2.PointAtEnd)
        Dim lineCurve As New OnLineCurve(points.Values(0), points.Values(3))
        Dim result As OnCurve = lineCurve.NurbsCurve.DuplicateCurve
        lineCurve.Dispose()
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
        sortedOgjList.Add(bbox.m_max.x, getObjects.Object(0))
        getObjects.Object(1).Object.GetTightBoundingBox(bbox)
        sortedOgjList.Add(bbox.m_max.x, getObjects.Object(1))
        getObjects.Dispose()
        bbox.Dispose()
        RhUtil.RhinoApp.RunScript("_SelNone", 0)
        If topSurface = eTopSurface.original Then
            RhUtil.RhinoApp.ActiveDoc.DeleteObject(sortedOgjList.Values.Item(1))
            Return sortedOgjList.Values.Item(0).m_uuid
        Else
            RhUtil.RhinoApp.ActiveDoc.DeleteObject(sortedOgjList.Values.Item(0))
            Return sortedOgjList.Values.Item(1).m_uuid
        End If
    End Function


    Public Overrides Sub DeleteSrf()
        MyBase.DeleteSrf()
        If IsSurfaceInDocument() Then RhUtil.RhinoApp.ActiveDoc.DeleteObject(New MRhinoObjRef(SurfaceID))
        If RhUtil.RhinoApp.ActiveDoc.LookupObject(InsoleTopSrfCopyId) IsNot Nothing Then
            RhUtil.RhinoApp.ActiveDoc.DeleteObject(New MRhinoObjRef(InsoleTopSrfCopyId))
        End If
    End Sub



    Function GetSweepSurface(ByRef extrusionCurveCutoutSrfRef As MRhinoObjRef, ByRef trimmedInsoleTopCopy As MRhinoObjRef,
                             ByRef borderCurve As OnCurve, ByRef offsetCurve As OnCurve) As OnSurface Implements IPartialCutoutAddiction.GetSweepSurface

        Dim insoleSrfRefLateral As MRhinoObjRef = IdElement3dManager.GetInstance().GetRhinoObjRef(IdElement3dManager.eReferences.insoleLateralSurface, Me.Side)
        Dim insoleSrfRefBottom As MRhinoObjRef = IdElement3dManager.GetInstance().GetRhinoObjRef(IdElement3dManager.eReferences.insoleBottomSurface, Me.Side)
        Dim intersectionCurves() As OnCurve = {}
        Dim intersectionPoints As New On3dPointArray
        Dim xform As New OnXform
        Dim getObjects As New MRhinoGetObject

        'Controllo che non ci sia intersezione tra la srf copiata tagliata e quella inferiore        
        Dim borderTrimmedInsoleTopCopy As OnCurve = RhGeometry.CurvaDiBordoUnicaSrfTrimmata(trimmedInsoleTopCopy.m_uuid)
        RhUtil.RhinoCurveBrepIntersect(borderTrimmedInsoleTopCopy, insoleSrfRefBottom.Surface.BrepForm, RhUtil.RhinoApp.ActiveDoc.AbsoluteTolerance, intersectionCurves, intersectionPoints)
        If intersectionCurves.Length > 0 Or intersectionPoints.Count > 0 Then
            Throw New Exception(LanguageManager.Message(374))
        End If
        ''CALCOLO LA CURVA DI INTERSEZIONE RELATIVA AL BORDO
        intersectionCurves = {}
        intersectionPoints = New On3dPointArray
        Dim intersectionResult As Boolean = RhUtil.RhinoIntersectBreps(trimmedInsoleTopCopy.Brep, extrusionCurveCutoutSrfRef.Geometry.BrepForm, 0.1, intersectionCurves, intersectionPoints)
        '#If DEBUG Then
        '            AddDocumentToDebug(intersectionCurves, "intersectionCurves")
        '#End If
        If Not intersectionResult OrElse intersectionCurves.Length = 0 Then
            Throw New Exception(LanguageManager.Message(375))
        End If
        Dim mergedCurves() As OnCurve = {}
        If intersectionCurves.Length <> 1 Then
            RhUtil.RhinoMergeCurves(intersectionCurves, mergedCurves)
        Else
            ReDim mergedCurves(0)
            mergedCurves(0) = intersectionCurves(0)
        End If
        If mergedCurves.Length <> 1 Then Throw New Exception(LanguageManager.Message(376))
        Dim externalProjCrvObj As MRhinoCurveObject = RhUtil.RhinoApp.ActiveDoc.AddCurveObject(mergedCurves(0))
        ''Prendo il punto della curva con minore X poi calcolcare il centro della serie di curve da proiettare
        Dim minX As New On3dPoint
        Dim maxX As New On3dPoint
        Dim minY As New On3dPoint
        Dim maxY As New On3dPoint
        If Not RhGeometry.CurveFindExtremePoints(mergedCurves(0), minX, maxX, minY, maxY) Then Throw New Exception(LanguageManager.Message(377))
        Dim heightSerieZ As Double = extrusionCurveCutoutSrfRef.Object.BoundingBox.m_max.z
        Dim centerSerie As On3dPoint = New On3dPoint(minX.x, minX.y, heightSerieZ)
        '#If DEBUG Then
        'AddDocumentToDebug(centerSerie, "centerSerie")
        '#End If    
        minX.Dispose()
        maxX.Dispose()
        minY.Dispose()
        maxY.Dispose()
        mergedCurves = {}
        'ESTRUDO CURVA OFFSET E DI BORDO
        Dim extrusionLenght As Double = insoleSrfRefLateral.Object.BoundingBox.Diagonal.x
        Dim offsetSrf As OnSurface = RhUtil.RhinoExtrudeCurveStraight(offsetCurve, New On3dVector(0, 0, -extrusionLenght))
        Dim borderSrf As OnSurface = RhUtil.RhinoExtrudeCurveStraight(borderCurve, New On3dVector(0, 0, -extrusionLenght))
        xform.Translation(0, 0, extrusionLenght / 2)
        offsetSrf.Transform(xform)
        borderSrf.Transform(xform)
        Dim offsetSrfObj As MRhinoObject = RhUtil.RhinoApp.ActiveDoc.AddSurfaceObject(offsetSrf)
        Dim borderSrfObj As MRhinoObject = RhUtil.RhinoApp.ActiveDoc.AddSurfaceObject(borderSrf)
        'SPLIT PER OTTENERE IL BINARIO INTERNO DELLO SWEEP1
        Dim sweep1RailObj As MRhinoObject = Nothing
        intersectionCurves = {}
        intersectionPoints = New On3dPointArray
        If Not RhUtil.RhinoCurveBrepIntersect(borderCurve, extrusionCurveCutoutSrfRef.Geometry.BrepForm, 0.1, intersectionCurves, intersectionPoints) Then
            Throw New Exception(LanguageManager.Message(378))
        End If
        If intersectionPoints Is Nothing Or intersectionPoints.Count <> 2 Then Throw New Exception(LanguageManager.Message(379))
        Dim splittedCurves As OnCurveArray = RhGeometry.SplitCurve(borderCurve, intersectionPoints)
        If splittedCurves Is Nothing OrElse splittedCurves.Count <> 2 Then Throw New Exception(LanguageManager.Message(380))
        Dim sweep1RailCurve As OnCurve = Nothing
        If splittedCurves.Item(0).BoundingBox.m_max.x > splittedCurves.Item(1).BoundingBox.m_max.x Then
            sweep1RailCurve = splittedCurves.Item(0)
        Else
            sweep1RailCurve = splittedCurves.Item(1)
        End If
        sweep1RailObj = RhUtil.RhinoApp.ActiveDoc.AddCurveObject(sweep1RailCurve)
        'CREO LA CURVA BASE PER CREARE LE CURVE PER LA SERIE
        Dim rotationAngle As Double = Math.PI / POLAR_CURVE_PER_PIGRECO
        Dim lineCurveToSerie As New OnLineCurve(centerSerie, New On3dPoint(externalProjCrvObj.Curve.PointAtStart.x, externalProjCrvObj.Curve.PointAtStart.y, heightSerieZ))
        Dim curveToSerie As OnCurve = lineCurveToSerie.NurbsCurve.DuplicateCurve
        Dim extensionLenght As Double = extrusionCurveCutoutSrfRef.Object.BoundingBox.Diagonal.x
        If Not RhUtil.RhinoExtendCurve(curveToSerie, IRhinoExtend.Type.Line, 1, extensionLenght) Then Throw New Exception(LanguageManager.Message(381))
        '#If DEBUG Then
        '            AddDocumentToDebug(curveToSerie, "curveToSerie")
        '#End If
        'CREO LA SERIE POLARE DI CURVE
        Dim polarCurves As New List(Of OnCurve)
        For i As Integer = 1 To POLAR_CURVE_PER_PIGRECO * 2
            xform.Identity()
            xform.Rotation(rotationAngle * i, New On3dVector(0, 0, -1), centerSerie)
            Dim curve As OnCurve = curveToSerie.DuplicateCurve()
            curve.Transform(xform)
            polarCurves.Add(curve)
        Next
        '#If DEBUG Then
        '        AddDocumentToDebug(polarCurves, "polarCurves")
        '#End If
        'ESTRUDO LE CURVE E PROIETTO
        Dim projectedCurves As New List(Of OnCurve)
        extrusionLenght = Me.GetBbox.MaximumDistanceTo(trimmedInsoleTopCopy.Object.BoundingBox)
        Dim externalProjCurve As OnCurve = externalProjCrvObj.Curve.DuplicateCurve()
        For Each polarCurve As OnCurve In polarCurves
            Dim extrudedlCurveSrf As OnSurface = RhUtil.RhinoExtrudeCurveStraight(polarCurve, New On3dVector(0, 0, -1), extrusionLenght)
            '#If DEBUG Then
            '                AddDocumentToDebug(extrudedlCurveSrf, "extrudedCurveSrf")
            '#End If
            'ESCLUDO LE CURVE CHE INTERSECANO QUELLA DI BORDO
            intersectionPoints = RhGeometry.IntersecaCurvaConSuperfice(externalProjCurve, extrudedlCurveSrf)
            If intersectionPoints IsNot Nothing AndAlso intersectionPoints.Count > 1 Then Continue For
            intersectionCurves = {}
            intersectionPoints = New On3dPointArray
            RhUtil.RhinoIntersectBreps(trimmedInsoleTopCopy.Brep, extrudedlCurveSrf.BrepForm, RhUtil.RhinoApp.ActiveDoc.AbsoluteTolerance, intersectionCurves, intersectionPoints)
            If intersectionPoints.Count = 0 And intersectionCurves.Length = 1 Then
                Dim projectedCurve As OnCurve = intersectionCurves(0).DuplicateCurve
                intersectionCurves = {}
                intersectionPoints = New On3dPointArray
                RhUtil.RhinoCurveBrepIntersect(sweep1RailCurve, extrudedlCurveSrf.BrepForm, RhUtil.RhinoApp.ActiveDoc.AbsoluteTolerance, intersectionCurves, intersectionPoints)
                If intersectionPoints.Count = 1 And intersectionCurves.Length = 0 Then
                    projectedCurves.Add(projectedCurve)
                End If
            End If
            intersectionPoints.Dispose()
        Next
        polarCurves.Clear()
        '#If DEBUG Then
        '            AddDocumentToDebug(projectedCurves, "projectedCurves")
        '#End If
        'ESTENDO LE CURVE PROIETTATE di molto perchè in casi di particolare pendenza potrebbe dare problemi
        extensionLenght = OFFSET_PARTIAL_CUTOUT * 10
        Dim extendedProjectedCurves As New List(Of OnCurve)
        For Each projectedCurve As OnCurve In projectedCurves
            If projectedCurve.PointAtStart.DistanceTo(centerSerie) > projectedCurve.PointAtEnd.DistanceTo(centerSerie) Then
                RhUtil.RhinoExtendCurve(projectedCurve, IRhinoExtend.Type.Line, 0, extensionLenght)
            Else
                RhUtil.RhinoExtendCurve(projectedCurve, IRhinoExtend.Type.Line, 1, extensionLenght)
            End If
            extendedProjectedCurves.Add(projectedCurve.DuplicateCurve)
            projectedCurve.Dispose()
        Next
        projectedCurves.Clear()
        '#If DEBUG Then
        '            AddDocumentToDebug(extendedProjectedCurves, "extendedProjectedCurves")
        '#End If
        'DOPPIO TAGLIO DELLE CURVE PER CREARE LE SEZIONI DELLO SWEEP 1
        Dim sweepSections As New List(Of Guid)
        Dim sortedObj As New SortedDictionary(Of Double, MRhinoObjRef)
        Dim offsetSrfObjId As String = offsetSrfObj.Attributes.m_uuid.ToString()
        Dim borderSrfObjId As String = borderSrfObj.Attributes.m_uuid.ToString()
        Dim splitCmd As String = "-_Split _SelID " & offsetSrfObjId & " _SelID " & borderSrfObjId & " _Enter"
        Dim testpoint As On3dPoint = Nothing
        For Each curve As OnCurve In extendedProjectedCurves
            If curve Is Nothing OrElse Not curve.IsValid Then Continue For
            intersectionPoints = RhGeometry.IntersecaCurvaConSuperfice(curve, offsetSrf)
            If intersectionPoints Is Nothing OrElse intersectionPoints.Count <> 1 Then Continue For
            intersectionPoints = RhGeometry.IntersecaCurvaConSuperfice(curve, borderSrf)
            If intersectionPoints Is Nothing OrElse intersectionPoints.Count <> 1 Then Continue For
            Dim curveObj As MRhinoObject = RhUtil.RhinoApp.ActiveDoc.AddCurveObject(curve)
            RhUtil.RhinoApp.RunScript("_SelNone", 0)
            getObjects.ClearObjects()
            getObjects.SetGeometryFilter(IRhinoGetObject.GEOMETRY_TYPE_FILTER.curve_object)
            curveObj.Select(True, True)
            RhUtil.RhinoApp.RunScript(splitCmd, 0)
            RhUtil.RhinoApp.RunScript("_SelLast", 0)
            getObjects.GetObjects(0, Integer.MaxValue)
            If getObjects.ObjectCount = 3 Then
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
            End If
            RhUtil.RhinoApp.ActiveDoc.PurgeObject(curveObj)
        Next
        projectedCurves.Clear()
        extendedProjectedCurves.Clear()
        'ESTENDO LA CURVA DEL FERRO DI CAVALLO
        extensionLenght = OFFSET_PARTIAL_CUTOUT * 2
        RhUtil.RhinoExtendCurve(externalProjCurve, IRhinoExtend.Type.Line, 0, extensionLenght)
        RhUtil.RhinoExtendCurve(externalProjCurve, IRhinoExtend.Type.Line, 1, extensionLenght)
        RhUtil.RhinoApp.ActiveDoc.DeleteObject(New MRhinoObjRef(externalProjCrvObj.Attributes.m_uuid))
        externalProjCrvObj = RhUtil.RhinoApp.ActiveDoc.AddCurveObject(externalProjCurve)
        'TRIPLO TAGLIO DELLA CURVA DI BORDO
        Dim borderSweepSection1 As OnCurve = Nothing
        Dim borderSweepSection2 As OnCurve = Nothing
        getObjects.ClearObjects()
        RhUtil.RhinoApp.RunScript("_SelNone", 0)
        sortedObj.Clear()
        externalProjCrvObj.Select(True, True)
        RhUtil.RhinoApp.RunScript("-_Split _SelID " & borderSrfObjId & " _Enter", 0)
        RhUtil.RhinoApp.RunScript("_SelNone", 0)
        RhUtil.RhinoApp.RunScript("_SelLast", 0)
        getObjects.SetGeometryFilter(IRhinoGetObject.GEOMETRY_TYPE_FILTER.curve_object)
        getObjects.GetObjects(0, Integer.MaxValue)
        If getObjects.ObjectCount <> 3 Then Throw New Exception(LanguageManager.Message(382))
        'Filtro i 3 pezzi in base alla Y massima e minima del centro della bbox
        sortedObj.Add(getObjects.Object(0).Object.BoundingBox.Center.y, getObjects.Object(0))
        sortedObj.Add(getObjects.Object(1).Object.BoundingBox.Center.y, getObjects.Object(1))
        sortedObj.Add(getObjects.Object(2).Object.BoundingBox.Center.y, getObjects.Object(2))
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
        If getObjects.ObjectCount <> 2 Then Throw New Exception(LanguageManager.Message(383))
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
        If getObjects.ObjectCount <> 2 Then Throw New Exception(LanguageManager.Message(384))
        testpoint = New On3dPoint(getObjects.Object(0).Object.BoundingBox.Center)
        testpoint.z = centerSerie.z
        sortedObj.Add(testpoint.DistanceTo(centerSerie), getObjects.Object(0))
        testpoint = New On3dPoint(getObjects.Object(1).Object.BoundingBox.Center)
        testpoint.z = centerSerie.z
        sortedObj.Add(testpoint.DistanceTo(centerSerie), getObjects.Object(1))
        RhUtil.RhinoApp.ActiveDoc.DeleteObject(sortedObj.Values(1))
        sweepSections.Add(sortedObj.Values(0).m_uuid)
        borderSweepSection2 = sortedObj.Values(0).Curve.DuplicateCurve
        RhUtil.RhinoApp.RunScript("_SelNone", 0)
        'PULIZIA
        RhUtil.RhinoApp.ActiveDoc.PurgeObject(offsetSrfObj)
        RhUtil.RhinoApp.ActiveDoc.PurgeObject(borderSrfObj)
        offsetSrfObj.Dispose()
        borderSrfObj.Dispose()
        offsetSrf.Dispose()
        borderSrf.Dispose()
        RhUtil.RhinoApp.RunScript("_SelNone", 0)
        'SWEEP1 PER OTTENERE LA SUPERFICIE AGGIUNTIVA PER IL TAGLIO DI QUELLA LATERALE DEL PLANTARE
        Dim stringaSweep As String = "-_Sweep1"
        For Each stringID As Guid In sweepSections
            stringaSweep &= " _SelID " & stringID.ToString()
        Next
        If RhDocument.RhinoMajorRelease() = 5 Then stringaSweep &= " _Enter"
        If RhinoLanguageSetting() = IdLanguageManager.elanguage.English Then
            'Con _U=Yes obbligo a farla unita (in teoria è _C=Yes)
            stringaSweep &= " _Enter _S=Freeform _I=None _C=Yes _U=Yes _Enter"
        Else
            stringaSweep &= " _Enter _S=TorsioneLibera _E=Nessuno _C=S _U=S _Enter"
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
        Dim sweepSurface As OnSurface = sweepSurfaceRef.Surface.DuplicateSurface
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
        splittedCurves.Dispose()
        borderTrimmedInsoleTopCopy.Dispose()
        centerSerie.Dispose()
        testpoint.Dispose()
        segmentCrv1.Dispose()
        segmentCrv2.Dispose()

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

    Public Overrides Function Deserialize(ByRef archive As RMA.OpenNURBS.OnBinaryArchive) As Boolean
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
        Dim plainObject As IdAddiction = IdAddictionFactory.Create(Me.Side, Me.Type, Me.Model, Me.Size)
        MyBase.CloneCommonField(plainObject)
        Dim plainCutout As IdHorseShoeTotalAddiction = DirectCast(plainObject, IdHorseShoeTotalAddiction)
        'Campi specifici IdCutoutToTalAddiction
        MyBase.CloneCommonHorseField(plainCutout)
        Dim result As IdHorseShoePartialAddiction = DirectCast(plainCutout, IdHorseShoePartialAddiction)
        'Campi specifici IdCutoutPartialAddiction
        result.Depth = Me.Depth
        result.BlendSurfaceID = New Guid(Me.BlendSurfaceID.ToString)
        result.InsoleTopSrfCopyId = New Guid(Me.InsoleTopSrfCopyId.ToString)
        Return (result)
    End Function

#End Region


End Class
