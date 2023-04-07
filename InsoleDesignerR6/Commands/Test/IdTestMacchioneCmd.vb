#If DEBUG Then


Imports System.Drawing
Imports RMA.Rhino
Imports RMA.OpenNURBS
Imports RhinoUtils
Imports Rhino.UI
Imports System.IO
Imports InsoleDesigner.bll.IdAlias
Imports InsoleDesigner.bll.IdLanguageManager
Imports InsoleDesigner.bll.IdElement3dManager
Imports InsoleDesigner.bll.IdGeometryUtils
Imports RhinoUtils.RhViewport
Imports RhinoUtils.RhDocument
Imports RhinoUtils.RhDebug
Imports InsoleDesigner.bll.IdAddiction
Imports InsoleDesigner.bll.IdMetbarAddiction
Imports InsoleDesigner.bll.AbstractPianiInclinati
Imports InsoleDesigner.bll.FactoryPianiInclinati
Imports System.Reflection
Imports InsoleDesigner.bll
Imports InsoleDesigner.IdPlugIn
Imports ORM
Imports Rhino
Imports Rhino.DocObjects
Imports RhinoUtils.RhGeometry
Imports ORM.DataBindingHelper
Imports ORM.IdDataSet
Imports ORM.IdDataSetTableAdapters
Imports Rhino.Geometry
Imports Rhino.Commands

Public Class IdTestMacchioneCmd
    Inherits IdBaseCommand



#Region " CONSTANT "


    Public Const BASE_FILE_TEST_DIR As String = IdPlugIn.BASE_ID_DIR & "file test\"
    Private Const ENGLISH_CMD_NAME As String = "IdTestPluginCmd"

#End Region


#Region " FULL DEBUG "


    Public Enum eFullDebug
        OFS
        TCO
    End Enum

    
    ''' <summary>
    ''' TEST PROCEDURA COMPLETA - COSE DA IMPOSTARE:
    ''' -OFS o TCO
    ''' -File forma
    ''' -File piede(mesh o curve)
    ''' -File curve di profilo e relativi Guid
    ''' -File ricostruzione tallone
    ''' </summary> 
    ''' <remarks></remarks>
    Public Shared Sub FullDebug(ByRef ThisPlugin As IdPlugIn, sender As System.Object, e As System.EventArgs)
        ''PREPARAZIONE
        'Dim type As eFullDebug = eFullDebug.OFS
        'If FormManager.CurrentForm IsNot Nothing Then
        '    DirectCast(FormManager.CurrentForm, System.Windows.Forms.Form).Close()
        'End If
        'RhUtil.RhinoApp.ActiveDoc.Destroy()

        ''IMPORT FORMA-PIEDE----------------------------------------------------------------------------------------------------------
        'ThisPlugin.MainPanel.btnImport_Click(sender, e)
        'Dim frmLoad As frmLoadFiles = DirectCast(FormManager.CurrentForm, frmLoadFiles)

        ''IMPOSTO FORMA 
        'frmLoad.valLeftLast.Text = "" 'BASE_ID_DIR & "Modelli CAD\FILE DEMO - 33607379\33607379Sx.3dm"
        'frmLoad.valRightLast.Text = BASE_FILE_TEST_DIR & "test srf liscia\forma.3dm"
        ''frmLoad.valRightLast.Text = ""

        'If type = eFullDebug.OFS Then
        '    'IMPORTO MESH PIEDE
        '    frmLoad.valLeftFoot.Text = "" 'BASE_FILE_TEST_DIR & "mappe pressione\Pedana Duna Test Valerio\piede_sinistro032018.stl"
        '    frmLoad.valRightFoot.Text = BASE_FILE_TEST_DIR & "test srf liscia\piede.stl"
        '    'frmLoad.valRightFoot.Text = BASE_FILE_TEST_DIR & "mappe pressione\Pedana Duna Test Valerio\piede_destro032018.stl"
        '    frmLoad.OK_Button_Click(sender, e)

        '    ''MODIFICA PERCENTUALE CREAZIONE CURVE
        '    'My.Settings.CurveFootLenghtPercent(eSide.left) = CDec(1)
        '    'My.Settings.CurveFootLenghtPercent(eSide.right) = CDec(1)

        '    'CURVE DI SEZIONE DELLA MESH DEL PIEDE
        '    RhUtil.RhinoApp.RunScript("_IdFootSectionCurves", 0)
        'Else
        '    frmLoad.rbnFootCurve.Checked = True
        '    My.Settings.UseTcoScanner = True
        '    'IMPORTO CURVE PIEDE
        '    frmLoad.valLeftFoot.Text = ""
        '    frmLoad.valRightFoot.Text = BASE_FILE_TEST_DIR & "test srf liscia\Luca_T_omar_45_dx.ply"
        '    frmLoad.OK_Button_Click(sender, e)
        '    Dim frmFixTcoCurves As FrmFixTcoCurves = DirectCast(FormManager.CurrentForm, FrmFixTcoCurves)
        '    frmFixTcoCurves.btnModifyAuto_Click(sender, e)
        '    frmFixTcoCurves.OK_Button_Click(sender, e)
        'End If
        ''--------------------------------------------------------------------------------------------------------------------------------

        ''ALLINEAMENTO PIEDE/CURVE - FORMA
        'ThisPlugin.MainPanel.btnAlign_Click(sender, e)
        'Dim frmAlign As frmAlign = DirectCast(FormManager.CurrentForm, frmAlign)
        'frmAlign.btnAutoAlign_Click(sender, e)
        'frmAlign.btnShift_Click(frmAlign.btnShiftZPlus, e)
        'frmAlign.btnShift_Click(frmAlign.btnShiftZPlus, e)
        'frmAlign.OK_Button_Click(sender, e)

        '''PRESSIONE
        '''IMPORT MAPPA PRESSIONE
        ''ThisPlugin.MainPanel.btnImportPressureMap_Click(sender, e)
        ''Dim frmPressure = DirectCast(FormManager.CurrentForm, FrmImportPressureMap)
        ''frmPressure.txtLeftMatrix.Text = BASE_ID_DIR & "file test\mappe pressione\Pedana Duna Test Valerio\Sinistro.txt"
        ''frmPressure.txtRightMatrix.Text = ""
        ''frmPressure.OK_Button_Click(sender, e)
        '''PROIEZIONE MAPPA
        ''RhUtil.RhinoApp.RunScript("_IdProjectPressureMap", 0)        

        ''CURVE DI PROFILO                
        'ThisPlugin.MainPanel.btnDrawUpperCurve_Click(sender, e)
        'Dim filename As String = BASE_FILE_TEST_DIR & "test srf liscia\curve profilo.3dm"
        'RhUtil.RhinoApp.RunScript("_Import """ & filename & """", 0)
        'RhUtil.RhinoApp.RunScript("_SelNone", 0)
        'Element3dManager.SetRhinoObj(eReferences.userExternalUpperCurve, eSide.right, Guid.Parse("b8100cc4-74f8-48fe-9f71-0e83ab659f89"))
        'Element3dManager.SetRhinoObj(eReferences.userInternalUpperCurve, eSide.right, Guid.Parse("2494efbe-8825-4a6a-8c33-b41a1fe6c7a4"))
        'Dim frmDrawUpperCrv As frmDrawUpperCurves = DirectCast(FormManager.CurrentForm, frmDrawUpperCurves)
        'frmDrawUpperCrv.btnCreateProfile_Click(frmDrawUpperCrv.btnCreateRightProfile, e)
        'frmDrawUpperCrv.OK_Button_Click(sender, e)

        ''CURVE GUIDA PLANTARE
        'ThisPlugin.MainPanel.btnCreateInsoleGuides_Click(sender, e)

        '''Ricostruzione tallone
        ''If type = eFullDebug.OFS Then
        ''  filename = BASE_FILE_TEST_DIR & "test srf liscia\curva ricostruzione tallone.3dm"
        ''  RhUtil.RhinoApp.RunScript("_Import """ & filename & """", 0)
        ''  RhUtil.RhinoApp.RunScript("_SelNone", 0)
        ''  ThisPlugin.MainPanel.btnRebuildHeel_Click(sender, e)
        ''  DirectCast(FormManager.CurrentForm, FrmRebuildHeel).mCmd.Side = eSide.right
        ''  'DEBUG SPECIFICO CON FILE BASE_FILE_TEST_DIR & "test srf liscia\curva ricostruzione tallone.3dm"
        ''  DirectCast(FormManager.CurrentForm, FrmRebuildHeel).mCmd.CurveId = Guid.Parse("064e2995-de02-4989-81ca-7bb841b5a253")
        ''  DirectCast(FormManager.CurrentForm, FrmRebuildHeel).OK_Button_Click(sender, e)
        ''End If


        ''CREAZIONE PLANTARE
        'ThisPlugin.MainPanel.btnMakeInsole_Click(sender, e)

        '''DEFORMAZIONE PLANTARE CON PRESSIONE
        ''ThisPlugin.MainPanel.btnDeformInsoleByPressure_Click(sender, e)

        ''AGGIORNO
        'RhLayer.DeleteEmptyLayers()
        'MRhinoView.EnableDrawing(True)
        'RhLayer.SetAllLayerLock(False)
        'Doc.Regen()
        'RhUtil.RhinoApp.ActiveDoc.Redraw()
        'ThisPlugin.MainPanel.EnableAllButton(True)
    End Sub


