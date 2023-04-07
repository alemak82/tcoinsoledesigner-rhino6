Imports RMA.Rhino
Imports RMA.OpenNURBS
Imports RhinoUtils
Imports System.IO
Imports InsoleDesigner.bll.IdAlias
Imports InsoleDesigner.bll.IdLanguageManager
Imports InsoleDesigner.bll.IdElement3dManager
Imports InsoleDesigner.bll.IdGeometryUtils
Imports InsoleDesigner.bll.IdAddiction


Public MustInherit Class AbstractCutoutCommons
    Inherits IdAddiction



#Region " ENUM "

    Public Enum eCutoutDirection
        internal
        external
        none
    End Enum

    Public Enum eCutoutCurve
        L1
        L2
        L1L2
    End Enum

    Enum eHorseShoeCrv
        L1
        L2
        L3
        L4
        L1L2
        L1L3
        L2L4
        Unique
    End Enum

    Enum eHorseShoeStraightCrv
        L1
        L2
        L3
        L4
    End Enum

    Enum eHorseShoeFilletCrv
        L1L2
        L1L3
        L2L4
    End Enum

    Enum eTrimmedSrf
        top
        lateral
        bottom
    End Enum


    Public Enum eTopSurface
        original
        copy
    End Enum


#End Region


#Region " Conversioni "



    Public Shared Function ConvertCutoutCrv(ByVal curve As eHorseShoeCrv) As eCutoutCurve
        Select Case curve
            Case eHorseShoeCrv.L1
                Return eCutoutCurve.L1
            Case eHorseShoeCrv.L2
                Return eCutoutCurve.L2
            Case eHorseShoeCrv.L1L2
                Return eCutoutCurve.L1L2
            Case Else
                Return CType(-1, eCutoutCurve)
        End Select
    End Function

    Public Shared Function ConvertCutoutCrv(ByVal curve As eCutoutCurve) As eHorseShoeCrv
        Select Case curve
            Case eCutoutCurve.L1
                Return eHorseShoeCrv.L1
            Case eCutoutCurve.L2
                Return eHorseShoeCrv.L2
            Case eCutoutCurve.L1L2
                Return eHorseShoeCrv.L1L2
            Case Else
                Return CType(-1, eHorseShoeCrv)
        End Select
    End Function

    Shared Function ConvertCutoutCurve(ByVal curve As eCutoutCurve) As eHorseShoeStraightCrv
        Select Case curve
            Case eCutoutCurve.L1
                Return eHorseShoeStraightCrv.L1
            Case eCutoutCurve.L2
                Return eHorseShoeStraightCrv.L2
            Case Else
                Return CType(-1, eHorseShoeStraightCrv)
        End Select
    End Function

    Shared Function ConvertCutoutCrv(ByVal curve As eHorseShoeStraightCrv) As eCutoutCurve
        Select Case curve
            Case eHorseShoeStraightCrv.L1
                Return eCutoutCurve.L1
            Case eHorseShoeStraightCrv.L2
                Return eCutoutCurve.L2
            Case Else
                Return CType(-1, eCutoutCurve)
        End Select
    End Function

    Shared Function ConvertEnumCurve(ByVal curve As eHorseShoeStraightCrv) As eHorseShoeCrv
        Return CType(curve, eHorseShoeCrv)
    End Function

    Shared Function ConvertEnumCurve(ByVal curve As eHorseShoeCrv) As eHorseShoeStraightCrv
        Return CType(curve, eHorseShoeStraightCrv)
    End Function

    Shared Function ConvertEnumCurve(ByVal curve As eHorseShoeFilletCrv) As eHorseShoeCrv
        Return CType(CInt(curve) + 4, eHorseShoeCrv)
    End Function

    Shared Function ConvertEnumCrv(ByVal curve As eHorseShoeCrv) As eHorseShoeFilletCrv
        Return CType(CInt(curve) - 4, eHorseShoeFilletCrv)
    End Function


#End Region


