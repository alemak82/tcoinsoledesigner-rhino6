Imports System.Text
Imports InsoleDesigner.bll.AbstractCutoutCommons


<TestClass()>
Public Class AddictionTest

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



    <TestMethod()>
    Public Sub HorseShoeGetCurveNameTest()
        Assert.AreEqual("L1", IdHorseShoeTotalAddiction.GetCurveName(AbstractCutoutCommons.eHorseShoeCrv.L1))
        Assert.AreEqual("L2", IdHorseShoeTotalAddiction.GetCurveName(AbstractCutoutCommons.eHorseShoeCrv.L2))
        Assert.AreEqual("L3", IdHorseShoeTotalAddiction.GetCurveName(AbstractCutoutCommons.eHorseShoeCrv.L3))
        Assert.AreEqual("L4", IdHorseShoeTotalAddiction.GetCurveName(AbstractCutoutCommons.eHorseShoeCrv.L4))
        Assert.AreEqual("L1L2", IdHorseShoeTotalAddiction.GetCurveName(AbstractCutoutCommons.eHorseShoeCrv.L1L2))
        Assert.AreEqual("L1L3", IdHorseShoeTotalAddiction.GetCurveName(AbstractCutoutCommons.eHorseShoeCrv.L1L3))
        Assert.AreEqual("L2L4", IdHorseShoeTotalAddiction.GetCurveName(AbstractCutoutCommons.eHorseShoeCrv.L2L4))
        Assert.AreEqual("Unique", IdHorseShoeTotalAddiction.GetCurveName(AbstractCutoutCommons.eHorseShoeCrv.Unique))
    End Sub

    <TestMethod()>
    Public Sub HorseShoeEnumConversionTest()
        'eHorseShoeStraightCrv -> eHorseShoeCrv
        Assert.AreEqual(AbstractCutoutCommons.eHorseShoeCrv.L1, ConvertEnumCurve(AbstractCutoutCommons.eHorseShoeStraightCrv.L1))
        Assert.AreEqual(AbstractCutoutCommons.eHorseShoeCrv.L2, ConvertEnumCurve(AbstractCutoutCommons.eHorseShoeStraightCrv.L2))
        Assert.AreEqual(AbstractCutoutCommons.eHorseShoeCrv.L3, ConvertEnumCurve(AbstractCutoutCommons.eHorseShoeStraightCrv.L3))
        Assert.AreEqual(AbstractCutoutCommons.eHorseShoeCrv.L4, ConvertEnumCurve(AbstractCutoutCommons.eHorseShoeStraightCrv.L4))
        'eHorseShoeCrv -> eHorseShoeStraightCrv
        Assert.AreEqual(AbstractCutoutCommons.eHorseShoeStraightCrv.L1, ConvertEnumCurve(AbstractCutoutCommons.eHorseShoeCrv.L1))
        Assert.AreEqual(AbstractCutoutCommons.eHorseShoeStraightCrv.L2, ConvertEnumCurve(AbstractCutoutCommons.eHorseShoeCrv.L2))
        Assert.AreEqual(AbstractCutoutCommons.eHorseShoeStraightCrv.L3, ConvertEnumCurve(AbstractCutoutCommons.eHorseShoeCrv.L3))
        Assert.AreEqual(AbstractCutoutCommons.eHorseShoeStraightCrv.L4, ConvertEnumCurve(AbstractCutoutCommons.eHorseShoeCrv.L4))
        'eHorseShoeFilletCrv -> eHorseShoeCrv
        Assert.AreEqual(AbstractCutoutCommons.eHorseShoeCrv.L1L2, ConvertEnumCurve(AbstractCutoutCommons.eHorseShoeFilletCrv.L1L2))
        Assert.AreEqual(AbstractCutoutCommons.eHorseShoeCrv.L1L3, ConvertEnumCurve(AbstractCutoutCommons.eHorseShoeFilletCrv.L1L3))
        Assert.AreEqual(AbstractCutoutCommons.eHorseShoeCrv.L2L4, ConvertEnumCurve(AbstractCutoutCommons.eHorseShoeFilletCrv.L2L4))
        'eHorseShoeCrv -> eHorseShoeFilletCrv
        Assert.AreEqual(AbstractCutoutCommons.eHorseShoeFilletCrv.L1L2, ConvertEnumCrv(AbstractCutoutCommons.eHorseShoeCrv.L1L2))
        Assert.AreEqual(AbstractCutoutCommons.eHorseShoeFilletCrv.L1L3, ConvertEnumCrv(AbstractCutoutCommons.eHorseShoeCrv.L1L3))
        Assert.AreEqual(AbstractCutoutCommons.eHorseShoeFilletCrv.L2L4, ConvertEnumCrv(AbstractCutoutCommons.eHorseShoeCrv.L2L4))
    End Sub



End Class
