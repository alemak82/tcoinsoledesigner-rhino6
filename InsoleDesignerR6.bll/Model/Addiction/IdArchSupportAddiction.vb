Imports System.IO
Imports RMA.OpenNURBS
Imports InsoleDesigner.bll.IdAlias
Imports InsoleDesigner.bll.IdLanguageManager
Imports InsoleDesigner.bll.IdElement3dManager
Imports RhinoUtils
Imports RMA.Rhino


Public Class IdArchSupportAddiction
    Inherits AbstractProjectableAddiction


#Region " ENUM "

    Public Enum eBaseCurve
        internal
        external
    End Enum

#End Region


#Region " FIELD "


    Public Const CURVE_LENGHT_PERCENT_TRIM As Double = 7 / 100
    Public Property InternalProjectedCrvId As Guid
    Public Property ExternalProjectedCrvId As Guid
    Public Property InternalProjectedCrv As OnCurve

#End Region


#Region " Constructor "


    Public Sub New(ByVal side As eSide, ByVal type As eAddictionType, ByVal model As eAddictionModel, ByVal size As eAddictionSize)
        MyBase.New(side, type, model, size)
    End Sub

    Protected Overrides Sub SetDefault()
        Me.Model = eAddictionModel.archsupport3030
    End Sub


#End Region