#Region " Funzioni CAD "


    Public Shared Sub CreateCutoutPartialSurface(ByRef sweepSurface As OnSurface, ByRef extrusionCurveCutoutSrfRef As MRhinoObjRef,
                                       ByRef lateralSrfToDropRef As MRhinoObjRef, ByRef insoleSrfRefLateral As MRhinoObjRef,
                                       ByRef trimmedInsoleTopCopy As MRhinoObjRef, ByRef backupInsoleTopCopyBrep As OnBrep, ByRef currentAddiction As IdAddiction)

        Dim intersectionCurves() As OnCurve = {}
        Dim intersectionPoints As New On3dPointArray
        Dim mergedCurves() As OnCurve = {}
        Dim xform As New OnXform
        Dim getObjects As New MRhinoGetObject
        Dim sortedObj As New SortedDictionary(Of Double, MRhinoObjRef)
        Dim filter() As IRhinoGetObject.GEOMETRY_TYPE_FILTER = Nothing
        Dim extensionLenght As Double = 0

        'SPLIT DELLA SUPERFICIE LATERALE CON LA SUPERICIE ESTRUSA DALLE CURVE DI CUTOUT
        Dim srfToSplitForLateralRef As MRhinoObjRef = Nothing
        Dim bbox1, bbox2 As New OnBoundingBox
        Dim lateralSrfPieces As New MRhinoObjRefArray
        App.RunScript("_SelNone", 0)
        getObjects.ClearObjects()
        sortedObj.Clear()
        insoleSrfRefLateral.Object.Select(True, True)
        Dim cutterId As String = extrusionCurveCutoutSrfRef.ObjectUuid.ToString
        App.RunScript("-_Split _SelID " & cutterId & " _Enter", 0)
        getObjects.GetObjects(0, Integer.MaxValue)
        If getObjects.ObjectCount <> 2 And getObjects.ObjectCount <> 3 Then Throw New Exception(LanguageManager.Message(352))
        For i As Integer = 0 To getObjects.ObjectCount - 1
            getObjects.Object(i).Object.GetTightBoundingBox(bbox1)
            If sortedObj.ContainsKey(bbox1.m_min.x) Then
                sortedObj.Add(bbox1.m_min.x + Integer.MinValue, getObjects.Object(i))
            Else
                sortedObj.Add(bbox1.m_min.x, getObjects.Object(i))
            End If
        Next
        ''In questo caso lo split mi generava sempre 3 superfici - DOPO IMPORTAZIONE A VOLTE 2
        If getObjects.ObjectCount = 2 Then
            'DELLE 2 SUPERFICI RISULTANTI, QUELLA CON MIN.X BBOX MINORE E' UNA DELLE PARTI DELLA SUPERFICIE LATERALE
            lateralSrfPieces.Append(sortedObj.Values(0))
            srfToSplitForLateralRef = sortedObj.Values(1)
        ElseIf getObjects.ObjectCount = 3 Then
            'DELLE 3 SUPERFICI RISULTANTI, LE 2 CON MIN.X BBOX MINORE COME PARTI DELLA SUPERFICIE LATERALE
            lateralSrfPieces.Append(sortedObj.Values(0))
            lateralSrfPieces.Append(sortedObj.Values(1))
            srfToSplitForLateralRef = sortedObj.Values(2)
        End If
        App.RunScript("_SelNone", 0)
        getObjects.ClearObjects()
        filter = {IRhinoGetObject.GEOMETRY_TYPE_FILTER.surface_object, IRhinoGetObject.GEOMETRY_TYPE_FILTER.polysrf_object}
        getObjects.SetGeometryFilter(filter)
        sortedObj.Clear()

        ''PER TAGLIARE LA SUPERFICIE ANTERIORE CREO SUPERFICIE PIANA DA DEFORMARE SEGUENDO LA POLISUPERFICIE(DATO CHE LA POLISUPERFICIE A VOLTE NON TAGLIA)       
        Dim cutoutPolySurfaceBbox As OnBoundingBox = sweepSurface.BoundingBox
        Dim trimmedInsoleTopBbox As New OnBoundingBox
        trimmedInsoleTopCopy.Object.GetTightBoundingBox(trimmedInsoleTopBbox)
        cutoutPolySurfaceBbox.Union(trimmedInsoleTopBbox)
        Dim planePoint1 As New On3dPoint(cutoutPolySurfaceBbox.m_min.x, cutoutPolySurfaceBbox.m_min.y, cutoutPolySurfaceBbox.Center.z)
        Dim planePoint2 As New On3dPoint(cutoutPolySurfaceBbox.m_max.x, cutoutPolySurfaceBbox.m_min.y, cutoutPolySurfaceBbox.Center.z)
        Dim planePoint3 As New On3dPoint(cutoutPolySurfaceBbox.m_min.x, cutoutPolySurfaceBbox.m_max.y, cutoutPolySurfaceBbox.Center.z)
        Dim xDirection = New On3dVector(planePoint2 - planePoint1)
        Dim yDirection = New On3dVector(planePoint3 - planePoint1)
        Dim plane As New OnPlane(planePoint1, xDirection, yDirection)
        Dim planeSurface As New OnPlaneSurface(plane)
        planeSurface.Extend(0, New OnInterval(-xDirection.Length() * 0.5, xDirection.Length() * 1.5))
        planeSurface.Extend(1, New OnInterval(-yDirection.Length() * 0.5, yDirection.Length() * 1.5))
        Dim rebuildedSrf As OnNurbsSurface = RhUtil.RhinoRebuildSurface(planeSurface, 4, 4, 50, 50)
