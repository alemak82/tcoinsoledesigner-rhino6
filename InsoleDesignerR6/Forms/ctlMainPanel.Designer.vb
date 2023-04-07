<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ctlMainPanel
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlMainPanel))
        Me.toolTip = New System.Windows.Forms.ToolTip(Me.components)
        Me.btnOpenTemplate = New System.Windows.Forms.Button()
        Me.btnSaveTemplate = New System.Windows.Forms.Button()
        Me.btnSettings = New System.Windows.Forms.Button()
        Me.btnImportTcoProfile = New System.Windows.Forms.Button()
        Me.btnRebuildHeel = New System.Windows.Forms.Button()
        Me.btnSottopiedi = New System.Windows.Forms.Button()
        Me.btnRuler = New System.Windows.Forms.Button()
        Me.btnJoinInsole = New System.Windows.Forms.Button()
        Me.btnRemoveAddiction = New System.Windows.Forms.Button()
        Me.btnAddAddiction = New System.Windows.Forms.Button()
        Me.btnLoadLD = New System.Windows.Forms.Button()
        Me.btnCreateInsoleGuides = New System.Windows.Forms.Button()
        Me.btnCreateInsole = New System.Windows.Forms.Button()
        Me.btnDrawUpperCurve = New System.Windows.Forms.Button()
        Me.btnDrawFootCurves = New System.Windows.Forms.Button()
        Me.btnAlign = New System.Windows.Forms.Button()
        Me.btnImport = New System.Windows.Forms.Button()
        Me.chkRightCurve = New System.Windows.Forms.CheckBox()
        Me.chkLeftCurve = New System.Windows.Forms.CheckBox()
        Me.btnImportPressureMap = New System.Windows.Forms.Button()
        Me.btnProjectPressure = New System.Windows.Forms.Button()
        Me.btnDeformInsoleByPressure = New System.Windows.Forms.Button()
        Me.btnPianiInclinati = New System.Windows.Forms.Button()
        Me.SuspendLayout
        '
        'btnOpenTemplate
        '
        resources.ApplyResources(Me.btnOpenTemplate, "btnOpenTemplate")
        Me.btnOpenTemplate.BackgroundImage = Global.InsoleDesigner.My.Resources.Resources.Database_read
        Me.btnOpenTemplate.Name = "btnOpenTemplate"
        Me.toolTip.SetToolTip(Me.btnOpenTemplate, resources.GetString("btnOpenTemplate.ToolTip"))
        Me.btnOpenTemplate.UseVisualStyleBackColor = true
        '
        'btnSaveTemplate
        '
        resources.ApplyResources(Me.btnSaveTemplate, "btnSaveTemplate")
        Me.btnSaveTemplate.BackgroundImage = Global.InsoleDesigner.My.Resources.Resources.Database_save
        Me.btnSaveTemplate.Name = "btnSaveTemplate"
        Me.toolTip.SetToolTip(Me.btnSaveTemplate, resources.GetString("btnSaveTemplate.ToolTip"))
        Me.btnSaveTemplate.UseVisualStyleBackColor = true
        '
        'btnSettings
        '
        resources.ApplyResources(Me.btnSettings, "btnSettings")
        Me.btnSettings.BackgroundImage = Global.InsoleDesigner.My.Resources.Resources.Settings
        Me.btnSettings.Name = "btnSettings"
        Me.toolTip.SetToolTip(Me.btnSettings, resources.GetString("btnSettings.ToolTip"))
        Me.btnSettings.UseVisualStyleBackColor = true
        '
        'btnImportTcoProfile
        '
        Me.btnImportTcoProfile.BackgroundImage = Global.InsoleDesigner.My.Resources.Resources.ProfileTCO
        resources.ApplyResources(Me.btnImportTcoProfile, "btnImportTcoProfile")
        Me.btnImportTcoProfile.Name = "btnImportTcoProfile"
        Me.toolTip.SetToolTip(Me.btnImportTcoProfile, resources.GetString("btnImportTcoProfile.ToolTip"))
        Me.btnImportTcoProfile.UseVisualStyleBackColor = true
        '
        'btnRebuildHeel
        '
        Me.btnRebuildHeel.BackgroundImage = Global.InsoleDesigner.My.Resources.Resources.ric_tallone
        resources.ApplyResources(Me.btnRebuildHeel, "btnRebuildHeel")
        Me.btnRebuildHeel.Name = "btnRebuildHeel"
        Me.toolTip.SetToolTip(Me.btnRebuildHeel, resources.GetString("btnRebuildHeel.ToolTip"))
        Me.btnRebuildHeel.UseVisualStyleBackColor = true
        '
        'btnSottopiedi
        '
        Me.btnSottopiedi.BackgroundImage = Global.InsoleDesigner.My.Resources.Resources.Sottopiede
        resources.ApplyResources(Me.btnSottopiedi, "btnSottopiedi")
        Me.btnSottopiedi.Name = "btnSottopiedi"
        Me.toolTip.SetToolTip(Me.btnSottopiedi, resources.GetString("btnSottopiedi.ToolTip"))
        Me.btnSottopiedi.UseVisualStyleBackColor = true
        '
        'btnRuler
        '
        Me.btnRuler.BackgroundImage = Global.InsoleDesigner.My.Resources.Resources.Ruler
        resources.ApplyResources(Me.btnRuler, "btnRuler")
        Me.btnRuler.Name = "btnRuler"
        Me.toolTip.SetToolTip(Me.btnRuler, resources.GetString("btnRuler.ToolTip"))
        Me.btnRuler.UseVisualStyleBackColor = true
        '
        'btnJoinInsole
        '
        Me.btnJoinInsole.BackgroundImage = Global.InsoleDesigner.My.Resources.Resources.Fine
        resources.ApplyResources(Me.btnJoinInsole, "btnJoinInsole")
        Me.btnJoinInsole.Name = "btnJoinInsole"
        Me.toolTip.SetToolTip(Me.btnJoinInsole, resources.GetString("btnJoinInsole.ToolTip"))
        Me.btnJoinInsole.UseVisualStyleBackColor = true
        '
        'btnRemoveAddiction
        '
        Me.btnRemoveAddiction.BackgroundImage = Global.InsoleDesigner.My.Resources.Resources.ScarichiMinus
        resources.ApplyResources(Me.btnRemoveAddiction, "btnRemoveAddiction")
        Me.btnRemoveAddiction.Name = "btnRemoveAddiction"
        Me.toolTip.SetToolTip(Me.btnRemoveAddiction, resources.GetString("btnRemoveAddiction.ToolTip"))
        Me.btnRemoveAddiction.UseVisualStyleBackColor = true
        '
        'btnAddAddiction
        '
        Me.btnAddAddiction.BackgroundImage = Global.InsoleDesigner.My.Resources.Resources.ScarichiPlus
        resources.ApplyResources(Me.btnAddAddiction, "btnAddAddiction")
        Me.btnAddAddiction.Name = "btnAddAddiction"
        Me.toolTip.SetToolTip(Me.btnAddAddiction, resources.GetString("btnAddAddiction.ToolTip"))
        Me.btnAddAddiction.UseVisualStyleBackColor = true
        '
        'btnLoadLD
        '
        resources.ApplyResources(Me.btnLoadLD, "btnLoadLD")
        Me.btnLoadLD.BackgroundImage = Global.InsoleDesigner.My.Resources.Resources.LoadLD
        Me.btnLoadLD.Name = "btnLoadLD"
        Me.toolTip.SetToolTip(Me.btnLoadLD, resources.GetString("btnLoadLD.ToolTip"))
        Me.btnLoadLD.UseVisualStyleBackColor = true
        '
        'btnCreateInsoleGuides
        '
        Me.btnCreateInsoleGuides.BackgroundImage = Global.InsoleDesigner.My.Resources.Resources.GuidePlantare
        resources.ApplyResources(Me.btnCreateInsoleGuides, "btnCreateInsoleGuides")
        Me.btnCreateInsoleGuides.Name = "btnCreateInsoleGuides"
        Me.toolTip.SetToolTip(Me.btnCreateInsoleGuides, resources.GetString("btnCreateInsoleGuides.ToolTip"))
        Me.btnCreateInsoleGuides.UseVisualStyleBackColor = true
        '
        'btnCreateInsole
        '
        Me.btnCreateInsole.BackgroundImage = Global.InsoleDesigner.My.Resources.Resources.FinePlantare
        resources.ApplyResources(Me.btnCreateInsole, "btnCreateInsole")
        Me.btnCreateInsole.Name = "btnCreateInsole"
        Me.toolTip.SetToolTip(Me.btnCreateInsole, resources.GetString("btnCreateInsole.ToolTip"))
        Me.btnCreateInsole.UseVisualStyleBackColor = true
        '
        'btnDrawUpperCurve
        '
        Me.btnDrawUpperCurve.BackgroundImage = Global.InsoleDesigner.My.Resources.Resources.CurvePlantare
        resources.ApplyResources(Me.btnDrawUpperCurve, "btnDrawUpperCurve")
        Me.btnDrawUpperCurve.Name = "btnDrawUpperCurve"
        Me.toolTip.SetToolTip(Me.btnDrawUpperCurve, resources.GetString("btnDrawUpperCurve.ToolTip"))
        Me.btnDrawUpperCurve.UseVisualStyleBackColor = true
        '
        'btnDrawFootCurves
        '
        Me.btnDrawFootCurves.BackgroundImage = Global.InsoleDesigner.My.Resources.Resources.CurvePiede
        resources.ApplyResources(Me.btnDrawFootCurves, "btnDrawFootCurves")
        Me.btnDrawFootCurves.Name = "btnDrawFootCurves"
        Me.toolTip.SetToolTip(Me.btnDrawFootCurves, resources.GetString("btnDrawFootCurves.ToolTip"))
        Me.btnDrawFootCurves.UseVisualStyleBackColor = true
        '
        'btnAlign
        '
        Me.btnAlign.BackgroundImage = Global.InsoleDesigner.My.Resources.Resources.Allinea
        resources.ApplyResources(Me.btnAlign, "btnAlign")
        Me.btnAlign.Name = "btnAlign"
        Me.toolTip.SetToolTip(Me.btnAlign, resources.GetString("btnAlign.ToolTip"))
        Me.btnAlign.UseVisualStyleBackColor = true
        '
        'btnImport
        '
        resources.ApplyResources(Me.btnImport, "btnImport")
        Me.btnImport.BackgroundImage = Global.InsoleDesigner.My.Resources.Resources.Import
        Me.btnImport.Name = "btnImport"
        Me.toolTip.SetToolTip(Me.btnImport, resources.GetString("btnImport.ToolTip"))
        Me.btnImport.UseVisualStyleBackColor = true
        '
        'chkRightCurve
        '
        resources.ApplyResources(Me.chkRightCurve, "chkRightCurve")
        Me.chkRightCurve.BackColor = System.Drawing.SystemColors.Control
        Me.chkRightCurve.Image = Global.InsoleDesigner.My.Resources.Resources.Show
        Me.chkRightCurve.Name = "chkRightCurve"
        Me.toolTip.SetToolTip(Me.chkRightCurve, resources.GetString("chkRightCurve.ToolTip"))
        Me.chkRightCurve.UseVisualStyleBackColor = false
        '
        'chkLeftCurve
        '
        resources.ApplyResources(Me.chkLeftCurve, "chkLeftCurve")
        Me.chkLeftCurve.BackColor = System.Drawing.SystemColors.Control
        Me.chkLeftCurve.Image = Global.InsoleDesigner.My.Resources.Resources.Show
        Me.chkLeftCurve.Name = "chkLeftCurve"
        Me.toolTip.SetToolTip(Me.chkLeftCurve, resources.GetString("chkLeftCurve.ToolTip"))
        Me.chkLeftCurve.UseVisualStyleBackColor = false
        '
        'btnImportPressureMap
        '
        Me.btnImportPressureMap.BackgroundImage = Global.InsoleDesigner.My.Resources.Resources.PressureMap
        resources.ApplyResources(Me.btnImportPressureMap, "btnImportPressureMap")
        Me.btnImportPressureMap.Name = "btnImportPressureMap"
        Me.toolTip.SetToolTip(Me.btnImportPressureMap, resources.GetString("btnImportPressureMap.ToolTip"))
        Me.btnImportPressureMap.UseVisualStyleBackColor = true
        '
        'btnProjectPressure
        '
        Me.btnProjectPressure.BackgroundImage = Global.InsoleDesigner.My.Resources.Resources.ProjectPressure
        resources.ApplyResources(Me.btnProjectPressure, "btnProjectPressure")
        Me.btnProjectPressure.Name = "btnProjectPressure"
        Me.toolTip.SetToolTip(Me.btnProjectPressure, resources.GetString("btnProjectPressure.ToolTip"))
        Me.btnProjectPressure.UseVisualStyleBackColor = true
        '
        'btnDeformInsoleByPressure
        '
        Me.btnDeformInsoleByPressure.BackgroundImage = Global.InsoleDesigner.My.Resources.Resources.DeformInsoleByPressure
        resources.ApplyResources(Me.btnDeformInsoleByPressure, "btnDeformInsoleByPressure")
        Me.btnDeformInsoleByPressure.Name = "btnDeformInsoleByPressure"
        Me.toolTip.SetToolTip(Me.btnDeformInsoleByPressure, resources.GetString("btnDeformInsoleByPressure.ToolTip"))
        Me.btnDeformInsoleByPressure.UseVisualStyleBackColor = true
        '
        'btnPianiInclinati
        '
        resources.ApplyResources(Me.btnPianiInclinati, "btnPianiInclinati")
        Me.btnPianiInclinati.BackgroundImage = Global.InsoleDesigner.My.Resources.Resources.PianoInclinato
        Me.btnPianiInclinati.Name = "btnPianiInclinati"
        Me.toolTip.SetToolTip(Me.btnPianiInclinati, resources.GetString("btnPianiInclinati.ToolTip"))
        Me.btnPianiInclinati.UseVisualStyleBackColor = true
        '
        'ctlMainPanel
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.btnPianiInclinati)
        Me.Controls.Add(Me.btnDeformInsoleByPressure)
        Me.Controls.Add(Me.btnProjectPressure)
        Me.Controls.Add(Me.btnImportPressureMap)
        Me.Controls.Add(Me.btnSettings)
        Me.Controls.Add(Me.btnSaveTemplate)
        Me.Controls.Add(Me.btnOpenTemplate)
        Me.Controls.Add(Me.btnImportTcoProfile)
        Me.Controls.Add(Me.btnRebuildHeel)
        Me.Controls.Add(Me.btnSottopiedi)
        Me.Controls.Add(Me.btnRuler)
        Me.Controls.Add(Me.btnJoinInsole)
        Me.Controls.Add(Me.btnRemoveAddiction)
        Me.Controls.Add(Me.btnAddAddiction)
        Me.Controls.Add(Me.btnLoadLD)
        Me.Controls.Add(Me.chkRightCurve)
        Me.Controls.Add(Me.chkLeftCurve)
        Me.Controls.Add(Me.btnCreateInsoleGuides)
        Me.Controls.Add(Me.btnCreateInsole)
        Me.Controls.Add(Me.btnDrawUpperCurve)
        Me.Controls.Add(Me.btnDrawFootCurves)
        Me.Controls.Add(Me.btnAlign)
        Me.Controls.Add(Me.btnImport)
        Me.Name = "ctlMainPanel"
        Me.ResumeLayout(false)
        Me.PerformLayout

