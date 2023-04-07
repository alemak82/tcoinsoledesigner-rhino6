Imports System.IO
Imports RMA.OpenNURBS
Imports InsoleDesigner.bll.IdElement3dManager
Imports GeometryUtils = InsoleDesigner.bll.IdGeometryUtils
Imports InsoleDesigner.bll.IdAlias
Imports InsoleDesigner.bll.IdLanguageManager
Imports RhinoUtils
Imports RMA.Rhino


''*****************************************************************************************************************************************************************
''***   PER DISTINGUERE LE CURVE DELLO SCARICO HO USATO LA SEGUENTE NOMENCLATURA:                                                                               ***
''***   Le curve di base(base curves) sono le 4 contenute in un unico piano // a XY di cui                                                                      ***
''***       - Front è la curva che definisce la linea dei metatarsi, la più avanti nel piede                                                                    ***
''***       - Retro è la curva opposta che sta più indietro di tutte nel piede                                                                                  ***
''***       - Lateral maxY è quella laterale con Y maggiore, quindi la interna per il piede destro ed esterna per il sinistro                                   ***
''***       - La lateral minY è curva opposta alla lateral up, quindi la esterna per il piede destro ed interna per il sinistro                                 ***
''***   Le curve longitudinali sono tutte le curve tra quelle superiori(quindi escluse quelle di base) che seguono la lunghezza del piede                       ***
''***   Le curve trasversali sono tutte le curve tra quelle superiori(quindi escluse quelle di base) che seguono la larghezza del piede                         ***
''*****************************************************************************************************************************************************************


Public Class IdMetbarAddiction
    Inherits AbstractProjectableAddiction


#Region " ENUM "

    Public Enum eBaseCurve
        anterior
        posterior
        lateralMaxY
        lateralMinY
    End Enum

    Public Enum eMetbarSrf
        anterior
        posterior
    End Enum

#End Region


#Region " Const "

    'Parametro fondamentale per la ricostruzione della superficie finale del plantare - con 0.2 a volte + preciso ma molto + lento
    Private Const INTERPOLATED_CURVE_MAX_DISTANCE As Double = 0.5


#End Region


#Region " FIELD "


    Private mAnteriorBlendRail As OnCurve
    Private mPosteriorBlendRail As OnCurve
    Private mAnteriorExtrusionSrf As OnSurface
    Private mPosteriorExtrusionSrf As OnSurface
    Private mAnteriorBlendSrfID As Guid
    Private mPosteriorBlendSrfID As Guid
    Private mAnteriorTrimmedInsoleSrf As Guid
    Private mPosteriorTrimmedInsoleSrf As Guid


#End Region


#Region " Constructor "


    Public Sub New(ByVal side As IdElement3dManager.eSide, ByVal type As eAddictionType, ByVal model As eAddictionModel, ByVal size As eAddictionSize)
        MyBase.New(side, type, model, size)
    End Sub


#End Region


#Region " Property "

    ''' <summary>
    ''' Curve usate per fare il raccordo con la superficie del plantare
    ''' </summary>
    ''' <param name="blendSrf"></param>
    ''' <value></value>
    ''' <returns></returns>
    Public Property BlendRail(ByVal blendSrf As eMetbarSrf) As OnCurve
        Get
            If blendSrf = eMetbarSrf.anterior Then
                Return mAnteriorBlendRail
            Else
                Return mPosteriorBlendRail
            End If
        End Get
        Set(value As OnCurve)
            If blendSrf = eMetbarSrf.anterior Then
                mAnteriorBlendRail = value
            Else
                mPosteriorBlendRail = value
            End If
        End Set
    End Property


    Public Property ExtrusionSrf(ByVal blendSrf As eMetbarSrf) As OnSurface
        Get
            If blendSrf = eMetbarSrf.anterior Then
                Return mAnteriorExtrusionSrf
            Else
                Return mPosteriorExtrusionSrf
            End If
        End Get
        Set(value As OnSurface)
            If blendSrf = eMetbarSrf.anterior Then
                mAnteriorExtrusionSrf = value
            Else
                mPosteriorExtrusionSrf = value
            End If
        End Set
    End Property

    ''' <summary>
    ''' Superfici superiori del plantare memorizzate qui perchè IdElement3dHelper era costriuto per salvarne una sola
    ''' </summary>
    ''' <param name="blendSrf"></param>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TrimmedInsoleSrf(ByVal blendSrf As eMetbarSrf) As Guid
        Get
            If blendSrf = eMetbarSrf.anterior Then
                Return mAnteriorTrimmedInsoleSrf
            Else
                Return mPosteriorTrimmedInsoleSrf
            End If
        End Get
        Set(value As Guid)
            If blendSrf = eMetbarSrf.anterior Then
                mAnteriorTrimmedInsoleSrf = value
            Else
                mPosteriorTrimmedInsoleSrf = value
            End If
        End Set
    End Property


#End Region


