Imports System.Drawing
Imports System.Text
Imports System.IO
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports InsoleDesigner.bll.IdPressureMapUtils


<TestClass()> Public Class IdPressureMapUtilsTest

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

  <TestMethod()> Public Sub SplitPressureMapImageTest()
    Dim originalImagePath As String = My.Settings.BASE_FILE_TEST_DIR & "mappe pressione\Pedana Duna Test Valerio\Complessivo.bmp"
    Dim leftImage As Bitmap = Nothing
    Dim rightImage As Bitmap = Nothing
    Assert.IsTrue(IdPressureMapUtils.SplitPressureMapImage(originalImagePath, leftImage, rightImage))
    Assert.IsNotNull(leftImage)
    Assert.IsNotNull(rightImage)    
    'Dim desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
    'leftImage.Save(desktopPath & "\leftImage.bmp", Imaging.ImageFormat.Bmp)
    'rightImage.Save(desktopPath & "\rightImage.bmp", Imaging.ImageFormat.Bmp) 
  End Sub

  <TestMethod()> Public Sub ResizePressureMapImageTest()
    Dim originalImagePath As String = My.Settings.BASE_FILE_TEST_DIR & "mappe pressione\Pedana Duna Test Valerio\destro-centerd.bmp"
    Assert.IsTrue(File.Exists(originalImagePath))
    Dim originalImage = CType(Bitmap.FromFile(originalImagePath), Bitmap)
    Assert.IsNotNull(originalImage)
    Dim newBitmap = IdPressureMapUtils.ResizePressureMapImage(originalImage)
    Assert.IsNotNull(newBitmap)
    Dim proporzione = newBitmap.Height / newBitmap.Width
    Assert.AreEqual(proporzione, IdPressureMap.DEFAULT_BITMAP_HEIGHT/IdPressureMap.DEFAULT_BITMAP_WIDTH, 0.01)
  End Sub

        <TestMethod()> Public Sub LeggiMatricePedadaAbbaBtsTest()
        Dim side = IdElement3dManager.eSide.right
        Dim filename = "RAFFAELEIELUZZI_right.txt"
        If side = IdElement3dManager.eSide.left Then filename = "RAFFAELEIELUZZI_left.txt"
        Dim dir = My.Settings.BASE_FILE_TEST_DIR & "mappe pressione\NEW export APPA BTS"
        Dim fullPath = IO.Path.Combine(dir, filename)
        Assert.IsTrue(IO.File.Exists(fullPath))
        Dim minP As Double = Double.MaxValue
        Dim maxP As Double = Double.MinValue
        IdPressureMapUtils.LeggiMatricePedadaAbbaBTS(fullPath, minP, maxP)
        Dim maxExpected = Convert.ToDouble(IIf(side = IdElement3dManager.eSide.right, 313 , 235))
        Dim minExpected = Convert.ToDouble(1)
        Assert.AreEqual(maxExpected, maxP)
        Assert.AreEqual(minExpected, minP)
    End Sub

End Class