Imports System.IO
Imports InsoleDesigner.bll
Imports InsoleDesigner.bll.IdAlias
Imports RMA.Rhino
Imports Rhino.Commands


'*******************************************************************************
'*** Classe per l'intercettazione di eventi di Rhino ***
'*** Vengono riportati alcuni possibili eventi: molti altri sono disponibili ***
'*******************************************************************************


Public Class IdEventWatcher
  Inherits MRhinoEventWatcher



#Region " Campi "

  Private ThisPlugIn As IdPlugIn = IdPlugIn.Instance()

#End Region


#Region " Constructor "

  Public Sub New()
    MyBase.New()    
  End Sub

#End Region


#Region " Alias "

  Private ReadOnly Property UndoRedoPlugin() As UndoRedoManager
    Get
      Return UndoRedoManager.GetInstance()
    End Get
  End Property

#End Region


#Region " Eventi "


#Region " Documenti "

  Public Overrides Sub OnBeginOpenDocument(ByRef doc As RMA.Rhino.MRhinoDoc, ByVal filename As String, ByVal bMerge As Boolean, ByVal bReference As Boolean)
    '#If DEBUG Then
    '        RhUtil.RhinoApp.Print("Event watcher caught OnBeginOpenDocument" + vbCrLf)
    '#End If
    MyBase.OnBeginOpenDocument(doc, filename, bMerge, bReference)

    'ToDo: Inserire operazioni necessarie sui dati del PlugIn
  End Sub

  Public Overrides Sub OnEndOpenDocument(ByRef doc As RMA.Rhino.MRhinoDoc, ByVal filename As String, ByVal bMerge As Boolean, ByVal bReference As Boolean)
    '#If DEBUG Then
    '        RhUtil.RhinoApp.Print("Event watcher caught OnEndOpenDocument" + vbCrLf)
    '#End If
    MyBase.OnEndOpenDocument(doc, filename, bMerge, bReference)

    'ToDo: Inserire operazioni necessarie sui dati del PlugIn      
  End Sub

  Public Overrides Sub OnCloseDocument(ByRef doc As RMA.Rhino.MRhinoDoc)
    '#If DEBUG Then
    '        RhUtil.RhinoApp.Print("Event watcher caught OnCloseDocument" + vbCrLf)
    '#End If
    MyBase.OnCloseDocument(doc)

    'Reset di tutti i riferimenti che non sono più validi
    Element3dManager.ResetAll()
    ThisPlugIn.MainPanel.SetPanelView(IdPlugIn.ePluginPhase.start)
    UndoRedoManager.GetInstance.Enable = False
    If Not ThisPlugIn.DisplayConduit Is Nothing Then ThisPlugIn.DisplayConduit.ClearObjectList()
  End Sub

#End Region


#Region " Oggetti "

  Public Overrides Sub OnAddObject(ByRef doc As RMA.Rhino.MRhinoDoc, ByRef obj As RMA.Rhino.MRhinoObject)
    '#If DEBUG Then
    '        RhUtil.RhinoApp.Print("Event watcher caught OnAddObject" + vbCrLf)
    '#End If
    MyBase.OnAddObject(doc, obj)

    'ToDo: Inserire operazioni necessarie sui dati del PlugIn
  End Sub

  Public Overrides Sub OnDeleteObject(ByRef doc As RMA.Rhino.MRhinoDoc, ByRef obj As RMA.Rhino.MRhinoObject)
    '#If DEBUG Then
    '        RhUtil.RhinoApp.Print("Event watcher caught OnDeleteObject" + vbCrLf)
    '#End If
    MyBase.OnDeleteObject(doc, obj)

    'ToDo: Inserire operazioni necessarie sui dati del PlugIn
  End Sub

  Public Overrides Sub OnReplaceObject(ByRef doc As RMA.Rhino.MRhinoDoc, ByRef old_object As RMA.Rhino.MRhinoObject, ByRef new_object As RMA.Rhino.MRhinoObject)
    '#If DEBUG Then
    '        RhUtil.RhinoApp.Print("Event watcher caught OnReplaceObject" + vbCrLf)
    '#End If
    MyBase.OnReplaceObject(doc, old_object, new_object)

    ''SOSTITUZIONE DI UN OGGETTO DEL PLUGIN TRA LE REFERENCE
    'If Helper.ReferenceExist(old_object.Attributes.m_uuid) Then
    '    Dim side As eSide = Helper.FindObjectSide(old_object.Attributes.m_uuid)
    '    Dim type As eReferences = Helper.FindObjectType(old_object.Attributes.m_uuid)
    '    Helper.SetRhinoObj(type, side, new_object.Attributes.m_uuid)
    'End If
    ''SOSTITUZIONE DI UNA CURVA DI SEZIONE DEL PIEDE
    'If new_object.ObjectType = RMA.OpenNURBS.IOn.object_type.curve_object AndAlso Helper.IsFootCurve(old_object.Attributes.m_uuid) Then
    '    Dim side As eSide = Helper.GetFootCurveSide(old_object.Attributes.m_uuid)
    '    Helper.ReplaceFootCurve(side, old_object.Attributes.m_uuid, new_object.Attributes.m_uuid)
    'End If
  End Sub


  Public Overrides Sub OnUnDeleteObject(ByRef doc As RMA.Rhino.MRhinoDoc, ByRef [object] As RMA.Rhino.MRhinoObject)
    '#If DEBUG Then
    '        RhUtil.RhinoApp.Print("Event watcher caught OnUnDeleteObject" + vbCrLf)
    '#End If
    MyBase.OnDeleteObject(doc, [object])

    'ToDo: Inserire operazioni necessarie sui dati del PlugIn
  End Sub