#If DEBUG Then
    'RhDebug.AddDocumentToDebug(rebuildedSrf, "rebuildedSrf")
#End If
    planeSurface.Dispose()
        plane.Dispose()
        planePoint1.Dispose()
        planePoint2.Dispose()
        planePoint3.Dispose()
        xDirection.Dispose()
        yDirection.Dispose()
        Dim cv0 As Integer = rebuildedSrf.m_cv_count(0)
        Dim cv1 As Integer = rebuildedSrf.m_cv_count(1)
        Dim newCVlist(cv0 - 1, cv1 - 1) As On3dPoint
        Dim srfHeight As Double = cutoutPolySurfaceBbox.m_max.z - cutoutPolySurfaceBbox.m_min.z
        For i As Integer = 0 To cv0 - 1
            For j As Integer = 0 To cv1 - 1
                Dim point As New On3dPoint
                rebuildedSrf.GetCV(i, j, point)
                Dim startPoint As New On3dPoint(point.x, point.y, point.z - srfHeight)
                Dim endPoint As New On3dPoint(point.x, point.y, point.z + srfHeight)
                intersectionPoints = New On3dPointArray
                intersectionPoints.Append(startPoint)
                intersectionPoints.Append(endPoint)
                Dim line As New OnPolylineCurve(intersectionPoints)
                intersectionPoints = New On3dPointArray
                RhUtil.RhinoCurveBrepIntersect(line.NurbsCurve, backupInsoleTopCopyBrep, 0.1, New OnCurve() {}, intersectionPoints)
                If intersectionPoints.Count <> 0 Then
                    newCVlist(i, j) = New On3dPoint(intersectionPoints.Item(0))
                Else
                    RhUtil.RhinoCurveBrepIntersect(line.NurbsCurve, sweepSurface.BrepForm, 0.1, New OnCurve() {}, intersectionPoints)
                    If intersectionPoints.Count <> 0 Then
                        newCVlist(i, j) = New On3dPoint(intersectionPoints.Item(0))
                    Else
                        newCVlist(i, j) = New On3dPoint(point)
                    End If
                End If
'#If DEBUG Then
'        'RhDebug.AddDocumentToDebug(line, "line")
'        RhDebug.AddDocumentToDebug(newCVlist(i, j), "newCVlist")
'#End If
        startPoint.Dispose()
                endPoint.Dispose()
                line.Dispose()
            Next
        Next
        Dim cutoutProjectedSrf As OnNurbsSurface = RhGeometry.CreaSuperficeDaCV(newCVlist)
#If DEBUG Then
    RhDebug.AddDocumentToDebug(cutoutProjectedSrf, "cutoutProjectedSrf")