#End Region


#Region " COMANDI TEST "

    ''' <summary>
    ''' PER TESTARE UNO O PIU' METODI GENERICI
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Public Shared Sub Esegui(sender As System.Object, e As System.EventArgs)
        App.RunScript(ENGLISH_CMD_NAME, 0)
        'EVENTUALI ALTRI COMANDI      
        '...
    End Sub


    'Protected Overrides Function RunCommand(ByVal doc As RhinoDoc, ByVal mode As RunMode) As Result
    Protected Overrides Function RunCommandNoDoc(mode As RunMode) As Result            
    '    If Not StartCommandCheck() Then Return Result.Cancel
        
    '    IdPlugIn.Instance().MainPanel.EnableAllButton(True)
    '    MRhinoView.EnableDrawing(True)
    '    RhLayer.SetAllLayerLock(False)

    '    Try
    '        RhinoCommonsGeometry()
    '        AddRhLayer()    


    '        'PianoInclinatoTotale()

    '        'PianoInclinatoPianta()

    '        'PianoInclinatoTallone()



    '        'ImportMapPressireErrore()

    '        'DeformaConPressione()

    '        'TestPression()    

    '        'CREAZIONE_SRF_CON_PUNTI_SUL_BORDO_PER_LISCIATURA_SRF_PLANATRE()

    '        'TestNuovaSrfTopDopoTagliMetbar()

    '        'IdPressureMapUtils.PulisciNeroDaBitmap(CType(Bitmap.FromFile(BASE_ID_DIR & "file test\mappe pressione\Pedana Duna\AleDx.bmp"), Bitmap))

    '        'JoinMetBar2()

    '        'TestFileLucaTarabelli()    

    '        'OffsetManualeTest()

    '        'ScarichiTest()

    '        'FilletTest()

    '        'GetPolysurfacePartTest()      

    '        'SplopTest()

    '        'CreazioneTubicinoPerRaccordo()



    '    Catch ex As Exception
    '        IdLanguageManager.PromptError(ex.ToString())
    '        MsgBox("Comando test fallito", MsgBoxStyle.Critical, My.Application.Info.Title)
    '        Return IRhinoCommand.result.failure
    '    Finally
    '        MRhinoView.EnableDrawing(True)
    '        Doc.Regen()
    '        Doc.Redraw()
    '    End Try

        Return Result.Success
    End Function

    
    'Private Sub RhinoCommonsGeometry()
    '    ''USANDO RhinoCommon.dll
    '    'Dim line1 = New Line(New Point3d(0, 0, 0), New Point3d(1, 0, 0))
    '    'Doc2().Objects.AddLine(line1)
    '    ''USANDO Rhino_DotNet.dll
    '    Dim lineCurve = New OnLineCurve(New On3dPoint(0,0,0), New On3dPoint(7,5,0))        
    '    Doc.AddCurveObject(lineCurve.NurbsCurve().DuplicateCurve())                    
    'End Sub

    'Private Sub AddRhLayer() 
    '    ''QUESTO FALLISCE PERCHè IL METODO FindLayer() è STATO RIMOSSO DA Rhino_DotNet.dll
    '    'RhLayer.CreateLayer("aaaa", True, False)

    '    ''USANDO RhinoCommon.dll
    '    Dim aaa = New Layer()
    '    aaa.Name = "provaaaa"
    '    Doc2.Layers.Add(aaa)  


    '     'Dim layerTable As MRhinoLayerTable = RhUtil.RhinoApp().ActiveDoc().m_layer_table
    '    'Dim layerIndex As Integer = layerTable.FindLayer(0,"", False,0)        
      
    '    Doc.m_layer_table.AddLayer()
    'End Sub







    'Private Sub PianoInclinatoTotale()
    '    Doc.Destroy()
    '    RhLayer.DeleteEmptyLayers()
    '    'Dim filename As String = BASE_FILE_TEST_DIR & "piani inclinati\plantareDX.3dm"
    '    Dim filename As String = BASE_FILE_TEST_DIR & "piani inclinati\TEST - Prova7_Luca.3dm"
    '    App.RunScript("_Import """ & filename & """", 0)
    '    App.RunScript("_SelNone", 0)

    '    Dim side = eSide.left  'eSide.right
    '    'Element3dManager.SetRhinoObj(eReferences.insoleBottomSurface, side, New Guid("05ca0da8-3c5a-469e-81bf-8617c62d32b8"))        
    '    Element3dManager.SetRhinoObj(eReferences.insoleBottomSurface, side, New Guid("4e8abd12-f4cf-40cb-ae0a-bb9c1f1f1e82"))
    '    Dim spessore = 10
    '    'Dim pianoInclinato As New PianoInclinatoTotale(ePosizionePianiInclinato.laterale, spessore)        
    '    Dim pianoInclinato As New PianoInclinatoTotale(ePosizionePianiInclinato.mediale, spessore)

    '    ''DA METTERE NEL RUN COMMAND--------------------------------------------------------
    '    Dim parentLayerName As String = GetLayerName(side, eLayerType.root)
    '    If Not RhLayer.LayerExists(parentLayerName) Then RhLayer.CreateLayer(parentLayerName, True, False, Nothing)
    '    Dim pianiLayerName As String = GetLayerName(side, eLayerType.pianoInclinato)
    '    RhLayer.CreateColoredSubLayer(pianiLayerName, parentLayerName, New OnColor(My.Settings.PianiInclinatiColor))
    '    RhLayer.RendiCorrenteLayer(pianiLayerName)
    '    ''----------------------------------------------------------------------------------

    '    'IMPOSTAZIONE VISTA
    '    Dim viste() As MRhinoView = Nothing
    '    Doc.GetViewList(viste)
    '    For Each vista As MRhinoView In viste
    '        vista.ActiveViewport.EnableGhostedShade(True)
    '    Next
    '    'MaximizePerspectiveView(True)
    '    App.ActiveView.MainViewport.SetToBottomView()
    '    If Not App.ActiveView.IsMaximized Then App.ActiveView.MaximizeRestoreView()


    '    pianoInclinato.Set_P1_P2_Asse(side)

    '    pianoInclinato.Set_BT_CT_CV()

    '    pianoInclinato.Set_A_B_C_D_E()

    '    pianoInclinato.ProjectPoints(side)

    '    pianoInclinato.SetBordoEsterno_Tallone_Or_Totale(side)

    '    pianoInclinato.SetBordoInterno(side)

    '    pianoInclinato.SetBordoEsternoProiettato_Tallone_Or_Totale()

    '    'pianoInclinato.SetSrfLateraleFinale(side)

    '    'pianoInclinato.SetSrfInferioreFinale(side)

    '    'pianoInclinato.JoinFinale()

    '    'pianoInclinato.PulisciOggetiCostruzione()
    'End Sub

    'Private Sub PianoInclinatoPianta()
    '    Doc.Destroy()
    '    RhLayer.DeleteEmptyLayers()
    '    Dim filename As String = BASE_FILE_TEST_DIR & "piani inclinati\plantareDX.3dm"
    '    App.RunScript("_Import """ & filename & """", 0)
    '    App.RunScript("_SelNone", 0)

    '    Dim side = eSide.right
    '    Element3dManager.SetRhinoObj(eReferences.insoleBottomSurface, side, New Guid("05ca0da8-3c5a-469e-81bf-8617c62d32b8"))
    '    Dim spessore = 10
    '    Dim pianoInclinato As New PianoInclinatoPianta(ePosizionePianiInclinato.laterale, spessore)

    '    ''DA METTERE NEL RUN COMMAND--------------------------------------------------------
    '    Dim parentLayerName As String = GetLayerName(side, eLayerType.root)
    '    If Not RhLayer.LayerExists(parentLayerName) Then RhLayer.CreateLayer(parentLayerName, True, False, Nothing)
    '    Dim pianiLayerName As String = GetLayerName(side, eLayerType.pianoInclinato)
    '    RhLayer.CreateColoredSubLayer(pianiLayerName, parentLayerName, New OnColor(My.Settings.PianiInclinatiColor))
    '    RhLayer.RendiCorrenteLayer(pianiLayerName)
    '    ''----------------------------------------------------------------------------------

    '    'IMPOSTAZIONE VISTA
    '    Dim viste() As MRhinoView = Nothing
    '    Doc.GetViewList(viste)
    '    For Each vista As MRhinoView In viste
    '        vista.ActiveViewport.EnableGhostedShade(True)
    '    Next
    '    'MaximizePerspectiveView(True)
    '    App.ActiveView.MainViewport.SetToBottomView()
    '    If Not App.ActiveView.IsMaximized Then App.ActiveView.MaximizeRestoreView()


    '    pianoInclinato.Set_P1_P2_Asse(side)

    '    pianoInclinato.Set_BT_CV()

    '    pianoInclinato.Set_A_B_C_D_E_F

    '    pianoInclinato.ProjectPoints(side)

    '    pianoInclinato.SetBordoEsterno(side)

    '    pianoInclinato.SetBordoInterno(side)

    '    pianoInclinato.SetBordoEsternoProiettato()

    '    pianoInclinato.SetSrfLateraleFinale(side)

    '    pianoInclinato.SetSrfInferioreFinale(side)

    '    pianoInclinato.JoinFinale()

    '    pianoInclinato.PulisciOggetiCostruzione()
    'End Sub


    'Private Sub PianoInclinatoTallone()
    '    Doc.Destroy()
    '    RhLayer.DeleteEmptyLayers()
    '    'Dim filename As String = BASE_FILE_TEST_DIR & "piani inclinati\plantareDX.3dm"
    '    Dim filename As String = BASE_FILE_TEST_DIR & "piani inclinati\plantareDX_sagomato.3dm"
    '    App.RunScript("_Import """ & filename & """", 0)
    '    App.RunScript("_SelNone", 0)

    '    Dim side = eSide.right
    '    'Element3dManager.SetRhinoObj(eReferences.insoleBottomSurface, side, New Guid("05ca0da8-3c5a-469e-81bf-8617c62d32b8"))        
    '    Element3dManager.SetRhinoObj(eReferences.insoleBottomSurface, side, New Guid("6085292b-7de1-41a6-9ff3-c4455a303d5a"))
    '    Dim spessore = 10
    '    Dim pianoInclinato As New PianoInclinatoTallone(ePosizionePianiInclinato.mediale, spessore)

    '    ''DA METTERE NEL RUN COMMAND--------------------------------------------------------
    '    Dim parentLayerName As String = GetLayerName(side, eLayerType.root)
    '    If Not RhLayer.LayerExists(parentLayerName) Then RhLayer.CreateLayer(parentLayerName, True, False, Nothing)
    '    Dim pianiLayerName As String = GetLayerName(side, eLayerType.pianoInclinato)
    '    RhLayer.CreateColoredSubLayer(pianiLayerName, parentLayerName, New OnColor(My.Settings.PianiInclinatiColor))
    '    RhLayer.RendiCorrenteLayer(pianiLayerName)
    '    ''----------------------------------------------------------------------------------

    '    'IMPOSTAZIONE VISTA
    '    Dim viste() As MRhinoView = Nothing
    '    Doc.GetViewList(viste)
    '    For Each vista As MRhinoView In viste
    '        vista.ActiveViewport.EnableGhostedShade(True)
    '    Next
    '    'MaximizePerspectiveView(True)
    '    App.ActiveView.MainViewport.SetToBottomView()
    '    If Not App.ActiveView.IsMaximized Then App.ActiveView.MaximizeRestoreView()


    '    pianoInclinato.Set_P1_P2_Asse(side)

    '    pianoInclinato.Set_CT_CV()

    '    pianoInclinato.Set_A_B_C_D_E()

    '    pianoInclinato.ProjectPoints(side)

    '    pianoInclinato.SetBordoEsterno_Tallone_Or_Totale(side)

    '    pianoInclinato.SetBordoInterno(side)

    '    pianoInclinato.SetBordoEsternoProiettato_Tallone_Or_Totale()

    '    pianoInclinato.SetSrfLateraleFinale(side)

    '    pianoInclinato.SetSrfInferioreFinale(side)

    '    pianoInclinato.JoinFinale()

    '    pianoInclinato.PulisciOggetiCostruzione()
    'End Sub




    'Private Sub ImportMapPressireErrore()
    '    Element3dManager.ClearAddictions()
    '    Element3dManager.ResetReferences()
    '    Doc.Destroy()
    '    RhLayer.DeleteEmptyLayers()
    '    Dim filename = BASE_ID_DIR & "Errori e problemi\PL1.3dm"
    '    If Not File.Exists(filename) Then Exit Sub
    '    App.RunScript("_Open """ & filename & """", 0)
    '    App.RunScript("_SelNone", 0)
    'End Sub


    'Private Sub DeformaConPressione()
    '    Element3dManager.ClearAddictions()
    '    Element3dManager.ResetReferences()
    '    Doc.Destroy()
    '    RhLayer.DeleteEmptyLayers()
    '    Dim filename As String = BASE_FILE_TEST_DIR & "\mappe pressione\test_deformazione.3dm"
    '    If Not File.Exists(filename) Then Exit Sub
    '    App.RunScript("_Open """ & filename & """", 0)
    '    App.RunScript("_SelNone", 0)
    '    'BORDO
    '    Dim insoleTopRef = Element3dManager.GetRhinoObjRef(eReferences.insoleTopSurface, eSide.right)
    '    Dim insoleTopBorder As IOnCurve = RhGeometry.ExtractSurfaceBorder(insoleTopRef)
    '    RhDebug.AddDocumentToDebug(insoleTopBorder, "insoleTopBorder")
    '    ''DEFORMAZIONE
    '    'Dim frmDeform As New FrmDeformInsoleByPressure
    '    'frmDeform.Show()
    '    'frmDeform.OK_Button_Click(ThisPlugIn().MainPanel, New EventArgs())
    '    'RhUtil.RhinoApp.RunScript("IdDeformInsoleByPressure", 0)
    'End Sub


    'Private Sub JoinMetBar2()
    '    Doc.Destroy()
    '    RhLayer.DeleteEmptyLayers()
    '    ''FILE VERSIONE 2
    '    'Dim filename As String = BASE_FILE_TEST_DIR & "join-metbar2.3dm"
    '    'Dim addiction As IdAddiction = IdAddictionFactory.Create(eSide.right, eAddictionType.metatarsalBar, eAddictionModel.metbar3450, eAddictionSize.M)
    '    'addiction.SurfaceID = Guid.Parse("81e2a48a-4f9b-40ab-adb1-21bf1abed201")
    '    'Dim metbar As IdMetbarAddiction = DirectCast(addiction, IdMetbarAddiction)
    '    'metbar.TrimmedInsoleSrf(eMetbarSrf.anterior) = Guid.Parse("9c85e2d7-ca3b-44dc-b6ee-7676daf84d3a")
    '    'metbar.TrimmedInsoleSrf(eMetbarSrf.posterior) = Guid.Parse("b61ac405-a712-4996-8dbf-c41126c3bbb1")
    '    ''FILE VERSIONE 3
    '    Dim filename As String = BASE_FILE_TEST_DIR & "join-metbar3.3dm"
    '    Dim type = IdAddiction.eAddictionType.metatarsalBar
    '    Dim model = IdAddiction.eAddictionModel.metbar3450
    '    Dim size = IdAddiction.eAddictionSize.M
    '    Dim addiction As IdAddiction = IdAddictionFactory.Create(eSide.right, type, model, size)
    '    addiction.SurfaceID = Guid.Parse("4cc9e134-efb8-4d2b-925c-e83c481c052d")
    '    Dim metbar As IdMetbarAddiction = DirectCast(addiction, IdMetbarAddiction)
    '    metbar.TrimmedInsoleSrf(eMetbarSrf.anterior) = Guid.Parse("8cb8d52f-3a75-4c6b-a1c2-6fdb568c7437")
    '    metbar.TrimmedInsoleSrf(eMetbarSrf.posterior) = Guid.Parse("12a646d7-f60d-4292-833a-bfe18ef86ff0")

    '    App.RunScript("_Import """ & filename & """", 0)
    '    App.RunScript("_SelNone", 0)
    '    'Impostazione Ghosted
    '    Dim viste() As MRhinoView = Nothing
    '    Doc.GetViewList(viste)
    '    For Each vista As MRhinoView In viste
    '        vista.ActiveViewport.EnableGhostedShade(True)
    '    Next

    '    metbar.CreateJoinSurface()

    '    MaximizePerspectiveView(False)
    'End Sub


    'Private Sub TestFileLucaTarabelli()
    '    Element3dManager.ClearAddictions()
    '    Element3dManager.ResetReferences()
    '    Doc.Destroy()
    '    RhLayer.DeleteEmptyLayers()
    '    Dim directry As String = "D:\Dropbox\Duna-Macchione\Prove template\"
    '    'Open file
    '    Dim filename As String = "Omar 42 (taglia campione 42) plantare piatto 26-02-2016.3Dm"
    '    Dim path As String = directry & filename
    '    App.RunScript("_Open """ & path & """", 0)
    '    App.RunScript("_SelNone", 0)
    '    Dim uuid As Guid = Element3dManager.GetRhinoObjID(eReferences.finalUpperCurve, eSide.right)
    '    If Doc.LookupDocumentObject(uuid, True) Is Nothing Then
    '        MsgBox("Curva di profilo non presente nel file " & filename)
    '    End If
    '    Dim objref As New MRhinoObjRef(uuid)
    '    AddDocumentToDebug(objref.Curve.DuplicateCurve, "curva di profilo rilevata file " & filename)
    '    'Altro file
    '    filename = "Omar 43 (taglia campione 42) plantare piatto 29-02-2016.3dm"
    '    path = directry & filename
    '    uuid = Element3dManager.GetRhinoObjID(eReferences.finalUpperCurve, eSide.right)
    '    If Doc.LookupDocumentObject(uuid, True) Is Nothing Then
    '        MsgBox("Curva di profilo non presente nel file " & filename)
    '    End If
    '    objref = New MRhinoObjRef(uuid)
    '    AddDocumentToDebug(objref.Curve.DuplicateCurve, "curva di profilo rilevata file " & filename)
    'End Sub


    'Private Sub TestNuovaSrfTopDopoTagliMetbar()
    '    Doc.Destroy()
    '    RhLayer.DeleteEmptyLayers()
    '    Dim filename As String = BASE_FILE_TEST_DIR & "Test nuova srfTop dopo tagli metbar.3dm"
    '    App.RunScript("_Import """ & filename & """", 0)
    '    App.RunScript("_SelNone", 0)
    '    'Impostazione Ghosted
    '    Dim viste() As MRhinoView = Nothing
    '    Doc.GetViewList(viste)
    '    For Each vista As MRhinoView In viste
    '        vista.ActiveViewport.EnableGhostedShade(True)
    '    Next
    '    Dim srfInsoleRef As New MRhinoObjRef(Guid.Parse("1ab71b01-63aa-4432-bf0b-ad7d00b84956"))
    '    Dim metbarRef As New MRhinoObjRef(Guid.Parse("7ec3742e-98fb-4828-84f6-6abe079c2b6b"))
    '    Dim metbarSrf As IOnSurface = metbarRef.Surface
    '    'Scorro tutti i CV della srfTop
    '    Dim cv0 As Integer = srfInsoleRef.Surface.NurbsSurface.m_cv_count(0)
    '    Dim cv1 As Integer = srfInsoleRef.Surface.NurbsSurface.m_cv_count(1)
    '    Dim filanCVlist(cv0 - 1, cv1 - 1) As On3dPoint
    '    For i As Integer = 0 To cv0 - 1
    '        For j As Integer = 0 To cv1 - 1
    '            Dim point As New On3dPoint
    '            srfInsoleRef.Surface.NurbsSurface.GetCV(i, j, point)
    '            'Se è un punto di bordo lo mantengo
    '            If i = 0 Or i = cv0 - 1 Or j = 0 And j = cv1 - 1 Then
    '                filanCVlist(i, j) = New On3dPoint(point)
    '            Else
    '                'Se no provo a intersecare con una retta la metbar
    '                Dim distancePointToSrf As Double = point.DistanceTo(metbarSrf.BoundingBox.m_max) + 0.1
    '                Dim startPoint As New On3dPoint(point.x, point.y, point.z - distancePointToSrf)
    '                Dim endPoint As New On3dPoint(point.x, point.y, point.z + distancePointToSrf)
    '                Dim line As New OnLine(startPoint, endPoint)
    '                Dim intersections As New ArrayOnX_EVENT
    '                line.IntersectSurface(metbarSrf, intersections)
    '                If intersections.Count = 0 Then
    '                    'Se non interseca aggiungo il punto originale
    '                    filanCVlist(i, j) = New On3dPoint(point)
    '                Else
    '                    'Se interseca cerco quello più vicino tra tutti quelli del metbar e lo sostituisco con quello
    '                    Dim testPoint As New On3dPoint(intersections.Item(0).m_pointA(0))
    '                    'Questo è geometricamente corretto ma impiega troppa memoria
    '                    'filanCVlist(i, j) = FindNearestCV(testPoint, metbarSrf)
    '                    'testPoint.Dispose()
    '                    filanCVlist(i, j) = testPoint
    '                End If
    '                startPoint.Dispose()
    '                endPoint.Dispose()
    '                line.Dispose()
    '                intersections.Dispose()
    '            End If
    '        Next
    '    Next
    '    'Creo nuava SRF
    '    Dim newSrf As OnNurbsSurface = RhGeometry.CreaSuperficeDaCV(filanCVlist)
    '    'Aggiungo al Doc 
    '    Doc.AddSurfaceObject(newSrf)
    'End Sub


    'Private Sub GetPolysurfacePartTest()
    '    Doc.Destroy()
    '    RhLayer.DeleteEmptyLayers()
    '    Dim filename As String = BASE_FILE_TEST_DIR & "polysurface trimmata.3dm"
    '    App.RunScript("_Import """ & filename & """", 0)
    '    App.RunScript("_SelNone", 0)
    '    'Impostazione Ghosted
    '    Dim viste() As MRhinoView = Nothing
    '    Doc.GetViewList(viste)
    '    For Each vista As MRhinoView In viste
    '        vista.ActiveViewport.EnableGhostedShade(True)
    '    Next
    '    Dim objref As New MRhinoObjRef(Guid.Parse(" 2a15ecff-a500-4f2f-92ea-2a6cd374b9a1"))
    '    RhGeometry.GetPolysurfacePart(objref)
    'End Sub



    'Private Sub OffsetManualeTest()
    '    Doc.Destroy()
    '    RhLayer.DeleteEmptyLayers()
    '    Dim filename As String = BASE_FILE_TEST_DIR & "preoffset.3dm"
    '    App.RunScript("_Import """ & filename & """", 0)
    '    App.RunScript("_SelNone", 0)
    '    'Impostazione Ghosted
    '    Dim viste() As MRhinoView = Nothing
    '    Doc.GetViewList(viste)
    '    For Each vista As MRhinoView In viste
    '        vista.ActiveViewport.EnableGhostedShade(True)
    '    Next
    '    Dim precurve1 As IOnCurve = New MRhinoObjRef(Guid.Parse("f311cfc9-78a8-474e-8789-9ec1d2970c2d")).Curve
    '    Dim precurve2 As IOnCurve = New MRhinoObjRef(Guid.Parse("ad77e733-d208-4b38-af02-dd224a2b07cc")).Curve
    '    Dim precurve3 As IOnCurve = New MRhinoObjRef(Guid.Parse("c0947af8-82d3-414b-82fb-7ccae1533ce1")).Curve
    '    ManualOffsetCurve(precurve1, IdDrawUpperCurvesCommand.OFFSET_CURVE_DISTANCE * 5)
    '    ManualOffsetCurve(precurve2, IdDrawUpperCurvesCommand.OFFSET_CURVE_DISTANCE * 5)
    '    ManualOffsetCurve(precurve3, IdDrawUpperCurvesCommand.OFFSET_CURVE_DISTANCE * 5)
    'End Sub


    'Private Sub ScarichiTest()
    '    'Preparazione
    '    Element3dManager.ClearFootCurvesTrimSurface()
    '    Element3dManager.ClearAddictions()
    '    Element3dManager.ResetReferences()
    '    Doc.Destroy()
    '    RhLayer.DeleteEmptyLayers()
    '    'Import file
    '    Dim filename As String = BASE_FILE_TEST_DIR & "base test scarichi.3dm"
    '    App.RunScript("_Import """ & filename & """", 0)
    '    App.RunScript("_SelNone", 0)
    '    'Impostazione Ghosted
    '    Dim viste() As MRhinoView = Nothing
    '    Doc.GetViewList(viste)
    '    For Each vista As MRhinoView In viste
    '        vista.ActiveViewport.EnableGhostedShade(True)
    '    Next
    '    'SUPERFICI PLANTARE
    '    Element3dManager.SetRhinoObj(eReferences.insoleTopSurface, eSide.right, Guid.Parse("1e16baa6-1b49-4a33-85d0-811540e5c87a"))
    '    Element3dManager.SetRhinoObj(eReferences.insoleLateralSurface, eSide.right, Guid.Parse("5b084949-b5c3-48f6-98c5-4b912b18e364"))
    '    Element3dManager.SetRhinoObj(eReferences.insoleBottomSurface, eSide.right, Guid.Parse("33def970-274e-4bb1-ae1b-0f8877998f8b"))
    '    App.RunScript("_IdAddAddiction", 0)
    'End Sub


    'Private Sub CREAZIONE_SRF_CON_PUNTI_SUL_BORDO_PER_LISCIATURA_SRF_PLANATRE()
    '    Doc.Destroy()
    '    RhLayer.DeleteEmptyLayers()
    '    'Import file
    '    Dim filename As String = BASE_FILE_TEST_DIR & "test srf liscia\test1 OFS.3dm"
    '    App.RunScript("_Import """ & filename & """", 0)
    '    App.RunScript("_SelNone", 0)
    '    'ATTENZIONE: per estrarre edge e vertici della SRF trimmata non devo usare MRhinoObjRef.Surface() ma MRhinoObjRef.Geometry.BrepForm
    '    'Dim insoleSrf As OnSurface = New MRhinoObjRef(Guid.Parse("b8def386-9951-43d2-8c08-27f49f6c86dc")).Surface().DuplicateSurface()
    '    Dim insoleBrep As OnBrep = New MRhinoObjRef(Guid.Parse("b8def386-9951-43d2-8c08-27f49f6c86dc")).Geometry.BrepForm
    '    Dim bordo As OnCurve = New MRhinoObjRef(Guid.Parse("1a6d0d05-f87c-484d-a6d8-f7f8532a97d1")).Curve.DuplicateCurve()
    '    For Each vertex As OnBrepVertex In insoleBrep.m_V
    '        AddDocumentToDebug(vertex.Point, "vertex")
    '    Next
    '    For Each edge As OnBrepEdge In insoleBrep.m_E
    '        AddDocumentToDebug(edge, "edge")
    '    Next
    '    For Each srf As OnSurface In insoleBrep.m_S
    '        AddDocumentToDebug(srf, "srf")
    '        Dim cv0 As Integer = srf.NurbsSurface.m_cv_count(0)
    '        Dim cv1 As Integer = srf.NurbsSurface.m_cv_count(1)
    '        Dim newCV(cv0 - 1, cv1 - 1) As On3dPoint
    '        For i As Integer = 0 To cv0 - 1
    '            For j As Integer = 0 To cv1 - 1
    '                Dim point As New On3dPoint
    '                srf.NurbsSurface.GetCV(i, j, point)
    '                AddDocumentToDebug(point, "OLD_point_i=" & i & "_j=" & j)
    '                If i = 0 Or i = cv0 - 1 Or j = 0 And j = cv1 - 1 Then
    '                    Dim t As Double
    '                    bordo.GetClosestPoint(point, t)
    '                    newCV(i, j) = New On3dPoint(bordo.PointAt(t))
    '                Else
    '                    newCV(i, j) = New On3dPoint(point)
    '                End If
    '            Next
    '        Next
    '        Dim newSrf As OnNurbsSurface = RhGeometry.CreaSuperficeDaCV(newCV)
    '        AddDocumentToDebug(newSrf, "newSrf")
    '    Next
    'End Sub


    'Private Sub FilletTest()
    '    Doc.Destroy()
    '    RhLayer.DeleteEmptyLayers()
    '    'Import file
    '    Dim filename As String = BASE_FILE_TEST_DIR & "test_raccordo curve dritte.3dm"
    '    App.RunScript("_Import """ & filename & """", 0)
    '    App.RunScript("_SelNone", 0)
    '    'Impostazione Ghosted
    '    Dim viste() As MRhinoView = Nothing
    '    Doc.GetViewList(viste)
    '    For Each vista As MRhinoView In viste
    '        vista.ActiveViewport.EnableGhostedShade(True)
    '    Next
    '    'SCARICHI
    '    Dim curve1 As OnCurve = New MRhinoObjRef(Guid.Parse("56dd875e-c43e-419e-b97b-1c47e62f6cf6")).Curve.DuplicateCurve()
    '    Dim curve2 As OnCurve = New MRhinoObjRef(Guid.Parse("3a04f6a2-48eb-4647-983d-635c9ff61e7e")).Curve.DuplicateCurve()
    '    Dim radius As Double = 5
    '    Dim plane As New OnPlane(OnUtil.On_xy_plane)
    '    Dim fillet As OnNurbsCurve = FilletLineCurve(curve1, curve2, plane, radius, IdGeometryUtils.eFilletSide.inner)
    '    AddDocumentToDebug(fillet, "fillet intersecanti")
    '    filename = BASE_FILE_TEST_DIR & "test_raccordo curve dritte2.3dm"
    '    App.RunScript("_Import """ & filename & """", 0)
    '    App.RunScript("_SelNone", 0)
    '    curve1 = New MRhinoObjRef(Guid.Parse("0ab4dbe8-02cd-4b37-9f37-addd010d09e7")).Curve.DuplicateCurve()
    '    curve2 = New MRhinoObjRef(Guid.Parse("0ae59f4e-42e9-48b7-a8ba-60d85c64b0da")).Curve.DuplicateCurve()
    '    fillet = FilletLineCurve(curve1, curve2, plane, radius, IdGeometryUtils.eFilletSide.inner)
    '    AddDocumentToDebug(fillet, "fillet non intersecanti")
    '    fillet = FilletLineCurve(curve1, curve2, plane, radius, IdGeometryUtils.eFilletSide.outer)
    '    AddDocumentToDebug(fillet, "fillet outer")
    'End Sub


    'Private Sub FilletSrfTest()
    '    '-_FilletSrf
    '    App.RunScript("_SelNone", 0)
    '    'Estraggo i bordi per i bianri dello sweep2
    '    Dim lateralSrfRef As New MRhinoObjRef(Guid.Parse("4161f809-4675-4c24-9938-2f22e9083521"))
    '    Dim bottomSrfRef As New MRhinoObjRef(Guid.Parse("a2fffec6-b13b-4f50-9002-11d273cdbb5c"))
    '    bottomSrfRef.Object.Select(True, True)
    '    RhUtil.RhinoApp().RunScript("-_DupBorder", 0)
    '    App.RunScript("_SelLast", 0)
    '    Dim getObjects As New MRhinoGetObject
    '    getObjects.SetGeometryFilter(IRhinoGetObject.GEOMETRY_TYPE_FILTER.curve_object)
    '    getObjects.GetObjects(0, Integer.MaxValue)
    '    If getObjects.ObjectCount <> 1 Then Exit Sub
    '    Dim bottomRailRef As MRhinoObjRef = getObjects.Object(0)
    '    App.RunScript("_SelNone", 0)
    '    lateralSrfRef.Object.Select(True, True)
    '    RhUtil.RhinoApp().RunScript("-_DupBorder", 0)
    '    App.RunScript("_SelLast", 0)
    '    getObjects.ClearObjects()
    '    getObjects.SetGeometryFilter(IRhinoGetObject.GEOMETRY_TYPE_FILTER.curve_object)
    '    getObjects.GetObjects(0, Integer.MaxValue)
    '    If getObjects.ObjectCount <> 2 Then Exit Sub
    '    Dim lateralRailRef As MRhinoObjRef = Nothing
    '    Dim min0, min1 As Double
    '    min0 = getObjects.Object(0).Object.BoundingBox().m_min.z  'Curve.GetLength(lenght0)
    '    min1 = getObjects.Object(1).Object.BoundingBox().m_min.z 'Curve.GetLength(lenght1)
    '    If min0 < min1 Then
    '        lateralRailRef = getObjects.Object(0)
    '        Doc.DeleteObject(getObjects.Object(1))
    '    Else
    '        lateralRailRef = getObjects.Object(1)
    '        Doc.DeleteObject(getObjects.Object(0))
    '    End If
    '    Dim t As Double
    '    'Creo l'arco per la sezione dello sweep2
    '    Dim refPoint As New On3dPoint(lateralSrfRef.Object.BoundingBox.m_max.x * 10, lateralSrfRef.Object.BoundingBox.m_max.y * 10, lateralSrfRef.Object.BoundingBox.m_max.z * 10)
    '    bottomRailRef.Curve.GetClosestPoint(refPoint, t)
    '    Dim bottomPoint As On3dPoint = bottomRailRef.Curve.PointAt(t)
    '    lateralRailRef.Curve.GetClosestPoint(refPoint, t)
    '    Dim lateralPoint As On3dPoint = lateralRailRef.Curve.PointAt(t)
    '    Dim arc As New OnArc()
    '    Dim arcDirection As On3dVector = refPoint - lateralPoint
    '    arcDirection.Unitize()
    '    arc.Create(bottomPoint, arcDirection, lateralPoint)
    '    Dim arcObj As MRhinoObject = Doc.AddCurveObject(arc)
    '    App.RunScript("_SelNone", 0)
    '    refPoint.Dispose()
    '    lateralPoint.Dispose()
    '    bottomPoint.Dispose()
    '    arc.Dispose()
    '    arcDirection.Dispose()
    '    bottomRailRef.Object.Select(True, True)
    '    lateralRailRef.Object.Select(True, True)
    '    Dim sweep2Cmd As String = "-_Sweep2 _SelID " & arcObj.Attributes.m_uuid.ToString
    '    If RhinoLanguageSetting() = elanguage.English Then
    '        sweep2Cmd &= " _Enter Simplify=None MaintainHeight=No _Enter"
    '    Else
    '        sweep2Cmd &= " _Enter Semplifica=No _M=_N _Enter"
    '    End If
    '    App.RunScript(sweep2Cmd, 1)
    '    App.RunScript("_SelLast", 0)
    '    getObjects.ClearObjects()
    '    getObjects.SetGeometryFilter(IRhinoGetObject.GEOMETRY_TYPE_FILTER.surface_object Or IRhinoGetObject.GEOMETRY_TYPE_FILTER.polysrf_object)
    '    getObjects.GetObjects(0, Integer.MaxValue)
    '    If getObjects.ObjectCount <> 1 Then Exit Sub
    '    Dim manufacturingBlendBottom As IRhinoObjRef = getObjects.Object(0)
    '    App.RunScript("_SelNone", 0)
    'End Sub



    'Private Sub CreazioneTubicinoPerRaccordo()
    '    App.RunScript("_SelNone", 0)
    '    Dim commandSweep As String = "-_Sweep1"
    '    Dim maxX As New On3dPoint
    '    Dim upperCurveRef As New MRhinoObjRef(Guid.Parse("42cb0301-4d3a-4127-bc59-fa6d4e45353f "))
    '    Dim sweepCrossSections As SortedList(Of Double, Guid) = GetSweepCrossSection(upperCurveRef.m_uuid, maxX, RhGeometry.eCrossSectionStart.maxX, 1.5)
    '    For Each stringID As Guid In sweepCrossSections.Values
    '        commandSweep &= " _SelID " & stringID.ToString()
    '    Next
    '    If RhinoMajorRelease() = 5 Then commandSweep &= " _Enter"
    '    If RhinoLanguageSetting() = elanguage.English Then
    '        commandSweep &= " Natural _Enter Style=RoadlikeSuperiore Simplify=None Closed=Yes ShapeBlending=Local RefitRail=No SimpleSweep=Yes _Enter"
    '    Else
    '        commandSweep &= " _N _Enter _S=TorsioneLibera _E=Nessuno _C=Sì _Enter"
    '    End If
    '    upperCurveRef.Object.Select(True, True)
    '    App.RunScript(commandSweep, 1)
    '    App.RunScript("_SelLast", 0)
    '    Dim getObjects As New MRhinoGetObject
    '    Dim filter() As IRhinoGetObject.GEOMETRY_TYPE_FILTER = {IRhinoGetObject.GEOMETRY_TYPE_FILTER.surface_object, IRhinoGetObject.GEOMETRY_TYPE_FILTER.polysrf_object}
    '    getObjects.SetGeometryFilter(filter)
    '    Dim refTubicino As MRhinoObjRef
    '    getObjects.GetObjects(0, Integer.MaxValue)
    '    If getObjects.ObjectCount > 1 Then
    '        App.RunScript("_Join", 0)
    '        App.RunScript("_SelLast", 0)
    '        getObjects.ClearObjects()
    '        getObjects.SetGeometryFilter(filter)
    '        getObjects.GetObjects(0, Integer.MaxValue)
    '    End If
    '    'Se lo sweep ha creato più di una superficie e non si possono unire esco
    '    If getObjects.ObjectCount <> 1 Then MsgBox("Impossibile creare il cilindro per il taglio")
    '    refTubicino = getObjects.Object(0)
    'End Sub


    'Private Sub SplopTest()
    '    RhLayer.RendiCorrenteLayer("Scarichi DX")
    '    Dim insoleId As Guid = Guid.Parse("168b80e8-28c9-449e-8bdf-63d4f5f67fc9")
    '    Dim insoleObjRef As New MRhinoObjRef(insoleId)
    '    Dim type = IdAddiction.eAddictionType.metatarsalBar
    '    Dim model = IdAddiction.eAddictionModel.metbar3870
    '    Dim size = IdAddiction.eAddictionSize.M
    '    Dim testAddiction As IdAddiction = IdAddictionFactory.Create(eSide.left, type, model, size)
    '    testAddiction.ParseAddictionId("Scarichi DX")
    '    Dim addictionBbox As OnBoundingBox = testAddiction.GetBbox()
    '    Dim point1 As New On3dPoint(addictionBbox.Center.x, addictionBbox.Center.y, addictionBbox.m_min.z)
    '    Dim point2 As New On3dPoint(point1.x, addictionBbox.m_max.y, point1.z)
    '    Dim curveLenght As Double = point1.DistanceTo(insoleObjRef.Object.BoundingBox.m_min) * 10
    '    Dim pointCurve As New On3dPoint(point1.x, point1.y, point1.z - curveLenght)
    '    Dim pointArray As New On3dPointArray
    '    pointArray.Append(point1)
    '    pointArray.Append(pointCurve)
    '    Dim polyline As New OnPolylineCurve(pointArray)
    '    Dim curve As OnCurve = polyline.NurbsCurve()
    '    Dim resultPoint As On3dPointArray = RhGeometry.IntersecaCurvaConSuperfice(curve, insoleObjRef.Surface())
    '    Dim point3 As On3dPoint = resultPoint.Item(0)
    '    Dim point4 As New On3dPoint(point2.x, point2.y, point3.z)
    '    Dim point3ObjRef As New MRhinoObjRef(RhUtil.RhinoApp().ActiveDoc.AddPointObject(point3).Attributes.m_uuid)
    '    Dim point4ObjRef As New MRhinoObjRef(RhUtil.RhinoApp().ActiveDoc.AddPointObject(point4).Attributes.m_uuid)
    '    Dim point1Str As String = point1.x.ToString.Replace(",", ".") & "," & point1.y.ToString.Replace(",", ".") & "," & point1.z.ToString.Replace(",", ".")
    '    Dim point2Str As String = point2.x.ToString.Replace(",", ".") & "," & point2.y.ToString.Replace(",", ".") & "," & point2.z.ToString.Replace(",", ".")
    '    RhUtil.RhinoApp().RunScript("_Osnap _Enter", 0)
    '    RhUtil.RhinoApp().RunScript("_SelNone", 0)
    '    testAddiction.SelectCurves()
    '    'Dim splopCmd As String = "-_Splop " & point1Str & " " & point2Str & " _SelID " & insoleId.ToString & " _R=No _F=No " & point3Str & " " & point4Str & " _Enter"
    '    Dim splopCmd As String = "-_Splop " & point1Str & " " & point2Str & " _SelID " & insoleId.ToString & " _C=No _R=No _F=No "
    '    RhUtil.RhinoApp().RunScript(splopCmd, 0)
    '    Doc.DeleteObject(point3ObjRef)
    '    Doc.DeleteObject(point4ObjRef)
    '    RhUtil.RhinoApp().RunScript("_SelNone", 0)
    'End Sub


#End Region


#Region " Command detail "

    '''<returns>The command name as it appears on the Rhino command line.</returns>
    Public Overrides ReadOnly Property EnglishName() As String
        Get
            Return ENGLISH_CMD_NAME
        End Get
    End Property

#End Region


End Class


#End If