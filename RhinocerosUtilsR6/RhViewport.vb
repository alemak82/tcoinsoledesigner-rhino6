Imports RMA.OpenNURBS
Imports RMA.Rhino


'**********************************************************************************************************************************************************
'*** 25-08-2015 ALESSANDRO MACCHIONE:                                                                                                                   ***
'*** Creata questa classe per gestire le viste e i piani di costruzione che influiscono ad esempio sui parametri per le sclature                        ***
'**********************************************************************************************************************************************************


Public Class RhViewport


#Region " Gestione Views - prese da last Designer "

    Public Enum eView As Integer
        perspective
        top
        bottom
        extern
        intern
        front
        back
    End Enum

    Public Enum eViewportNames As Integer
        Top
        Bottom
        Left
        Right
        Front
        Back
    End Enum


    ''' <summary>
    ''' Massimizza la vista prospettica e imposta le viste Ghosted
    ''' </summary>
    ''' <param name="restore4view">Serve se le viste erano state cambiate in precedenza</param>
    ''' <remarks></remarks>
    Public Shared Sub MaximizePerspectiveView(Optional ByVal restore4view As Boolean = False)
        If restore4view Then RhUtil.RhinoApp().RunScript("_4View", 0)
        Dim viste() As MRhinoView = Nothing
        If Not RhUtil.RhinoApp.ActiveDoc Is Nothing Then
            RhUtil.RhinoApp.ActiveDoc.GetViewList(viste)
            For Each vista As MRhinoView In viste
                'Impostazione Ghosted
                vista.ActiveViewport.EnableGhostedShade(True)
                If vista.ActiveViewport.VP.Projection = RMA.OpenNURBS.IOn.view_projection.perspective_view Then
                    If Not vista.IsMaximized Then
                        vista.MaximizeRestoreView()
                    End If
                    RhUtil.RhinoApp.RunScript("_Zoom _All _Extents", 0)
                End If
            Next
        End If
    End Sub


    Public Shared Function GetViewName(ByVal view As eView) As String
        Select Case view
            Case eView.perspective
                Return "Perspective"
            Case eView.top
                Return "Top"
            Case eView.bottom
                Return "Bottom"
            Case eView.front
                Return "Front"
            Case eView.back
                Return "Back"
            Case eView.extern
                Return "Extern"
            Case eView.intern
                Return "Intern"
        End Select
        Return ""
    End Function


    Public Shared Sub SetView(ByVal view As eView)
        Dim command As String = "_IdView _"
        command &= GetViewName(view)
        RhUtil.RhinoApp.RunScript(command, 0)
    End Sub



    ''' <summary>
    ''' Data la viewport di riferimento ritorna la direzione della camera
    ''' </summary>
    ''' <param name="referenceView"></param>
    ''' <returns></returns>
    Public Shared Function ViewportGetCameraDirection(ByVal referenceView As eViewportNames) As IOn3dVector
        Select Case referenceView
            Case eViewportNames.Top : Return New On3dVector(0, 0, -1)
            Case eViewportNames.Bottom : Return New On3dVector(0, 0, 1)
            Case eViewportNames.Left : Return New On3dVector(1, 0, 0)
            Case eViewportNames.Right : Return New On3dVector(-1, 0, 0)
            Case eViewportNames.Front : Return New On3dVector(0, 1, 0)
            Case eViewportNames.Back : Return New On3dVector(0, -1, 0)
            Case Else : Return Nothing
        End Select
    End Function


    ''' <summary>
    ''' Attiva una Viewport specifica
    ''' </summary>
    ''' <param name="referenceView"></param>
    ''' <returns></returns>
    Public Shared Function ViewportActivate(ByVal referenceView As eViewportNames) As Boolean
        Dim cameraDirection As IOn3dVector = ViewportGetCameraDirection(referenceView)
        Return ViewportActivate(cameraDirection)
    End Function

    ''' <summary>
    ''' Attiva una Viewport specifica
    ''' </summary>
    ''' <param name="cameraDirection"></param>
    ''' <returns></returns>
    Public Shared Function ViewportActivate(ByVal cameraDirection As IOn3dVector) As Boolean
        'Cicla le views e attiva quella scelta
        Dim result As Boolean = False
        Dim views() As MRhinoView = Nothing
        RhUtil.RhinoApp.ActiveDoc.GetViewList(views)
        For i As Integer = 0 To views.GetUpperBound(0)
            'Estraggo la camera direction e la normalizzo (per evitare problemi con il valore della coordinata che Rhino imposta di default +-100)
            Dim tmpCameraDirection As On3dVector = views(i).ActiveViewport.m_v.m_vp.CameraDirection
            tmpCameraDirection.Unitize()
            'Verifico che sia la stessa direazione e stesso verso: se si attivo questa vista
            If (tmpCameraDirection = cameraDirection) Then
                RhUtil.RhinoApp.SetActiveView(views(i))
                result = True
                Exit For
            End If
        Next
        Return result
    End Function

