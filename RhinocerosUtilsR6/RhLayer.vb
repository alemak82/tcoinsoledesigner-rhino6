Imports RMA.Rhino
Imports RMA.OpenNURBS


'****************************************
'*** Classe per la gestione dei layer ***
'****************************************

Public Class RhLayer



#Region " Creazione "


    ''' <summary>
    ''' Crea un nuovo layer
    ''' </summary>
    ''' <param name="layerName"></param>
    ''' <param name="color"></param>
    ''' <remarks></remarks>
    Public Shared Function CreateLayer(ByVal layerName As String, ByVal visible As Boolean, ByVal locked As Boolean, Optional ByVal color As IOnColor = Nothing) As IRhinoLayer
        Dim layerTable As MRhinoLayerTable = RhUtil.RhinoApp().ActiveDoc().m_layer_table
        Dim newLayer As New OnLayer()
        newLayer.SetLayerName(layerName)
        If Not color Is Nothing Then newLayer.SetColor(color)
        newLayer.SetVisible(visible)
        newLayer.SetLocked(locked)
        Dim layerIndex As Integer = layerTable.FindLayer(layerName)
        If (layerIndex < 0) Then
            layerIndex = layerTable.AddLayer(newLayer)
        Else
            layerTable.ModifyLayer(newLayer, layerIndex)
        End If
        newLayer.Dispose()
        If layerIndex < 0 Then Return Nothing
        Return layerTable.Item(layerIndex)
    End Function


    ''' <summary>
    ''' Aggiunge un layer con il colore passato per parametro. 
    ''' Passando come parametro un Guid valido, il layer creato sarà un SubLayer del Layer con il suddetto Guid
    ''' </summary>
    Public Shared Function CreateColoredSubLayer(ByVal layerName As String, ByVal fatherLayerGuid As Guid, Optional ByVal color As OnColor = Nothing) As Int32

        If RhUtil.RhinoApp.ActiveDoc() Is Nothing Then Return -1

        Dim tabellaLayer As MRhinoLayerTable = RhUtil.RhinoApp.ActiveDoc.m_layer_table
        Dim newLayer As OnLayer = New OnLayer
        newLayer.SetLayerName(layerName)
        If color Is Nothing Then color = New OnColor(0, 0, 255, 0)
        newLayer.SetColor(color)
        newLayer.SetVisible(True)
        newLayer.SetLocked(False)
        If Not fatherLayerGuid = Guid.Empty Then newLayer.m_parent_layer_id = fatherLayerGuid
        Dim layerIndex As Int32 = tabellaLayer.FindLayer(layerName)
        If layerIndex < 0 Then
            layerIndex = tabellaLayer.AddLayer(newLayer)
        Else
            tabellaLayer.ModifyLayer(newLayer, layerIndex)
        End If

        newLayer.Dispose()
        Return layerIndex

    End Function


    ''' <summary>
    ''' Aggiunge un layer con il colore passato per parametro. 
    ''' Passando come parametro un nome layer valido, il layer creato sarà un SubLayer del Layer con il suddetto Guid
    ''' </summary>
    Public Shared Function CreateColoredSubLayer(ByVal layerName As String, ByVal fatherLayerName As String, Optional ByVal color As OnColor = Nothing) As Int32

        If RhUtil.RhinoApp.ActiveDoc() Is Nothing Then Return -1

        Dim tabellaLayer As MRhinoLayerTable = RhUtil.RhinoApp.ActiveDoc.m_layer_table
        Dim newLayer As OnLayer = New OnLayer
        newLayer.SetLayerName(layerName)
        If color Is Nothing Then color = New OnColor(0, 0, 255, 0)
        newLayer.SetColor(color)
        newLayer.SetVisible(True)
        newLayer.SetLocked(False)
        Dim fatherLayerGuid As Guid = GetLayerGuid(fatherLayerName)
        If Not fatherLayerGuid = Guid.Empty Then newLayer.m_parent_layer_id = fatherLayerGuid
        Dim layerIndex As Int32 = tabellaLayer.FindLayer(layerName)
        If layerIndex < 0 Then
            layerIndex = tabellaLayer.AddLayer(newLayer)
        Else
            tabellaLayer.ModifyLayer(newLayer, layerIndex)
        End If

        newLayer.Dispose()
        Return layerIndex

    End Function


