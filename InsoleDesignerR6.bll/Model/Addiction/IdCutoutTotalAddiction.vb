Imports System.IO
Imports RMA.OpenNURBS
Imports InsoleDesigner.bll.IdLanguageManager
Imports InsoleDesigner.bll.IdElement3dManager
Imports InsoleDesigner.bll.IdGeometryUtils
Imports InsoleDesigner.bll.AbstractCutoutCommons
Imports InsoleDesigner.bll.IdAlias
Imports System.Reflection
Imports ORM
Imports RhinoUtils
Imports RMA.Rhino
Imports RhinoUtils.RhGeometry


Public Class IdCutoutToTalAddiction
    Inherits AbstractCutoutCommons
    Implements ICutoutAddiction



#Region " UUID INIZIALI - MANTENUTI SOLO PER OTTIMIZZAZIONE MA SE NON ALLINEATI VIENE COMUNQUE FATTO RICONOSCIMENTO GEOMETRICO "


    Public Property CUTOUT_EXT_DX_L1_ID As String = "7fc66f30-3c6e-4a5b-a89a-7f2f63044a9c"
    Public Property CUTOUT_EXT_DX_L2_ID As String = "6e655c82-ea45-47f5-98ff-8d4e50bc1e10"
    Public Property CUTOUT_EXT_SN_L1_ID As String = "75405dff-4ccc-456e-8f4f-88117e1d8163"
    Public Property CUTOUT_EXT_SN_L2_ID As String = "f4d3c3ef-0a7b-4de6-a51d-dd2bfe09091f"
    Public Property CUTOUT_INT_DX_L1_ID As String = "56dd875e-c43e-419e-b97b-1c47e62f6cf6"
    Public Property CUTOUT_INT_DX_L2_ID As String = "3a04f6a2-48eb-4647-983d-635c9ff61e7e"
    Public Property CUTOUT_INT_SN_L1_ID As String = "b57981cc-c6f7-4058-a073-af1134af8dbd"
    Public Property CUTOUT_INT_SN_L2_ID As String = "8123204b-2943-4837-9f90-35795be94d5e"


#End Region


#Region " FIELD "

    Public FilletCurveId As Guid
    Protected mFilletRadiusL1L2 As Double
    Protected mCutoutDirection As eCutoutDirection


#End Region


#Region " Constructor "

    Public Sub New(ByVal side As eSide, ByVal direction As eCutoutDirection)
        Me.Type = eAddictionType.cutout
        Me.Side = side
        Me.CutoutDirection = direction
        CurvesID = New List(Of Guid)
        SetCutout()
        SetFileName()
    End Sub

    Protected Overrides Sub SetDefault()
        mCutoutDirection = eCutoutDirection.none
        'Valore default ricavato dal file cad della procedura
        mFilletRadiusL1L2 = 15.0
    End Sub

    Protected Overridable Sub SetCutout()
        Me.Model = eAddictionModel.cutoutTotal
    End Sub

#End Region