#Region " Funzioni CAD "


    Public Function GetBaseCurveaRef(ByVal baseCurveName As eBaseCurve) As MRhinoObjRef
        Select Case baseCurveName
            Case eBaseCurve.anterior
                Return GetAnteriorCurveRef()
            Case eBaseCurve.posterior
                Return GetPosteriorCurveRef()
            Case eBaseCurve.lateralMaxY
                Return GetLateralMaxYCurveRef()
            Case eBaseCurve.lateralMinY
                Return GetLateralMinYCurveRef()
            Case Else
                Return Nothing
        End Select
    End Function


    ''' <summary>
    ''' Ritorno l'oggetto con X maggiore
    ''' </summary>
    ''' <returns></returns>
    Private Function GetAnteriorCurveRef() As MRhinoObjRef
        Dim maxX As Double = Double.MinValue
        Dim result As MRhinoObjRef = Nothing
        For Each objRef As MRhinoObjRef In Me.GetBaseCurvesObjRef()
            Dim maxPoint As On3dPoint = objRef.Curve().BoundingBox().m_max
            If maxPoint.x > maxX Then
                maxX = maxPoint.x
                result = objRef
            End If
            maxPoint.Dispose()
        Next
        Return result
    End Function

    ''' <summary>
    ''' Ritorno l'oggetto con X minore (NON SO COME FA A FUNZIONARE PERCHè NORMALEMENTE IL PUNTO MINORE E' IN COMUNE CON UN BORDO LATERALE
    ''' </summary>
    ''' <returns></returns>
    Private Function GetPosteriorCurveRef() As MRhinoObjRef
        Dim minX As Double = Double.MaxValue
        Dim result As MRhinoObjRef = Nothing
        For Each objRef As MRhinoObjRef In Me.GetBaseCurvesObjRef()
            Dim minPoint As On3dPoint = objRef.Curve().BoundingBox().m_min
            If minPoint.x < minX Then
                minX = minPoint.x
                result = objRef
            End If
            minPoint.Dispose()
        Next
        Return result
    End Function

    ''' <summary>
    ''' Tra i due oggetti che NON sono le curve "front" e "retro" ritorno quella con Y maggiore
    ''' </summary>
    ''' <returns></returns>
    Private Function GetLateralMaxYCurveRef() As MRhinoObjRef
        Dim maxY As Double = Double.MinValue
        Dim result As MRhinoObjRef = Nothing
        For Each objRef As MRhinoObjRef In Me.GetBaseCurvesObjRef()
            If Not objRef.m_uuid.Equals(GetAnteriorCurveRef().m_uuid) And Not objRef.m_uuid.Equals(GetPosteriorCurveRef().m_uuid) Then
                Dim maxPoint As On3dPoint = objRef.Curve().BoundingBox().m_max
                If maxPoint.y > maxY Then
                    maxY = maxPoint.y
                    result = objRef
                End If
                maxPoint.Dispose()
            End If
        Next
        Return result
    End Function

    ''' <summary>
    ''' Tra i due oggetti che NON sono le curve "front" e "retro" ritorno quella con Y minore
    ''' </summary>
    ''' <returns></returns>
    Private Function GetLateralMinYCurveRef() As MRhinoObjRef
        Dim minY As Double = Double.MaxValue
        Dim result As MRhinoObjRef = Nothing
        For Each objRef As MRhinoObjRef In Me.GetBaseCurvesObjRef()
            If Not objRef.m_uuid.Equals(GetAnteriorCurveRef().m_uuid) And Not objRef.m_uuid.Equals(GetPosteriorCurveRef().m_uuid) Then
                Dim minPoint As On3dPoint = objRef.Curve().BoundingBox().m_min
                If minPoint.y < minY Then
                    minY = minPoint.y
                    result = objRef
                End If
                minPoint.Dispose()
            End If

        Next
        Return result
    End Function


    ''' <summary>
    ''' Identifico l'estremo più vicino alla curva front e retro
    ''' </summary>
    ''' <param name="uuid"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overrides Function IsCurveLongitudinal(ByVal uuid As Guid) As Boolean
        Dim curve As IOnCurve = New MRhinoObjRef(uuid).Curve()
        Dim deltaDistance As Double = 0.1
        Dim tFront, tRetro As Double
        Dim frontBaseCurve As IOnCurve = Me.GetAnteriorCurveRef().Curve()
        Dim retroBaseCurve As IOnCurve = Me.GetPosteriorCurveRef().Curve()
        Dim testPointFront As On3dPoint = Nothing
        Dim testPointRetro As On3dPoint = Nothing
        If curve.PointAtStart().x > curve.PointAtEnd().x Then
            testPointFront = curve.PointAtStart()
            testPointRetro = curve.PointAtEnd()
        Else
            testPointFront = curve.PointAtEnd()
            testPointRetro = curve.PointAtStart()
        End If
        'Verifico che la distanza sia inferiore ad un delta
        frontBaseCurve.GetClosestPoint(testPointFront, tFront)
        retroBaseCurve.GetClosestPoint(testPointRetro, tRetro)
        Return (frontBaseCurve.PointAt(tFront).DistanceTo(testPointFront) < deltaDistance And retroBaseCurve.PointAt(tRetro).DistanceTo(testPointRetro) < deltaDistance)
        testPointFront.Dispose()
        testPointRetro.Dispose()
    End Function


    Public Function CheckMetbarPosition(ByVal lateralMaxYBaseCurve As IOnCurve, ByVal lateralMinYBaseCurve As IOnCurve, ByVal extrusionVector As On3dVector) As Boolean
        Dim insoleTopBrep As IOnBrep = IdElement3dManager.GetInstance.GetRhinoObjRef(IdElement3dManager.eReferences.insoleTopSurface, Me.Side).Brep()
        Dim lateralMaxYBaseBrep As OnBrep = RhUtil.RhinoExtrudeCurveStraight(lateralMaxYBaseCurve, extrusionVector).BrepForm()
        Dim lateralMinYBaseBrep As OnBrep = RhUtil.RhinoExtrudeCurveStraight(lateralMinYBaseCurve, extrusionVector).BrepForm()
        '#If DEBUG Then
        '        AddDocumentToDebug(lateralMaxYBaseBrep, "lateralMaxYBaseBrep")
        '        AddDocumentToDebug(lateralMinYBaseBrep, "lateralMinYBaseBrep")
        '#End If
        Dim curvesIntersection() As OnCurve = {}
        Dim pointsIntersection As New On3dPointArray
        'RhUtil.RhinoIntersectSurfaces(insoleTopSurface, lateralMaxYBaseSurface, 0.001, curvesIntersection, pointsIntersection)
        RhUtil.RhinoIntersectBreps(insoleTopBrep, lateralMaxYBaseBrep, 0.001, curvesIntersection, pointsIntersection)
        If curvesIntersection.Length > 0 Or pointsIntersection.Count > 0 Then Return False
        curvesIntersection = {}
        pointsIntersection = New On3dPointArray
        'RhUtil.RhinoIntersectSurfaces(insoleTopSurface, lateralMinYBasesurface, 0.001, curvesIntersection, pointsIntersection)
        RhUtil.RhinoIntersectBreps(insoleTopBrep, lateralMinYBaseBrep, 0.001, curvesIntersection, pointsIntersection)
        Return (curvesIntersection.Length = 0 And pointsIntersection.Count = 0)
    End Function


