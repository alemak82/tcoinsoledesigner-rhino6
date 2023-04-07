Imports RMA.Rhino
Imports RMA.OpenNURBS
Imports System.Windows.Forms
Imports InsoleDesigner.bll
Imports InsoleDesigner.IdPlugIn
Imports InsoleDesigner.bll.IdElement3dManager
Imports InsoleDesigner.bll.IdAlias



'*************************************************************
'*** Classe che gestisce il pannello principale del PlugIn ***
'*************************************************************

Public Class ctlMainPanel


#Region " Fields & loading "

    Private Sub ctlMainPanel_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
#If DEBUG Then
        AddDebugButton()
#End If
    End Sub

#End Region


#Region " Common Plugin Functions "

    Public Shared Function ID() As Guid
        Return New Guid("{B0C19F55-B6C1-48cb-BA87-D83AB4E58C01}")
    End Function

#End Region


#Region " Panel UI"

    Public Sub EnableAllButton(ByVal enable As Boolean, Optional ByVal exceptRuler As Boolean = True)
        For Each control As Control In Me.Controls
            control.Enabled = enable
#If DEBUG Then
            If control.Name = "btnDebug" Or control.Name = "rbnFullDebug" Or control.Name = "testCmdDebug" Or control.Name = "rbnPressureMap" Then control.Enabled = True
