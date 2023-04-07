Imports System.Text
Imports System.IO
Imports InsoleDesigner
Imports ORM
Imports ORM.IdDataSet
Imports ORM.IdDataSetTableAdapters
Imports ORM.DataBindingHelper
Imports System.Reflection


<TestClass()>
Public Class IdTemplateTest

    Private testContextInstance As TestContext

    '''<summary>
    '''Ottiene o imposta il contesto del test che fornisce
    '''le informazioni e le funzionalità per l'esecuzione del test corrente.
    '''</summary>
    Public Property TestContext() As TestContext
        Get
            Return testContextInstance
        End Get
        Set(ByVal value As TestContext)
            testContextInstance = value
        End Set
    End Property


#Region "Attributi di test aggiuntivi"
    '
    ' È possibile utilizzare i seguenti attributi aggiuntivi per la scrittura dei test:
    '
    ' Utilizzare ClassInitialize per eseguire il codice prima di eseguire il primo test della classe
    ' <ClassInitialize()> Public Shared Sub MyClassInitialize(ByVal testContext As TestContext)
    ' End Sub
    '
    ' Utilizzare ClassCleanup per eseguire il codice dopo l'esecuzione di tutti i test della classe
    ' <ClassCleanup()> Public Shared Sub MyClassCleanup()
    ' End Sub
    '
    ' Utilizzare TestInitialize per eseguire il codice prima di eseguire ciascun test
    <TestInitialize()> Public Sub MyTestInitialize()
        DbHelper.GetInstance.SetDataDirectory(My.Application.Info.DirectoryPath)
    End Sub
    '
    ' Utilizzare TestCleanup per eseguire il codice dopo l'esecuzione di ciascun test
    ' <TestCleanup()> Public Sub MyTestCleanup()
    ' End Sub
    '
