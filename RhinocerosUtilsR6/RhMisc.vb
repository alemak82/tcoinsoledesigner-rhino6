Imports RMA.Rhino
Imports RMA.OpenNURBS


'**************************************************
'*** Classe fer funzionalità varie dentro Rhino ***
'**************************************************

Public Class RhMisc


    ''' <summary>
    ''' Crea una finestra MsgBox dentro Rhino
    ''' </summary>
    ''' <param name="strText"></param>
    ''' <param name="strTitle"></param>
    ''' <remarks></remarks>
    Public Shared Sub RhMsgbox(ByVal strText As String, Optional ByVal strTitle As String = "")
        RhUtil.RhinoMessageBox(strText, strTitle, Convert.ToUInt32(Windows.Forms.MessageBoxButtons.OK))
    End Sub



    ''' <summary>
    ''' Mantieni anteprima degli oggetti grafici
    ''' </summary>
    ''' <param name="oggetti"></param>
    ''' <remarks></remarks>
    Public Shared Sub GestisciAnteprima(ByRef oggetti As ArrayList)
        Dim gop As New MRhinoGetOption
        gop.AcceptNothing()
        gop.SetCommandPrompt("Premere invio per chiudere")
        Dim optionCancellaAnteprima As New MRhinoGet.BooleanOption(True)
        gop.AddCommandOptionToggle(New MRhinoCommandOptionName("DeletePreview", "CancellaAnteprima"), New MRhinoCommandOptionValue("False", "False"), New MRhinoCommandOptionValue("True", "True"), optionCancellaAnteprima.Value, optionCancellaAnteprima)
        Do
            gop.GetOption()
        Loop While gop.GetResult = IRhinoGet.result.option

        'Cancella oggetti in anteprima
        If optionCancellaAnteprima.Value Then
            For i As Integer = 0 To oggetti.Count - 1
                RhUtil.RhinoApp.ActiveDoc.PurgeObject(DirectCast(oggetti(i), MRhinoObject))
            Next
            RhUtil.RhinoApp.ActiveDoc.Redraw()
        End If
        optionCancellaAnteprima.Dispose()
        gop.Dispose()
    End Sub


    ''' <summary>
    ''' Calcola valori statistici di un array di vettori
    ''' </summary>
    ''' <param name="vectors"></param>
    ''' <param name="meanVector"></param>
    ''' <param name="deviationVector"></param>
    ''' <remarks></remarks>
    Public Shared Function VectorStatistics(ByVal vectors As IOn3fVectorArray, ByRef meanVector As On3fVector, ByRef deviationVector As On3fVector) As Boolean
        If vectors Is Nothing Then Return False
        If vectors.Count = 0 Then Return False
        If meanVector Is Nothing Then
            meanVector = New On3fVector(0, 0, 0)
        Else
            meanVector.Zero()
        End If
        For i As Integer = 0 To vectors.Count - 1
            meanVector += vectors(i)
        Next
        meanVector *= CSng(1 / vectors.Count)
        If deviationVector Is Nothing Then
            deviationVector = New On3fVector(0, 0, 0)
        Else
            deviationVector.Zero()
        End If
        For i As Integer = 0 To vectors.Count - 1
            Dim standardDeviation As On3fVector = New On3fVector(vectors(i)) - meanVector
            standardDeviation.x = standardDeviation.x * standardDeviation.x
            standardDeviation.y = standardDeviation.y * standardDeviation.y
            standardDeviation.z = standardDeviation.z * standardDeviation.z
            deviationVector += standardDeviation
        Next
        deviationVector *= CSng(1 / vectors.Count)
        deviationVector.x = CSng(Math.Sqrt(deviationVector.x))
        deviationVector.y = CSng(Math.Sqrt(deviationVector.y))
        deviationVector.z = CSng(Math.Sqrt(deviationVector.z))
        Return True
    End Function


    ''' <summary>
    ''' Funzione per mappare una fattore lineare in sinusoidale nel dominio (0,1)
    ''' </summary>
    ''' <param name="fattoreLineare"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function FattoreSinusoidale(ByVal fattoreLineare As Double) As Double
        Return 0.5 * Math.Sin(Math.PI * (fattoreLineare - 0.5)) + 0.5
    End Function


End Class

