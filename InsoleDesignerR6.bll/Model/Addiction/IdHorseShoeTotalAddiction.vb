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


Public Class IdHorseShoeTotalAddiction
    Inherits AbstractCutoutCommons
    Implements ICutoutAddiction



#Region " UUID INIZIALI - MANTENUTI SOLO PER OTTIMIZZAZIONE MA SE NON ALLINEATI VIENE COMUNQUE FATTO RICONOSCIMENTO GEOMETRICO "


    Public Property HORSESHOE_DX_L1L2 As String = "8aa9057b-4a54-4bb2-9721-79d7c184f3bc"
    Public Property HORSESHOE_DX_L1 As String = "0ae59f4e-42e9-48b7-a8ba-60d85c64b0da"
    Public Property HORSESHOE_DX_L2 As String = "0ab4dbe8-02cd-4b37-9f37-addd010d09e7"
    Public Property HORSESHOE_DX_L3 As String = "4ea0b0f4-566f-4f9c-a577-e84762123001"
    Public Property HORSESHOE_DX_L4 As String = "cddf5e4f-df4d-4f23-8d63-0cc3aceca1f7"
    Public Property HORSESHOE_SN_L1L2 As String = "485112d5-663b-49b2-9e51-6f312ed26459"
    Public Property HORSESHOE_SN_L1 As String = "4b45ba4d-7198-47f5-99ac-fbaca85a01eb"
    Public Property HORSESHOE_SN_L2 As String = "8fd2c4f4-55d7-4a58-ad5f-1068a4b110a7"
    Public Property HORSESHOE_SN_L3 As String = "7dcf5a47-33b9-4b85-9d33-06c25f8f5810"
    Public Property HORSESHOE_SN_L4 As String = "14e71a93-fa99-4ef1-ac94-e3287ed06608"

#End Region


#Region " FIELD "

    Public UniqueCurveId As Guid
    Protected mFilletRadiusL1L2 As Double
    Protected mFilletRadiusL1L3 As Double
    Protected mFilletRadiusL2L4 As Double
    Protected mFilletIdL1L3 As Guid
    Protected mFilletIdL2L4 As Guid

#End Region


#Region " Constructor "


    Public Sub New(ByVal side As eSide)
        Me.Type = eAddictionType.horseShoe
        Me.Side = side
        CurvesID = New List(Of Guid)
        SetModel()
        SetFileName()
    End Sub

    Protected Overrides Sub SetDefault()
        'Valore default ricavato dal file cad della procedura
        SetAllFilletRadius(15.0)
    End Sub

    Protected Overridable Sub SetModel()
        Me.Model = eAddictionModel.horseShoeTotal
    End Sub


#End Region


#Region " Property "


    Public Sub SetAllFilletRadius(ByVal value As Double)
        mFilletRadiusL1L2 = value
        mFilletRadiusL1L3 = value
        mFilletRadiusL2L4 = value
    End Sub

    Public Property FilletRadius(ByVal horseShoeFillet As eHorseShoeFilletCrv) As Double Implements ICutoutAddiction.FilletRadius
        Get
            Select Case horseShoeFillet
                Case eHorseShoeFilletCrv.L1L2
                    Return mFilletRadiusL1L2
                Case eHorseShoeFilletCrv.L1L3
                    Return mFilletRadiusL1L3
                Case eHorseShoeFilletCrv.L2L4
                    Return mFilletRadiusL2L4
                Case Else
                    Return -1
            End Select
        End Get
        Set(value As Double)
            Select Case horseShoeFillet
                Case eHorseShoeFilletCrv.L1L2
                    mFilletRadiusL1L2 = value
                Case eHorseShoeFilletCrv.L1L3
                    mFilletRadiusL1L3 = value
                Case eHorseShoeFilletCrv.L2L4
                    mFilletRadiusL2L4 = value
            End Select
        End Set
    End Property

    Public Property FilletId(ByVal horseShoeFillet As eHorseShoeFilletCrv) As Guid
        Get
            Select Case horseShoeFillet
                Case eHorseShoeFilletCrv.L1L2
                    Dim result As Guid
                    If Me.Side = eSide.right Then
                        Guid.TryParse(HORSESHOE_DX_L1L2, result)
                    Else
                        Guid.TryParse(HORSESHOE_SN_L1L2, result)
                    End If
                    Return result
                Case eHorseShoeFilletCrv.L1L3
                    Return mFilletIdL1L3
                Case eHorseShoeFilletCrv.L2L4
                    Return mFilletIdL2L4
                Case Else
                    Return Guid.Empty
            End Select
        End Get
        Set(value As Guid)
            Select Case horseShoeFillet
                Case eHorseShoeFilletCrv.L1L2
                    If Me.Side = eSide.right Then
                        HORSESHOE_DX_L1L2 = value.ToString
                    Else
                        HORSESHOE_SN_L1L2 = value.ToString
                    End If
                Case eHorseShoeFilletCrv.L1L3
                    mFilletIdL1L3 = value
                Case eHorseShoeFilletCrv.L2L4
                    mFilletIdL2L4 = value
            End Select
        End Set
    End Property


#End Region