#End Region


#Region " Comandi "



  Public Overrides Sub OnBeginCommand(command As RMA.Rhino.IRhinoCommand, context As RMA.Rhino.IRhinoCommandContext)
    MyBase.OnBeginCommand(command, context)

    ''ATTENZIONE UN MIO COMANDO LANCIA DIVERSI ALTRI COMANDI -> QUINDI SE SONO DENTRO UN MIO COMANDO SCARIGO TUTTO
    If RhUtil.RhinoApp.InCommand > 1 Or Not FormManager.CheckNotModal() Then Exit Sub

    ''DEVO CAPIRE PRIMA DI TUTTO SE SI TRATTA DI UN MIO COMANDO ED EVENTUALMENTE ABILITARE IL GESTORE
    UndoRedoPlugin.Enable = False
    If IsMyCommand(command) AndAlso Not UndoRedoPlugin.CommandUseRhinoUndo(command) Then UndoRedoPlugin.Enable = True

    ''SE SI TRATTA DI UN UNDO E REDO ABILITO IL GESTORE SE NON E' VUOTO -> PREVALE SU QUELLO DI RHINO CHE DI DEFAUL E' ABILITATO
    If command.EnglishCommandName() = "Undo" Or command.EnglishCommandName() = "Redo" Then
      UndoRedoPlugin.Enable = Not UndoRedoPlugin.IsStackEmpty()
      Exit Sub
    End If

    ''SE E' ABILITATO IL GESTORE E SALVO LO STATO
    If UndoRedoPlugin.Enable Then
      UndoRedoPlugin.TakeSnapshot(command)
    Else
      ''SE E' DISABILITATO SVUOTO LO STACK
      UndoRedoPlugin.ClearStack()
    End If

  End Sub




  Public Overrides Sub OnEndCommand(command As RMA.Rhino.IRhinoCommand, context As RMA.Rhino.IRhinoCommandContext, rc As RMA.Rhino.IRhinoCommand.result)
    MyBase.OnEndCommand(command, context, rc)

    ''SE IL COMANDO E' UN UNDO/REDO E IL GESTORE E' ABILITATO E NON CI SONO FINESTRE APERTE ALLORA USO IL GESTORE
    If UndoRedoManager.GetInstance.Enable And FormManager.CheckNotModal() Then
      Dim needMainPanelUpdate = False
      If command.EnglishCommandName() = "Undo" Then
        UndoRedoPlugin.UndoCustom(needMainPanelUpdate)
      End If
      If command.EnglishCommandName() = "Redo" Then
        UndoRedoPlugin.RedoCustom(needMainPanelUpdate)
      End If
      If needMainPanelUpdate Then ''AGGIORNO GUI
        ThisPlugIn.MainPanel.SetPanelView(IdPlugIn.ePluginPhase.autoSetting)
      End If
    End If

    ''ALLA FINE NON POSSO SAPERE QUALE RIABILITARE QUINDI PER DEFAULT ABILITO QUELLO DI RHINO
    UndoRedoPlugin.Enable = False
  End Sub


  'Public Overrides Sub UndoEvent(undo_event As RMA.Rhino.IRhinoEventWatcher.undo_event, undo_record_serialnumber As UInteger, cmd As RMA.Rhino.IRhinoCommand)
  '    MyBase.UndoEvent(undo_event, undo_record_serialnumber, cmd)

  '    'For Each command As MRhinoCommand In Me.ThisPlugIn.Commands()
  '    '    If cmd.EnglishCommandName = command.EnglishCommandName Then
  '    '        'UNDO
  '    '        If undo_event = IRhinoEventWatcher.undo_event.begin_undo Then MsgBox("IsUndoable = " & cmd.IsUndoable.ToString)
  '    '        If undo_event = IRhinoEventWatcher.undo_event.end_undo Then MsgBox("undo " & cmd.EnglishCommandName & " " & undo_event.ToString)
  '    '        'REDO
  '    '        If undo_event = IRhinoEventWatcher.undo_event.begin_redo Then MsgBox("IsUndoable = " & cmd.IsUndoable.ToString)
  '    '        If undo_event = IRhinoEventWatcher.undo_event.end_redo Then MsgBox("redo " & cmd.EnglishCommandName & " " & undo_event.ToString)     
  '    '    End If
  '    'Next

  'End Sub


#End Region


#End Region


#Region " Utils "


  Private Function IsMyCommand(command As Command) As Boolean
    For Each mycommand As Command In IdPlugIn.Instance().GetCommands()
      If mycommand.EnglishName = command.EnglishName Then Return True
    Next
    Return False
  End Function


#End Region



End Class