#End If
        Next
        'Il righello deve essere sempre abilitato a meno che richiesto esplicitamente
        btnRuler.Enabled = exceptRuler
        'Il bottone impostazioni deve essere sempre abilitato
        btnSettings.Enabled = True
    End Sub

    ''' <summary>
    ''' Se l'utente inizia un comando e poi annulla, tenta di ripristinare lo stato precedente dei bottoni in base agli elementi presenti
    ''' </summary>
    ''' <remarks>Se l'utente cancella alcuni elementi manualmente non può funzionare</remarks>
    Private Sub AutoSetPanelView()
        '#If DEBUG Then
        '        EnableAllButton(True)
        '#Else
        SetPanelView(IdPlugIn.ePluginPhase.start)
        If Element3dManager.ObjectExist(eReferences.lastLateralSurface) Or Element3dManager.ObjectExist(eReferences.footMesh) Then
            SetPanelView(IdPlugIn.ePluginPhase.importDone)
        End If
        If Element3dManager.ObjectExist(eReferences.lastLateralSurface) And Element3dManager.FootCurvesExists() Then
            SetPanelView(IdPlugIn.ePluginPhase.footCurvesDone)
        End If
        If Element3dManager.ObjectExist(eReferences.finalUpperCurve) Then
            SetPanelView(IdPlugIn.ePluginPhase.insoleUpperCurveDone)
        End If
        'ATTENZIONE: questo controllo è strettamente legato alla nuova procedura
        If Element3dManager.ObjectExist(eReferences.sweepCuttingSurface) Then
            SetPanelView(ePluginPhase.insoleGuidesDone)
        End If
        If Element3dManager.ObjectExist(eReferences.insoleTopSurface) Then
            SetPanelView(IdPlugIn.ePluginPhase.surfacesInsoleDone)
        End If
        If Element3dManager.AddictionsExist Then
            SetPanelView(IdPlugIn.ePluginPhase.addictionsDone)
        End If
        '#End If
    End Sub


    ''' <summary>
    ''' Verifica se esiste almeno una forma
    ''' </summary>
    ''' <remarks>Non considero il piede perchè potrebbe essere caricato dal nuovo scanner</remarks>
    Private Function EnableLoadLD() As Boolean
        Return True
        'If ObjectByName(LD_LAST_UPPER_RIGHT) IsNot Nothing And ObjectByName(LD_LAST_SOLE_RIGHT) IsNot Nothing Then Return True
        'If ObjectByName(LD_LAST_UPPER_LEFT) IsNot Nothing And ObjectByName(LD_LAST_SOLE_LEFT) IsNot Nothing Then Return True
        'Return False
    End Function

    Private Function EnableProjectPressure() As Boolean
        Return Element3dManager().ModelPressureMap.NumericMapExist() And Element3dManager().ObjectExist(eReferences.footMesh)
    End Function

    Public Sub SetPanelView(ByVal currentPhase As ePluginPhase)
        '#If DEBUG Then
        '        EnableAllButton(True)
        '#Else
        'Righello e profili_TCO sempre abilitati
        btnImportTcoProfile.Enabled = True
        btnRuler.Enabled = True
        Select Case currentPhase
            Case ePluginPhase.start
                btnImport.Enabled = True
                btnLoadLD.Enabled = EnableLoadLD()
                btnImportPressureMap.Enabled = True
                btnProjectPressure.Enabled = EnableProjectPressure()
                btnOpenTemplate.Enabled = True
                btnAlign.Enabled = False
                btnDrawFootCurves.Enabled = False
                btnDrawUpperCurve.Enabled = False
                btnCreateInsoleGuides.Enabled = False
                btnRebuildHeel.Enabled = False
                btnCreateInsole.Enabled = False
                btnDeformInsoleByPressure.Enabled = False
                btnSaveTemplate.Enabled = False
                btnAddAddiction.Enabled = False
                btnRemoveAddiction.Enabled = False
                btnPianiInclinati.Enabled = False
                btnJoinInsole.Enabled = False
                btnSottopiedi.Enabled = True
                chkLeftCurve.Enabled = True
                chkRightCurve.Enabled = True
            Case ePluginPhase.importDone
                btnImport.Enabled = True
                btnLoadLD.Enabled = EnableLoadLD()
                btnImportPressureMap.Enabled = True
                btnProjectPressure.Enabled = EnableProjectPressure()
                btnOpenTemplate.Enabled = True
                btnAlign.Enabled = True
                btnDrawFootCurves.Enabled = True
                btnDrawUpperCurve.Enabled = True
                btnCreateInsoleGuides.Enabled = False
                btnRebuildHeel.Enabled = False
                btnCreateInsole.Enabled = False
                btnDeformInsoleByPressure.Enabled = False
                btnSaveTemplate.Enabled = False
                btnAddAddiction.Enabled = False
                btnRemoveAddiction.Enabled = False
                btnPianiInclinati.Enabled = False
                btnJoinInsole.Enabled = False
                btnSottopiedi.Enabled = True
                chkLeftCurve.Enabled = True
                chkRightCurve.Enabled = True
            Case ePluginPhase.footCurvesDone
                SetPanelView(ePluginPhase.importDone)
                If Element3dManager.FootCurvesExists(eSide.left) Then chkLeftCurve.Checked = True
                If Element3dManager.FootCurvesExists(eSide.right) Then chkRightCurve.Checked = True
            Case ePluginPhase.insoleUpperCurveDone
                btnImport.Enabled = True
                btnLoadLD.Enabled = EnableLoadLD()
                btnImportPressureMap.Enabled = True
                btnProjectPressure.Enabled = EnableProjectPressure()
                btnOpenTemplate.Enabled = True
                btnAlign.Enabled = False
                btnDrawFootCurves.Enabled = True
                btnDrawUpperCurve.Enabled = True
                btnCreateInsoleGuides.Enabled = True
                btnRebuildHeel.Enabled = False
                btnCreateInsole.Enabled = False
                btnDeformInsoleByPressure.Enabled = False
                btnSaveTemplate.Enabled = False
                btnAddAddiction.Enabled = False
                btnRemoveAddiction.Enabled = False
                btnPianiInclinati.Enabled = False
                btnJoinInsole.Enabled = False
                btnSottopiedi.Enabled = True
                chkLeftCurve.Enabled = True
                chkRightCurve.Enabled = True
            Case ePluginPhase.insoleGuidesDone
                btnImport.Enabled = True
                btnLoadLD.Enabled = EnableLoadLD()
                btnImportPressureMap.Enabled = True
                btnProjectPressure.Enabled = EnableProjectPressure()
                btnOpenTemplate.Enabled = True
                btnAlign.Enabled = False
                btnDrawFootCurves.Enabled = True
                btnDrawUpperCurve.Enabled = True
                btnCreateInsoleGuides.Enabled = False
                btnRebuildHeel.Enabled = True
                btnCreateInsole.Enabled = True
                btnDeformInsoleByPressure.Enabled = False
                btnSaveTemplate.Enabled = False
                btnAddAddiction.Enabled = False
                btnRemoveAddiction.Enabled = False
                btnPianiInclinati.Enabled = False
                btnJoinInsole.Enabled = False
                btnSottopiedi.Enabled = True
                chkLeftCurve.Enabled = True
                chkRightCurve.Enabled = True
            Case ePluginPhase.surfacesInsoleDone
                btnImport.Enabled = True
                btnLoadLD.Enabled = EnableLoadLD()
                btnImportPressureMap.Enabled = True
                btnProjectPressure.Enabled = EnableProjectPressure()
                btnOpenTemplate.Enabled = True
                btnAlign.Enabled = False
                btnDrawFootCurves.Enabled = False
                btnDrawUpperCurve.Enabled = False
                btnCreateInsoleGuides.Enabled = False
                btnRebuildHeel.Enabled = False
                btnCreateInsole.Enabled = False
                btnDeformInsoleByPressure.Enabled = Element3dManager().ModelPressureMap.NumericMapExist()
                btnSaveTemplate.Enabled = True
                btnAddAddiction.Enabled = True
                btnRemoveAddiction.Enabled = False
                btnPianiInclinati.Enabled = True
                btnJoinInsole.Enabled = True
                btnSottopiedi.Enabled = True
                chkLeftCurve.Enabled = True
                chkRightCurve.Enabled = True
            Case ePluginPhase.addictionsDone
                btnImport.Enabled = True
                btnLoadLD.Enabled = EnableLoadLD()
                btnImportPressureMap.Enabled = True
                btnProjectPressure.Enabled = EnableProjectPressure()
                btnOpenTemplate.Enabled = True
                btnAlign.Enabled = False
                btnDrawFootCurves.Enabled = False
                btnDrawUpperCurve.Enabled = False
                btnCreateInsoleGuides.Enabled = False
                btnRebuildHeel.Enabled = False
                btnCreateInsole.Enabled = False
                btnDeformInsoleByPressure.Enabled = False
                btnSaveTemplate.Enabled = True
                btnAddAddiction.Enabled = True
                btnRemoveAddiction.Enabled = True
                btnPianiInclinati.Enabled = True
                btnJoinInsole.Enabled = True
                btnSottopiedi.Enabled = True
                chkLeftCurve.Enabled = True
                chkRightCurve.Enabled = True
            Case ePluginPhase.finalInsoleDone
                btnImport.Enabled = True
                btnLoadLD.Enabled = EnableLoadLD()
                btnImportPressureMap.Enabled = True
                btnProjectPressure.Enabled = EnableProjectPressure()
                btnOpenTemplate.Enabled = True
                btnAlign.Enabled = False
                btnDrawFootCurves.Enabled = False
                btnDrawUpperCurve.Enabled = False
                btnCreateInsoleGuides.Enabled = False
                btnRebuildHeel.Enabled = False
                btnCreateInsole.Enabled = False
                btnDeformInsoleByPressure.Enabled = False
                btnSaveTemplate.Enabled = False
                btnAddAddiction.Enabled = False
                btnRemoveAddiction.Enabled = False
                btnPianiInclinati.Enabled = False
                btnJoinInsole.Enabled = False
                btnSottopiedi.Enabled = True
                chkLeftCurve.Enabled = True
                chkRightCurve.Enabled = True
            Case ePluginPhase.autoSetting
                AutoSetPanelView()
        End Select
        '#End If
    End Sub


    ''' <summary>
    ''' Mostra o nasconde le curve di cosruzione del plantare
    ''' </summary>
    Public Sub CurveVisible(ByVal side As eSide, ByVal visible As Boolean)
        Element3dManager.ShowFootCurve(visible, side)
        Element3dManager.ShowFinalUpperCurve(visible, side)
        If side = eSide.left Then
            If visible Then
                chkLeftCurve.Image = My.Resources.Show
            Else
                chkLeftCurve.Image = My.Resources.Hide
            End If
            chkLeftCurve.Checked = visible
        Else
            If visible Then
                chkRightCurve.Image = My.Resources.Show
            Else
                chkRightCurve.Image = My.Resources.Hide
            End If
            chkRightCurve.Checked = visible
        End If
        RhUtil.RhinoApp().ActiveDoc.Redraw()
    End Sub


#End Region


#Region " Eventi "



    Friend Sub btnImport_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnImport.Click
        App.RunScript("_IdLoadFiles", 0)
    End Sub

    Friend Sub btnLoadLD_Click(sender As System.Object, e As System.EventArgs) Handles btnLoadLD.Click
        App.RunScript("_IdLoadLastDesigner", 0)
    End Sub

    Private Sub btnOpenTemplate_Click(sender As Object, e As EventArgs) Handles btnOpenTemplate.Click
        App.RunScript("_IdOpenTemplate", 0)
    End Sub

    Friend Sub btnAlign_Click(sender As System.Object, e As System.EventArgs) Handles btnAlign.Click
        App.RunScript("_IdAlign", 0)
    End Sub

    Friend Sub btnImportPressureMap_Click(sender As Object, e As EventArgs) Handles btnImportPressureMap.Click
        App.RunScript("_IdImportPressureMap", 0)
    End Sub

    Friend Sub btnProjectPressure_Click(sender As Object, e As EventArgs) Handles btnProjectPressure.Click
        App.RunScript("_IdProjectPressureMap", 0)
    End Sub

    '' ToDo - SCOMMENTARE
    Private Sub btnDrawFootCurves_Click(sender As System.Object, e As System.EventArgs) Handles btnDrawFootCurves.Click
        ''QUESTO COMANDO DIFFERENTEMENTE DAGLI ALTRI NON VIENE PARAMETRIZZATO DAL FORM CHE SCRIVE IN MySettings.CurveFootLenghtPercent
        'Dim formDrawFootCurves As New FrmFootSectionCurves()
        'formDrawFootCurves.ShowDialog()
        'If formDrawFootCurves.DialogResult = DialogResult.OK Then
        '    App.RunScript("_IdFootSectionCurves", 0)
        'End If
    End Sub

    Friend Sub btnRuler_Click(sender As System.Object, e As System.EventArgs) Handles btnRuler.Click
        App.RunScript("_IdAddRuler", 0)
    End Sub

    Friend Sub btnProfile_Click(sender As System.Object, e As System.EventArgs) Handles btnImportTcoProfile.Click
        App.RunScript("_IdAddTcoProfile", 0)
    End Sub

    Friend Sub btnDrawUpperCurve_Click(sender As System.Object, e As System.EventArgs) Handles btnDrawUpperCurve.Click
        App.RunScript("_IdDrawUpperCurves", 0)
    End Sub

    Friend Sub btnCreateInsoleGuides_Click(sender As System.Object, e As System.EventArgs) Handles btnCreateInsoleGuides.Click
        App.RunScript("_IdCreateInsoleGuides", 0)
    End Sub

    Friend Sub btnRebuildHeel_Click(sender As System.Object, e As System.EventArgs) Handles btnRebuildHeel.Click
        App.RunScript("_IdRebuildHeel", 0)
    End Sub

    Friend Sub btnMakeInsole_Click(sender As System.Object, e As System.EventArgs) Handles btnCreateInsole.Click
        App.RunScript("_IdCreateInsole", 0)
    End Sub

    '' ToDo - SCOMMENTARE
    Friend Sub btnDeformInsoleByPressure_Click(sender As Object, e As EventArgs) Handles btnDeformInsoleByPressure.Click
        'Dim formDeformInsoleByPressure As New FrmDeformInsoleByPressure
        'formDeformInsoleByPressure.ShowDialog()
        'If formDeformInsoleByPressure.DialogResult = DialogResult.OK Then
        '    App.RunScript("_IdDeformInsoleByPressure", 0)
        'End If
    End Sub

    Private Sub btnSaveTemplate_Click(sender As Object, e As EventArgs) Handles btnSaveTemplate.Click
        App.RunScript("_IdSaveTemplate", 0)
    End Sub

    Friend Sub btnAddAddiction_Click(sender As System.Object, e As System.EventArgs) Handles btnAddAddiction.Click
        App.RunScript("_IdAddAddiction", 0)
    End Sub

    Friend Sub btnRemoveAddiction_Click(sender As System.Object, e As System.EventArgs) Handles btnRemoveAddiction.Click
        App.RunScript("_IdRemoveLastAddiction", 0)
    End Sub

    Friend Sub btnJoinInsole_Click(sender As System.Object, e As System.EventArgs) Handles btnJoinInsole.Click
        App.RunScript("_IdJoinInsole", 0)
    End Sub

    Friend Sub btnSottopiedi_Click(sender As System.Object, e As System.EventArgs) Handles btnSottopiedi.Click
        App.RunScript("_IdSottopiede", 0)
    End Sub

    Friend Sub btnPianiInclinati_Click(sender As Object, e As EventArgs) Handles btnPianiInclinati.Click
        App.RunScript("_IdWedge", 0)
    End Sub

    Public Sub chkShowCurve_Click(sender As System.Object, e As System.EventArgs) Handles chkLeftCurve.Click
        CurveVisible(eSide.left, DirectCast(sender, CheckBox).Checked)
    End Sub

    Public Sub chkRightCurve_Click(sender As System.Object, e As System.EventArgs) Handles chkRightCurve.Click
        CurveVisible(eSide.right, DirectCast(sender, CheckBox).Checked)
    End Sub

    ''' <summary>
    ''' Do il focus per mostrare il ToolTip
    ''' </summary>
    Private Sub ctlMainPanel_MouseEnter(sender As System.Object, e As System.EventArgs) Handles MyBase.MouseEnter
        Me.Focus()
    End Sub

    '' ToDo - SCOMMENTARE
    Private Sub btnSettings_Click(sender As Object, e As EventArgs) Handles btnSettings.Click