#End If
    Dim cutoutProjectedSrfRef As New MRhinoObjRef(Doc.AddSurfaceObject(cutoutProjectedSrf).Attributes.m_uuid)
        'RICAVO INTERSEZIONE PRIMA DEL TAGLIO
        intersectionCurves = {}
        RhUtil.RhinoIntersectBreps(srfToSplitForLateralRef.Brep, cutoutProjectedSrf.BrepForm, 0.1, intersectionCurves, New On3dPointArray)
        If intersectionCurves.Length <> 1 Then Throw New Exception(LanguageManager.Message(353))
        Dim curveForSplit As OnCurve = intersectionCurves(0)
'#If DEBUG Then
'    RhDebug.AddDocumentToDebug(curveForSplit, "curveForSplit")
'#End If
    'TAGLIO IL PEZZO FINALE PER LA SUPERFICIE LATERALE           
    App.RunScript("_SelNone", 0)
        getObjects.ClearObjects()
        filter = {IRhinoGetObject.GEOMETRY_TYPE_FILTER.surface_object, IRhinoGetObject.GEOMETRY_TYPE_FILTER.polysrf_object}
        getObjects.SetGeometryFilter(filter)
        srfToSplitForLateralRef.Object.Select(True, True)
        App.RunScript("-_Split _SelID " & cutoutProjectedSrfRef.m_uuid.ToString & " _Enter", 0)
        getObjects.GetObjects(0, Integer.MaxValue)
        If getObjects.ObjectCount <> 2 Then Throw New Exception(LanguageManager.Message(354))
        getObjects.Object(0).Object.GetTightBoundingBox(bbox1)
        getObjects.Object(1).Object.GetTightBoundingBox(bbox2)

        'L'ULTIMO PEZZO DELLA SUPERFICIE LATERALE E' QUELLO CON BBOX_MIN MINORE
        If bbox1.m_min.z < bbox2.m_min.z Then
            lateralSrfPieces.Append(getObjects.Object(0))
            lateralSrfToDropRef = getObjects.Object(1)
        Else
            lateralSrfPieces.Append(getObjects.Object(1))
            lateralSrfToDropRef = getObjects.Object(0)
        End If
        'TAGLIO LA POLISUPERFICIE CON LA CURVA DI INTERSEZIONE ESTRUSA
        'ESTENDO LA SUPERFICIE DI TAGLIO PER SICUREZZA
        Dim extrusionCutoutSrf As OnSurface = extrusionCurveCutoutSrfRef.Surface.DuplicateSurface
        Doc.DeleteObject(extrusionCurveCutoutSrfRef)
        extensionLenght = cutoutProjectedSrf.BoundingBox.Diagonal.Length
        Dim verticalEdgeIndexes() As Integer = GetVerticalEdgeIndexToExtend(extrusionCutoutSrf)
        For i As Integer = 3 To 6            'W_iso = 3, S_iso = 4, E_iso = 5, N_iso = 6
            Dim edgeIndex As IOnSurface.ISO = CType(i, IOnSurface.ISO)
            If verticalEdgeIndexes.Contains(i) Then
                RhUtil.RhinoExtendSurface(extrusionCutoutSrf, edgeIndex, extensionLenght, False)
            End If
        Next
        extrusionCurveCutoutSrfRef = New MRhinoObjRef(Doc.AddSurfaceObject(extrusionCutoutSrf).Attributes.m_uuid)
        App.RunScript("_SelNone", 0)
        'ESTENDO LA CURVA DI INTERSEZIONE PER SICUREZZA DEL TAGLIO
        RhUtil.RhinoExtendCurve(curveForSplit, IRhinoExtend.Type.Line, 0, extensionLenght)
        RhUtil.RhinoExtendCurve(curveForSplit, IRhinoExtend.Type.Line, 1, extensionLenght)
        Dim extrusionLenght As Double = cutoutProjectedSrf.BoundingBox.Diagonal.Length
        Dim intersectionExtrusionSrf As OnSurface = RhUtil.RhinoExtrudeCurveStraight(curveForSplit, New On3dVector(0, 0, -extrusionLenght))
        xform.Identity()
        xform.Translation(0, 0, extrusionLenght / 2)
        intersectionExtrusionSrf.Transform(xform)
        Dim extrusionObj As MRhinoObject = Doc.AddSurfaceObject(intersectionExtrusionSrf)
        App.RunScript("_SelNone", 0)
        getObjects.ClearObjects()
        filter = {IRhinoGetObject.GEOMETRY_TYPE_FILTER.surface_object, IRhinoGetObject.GEOMETRY_TYPE_FILTER.polysrf_object}
        getObjects.SetGeometryFilter(filter)

        'DOPPIO SPLIT DELLA SUPERFICIE RECREATA
        cutoutProjectedSrfRef.Object.Select(True, True)
        Dim cutter1 As String = extrusionObj.Attributes.m_uuid.ToString
        Dim cutter2 As String = extrusionCurveCutoutSrfRef.m_uuid.ToString
        App.RunScript("-_Split _SelID " & cutter1 & " _SelID " & cutter2 & " _Enter", 0)
        getObjects.GetObjects(0, Integer.MaxValue)
        If getObjects.ObjectCount <> 5 Then Throw New Exception(LanguageManager.Message(355))
        'SALVO BREP CON CENTRO DELLA BBOX PIU' VICINO A QUELLA DI RIFERIMENTO E CANCELLO GLI ALTRI
        For i As Integer = 0 To getObjects.ObjectCount - 1
            getObjects.Object(i).Object.GetTightBoundingBox(bbox1)
            Dim testDistance As Double = bbox1.Center.DistanceTo(trimmedInsoleTopBbox.Center())
            sortedObj.Add(testDistance, getObjects.Object(i))
        Next
        currentAddiction.SurfaceID = sortedObj.Values(0).m_uuid
        For i As Integer = 1 To sortedObj.Count - 1
            Doc.DeleteObject(sortedObj.Values(i))
        Next
        Doc.PurgeObject(extrusionObj)
        extrusionObj.Dispose()
        intersectionExtrusionSrf.Dispose()
        Doc.DeleteObject(trimmedInsoleTopCopy)

        'RIUNISCO LA SUPERFICIE LATERALE E SPOSTO NEL LAYER INSOLE
        App.RunScript("_SelNone", 0)
        getObjects.ClearObjects()
        filter = {IRhinoGetObject.GEOMETRY_TYPE_FILTER.surface_object, IRhinoGetObject.GEOMETRY_TYPE_FILTER.polysrf_object}
        getObjects.SetGeometryFilter(filter)
        sortedObj.Clear()
        For Each objref As MRhinoObjRef In lateralSrfPieces
            objref.Object.Select(True, True)
        Next       
        App.RunScript("_Join", 0)
        getObjects.GetObjects(0, Integer.MaxValue)
        If getObjects.ObjectCount <> 1 Then Throw New Exception(LanguageManager.Message(356))
        Element3dManager.SetRhinoObj(eReferences.insoleLateralSurface, currentAddiction.Side, getObjects.Object(0).m_uuid)
        App.RunScript("_SelNone", 0)
        Dim layerInsole As String = GetLayerName(currentAddiction.Side, eLayerType.insole)
        Element3dManager.GetRhinoObj(eReferences.insoleLateralSurface, currentAddiction.Side).Select(True, True)
        App.RunScript("-_ChangeLayer """ & layerInsole & """", 0)
        App.RunScript("_SelNone", 0)
        App.RunScript("_SelNone", 0)

        'Dispose        
        cutoutProjectedSrfRef.Dispose()
        lateralSrfPieces.Dispose()
        trimmedInsoleTopBbox.Dispose()
        getObjects.Dispose()
        bbox1.Dispose()
        bbox2.Dispose()
    End Sub



    Public Shared Function GetVerticalEdgeIndexToExtend(ByRef surface As OnSurface) As Integer()
        Dim result As New List(Of Integer)
        Dim height As Double = surface.BoundingBox.m_max.z - surface.BoundingBox.m_min.z
        For Each edge As OnBrepEdge In surface.BrepForm.m_E
            Dim bbox As OnBoundingBox = edge.NurbsCurve.BoundingBox
            If Math.Abs(height - (bbox.m_max.z - bbox.m_min.z)) < 0.01 Then
                result.Add(edge.Trim(0).m_iso)
            End If
            bbox.Dispose()
        Next
        Return result.ToArray
    End Function


    Public Shared Function GetSweepEdgeIndexToExtend(ByRef surface As OnSurface, ByRef borderSweepCrv1 As OnCurve, ByRef borderSweepCrv2 As OnCurve) As Integer()
        Dim result(2) As Integer
        Dim center1 As On3dPoint = borderSweepCrv1.BoundingBox.Center
        Dim center2 As On3dPoint = borderSweepCrv2.BoundingBox.Center
        Dim maxLenght As Double = Double.MinValue
        Dim minDistanceBorder1 As Double = Double.MaxValue
        Dim minDistanceBorder2 As Double = Double.MaxValue
        For Each edge As OnBrepEdge In surface.BrepForm.m_E
            Dim lenght As Double
            edge.NurbsCurve.GetLength(lenght)
            If lenght > maxLenght Then
                maxLenght = lenght
                result(0) = edge.Trim(0).m_iso
            End If
            Dim distance As Double = edge.NurbsCurve.BoundingBox.Center.DistanceTo(center1)
            If distance < minDistanceBorder1 Then
                minDistanceBorder1 = distance
                result(1) = edge.Trim(0).m_iso
            End If
            distance = edge.NurbsCurve.BoundingBox.Center.DistanceTo(center2)
            If distance < minDistanceBorder2 Then
                minDistanceBorder2 = distance
                result(2) = edge.Trim(0).m_iso
            End If
        Next
        center1.Dispose()
        center2.Dispose()
        Return result
    End Function


    Public Shared Sub CheckTotalCutoutDirection(ByRef addiction As IdAddiction, ByRef objRef As MRhinoObjRef)
        Select Case addiction.Type

            Case eAddictionType.cutout
                If addiction.Model = eAddictionModel.cutoutTotal Then
                    Dim cutout As IdCutoutToTalAddiction = DirectCast(addiction, IdCutoutToTalAddiction)
                    If cutout.Side = eSide.right And cutout.CutoutDirection = eCutoutDirection.external Or
                        cutout.Side = eSide.left And cutout.CutoutDirection = eCutoutDirection.internal Then
                        CheckSurfaceDirection(objRef, eDirectionCheck.negativeY)
                    Else
                        CheckSurfaceDirection(objRef, eDirectionCheck.positiveY)
                    End If
                End If

            Case eAddictionType.horseShoe
                If addiction.Model = eAddictionModel.horseShoeTotal Then
                    CheckSurfaceDirection(objRef, eDirectionCheck.positiveX)
                End If

        End Select
    End Sub


#End Region


#Region " METODI IMPLEMENTATI "


    Public Overrides Function AddictionCanBeAdded(ByRef errorMessage As String) As Boolean
        If IdElement3dManager.GetInstance.GetAddictions(Me.Side).Count = 0 Then Return True

        'Controllo se esiste già uno scarico di tipo cutout o ferro di cavallo
        For Each addiction As IdAddiction In IdElement3dManager.GetInstance.GetAddictions(Me.Side)
            If addiction.Type = eAddictionType.cutout Or addiction.Type = eAddictionType.horseShoe Then
                errorMessage = LanguageManager.Message(1, Me.Type, addiction.Type)
                Return False
            End If
        Next

        Return True
    End Function

    Public Overrides Function Serialize(ByRef archive As OnBinaryArchive) As Boolean
        If Not MyBase.CommonSerialize(archive) Then Return False

        'BREP
        If Not archive.WriteObject(Me.BackupInsoleSurface(eAddictionBkSrf.lateral)) Then Return False
        If Not archive.WriteObject(Me.BackupInsoleSurface(eAddictionBkSrf.bottom)) Then Return False

        Return True
    End Function

    Public Overrides Function Deserialize(ByRef archive As RMA.OpenNURBS.OnBinaryArchive) As Boolean
        'BREP
        Dim onobj As OnObject = New OnBrep()
        If Not CBool(archive.ReadObject(onobj)) Then Return False
        Me.BackupInsoleSurface(eAddictionBkSrf.lateral) = OnBrep.Cast(onobj).Duplicate
        onobj = New OnBrep()
        If Not CBool(archive.ReadObject(onobj)) Then Return False
        Me.BackupInsoleSurface(eAddictionBkSrf.bottom) = OnBrep.Cast(onobj).Duplicate
        onobj.Dispose()

        Return True
    End Function


#End Region



End Class