End Sub
    Friend WithEvents btnImport As System.Windows.Forms.Button
    Friend WithEvents btnAlign As System.Windows.Forms.Button
    Friend WithEvents btnDrawFootCurves As System.Windows.Forms.Button
    Friend WithEvents btnDrawUpperCurve As System.Windows.Forms.Button
    Friend WithEvents btnCreateInsole As System.Windows.Forms.Button
    Friend WithEvents btnCreateInsoleGuides As System.Windows.Forms.Button
    Friend WithEvents toolTip As System.Windows.Forms.ToolTip
    Friend WithEvents chkLeftCurve As System.Windows.Forms.CheckBox
    Friend WithEvents chkRightCurve As System.Windows.Forms.CheckBox
    Friend WithEvents btnLoadLD As System.Windows.Forms.Button
    Friend WithEvents btnAddAddiction As System.Windows.Forms.Button
    Friend WithEvents btnRemoveAddiction As System.Windows.Forms.Button
    Friend WithEvents btnJoinInsole As System.Windows.Forms.Button
    Friend WithEvents btnRuler As System.Windows.Forms.Button
    Friend WithEvents btnSottopiedi As System.Windows.Forms.Button
    Friend WithEvents btnRebuildHeel As System.Windows.Forms.Button
    Friend WithEvents btnImportTcoProfile As System.Windows.Forms.Button
    Friend WithEvents btnOpenTemplate As Windows.Forms.Button
    Friend WithEvents btnSaveTemplate As Windows.Forms.Button
    Friend WithEvents btnSettings As Windows.Forms.Button
  Friend WithEvents btnImportPressureMap As Windows.Forms.Button
  Friend WithEvents btnProjectPressure As Windows.Forms.Button
  Friend WithEvents btnDeformInsoleByPressure As Windows.Forms.Button
    Friend WithEvents btnPianiInclinati As Windows.Forms.Button
End Class