#Region " Property "

    ''' <summary>
    ''' Solo lo scarico cutout ha la direzione
    ''' </summary>
    Public Property CutoutDirection() As eCutoutDirection
        Get
            If Me.Type = eAddictionType.cutout Then
                Return Me.mCutoutDirection
            Else
                Return eCutoutDirection.none
            End If
        End Get
        Set(value As eCutoutDirection)
            If Me.Type = eAddictionType.cutout Then
                Me.mCutoutDirection = value
                SetFileName()
            Else
                Me.mCutoutDirection = eCutoutDirection.none
            End If
        End Set
    End Property

    Public Property FilletRadius(ByVal horseShoeFillet As eHorseShoeFilletCrv) As Double Implements ICutoutAddiction.FilletRadius
        Get
            Return mFilletRadiusL1L2
        End Get
        Set(value As Double)
            mFilletRadiusL1L2 = value
            DeleteFilletCurve()
        End Set
    End Property



#End Region


#Region " ID e Nomi file e oggetti "



    Public Function OtherCutoutCurve(ByVal cutoutCurve As eCutoutCurve) As eCutoutCurve
        If cutoutCurve = eCutoutCurve.L1 Then
            Return eCutoutCurve.L2
        Else
            Return eCutoutCurve.L1
        End If
    End Function


    Public Overloads Shared Function GetFileName(ByVal side As eSide, ByVal type As eAddictionType, ByVal model As eAddictionModel, ByVal direction As eCutoutDirection) As String
        Dim basePath As String = My.Application.Info.DirectoryPath
        'Assemblaggio stringhe
        Dim filename As String = ""
        filename = GetFileTypeString(type) & GetDirectionName(direction) & GetFileSideString(side)
        Return Path.Combine(LibraryManager.GetDirectory(LibraryManager.eDirectoryLibrary.addiction), filename & ".3dm")
    End Function


    Protected Shared Function GetDirectionName(ByVal direction As eCutoutDirection) As String
        Select Case direction
            Case eCutoutDirection.external
                Return "_Ext"
            Case eCutoutDirection.internal
                Return "_Int"
            Case eCutoutDirection.none
                Return ""
            Case Else
                Return ""
        End Select
    End Function


    Protected Overrides Sub SetFileName()
        mOriginFileName = GetFileName(Me.Side, Me.Type, Me.Model, Me.CutoutDirection)
#If DEBUG Then
        If Not File.Exists(mOriginFileName) Then MsgBox("Non esiste uno scarico con le caratteristiche richieste", MsgBoxStyle.Exclamation, My.Application.Info.Title)
#End If
    End Sub


    Public Shared Function GetCurveName(ByVal cutoutCurve As eCutoutCurve) As String
        If cutoutCurve = eCutoutCurve.L1 Then
            Return "L1"
        ElseIf cutoutCurve = eCutoutCurve.L2 Then
            Return "L2"
        ElseIf cutoutCurve = eCutoutCurve.L1L2 Then
            Return "Cutout"
        End If
        Return ""
    End Function


    Public Function GetCurveID(ByVal cutoutCurve As eCutoutCurve) As Guid
        If cutoutCurve = eCutoutCurve.L1L2 Then Return FilletCurveId
        Dim stringId As String = ""
        If cutoutCurve = eCutoutCurve.L1 Then
            If Me.Side = eSide.left Then
                If Me.CutoutDirection = eCutoutDirection.external Then stringId = CUTOUT_EXT_SN_L1_ID
                If Me.CutoutDirection = eCutoutDirection.internal Then stringId = CUTOUT_INT_SN_L1_ID
            Else
                If Me.CutoutDirection = eCutoutDirection.external Then stringId = CUTOUT_EXT_DX_L1_ID
                If Me.CutoutDirection = eCutoutDirection.internal Then stringId = CUTOUT_INT_DX_L1_ID
            End If
        Else
            If Me.Side = eSide.left Then
                If Me.CutoutDirection = eCutoutDirection.external Then stringId = CUTOUT_EXT_SN_L2_ID
                If Me.CutoutDirection = eCutoutDirection.internal Then stringId = CUTOUT_INT_SN_L2_ID
            Else
                If Me.CutoutDirection = eCutoutDirection.external Then stringId = CUTOUT_EXT_DX_L2_ID
                If Me.CutoutDirection = eCutoutDirection.internal Then stringId = CUTOUT_INT_DX_L2_ID
            End If
        End If
        Dim result As Guid
        Guid.TryParse(stringId.Trim(), result)
        Return result
    End Function


    Public Sub UpdateCurveId(ByVal cutoutCurve As eCutoutCurve, ByVal oldId As Guid, ByVal newId As String)
        newId = newId.Trim
        Dim uuid As Guid
        Guid.TryParse(newId, uuid)
        If cutoutCurve = eCutoutCurve.L1L2 Then
            FilletCurveId = uuid
            Exit Sub
        Else
            MyBase.ReplaceId(oldId, uuid)
        End If
        If cutoutCurve = eCutoutCurve.L1 Then
            If Me.Side = eSide.left Then
                If Me.CutoutDirection = eCutoutDirection.external Then CUTOUT_EXT_SN_L1_ID = newId
                If Me.CutoutDirection = eCutoutDirection.internal Then CUTOUT_INT_SN_L1_ID = newId
            Else
                If Me.CutoutDirection = eCutoutDirection.external Then CUTOUT_EXT_DX_L1_ID = newId
                If Me.CutoutDirection = eCutoutDirection.internal Then CUTOUT_INT_DX_L1_ID = newId
            End If
        Else
            If Me.Side = eSide.left Then
                If Me.CutoutDirection = eCutoutDirection.external Then CUTOUT_EXT_SN_L2_ID = newId
                If Me.CutoutDirection = eCutoutDirection.internal Then CUTOUT_INT_SN_L2_ID = newId
            Else
                If Me.CutoutDirection = eCutoutDirection.external Then CUTOUT_EXT_DX_L2_ID = newId
                If Me.CutoutDirection = eCutoutDirection.internal Then CUTOUT_INT_DX_L2_ID = newId
            End If
        End If
    End Sub


    Public Sub ClearCurvesID() Implements ICutoutAddiction.ClearCurveID
        CUTOUT_EXT_DX_L1_ID = ""
        CUTOUT_EXT_DX_L2_ID = ""
        CUTOUT_EXT_SN_L1_ID = ""
        CUTOUT_EXT_SN_L2_ID = ""
        CUTOUT_INT_DX_L1_ID = ""
        CUTOUT_INT_DX_L2_ID = ""
        CUTOUT_INT_SN_L1_ID = ""
        CUTOUT_INT_SN_L2_ID = ""
        FilletCurveId = Guid.Empty
        Me.CurvesID.Clear()
    End Sub


#End Region


#Region " Adapter Implementation "


    ''' <summary>
    ''' Per comodità rendo compatibile con Cutout
    ''' </summary>
    Public Function GetCurveRef(ByVal type As eHorseShoeCrv) As MRhinoObjRef Implements ICutoutAddiction.GetCurveRef
        Return Me.GetCurveRef(ConvertCutoutCrv(type))
    End Function

    Public Function GetCurveID(ByVal type As eHorseShoeCrv) As Guid Implements ICutoutAddiction.GetCurvesID
        Return Me.GetCurveID(ConvertCutoutCrv(type))
    End Function

    Public Sub UpdateCurveId(ByVal type As eHorseShoeCrv, ByVal oldId As Guid, ByVal newId As String) Implements ICutoutAddiction.UpdateCurveId
        Me.UpdateCurveId(ConvertCutoutCrv(type), oldId, newId)
    End Sub

    Public Function AddCurveToDoc(ByVal type As eHorseShoeCrv, ByVal curve As OnCurve, ByVal oldId As Guid) As MRhinoCurveObject Implements ICutoutAddiction.AddCurveToDoc
        Return Me.AddCurveToDoc(ConvertCutoutCrv(type), curve, oldId)
    End Function

    Public Function SetMaxFilletRadius(ByVal fillet As eHorseShoeFilletCrv) As Decimal Implements ICutoutAddiction.MaxFilletRadius
        Return Me.MaxFilletRadius()
    End Function

    Public Function CurveIsInDoc(ByVal horseShoeCrv As eHorseShoeCrv) As Boolean Implements ICutoutAddiction.CurveIsInDoc
        Return Me.CurveIsInDoc(ConvertCutoutCrv(horseShoeCrv))
    End Function

    Public Sub RotateCurve(ByVal type As eHorseShoeStraightCrv, ByVal angle As Double, Optional ByVal rotationCenter As On3dPoint = Nothing) Implements ICutoutAddiction.RotateCurve
        Me.RotateCurve(ConvertCutoutCrv(type), angle)
    End Sub

    Function GetCenterOfRotation(ByVal straightCrv As eHorseShoeStraightCrv) As On3dPoint Implements ICutoutAddiction.GetCenterOfRotation
        Return Me.GetCenterOfRotation()
    End Function

    Public Function SetCurveLenght(ByVal straightCrv As eHorseShoeStraightCrv, ByVal totLenght As Double) As Boolean Implements ICutoutAddiction.SetCurveLenght
        Return Me.SetCurveLenght(ConvertCutoutCrv(straightCrv), totLenght)
    End Function

    Public Function AreCurvesParallel(ByVal type1 As eHorseShoeStraightCrv, ByVal type2 As eHorseShoeStraightCrv) As Boolean Implements ICutoutAddiction.AreCurvesParallel
        Return Me.AreCurvesParallel(ConvertCutoutCrv(type1), ConvertCutoutCrv(type2))
    End Function

    Public Function CreateFillet(ByVal filletCrv As eHorseShoeFilletCrv, Optional ByVal docRedraw As Boolean = True) As Boolean Implements ICutoutAddiction.CreateFillet
        Return Me.CreateFillet(docRedraw)
    End Function

    Public Sub DeleteCurve(ByVal eHorseShoeCrv As eHorseShoeCrv) Implements ICutoutAddiction.DeleteCurve
        If eHorseShoeCrv = AbstractCutoutCommons.eHorseShoeCrv.L1L2 Then
            DeleteFilletCurve()
        ElseIf eHorseShoeCrv = AbstractCutoutCommons.eHorseShoeCrv.L1 Or eHorseShoeCrv = AbstractCutoutCommons.eHorseShoeCrv.L2 Then
            DeleteCurve(ConvertCutoutCrv(eHorseShoeCrv))
        End If
    End Sub