#Region " Funzioni CAD "

    ''' <summary>
    ''' La curva anteriore sarà sempre più lunga di quella esterna
    ''' </summary>
    ''' <param name="baseCurve"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetBaseCurveRef(ByVal baseCurve As eBaseCurve) As MRhinoObjRef
        If Me.GetBaseCurves.Count <> 2 Then Return Nothing
        Dim currentLenght As Double = Double.MinValue
        If baseCurve = eBaseCurve.external Then currentLenght = Double.MaxValue
        Dim resultId As Guid = Guid.Empty
        For Each uuid As Guid In Me.GetBaseCurvesId
            Dim objref As New MRhinoObjRef(uuid)
            If objref Is Nothing Then Return Nothing
            Dim curve As IOnCurve = objref.Curve
            If curve Is Nothing Then Return Nothing
            Dim lenght As Double = 0
            curve.GetLength(lenght)
            If baseCurve = eBaseCurve.internal And lenght > currentLenght Or baseCurve = eBaseCurve.external And lenght < currentLenght Then
                currentLenght = lenght
                resultId = uuid
            End If
            objref.Dispose()
        Next
        If RhUtil.RhinoApp.ActiveDoc.LookupObject(resultId) Is Nothing Then Return Nothing
        Return New MRhinoObjRef(resultId)
    End Function


    ''' <summary>
    ''' Controllo che la distanza dello start point e end point dalla curva interna sia minore di un delta
    ''' </summary>
    ''' <returns></returns>
    Public Overrides Function IsCurveLongitudinal(uuid As System.Guid) As Boolean
        Dim objref As New MRhinoObjRef(uuid)
        If objref Is Nothing Then Return False
        Dim curve As IOnCurve = objref.Curve
        objref.Dispose()
        If curve Is Nothing Then Return False
        Dim internalCrvRef As MRhinoObjRef = GetBaseCurveRef(eBaseCurve.internal)
        If internalCrvRef Is Nothing Then Return False
        Dim internalCurve As IOnCurve = internalCrvRef.Curve()
        internalCrvRef.Dispose()
        If internalCurve Is Nothing Then Return False
        Dim t As Double
        internalCurve.GetClosestPoint(curve.PointAtStart, t)
        Dim closestStart As On3dPoint = internalCurve.PointAt(t)
        internalCurve.GetClosestPoint(curve.PointAtEnd, t)
        Dim closestEnd As On3dPoint = internalCurve.PointAt(t)
        Dim distanceStart As Double = curve.PointAtStart.DistanceTo(closestStart)
        Dim distanceEnd As Double = curve.PointAtEnd.DistanceTo(closestEnd)
        closestStart.Dispose()
        closestEnd.Dispose()
        Return (Math.Max(distanceStart, distanceEnd) < RhUtil.RhinoApp.ActiveDoc.AbsoluteTolerance)
    End Function


    ''' <summary>
    ''' Split delle due curve di base e _NetworkSrf
    ''' </summary>
    ''' <param name="layerName"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overrides Function CreateSrfFromCurves(ByVal layerName As String) As Boolean
        If Not AreAllCurvesInDocument() Then Return False
        If RhUtil.RhinoApp.ActiveDoc.LookupObject(InternalProjectedCrvId) Is Nothing Then Return False
        If RhUtil.RhinoApp.ActiveDoc.LookupObject(ExternalProjectedCrvId) Is Nothing Then Return False
        RhLayer.RendiCorrenteLayer(GetLayerName(Me.Side, eLayerType.addiction))
        Dim internalCurve As OnCurve = New MRhinoObjRef(InternalProjectedCrvId).Curve.DuplicateCurve()
        Dim externalCurve As OnCurve = New MRhinoObjRef(ExternalProjectedCrvId).Curve.DuplicateCurve()
        'Prendo gli estremi della curva esterna come riferimenti per i piani
        Dim pointMinX, pointMaxX As On3dPoint
        If externalCurve.PointAtStart.x < externalCurve.PointAtEnd.x Then
            pointMinX = externalCurve.PointAtStart
            pointMaxX = externalCurve.PointAtEnd
        Else
            pointMinX = externalCurve.PointAtEnd
            pointMaxX = externalCurve.PointAtStart
        End If
        'Calcolo spostamento = 7% della lunghezza della curva esterna
        Dim curveLenght As Double
        externalCurve.GetLength(curveLenght)
        Dim gapX As Double = curveLenght * CURVE_LENGHT_PERCENT_TRIM
        'Da pointMaxX mi sposto di -gapX e costruisco piano con relativa superficie
        Dim originPoint As New On3dPoint(pointMaxX.x - gapX, pointMaxX.y - Me.GetBbox.Diagonal.y, pointMaxX.z - Me.GetBbox.Diagonal.z)
        Dim planePoint2 As New On3dPoint(originPoint.x, pointMaxX.y + Me.GetBbox.Diagonal.y, pointMaxX.z - Me.GetBbox.Diagonal.z)
        Dim planePoint3 As New On3dPoint(originPoint.x, pointMaxX.y + Me.GetBbox.Diagonal.y, pointMaxX.z + Me.GetBbox.Diagonal.z)
        Dim yDirection As New On3dVector(planePoint2 - originPoint)
        Dim zDirection As New On3dVector(planePoint3 - originPoint) '
        Dim plane As New OnPlane(originPoint, yDirection, zDirection)
        Dim planeSurfaceMax As New OnPlaneSurface(plane)
        planeSurfaceMax.Extend(0, New OnInterval(-yDirection.Length(), yDirection.Length() * 2))
        planeSurfaceMax.Extend(1, New OnInterval(0, zDirection.Length()))
        Dim cuttingSrfObj1 As MRhinoObject = RhUtil.RhinoApp.ActiveDoc.AddSurfaceObject(planeSurfaceMax)
        'Da pointMinX mi sposto di +gapX  e costruisco piano con relativa superficie
        originPoint = New On3dPoint(pointMinX.x + gapX, pointMinX.y - Me.GetBbox.Diagonal.y, pointMinX.z - Me.GetBbox.Diagonal.z)
        planePoint2 = New On3dPoint(originPoint.x, pointMinX.y + Me.GetBbox.Diagonal.y, pointMinX.z - Me.GetBbox.Diagonal.z)
        planePoint3 = New On3dPoint(originPoint.x, pointMinX.y + Me.GetBbox.Diagonal.y, pointMinX.z + Me.GetBbox.Diagonal.z)
        yDirection = New On3dVector(planePoint2 - originPoint)
        zDirection = New On3dVector(planePoint3 - originPoint)
        plane = New OnPlane(originPoint, yDirection, zDirection)
        Dim planeSurfaceMin As New OnPlaneSurface(plane)
        planeSurfaceMin.Extend(0, New OnInterval(-yDirection.Length(), yDirection.Length() * 2))
        planeSurfaceMin.Extend(1, New OnInterval(0, zDirection.Length()))
        Dim cuttingSrfObj2 As MRhinoObject = RhUtil.RhinoApp.ActiveDoc.AddSurfaceObject(planeSurfaceMin)
        'Unisco curva interna ed esterna
        Me.InternalProjectedCrv = internalCurve.DuplicateCurve()
        Dim mergedCurves() As OnCurve = {}
        If Not RhUtil.RhinoMergeCurves(New OnCurve() {externalCurve, internalCurve}, mergedCurves) Then Return False
        If mergedCurves.Length <> 1 Then Return False
        Dim mergedCrvObj As MRhinoObject = RhUtil.RhinoApp.ActiveDoc.AddCurveObject(mergedCurves(0))
        'Elimino vecchie curve dal Doc
        RhUtil.RhinoApp.ActiveDoc.DeleteObject(New MRhinoObjRef(InternalProjectedCrvId))
        RhUtil.RhinoApp.ActiveDoc.DeleteObject(New MRhinoObjRef(ExternalProjectedCrvId))
        'Split della curva dal Doc per fare un taglio unico     
        RhUtil.RhinoApp.RunScript("_SelNone", 0)
        mergedCrvObj.Select(True, True)
        Dim getObjects As New MRhinoGetObject
        Dim cuttingId1 As String = cuttingSrfObj1.Attributes.m_uuid.ToString()
        Dim cuttingId2 As String = cuttingSrfObj2.Attributes.m_uuid.ToString()
        Dim splitCommand As String = "-_Split _SelID " & cuttingId1 & " _SelId " & cuttingId2 & " _Enter"
        RhUtil.RhinoApp().RunScript(splitCommand, 0)
        RhUtil.RhinoApp.RunScript("_SelNone", 0)
        RhUtil.RhinoApp().RunScript("_SelLast", 0)
        'Prendo i due oggetti risultanti dallo split            
        getObjects.SetGeometryFilter(IRhinoGetObject.GEOMETRY_TYPE_FILTER.curve_object)
        getObjects.GetObjects(0, Integer.MaxValue)
        If getObjects.ObjectCount <> 4 Then Throw New Exception(LanguageManager.Message(357))
        'Aggiungo i risultati a Me.Curves
        Me.CurvesID.Add(getObjects.Object(0).ObjectUuid)
        Me.CurvesID.Add(getObjects.Object(1).ObjectUuid)
        Me.CurvesID.Add(getObjects.Object(2).ObjectUuid)
        Me.CurvesID.Add(getObjects.Object(3).ObjectUuid)
        RhUtil.RhinoApp.RunScript("_SelNone", 0)
        'Elimino superfici di taglio
        RhUtil.RhinoApp.ActiveDoc.PurgeObject(cuttingSrfObj1)
        RhUtil.RhinoApp.ActiveDoc.PurgeObject(cuttingSrfObj2)
        'Creo la superficie con il _NetworkSrf        
        MyBase.CreateSrfFromCurves(layerName)
        'Dispose
        pointMinX.Dispose()
        pointMaxX.Dispose()
        originPoint.Dispose()
        planePoint2.Dispose()
        planePoint3.Dispose()
        yDirection.Dispose()
        zDirection.Dispose()
        plane.Dispose()
        planeSurfaceMax.Dispose()
        planeSurfaceMin.Dispose()
        getObjects.Dispose()
        Return True
    End Function