#Region " Nomi file e oggetti "


    Public Overloads Shared Function GetFileName(ByVal side As eSide, ByVal type As eAddictionType, ByVal model As eAddictionModel) As String
        Dim basePath As String = My.Application.Info.DirectoryPath
        'Assemblaggio stringhe
        Dim filename As String = ""
        filename = GetFileTypeString(type) & GetFileSideString(side)
        Return Path.Combine(LibraryManager.GetDirectory(LibraryManager.eDirectoryLibrary.addiction), filename & ".3dm")
    End Function


    Protected Overrides Sub SetFileName()
        mOriginFileName = GetFileName(Me.Side, Me.Type, Me.Model)
#If DEBUG Then
        If Not File.Exists(mOriginFileName) Then MsgBox("Non esiste uno scarico con le caratteristiche richieste", MsgBoxStyle.Exclamation, My.Application.Info.Title)
#End If
    End Sub


    Public Shared Function GetCurveName(ByVal horseShoeCurve As eHorseShoeCrv) As String
        Return [Enum].GetName(GetType(eHorseShoeCrv), horseShoeCurve)
    End Function


    Public Function GetCurveID(ByVal horseShoeCurve As eHorseShoeCrv) As Guid Implements ICutoutAddiction.GetCurvesID
        Dim stringId As String = ""
        Select Case horseShoeCurve
            Case eHorseShoeCrv.L1L2
                If Me.Side = eSide.left Then
                    stringId = HORSESHOE_SN_L1L2
                Else
                    stringId = HORSESHOE_DX_L1L2
                End If
            Case eHorseShoeCrv.L1
                If Me.Side = eSide.left Then
                    stringId = HORSESHOE_SN_L1
                Else
                    stringId = HORSESHOE_DX_L1
                End If
            Case eHorseShoeCrv.L2
                If Me.Side = eSide.left Then
                    stringId = HORSESHOE_SN_L2
                Else
                    stringId = HORSESHOE_DX_L2
                End If
            Case eHorseShoeCrv.L3
                If Me.Side = eSide.left Then
                    stringId = HORSESHOE_SN_L3
                Else
                    stringId = HORSESHOE_DX_L3
                End If
            Case eHorseShoeCrv.L4
                If Me.Side = eSide.left Then
                    stringId = HORSESHOE_SN_L4
                Else
                    stringId = HORSESHOE_DX_L4
                End If
        End Select
        Dim result As Guid
        Guid.TryParse(stringId.Trim(), result)
        Return result
    End Function


    Public Sub UpdateCurveId(ByVal horseShoeCrv As eHorseShoeCrv, ByVal oldId As Guid, ByVal newId As String) Implements ICutoutAddiction.UpdateCurveId
        Dim uuid As Guid
        Guid.TryParse(newId, uuid)
        Select Case horseShoeCrv
            Case eHorseShoeCrv.L1L2
                If Me.Side = eSide.right Then
                    HORSESHOE_DX_L1L2 = newId
                Else
                    HORSESHOE_SN_L1L2 = newId
                End If
            Case eHorseShoeCrv.L1L3, eHorseShoeCrv.L2L4
                FilletId(ConvertEnumCrv(horseShoeCrv)) = uuid
            Case eHorseShoeCrv.Unique
                UniqueCurveId = uuid
                ClearCurvesID()
                Me.CurvesID.Add(uuid)
            Case eHorseShoeCrv.L1
                If Me.Side = eSide.right Then
                    HORSESHOE_DX_L1 = newId
                Else
                    HORSESHOE_SN_L1 = newId
                End If
            Case eHorseShoeCrv.L2
                If Me.Side = eSide.right Then
                    HORSESHOE_DX_L2 = newId
                Else
                    HORSESHOE_SN_L2 = newId
                End If
            Case eHorseShoeCrv.L3
                If Me.Side = eSide.right Then
                    HORSESHOE_DX_L3 = newId
                Else
                    HORSESHOE_SN_L3 = newId
                End If
            Case eHorseShoeCrv.L4
                If Me.Side = eSide.right Then
                    HORSESHOE_DX_L4 = newId
                Else
                    HORSESHOE_SN_L4 = newId
                End If
        End Select
        Select Case horseShoeCrv
            Case eHorseShoeCrv.L1L2, eHorseShoeCrv.L1, eHorseShoeCrv.L2, eHorseShoeCrv.L3, eHorseShoeCrv.L4
                Me.ReplaceId(oldId, uuid)
            Case eHorseShoeCrv.Unique
                Me.CurvesID.Clear()
                Me.CurvesID.Add(uuid)
        End Select
    End Sub


    Public Sub ClearCurvesID() Implements ICutoutAddiction.ClearCurveID
        HORSESHOE_DX_L1L2 = ""
        HORSESHOE_SN_L1L2 = ""
        HORSESHOE_DX_L1 = ""
        HORSESHOE_DX_L2 = ""
        HORSESHOE_DX_L3 = ""
        HORSESHOE_DX_L4 = ""
        HORSESHOE_SN_L1 = ""
        HORSESHOE_SN_L2 = ""
        HORSESHOE_SN_L3 = ""
        HORSESHOE_SN_L4 = ""
        mFilletIdL1L3 = Guid.Empty
        mFilletIdL2L4 = Guid.Empty
        Me.CurvesID.Clear()
    End Sub


#End Region


