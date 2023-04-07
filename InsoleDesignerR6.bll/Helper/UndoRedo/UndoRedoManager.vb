Imports RMA.Rhino
Imports RMA.OpenNURBS
Imports InsoleDesigner.bll.IdElement3dManager
Imports InsoleDesigner.bll.IdAlias
Imports System.Reflection
Imports RhinoUtils


Public Class UndoRedoManager


#Region " Constant "

    Private Const UNDO_CMD As String = "Undo"

#End Region


#Region " Field "

    Private Shared mInstance As UndoRedoManager
    Private mEnable As Boolean

    'ANDREBBERO SOSTITUITE CON  Private mStack As Stack(Of UndoRedoRecord)
    Private mStack As List(Of UndoRedoRecord)
    Private mCurrentIndex As Integer

    ''QUI VANNO INSERITI I NOMI DEI COMANDI CHE USANO L'UNDO DI RHINO OVVERO QUELLI CHE NON AGGIUNGONO, MODIFICANO(UUID), ELIMINANO OGGETTI USATI DAL PLUGIN
    'AD ESEMPIO IL COMANDO DI ALLINEAMENTO NON MODIFICA L'UUID
    'AD ESEMPIO IL COMANDO RIGHELLO AGGIUNGE OGGETTI NON SALVATI NEL PLUGIN MA SONO OGGETTI "USA E GETTA"
    Private mCommandUndoByRhino() As String = {"IdAlign", "IdAddRuler"}


#End Region


#Region " Constructor "

    Private Sub New()
        mEnable = False
        mStack = New List(Of UndoRedoRecord)
        mCurrentIndex = 0
    End Sub

    Public Shared Function GetInstance() As UndoRedoManager
        If mInstance Is Nothing Then
            mInstance = New UndoRedoManager()
        End If
        Return mInstance
    End Function

#End Region


#Region " Property "


    ''' <summary>
    ''' Garantisce che sia abilitato il recorder di Rhino o quello Custom
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Enable() As Boolean
        Get
            Return mEnable
        End Get
        Set(value As Boolean)
            If value Then
                RhUtil.RhinoApp.ActiveDoc.ClearRedoRecords()
                RhUtil.RhinoApp.ActiveDoc.ClearUndoRecords()
            End If
            RhUtil.RhinoApp.ActiveDoc.EnableUndoRecording(Not value)
            mEnable = value
        End Set
    End Property


    Public ReadOnly Property LastRecord As UndoRedoRecord
        Get
            If mStack.Count = 0 Then Return Nothing
            Dim index As Integer = mStack.Count - 1
            Return mStack.Item(index)
        End Get
    End Property


#End Region