#End Region


#Region " Final Insole Suuface "

    ''' <summary>
    ''' La ricostruzione avviene secondo il principio di estrarre i bordi e le isocurve dalle superfici da "unire"
    ''' La funzione in fondo si divide in due metodologie che dipendono dal parametro:
    ''' Con lo script _NetworkSrf si usano le curve
    ''' Con l'API RhUtil.RhinoSrfPtGrid che usa i punti ricavati dalle stesse curve
    ''' </summary>
    ''' <param name="useScript_NetworkSrf">Parametro che indica quale tipo di ricostruzione eseguire</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CreateJoinSurface(Optional ByVal useScript_NetworkSrf As Boolean = False) As Boolean
        'Controlli
        If Me.SurfaceID = Nothing OrElse Me.SurfaceID = Guid.Empty Then Return False
        If New MRhinoObjRef(Me.SurfaceID).Surface Is Nothing Then Return False

        Dim metBarObjRef As New MRhinoObjRef(Me.SurfaceID)
        Dim anteriorTrimmedInsoleRef As New MRhinoObjRef(Me.mAnteriorTrimmedInsoleSrf)
        Dim posteriorTrimmedInsoleRef As New MRhinoObjRef(Me.mPosteriorTrimmedInsoleSrf)
        If anteriorTrimmedInsoleRef Is Nothing Or posteriorTrimmedInsoleRef Is Nothing Then Return False
        If anteriorTrimmedInsoleRef.Brep Is Nothing Or posteriorTrimmedInsoleRef.Brep Is Nothing Then Return False

        Dim metbarSurface As OnSurface = metBarObjRef.Surface.DuplicateSurface
        Dim anteriorBrep As OnBrep = anteriorTrimmedInsoleRef.Brep.BrepForm
        Dim posteriorBrep As OnBrep = posteriorTrimmedInsoleRef.Brep.BrepForm
        '#If DEBUG Then
        '        AddDocumentToDebug(anteriorBrep, "anteriorBrep")
        '        AddDocumentToDebug(posteriorBrep, "posteriorBrep")
        '        AddDocumentToDebug(metbarSurface, "metbarSurface")
        '#End If

        Dim Un As Integer = -1  'lo otterrò come numero di isocurve totali
        Dim Vn As Integer = 51

        Dim finalPosteriorBorder As OnCurve = Nothing
        Dim finalAnteriorBorder As OnCurve = Nothing
        Dim lateralBorderMaxY(2) As OnCurve
        Dim lateralBorderMinY(2) As OnCurve
        Dim finalLateralMaxYBorder As OnCurve = Nothing
        Dim finalLateralMinYBorder As OnCurve = Nothing
        Dim sortedCurves As New SortedList(Of Double, IOnCurve)
        Dim delta As Double = 0.000001

        'Estraggo bordi brep posteriore
        If posteriorBrep.m_E.Count <> 4 Then Return False
        For Each edge As OnBrepEdge In posteriorBrep.m_E
            Dim curve As IOnCurve = edge.NurbsCurve
            Dim key As Double = curve.BoundingBox.Center.x
            If sortedCurves.ContainsKey(key) Then key += delta
            sortedCurves.Add(key, curve)
        Next
        finalPosteriorBorder = sortedCurves.Values.Item(0).DuplicateCurve
        'Scarto curva anteriore e posteriore
        sortedCurves.RemoveAt(3)
        sortedCurves.RemoveAt(0)
        'Recupero i bordi laterali ordinando per centro della bbox in Y
        lateralBorderMinY(0) = (From entry In sortedCurves Order By entry.Value.BoundingBox.Center.y Ascending Select entry).First.Value.DuplicateCurve
        lateralBorderMaxY(0) = (From entry In sortedCurves Order By entry.Value.BoundingBox.Center.y Descending Select entry).First.Value.DuplicateCurve
        sortedCurves.Clear()

        'Estraggo bordi metbar
        If metbarSurface.BrepForm.m_E.Count <> 4 Then Return False
        For Each edge As OnBrepEdge In metbarSurface.BrepForm.m_E
            Dim curve As IOnCurve = edge.NurbsCurve
            Dim key As Double = curve.BoundingBox.Center.x
            If sortedCurves.ContainsKey(key) Then key += delta
            sortedCurves.Add(key, curve)
        Next
        'Scarto curva anteriore
        sortedCurves.RemoveAt(3)
        'Recupero i bordi laterali ordinando per centro della bbox in Y
        lateralBorderMinY(1) = (From entry In sortedCurves Order By entry.Value.BoundingBox.Center.y Ascending Select entry).First.Value.DuplicateCurve
        lateralBorderMaxY(1) = (From entry In sortedCurves Order By entry.Value.BoundingBox.Center.y Descending Select entry).First.Value.DuplicateCurve
        sortedCurves.Clear()

        'Estraggo bordi brep anteriore
        'Estraggo bordi brep posteriore
        If anteriorBrep.m_E.Count <> 4 Then Return False
        For Each edge As OnBrepEdge In anteriorBrep.m_E
            Dim curve As IOnCurve = edge.NurbsCurve
            Dim key As Double = curve.BoundingBox.Center.x
            If sortedCurves.ContainsKey(key) Then key += delta
            sortedCurves.Add(key, curve)
        Next
        finalAnteriorBorder = sortedCurves.Values.Item(3).DuplicateCurve
        'Scarto curva anteriore
        sortedCurves.RemoveAt(3)
        'Recupero i bordi laterali ordinando per centro della bbox in Y
        lateralBorderMinY(2) = (From entry In sortedCurves Order By entry.Value.BoundingBox.Center.y Ascending Select entry).First.Value.DuplicateCurve
        lateralBorderMaxY(2) = (From entry In sortedCurves Order By entry.Value.BoundingBox.Center.y Descending Select entry).First.Value.DuplicateCurve
        sortedCurves.Clear()

        'Unisco bordi laterali        
        Dim mergerCurves() As OnCurve = {}
        If Not RhUtil.RhinoMergeCurves(lateralBorderMaxY, mergerCurves) Then Return False
        If mergerCurves.Length <> 1 Then Return False
        finalLateralMaxYBorder = mergerCurves(0).DuplicateCurve
        mergerCurves = {}
        If Not RhUtil.RhinoMergeCurves(lateralBorderMinY, mergerCurves) Then Return False
        If mergerCurves.Length <> 1 Then Return False
        finalLateralMinYBorder = mergerCurves(0).DuplicateCurve
        '#If DEBUG Then
        '        AddDocumentToDebug(finalPosteriorBorder, "finalPosteriorBorder")
        '        AddDocumentToDebug(finalLateralMaxYBorder, "finalLateralMaxYBorder")
        '        AddDocumentToDebug(finalLateralMinYBorder, "finalLateralMinYBorder")
        '        AddDocumentToDebug(finalAnteriorBorder, "finalAnteriorBorder")
        '#End If

        ''CREO LISTA CURVE DI COSTRUZIONE
        sortedCurves.Clear()
        'Aggiungo le curve di bordo anteriore e posteriore
        sortedCurves.Add(finalPosteriorBorder.BoundingBox.m_min.x, finalPosteriorBorder)
        sortedCurves.Add(finalAnteriorBorder.BoundingBox.m_min.x, finalAnteriorBorder)

        ''ESTRAGGO LE ISOCURVE          
        'Isocurve superficie posteriore
        Dim posteriorIsocurves As SortedList(Of Double, OnCurve) = GetBrepIsocurves(eBrepIsocurves.posterior, finalLateralMinYBorder, finalLateralMaxYBorder)
        If posteriorIsocurves.Count = 0 Then Return False
        For Each curve As OnCurve In posteriorIsocurves.Values
            Dim key As Double = curve.BoundingBox.m_min.x
            If Not sortedCurves.ContainsKey(key) Then sortedCurves.Add(key, curve)
        Next
        'Isocurve superficie metbar
        Dim metbarIsocurves As SortedList(Of Double, OnCurve) = GetBrepIsocurves(eBrepIsocurves.metbar, finalLateralMinYBorder, finalLateralMaxYBorder)
        If metbarIsocurves.Count = 0 Then Return False
        For Each curve As OnCurve In metbarIsocurves.Values
            Dim key As Double = curve.BoundingBox.m_min.x
            If Not sortedCurves.ContainsKey(key) Then sortedCurves.Add(key, curve)
        Next
        'Isocurve superficie anteriore
        Dim anteriorIsocurves As SortedList(Of Double, OnCurve) = GetBrepIsocurves(eBrepIsocurves.anterior, finalLateralMinYBorder, finalLateralMaxYBorder)
        If anteriorIsocurves.Count = 0 Then Return False
        For Each curve As OnCurve In anteriorIsocurves.Values
            Dim key As Double = curve.BoundingBox.m_min.x
            If Not sortedCurves.ContainsKey(key) Then sortedCurves.Add(key, curve)
        Next
        '#If DEBUG Then
        '        Dim isocurves(sortedCurves.Count - 1) As OnCurve
        '        sortedCurves.Values.CopyTo(isocurves, 0)
        '        AddDocumentToDebug(isocurves, "isocurves")
        '#End If


        'Curve create manualmente all'inizio superficie posteriore
        Dim curve1 As OnCurve = finalPosteriorBorder
        Dim curve2 As OnCurve = posteriorIsocurves.Values(0)
        For Each curve As OnCurve In GetInterpolatedCurves(curve1, curve2, posteriorBrep).Values
            Dim key As Double = curve.BoundingBox.m_min.x
            If Not sortedCurves.ContainsKey(key) Then sortedCurves.Add(key, curve)
        Next
        'Curve create manualmente alla fine della superficie posteriore
        curve1 = posteriorIsocurves.Values(posteriorIsocurves.Count - 1)
        curve2 = metbarIsocurves.Values(0)
        For Each curve As OnCurve In GetInterpolatedCurves(curve1, curve2, posteriorBrep).Values
            Dim key As Double = curve.BoundingBox.m_min.x
            If Not sortedCurves.ContainsKey(key) Then sortedCurves.Add(key, curve)
        Next
        'Curve create manualmente sulla superficie della barra
        curve1 = metbarIsocurves.Values(0)
        curve2 = metbarIsocurves.Values(1)
        For Each curve As OnCurve In GetInterpolatedCurves(curve1, curve2, metbarSurface.BrepForm).Values
            Dim key As Double = curve.BoundingBox.m_min.x
            If Not sortedCurves.ContainsKey(key) Then sortedCurves.Add(key, curve)
        Next
        curve1 = metbarIsocurves.Values(metbarIsocurves.Count - 2)
        curve2 = metbarIsocurves.Values(metbarIsocurves.Count - 1)
        For Each curve As OnCurve In GetInterpolatedCurves(curve1, curve2, metbarSurface.BrepForm).Values
            Dim key As Double = curve.BoundingBox.m_min.x
            If Not sortedCurves.ContainsKey(key) Then sortedCurves.Add(key, curve)
        Next
        'Curve create manualmente all'inizio superficie anteriore
        curve1 = metbarIsocurves.Values(metbarIsocurves.Count - 1)
        curve2 = anteriorIsocurves.Values(0)
        For Each curve As OnCurve In GetInterpolatedCurves(curve1, curve2, anteriorBrep).Values
            Dim key As Double = curve.BoundingBox.m_min.x
            If Not sortedCurves.ContainsKey(key) Then sortedCurves.Add(key, curve)
        Next
        'Curve create manualmente alla fine superficie anteriore
        curve1 = anteriorIsocurves.Values(anteriorIsocurves.Count - 1)
        curve2 = finalAnteriorBorder
        For Each curve As OnCurve In GetInterpolatedCurves(curve1, curve2, anteriorBrep).Values
            Dim key As Double = curve.BoundingBox.m_min.x
            If Not sortedCurves.ContainsKey(key) Then sortedCurves.Add(key, curve)
        Next

        ''RENDO UNIFORME LA DIREZIONE CURVE
        For Each curve As OnCurve In sortedCurves.Values
            Dim curveDirection As On3dVector = curve.PointAtEnd - curve.PointAtStart
            If curveDirection.y < 0 Then curve.Reverse()
        Next

        '#If DEBUG Then
        '        Dim isocurves(sortedCurves.Count - 1) As OnCurve
        '        sortedCurves.Values.CopyTo(isocurves, 0)
        '        AddDocumentToDebug(isocurves, "isocurves")
        '#End If

        Dim layerName As String = IdElement3dManager.GetLayerName(Me.Side, IdElement3dManager.eLayerType.insole)
        If useScript_NetworkSrf Then
            ''---------------------------------------- CREAZIONE CON _NetworkSrf ------------------------------------------------------------
            Me.CurvesID.Clear()
            For Each curve As OnCurve In sortedCurves.Values
                CurvesID.Add(Doc.AddCurveObject(curve).Attributes.m_uuid)
            Next
            CurvesID.Add(Doc.AddCurveObject(finalLateralMaxYBorder).Attributes.m_uuid)
            CurvesID.Add(Doc.AddCurveObject(finalLateralMinYBorder).Attributes.m_uuid)
            Me.CreateSrfFromCurves(layerName)
            Me.DeleteAllCurves()
            'Aggiorno ID
            Dim surfaceGuid As Guid = New Guid(Me.SurfaceID.ToString)
            IdElement3dManager.GetInstance.SetRhinoObj(IdElement3dManager.eReferences.insoleTopSurface, Me.Side, surfaceGuid)
            ''-------------------------------------------------------------------------------------------------------------------------------------------
        Else
            ''---------------------------------------- CREAZIONE MEDIANTE GRIGLIA DI PUNTI ------------------------------------------------------------            
            'TROVO I PUNTI
            Dim gridPoints As New On3dPointArray
            For Each curve As OnCurve In sortedCurves.Values
                'Creo Vn punti equidistanti su ogni curva
                Dim points As New ArrayOn3dPoint
                Dim pointsDomain As New Arraydouble
                If Not RhUtil.RhinoDivideCurve(curve, Vn - 1, 0, False, True, points, pointsDomain) Then Return False
                'Ordino i punti in base al dominio
                Dim sortedPoints As New SortedList(Of Double, On3dPoint)
                For i As Integer = 0 To points.Count - 1
                    sortedPoints.Add(pointsDomain.Item(i), points.Item(i))
                Next
                For Each point As On3dPoint In sortedPoints.Values
                    gridPoints.Append(point)
                Next
                pointsDomain.Dispose()
            Next
            '#If DEBUG Then
            '            AddDocumentToDebug(gridPoints, "gridPoints")
            '#End If

            Un = sortedCurves.Count
            Dim surface As OnNurbsSurface = RhUtil.RhinoSrfPtGrid(New Integer() {Un, Vn}, New Integer() {2, 2}, New Boolean() {False, False}, gridPoints)
            RhLayer.RendiCorrenteLayer(layerName)
            IdElement3dManager.GetInstance.SetRhinoObj(IdElement3dManager.eReferences.insoleTopSurface, Me.Side, Doc.AddSurfaceObject(surface).Attributes.m_uuid)
            ''-------------------------------------------------------------------------------------------------------------------------------------------
        End If
        Me.SurfaceID = Guid.Empty

        'Dispose
        anteriorTrimmedInsoleRef.Dispose()
        posteriorTrimmedInsoleRef.Dispose()
        metBarObjRef.Dispose()
        metbarSurface.Dispose()
        anteriorBrep.Dispose()
        posteriorBrep.Dispose()

        Return True
    End Function


    Private Enum eBrepIsocurves
        anterior
        metbar
        posterior
    End Enum


    Private Function GetBrepIsocurves(ByVal brepIsocurves As eBrepIsocurves, ByRef border1 As OnCurve, ByRef border2 As OnCurve) As SortedList(Of Double, OnCurve)
        Dim result As New SortedList(Of Double, OnCurve)

        App.RunScript("_SelNone", 0)
        'Seleziono brep da cui estrarre le isocurve
        Select Case brepIsocurves
            Case eBrepIsocurves.posterior
                Dim posteriorTrimmedInsoleRef As New MRhinoObjRef(Me.mPosteriorTrimmedInsoleSrf)
                posteriorTrimmedInsoleRef.Object.Select(True, True)
            Case eBrepIsocurves.metbar
                Dim metBarObjRef As New MRhinoObjRef(Me.SurfaceID)
                metBarObjRef.Object.Select(True, True)
            Case eBrepIsocurves.anterior
                Dim anteriorTrimmedInsoleRef As New MRhinoObjRef(Me.mAnteriorTrimmedInsoleSrf)
                anteriorTrimmedInsoleRef.Object.Select(True, True)
        End Select

        Dim allIsocurves As New OnCurveArray
        Dim getObjects As New MRhinoGetObject
        getObjects.SetGeometryFilter(IRhinoGetObject.GEOMETRY_TYPE_FILTER.curve_object)
        Dim commandExtractIsocurve As String = ""
        If IdLanguageManager.RhinoLanguageSetting = IdLanguageManager.elanguage.Italian Then
            commandExtractIsocurve = "EstraiIsocurve _D _E _I=N _S _Enter"
        Else
            commandExtractIsocurve = "_ExtractIsocurve _D _B _I=N _X _Enter"
        End If
        App.RunScript(commandExtractIsocurve, 0)
        App.RunScript("_SelLast", 0)
        If getObjects.GetObjects(0, Integer.MaxValue) <> IRhinoGet.result.object OrElse getObjects.ObjectCount = 0 Then Return result
        For i As Integer = 0 To getObjects.ObjectCount - 1
            allIsocurves.Append(getObjects.Object(i).Curve.DuplicateCurve)
            Doc.DeleteObject(getObjects.Object(i))
        Next
        App.RunScript("_SelNone", 0)
        '#If DEBUG Then
        '        AddDocumentToDebug(allIsocurves, "allIsocurves")
        '#End If

        'Filtro isocurve -> devono avere gli estremi sulle curve due curve di bordo laterali
        Dim testDistance As Double = 0.001
        Dim startPoint, endPoint, closestPoint1, closestPoint2 As On3dPoint
        For Each curve As OnCurve In allIsocurves
            'Start point
            startPoint = curve.PointAtStart()
            Dim t1, t2 As Double
            border1.GetClosestPoint(startPoint, t1)
            border2.GetClosestPoint(startPoint, t2)
            closestPoint1 = border1.PointAt(t1)
            closestPoint2 = border2.PointAt(t2)
            Dim distanceStart1 As Double = startPoint.DistanceTo(closestPoint1)
            Dim distanceStart2 As Double = startPoint.DistanceTo(closestPoint2)
            startPoint.Dispose()
            If Math.Min(distanceStart1, distanceStart2) > testDistance Then Continue For
            'End point
            endPoint = curve.PointAtEnd()
            border1.GetClosestPoint(endPoint, t1)
            border2.GetClosestPoint(endPoint, t2)
            closestPoint1 = border1.PointAt(t1)
            closestPoint2 = border2.PointAt(t2)
            Dim distanceEnd1 As Double = endPoint.DistanceTo(closestPoint1)
            Dim distanceEnd2 As Double = endPoint.DistanceTo(closestPoint2)
            endPoint.Dispose()
            closestPoint1.Dispose()
            closestPoint2.Dispose()
            If Math.Min(distanceEnd1, distanceEnd2) > testDistance Then Continue For
            'Start end End point
            If Math.Max(distanceStart1, distanceEnd1) < testDistance Then Continue For
            If Math.Max(distanceStart2, distanceEnd2) < testDistance Then Continue For
            Dim key As Double = curve.BoundingBox.m_min.x
            If Not result.ContainsKey(key) Then result.Add(key, curve.DuplicateCurve)
            '#If DEBUG Then
            '            AddDocumentToDebug(curve, "Isocurves " & brepIsocurves.ToString)
            '#End If
        Next

        'Dispose
        getObjects.Dispose()
        allIsocurves.Dispose()

        Return result
    End Function


    Private Function GetInterpolatedCurves(ByRef curve1 As OnCurve, ByRef curve2 As OnCurve, ByRef originalBrep As OnBrep) As SortedList(Of Double, OnCurve)
        Dim result As New SortedList(Of Double, OnCurve)

        'Controlli
        If curve1 Is Nothing Or curve2 Is Nothing Then Return result
        If curve2.PointAtStart.x < curve1.PointAtStart.x Or curve2.PointAtStart.x < curve1.PointAtEnd.x Then Return result
        If curve2.PointAtEnd.x < curve1.PointAtStart.x Or curve2.PointAtEnd.x < curve1.PointAtEnd.x Then Return result

        'Calcolo massima distanza estremi
        Dim curve1MaxY, curve1MinY, curve2MaxY, curve2MinY As On3dPoint
        If curve1.PointAtStart.y > curve1.PointAtEnd.y Then
            curve1MaxY = curve1.PointAtStart
            curve1MinY = curve1.PointAtEnd
        Else
            curve1MaxY = curve1.PointAtEnd
            curve1MinY = curve1.PointAtStart
        End If
        If curve2.PointAtStart.y > curve2.PointAtEnd.y Then
            curve2MaxY = curve2.PointAtStart
            curve2MinY = curve2.PointAtEnd
        Else
            curve2MaxY = curve2.PointAtEnd
            curve2MinY = curve2.PointAtStart
        End If
        Dim curveDistance As Double = Math.Max(curve1MaxY.DistanceTo(curve2MaxY), curve1MinY.DistanceTo(curve2MinY))
        ''Calcolo numero curve da creare in base alla distanza - impongo una curva ogni 0.5 millimetri (con 0.2 a volte + preciso ma molto + lento)
        Dim numberOfCurve As Integer = CInt(Math.Floor(curveDistance / INTERPOLATED_CURVE_MAX_DISTANCE))

        '#If DEBUG Then
        '        AddDocumentToDebug(curve1MaxY, "curve1MaxY")
        '        AddDocumentToDebug(curve1MinY, "curve1MinY")
        '        AddDocumentToDebug(curve2MaxY, "curve2MaxY")
        '        AddDocumentToDebug(curve2MinY, "curve2MinY")
        '        AddDocumentToDebug(curve1, "curve1")
        '        AddDocumentToDebug(curve2, "curve2")
        '#End If

        'AGGIUNGO CURVE AL DOC PER SCRITP CURVE INTERMEDIE
        Dim curve1Obj As MRhinoObject = Doc.AddCurveObject(curve1)
        Dim curve2Obj As MRhinoObject = Doc.AddCurveObject(curve2)

        'Creo le curve intermedie       
        App.RunScript("_SelNone", 0)
        curve1Obj.Select(True)
        curve2Obj.Select(True)
        Dim cmd As String = "_TweenCurves "
        If IdLanguageManager.RhinoLanguageSetting = IdLanguageManager.elanguage.Italian Then
            cmd &= "_N=" & numberOfCurve & " _M _P _U=100 _Enter"
        Else
            cmd &= "_N=" & numberOfCurve & " _M _S _S=100 _Enter"
        End If
        App.RunScript(cmd, 0)

        'Parametri estrusione
        Dim extrudeLenght As Double = 100
        Dim extrusionVector As New On3dVector(0, 0, extrudeLenght)
        Dim xform As New OnXform
        xform.Translation(0, 0, -extrudeLenght / 2)

        'Seleziono curve
        App.RunScript("_SelLast", 0)
        Dim getObjects As New MRhinoGetObject
        getObjects.SetGeometryFilter(IRhinoGetObject.GEOMETRY_TYPE_FILTER.curve_object)
        If getObjects.GetObjects(0, Integer.MaxValue) <> IRhinoGet.result.object OrElse getObjects.ObjectCount = 0 Then Return result
        'Scorro curve intermedie
        For i As Integer = 0 To getObjects.ObjectCount - 1
            'Estendo le curve, creo le superfici per estrusione e interseco con il brep
            Dim curve As OnCurve = getObjects.Object(i).Curve.DuplicateCurve
            Doc.DeleteObject(getObjects.Object(i))
            ''ESTENSIONE
            RhUtil.RhinoExtendCurve(curve, IRhinoExtend.Type.Line, 0, 10)
            RhUtil.RhinoExtendCurve(curve, IRhinoExtend.Type.Line, 1, 10)
            '#If DEBUG Then
            '        AddDocumentToDebug(curve, "middleCurve extended")
            '#End If
            ''ESTRUSIONE
            Dim middleSrf As OnSurface = RhUtil.RhinoExtrudeCurveStraight(curve, extrusionVector)
            middleSrf.Transform(xform)
            '#If DEBUG Then
            '            AddDocumentToDebug(middleSrf, "middleSrf")
            '#End If
            ''INTERSEZIONE
            Dim intersectionCurves() As OnCurve = {}
            RhUtil.RhinoIntersectBreps(originalBrep, middleSrf.BrepForm, Doc.AbsoluteTolerance, intersectionCurves, New On3dPointArray)
            middleSrf.Dispose()
            If intersectionCurves.Length <> 1 Then Continue For
            Dim newCurve As OnCurve = intersectionCurves(0) '.DuplicateCurve
            Dim key As Double = newCurve.BoundingBox.m_min.x
            If Not result.ContainsKey(key) Then result.Add(key, newCurve)
            '#If DEBUG Then
            '            AddDocumentToDebug(newCurve, "newCurve " & i.ToString)
            '#End If
        Next

        'Pulizia
        Doc.PurgeObject(curve1Obj)
        Doc.PurgeObject(curve2Obj)
        curve1Obj.Dispose()
        curve2Obj.Dispose()
        xform.Dispose()
        extrusionVector.Dispose()

        Return result
    End Function



#End Region


#Region " Serializzazione/deserializzazione"


    Public Overrides Function Serialize(ByRef archive As OnBinaryArchive) As Boolean
        If Not MyBase.CommonSerialize(archive) Then Return False

        If Not archive.WriteUuid(mAnteriorBlendSrfID) Then Return False
        If Not archive.WriteUuid(mPosteriorBlendSrfID) Then Return False
        If Not archive.WriteUuid(mAnteriorTrimmedInsoleSrf) Then Return False
        If Not archive.WriteUuid(mPosteriorTrimmedInsoleSrf) Then Return False

        Return True
    End Function


    Public Overrides Function Deserialize(ByRef archive As OnBinaryArchive) As Boolean

        Me.mAnteriorBlendSrfID = New Guid
        If Not archive.ReadUuid(Me.mAnteriorBlendSrfID) Then Return False
        Me.mPosteriorBlendSrfID = New Guid
        If Not archive.ReadUuid(Me.mPosteriorBlendSrfID) Then Return False
        Me.mAnteriorTrimmedInsoleSrf = New Guid
        If Not archive.ReadUuid(Me.mAnteriorTrimmedInsoleSrf) Then Return False
        Me.mPosteriorTrimmedInsoleSrf = New Guid
        If Not archive.ReadUuid(Me.mPosteriorTrimmedInsoleSrf) Then Return False

        Return True
    End Function


#End Region


#Region " IClonable "


    Public Overrides Function Clone() As Object
        Dim plainObject As IdAddiction = IdAddictionFactory.Create(Me.Side, Me.Type, Me.Model, Me.Size)
        MyBase.CloneCommonField(plainObject)
        Dim result As IdMetbarAddiction = DirectCast(plainObject, IdMetbarAddiction)
        'Campi specifici metbar
        If Me.mAnteriorBlendRail IsNot Nothing AndAlso Me.mAnteriorBlendRail.InternalPointer <> IntPtr.Zero Then
            result.mAnteriorBlendRail = Me.mAnteriorBlendRail.DuplicateCurve
        End If
        If Me.mPosteriorBlendRail IsNot Nothing AndAlso Me.mPosteriorBlendRail.InternalPointer <> IntPtr.Zero Then
            result.mPosteriorBlendRail = Me.mPosteriorBlendRail.DuplicateCurve
        End If
        If Me.mAnteriorExtrusionSrf IsNot Nothing AndAlso Me.mAnteriorExtrusionSrf.InternalPointer <> IntPtr.Zero Then
            result.mAnteriorExtrusionSrf = Me.mAnteriorExtrusionSrf.DuplicateSurface
        End If
        If Me.mPosteriorExtrusionSrf IsNot Nothing AndAlso Me.mPosteriorExtrusionSrf.InternalPointer <> IntPtr.Zero Then
            result.mPosteriorExtrusionSrf = Me.mPosteriorExtrusionSrf.DuplicateSurface
        End If
        result.mAnteriorBlendSrfID = New Guid(Me.mAnteriorBlendSrfID.ToString)
        result.mPosteriorBlendSrfID = New Guid(Me.mPosteriorBlendSrfID.ToString)
        result.mAnteriorTrimmedInsoleSrf = New Guid(Me.mAnteriorTrimmedInsoleSrf.ToString)
        result.mPosteriorTrimmedInsoleSrf = New Guid(Me.mPosteriorTrimmedInsoleSrf.ToString)
        Return result
    End Function


#End Region


#Region " Overrides "


    Public Overrides Function AddictionCanBeAdded(ByRef errorMessage As String) As Boolean
        If IdElement3dManager.GetInstance.GetAddictions(Me.Side).Count > 0 Then
            errorMessage = LanguageManager.Message(0, Me.Type)
            Return False
        Else
            Return True
        End If        
    End Function


#Region " VECCHIA GESTIONE CON DOPPIO RACCORDO "

    'Public Overrides Sub DeleteBlendSrf()
    '    If IsBlendSrfInDocument(eBlendSrf.anterior) Then RhUtil.RhinoApp.ActiveDoc.DeleteObject(New MRhinoObjRef(BlendSurfaceID(eBlendSrf.anterior)))
    '    If IsBlendSrfInDocument(eBlendSrf.posterior) Then RhUtil.RhinoApp.ActiveDoc.DeleteObject(New MRhinoObjRef(BlendSurfaceID(eBlendSrf.posterior)))
    'End Sub


    'Public Overrides Sub SelectBlendSrf()
    '    If AreBlendSrfInDocument() Then
    '        RhUtil.RhinoApp().RunScript("_PointsOff", 0)
    '        Dim rhinoObjRef As New MRhinoObjRef(BlendSurfaceID(eBlendSrf.anterior))
    '        rhinoObjRef.Object.Select(True, True)
    '        rhinoObjRef = New MRhinoObjRef(BlendSurfaceID(eBlendSrf.posterior))
    '        rhinoObjRef.Object.Select(True, True)
    '        rhinoObjRef.Dispose()
    '    End If
    'End Sub

    '''' <summary>
    '''' Funzione specifica per la barra metatarsale
    '''' </summary>
    '''' <param name="blendSrf"></param>
    '''' <returns></returns>
    '''' <remarks></remarks>
    'Public Overloads Function IsBlendSrfInDocument(ByVal blendSrf As eBlendSrf) As Boolean
    '    Return (RhUtil.RhinoApp.ActiveDoc.LookupObject(BlendSurfaceID(blendSrf)) IsNot Nothing)
    'End Function

    'Public Function AreBlendSrfInDocument() As Boolean
    '    If RhUtil.RhinoApp.ActiveDoc.LookupObject(BlendSurfaceID(eBlendSrf.anterior)) IsNot Nothing And _
    '        RhUtil.RhinoApp.ActiveDoc.LookupObject(BlendSurfaceID(eBlendSrf.posterior)) IsNot Nothing Then Return True
    '    Return False
    'End Function

#End Region


#End Region


End Class
