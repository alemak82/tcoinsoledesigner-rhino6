Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports ORM


<TestClass()> Public Class DbFileTest


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
    ' <TestInitialize()> Public Sub MyTestInitialize()
    ' End Sub
    '
    ' Utilizzare TestCleanup per eseguire il codice dopo l'esecuzione di ciascun test
    ' <TestCleanup()> Public Sub MyTestCleanup()
    ' End Sub
    '
#End Region



    ''' <summary>
    ''' Se la copia del file è ben impostata sarà disponibile per tutti i progetti che hanno ORM come diendenza
    ''' </summary>
    <TestMethod()> Public Sub DatabaseFileExistTest()
        Assert.IsTrue(DbHelper.GetInstance.DatabaseFileExist(My.Application.Info.DirectoryPath))
    End Sub

    ''' <summary>
    ''' Se la copia del file è ben impostata sarà disponibile AUTOMATICAMENTE per tutti i progetti che hanno ORM come diendenza
    ''' </summary>
    <TestMethod()> Public Sub DatabaseFileExistTest2()
        Assert.IsTrue(DbHelper.GetInstance.DatabaseFileExist())
    End Sub




End Class