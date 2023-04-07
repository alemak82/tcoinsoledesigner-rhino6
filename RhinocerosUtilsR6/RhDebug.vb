Imports System.Linq
Imports RMA.OpenNURBS
Imports RMA.Rhino


Public Class RhDebug


    Private Const DEBUG_LAYER_NAME As String = "DEBUG"



    #Region " AddDocumentToDebug "
    

    Public Shared Sub AddDocumentToDebug(ByVal line As IOnLine, ByVal objName As String)
        If line Is Nothing Then
            MsgBox("DEBUG: Impossibile aggiungere la curva " & objName & " perchè nulla")
            Exit Sub
        End If
        Dim bkLayerIndex As Integer = RhUtil.RhinoApp.ActiveDoc.m_layer_table.CurrentLayerIndex
        RhLayer.CreateLayer(DEBUG_LAYER_NAME, True, False)
        RhLayer.RendiCorrenteLayer(DEBUG_LAYER_NAME)
        Dim rhinoObj As MRhinoObject = RhUtil.RhinoApp.ActiveDoc.AddCurveObject(line)
        Dim rhinoObjAttr As New MRhinoObjectAttributes(rhinoObj.Attributes)
        rhinoObjAttr.m_name = objName
        RhUtil.RhinoApp.ActiveDoc.ModifyObjectAttributes(New MRhinoObjRef(rhinoObj.Attributes.m_uuid), rhinoObjAttr)
        RhUtil.RhinoApp.ActiveDoc.m_layer_table.SetCurrentLayerIndex(bkLayerIndex, True)
        'RhLayer.RendiVisibileLayer(DEBUG_LAYER_NAME, False)
    End Sub

    Public Shared Sub AddDocumentToDebug(ByVal curve As IOnCurve, ByVal objName As String)
        If curve Is Nothing Then
            MsgBox("DEBUG: Impossibile aggiungere la curva " & objName & " perchè nulla")
            Exit Sub
        End If
        Dim bkLayerIndex As Integer = RhUtil.RhinoApp.ActiveDoc.m_layer_table.CurrentLayerIndex
        RhLayer.CreateLayer(DEBUG_LAYER_NAME, True, False)
        RhLayer.RendiCorrenteLayer(DEBUG_LAYER_NAME)
        Dim rhinoObj As MRhinoObject = RhUtil.RhinoApp.ActiveDoc.AddCurveObject(curve)
        Dim rhinoObjAttr As New MRhinoObjectAttributes(rhinoObj.Attributes)
        rhinoObjAttr.m_name = objName
        RhUtil.RhinoApp.ActiveDoc.ModifyObjectAttributes(New MRhinoObjRef(rhinoObj.Attributes.m_uuid), rhinoObjAttr)
        RhUtil.RhinoApp.ActiveDoc.m_layer_table.SetCurrentLayerIndex(bkLayerIndex, True)
        'RhLayer.RendiVisibileLayer(DEBUG_LAYER_NAME, False)
    End Sub

    Public Shared Sub AddDocumentToDebug(ByVal curves As List(Of OnPolyCurve), ByVal baseName As String)
        If curves Is Nothing Then
            MsgBox("DEBUG: Impossibile aggiungere la lista di curve " & baseName & " perchè nulla")
            Exit Sub
        End If
        Dim bkLayerIndex As Integer = RhUtil.RhinoApp.ActiveDoc.m_layer_table.CurrentLayerIndex
        RhLayer.CreateLayer(DEBUG_LAYER_NAME, True, False)
        RhLayer.RendiCorrenteLayer(DEBUG_LAYER_NAME)
        For i As Int32 = 0 To curves.Count - 1
            Dim curveRhinoObj As MRhinoCurveObject = RhUtil.RhinoApp.ActiveDoc.AddCurveObject(curves.Item(i))
            Dim curveObjRef As New MRhinoObjRef(curveRhinoObj.Attributes.m_uuid)
            Dim postOffsetCurveAttr As New MRhinoObjectAttributes(curveObjRef.Object.Attributes)
            If curves.Count > 1 Then
                postOffsetCurveAttr.m_name = baseName & "_" & i
            Else
                postOffsetCurveAttr.m_name = baseName
            End If
            RhUtil.RhinoApp.ActiveDoc.ModifyObjectAttributes(curveObjRef, postOffsetCurveAttr)
        Next
        RhUtil.RhinoApp.ActiveDoc.m_layer_table.SetCurrentLayerIndex(bkLayerIndex, True)
    End Sub

    Public Shared Sub AddDocumentToDebug(ByVal curves As List(Of OnCurve), ByVal baseName As String)
        If curves Is Nothing Then
            MsgBox("DEBUG: Impossibile aggiungere la lista di curve " & baseName & " perchè nulla")
            Exit Sub
        End If
        Dim bkLayerIndex As Integer = RhUtil.RhinoApp.ActiveDoc.m_layer_table.CurrentLayerIndex
        RhLayer.CreateLayer(DEBUG_LAYER_NAME, True, False)
        RhLayer.RendiCorrenteLayer(DEBUG_LAYER_NAME)
        For i As Int32 = 0 To curves.Count - 1
            Dim curveRhinoObj As MRhinoCurveObject = RhUtil.RhinoApp.ActiveDoc.AddCurveObject(curves.Item(i))
            Dim curveObjRef As New MRhinoObjRef(curveRhinoObj.Attributes.m_uuid)
            Dim postOffsetCurveAttr As New MRhinoObjectAttributes(curveObjRef.Object.Attributes)
            If curves.Count > 1 Then
                postOffsetCurveAttr.m_name = baseName & "_" & i
            Else
                postOffsetCurveAttr.m_name = baseName
            End If
            RhUtil.RhinoApp.ActiveDoc.ModifyObjectAttributes(curveObjRef, postOffsetCurveAttr)
        Next
        RhUtil.RhinoApp.ActiveDoc.m_layer_table.SetCurrentLayerIndex(bkLayerIndex, True)
        'RhLayer.RendiVisibileLayer(DEBUG_LAYER_NAME, False)
    End Sub

    Public Shared Sub AddDocumentToDebug(ByVal curves() As OnCurve, ByVal baseName As String)
        If curves Is Nothing Then
            MsgBox("DEBUG: Impossibile aggiungere la lista di curve " & baseName & " perchè nulla")
            Exit Sub
        End If
        Dim debugList As New List(Of OnCurve)
        For i As Integer = 0 To curves.Count - 1
            AddDocumentToDebug(curves(i), baseName & "_" & i)
        Next
        AddDocumentToDebug(debugList, baseName)
        'RhLayer.RendiVisibileLayer(DEBUG_LAYER_NAME, False)
    End Sub

    Public Shared Sub AddDocumentToDebug(ByVal curves As OnCurveArray, ByVal baseName As String)
        If curves Is Nothing Then
            MsgBox("DEBUG: Impossibile aggiungere la lista di curve " & baseName & " perchè nulla")
            Exit Sub
        End If
        Dim debugList As New List(Of OnCurve)
        For i As Integer = 0 To curves.Count - 1
            AddDocumentToDebug(curves.Item(i), baseName & "_" & i)
        Next
        AddDocumentToDebug(debugList, baseName)
        'RhLayer.RendiVisibileLayer(DEBUG_LAYER_NAME, False)
    End Sub

    Public Shared Sub AddDocumentToDebug(ByVal point As IOn3dPoint, ByVal objName As String)
        If point Is Nothing Then
            MsgBox("DEBUG: Impossibile aggiungere il punto " & objName & " perchè nullo")
            Exit Sub
        End If
        Dim bkLayerIndex As Integer = RhUtil.RhinoApp.ActiveDoc.m_layer_table.CurrentLayerIndex
        RhLayer.CreateLayer(DEBUG_LAYER_NAME, True, False)
        RhLayer.RendiCorrenteLayer(DEBUG_LAYER_NAME)
        Dim rhinoObj As MRhinoObject = RhUtil.RhinoApp.ActiveDoc.AddPointObject(point)
        Dim rhinoObjAttr As New MRhinoObjectAttributes(rhinoObj.Attributes)
        rhinoObjAttr.m_name = objName
        RhUtil.RhinoApp.ActiveDoc.ModifyObjectAttributes(New MRhinoObjRef(rhinoObj.Attributes.m_uuid), rhinoObjAttr)
        RhUtil.RhinoApp.ActiveDoc.m_layer_table.SetCurrentLayerIndex(bkLayerIndex, True)
        'RhLayer.RendiVisibileLayer(DEBUG_LAYER_NAME, False)
    End Sub

    Public Shared Sub AddDocumentToDebug(ByVal points As On3dPointArray, ByVal baseName As String)
        If points Is Nothing Then
            MsgBox("DEBUG: Impossibile aggiungere la lista di points " & baseName & " perchè nulla")
            Exit Sub
        End If
        For i As Integer = 0 To points.Count - 1
            AddDocumentToDebug(points.Item(i), baseName & "_" & i)
        Next
        'RhLayer.RendiVisibileLayer(DEBUG_LAYER_NAME, False)
    End Sub

    Public Shared Sub AddDocumentToDebug(ByVal points As SortedList(Of Double, On3dPoint), ByVal baseName As String)
        If points Is Nothing Then
            MsgBox("DEBUG: Impossibile aggiungere la lista di points " & baseName & " perchè nulla")
            Exit Sub
        End If
        For i As Integer = 0 To points.Count - 1
            AddDocumentToDebug(points.Values(i), baseName & "_" & i)
        Next
        'RhLayer.RendiVisibileLayer(DEBUG_LAYER_NAME, False)
    End Sub

    Public Shared Sub AddDocumentToDebug(ByVal surface As OnSurface, ByVal objName As String)
        If surface Is Nothing Then
            MsgBox("DEBUG: Impossibile aggiungere la superfice " & objName & " perchè nulla")
            Exit Sub
        End If
        Dim bkLayerIndex As Integer = RhUtil.RhinoApp.ActiveDoc.m_layer_table.CurrentLayerIndex
        RhLayer.CreateLayer(DEBUG_LAYER_NAME, True, False)
        RhLayer.RendiCorrenteLayer(DEBUG_LAYER_NAME)
        Dim rhinoObj As MRhinoObject = RhUtil.RhinoApp.ActiveDoc.AddSurfaceObject(surface)
        Dim rhinoObjAttr As New MRhinoObjectAttributes(rhinoObj.Attributes)
        rhinoObjAttr.m_name = objName
        RhUtil.RhinoApp.ActiveDoc.ModifyObjectAttributes(New MRhinoObjRef(rhinoObj.Attributes.m_uuid), rhinoObjAttr)
        RhUtil.RhinoApp.ActiveDoc.m_layer_table.SetCurrentLayerIndex(bkLayerIndex, True)
        'RhLayer.RendiVisibileLayer(DEBUG_LAYER_NAME, False)
    End Sub

    Public Shared Sub AddDocumentToDebug(ByVal surface As IOnSurface, ByVal objName As String)
        If surface Is Nothing Then
            MsgBox("DEBUG: Impossibile aggiungere la superfice " & objName & " perchè nulla")
            Exit Sub
        End If
        Dim bkLayerIndex As Integer = RhUtil.RhinoApp.ActiveDoc.m_layer_table.CurrentLayerIndex
        RhLayer.CreateLayer(DEBUG_LAYER_NAME, True, False)
        RhLayer.RendiCorrenteLayer(DEBUG_LAYER_NAME)
        Dim rhinoObj As MRhinoObject = RhUtil.RhinoApp.ActiveDoc.AddSurfaceObject(surface)
        Dim rhinoObjAttr As New MRhinoObjectAttributes(rhinoObj.Attributes)
        rhinoObjAttr.m_name = objName
        RhUtil.RhinoApp.ActiveDoc.ModifyObjectAttributes(New MRhinoObjRef(rhinoObj.Attributes.m_uuid), rhinoObjAttr)
        RhUtil.RhinoApp.ActiveDoc.m_layer_table.SetCurrentLayerIndex(bkLayerIndex, True)
        'RhLayer.RendiVisibileLayer(DEBUG_LAYER_NAME, False)
    End Sub

    Public Shared Sub AddDocumentToDebug(ByVal brep As IOnBrep, ByVal objName As String)
        If brep Is Nothing Then
            MsgBox("DEBUG: Impossibile aggiungere la superfice " & objName & " perchè nulla")
            Exit Sub
        End If
        Dim bkLayerIndex As Integer = RhUtil.RhinoApp.ActiveDoc.m_layer_table.CurrentLayerIndex
        RhLayer.CreateLayer(DEBUG_LAYER_NAME, True, False)
        RhLayer.RendiCorrenteLayer(DEBUG_LAYER_NAME)
        Dim rhinoObj As MRhinoObject = RhUtil.RhinoApp.ActiveDoc.AddBrepObject(brep)
        Dim rhinoObjAttr As New MRhinoObjectAttributes(rhinoObj.Attributes)
        rhinoObjAttr.m_name = objName
        RhUtil.RhinoApp.ActiveDoc.ModifyObjectAttributes(New MRhinoObjRef(rhinoObj.Attributes.m_uuid), rhinoObjAttr)
        RhUtil.RhinoApp.ActiveDoc.m_layer_table.SetCurrentLayerIndex(bkLayerIndex, True)
        'RhLayer.RendiVisibileLayer(DEBUG_LAYER_NAME, False)
    End Sub

    Public Shared Sub AddDocumentToDebug(ByVal brep As OnBrep, ByVal objName As String)
        If brep Is Nothing Then
            MsgBox("DEBUG: Impossibile aggiungere la superfice " & objName & " perchè nulla")
            Exit Sub
        End If
        Dim bkLayerIndex As Integer = RhUtil.RhinoApp.ActiveDoc.m_layer_table.CurrentLayerIndex
        RhLayer.CreateLayer(DEBUG_LAYER_NAME, True, False)
        RhLayer.RendiCorrenteLayer(DEBUG_LAYER_NAME)
        Dim rhinoObj As MRhinoObject = RhUtil.RhinoApp.ActiveDoc.AddBrepObject(brep)
        Dim rhinoObjAttr As New MRhinoObjectAttributes(rhinoObj.Attributes)
        rhinoObjAttr.m_name = objName
        RhUtil.RhinoApp.ActiveDoc.ModifyObjectAttributes(New MRhinoObjRef(rhinoObj.Attributes.m_uuid), rhinoObjAttr)
        RhUtil.RhinoApp.ActiveDoc.m_layer_table.SetCurrentLayerIndex(bkLayerIndex, True)
        'RhLayer.RendiVisibileLayer(DEBUG_LAYER_NAME, False)
    End Sub

    Public Shared Sub AddDocumentToDebug(ByVal brep() As OnBrep, ByVal baseName As String)
        If brep Is Nothing Then
            MsgBox("DEBUG: Impossibile aggiungere la superfice " & baseName & " perchè nulla")
            Exit Sub
        End If
        For i As Integer = 0 To brep.Count - 1
            AddDocumentToDebug(brep(i), baseName & "_" & i)
        Next
    End Sub

    Public Shared Sub AddDocumentToDebug(ByVal mesh As IOnMesh, ByVal objName As String)
        If mesh Is Nothing Then
            MsgBox("DEBUG: Impossibile aggiungere la mesh " & objName & " perchè nulla")
            Exit Sub
        End If
        Dim bkLayerIndex As Integer = RhUtil.RhinoApp.ActiveDoc.m_layer_table.CurrentLayerIndex
        RhLayer.CreateLayer(DEBUG_LAYER_NAME, True, False)
        RhLayer.RendiCorrenteLayer(DEBUG_LAYER_NAME)
        Dim rhinoObj As MRhinoObject = RhUtil.RhinoApp.ActiveDoc.AddMeshObject(mesh)
        Dim rhinoObjAttr As New MRhinoObjectAttributes(rhinoObj.Attributes)
        rhinoObjAttr.m_name = objName
        RhUtil.RhinoApp.ActiveDoc.ModifyObjectAttributes(New MRhinoObjRef(rhinoObj.Attributes.m_uuid), rhinoObjAttr)
        RhUtil.RhinoApp.ActiveDoc.m_layer_table.SetCurrentLayerIndex(bkLayerIndex, True)
    End Sub


#End Region

 

End Class