#End Region


    ''' <summary>
    ''' Varie combinazioni di campi template errate e alla fine 2 template corretti di cui uno con patologie
    ''' </summary>
    <TestMethod()>
    Public Sub WriteInDbTemplateTest()
        Dim template As New IdTemplate

        'ASSERT FALLIMENTI
        Assert.IsFalse(template.WriteInDB())
        template.TemplateID = Guid.NewGuid.ToString
        Assert.IsFalse(template.WriteInDB())
        Dim lastModelAdapter As New LastModelTableAdapter
        template.LastModelID = lastModelAdapter.GetData.Rows(0).Item(0)
        Assert.IsFalse(template.WriteInDB())
        Dim sizeAdapter As New SizeTableAdapter
        template.SizeID = sizeAdapter.GetData.Rows(0).Item(0)
        Assert.IsFalse(template.WriteInDB())
        Dim bottomTypeAdapter As New BottomTypeTableAdapter
        template.BottomTypeID = bottomTypeAdapter.GetData.Rows(0).Item(0)
        Assert.IsFalse(template.WriteInDB())
        Dim vaultAdapter As New VaultTableAdapter
        template.VaultID = vaultAdapter.GetData.Rows(0).Item(0)
        Assert.IsFalse(template.WriteInDB())
        Dim thicknessAdapter As New ThicknessTableAdapter
        template.ThicknessID = thicknessAdapter.GetData.Rows(1).Item(0)
        Assert.IsFalse(template.WriteInDB())
        Dim userAdapter As New UserTableAdapter
        template.UserID = userAdapter.GetData.Rows(0).Item(0)
        Assert.IsFalse(template.WriteInDB())
        template.Pathologies.Add(-1)
        Assert.IsFalse(template.WriteInDB())
        template.File3dName(IdElement3dManager.eSide.left) = "test"
        Assert.IsFalse(template.WriteInDB())
        template.Pathologies.Clear()
        'Eccezione causata da ID non coerente
        template.LastModelID = 9999
        Try
            Assert.IsFalse(template.WriteInDB())
        Catch ex As Exception
            Assert.IsTrue(True)
        End Try

        'ASSERT SUCCESSI
        template.LastModelID = lastModelAdapter.GetData.Rows(0).Item(0)
        Assert.IsTrue(template.WriteInDB())
        Dim pathologyAdapter As New PathologyTableAdapter
        template.Pathologies.Add(pathologyAdapter.GetData.Rows(0).Item(0))
        template.ThicknessID = thicknessAdapter.GetData.Rows(0).Item(0)
        Assert.IsTrue(template.WriteInDB())
        Dim newPatientId As Integer = -1
        Dim stringaUnivocaPerOggi As String = Now.TimeOfDay.Hours & Now.TimeOfDay.Minutes & Now.TimeOfDay.Seconds
        Assert.IsTrue(DbHelper.GetInstance.InsertPatientAndSetId("Ale" & stringaUnivocaPerOggi, "Macchio", Now.Date, newPatientId))
        Dim templatePatientAdapter As New Template_PatientTableAdapter
        Assert.AreEqual(1, templatePatientAdapter.Insert(template.TemplateID, newPatientId))
    End Sub


    '''' <summary>
    '''' Template con tutte le possibili combinazioni(anche non ammissibili a livello di filtri) accettate dal DB
    '''' Utile per testare ordinamento dei template
    '''' </summary>
    '<TestMethod()>
    'Public Sub WriteAllCombinationTemplateTest()

    '    Dim ds As New IdDataSet
    '    FillLastModelTable(ds)
    '    FillSizeTable(ds)
    '    FillThicknessTable(ds)
    '    FillBottomTypeTable(ds)
    '    FillVaultTable(ds)
    '    FillTemplateTable(ds)
    '    Dim templateAdapter As New TemplateTableAdapter

    '    'Pulisco tabella
    '    For Each t As TemplateRow In ds.Template
    '        templateAdapter.Delete(t.ID, t.LastModelID, t.SizeID, t.ThicknessID, t.BottomTypeID, t.VaultID, t.UserID, t.File3DLeft, t.File3DRight)
    '    Next
    '    Assert.AreEqual(0, templateAdapter.GetData.Rows.Count)
    '    templateAdapter.Update(ds.Template)

    '    'Inserimento
    '    Dim counter As Integer = 1
    '    For i As Integer = 1 To ds.LastModel.Rows.Count
    '        For j As Integer = 1 To ds.Size.Rows.Count
    '            For k As Integer = 1 To ds.Thickness.Rows.Count
    '                For v As Integer = 1 To ds.BottomType.Rows.Count
    '                    For w As Integer = 1 To ds.Vault.Rows.Count

    '                        Dim templateID As String = counter  '"test_" & counter
    '                        Dim lastModelId As Integer = ds.LastModel.Item(i Mod ds.LastModel.Rows.Count).ID
    '                        Dim sizeId As Integer = ds.Size.Item(j Mod ds.Size.Rows.Count).ID
    '                        Dim thicknessId As Integer = ds.Thickness.Item(k Mod ds.Thickness.Rows.Count).ID
    '                        Dim bottomId As Integer = ds.BottomType.Item(v Mod ds.BottomType.Rows.Count).ID
    '                        Dim vaultId As Integer = ds.Vault.Item(w Mod ds.Vault.Rows.Count).ID
    '                        templateAdapter.Insert(templateID, lastModelId, sizeId, thicknessId, bottomId, vaultId, 2, "", "")
    '                        counter += 1
    '                    Next
    '                Next
    '            Next
    '        Next
    '    Next
    '    Dim combination As Integer = ds.LastModel.Rows.Count * ds.Size.Rows.Count * ds.Thickness.Rows.Count * ds.BottomType.Rows.Count * ds.Vault.Rows.Count
    '    Assert.AreEqual(combination, templateAdapter.GetData.Rows.Count)
    'End Sub



    ''NON SONO RIUSCITO (e forse non è possibile) A OTTENERE VIA CODICE LA DIRECTORY DI OUTPUT DELLA dll PRINCIPALE (InsoleDesigner.dll)
    '''' <summary>
    '''' Elimin la cartella ..\InsoleDesigner\bin\x86\Release\Library\Template che potrebbe avere dei file generati in fase di testing manuale
    '''' Una volta eliminata nessuno dei test automatici scrive in quella directory
    '''' </summary>
    '<TestMethod()>
    'Public Sub ClearTemplateFileTest()
    '    'Assembly.GetAssembly(Type.GetType("InsoleDesigner")).Location
    '    'Ricostruisco il percorso        
    '    Dim templatePath As String = AppDomain.CurrentDomain.BaseDirectory & "\" & GetDirectory(eDirectoryLibrary.template)
    '    Dim customPath As String = AppDomain.CurrentDomain.BaseDirectory & "\" & TemplateRelativeDirectoryPath(False)
    '    Dim staticPath As String = AppDomain.CurrentDomain.BaseDirectory & "\" & TemplateRelativeDirectoryPath(True)
    '    If Not Directory.Exists(templatePath) Then
    '        Assert.IsTrue(True)
    '        Exit Sub
    '    End If
    '    'Creo le directory
    '    Dim templateDirectory As New DirectoryInfo(templatePath)
    '    Dim staticDirectory As New DirectoryInfo(staticPath)
    '    Dim customDirectory As New DirectoryInfo(customPath)
    '    'Cancello la cartella tempalte\custom
    '    If Directory.Exists(customPath) Then customDirectory.Delete(True)
    '    'Cancello tutti i file .igs dentro static che non hanno corrispondenza nel DB
    '    If Directory.Exists(staticPath) Then
    '        Dim dataSet As New IdDataSet
    '        DataBindingHelper.FillTemplateTable(dataSet)
    '        For Each file As FileInfo In staticDirectory.GetFiles
    '            If file.Extension = TEMPLATE_3D_EXTENSION Then
    '                Dim dbName As String = file.Name.Replace(file.Extension, "")
    '                For Each template As TemplateRow In dataSet.Template.Rows
    '                    If template.File3DLeft = dbName Or template.File3DRight = dbName Then Continue For
    '                Next
    '                file.Delete()
    '            Else
    '                file.Delete()
    '            End If
    '        Next
    '        'Se la cartella static è vuota cancello tutta la cartella template
    '        If staticDirectory.GetFiles.Length = 0 Then staticDirectory.Delete(True)
    '    End If

    '    'Se la directory dei template non ha sottocartelle la posso cancellare        
    '    If templateDirectory.GetDirectories().Length = 0 Then templateDirectory.Delete(True)
    '    Assert.IsTrue(True)
    'End Sub


End Class
