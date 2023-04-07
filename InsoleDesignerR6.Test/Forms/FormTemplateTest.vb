Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports InsoleDesigner
Imports System.Threading
Imports System.Globalization
Imports ORM
Imports ORMTest
Imports ORM.IdDataSet
Imports ORM.IdDataSetTableAdapters


<TestClass()> Public Class FormTemplateTest

    Private testContextInstance As TestContext

    Private itaGenre As String
    Private enGenre As String

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

        Dim dataset As New IdDataSet
        Dim adapter As New GenreTableAdapter
        adapter.Fill(dataset.Genre)
        Dim langID = DirectCast(dataset.Genre.Rows.Item(0), GenreRow).LanguageID

        Dim adapter2 As New LanguageTableAdapter
        adapter2.Fill(dataset.Language)
        itaGenre = dataset.Language.FindByID(langID).ITA
        enGenre = dataset.Language.FindByID(langID).ENG

        adapter.Dispose()
        adapter2.Dispose()
    End Sub
    '
    ' Utilizzare TestCleanup per eseguire il codice dopo l'esecuzione di ciascun test
    ' <TestCleanup()> Public Sub MyTestCleanup()
    ' End Sub
    '
#End Region


    <TestMethod()> Public Sub Test0()
        Assert.IsTrue(True)
    End Sub

    '''' <summary>
    '''' Avvio form salvataggio template in italiano
    '''' </summary>
    '<TestMethod()> Public Sub FrmSaveTemplateItaTest()
    '    Thread.CurrentThread.CurrentCulture = New CultureInfo("it-IT")
    '    Dim formSaveTemplate As New FrmSaveTemplate()

    '    formSaveTemplate.Show()
    '    For Each item As Object In formSaveTemplate.cmbGenre.Items
    '        Dim val = DirectCast(DirectCast(item, System.Data.DataRowView).Row, IdDataSet.GenreRow).LanguageID 'DirectCast(item, IdDataSet.GenreRow).LanguageID
    '        If String.Equals(val, enGenre, StringComparison.OrdinalIgnoreCase) Then
    '            Assert.Fail()
    '        End If
    '        If String.Equals(val, itaGenre, StringComparison.OrdinalIgnoreCase) Then
    '            formSaveTemplate.Close()
    '            Assert.IsTrue(True)
    '            Exit Sub
    '        End If
    '    Next
    '    Assert.Fail()
    'End Sub

    '''' <summary>
    '''' Avvio form salvataggio template in inglese
    '''' </summary>
    '<TestMethod()> Public Sub FrmSaveTemplateEngTest()
    '    Thread.CurrentThread.CurrentCulture = New CultureInfo("en")
    '    Dim formSaveTemplate As New FrmSaveTemplate()

    '    formSaveTemplate.Show()
    '    For Each item As Object In formSaveTemplate.cmbGenre.Items
    '        Dim val = DirectCast(DirectCast(item, System.Data.DataRowView).Row, IdDataSet.GenreRow).LanguageID 'DirectCast(item, IdDataSet.GenreRow).LanguageID
    '        If String.Equals(val, itaGenre, StringComparison.OrdinalIgnoreCase) Then
    '            Assert.Fail()
    '        End If
    '        If String.Equals(val, enGenre, StringComparison.OrdinalIgnoreCase) Then
    '            formSaveTemplate.Close()
    '            Assert.IsTrue(True)
    '            Exit Sub
    '        End If
    '    Next
    '    Assert.Fail()
    'End Sub

    '''' <summary>
    '''' Avvio form apertura template in italiano
    '''' </summary>
    '<TestMethod()> Public Sub FrmOpenTemplateItaTest()
    '    Thread.CurrentThread.CurrentCulture = New CultureInfo("it-IT")
    '    Dim formOpenTemplate As New FrmOpenTemplate()

    '    formOpenTemplate.Show()
    '    For Each item As Object In formOpenTemplate.cmbGenre.Items
    '        Dim val = DirectCast(DirectCast(item, System.Data.DataRowView).Row, IdDataSet.GenreRow).LanguageID 'DirectCast(item, IdDataSet.GenreRow).LanguageID
    '        If String.Equals(val, enGenre, StringComparison.OrdinalIgnoreCase) Then
    '            Assert.Fail()
    '        End If
    '        If String.Equals(val, itaGenre, StringComparison.OrdinalIgnoreCase) Then
    '            formOpenTemplate.Close()
    '            Assert.IsTrue(True)
    '            Exit Sub
    '        End If
    '    Next
    '    Assert.Fail()
    'End Sub

    '''' <summary>
    '''' Avvio form apertura template in inglese
    '''' </summary>
    '<TestMethod()> Public Sub FrmOpenTemplateEngTest()
    '    Thread.CurrentThread.CurrentCulture = New CultureInfo("en")
    '    Dim formOpenTemplate As New FrmOpenTemplate()

    '    formOpenTemplate.Show()
    '    For Each item As Object In formOpenTemplate.cmbGenre.Items
    '        Dim val = DirectCast(DirectCast(item, System.Data.DataRowView).Row, IdDataSet.GenreRow).LanguageID 'DirectCast(item, IdDataSet.GenreRow).LanguageID
    '        If String.Equals(val, itaGenre, StringComparison.OrdinalIgnoreCase) Then
    '            Assert.Fail()
    '        End If
    '        If String.Equals(val, enGenre, StringComparison.OrdinalIgnoreCase) Then
    '            formOpenTemplate.Close()
    '            Assert.IsTrue(True)
    '            Exit Sub
    '        End If
    '    Next
    '    Assert.Fail()
    'End Sub

    '''' <summary>
    '''' Scrittura nel DB di template con tutte le possibili combinazioni e avvio form per apertura per testare performance datagrid(caricamento iniziale lento)
    '''' </summary>
    '<TestMethod()> Public Sub FrmOpenTemplatePerformanceTest()
    '    Dim queryTester As New DbQueryTest
    '    queryTester.WriteAllCombinationTemplateTest()
    '    MsgBox("Test per verificare le performance del datagrid")
    '    Dim formOpenTemplate As New FrmOpenTemplate()
    '    formOpenTemplate.ShowDialog()
    'End Sub

    '''' <summary>
    '''' Scrittura nel DB di alcuni template e avvio form per apertura
    '''' </summary>
    '<TestMethod()> Public Sub FrmOpenTemplateScrollTest()
    '    'Dim templateTest As New IdTemplateTest
    '    'templateTest.WriteInDbTemplateTest()
    '    ''templateTest.WriteInDbTemplateTest()
    '    ''templateTest.WriteInDbTemplateTest()
    '    ''templateTest.WriteInDbTemplateTest()
    '    ''templateTest.WriteInDbTemplateTest()
    '    ''templateTest.WriteInDbTemplateTest()
    '    ''templateTest.WriteInDbTemplateTest()
    '    ''templateTest.WriteInDbTemplateTest()
    '    ''templateTest.WriteInDbTemplateTest()
    '    ''templateTest.WriteInDbTemplateTest()
    '    ''templateTest.WriteInDbTemplateTest()
    '    'Dim formOpenTemplate As New FrmOpenTemplate()
    '    'formOpenTemplate.ShowDialog()
    'End Sub

    '<TestMethod()> Public Sub FrmOpenTemplatePatientTest()
    '    'Dim templateTest As New IdTemplateTest        
    '    'templateTest.WriteInDbTemplateTest()
    '    'Dim formOpenTemplate As New FrmOpenTemplate()
    '    'formOpenTemplate.ShowDialog()
    'End Sub

End Class