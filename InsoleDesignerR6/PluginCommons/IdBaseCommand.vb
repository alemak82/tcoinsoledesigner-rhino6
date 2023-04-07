Imports InsoleDesigner.bll
Imports InsoleDesigner.bll.IdAlias
Imports RMA.Rhino
Imports Rhino
Imports Rhino.Commands
Imports Rhino.Geometry
Imports Rhino.Input
Imports Rhino.Input.Custom
Imports System.Linq


'********************************************************
'*** Classe base da cui derivare i comandi del PlugIn ***
'********************************************************

Public MustInherit Class IdBaseCommand
    Inherits Command



#Region " Common Plugin Functions "

    Public Overrides ReadOnly Property EnglishName() As String
        Get
            Return "IdBaseCommand"
        End Get
    End Property

#End Region


    ''' <summary>
    ''' Supporto per la doppia chiamata del comando (supporto finestra modale ed Undo)
    ''' </summary>
    Protected mFirstCall As Boolean = True


    Public ReadOnly Property FirstCall() As Boolean
        Get
            Return Me.mFirstCall
        End Get
    End Property

    Public Sub SetFirstCall(ByVal firstCall As Boolean)
        Me.mFirstCall = firstCall
        'Dato che IdEventWatcher non posso farlo perchè ho solo l'interfaccia generica dei comandi, rimuovo il record intermedio di un comando che è inutile
        If firstCall Then
            Dim undoRedoManager As UndoRedoManager = undoRedoManager.GetInstance()
            If undoRedoManager.Enable And Not undoRedoManager.IsStackEmpty Then undoRedoManager.RemoveLastRecord()
        End If
    End Sub


    ''' <summary>
    ''' NB: Il comando sarà visibile in Rhino ma non deve fare nulla
    ''' </summary>
    Protected Overrides Function RunCommand(ByVal doc As RhinoDoc, ByVal mode As RunMode) As Result
        'Return Result.Cancel
        Return RunCommandNoDoc(mode)
    End Function


    Protected MustOverride Function RunCommandNoDoc(ByVal mode As RunMode) As Result
    

    ''' <summary>
    ''' Verifica che sia possibile eseguire il comando se non ci sono finestre caricate 
    ''' </summary>
    ''' <param name="formType">In caso di finestra di questo tipo viene ignorata e ritorna TRUE</param>
    ''' <returns></returns>
    Public Shared Function StartCommandCheck(Optional formType As Type = Nothing) As Boolean
        If RhUtil.RhinoApp.InCommand > 1 Then
            MsgBox(LanguageManager.Message(232), MsgBoxStyle.Information, My.Application.Info.Title)
            Return False
        End If 
        ''ToDo???????????????????????????????????????????????????????
        'If Not IdPlugIn.ThisPlugIn().ActiveCommandForm Is Nothing Then       
        '    MsgBox(LanguageManager.Message(233), MsgBoxStyle.Information, My.Application.Info.Title)
        '    Return False
        'End If
        ''AGGIUNTO CONTROLLO CHE PERMETTE DI AVVIARE COMANDO CON UN FORM APERTO DI UN CERTO TIPO: USATO PER IdRemoveLastAddictionCommand
        If FormManager.CurrentForm Is Nothing Then Return True
        If formType IsNot Nothing And FormManager.CurrentForm.GetType() = formType Then Return True
        ''-------------------------------------------------------------------------------------------
        If Not FormManager.CheckNotModal Then
            MsgBox(LanguageManager.Message(24, FormManager.CurrentForm.Text), MsgBoxStyle.Information, My.Application.Info.Title)
            Return False
        End If
        Return True
    End Function


    ''' <summary>
    ''' Stampa il messaggio nel prompt di Rhino
    ''' </summary>
    ''' <param name="finishMessage"></param>
    Public Sub FinishMessage(ByVal finishMessage As String)
        'RhUtil.RhinoApp.ClearCommandHistoryWindowText()        
        RhinoApp.WriteLine(vbCrLf & My.Application.Info.Title & " - " & finishMessage & vbCrLf & vbCrLf)
    End Sub

    ''' <summary>
    ''' Recupera il messaggio di fine comando nella lingua corrente e chiama il metodo per la stampa nel prompt
    ''' </summary>
    ''' <param name="finishMessageIndex">Indice per la traduzione del messaggio</param>
    ''' <remarks></remarks>
    Public Sub FinishMessage(ByVal finishMessageIndex As Integer)
        FinishMessage(LanguageManager.Message(finishMessageIndex))
    End Sub


    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub


End Class