#End Region


#Region " Modifica "


    ''' <summary>
    ''' Modifica lo stato di un layer passando il suo nome
    ''' </summary>
    ''' <param name="nomeLayer"></param>
    ''' <param name="nuovoStato"></param>
    ''' <returns></returns>
    ''' <remarks>La funzione si comporta come in Rhinoceros v3</remarks>
    Public Shared Function ModificaStatoLayer(ByVal nomeLayer As String, ByVal nuovoStato As IOn.layer_mode) As Boolean
        Dim tabellaLayer As MRhinoLayerTable = RhUtil.RhinoApp.ActiveDoc.m_layer_table
        Dim iLayer As Integer = tabellaLayer.FindLayer(nomeLayer)
        If iLayer < 0 Then Return False
        If iLayer = tabellaLayer.CurrentLayerIndex Then
            Return (nuovoStato = IOn.layer_mode.normal_layer)
        End If
        Dim layerCorrente As IOnLayer = tabellaLayer.Item(iLayer)
        Dim nuovoLayer As New OnLayer(layerCorrente)
        Select Case nuovoStato
            Case IOn.layer_mode.hidden_layer
                nuovoLayer.SetVisible(False)
                nuovoLayer.SetLocked(False)

            Case IOn.layer_mode.locked_layer
                nuovoLayer.SetLocked(True)
                nuovoLayer.SetVisible(True)

            Case IOn.layer_mode.normal_layer
                nuovoLayer.SetLocked(False)
                nuovoLayer.SetVisible(True)

            Case IOn.layer_mode.layer_mode_count
        End Select
        Dim res As Boolean = tabellaLayer.ModifyLayer(nuovoLayer, iLayer)
        nuovoLayer.Dispose()
        Return res
    End Function


    ''' <summary>
    ''' Modifica lo stato di un layer passando il suo nome
    ''' </summary>
    ''' <param name="nomeLayer"></param>
    ''' <param name="visible"></param>
    ''' <param name="locked"></param>
    ''' <returns></returns>
    Public Shared Function ModificaStatoLayer(ByVal nomeLayer As String, ByVal visible As Boolean, ByVal locked As Boolean) As Boolean
        Dim tabellaLayer As MRhinoLayerTable = RhUtil.RhinoApp.ActiveDoc.m_layer_table
        Dim iLayer As Integer = tabellaLayer.FindLayer(nomeLayer)
        If iLayer < 0 Then Return False
        If iLayer = tabellaLayer.CurrentLayerIndex Then
            If visible And Not locked Then Return True
            Return False
        End If
        Dim layerCorrente As IOnLayer = tabellaLayer.Item(iLayer)
        If (layerCorrente.IsLocked = locked) And (layerCorrente.IsVisible = visible) Then Return True
        Dim nuovoLayer As New OnLayer(layerCorrente)
        nuovoLayer.SetLocked(locked)
        nuovoLayer.SetVisible(visible)
        Dim res As Boolean = tabellaLayer.ModifyLayer(nuovoLayer, iLayer)
        nuovoLayer.Dispose()
        Return res
    End Function


    ''' <summary>
    ''' Modifica lo stato di un layer passando il suo guid
    ''' </summary>
    ''' <param name="doc"></param>
    ''' <param name="visible"></param>
    ''' <param name="locked"></param>
    ''' <param name="guid"></param>
    Public Shared Sub ModificaStatoLayer(ByVal doc As MRhinoDoc, ByVal visible As Boolean, ByVal locked As Boolean, ByVal guid As Guid)
        If RhUtil.RhinoApp.ActiveDoc() Is Nothing Then Exit Sub

        Dim tabellaLayer As MRhinoLayerTable = doc.m_layer_table
        Dim layerToModify As IRhinoLayer = Nothing
        For i As Integer = 0 To tabellaLayer.LayerCount - 1
            Dim layer As IRhinoLayer = tabellaLayer.Item(i)
            If layer.m_layer_id = guid Then
                layerToModify = layer
                Exit For
            End If
        Next

        Dim newLayer As New OnLayer(layerToModify)
        newLayer.m_bVisible = visible
        newLayer.m_bLocked = locked
        tabellaLayer.ModifyLayer(newLayer, layerToModify.m_layer_index)
    End Sub


    ''' <summary>
    ''' Modifica lo stato di un layer passando il suo nome
    ''' </summary>
    ''' <param name="nomeLayer"></param>
    ''' <param name="visible"></param>
    ''' <returns></returns>
    Public Shared Function RendiVisibileLayer(ByVal nomeLayer As String, ByVal visible As Boolean) As Boolean        
        Dim tabellaLayer As MRhinoLayerTable = RhUtil.RhinoApp.ActiveDoc.m_layer_table
        Dim iLayer As Integer = tabellaLayer.FindLayer(nomeLayer)
        If iLayer < 0 Then Return False
        If iLayer = tabellaLayer.CurrentLayerIndex Then Return visible
        Dim layerCorrente As IOnLayer = tabellaLayer.Item(iLayer)
        If layerCorrente.IsVisible = visible Then Return True
        Dim nuovoLayer As New OnLayer(layerCorrente)
        nuovoLayer.SetVisible(visible)
        Dim res As Boolean = tabellaLayer.ModifyLayer(nuovoLayer, iLayer)
        nuovoLayer.Dispose()
        Return res
    End Function


    Public Shared Sub RendiVisibiliLayerTranne(ByVal layerNames As List(Of String), ByVal visible As Boolean, Optional ByVal alsoWithLockedObject As Boolean = True)
        If RhUtil.RhinoApp.ActiveDoc() Is Nothing Then Return
        If alsoWithLockedObject Then RhUtil.RhinoApp().RunScript("_UnLock", 0)
        Dim layerTable As MRhinoLayerTable = RhUtil.RhinoApp.ActiveDoc.m_layer_table
        For i As Int32 = 0 To layerTable.LayerCount - 1
            If Not layerNames.Contains(layerTable.Item(i).m_name) Then                
                RendiVisibileLayer(layerTable.Item(i).m_name, visible)
            End If
        Next
    End Sub


    Public Shared Sub RendiVisibiliTuttiLayer(Optional ByVal visible As Boolean = True)
        Dim tabellaLayer As MRhinoLayerTable = RhUtil.RhinoApp.ActiveDoc.m_layer_table
        For i As Int32 = 0 To tabellaLayer.LayerCount() - 1
            RendiVisibileLayer(tabellaLayer.Item(i).m_name, visible)
        Next
    End Sub

    ''' <summary>
    ''' Rendi corrente un layer passando il suo nome
    ''' </summary>
    ''' <param name="nomeLayer"></param>
    ''' <returns></returns>
    Public Shared Function RendiCorrenteLayer(ByVal nomeLayer As String) As Boolean
        Dim tabellaLayer As MRhinoLayerTable = RhUtil.RhinoApp.ActiveDoc.m_layer_table
        Dim iLayer As Integer = tabellaLayer.FindLayer(nomeLayer)
        If iLayer < 0 Then Return False
        Return tabellaLayer.SetCurrentLayerIndex(iLayer, True)
    End Function


    ''' <summary>
    ''' Modifica lo stato di un layer passando il suo nome
    ''' </summary>
    ''' <param name="nomeLayer"></param>
    ''' <param name="locked"></param>
    ''' <returns></returns>
    Public Shared Function BloccaLayer(ByVal nomeLayer As String, ByVal locked As Boolean) As Boolean
        Dim tabellaLayer As MRhinoLayerTable = RhUtil.RhinoApp.ActiveDoc.m_layer_table
        Dim iLayer As Integer = tabellaLayer.FindLayer(nomeLayer)
        If iLayer < 0 Then Return False
        If iLayer = tabellaLayer.CurrentLayerIndex Then Return Not locked
        Dim layerCorrente As IOnLayer = tabellaLayer.Item(iLayer)
        If layerCorrente.IsLocked = locked Then Return True
        Dim nuovoLayer As New OnLayer(layerCorrente)
        nuovoLayer.SetLocked(locked)
        Dim res As Boolean = tabellaLayer.ModifyLayer(nuovoLayer, iLayer)
        nuovoLayer.Dispose()
        Return res
    End Function


    ''' <summary>
    ''' Setta il lock al livello di cui viene passato il guid e a tutti i suoi sublayer se erano stati loccati dal padre
    ''' </summary>
    ''' <param name="lock">Con lock=true i layer vengono bloccati, con lock=false i layer vengono sbloccati</param>
    ''' <param name="guid">Guid del layer su cui agire</param>
    ''' <remarks>ATTENZIONE: i sublayer hanno 2 livelli di lock, uno proprio e uno dovuto al padre</remarks>
    Public Shared Sub BloccaLayer(ByVal lock As Boolean, ByVal guid As Guid)
        If RhUtil.RhinoApp.ActiveDoc() Is Nothing Then Exit Sub

        If lock Then
            RhUtil.RhinoApp.RunScript("-_Layer _Lock """ & guid.ToString() & """ _Enter", 0)
        Else
            RhUtil.RhinoApp.RunScript("-_Layer _Unlock """ & guid.ToString() & """ _Enter", 0)
        End If
    End Sub


    ''' <summary>
    ''' Setta tutti i layer del Doc come lock o unlock
    ''' </summary>
    ''' <param name="lock">Con lock=true i layer vengono bloccati, con lock=false i layer vengono sbloccati</param>
    Public Shared Sub SetAllLayerLock(ByVal lock As Boolean)
        If RhUtil.RhinoApp.ActiveDoc() Is Nothing Then Exit Sub

        If lock Then
            RhUtil.RhinoApp.RunScript("-_Layer _Lock * _Enter", 0)
        Else
            RhUtil.RhinoApp.RunScript("-_Layer _Unlock * _Enter", 0)
        End If
    End Sub


#End Region


#Region " Cancellazione "

    Public Shared Sub DeleteAllLayers(Optional ByVal onlyEmptyLayer As Boolean = False, Optional ByVal alsoWithLockedObject As Boolean = True)
        If RhUtil.RhinoApp.ActiveDoc() Is Nothing Then Return

        If (Not onlyEmptyLayer) And alsoWithLockedObject Then RhUtil.RhinoApp().RunScript("_UnLock", 0)
        Dim layerTable As MRhinoLayerTable = RhUtil.RhinoApp.ActiveDoc.m_layer_table
        layerTable.SetCurrentLayerIndex(0)
        For i As Int32 = 0 To layerTable.LayerCount()
            If Not onlyEmptyLayer Then DeleteLayerObjects(layerTable.Item(i).m_layer_index, Nothing, False)
            layerTable.DeleteLayer(i, True)
        Next
    End Sub


    ''' <summary>
    ''' Metodo per eliminare tutti i layer, tranne quello specificato
    ''' </summary>
    Public Shared Sub DeleteAllLayersExcept(ByVal layerName As String, Optional ByVal onlyEmptyLayer As Boolean = False, Optional ByVal alsoWithLockedObject As Boolean = True)
        If RhUtil.RhinoApp.ActiveDoc() Is Nothing Then Return

        If (Not onlyEmptyLayer) And alsoWithLockedObject Then RhUtil.RhinoApp().RunScript("_UnLock", 0)
        Dim layerTable As MRhinoLayerTable = RhUtil.RhinoApp.ActiveDoc.m_layer_table
        Dim numberOfFindLayer As Int32 = layerTable.FindLayer(layerName)

        'Due casi:
        '1. il layer passato non esiste, esco
        '2. il layer passato esiste, scorro tutti i layer e li cancello se sono diversi dal layer passato
        If numberOfFindLayer < 0 Then
            Return
        Else
            For i As Int32 = 0 To layerTable.LayerCount()
                If i <> numberOfFindLayer Then
                    If Not onlyEmptyLayer Then DeleteLayerObjects(layerTable.Item(i).m_layer_index, Nothing, False)
                    layerTable.DeleteLayer(i, True)
                End If
            Next
        End If
    End Sub


    ''' <summary>
    ''' Elimina tutti i layer tranne quelli passati per parametro
    ''' </summary>
    ''' <param name="layerNames"></param>
    Public Shared Sub DeleteAllLayersExcept(ByVal layerNames As List(Of String), Optional ByVal onlyEmptyLayer As Boolean = False, Optional ByVal alsoWithLockedObject As Boolean = True)
        If RhUtil.RhinoApp.ActiveDoc() Is Nothing Then Return

        If (Not onlyEmptyLayer) And alsoWithLockedObject Then RhUtil.RhinoApp().RunScript("_UnLock", 0)
        Dim layerTable As MRhinoLayerTable = RhUtil.RhinoApp.ActiveDoc.m_layer_table
        For i As Int32 = 0 To layerTable.LayerCount - 1
            If Not layerNames.Contains(layerTable.Item(i).m_name) Then
                If Not onlyEmptyLayer Then DeleteLayerObjects(layerTable.Item(i).m_layer_index, Nothing, False)
                layerTable.DeleteLayer(layerTable.Item(i).m_layer_index, True)
            End If
        Next
    End Sub

    ''' <summary>
    ''' Elimina i layer vuoti
    ''' </summary>
    ''' <remarks>ATTENZIONE NON TESTATO A FONDO COME SI COMPORTA CON IL LAYER CORRENTE</remarks>
    Public Shared Sub DeleteEmptyLayers()
        If RhUtil.RhinoApp.ActiveDoc() Is Nothing Then Exit Sub
        Dim layerTable As MRhinoLayerTable = RhUtil.RhinoApp.ActiveDoc.m_layer_table
        For i As Int32 = 0 To layerTable.LayerCount - 1
            Dim layer As IRhinoLayer = layerTable(i)
            Dim obj_list() As MRhinoObject = Nothing
            If Not RhUtil.RhinoApp.ActiveDoc.LookupObject(layer, obj_list) > 0 Then
                layerTable.DeleteLayer(i, True)
            End If
        Next
    End Sub


    ''' <summary>
    ''' Elimina un piano dato il suo Guid, senza messaggi di avviso
    ''' </summary>
    ''' <param name="layerId"></param>
    ''' <remarks></remarks>
    Public Shared Sub DeleteLayerHisSublayersAndObjects(ByVal layerId As Guid, Optional ByVal alsoWithLockedObject As Boolean = True)
        If RhUtil.RhinoApp.ActiveDoc() Is Nothing Then Exit Sub
        If layerId = Guid.Empty Then Exit Sub

        If alsoWithLockedObject Then RhUtil.RhinoApp().RunScript("_UnLock", 0)
        Dim layerTable As MRhinoLayerTable = RhUtil.RhinoApp.ActiveDoc.m_layer_table
        Dim layerFatherIndex As Integer = -1

        For i As Integer = layerTable.LayerCount - 1 To 0 Step -1
            If layerTable.Item(i).m_layer_id = layerId Then
                layerFatherIndex = layerTable.Item(i).m_layer_index
            End If
            If layerTable.Item(i).m_parent_layer_id = layerId Then
                Dim sublayerIndex As Integer = layerTable.Item(i).m_layer_index
                DeleteLayerObjects(sublayerIndex)
                layerTable.DeleteLayer(sublayerIndex, True)
            End If
        Next
        If layerFatherIndex >= 0 Then
            DeleteLayerObjects(layerFatherIndex)
            layerTable.DeleteLayer(layerFatherIndex, True)
        End If
    End Sub


    Public Shared Sub DeleteLayerHisSublayersAndObjects(ByVal layerName As String, Optional ByVal alsoWithLockedObject As Boolean = True)
        If RhUtil.RhinoApp.ActiveDoc() Is Nothing Then Exit Sub

        If alsoWithLockedObject Then RhUtil.RhinoApp().RunScript("_UnLock", 0)
        Dim layerTable As MRhinoLayerTable = RhUtil.RhinoApp.ActiveDoc.m_layer_table
        Dim layerGuid As Guid = GetLayerGuid(layerName)
        If layerGuid = Guid.Empty Then Exit Sub
        Dim layerFatherIndex As Integer = -1

        For i As Integer = layerTable.LayerCount - 1 To 0 Step -1
            If layerTable.Item(i).m_layer_id = layerGuid Then
                layerFatherIndex = layerTable.Item(i).m_layer_index
            End If
            If layerTable.Item(i).m_parent_layer_id = layerGuid Then
                Dim sublayerIndex As Integer = layerTable.Item(i).m_layer_index
                Dim name As String = layerTable.Item(i).m_name
                DeleteLayerObjects(sublayerIndex)
                layerTable.DeleteLayer(sublayerIndex, True)
            End If
        Next
        If layerFatherIndex >= 0 Then
            DeleteLayerObjects(layerFatherIndex)
            layerTable.DeleteLayer(layerFatherIndex, True)
        End If
    End Sub


    ''' <summary>
    ''' Seleziona tutti gli oggetti di un layer e fa il Dispose()
    ''' </summary>
    ''' <param name="layerIndex"></param>
    ''' <param name="objects"></param>
    ''' <remarks></remarks>
    Public Shared Sub DeleteLayerObjects(ByVal layerIndex As Integer, Optional ByRef objects As MRhinoObjRefArray = Nothing, _
                                         Optional ByVal onlyVisibleObjects As Boolean = False, Optional ByVal alsoWithLockedObject As Boolean = True)
        If layerIndex > RhUtil.RhinoApp.ActiveDoc.m_layer_table.LayerCount Then Exit Sub

        If alsoWithLockedObject Then RhUtil.RhinoApp().RunScript("_UnLock", 0)
        'Se si vogliono eliminare anche gli oggetti non visibili la procedura è completamente diversa
        If onlyVisibleObjects Then
            RhUtil.RhinoApp.RunScript("_SelNone ")
            RhUtil.RhinoApp.RunScript("_SelLayerNumber " & layerIndex, 0)
            Dim getLayerObjects As New MRhinoGetObject
            'NOTA: il GetObjects non seleziona gli oggetti non visibili
            getLayerObjects.GetObjects(0, Integer.MaxValue)
            objects = New MRhinoObjRefArray
            For i As Integer = 0 To getLayerObjects.ObjectCount - 1
                objects.Append(getLayerObjects.Object(i))
            Next
            For Each item As MRhinoObjRef In objects
                RhUtil.RhinoApp.ActiveDoc.DeleteObject(item, True, True)
            Next
        Else
            Dim layer As IRhinoLayer = RhUtil.RhinoApp.ActiveDoc.m_layer_table(layerIndex)
            'seleziono tutti gli oggetti nel layer
            Dim obj_list() As MRhinoObject = Nothing
            Dim obj_count As Integer = RhUtil.RhinoApp.ActiveDoc.LookupObject(layer, obj_list)
            'Da oggetto passo a ObjRef ed elimino
            For Each obj As MRhinoObject In obj_list
                If obj IsNot Nothing Then
                    Dim mio_oggetto_Rhino_Ref As MRhinoObjRef = New MRhinoObjRef(obj.Attributes.m_uuid)
                    RhUtil.RhinoApp.ActiveDoc.ShowObject(mio_oggetto_Rhino_Ref)
                    RhUtil.RhinoApp.ActiveDoc.DeleteObject(mio_oggetto_Rhino_Ref, True, True)
                End If
            Next
        End If
    End Sub

#End Region


#Region " Utility "

    ''' <summary>
    ''' Testa l'esistenza di un layer
    ''' </summary>
    ''' <param name="nomeLayer"></param>
    ''' <returns></returns>
    Public Shared Function LayerExists(ByVal nomeLayer As String) As Boolean
        Dim tabellaLayer As MRhinoLayerTable = RhUtil.RhinoApp.ActiveDoc.m_layer_table
        Dim index As Integer = tabellaLayer.FindLayer(nomeLayer)
        If index >= 0 Then Return True
        Return False
    End Function

    ''' <summary>
    ''' Ritorna l'indice di un layer dal suo nome; crea il layer se non esiste
    ''' </summary>
    ''' <param name="nomeLayer"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function IndiceLayer(ByVal nomeLayer As String) As Integer
        Dim tabellaLayer As MRhinoLayerTable = RhUtil.RhinoApp.ActiveDoc.m_layer_table
        Dim index As Integer = tabellaLayer.FindLayer(nomeLayer)
        If index >= 0 Then Return index
        Dim nuovoLayer As New OnLayer
        nuovoLayer.m_name = nomeLayer
        index = tabellaLayer.AddLayer(nuovoLayer)
        nuovoLayer.Dispose()
        Return index
    End Function


    ''' <summary>
    ''' Dato il nome di un layer ritorna il guid corrispondente
    ''' </summary>
    ''' <param name="layerName">Nome del layer da cercare</param>
    ''' <returns>Guid del layer richiesto</returns>
    Public Shared Function GetLayerGuid(ByVal layerName As String) As Guid
        If RhUtil.RhinoApp.ActiveDoc() Is Nothing Then Exit Function
        If layerName = String.Empty Or layerName = "" Then Return Guid.Empty

        Dim res As Guid = Guid.Empty
        Dim layerTable As MRhinoLayerTable = RhUtil.RhinoApp.ActiveDoc.m_layer_table
        Dim layerID As Integer = -1
        layerID = layerTable.FindLayer(layerName)
        If layerID >= 0 Then
            Dim rhinoLayer As IRhinoLayer = layerTable.Item(layerID)
            res = rhinoLayer.m_layer_id
        End If

        Return res
    End Function


    ''' <summary>
    ''' Restituisce gli oggetti di un layer
    ''' </summary>
    ''' <param name="layerName"></param>
    ''' <returns></returns>
    Public Shared Function GetLayerObjects(ByVal layerName As String) As List(Of MRhinoObject)
        Dim result As New List(Of MRhinoObject)
        Dim doc As MRhinoDoc = RhUtil.RhinoApp.ActiveDoc()
        Dim layerIndex As Integer = doc.m_layer_table.FindLayer(layerName)
        If layerIndex >= 0 And layerIndex <= doc.m_layer_table.LayerCount Then
            Dim layer As IRhinoLayer = doc.m_layer_table(layerIndex)
            Dim objList() As MRhinoObject = Nothing
            If doc.LookupObject(layer, objList) > 0 Then
                For i As Int32 = 0 To objList.Length - 1
                    result.Add(objList(i))
                Next
            End If
        End If
        Return result
    End Function


    ''' <summary>
    ''' Restituisce gli oggetti di un layer
    ''' </summary>
    ''' <param name="layerIndex"></param>
    ''' <returns></returns>
    Public Shared Function GetLayerObjects(ByVal layerIndex As Integer) As List(Of MRhinoObject)
        Dim result As New List(Of MRhinoObject)
        Dim doc As MRhinoDoc = RhUtil.RhinoApp.ActiveDoc()
        If layerIndex >= 0 And layerIndex <= doc.m_layer_table.LayerCount Then
            Dim layer As IRhinoLayer = doc.m_layer_table(layerIndex)
            Dim objList() As MRhinoObject = Nothing
            If doc.LookupObject(layer, objList) > 0 Then
                For i As Int32 = 0 To objList.Length - 1
                    result.Add(objList(i))
                Next
            End If
        End If
        Return result
    End Function


    ''' <summary>
    ''' Dato l'id di un layer ritorna true/false se è stato trovato o meno e scrive il guid nella variabile passata per riferimento
    ''' </summary>
    ''' <param name="layerIndex"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function GetLayerGuid(ByVal layerIndex As Int32) As Guid
        If RhUtil.RhinoApp.ActiveDoc() Is Nothing Then Exit Function
        If layerIndex < 0 Then Return Guid.Empty

        Dim res As Guid = Guid.Empty
        Dim layerTable As MRhinoLayerTable = RhUtil.RhinoApp.ActiveDoc.m_layer_table
        Dim rhinoLayer As IRhinoLayer = layerTable.Item(layerIndex)
        If Not rhinoLayer Is Nothing Then res = rhinoLayer.m_layer_id

        Return res
    End Function


    ''' <summary>
    ''' Seleziona tutti gli oggetti di un layer, eliminando eventuali oggetti preselezionati
    ''' </summary>
    Public Shared Sub SelectLayerObjects(ByVal layerIndex As Integer, ByVal recursive As Boolean, _
                                              Optional ByRef objects As MRhinoObjRefArray = Nothing, _
                                              Optional ByVal onlyVisibleObjects As Boolean = False)
        If layerIndex < 0 Or layerIndex > RhUtil.RhinoApp.ActiveDoc.m_layer_table.LayerCount Then Exit Sub

        If Not recursive Then RhUtil.RhinoApp.ActiveDoc.UnselectAll()
        'Se si vogliono eliminare anche gli oggetti non visibili la procedura è completamente diversa
        objects = New MRhinoObjRefArray
        If onlyVisibleObjects Then
            RhUtil.RhinoApp.RunScript("_SelLayerNumber " & layerIndex, 0)

            Dim getLayerObjects As New MRhinoGetObject
            getLayerObjects.GetObjects(0, Integer.MaxValue)

            For i As Integer = 0 To getLayerObjects.ObjectCount - 1
                objects.Append(getLayerObjects.Object(i))
            Next
        Else
            Dim layer As IRhinoLayer = RhUtil.RhinoApp.ActiveDoc.m_layer_table(layerIndex)
            'seleziono tutti gli oggetti nel layer
            Dim obj_list() As MRhinoObject = Nothing
            Dim obj_count As Integer = RhUtil.RhinoApp.ActiveDoc.LookupObject(layer, obj_list)
            For Each obj As MRhinoObject In obj_list
                If obj IsNot Nothing Then obj.Select()
                objects.Append(New MRhinoObjRef(obj.Attributes.m_uuid))
            Next
        End If
    End Sub


    ''' <summary>
    ''' Seleziona tutti gli oggetti di un layer, eliminando eventuali oggetti preselezionati
    ''' </summary>
    Public Shared Sub SelectLayerObjects(ByVal layerName As String, ByVal recursive As Boolean, _
                                              Optional ByRef objects As MRhinoObjRefArray = Nothing, _
                                              Optional ByVal onlyVisibleObjects As Boolean = False)

        If RhUtil.RhinoApp.ActiveDoc() Is Nothing Then Exit Sub
        If layerName = String.Empty Or layerName = "" Then Exit Sub
        Dim layerTable As MRhinoLayerTable = RhUtil.RhinoApp.ActiveDoc.m_layer_table
        Dim layerIndex As Integer = -1
        layerIndex = layerTable.FindLayer(layerName)

        If layerIndex >= 0 And layerIndex <= layerTable.LayerCount Then
            If Not recursive Then RhUtil.RhinoApp.ActiveDoc.UnselectAll()
            'Se si vogliono eliminare anche gli oggetti non visibili la procedura è completamente diversa
            objects = New MRhinoObjRefArray
            If onlyVisibleObjects Then
                RhUtil.RhinoApp.RunScript("_SelLayerNumber " & layerIndex, 0)

                Dim getLayerObjects As New MRhinoGetObject
                getLayerObjects.GetObjects(0, Integer.MaxValue)

                For i As Integer = 0 To getLayerObjects.ObjectCount - 1
                    objects.Append(getLayerObjects.Object(i))
                Next
            Else
                Dim layer As IRhinoLayer = RhUtil.RhinoApp.ActiveDoc.m_layer_table(layerIndex)
                'seleziono tutti gli oggetti nel layer
                Dim obj_list() As MRhinoObject = Nothing
                Dim obj_count As Integer = RhUtil.RhinoApp.ActiveDoc.LookupObject(layer, obj_list)
                For Each obj As MRhinoObject In obj_list
                    If obj IsNot Nothing Then obj.Select()
                    objects.Append(New MRhinoObjRef(obj.Attributes.m_uuid))
                Next
            End If
        End If
    End Sub


#End Region






End Class