#End Region


#Region " Serializzazione/deserializzazione"


    Public Overrides Function Serialize(ByRef archive As RMA.OpenNURBS.OnBinaryArchive) As Boolean
        Return MyBase.CommonSerialize(archive)
        ''Sviluppi futuri...
    End Function

    Public Overrides Function Deserialize(ByRef archive As RMA.OpenNURBS.OnBinaryArchive) As Boolean
        ''Sviluppi futuri...
        Return True
    End Function


#End Region


#Region " IClonable "


    Public Overrides Function Clone() As Object
        Dim plainObject As IdAddiction = IdAddictionFactory.Create(Me.Side, Me.Type, Me.Model, Me.Size)
        MyBase.CloneCommonField(plainObject)
        'Campi specifici IdArchSupportAddiction
        Dim result As IdArchSupportAddiction = DirectCast(plainObject, IdArchSupportAddiction)
        result.InternalProjectedCrvId = New Guid(Me.InternalProjectedCrvId.ToString)
        result.ExternalProjectedCrvId = New Guid(Me.ExternalProjectedCrvId.ToString)
        If Me.InternalProjectedCrv IsNot Nothing AndAlso Me.InternalProjectedCrv.InternalPointer <> IntPtr.Zero Then
            result.InternalProjectedCrv = Me.InternalProjectedCrv.DuplicateCurve()
        End If
        Return result
    End Function


#End Region



End Class
