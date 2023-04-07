Imports System.Threading
Imports System.Globalization
Imports InsoleDesigner.bll
Imports RMA.OpenNURBS
Imports RhinoUtils.RhViewport
Imports RhinoUtils.RhLayer
Imports InsoleDesigner.bll.IdAlias
Imports InsoleDesigner.bll.IdElement3dManager
Imports RhinoUtils
Imports RMA.Rhino


Public Class LoadInputCommons


    Public Shared Sub DeleteOldLastRef(ByVal side As eSide)
        Dim helper As IdElement3dManager = IdElement3dManager.GetInstance()
        helper.ClearRhinoObjRef(eReferences.lastBottomSurface, side)
        helper.ClearRhinoObjRef(eReferences.lastLateralSurface, side)
        helper.ClearRhinoObjRef(eReferences.lastTotalSurface, side)
        helper.ClearRhinoObjRef(eReferences.userExternalUpperCurve, side)
        helper.ClearRhinoObjRef(eReferences.userInternalUpperCurve, side)
        helper.ClearRhinoObjRef(eReferences.finalUpperCurve, side)
        helper.ClearRhinoObjRef(eReferences.insoleBottomBlendSurface, side)
        helper.ClearRhinoObjRef(eReferences.insoleFinalSurface, side)
        helper.ClearRhinoObjRef(eReferences.insoleLateralSurface, side)
        helper.ClearRhinoObjRef(eReferences.insoleTopSurface, side)
        RhLayer.DeleteLayerObjects(RhLayer.IndiceLayer(GetLayerName(side, eLayerType.last)), Nothing, False, True)
    End Sub


    Public Shared Sub DeleteOldFootRef(ByVal side As eSide)
        Dim helper As IdElement3dManager = IdElement3dManager.GetInstance()
        helper.ClearRhinoObjRef(eReferences.footMesh, side)
        'helper.ClearRhinoObjRef(eReferences.insoleBottomBlendSurface, side)
        'helper.ClearRhinoObjRef(eReferences.insoleFinalSurface, side)
        'helper.ClearRhinoObjRef(eReferences.insoleLateralSurface, side)
        'helper.ClearRhinoObjRef(eReferences.insoleTopSurface, side)
        RhLayer.DeleteLayerObjects(RhLayer.IndiceLayer(GetLayerName(side, eLayerType.foot)), Nothing, False, True)
    End Sub


    Public Shared Sub DeleteConstructionLayer(ByVal side As eSide, ByVal alsoTCOcurves As Boolean, ByVal alsoInsole As Boolean)
        Dim helper As IdElement3dManager = IdElement3dManager.GetInstance()
        'NEL LAYER ROOT SI TROVANO LE CURVE TCO CHE CANCELLO SOLO SE LE STO REIMPORTANDO
        If alsoTCOcurves Then
            RhLayer.DeleteLayerObjects(RhLayer.IndiceLayer(GetLayerName(side, eLayerType.root)), Nothing, False, True)
            'ELIMINO ANCHE I RIFERIMENTI
            helper.DeleteFootCurves(side)
        Else
            'SALVO LE CURVE
            Dim backupCurves As New SortedList(Of Double, IOnCurve)
            For Each pair As KeyValuePair(Of Double, Guid) In helper.GetFootCurves(side)
                Dim curveRef As New MRhinoObjRef(pair.Value)
                If curveRef IsNot Nothing AndAlso curveRef.Curve() IsNot Nothing Then backupCurves.Add(pair.Key, curveRef.Curve())
            Next
            'CANCELLO            
            RhLayer.DeleteLayerObjects(RhLayer.IndiceLayer(GetLayerName(side, eLayerType.root)), Nothing, False, True)
            helper.DeleteFootCurves(side)
            'RIAGGIUNGO LE CURVE
            RhLayer.RendiCorrenteLayer(GetLayerName(side, eLayerType.root))
            RhLayer.BloccaLayer(GetLayerName(side, eLayerType.root), False)
            For Each pair As KeyValuePair(Of Double, IOnCurve) In backupCurves
                Dim id As Guid = RhUtil.RhinoApp.ActiveDoc.AddCurveObject(pair.Value).Attributes.m_uuid
                helper.AddFootCurve(side, pair.Key, id)
            Next
        End If
        'CANCELLO GLI ALTRI LAYER
        If alsoInsole Then
            RhLayer.DeleteLayerHisSublayersAndObjects(GetLayerName(side, eLayerType.addiction), True)
            RhLayer.DeleteLayerHisSublayersAndObjects(GetLayerName(side, eLayerType.insole), True)
        End If
    End Sub


    Public Shared Sub UpdateAfterLoading()
        'Verifico e impedisco eventuali sovrapposizioni dei lati
        CheckOverlap()
        'Aggiorno la vista
        MaximizePerspectiveView()
        RhUtil.RhinoApp.RunScript("_Zoom E _Enter", 0)
        RhLayer.RendiCorrenteLayer(GetLayerName(eSide.left, eLayerType.root))
        Dim myLayers As New List(Of String)
        myLayers.Add(GetLayerName(eSide.left, eLayerType.root))
        myLayers.Add(GetLayerName(eSide.right, eLayerType.root))
        myLayers.Add(GetLayerName(eSide.left, eLayerType.foot))
        myLayers.Add(GetLayerName(eSide.right, eLayerType.foot))
        myLayers.Add(GetLayerName(eSide.left, eLayerType.last))
        myLayers.Add(GetLayerName(eSide.right, eLayerType.last))
        RhLayer.RendiVisibiliTuttiLayer(False)
        For Each layerName As String In myLayers
            If LayerExists(layerName) Then RendiVisibileLayer(layerName, True)
        Next
        MRhinoView.EnableDrawing(True)
        RhUtil.RhinoApp.ActiveDoc.Redraw()
    End Sub


    ''' <summary>
    ''' In caso di sovrapposizione traslo la parte sinistra con Y positiva
    ''' </summary>
    Public Shared Sub CheckOverlap()
        Dim bbox As New OnBoundingBox
        Dim helper As IdElement3dManager = IdElement3dManager.GetInstance()
        Dim needTranslation As Boolean = False
        Dim listOfLeftItem As New List(Of OnBoundingBox)
        Dim listOfRightItem As New List(Of OnBoundingBox)
        If helper.ObjectExist(eReferences.footMesh, eSide.left) Then
            helper.GetRhinoObj(eReferences.footMesh, eSide.left).GetTightBoundingBox(bbox)
            listOfLeftItem.Add(New OnBoundingBox(bbox))
        End If
        If helper.ObjectExist(eReferences.lastLateralSurface, eSide.left) Then
            helper.GetRhinoObj(eReferences.lastLateralSurface, eSide.left).GetTightBoundingBox(bbox)
            listOfLeftItem.Add(New OnBoundingBox(bbox))
        End If
        If helper.ObjectExist(eReferences.footMesh, eSide.right) Then
            helper.GetRhinoObj(eReferences.footMesh, eSide.right).GetTightBoundingBox(bbox)
            listOfRightItem.Add(New OnBoundingBox(bbox))
        End If
        If helper.ObjectExist(eReferences.lastLateralSurface, eSide.right) Then
            helper.GetRhinoObj(eReferences.lastLateralSurface, eSide.right).GetTightBoundingBox(bbox)
            listOfRightItem.Add(New OnBoundingBox(bbox))
        End If
        'Controllo intersezione tra bbox destre e sinistre
        For Each leftItem As OnBoundingBox In listOfLeftItem
            For Each rightItem As OnBoundingBox In listOfRightItem
                If leftItem.Intersection(rightItem) Then
                    needTranslation = True
                    Exit For
                End If
            Next
        Next
        bbox.Dispose()
        If needTranslation Then
            MsgBox(LanguageManager.Message(243), MsgBoxStyle.Information)
            RhLayer.SetAllLayerLock(False)
            RhUtil.RhinoApp.RunScript("_SelNone", 0)
            Dim footObj As MRhinoObject = Nothing
            Dim originalPoint As On3dPoint = Nothing
            If helper.ObjectExist(eReferences.footMesh, eSide.left) Then
                footObj = helper.GetRhinoObj(IdElement3dManager.eReferences.footMesh, IdElement3dManager.eSide.left)
                footObj.Select(True, True)
                originalPoint = footObj.BoundingBox.Center()
            End If
            Dim lastTotalObj As MRhinoObject = Nothing
            If helper.ObjectExist(eReferences.lastTotalSurface, eSide.left) Then
                lastTotalObj = helper.GetRhinoObj(IdElement3dManager.eReferences.lastTotalSurface, IdElement3dManager.eSide.left)
                If Not lastTotalObj Is Nothing Then lastTotalObj.Select(True, True)
                If originalPoint = Nothing Then originalPoint = lastTotalObj.BoundingBox.Center()
            End If
            Dim lastlateralObj As MRhinoObject = Nothing
            If helper.ObjectExist(eReferences.lastLateralSurface, eSide.left) Then
                lastlateralObj = helper.GetRhinoObj(IdElement3dManager.eReferences.lastLateralSurface, IdElement3dManager.eSide.left)
                If Not lastlateralObj Is Nothing Then lastlateralObj.Select(True, True)
                If originalPoint = Nothing Then originalPoint = lastlateralObj.BoundingBox.Center()
            End If
            Dim lastBottomObj As MRhinoObject = Nothing
            If helper.ObjectExist(eReferences.lastBottomSurface, eSide.left) Then
                lastBottomObj = helper.GetRhinoObj(IdElement3dManager.eReferences.lastBottomSurface, IdElement3dManager.eSide.left)
                If Not lastBottomObj Is Nothing Then lastBottomObj.Select(True, True)
                If originalPoint = Nothing Then originalPoint = lastBottomObj.BoundingBox.Center()
            End If
            If helper.ObjectExist(eReferences.lastTopSurface, eSide.left) Then helper.GetRhinoObj(IdElement3dManager.eReferences.lastTopSurface, IdElement3dManager.eSide.left).Select(True, True)
            Dim targetPoint As New On3dPoint(originalPoint.x, originalPoint.y + My.Settings.DistanceLeftRight, originalPoint.z)
            Dim origPointString As String = originalPoint.x.ToString().Replace(",", ".") & "," & originalPoint.y.ToString().Replace(",", ".") & "," & originalPoint.z.ToString().Replace(",", ".")
            Dim targetPointString As String = targetPoint.x.ToString().Replace(",", ".") & "," & targetPoint.y.ToString().Replace(",", ".") & "," & targetPoint.z.ToString().Replace(",", ".")
            RhUtil.RhinoApp.RunScript("-_Move _V=No " & origPointString & " " & targetPointString, 0)
            RhUtil.RhinoApp.RunScript("_SelNone", 0)
            MRhinoView.EnableDrawing(True)
            RhUtil.RhinoApp.ActiveDoc.Redraw()
            'Dispose
            If Not footObj Is Nothing Then footObj.Dispose()
            If Not lastBottomObj Is Nothing Then lastBottomObj.Dispose()
            If Not lastlateralObj Is Nothing Then lastlateralObj.Dispose()
            If Not lastTotalObj Is Nothing Then lastTotalObj.Dispose()
            originalPoint.Dispose()
            targetPoint.Dispose()
        End If
        listOfLeftItem.Clear()
        listOfLeftItem = Nothing
        listOfRightItem.Clear()
        listOfRightItem = Nothing
    End Sub



    Public Shared Sub UnLockObject(ByRef rhinoObj As MRhinoObject)
        If rhinoObj.IsLocked() Then
            RhUtil.RhinoApp.RunScript("_SelNone", 0)
            rhinoObj.Select(True, True)
            RhUtil.RhinoApp.RunScript("_UnlockSelected _Enter", 0)
        End If
    End Sub


    Public Shared Sub LockObject(ByRef rhinoObj As MRhinoObject)
        If Not rhinoObj.IsLocked() Then
            RhUtil.RhinoApp.RunScript("_SelNone", 0)
            rhinoObj.Select(True, True)
            RhUtil.RhinoApp.RunScript("_Lock _Enter", 0)
        End If
    End Sub


End Class