#End Region


#Region " Gestione Display Modes - presi da last Designer "

    Public Enum eDisplayMode As Integer
        ghostedThick
        ghosted40
        ghosted50
        ghosted60
        shadedNoWireThick
        shadedNoWire
        shadedWire
    End Enum


    Public Shared Function DisplayModeName(ByVal mode As eDisplayMode) As String
        Select Case mode
            Case eDisplayMode.shadedNoWireThick
                Return "ID Shaded NoWire DLG70"
            Case eDisplayMode.shadedNoWire
                Return "ID Shaded NoWire G70"
            Case eDisplayMode.shadedWire
                Return "ID Shaded Wire G70"
            Case eDisplayMode.ghostedThick
                Return "ID Ghosted 01 DLDPG55T40"
            Case eDisplayMode.ghosted40
                Return "ID Ghosted 02 G55T40"
            Case eDisplayMode.ghosted50
                Return "ID Ghosted 03 G55T50"
            Case eDisplayMode.ghosted60
                Return "ID Ghosted 04 G55T60"
        End Select
        Return ""
    End Function


    Public Shared Sub SetDisplayMode(ByVal mode As eDisplayMode)
        Dim command As String = "_SetDisplayMode _M "
        command &= DisplayModeName(mode).Replace(" ", "")
        MRhinoView.EnableDrawing(False)
        Dim views() As MRhinoView = Nothing
        Dim currentView As MRhinoView = RhUtil.RhinoApp.ActiveView
        If Not RhUtil.RhinoApp.ActiveDoc Is Nothing Then
            RhUtil.RhinoApp.ActiveDoc.GetViewList(views)
            For Each view As MRhinoView In views
                RhUtil.RhinoApp.SetActiveView(view)
                RhUtil.RhinoApp.RunScript(command, 0)
            Next
        End If
        RhUtil.RhinoApp.SetActiveView(currentView)
        MRhinoView.EnableDrawing(True)
    End Sub

#End Region


#Region " Gestione Piani di costruzione "


    ''' <summary>
    ''' Crea una copia dell'oggetto Rhino On3dmConstructionPlane e gli assegna una nuova copia di OnPlane
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>Questo serve per poter ripristinare il piano di costruzione senza problemi</remarks>
    Public Shared Function BackupConstructionPlane() As On3dmConstructionPlane
        Dim result As On3dmConstructionPlane = New On3dmConstructionPlane(RhUtil.RhinoApp().ActiveView.MainViewport.ConstructionPlane)
        Dim onPlaneBackup As OnPlane = New OnPlane(result.m_plane)
        result.m_plane = onPlaneBackup
        'onPlaneBackup.Dispose()
        Return result
    End Function


    ''' <summary>
    ''' Ripristino e Dispose()
    ''' </summary>
    ''' <param name="cPlane">Piano di cui verrà fatto il dispose perchè si presuppone non più utile</param>
    ''' <remarks></remarks>
    Public Shared Sub RestoreConstructionPlane(ByVal cPlane As On3dmConstructionPlane)
        If cPlane Is Nothing Then Exit Sub
        Try
            RhUtil.RhinoApp.ActiveView.MainViewport.PushConstructionPlane(cPlane)
        Catch ex As Exception
        End Try
    End Sub


    Public Shared Sub SetNewConstructionPlane(ByVal plane As IOnPlane)
        Dim cPlaneNew As New On3dmConstructionPlane(RhUtil.RhinoApp.ActiveView.MainViewport.ConstructionPlane)
        Dim planeNew As New OnPlane(plane)
        cPlaneNew.m_plane = planeNew
        RhUtil.RhinoApp.ActiveView.MainViewport.PushConstructionPlane(cPlaneNew)
        cPlaneNew.Dispose()
        planeNew.Dispose()
    End Sub


#End Region


End Class