#End Region


#Region " CAD Method ICutout "


    Public Function GetCurveToExtrude() As IOnCurve Implements ICutoutAddiction.GetCurveToExtrude
        If Not CurveIsInDoc(eCutoutCurve.L1L2) Then Return Nothing
        Return GetCurveRef(eCutoutCurve.L1L2).Curve
    End Function

    ''' <summary>
    ''' Aggiunge la curva mantenendo i nomi L1-L2
    ''' </summary>
    ''' <param name="cutoutCurve"></param>
    ''' <param name="curve"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function AddCurveToDoc(ByVal cutoutCurve As eCutoutCurve, ByVal curve As OnCurve, ByVal oldId As Guid) As MRhinoCurveObject
        If CurveIsInDoc(cutoutCurve) Then DeleteCurve(ConvertCutoutCrv(cutoutCurve))
        If cutoutCurve = eCutoutCurve.L1 Or cutoutCurve = eCutoutCurve.L2 Then
            RhLayer.RendiCorrenteLayer(GetLayerName(Me.Side, eLayerType.addiction))
        Else
            RhLayer.RendiCorrenteLayer(GetLayerName(Me.Side, eLayerType.root))
        End If
        Dim curveObj As MRhinoCurveObject = RhUtil.RhinoApp.ActiveDoc.AddCurveObject(curve)
        Dim objMinYAttr As New MRhinoObjectAttributes(curveObj.Attributes)
        objMinYAttr.m_name = GetCurveName(cutoutCurve)
        RhUtil.RhinoApp.ActiveDoc.ModifyObjectAttributes(New MRhinoObjRef(curveObj.Attributes.m_uuid), objMinYAttr)
        RhUtil.RhinoApp.RunScript("_SelNone", 0)
        'Aggiorno ID
        UpdateCurveId(cutoutCurve, oldId, curveObj.Attributes.m_uuid.ToString)
        Return curveObj
    End Function


    Private Sub RotateCurve(ByVal cutoutCurve As eCutoutCurve, ByVal angle As Double)
        Dim xForm As New OnXform()
        Dim radiantAngle As Double = angle * Math.PI / 180
        xForm.Rotation(radiantAngle, OnUtil.On_zaxis, GetCenterOfRotation())
        Dim curve As OnCurve = GetCurveRef(cutoutCurve).Curve.DuplicateCurve()
        curve.Transform(xForm)
        ''ELIMINO LA VECCHIA CURVA DAL DOC, AGGIUNGO LA NUOVE AL DOC E AGGIORNO ID CURVE
        AddCurveToDoc(cutoutCurve, curve, GetCurveRef(cutoutCurve).m_uuid)
        curve.Dispose()
        xForm.Dispose()
    End Sub


    ''' <summary>
    ''' Se le curve non sono ancora raccordate ritorna il punto di intersezione tra le due curve per esigenze del progettista, altrimenti ritorna il centro della bbox
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>Si poteva fare come intersezione di una curva con l'estrusione dell'altra</remarks>
    Public Function GetCenterOfRotation() As On3dPoint
        Dim baseCurves As List(Of IOnCurve) = Me.GetBaseCurves()
        If baseCurves.Count = 1 Then
            Return Me.GetCurveRef(eCutoutCurve.L1L2).Object.BoundingBox.Center()
        ElseIf baseCurves.Count = 2 Then
            'Calcolo il centro di rotazione come estremo di una curva che ha un estremo dell'altra più vicino
            Dim distanceCenter1 As Double = Math.Min(baseCurves.Item(0).PointAtStart().DistanceTo(baseCurves.Item(1).PointAtStart()), baseCurves.Item(0).PointAtStart().DistanceTo(baseCurves.Item(1).PointAtEnd()))
            Dim distanceCenter2 As Double = Math.Min(baseCurves.Item(0).PointAtEnd().DistanceTo(baseCurves.Item(1).PointAtStart()), baseCurves.Item(0).PointAtEnd().DistanceTo(baseCurves.Item(1).PointAtEnd()))
            If distanceCenter1 < distanceCenter2 Then
                Return baseCurves.Item(0).PointAtStart()
            Else
                Return baseCurves.Item(0).PointAtEnd()
            End If
        Else
            Return Nothing
        End If
    End Function

    ''' <summary>
    ''' Prima cerco per GUID, se non trovo per nome oggetto(PERICOLOSO) altrimenti faccio riconoscimento geometrico
    ''' </summary>
    ''' <param name="cutoutCurve"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function GetCurveRef(ByVal cutoutCurve As eCutoutCurve) As MRhinoObjRef
        'Se ho già raccordato ritorno la curva unica
        If cutoutCurve = eCutoutCurve.L1L2 Then
            Return New MRhinoObjRef(Me.FilletCurveId)
        End If
        Dim result As MRhinoObjRef = Nothing
        'Prima cerco con GUID
        Dim curveId As Guid = GetCurveID(cutoutCurve)
        Dim obj As IOnObject = RhUtil.RhinoApp.ActiveDoc.LookupDocumentObject(curveId, True)
        If obj IsNot Nothing AndAlso obj.ObjectType = IOn.object_type.curve_object Then Return New MRhinoObjRef(curveId)
        If Me.CurvesID.Count = 0 Then Return Nothing
        'Poi cerco oggetti con nome L1 e L2
        For Each uuid As Guid In Me.CurvesID
            result = New MRhinoObjRef(uuid)
            If result.Object IsNot Nothing AndAlso result.Object.Attributes.m_name = GetCurveName(cutoutCurve) Then Return result
            result.Dispose()
        Next
        'SE E' GIA' STATO FATTO IL RACCORDO' RITORNO NOTHING
        Dim obj0 As New MRhinoObjRef(Me.CurvesID.Item(0))
        Dim obj1 As New MRhinoObjRef(Me.CurvesID.Item(1))
        If obj0.Object Is Nothing Or obj1.Object Is Nothing Then Return Nothing
        'Se non li ho ancora trovati L1 è quello con minore variazione in X (SE NON CAMBIA LA GEOMETRIA DELLO SCARICO)
        Dim deltaX0 As Double = obj0.Object.BoundingBox().Diagonal.x
        Dim deltaX1 As Double = obj1.Object.BoundingBox().Diagonal.x
        If (cutoutCurve = eCutoutCurve.L1 And deltaX0 < deltaX1) Or (cutoutCurve = eCutoutCurve.L2 And deltaX0 > deltaX1) Then
            Return obj0
        Else
            Return obj1
        End If
        obj0.Dispose()
        obj1.Dispose()
    End Function


    Private Function SetCurveLenght(ByVal cutoutCurve As eCutoutCurve, ByVal newLenght As Double) As Boolean
        If cutoutCurve = eCutoutCurve.L1L2 Then Return False
        Dim curveRef As MRhinoObjRef = GetCurveRef(cutoutCurve)
        If curveRef Is Nothing Then Return False
        Dim curve As OnCurve = curveRef.Curve().DuplicateCurve()
        Dim testPoint As On3dPoint = GetCenterOfRotation()
        Dim fixedPoint As On3dPoint = Nothing
        Dim pointToMove As On3dPoint = Nothing
        If curve.PointAtStart.DistanceTo(testPoint) < curve.PointAtEnd.DistanceTo(testPoint) Then
            fixedPoint = curve.PointAtStart
            pointToMove = curve.PointAtEnd
        Else
            fixedPoint = curve.PointAtEnd
            pointToMove = curve.PointAtStart
        End If
        Dim vector As New On3dVector(pointToMove - fixedPoint)
        vector.Unitize()
        Dim xform As New OnXform
        xform.Scale(fixedPoint, newLenght)
        vector.Transform(xform)
        Dim newPoint = New On3dPoint(vector.x + fixedPoint.x, vector.y + fixedPoint.y, vector.z + fixedPoint.z)
        Dim newCurve As New OnLineCurve(fixedPoint, newPoint)
        'Aggiorno        
        Me.AddCurveToDoc(cutoutCurve, newCurve, curveRef.m_uuid)
        'Dispose
        vector.Dispose()
        xform.Dispose()
        newCurve.Dispose()
        curveRef.Dispose()
        curve.Dispose()
        fixedPoint.Dispose()
        pointToMove.Dispose()
        newPoint.Dispose()
        DeleteFilletCurve()
        Return True
    End Function


    Private Function AreCurvesParallel(ByVal type1 As eCutoutCurve, ByVal type2 As eCutoutCurve) As Boolean
        Dim curve1 As IOnCurve = GetCurveRef(type1).Curve
        Dim curve2 As IOnCurve = GetCurveRef(type2).Curve
        If curve1 Is Nothing Or curve2 Is Nothing Then Return False
        Dim v1 As New On3dVector(curve1.PointAtStart - curve1.PointAtEnd)
        Dim v2 As New On3dVector(curve2.PointAtStart - curve2.PointAtEnd)
        Dim result As Boolean = (v1.IsParallelTo(v2) <> 0)
        v1.Dispose()
        v2.Dispose()
        Return result
    End Function


    ''' <summary>
    ''' Raccorda le due curve di base generandone una sola
    ''' </summary>
    ''' <remarks>Salva l'Id della nuova curva in FilletCurveId</remarks>
    Private Function CreateFillet(Optional ByVal docRedraw As Boolean = True) As Boolean
        Try
            MRhinoView.EnableDrawing(False)
            'CONTROLLI
            If Not CurveIsInDoc(eCutoutCurve.L1) Or Not CurveIsInDoc(eCutoutCurve.L2) Then Return False
            If CurveIsInDoc(eCutoutCurve.L1L2) Then DeleteFilletCurve()
            'RECUPERO L'ARCO CHE FA DA RACCORDO
            Dim curveL1 As OnCurve = GetCurveRef(eCutoutCurve.L1).Curve.DuplicateCurve()
            Dim curveL2 As OnCurve = GetCurveRef(eCutoutCurve.L2).Curve.DuplicateCurve()
            Dim planeXY As New OnPlane(OnUtil.On_xy_plane)
            Dim fillet As OnNurbsCurve = FilletLineCurve(curveL1, curveL2, planeXY, mFilletRadiusL1L2, eFilletSide.inner)
            If fillet Is Nothing Then Return False
            Dim centerPoint As On3dPoint = GetCenterOfRotation()
            'TAGLIO LE CURVE ORIGINALI
            Dim line As New OnLineCurve(fillet.PointAtStart, fillet.PointAtEnd)
            Dim extendLine As OnCurve = line.DuplicateCurve
            Dim extensionFactor As Double = 100
            RhUtil.RhinoExtendCurve(extendLine, IRhinoExtend.Type.Line, 0, extensionFactor)
            RhUtil.RhinoExtendCurve(extendLine, IRhinoExtend.Type.Line, 1, extensionFactor)
            Dim extrusionSrf As OnSurface = RhUtil.RhinoExtrudeCurveStraight(extendLine, OnUtil.On_zaxis, extensionFactor)
            Dim xform As New OnXform
            xform.Translation(0, 0, -extensionFactor / 2)
            extrusionSrf.Transform(xform)
            '#If DEBUG Then
            '       AddDocumentToDebug(centerPoint, "centerPoint")
            '        AddDocumentToDebug(extrusionSrf, "extrusionSrf")
            '#End If            
            Dim splittedCurves As OnCurveArray = RhGeometry.SplitCurveBySurface(curveL1, extrusionSrf, New On3dPointArray)
            If splittedCurves.Count <> 2 Then Return False
            Dim splitCurveL1 As OnCurve = Nothing
            Dim t0, t1 As Double
            splittedCurves.Item(0).GetClosestPoint(centerPoint, t0)
            splittedCurves.Item(1).GetClosestPoint(centerPoint, t1)
            Dim testPoint0 As On3dPoint = splittedCurves.Item(0).PointAt(t0)
            Dim testPoint1 As On3dPoint = splittedCurves.Item(1).PointAt(t1)
            If testPoint0.DistanceTo(centerPoint) > testPoint1.DistanceTo(centerPoint) Then
                splitCurveL1 = splittedCurves.Item(0)
            Else
                splitCurveL1 = splittedCurves.Item(1)
            End If
            '#If DEBUG Then
            '        AddDocumentToDebug(splitCurveL1, "splitCurveL1")
            '#End If
            splittedCurves.Empty()
            splittedCurves = RhGeometry.SplitCurveBySurface(curveL2, extrusionSrf, New On3dPointArray)
            If splittedCurves.Count <> 2 Then Return False
            Dim splitCurveL2 As OnCurve = Nothing
            splittedCurves.Item(0).GetClosestPoint(centerPoint, t0)
            splittedCurves.Item(1).GetClosestPoint(centerPoint, t1)
            testPoint0 = splittedCurves.Item(0).PointAt(t0)
            testPoint1 = splittedCurves.Item(1).PointAt(t1)
            If testPoint0.DistanceTo(centerPoint) > testPoint1.DistanceTo(centerPoint) Then
                splitCurveL2 = splittedCurves.Item(0)
            Else
                splitCurveL2 = splittedCurves.Item(1)
            End If
            '#If DEBUG Then
            '        AddDocumentToDebug(splitCurveL2, "splitCurveL2")
            '#End If
            Dim curvesToJoin() As IOnCurve = {splitCurveL1, fillet, splitCurveL2}
            Dim mergedCurves(0) As OnCurve
            RhUtil.RhinoMergeCurves(curvesToJoin, mergedCurves)
            If mergedCurves.Length <> 1 Then Return Nothing
            FilletCurveId = RhUtil.RhinoApp.ActiveDoc.AddCurveObject(mergedCurves(0)).Attributes.m_uuid
            'DISPOSE
            curveL1.Dispose()
            curveL2.Dispose()
            planeXY.Dispose()
            line.Dispose()
            extendLine.Dispose()
            extrusionSrf.Dispose()
            xform.Dispose()
            centerPoint.Dispose()
            splittedCurves.Dispose()
            splitCurveL1.Dispose()
            splitCurveL2.Dispose()
            testPoint0.Dispose()
            testPoint1.Dispose()
            fillet.Dispose()
            Return True
        Catch ex As Exception
            MsgBox(LanguageManager.Message(230), MsgBoxStyle.Critical, My.Application.Info.Title)
            IdLanguageManager.PromptError(ex.Message)
            Return False
        Finally
            MRhinoView.EnableDrawing(True)
            If docRedraw Then RhUtil.RhinoApp.ActiveDoc.Redraw()
        End Try
    End Function


    Private Function CurveIsInDoc(ByVal cutoutCurve As eCutoutCurve) As Boolean
        Dim objRef As MRhinoObjRef = Me.GetCurveRef(cutoutCurve)
        Return (objRef IsNot Nothing AndAlso RhUtil.RhinoApp.ActiveDoc.LookupObject(objRef.m_uuid) IsNot Nothing)
    End Function

    Private Sub DeleteFilletCurve()
        If CurveIsInDoc(eCutoutCurve.L1L2) Then
            Dim objRef As MRhinoObjRef = Me.GetCurveRef(eCutoutCurve.L1L2)
            RhUtil.RhinoApp.ActiveDoc.DeleteObject(objRef)
            FilletCurveId = Guid.Empty
            objRef.Dispose()
            RhUtil.RhinoApp.ActiveDoc.Redraw()
        End If
    End Sub

    Private Sub DeleteCurve(ByVal type As eCutoutCurve)
        If CurveIsInDoc(type) Then
            Dim objRefL As MRhinoObjRef = Me.GetCurveRef(type)
            RhUtil.RhinoApp.ActiveDoc.DeleteObject(objRefL)
            objRefL.Dispose()
            RhUtil.RhinoApp.ActiveDoc.Redraw()
        End If
    End Sub

    Public Overrides Sub DeleteAllCurves() Implements ICutoutAddiction.DeleteAllCurves
        DeleteCurve(eCutoutCurve.L1)
        DeleteCurve(eCutoutCurve.L2)
        DeleteFilletCurve()
    End Sub


    ''' <summary>
    ''' Funzione specifica per il cutout
    ''' </summary>
    ''' <param name="extrusionLenght"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overloads Function CreateSrfFromCurves(ByVal layerName As String, ByVal extrusionLenght As Double) As Boolean Implements ICutoutAddiction.CreateSrfFromCurves
        If Not CurveIsInDoc(eCutoutCurve.L1L2) Then Return False
        If Not extrusionLenght > 0 Then Return False
        Dim filletCurve As IOnCurve = GetCurveRef(eCutoutCurve.L1L2).Curve()
        Dim extrusionSrf As OnSurface = RhUtil.RhinoExtrudeCurveStraight(filletCurve, New On3dVector(0, 0, -1), extrusionLenght)
        If extrusionSrf Is Nothing Then Return False
        Dim xform As New OnXform
        xform.Translation(0, 0, extrusionLenght / 2)
        extrusionSrf.Transform(xform)
        'INSERISCO NEL DOC
        RhUtil.RhinoApp().RunScript("_SelNone", 0)
        RhLayer.RendiCorrenteLayer(layerName)
        Me.SurfaceID = RhUtil.RhinoApp.ActiveDoc.AddSurfaceObject(extrusionSrf).Attributes.m_uuid
        RhUtil.RhinoApp().RunScript("_SelNone", 0)
        CheckSurfaceDirection(New MRhinoObjRef(Me.SurfaceID), eDirectionCheck.lateral360)
        'DISPOSE
        xform.Dispose()
        extrusionSrf.Dispose()
        Return True
    End Function


    Private Function MaxFilletRadius() As Decimal
        'IN REALTÀ IL VALORE MASSIMO DIPENDE DALL'ANGOLO TRA LE CURVE E DA UNA CERTA TOLLERANZA 
        'PERCHÈ LA CURVA PIÙ CORTA PUÒ DEGENERARE IN UN PUNTO DOPO IL TRIM
        'PER SEMPLICITÀ IMPOSTO IL MINIMO DELLA LUNGHEZZA DELLE DUE CURVE
        If Me.Type <> eAddictionType.cutout Then Return -1
        Dim lenghtL1, lenghtL2 As Double
        Me.GetCurveRef(eCutoutCurve.L1).Curve.GetLength(lenghtL1)
        Me.GetCurveRef(eCutoutCurve.L2).Curve.GetLength(lenghtL2)
        Return Convert.ToDecimal(Math.Min(lenghtL1, lenghtL2))
    End Function



#End Region


#Region " IClonable "

    Public Overrides Function Clone() As Object
        Dim plainObject As IdAddiction = IdAddictionFactory.Create(Me.Side, Me.Type, Me.Model, Me.Size, Me.CutoutDirection)
        MyBase.CloneCommonField(plainObject)
        'Campi specifici IdCutoutToTalAddiction
        Dim result As IdCutoutToTalAddiction = DirectCast(plainObject, IdCutoutToTalAddiction)
        CloneCommonCutoutField(result)
        Return result
    End Function

    Public Overridable Sub CloneCommonCutoutField(ByRef ohterAddiction As IdCutoutToTalAddiction)
        ohterAddiction.mFilletRadiusL1L2 = Me.mFilletRadiusL1L2
        ohterAddiction.mCutoutDirection = Me.mCutoutDirection
    End Sub


#End Region



End Class