'        If Not IdBaseCommand.StartCommandCheck() Then Exit Sub

'        Dim frmSettingPlugin As New FrmSettingPlugin
'        If frmSettingPlugin.ShowDialog() <> DialogResult.OK Then Exit Sub
'        'Salvo lo stato dei bottoni
'        Dim controlState As New Dictionary(Of String, Boolean)
'        For Each control As Control In Me.Controls
'            controlState.Add(control.Name, control.Enabled)
'        Next
'        'Aggiorno pannello principale
'        Me.Controls.Clear()
'        Me.InitializeComponent()
'#If DEBUG Then
'        AddDebugButton()
'#End If
'        'Ripristino lo stato dei bottoni
'        For Each control As Control In Me.Controls
'            If controlState.ContainsKey(control.Name) Then control.Enabled = controlState.Item(control.Name)
'        Next
    End Sub


#End Region


#Region " DEBUG "

    Dim btnDebug As New Button()
    Dim rbnFullDebug As New RadioButton
    Dim rbnCmdDebug As New RadioButton

    Private Sub AddDebugButton()
        btnDebug.Name = "btnDebug"
        btnDebug.Text = "DEBUG"
        btnDebug.Tag = "DEBUG"
        btnDebug.Location = New Drawing.Point(5, chkLeftCurve.Location.Y + btnDebug.Height)
        AddHandler btnDebug.MouseClick, AddressOf DEBUG_Click
        Me.Controls.Add(btnDebug)

        rbnFullDebug.Name = "rbnFullDebug"
        rbnFullDebug.Text = "FullDebug"
        rbnFullDebug.Location = New Drawing.Point(0, btnDebug.Location.Y + rbnFullDebug.Height)
        'rbnFullDebug.Checked = True
        Me.Controls.Add(rbnFullDebug)

        rbnCmdDebug.Name = "testCmdDebug"
        rbnCmdDebug.Text = "CmdDebug"
        rbnCmdDebug.Location = New Drawing.Point(rbnFullDebug.Location.X + rbnFullDebug.Width, rbnFullDebug.Location.Y)
        rbnCmdDebug.Checked = True
        Me.Controls.Add(rbnCmdDebug)
    End Sub


    Private Sub DEBUG_Click(sender As System.Object, e As System.EventArgs)
#If DEBUG Then
        App.ActiveDoc.EnableUndoRecording(True)
        If rbnCmdDebug.Checked Then
            IdTestMacchioneCmd.Esegui(sender, e)
        ElseIf rbnFullDebug.Checked Then
            IdTestMacchioneCmd.FullDebug(IdPlugIn.ThisPlugIn(), sender, e)
        End If
#End If
    End Sub



#End Region




End Class