#Region " GESTIONE STACK DEI RECORD "


    ''' <summary>
    ''' Se il recorder è abilitato salva lo stato nello stack
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub TakeSnapshot(command As IRhinoCommand)
        If Not Me.mEnable Then Exit Sub
        'Per il problema dei comandi doppi potrei banalmente controllare se l'ultimo record ha lo stesso EnglishCmdName
        'Okkio ci potrebbero essere delle eccezioni in certi comandi 
        If mStack.Count = 0 Then
            AddRecord(New UndoRedoRecord(command.EnglishCommandName))
        Else
            If LastRecord.EnglishCmdName = UNDO_CMD Then
                ReplaceUndoRecord(New UndoRedoRecord(command.EnglishCommandName))
            Else
                AddRecord(New UndoRedoRecord(command.EnglishCommandName))
            End If
        End If
    End Sub

    ''' <summary>
    ''' Dopo uno o più Undo sovrascrivo 
    ''' </summary>
    ''' <param name="newRecord"></param>
    ''' <remarks></remarks>
    Private Sub ReplaceUndoRecord(ByVal newRecord As UndoRedoRecord)
        If mStack.Count > mCurrentIndex Then
            mStack.RemoveRange(mCurrentIndex, (mStack.Count - mCurrentIndex))
        End If
        AddRecord(newRecord)
    End Sub

    Private Sub AddRecord(ByVal newRecord As UndoRedoRecord)
        mStack.Add(newRecord)
        If Not newRecord.EnglishCmdName = UNDO_CMD Then mCurrentIndex += 1
    End Sub

    ''' <summary>
    ''' Serve per evitare che i comandi doppi creino due record
    ''' </summary>
    ''' <remarks>Con questo metodo si deve rimuovere il secondo record</remarks>
    Public Sub RemoveLastRecord()
        Dim index As Integer = mStack.Count - 1
        mStack.RemoveAt(index)
        mCurrentIndex -= 1
    End Sub


    Public Function IsStackEmpty() As Boolean
        Return (mStack.Count = 0)
    End Function

    Public Sub ClearStack()
        ''Ho rimosso il supporto per il Dispose perchè dava problemi
        'While mStack.Count() > 0
        '    Dim item As CustomUndoRedoRecord = mStack(0)
        '    mStack.RemoveAt(0)
        '    If item IsNot Nothing Then item.Dispose()
        'End While
        mStack.Clear()
        mStack = Nothing
        mStack = New List(Of UndoRedoRecord)
        mCurrentIndex = 0
    End Sub



#End Region


#Region " APPLICATION LOGIC "


    Public Sub UndoCustom(ByRef needMainPanelUpdate As Boolean)
        If mStack.Count > 0 And mCurrentIndex > 0 Then
            'Se dopo una serie di comandi viene fatto Undo aggiungo un record con lo stato corrente allo stack ma senza avanzare il contatore dello stato corrente
            If mCurrentIndex = mStack.Count Then AddRecord(New UndoRedoRecord(UNDO_CMD))
            mCurrentIndex -= 1
            MoveIntoStack(mCurrentIndex)
            RhUtil.RhinoApp.Print(My.Application.Info.Title & " - " & LanguageManager.Message(104) & vbCrLf & vbCrLf)
            needMainPanelUpdate = True
        Else
            'MsgBox("NESSUN RECORD X UNDO")
            needMainPanelUpdate = False
        End If
    End Sub

    Public Sub RedoCustom(ByRef needUpdate As Boolean)
        If mStack.Count > 0 And mCurrentIndex < mStack.Count - 1 Then
            mCurrentIndex += 1
            MoveIntoStack(mCurrentIndex)
            RhUtil.RhinoApp.Print(My.Application.Info.Title & " - " & LanguageManager.Message(105) & vbCrLf)
            needUpdate = True
        Else
            needUpdate = False
        End If
    End Sub

    Private Sub MoveIntoStack(ByVal index As Integer)
        Dim record As UndoRedoRecord = mStack.Item(index)
        'Aggiorno DOC
        OverwriteDoc(record)
        'Aggiorno elementi 3D
        Overwrite3dElement(record)
    End Sub

    Private Sub OverwriteDoc(ByVal record As UndoRedoRecord)
        Try
            MRhinoView.EnableDrawing(False)
            ''IL DOC.Destroy() DAVA PROBLEMI CON L'EVENTUALE SUCCESSIVO SALVATAGGIO
            'RhUtil.RhinoApp.ActiveDoc.Destroy()
            'ELIMINO TUTTI GLI OGGETTI NEL DOC
            Dim tabellaLayer As MRhinoLayerTable = RhUtil.RhinoApp.ActiveDoc.m_layer_table
            For i As Integer = 0 To tabellaLayer.LayerCount - 1
                Dim layer As IRhinoLayer = tabellaLayer.Item(i)
                RhLayer.DeleteLayerObjects(layer.m_layer_index, Nothing, False, True)
            Next
            ''ELIMINO TUTTI I LIVELLI TRANNE IL CORRENTE PER EVITARE ALERT DI RHINO
            Dim currentLayer As IRhinoLayer = tabellaLayer.CurrentLayer
            RhLayer.DeleteAllLayersExcept(currentLayer.m_name, False, True)

            ''Aggiorno layer
            Dim removeOldCurrentLayer As Boolean = True
            For Each layer As OnLayer In record.Layers
                If layer.m_name = currentLayer.m_name Then removeOldCurrentLayer = False
                RhUtil.RhinoApp.ActiveDoc.m_layer_table.AddLayer(layer)
            Next
            'RIPRISTINO IL LAYER CORRENTE
            RhUtil.RhinoApp.ActiveDoc.m_layer_table.SetCurrentLayerIndex(record.CurrentLayer)
            'SE IL VECCHIO LIVELLO CORRENTE NON ESISTE PIU' LO CANCELLO
            If removeOldCurrentLayer Then RhLayer.DeleteLayerHisSublayersAndObjects(currentLayer.m_name, True)

            ''Aggiorno oggetti
            For Each rhinoobj As MRhinoObject In record.Objects
                RhUtil.RhinoApp.ActiveDoc.AddObject(rhinoobj)
            Next

            RhUtil.RhinoApp.RunScript("_SelNone", 0)
        Catch ex As Exception
            MsgBox(LanguageManager.Message(244))
            IdLanguageManager.PromptError(ex.Message)
        Finally
            MRhinoView.EnableDrawing(True)
            RhUtil.RhinoApp.ActiveDoc.Redraw()
        End Try
    End Sub

    Private Sub Overwrite3dElement(ByVal record As UndoRedoRecord)
        Dim helper As IdElement3dManager = IdElement3dManager.GetInstance()
        'Oggetti globali
        helper.ResetReferences()
        helper.DeleteAllFootCurves(False)
        helper.ClearFootCurvesTrimSurface()
        helper.ClearAddictions(False)
        For Each sideType As eSide In System.Enum.GetValues(GetType(eSide))
            For Each refType As eReferences In System.Enum.GetValues(GetType(eReferences))
                Dim uuid As Guid = record.ReferenceGuid(refType, sideType)
                helper.SetRhinoObj(refType, sideType, uuid)
            Next
        Next
        Overwrite3dElement(record, eSide.left)
        Overwrite3dElement(record, eSide.right)
        'Mappe pressione
        helper.ModelPressureMap = record.GetModelPressureMap
    End Sub

    Private Sub Overwrite3dElement(ByVal record As UndoRedoRecord, ByVal side As eSide)
        Dim helper As IdElement3dManager = IdElement3dManager.GetInstance()
        'Curve di sezione
        For Each pair As KeyValuePair(Of Double, Guid) In record.FootCurves(side)
            helper.AddFootCurve(side, pair.Key, pair.Value)
        Next
        'Superfici di taglio
        For Each pair As KeyValuePair(Of eTrimSurface, OnSurface) In record.FootCurvesTrimSurface(side)
            helper.SetFootCurvesTrimSurface(side, pair.Key, pair.Value)
        Next
        'Scarichi
        For Each addiction As IdAddiction In record.GetAddictions(side)
            helper.AddAddiction(addiction)
        Next
        'Piani inclinati
        Dim pianoInclinato = record.GetPianoInclinato(side)
        If pianoInclinato Is Nothing Then 
            helper.ClearPianoInclinato(side, False)
        Else 
            helper.SetPianoInclinato(side, pianoInclinato)
        End If
    End Sub


#End Region


#Region " Utils "

    Public Function CommandUseRhinoUndo(ByVal command As IRhinoCommand) As Boolean
        For Each cmd As String In mCommandUndoByRhino
            If cmd = command.EnglishCommandName Then Return True
        Next
        Return False
    End Function


#End Region


End Class
