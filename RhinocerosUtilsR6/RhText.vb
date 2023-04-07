
Imports RMA.Rhino
Imports RMA.OpenNURBS


Public Class RhText


    Public Shared Function ChangeObjectText(objId As Guid, newText As String) As Boolean
        Dim obj As MRhinoObject = RhUtil.RhinoApp.ActiveDoc.LookupObject(objId)
        If obj Is Nothing Then
            MsgBox("Impossibile trovare oggetto con id = " & objId.ToString())
            Return false
        End If
        Return ChangeObjectText(obj, newText)
    End Function

    Public Shared Function ChangeObjectText(objName As String, newText As String) As Boolean
        Dim obj As MRhinoObject = RhDocument.ObjectByName(objName, False)
        If obj Is Nothing Then
            MsgBox("Impossibile trovare oggetto con nome = " & objName)
            Return false
        End If
        Return ChangeObjectText(obj, newText)
    End Function

    Private Shared Function ChangeObjectText(obj As MRhinoObject, newText As String) As Boolean
        'CONTROLLI
        If obj.ObjectType <> IOn.object_type.annotation_object Then
            MsgBox("L'oggetto non è un'annotazione")
            Return false
        End If
        Dim objref As New MRhinoObjRef(obj.Attributes.m_uuid)
        Dim oldAnnotation As IOnAnnotation2 = objref.Annotation()
        If oldAnnotation Is Nothing OrElse Not oldAnnotation.IsText() Then
            MsgBox("Annotazione non testuale")
            Return false
        End If
        objref.Dispose()
        'MODIFICA      
        Dim annotationObj As MRhinoAnnotationObject = MRhinoAnnotationObject.Cast(obj)
        'Dim new_obj As MRhinoAnnotationObject = annotation_obj.Duplicate()                               
        Dim text As New MRhinoText(annotationObj.m_text)
        text.SetString(newText)
        annotationObj.m_text = text
        annotationObj.SetUserText(newText)
        annotationObj.UpdateText()
        Return True
    End Function


End Class
