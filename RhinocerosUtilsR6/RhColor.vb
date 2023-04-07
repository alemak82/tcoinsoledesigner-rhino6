Imports RMA.OpenNURBS

'********************************************
'*** Classe per la gestione del colore    ***
'*** Non utilizzare: usare classe OnColor ***
'********************************************

Public Class RhColor

    ''' <summary>
    ''' Crea un colore tra fuxsia e verde da un valore [0,1]
    ''' </summary>
    ''' <param name="valore"></param>
    ''' <returns></returns>
    ''' <remarks>Valutare l'utilizzo dello "hue tramite rappresentazione HSV"</remarks>
    Public Shared Function ColoreDaValore(ByVal valore As Double) As OnColor
        If valore < 0 Then valore = 0
        If valore > 1 Then valore = 1
        Dim r, g, b As Integer
        If valore < (1 / 3) Then
            r = 255
            g = 0
            b = CInt(255 * (1 - 3 * valore))
        ElseIf valore < (2 / 3) Then
            r = 255
            g = CInt(255 * (3 * valore - 1))
            b = 0
        Else
            r = CInt(255 * (3 - 3 * valore))
            g = 255
            b = 0
        End If
        Return New OnColor(r, g, b)
    End Function


    ''' <summary>
    ''' Ricava un valore [0,1] da un colore tra fuxsia e verde 
    ''' </summary>
    ''' <param name="colore"></param>
    ''' <returns></returns>
    ''' <remarks>Valutare l'utilizzo dello "hue" tramite rappresentazione HSV</remarks>
    Public Shared Function ValoreDaColore(ByVal colore As IOnColor) As Double
        If colore.Green = 0 Then
            Return (1 - colore.Blue / 255) / 3
        ElseIf colore.Red = 255 Then
            Return (1 + colore.Green / 255) / 3
        Else
            Return (3 - colore.Red / 255) / 3
        End If
    End Function


    ''FUNZIONE GIA' PRESENTE IN RHMISC
    'Public Shared Sub ChangeObjectColor(ByRef rhinoObj As MRhinoObject, ByVal color As Drawing.Color)
    '    If RhUtil.RhinoApp.ActiveDoc Is Nothing Then Exit Sub
    '    If rhinoObj Is Nothing Then Exit Sub
    '    Dim objAttr As New MRhinoObjectAttributes(rhinoObj.Attributes)
    '    objAttr.SetColorSource(IOn.object_color_source.color_from_object)
    '    objAttr.m_color = New OnColor(Drawing.Color.Red)
    '    RhUtil.RhinoApp.ActiveDoc.ModifyObjectAttributes(New MRhinoObjRef(rhinoObj.Attributes.m_uuid), objAttr)
    '    objAttr.Dispose()
    'End Sub


End Class


