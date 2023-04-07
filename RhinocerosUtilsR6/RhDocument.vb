Imports RMA.Rhino
Imports RMA.OpenNURBS


Public Class RhDocument


    Public Shared Function CheckDocIsEmpty() As Boolean
        Dim layers() As IRhinoLayer = {}
        RhUtil.RhinoApp.ActiveDoc.m_layer_table.GetSortedList(layers)
        For i As Int32 = 0 To layers.Length - 1
            Dim obj_list() As MRhinoObject = {}
            RhUtil.RhinoApp.ActiveDoc.LookupObject(layers(i), obj_list)
            If obj_list.Length > 0 Then Return False
        Next
        Return True
    End Function


    ''' <summary>
    ''' Conta il numero di oggetti di un certo tipo presenti dentro Rhino
    ''' </summary>
    ''' <param name="typeObj"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function ObjectsCount(ByVal typeObj As IOn.object_type) As Integer
        Dim count As Integer = 0
        Dim rhinoObjIter As New MRhinoObjectIterator(IRhinoObjectIterator.object_state.undeleted_objects, IRhinoObjectIterator.object_category.active_objects)
        Dim rhinoObj As MRhinoObject = rhinoObjIter.First
        Do
            If rhinoObj Is Nothing Then Exit Do
            Select Case typeObj
                Case IOn.object_type.any_object
                    count += 1
                Case Else
                    If rhinoObj.ObjectType = typeObj Then count += 1
            End Select
            rhinoObj = rhinoObjIter.Next
        Loop
        rhinoObjIter.Dispose()
        Return count
    End Function


    ''' <summary>
    ''' Elimina tutti gli oggetti che hanno il nome che inizia per "inizioNomeOggetti"; si può applicare un filtro opzionale per il tipo di oggetti
    ''' </summary>
    ''' <param name="inizioNomeOggetti"></param>
    ''' <param name="tipoOggetti"></param>
    ''' <remarks></remarks>
    Public Shared Sub EliminaOggettiPerNome(ByVal inizioNomeOggetti As String, Optional ByVal tipoOggetti As IOn.object_type = IOn.object_type.unknown_object_type, Optional ByVal exceptionNames() As String = Nothing)
        Dim rhinoObjIter As New MRhinoObjectIterator(IRhinoObjectIterator.object_state.undeleted_objects, IRhinoObjectIterator.object_category.active_objects)
        If (tipoOggetti <> IOn.object_type.unknown_object_type) Then rhinoObjIter.SetObjectFilter(UInt32.Parse(Strings.Format(tipoOggetti)))
        Dim obj As IRhinoObject = rhinoObjIter.First       'Primo oggetto
        Do Until obj Is Nothing
            'Se necessario, verifico se è un oggetto da cancellare
            Dim delete As Boolean = True
            If exceptionNames IsNot Nothing Then
                For i As Integer = 0 To exceptionNames.GetUpperBound(0)
                    If (obj.Attributes.m_name = exceptionNames(i)) Then
                        delete = False
                        Exit For
                    End If
                Next
            End If
            'Se è un nome da cancellare, allora lo elimino
            If (delete = True) And (obj.Attributes.m_name.StartsWith(inizioNomeOggetti)) Then
                RhUtil.RhinoApp.ActiveDoc.DeleteObject(New MRhinoObjRef(obj), True, True)
            End If
            'Vado all'oggetto successivo
            obj = rhinoObjIter.Next
        Loop
        rhinoObjIter.Dispose()
    End Sub


    ''' <summary>
    ''' Restituisce un Rhino Object passando il nome "strName"; nel caso di due oggetti con lo stesso nome restituisce Nothing
    ''' </summary>
    ''' <param name="strName"></param>
    ''' <param name="selectObject"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function ObjectByName(ByVal strName As String, Optional ByVal selectObject As Boolean = False) As MRhinoObject
        Dim rhinoObj As MRhinoObject
        Dim rhinoObjIter As New MRhinoObjectIterator(IRhinoObjectIterator.object_state.undeleted_objects, IRhinoObjectIterator.object_category.active_objects)
        Dim objClone(0) As MRhinoObject

        rhinoObjIter.IncludeGrips(True)
        rhinoObjIter.IncludeLights(False)
        rhinoObj = rhinoObjIter.First       'Primo oggetto
        Do
            If rhinoObj Is Nothing Then Exit Do
            If rhinoObj.Attributes.m_name.ToLower = strName.ToLower Then
                If objClone(0) Is Nothing Then
                    objClone(0) = rhinoObj
                Else
                    ReDim Preserve objClone(objClone.GetUpperBound(0) + 1)
                    objClone(objClone.GetUpperBound(0)) = rhinoObj
                End If
            End If
            If objClone.GetLength(0) = 2 Then Exit Do
            rhinoObj = rhinoObjIter.Next
        Loop
        'Analisi finale
        Dim res As MRhinoObject = Nothing
        If (objClone.GetLength(0) = 1) And (Not objClone(0) Is Nothing) Then
            If selectObject Then
                RhUtil.RhinoApp.ActiveDoc.UnselectAll()
                objClone(0).Select()
            End If
            res = objClone(0)
        End If
        rhinoObjIter.Dispose()
        Return res
    End Function


    ''' <summary>
    ''' Intanzia un riferimento ad un oggetto rispettivamente per uuid e per nomeOggetto
    ''' </summary>
    ''' <param name="uuid">uuid dell'oggetto cercato</param>
    ''' <param name="nomeOggetto">nome oggetto con cui viene cercato l'oggetto</param>
    ''' <returns>Istanza di un oggetto MRhinoObjRef che punta all'oggetto cercato</returns>
    ''' <remarks></remarks>
    Public Shared Function CreaRiferimentoOggetto(ByRef uuid As System.Guid, ByVal nomeOggetto As String) As MRhinoObjRef
        Dim result As New MRhinoObjRef(uuid)
        If result.Object Is Nothing Then
            result.Dispose()
            Dim obj As MRhinoObject = ObjectByName(nomeOggetto)
            If obj Is Nothing Then
                uuid = Guid.Empty
                Return Nothing
            End If
            result = New MRhinoObjRef(obj)
            uuid = obj.Attributes.m_uuid
        Else
            'Prima di ritornare verifico che se il nome è stato passato corrisponda
            If nomeOggetto <> "" Then
                Dim objectName As String = result.Object.Attributes.m_name
                If objectName <> "" And objectName <> nomeOggetto Then
                    result.Dispose()
                    result = Nothing
                    uuid = Guid.Empty
                End If
            End If
        End If
        Return result
    End Function


    ''' <summary>
    ''' Verifica l'esitenza e la correttezza di un Oggetto chiamato objName e che sia di un certo tipo
    ''' </summary>
    ''' <param name="objName"></param>
    ''' <param name="typeObj"></param>
    ''' <param name="alertMsgBox"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function ObjectExist(ByVal objName As String, ByVal typeObj As IOn.object_type, Optional ByVal alertMsgBox As Boolean = False) As Boolean
        If ObjectByName(objName, False) Is Nothing Then
            If alertMsgBox Then RhMisc.RhMsgbox("Object """ & objName & """ doesn't exist or duplicated.", "Object not valid")
            Return False
        ElseIf (ObjectByName(objName, False).ObjectType <> typeObj) Then
            If alertMsgBox Then RhMisc.RhMsgbox("Object """ & objName & """ not of type " & typeObj.ToString, "Object not valid")
            Return False
        Else
            Return True
        End If
    End Function

    ''' <summary>
    ''' Imposta il nome ad un oggetto di Rhino
    ''' </summary>
    ''' <param name="refOggetto"></param>
    ''' <param name="nome"></param>
    ''' <remarks></remarks>
    Public Shared Sub ImpostaNomeAdOggetto(ByVal refOggetto As MRhinoObjRef, ByVal nome As String)
        Dim attributi As New MRhinoObjectAttributes(refOggetto.Object.Attributes)
        attributi.m_name = nome
        RhUtil.RhinoApp.ActiveDoc.ModifyObjectAttributes(refOggetto, attributi)
        attributi.Dispose()
    End Sub


    ''' <summary>
    ''' Imposta il nome ad un oggetto di Rhino 
    ''' </summary>
    ''' <param name="oggetto"></param>
    ''' <param name="nome"></param>
    ''' <remarks></remarks>
    Public Shared Sub ImpostaNomeAdOggetto(ByVal oggetto As MRhinoObject, ByVal nome As String)
        Dim refOggetto As New MRhinoObjRef(oggetto)
        Dim attributi As New MRhinoObjectAttributes(refOggetto.Object.Attributes)
        attributi.m_name = nome
        RhUtil.RhinoApp.ActiveDoc.ModifyObjectAttributes(refOggetto, attributi)
        attributi.Dispose()
        refOggetto.Dispose()
    End Sub


    ''' <summary>
    ''' Imposta un colore specifico ad un oggetto di Rhino 
    ''' </summary>
    ''' <param name="oggetto"></param>
    ''' <param name="colore"></param>
    ''' <remarks></remarks>
    Public Shared Sub ImpostaColoreAdOggetto(ByVal oggetto As MRhinoObject, ByVal colore As Drawing.Color)
        Dim refOggetto As New MRhinoObjRef(oggetto)
        Dim attributi As New MRhinoObjectAttributes(refOggetto.Object.Attributes)
        attributi.SetColorSource(IOn.object_color_source.color_from_object)
        attributi.m_color = New OnColor(colore)
        RhUtil.RhinoApp.ActiveDoc.ModifyObjectAttributes(refOggetto, attributi)
        attributi.Dispose()
        refOggetto.Dispose()
    End Sub


    ''' <summary>
    ''' Ritorna la versione major di Rhinoceros
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function RhinoMajorRelease() As Integer
        Dim name As String = ""
        RhUtil.RhinoApp.GetApplicationName(name)
        If name.Contains("4.0") Then Return 4
        If name.Contains("5.0") Then Return 5
        If name.Contains("6.0") Then Return 6
        Return -1
    End Function


End Class
