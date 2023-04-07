Imports RMA.Rhino
Imports System.Windows.Forms


'**********************************************************************************************************************************************************
'*** QUESTA CLASSE HA LO SCOPO DI MANTENERE UN RIFERIMENTO DEI FORM NON MODALI IN MODO DA VERIFICARE SE SIANO ANCORA VISIBILI O MENO                    ***
'*** QUINDI INVECE DI FARE form.show() SI DEVE USARE IL METODO DI QUESTA CLASSE                                                                         ***
'**********************************************************************************************************************************************************


Public Class RhFormNonModalManager



#Region " Field "


    Private Shared mInstance As RhFormNonModalManager
    Public CurrentForm As Form

#End Region


#Region " Constructor "


    Private Sub New()
    End Sub

    Public Shared Function GetInstance() As RhFormNonModalManager
        If mInstance Is Nothing Then
            mInstance = New RhFormNonModalManager()
        End If
        Return mInstance
    End Function


#End Region


#Region " CLASS LOGIC "

    Public Sub ShowForm(ByVal formType As Type, ByVal cmd As MRhinoScriptCommand)
        CurrentForm = CType(Activator.CreateInstance(formType, New Object() {cmd}), Form)
        CurrentForm.Show(RhUtil.RhinoApp.MainWnd)
        CurrentForm.BringToFront()
        CurrentForm.Focus()
    End Sub


    Public Function CheckNotModal() As Boolean
        If CurrentForm IsNot Nothing AndAlso CurrentForm.Visible Then Return False
        'Questo è un test che dovrebbe risolvere il problema di certi delegati dei form che rimangono attivi anche dopo che il form è stato chiuso
        If CurrentForm IsNot Nothing AndAlso Not CurrentForm.Visible Then CurrentForm.Dispose()
        Return True
    End Function


#End Region


End Class
