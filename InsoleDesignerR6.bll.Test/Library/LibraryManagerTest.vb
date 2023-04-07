Imports System.Text
Imports System.IO
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports InsoleDesigner.bll.LibraryManager
Imports ORM
Imports ORM.DataBindingHelper


<TestClass()> Public Class LibraryManagerTest

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
    '<ClassInitialize()> Public Shared Sub MyClassInitialize(ByVal testContext As TestContext)
    'End Sub
    '
    ' Utilizzare ClassCleanup per eseguire il codice dopo l'esecuzione di tutti i test della classe
    ' <ClassCleanup()> Public Shared Sub MyClassCleanup()
    ' End Sub
    '
    ' Utilizzare TestInitialize per eseguire il codice prima di eseguire ciascun test
    '<TestInitialize()> Public Sub MyTestInitialize()
    'End Sub
    '
    ' Utilizzare TestCleanup per eseguire il codice dopo l'esecuzione di ciascun test
    ' <TestCleanup()> Public Sub MyTestCleanup()
    ' End Sub
    '
#End Region


    <TestMethod()> Public Sub AllEnumExist()
        For Each val As Object In System.Enum.GetValues(GetType(LibraryManager.eDirectoryLibrary))
            Dim dir = LibraryManager.GetDirectory(val)
            Assert.IsTrue(Directory.Exists(dir))
            Assert.IsTrue(Directory.GetFiles(dir).Length + Directory.GetDirectories(dir).Length > 0)
        Next
    End Sub

    <TestMethod()> Public Sub TemplateExist()
        Dim dataSet As IdDataSet = New IdDataSet
        FillUserTable(dataSet)
        Dim userId = dataSet.User.FirstOrDefault(Function(u) u.Admin).ID
        Dim dir = LibraryManager.TemplateDirectoryPath(userId)
        Assert.IsTrue(Directory.Exists(dir))
        Assert.IsTrue(Directory.GetFiles(dir).Length + Directory.GetDirectories(dir).Length > 0)
        'La directory con i template custom viene creata automaticamente sulla macchina del cliente
        userId = dataSet.User.FirstOrDefault(Function(u) Not u.Admin).ID
        dir = LibraryManager.TemplateDirectoryPath(userId)
        Assert.IsFalse(Directory.Exists(dir))
    End Sub


End Class