#Region " CAD Method ICutout "



    Public Function GetCurveToExtrude() As IOnCurve Implements ICutoutAddiction.GetCurveToExtrude
        If Not CurveIsInDoc(eHorseShoeCrv.Unique) Then Return Nothing
        Return GetCurveRef(eHorseShoeCrv.Unique).Curve
    End Function


    ''' <summary>
    ''' Aggiunge la curva mantenendo i nomi L1-L2
    ''' </summary>
    ''' <param name="horseShoeCrv"></param>
    ''' <param name="curve"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function AddCurveToDoc(ByVal horseShoeCrv As eHorseShoeCrv, ByVal curve As OnCurve, ByVal oldId As Guid) As MRhinoCurveObject Implements ICutoutAddiction.AddCurveToDoc
        If CurveIsInDoc(horseShoeCrv) Then DeleteCurve(horseShoeCrv)
        Select Case horseShoeCrv
            Case eHorseShoeCrv.L1, eHorseShoeCrv.L2, eHorseShoeCrv.L3, eHorseShoeCrv.L4, eHorseShoeCrv.Unique
                RhLayer.RendiCorrenteLayer(GetLayerName(Me.Side, eLayerType.addiction))
            Case eHorseShoeCrv.L1L2, eHorseShoeCrv.L1L3, eHorseShoeCrv.L2L4
                RhLayer.RendiCorrenteLayer(GetLayerName(Me.Side, eLayerType.root))
        End Select
        Dim curveObj As MRhinoCurveObject = RhUtil.RhinoApp.ActiveDoc.AddCurveObject(curve)
        Dim objMinYAttr As New MRhinoObjectAttributes(curveObj.Attributes)
        objMinYAttr.m_name = GetCurveName(horseShoeCrv)
        RhUtil.RhinoApp.ActiveDoc.ModifyObjectAttributes(New MRhinoObjRef(curveObj.Attributes.m_uuid), objMinYAttr)
        RhUtil.RhinoApp.RunScript("_SelNone", 0)
        'Aggiorno ID
        UpdateCurveId(horseShoeCrv, oldId, curveObj.Attributes.m_uuid.ToString)
        Return curveObj
    End Function


    Public Function CurveIsInDoc(ByVal horseShoeCrv As eHorseShoeCrv) As Boolean Implements ICutoutAddiction.CurveIsInDoc
        Dim objRef As MRhinoObjRef = Me.GetCurveRef(horseShoeCrv)
        Return (objRef IsNot Nothing AndAlso RhUtil.RhinoApp.ActiveDoc.LookupObject(objRef.m_uuid) IsNot Nothing)
    End Function


    Public Function GetCurveRef(ByVal horseShoeCrv As eHorseShoeCrv) As MRhinoObjRef Implements ICutoutAddiction.GetCurveRef
        'Se ho già raccordato ritorno la curva unica
        If horseShoeCrv = eHorseShoeCrv.Unique Then Return New MRhinoObjRef(Me.UniqueCurveId)
        If Me.CurvesID.Count = 1 Then Return Nothing
        If horseShoeCrv = eHorseShoeCrv.L1L2 Or horseShoeCrv = eHorseShoeCrv.L1L3 Or horseShoeCrv = eHorseShoeCrv.L2L4 Then
            Return New MRhinoObjRef(FilletId(ConvertEnumCrv(horseShoeCrv)))
        End If
        Dim result As MRhinoObjRef = Nothing
        'Prima cerco con GUID
        Dim curveId As Guid = GetCurveID(horseShoeCrv)
        Dim obj As IOnObject = RhUtil.RhinoApp.ActiveDoc.LookupDocumentObject(curveId, True)
        If obj IsNot Nothing AndAlso obj.ObjectType = IOn.object_type.curve_object Then Return New MRhinoObjRef(curveId)
        'Poi cerco oggetti per nome
        For Each uuid As Guid In Me.CurvesID
            result = New MRhinoObjRef(uuid)
            If result.Object IsNot Nothing AndAlso result.Object.Attributes.m_name = GetCurveName(horseShoeCrv) Then Return result
            result.Dispose()
        Next
        ''RICONOSCIMENTO GEOMETRICO
        'Divido le curve in base al numero dei punti di controllo
        Dim straightCrv As New List(Of MRhinoObjRef)
        Dim filletCrv As New List(Of MRhinoObjRef)
        For Each uuid As Guid In Me.CurvesID
            result = New MRhinoObjRef(uuid)
            If result IsNot Nothing AndAlso result.Curve IsNot Nothing Then
                If result.Curve.NurbsCurve.CVCount > 2 Then
                    filletCrv.Add(result)
                Else
                    straightCrv.Add(result)
                End If
            End If
        Next
        result = Nothing
        'Vado per esclusione
        Dim objrefL0, objrefL1, objrefL2, objrefL3, objrefL4 As New MRhinoObjRef
        'L3 e L4 hanno rispettivamente minY e maxY
        Dim minY As Double = Double.MaxValue
        Dim maxY As Double = Double.MinValue
        For Each objref As MRhinoObjRef In straightCrv
            If objref.Object.BoundingBox.m_max.y > maxY Then
                maxY = objref.Object.BoundingBox.m_max.y
                objrefL4 = New MRhinoObjRef(objref)
            End If
            If objref.Object.BoundingBox.m_min.y < minY Then
                minY = objref.Object.BoundingBox.m_min.y
                objrefL3 = New MRhinoObjRef(objref)
            End If
        Next
        If horseShoeCrv = eHorseShoeCrv.L3 Then Return objrefL3
        If horseShoeCrv = eHorseShoeCrv.L4 Then Return objrefL4
        minY = Double.MaxValue
        maxY = Double.MinValue
        'Tra L1 e L2 il primo ha minY l'altro maxY
        For Each objref As MRhinoObjRef In straightCrv
            If objref.m_uuid <> objrefL0.m_uuid And objref.m_uuid <> objrefL3.m_uuid And objref.m_uuid <> objrefL4.m_uuid Then
                If objref.Object.BoundingBox.m_max.y > maxY Then
                    maxY = objref.Object.BoundingBox.m_max.y
                    objrefL2 = New MRhinoObjRef(objref)
                End If
                If objref.Object.BoundingBox.m_min.y < minY Then
                    minY = objref.Object.BoundingBox.m_min.y
                    objrefL1 = New MRhinoObjRef(objref)
                End If
            End If
        Next
        If horseShoeCrv = eHorseShoeCrv.L1 Then Return objrefL1
        Return Nothing
    End Function

    Public Function AreCurvesParallel(ByVal type1 As eHorseShoeStraightCrv, ByVal type2 As eHorseShoeStraightCrv) As Boolean Implements ICutoutAddiction.AreCurvesParallel
        Dim curve1 As IOnCurve = GetCurveRef(ConvertEnumCurve(type1)).Curve
        Dim curve2 As IOnCurve = GetCurveRef(ConvertEnumCurve(type2)).Curve
        Dim v1 As New On3dVector(curve1.PointAtStart - curve1.PointAtEnd)
        Dim v2 As New On3dVector(curve2.PointAtStart - curve2.PointAtEnd)
        Dim result As Boolean = (v1.IsParallelTo(v2) <> 0)
        v1.Dispose()
        v2.Dispose()
        Return result
    End Function

    Public Function CreateFillet(ByVal filletCrv As eHorseShoeFilletCrv, Optional ByVal docRedraw As Boolean = True) As Boolean Implements ICutoutAddiction.CreateFillet
        Try
            MRhinoView.EnableDrawing(False)
            Dim type1 As eHorseShoeStraightCrv
            Dim type2 As eHorseShoeStraightCrv
            'Controlli
            Select Case filletCrv
                Case eHorseShoeFilletCrv.L1L2
                    type1 = eHorseShoeStraightCrv.L1
                    type2 = eHorseShoeStraightCrv.L2
                Case eHorseShoeFilletCrv.L1L3
                    type1 = eHorseShoeStraightCrv.L1
                    type2 = eHorseShoeStraightCrv.L3
                Case eHorseShoeFilletCrv.L2L4
                    type1 = eHorseShoeStraightCrv.L2
                    type2 = eHorseShoeStraightCrv.L4
            End Select
            If Not CurveIsInDoc(ConvertEnumCurve(type1)) Or Not CurveIsInDoc(ConvertEnumCurve(type2)) Then Return False
            Dim curve1 As OnCurve = GetCurveRef(ConvertEnumCurve(type1)).Curve.DuplicateCurve()
            Dim curve2 As OnCurve = GetCurveRef(ConvertEnumCurve(type2)).Curve.DuplicateCurve()
            Dim oldId As Guid = GetCurveRef(ConvertEnumCurve(filletCrv)).m_uuid
            Dim fillet As New OnNurbsCurve
            'Nei casi L1L3 e L2L4 cerco di mantenere il raggio di raccordo e posso escludere che le curve siano parallele
            If filletCrv = eHorseShoeFilletCrv.L1L3 Or filletCrv = eHorseShoeFilletCrv.L2L4 Then
                Dim planeXY As New OnPlane(OnUtil.On_xy_plane)
                fillet = FilletLineCurve(curve1, curve2, planeXY, FilletRadius(filletCrv), eFilletSide.inner)
                If fillet Is Nothing Then Return False
                '                planeXY.Dispose()
                '                If fillet Is Nothing Then Return False
                '                'Taglio le curve originali
                '                Dim cutterline As New OnLineCurve(fillet.PointAtStart, fillet.PointAtEnd)
                '                Dim extendCutterLine As OnCurve = cutterline.DuplicateCurve
                '                Dim extensionFactor As Double = 100
                '                RhUtil.RhinoExtendCurve(extendCutterLine, IRhinoExtend.Type.Line, 0, extensionFactor)
                '                RhUtil.RhinoExtendCurve(extendCutterLine, IRhinoExtend.Type.Line, 1, extensionFactor)
                '                Dim extrusionSrf As OnSurface = RhUtil.RhinoExtrudeCurveStraight(extendCutterLine, OnUtil.On_zaxis, extensionFactor)
                '                Dim xform As New OnXform
                '                xform.Translation(0, 0, -extensionFactor / 2)
                '                extrusionSrf.Transform(xform)
                '                Dim intersectionPoint As On3dPoint = Nothing
                '                intersectionPoint = Me.GetCenterOfRotation(type2)
                '                Dim splittedCurves As OnCurveArray = SplitCurveBySurface(curve1, extrusionSrf, New On3dPointArray)
                '#If DEBUG Then
                '                'AddDocumentToDebug(extendedCrv, "extendedCrv")
                '                'AddDocumentToDebug(extrusionSrf, "extrusionSrf")
                '                'AddDocumentToDebug(intersectionPoint, "centerPoint " & filletCrv.ToString)
                '                'Then AddDocumentToDebug(splittedCurves, "splittedCurves")
                '#End If
                '                If splittedCurves.Count <> 2 Then Return False
                '                Dim splitCurve1 As OnCurve = Nothing
                '                Dim t0, t1 As Double
                '                splittedCurves.Item(0).GetClosestPoint(intersectionPoint, t0)
                '                splittedCurves.Item(1).GetClosestPoint(intersectionPoint, t1)
                '                Dim testPoint0 As On3dPoint = splittedCurves.Item(0).PointAt(t0)
                '                Dim testPoint1 As On3dPoint = splittedCurves.Item(1).PointAt(t1)
                '                If testPoint0.DistanceTo(intersectionPoint) > testPoint1.DistanceTo(intersectionPoint) Then
                '                    splitCurve1 = splittedCurves.Item(0)
                '                Else
                '                    splitCurve1 = splittedCurves.Item(1)
                '                End If
                '#If DEBUG Then
                '                'AddDocumentToDebug(splitCurve1, "splitCurve")
                '#End If
                '                splittedCurves.Empty()
                '                splittedCurves = SplitCurveBySurface(curve2, extrusionSrf, New On3dPointArray)
                '                If splittedCurves.Count <> 2 Then Return False
                '                Dim splitCurve2 As OnCurve = Nothing
                '                splittedCurves.Item(0).GetClosestPoint(intersectionPoint, t0)
                '                splittedCurves.Item(1).GetClosestPoint(intersectionPoint, t1)
                '                testPoint0 = splittedCurves.Item(0).PointAt(t0)
                '                testPoint1 = splittedCurves.Item(1).PointAt(t1)
                '                If testPoint0.DistanceTo(intersectionPoint) > testPoint1.DistanceTo(intersectionPoint) Then
                '                    splitCurve2 = splittedCurves.Item(0)
                '                Else
                '                    splitCurve2 = splittedCurves.Item(1)
                '                End If
                '#If DEBUG Then
                '                'AddDocumentToDebug(splitCurve2, "splitCurve")
                '#End If
                '                Dim curvesToJoin() As IOnCurve = {splitCurve1, fillet, splitCurve2}
                '                Dim mergedCurves(0) As OnCurve
                '                RhUtil.RhinoMergeCurves(curvesToJoin, mergedCurves)
                '                If mergedCurves.Length <> 1 Then Return Nothing
                'fillet = mergedCurves(0).NurbsCurve
                ''Dispose
                'cutterline.Dispose()
                'extendCutterLine.Dispose()
                'extrusionSrf.Dispose()
                'xform.Dispose()
                'splittedCurves.Dispose()
                'splitCurve1.Dispose()
                'splitCurve2.Dispose()
                'testPoint0.Dispose()
                'testPoint1.Dispose()
                'intersectionPoint.Dispose()
            Else
                'Nel caso L1L2 per mantenere la tangenza non so il raggio, ma sogli estremi quindi il procedimento è diverso:
                'il raccordo non è un arco ma una curva di raggio variabile per mantenere la tangenza e raccordare esattamente negli estremi                
                Dim point1 As On3dPoint = GetCenterOfRotation(eHorseShoeStraightCrv.L1)
                Dim point2 As On3dPoint = GetCenterOfRotation(eHorseShoeStraightCrv.L2)
                'Calcolo il terzo punto del raccordo con Y che è la metà della differenza
                Dim coordY As Double = Math.Min(point1.y, point2.y) + Math.Abs(point1.y - point2.y) / 2
                Dim thirdPoint As New On3dPoint(point1.x - FilletRadius(filletCrv), coordY, point1.z)
#If DEBUG Then
                'AddDocumentToDebug(point1, "point1")
                'AddDocumentToDebug(point2, "point2")
                'AddDocumentToDebug(thirdPoint, "thirdPoint")
#End If
                'Creo l'arco per i 3 punti
                Dim arc As New OnArc(point1, thirdPoint, point2)
                arc.GetNurbForm(fillet)
                'Dispose
                point1.Dispose()
                point2.Dispose()
                thirdPoint.Dispose()
            End If
            Me.AddCurveToDoc(ConvertEnumCurve(filletCrv), fillet, oldId)
            'Dispose
            curve1.Dispose()
            curve2.Dispose()
            fillet.Dispose()
            Return True
        Catch ex As Exception
            MsgBox(LanguageManager.Message(231), MsgBoxStyle.Critical, My.Application.Info.Title)
            IdLanguageManager.PromptError(ex.Message)
            Return False
        Finally
            MRhinoView.EnableDrawing(True)
            If docRedraw Then RhUtil.RhinoApp.ActiveDoc.Redraw()
        End Try
    End Function


    Public Sub RotateCurve(ByVal straightCrv As eHorseShoeStraightCrv, ByVal angle As Double, Optional ByVal rotationCenter As On3dPoint = Nothing) Implements ICutoutAddiction.RotateCurve
        Dim xForm As New OnXform()
        Dim radiantAngle As Double = angle * Math.PI / 180
        If rotationCenter Is Nothing Then rotationCenter = GetCenterOfRotation(straightCrv)
#If DEBUG Then
        'AddDocumentToDebug(rotationCenter, "rotationCenter")
#End If
        xForm.Rotation(radiantAngle, OnUtil.On_zaxis, rotationCenter)
        Dim enumCurve As eHorseShoeCrv = ConvertEnumCurve(straightCrv)
        Dim curve As OnCurve = GetCurveRef(enumCurve).Curve.DuplicateCurve()
        curve.Transform(xForm)
        ''ELIMINO LA VECCHIA CURVA DAL DOC, AGGIUNGO LA NUOVE AL DOC E AGGIORNO ID CURVE
        AddCurveToDoc(enumCurve, curve, GetCurveRef(enumCurve).m_uuid)
        curve.Dispose()
        xForm.Dispose()
        Select Case straightCrv
            Case eHorseShoeStraightCrv.L1, eHorseShoeStraightCrv.L3
                DeleteCurve(eHorseShoeCrv.L1L3)
            Case eHorseShoeStraightCrv.L2, eHorseShoeStraightCrv.L4
                DeleteCurve(eHorseShoeCrv.L2L4)
        End Select
    End Sub


    ''' <summary>
    ''' Se sono parallele per le curve L1 e L2 basta vedere l'estremo con X minore(L1/L2)/maggiore(L3/L4), altrimenti prendo l'estremo più vicino alla curva da raccordare
    ''' </summary>
    ''' <param name="straightCrv"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetCenterOfRotation(ByVal straightCrv As eHorseShoeStraightCrv) As On3dPoint Implements ICutoutAddiction.GetCenterOfRotation
        Dim curve As IOnCurve = GetCurveRef(ConvertEnumCurve(straightCrv)).Curve
        If curve Is Nothing Then Return Nothing
        Select Case straightCrv
            Case eHorseShoeStraightCrv.L1, eHorseShoeStraightCrv.L2
                If curve.PointAtStart.x < curve.PointAtEnd.x Then
                    Return curve.PointAtStart
                Else
                    Return curve.PointAtEnd
                End If
            Case eHorseShoeStraightCrv.L3
                If curve.PointAtStart.y > curve.PointAtEnd.y Then
                    Return curve.PointAtStart
                Else
                    Return curve.PointAtEnd
                End If
            Case eHorseShoeStraightCrv.L4
                If curve.PointAtStart.y < curve.PointAtEnd.y Then
                    Return curve.PointAtStart
                Else
                    Return curve.PointAtEnd
                End If
            Case Else
                Return Nothing
        End Select
    End Function


    Public Function SetCurveLenght(ByVal straightCrv As eHorseShoeStraightCrv, ByVal totLenght As Double) As Boolean Implements ICutoutAddiction.SetCurveLenght
        Dim curveRef As MRhinoObjRef = GetCurveRef(ConvertEnumCurve(straightCrv))
        Dim curve As OnCurve = curveRef.Curve().DuplicateCurve()
        If curve Is Nothing Then Return False
        Dim fixedPoint As On3dPoint = Nothing
        Dim pointToMove As On3dPoint = Nothing
        Select Case straightCrv
            Case eHorseShoeStraightCrv.L1, eHorseShoeStraightCrv.L2
                'il fisso è quello con X minore
                If curve.PointAtStart.x < curve.PointAtEnd.x Then
                    fixedPoint = curve.PointAtStart
                    pointToMove = curve.PointAtEnd
                Else
                    fixedPoint = curve.PointAtEnd
                    pointToMove = curve.PointAtStart
                End If
            Case eHorseShoeStraightCrv.L3
                'List fisso è quello con Y maggiore
                If curve.PointAtStart.y > curve.PointAtEnd.y Then
                    fixedPoint = curve.PointAtStart
                    pointToMove = curve.PointAtEnd
                Else
                    fixedPoint = curve.PointAtEnd
                    pointToMove = curve.PointAtStart
                End If
            Case eHorseShoeStraightCrv.L4
                'List fisso è quello con Y minore
                If curve.PointAtStart.y < curve.PointAtEnd.y Then
                    fixedPoint = curve.PointAtStart
                    pointToMove = curve.PointAtEnd
                Else
                    fixedPoint = curve.PointAtEnd
                    pointToMove = curve.PointAtStart
                End If
        End Select
        Dim vector As New On3dVector(pointToMove - fixedPoint)
        vector.Unitize()
        Dim xform As New OnXform
        xform.Scale(fixedPoint, totLenght)
        vector.Transform(xform)
        Dim newPoint = New On3dPoint(vector.x + fixedPoint.x, vector.y + fixedPoint.y, vector.z + fixedPoint.z)
        Dim newCurve As New OnLineCurve(fixedPoint, newPoint)
        'Aggiorno        
        Me.AddCurveToDoc(ConvertEnumCurve(straightCrv), newCurve, curveRef.m_uuid)
        'Nel caso delle curve L1 e L2 la modifica porta ad uno spostamento rispettivamente delle curve L3 e L4
        If straightCrv = eHorseShoeStraightCrv.L1 Or straightCrv = eHorseShoeStraightCrv.L2 Then
            Dim otherCrv As eHorseShoeStraightCrv
            If straightCrv = eHorseShoeStraightCrv.L1 Then
                otherCrv = eHorseShoeStraightCrv.L3
            Else
                otherCrv = eHorseShoeStraightCrv.L4
            End If
            curveRef = GetCurveRef(ConvertEnumCurve(otherCrv))
            curve = curveRef.Curve().DuplicateCurve()
            If curve Is Nothing Then Return False
            xform.Identity()
            xform.Translation(newPoint.x - pointToMove.x, newPoint.y - pointToMove.y, newPoint.z - pointToMove.z)
            curve.Transform(xform)
            Me.AddCurveToDoc(ConvertEnumCurve(otherCrv), curve, curveRef.m_uuid)
        End If
        'Dispose
        vector.Dispose()
        xform.Dispose()
        fixedPoint.Dispose()
        pointToMove.Dispose()
        newPoint.Dispose()
        curve.Dispose()
        newCurve.Dispose()
        Return True
    End Function


    Public Sub DeleteCurve(ByVal eHorseShoeCrv As eHorseShoeCrv) Implements ICutoutAddiction.DeleteCurve
        If Not CurveIsInDoc(eHorseShoeCrv) Then Exit Sub
        If eHorseShoeCrv = eHorseShoeCrv.Unique Then
            Dim objRef As New MRhinoObjRef(Me.UniqueCurveId)
            RhUtil.RhinoApp.ActiveDoc.DeleteObject(objRef)
            UniqueCurveId = Guid.Empty
            objRef.Dispose()
        Else
            Dim objRef As MRhinoObjRef = GetCurveRef(eHorseShoeCrv)
            RhUtil.RhinoApp.ActiveDoc.DeleteObject(objRef)
            'If Me.CurvesID.Contains(objRef.m_uuid) Then Me.CurvesID.Remove(objRef.m_uuid)
            objRef.Dispose()
        End If
    End Sub


    Public Overrides Sub DeleteAllCurves() Implements ICutoutAddiction.DeleteAllCurves
        For Each item As eHorseShoeCrv In [Enum].GetValues(GetType(eHorseShoeCrv))
            DeleteCurve(item)
        Next
    End Sub


    Public Overloads Function CreateSrfFromCurves(ByVal layerName As String, ByVal extrusionLenght As Double) As Boolean Implements ICutoutAddiction.CreateSrfFromCurves
        If Not CurveIsInDoc(eHorseShoeCrv.Unique) Then Return False
        If Not extrusionLenght > 0 Then Return False
        Dim uniqueCurve As IOnCurve = GetCurveRef(eHorseShoeCrv.Unique).Curve()
        'IN CERTI CASI SI POTREBBE CREARE UNA POLISUPERFICIE CHE DA PROBLEMI QUINDI FACCIO UN REBUILD PER ASSICURARMI DI AVERE UNA SUPERFICIE
        Dim rebuildedCrv As OnNurbsCurve = RhUtil.RhinoRebuildCurve(uniqueCurve, 3, 99)
        Dim extrusionSrf As OnSurface = RhUtil.RhinoExtrudeCurveStraight(rebuildedCrv, New On3dVector(0, 0, -1), extrusionLenght)
        If extrusionSrf Is Nothing Then Return False
        Dim xform As New OnXform
        xform.Translation(0, 0, extrusionLenght / 2)
        extrusionSrf.Transform(xform)
        'Inserisco nel Doc
        RhUtil.RhinoApp().RunScript("_SelNone", 0)
        RhLayer.RendiCorrenteLayer(layerName)
        Me.SurfaceID = RhUtil.RhinoApp.ActiveDoc.AddSurfaceObject(extrusionSrf).Attributes.m_uuid
        RhUtil.RhinoApp().RunScript("_SelNone", 0)
        CheckSurfaceDirection(New MRhinoObjRef(Me.SurfaceID), eDirectionCheck.lateral360)
        'Dispose
        xform.Dispose()
        rebuildedCrv.Dispose()
        extrusionSrf.Dispose()
        Return True
    End Function


    ''' <summary>
    ''' Per semplicità imposto il massimo raccordo al minimo della lunghezza delle due curve
    ''' </summary>
    ''' <param name="filletCrv"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function MaxFilletRadius(ByVal filletCrv As eHorseShoeFilletCrv) As Decimal Implements ICutoutAddiction.MaxFilletRadius
        Dim curve1 As eHorseShoeStraightCrv
        Dim curve2 As eHorseShoeStraightCrv
        Select Case filletCrv
            Case eHorseShoeFilletCrv.L1L2
                curve1 = eHorseShoeStraightCrv.L1
                curve2 = eHorseShoeStraightCrv.L2
            Case eHorseShoeFilletCrv.L1L3
                curve1 = eHorseShoeStraightCrv.L1
                curve2 = eHorseShoeStraightCrv.L3
            Case eHorseShoeFilletCrv.L2L4
                curve1 = eHorseShoeStraightCrv.L2
                curve2 = eHorseShoeStraightCrv.L4
        End Select
        Dim lenghtL1, lenghtL2 As Double
        Me.GetCurveRef(ConvertEnumCurve(curve1)).Curve.GetLength(lenghtL1)
        Me.GetCurveRef(ConvertEnumCurve(curve2)).Curve.GetLength(lenghtL2)
        Return Convert.ToDecimal(Math.Min(lenghtL1, lenghtL2))
    End Function


#End Region


#Region " CAD Method Specifiche "


    Public Overrides Sub SelectCurves()
        If AreAllCurvesInDocument() Then
            Dim allCurves As New List(Of Guid)
            allCurves.AddRange(Me.CurvesID)
            If allCurves.Count <> 1 Then
                allCurves.Add(FilletId(eHorseShoeFilletCrv.L1L2))
                allCurves.Add(FilletId(eHorseShoeFilletCrv.L1L3))
                allCurves.Add(FilletId(eHorseShoeFilletCrv.L2L4))
            End If
            For Each curveId As Guid In allCurves
                RhUtil.RhinoApp().RunScript("_PointsOff", 0)
                Dim rhinoObjRef As New MRhinoObjRef(curveId)
                If rhinoObjRef.Object IsNot Nothing Then rhinoObjRef.Object.Select(True, True)
                rhinoObjRef.Dispose()
            Next
        End If
    End Sub

    Public Function GetDistanceL1L2() As Double
        Return GetCenterOfRotation(eHorseShoeStraightCrv.L1).DistanceTo(GetCenterOfRotation(eHorseShoeStraightCrv.L2))
    End Function

    Public Function SetDistanceL1L2(ByVal newDistance As Double) As Boolean
        Dim curveRefL1 As MRhinoObjRef = GetCurveRef(eHorseShoeCrv.L1)
        Dim curveRefL2 As MRhinoObjRef = GetCurveRef(eHorseShoeCrv.L2)
        Dim curveRefL3 As MRhinoObjRef = GetCurveRef(eHorseShoeCrv.L3)
        Dim curveRefL4 As MRhinoObjRef = GetCurveRef(eHorseShoeCrv.L4)
        Dim curveRefL1L3 As MRhinoObjRef = GetCurveRef(eHorseShoeCrv.L1L3)
        Dim curveRefL2L4 As MRhinoObjRef = GetCurveRef(eHorseShoeCrv.L2L4)
        If curveRefL1 Is Nothing Or curveRefL2 Is Nothing Or curveRefL3 Is Nothing Or curveRefL4 Is Nothing Or curveRefL1L3 Is Nothing Or curveRefL2L4 Is Nothing Then Return False
        Dim curveL1 As OnCurve = curveRefL1.Curve.DuplicateCurve()
        Dim curveL2 As OnCurve = curveRefL2.Curve.DuplicateCurve()
        Dim curveL3 As OnCurve = curveRefL3.Curve.DuplicateCurve()
        Dim curveL4 As OnCurve = curveRefL4.Curve.DuplicateCurve()
        Dim filletL1L3 As OnCurve = curveRefL1L3.Curve.DuplicateCurve()
        Dim filletL2L4 As OnCurve = curveRefL2L4.Curve.DuplicateCurve()
        Dim deltaDistance As Double = (newDistance - GetDistanceL1L2()) / 2
        Dim xform As New OnXform
        xform.Translation(0, deltaDistance, 0)
        curveL2.Transform(xform)
        curveL4.Transform(xform)
        filletL2L4.Transform(xform)
        xform.Identity()
        xform.Translation(0, -deltaDistance, 0)
        curveL1.Transform(xform)
        curveL3.Transform(xform)
        filletL1L3.Transform(xform)
        FilletRadius(eHorseShoeFilletCrv.L1L2) = newDistance / 2
        'Aggiorno
        AddCurveToDoc(eHorseShoeCrv.L1, curveL1, curveRefL1.m_uuid)
        AddCurveToDoc(eHorseShoeCrv.L2, curveL2, curveRefL2.m_uuid)
        AddCurveToDoc(eHorseShoeCrv.L3, curveL3, curveRefL3.m_uuid)
        AddCurveToDoc(eHorseShoeCrv.L4, curveL4, curveRefL4.m_uuid)
        AddCurveToDoc(eHorseShoeCrv.L1L3, filletL1L3, curveRefL1L3.m_uuid)
        AddCurveToDoc(eHorseShoeCrv.L2L4, filletL2L4, curveRefL2L4.m_uuid)
        'Dispose
        curveL1.Dispose()
        curveL2.Dispose()
        curveL3.Dispose()
        curveL4.Dispose()
        filletL1L3.Dispose()
        filletL2L4.Dispose()
        curveRefL1.Dispose()
        curveRefL2.Dispose()
        curveRefL3.Dispose()
        curveRefL4.Dispose()
        curveRefL1L3.Dispose()
        curveRefL2L4.Dispose()
        Return True
    End Function


#End Region


#Region " IClonable "

    Public Overrides Function Clone() As Object
        Dim plainObject As IdAddiction = IdAddictionFactory.Create(Me.Side, Me.Type, Me.Model, Me.Size)
        MyBase.CloneCommonField(plainObject)
        'Campi specifici IdCutoutToTalAddiction
        Dim result As IdHorseShoeTotalAddiction = DirectCast(plainObject, IdHorseShoeTotalAddiction)
        CloneCommonHorseField(result)
        Return result
    End Function

    Public Overridable Sub CloneCommonHorseField(ByRef ohterAddiction As IdHorseShoeTotalAddiction)
        ohterAddiction.mFilletRadiusL1L2 = Me.mFilletRadiusL1L2
        ohterAddiction.mFilletRadiusL1L3 = Me.mFilletRadiusL1L3
        ohterAddiction.mFilletRadiusL2L4 = Me.mFilletRadiusL2L4
        ohterAddiction.UniqueCurveId = New Guid(Me.UniqueCurveId.ToString)
        ohterAddiction.mFilletIdL1L3 = New Guid(Me.mFilletIdL1L3.ToString)
        ohterAddiction.mFilletIdL2L4 = New Guid(Me.mFilletIdL2L4.ToString)
    End Sub

#End Region



End Class
