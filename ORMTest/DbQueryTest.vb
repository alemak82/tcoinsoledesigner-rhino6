Imports System.Text
Imports System.IO
Imports System.Reflection
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports ORM
Imports ORM.IdDataSet
Imports ORM.IdDataSetTableAdapters
Imports System.Threading
Imports System.Globalization
Imports ORM.DataBindingHelper




<TestClass()> Public Class DbQueryTest

    Private testContextInstance As TestContext


    ''SE DEVO TESTARE UN PARTICOLARE DB BASTA SOVRASCRIVERLO NELLA CARTELLA "DbToTest\DB" E IMPOSTARE UseDbToTest=True
    ''ALTRIMENTI SI USA UNA COPIA DEL database ORIGINALE IN PRESENTE NEL PROGETTO "ORM"
    Private UseDbToTest As Boolean = False
    Private Const DB_TO_TEST As String = "\DbToTest"

    Private Const INNOVAME_LAB_USER_ID As Integer = 2
    Private Const TEST_USER_ID As Integer = 6

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

    ' È possibile utilizzare i seguenti attributi aggiuntivi per la scrittura dei test:
    '
    ' Utilizzare ClassInitialize per eseguire il codice prima di eseguire il primo test della classe
    '<ClassInitialize()> Public Shared Sub MyClassInitialize(ByVal testContext As TestContext)
    'End Sub
    '
    ' Utilizzare ClassCleanup per eseguire il codice dopo l'esecuzione di tutti i test della classe
    ' <ClassCleanup()> Public Shared Sub MyClassCleanup()
    ' End Sub


    ' Utilizzare TestInitialize per eseguire il codice prima di eseguire ciascun test
    <TestInitialize()> Public Sub MyTestInitialize()

        If UseDbToTest Then
            DbHelper.GetInstance.SetDataDirectory(My.Application.Info.DirectoryPath & DB_TO_TEST)
        Else
            DbHelper.GetInstance.SetDataDirectory(My.Application.Info.DirectoryPath)
        End If

    End Sub


    ' Utilizzare TestCleanup per eseguire il codice dopo l'esecuzione di ciascun test
    ' <TestCleanup()> Public Sub MyTestCleanup()
    ' End Sub


#End Region


#Region " Autenticazione "


    <TestMethod()> Public Sub AutenticateNothingTest()
        Assert.IsFalse(DbHelper.GetInstance.Authenticate(Nothing, Nothing))
    End Sub

    <TestMethod()> Public Sub AutenticateEmptyStringTest()
        Assert.IsFalse(DbHelper.GetInstance.Authenticate("", ""))
    End Sub

    <TestMethod()> Public Sub AutenticateInnovaMeLabTest()
        Dim innovaMeLabSerial As String = "34f8bd51-69f9-466e-a8b4-e1841356b549"
        Dim innovaMeLabHashSerial As String = "A4A7E9559B2B90BBD3F0BE392808019B9D7973B8"
        'L'autenticazione con l'hash deve fallire
        Assert.IsFalse(DbHelper.GetInstance.Authenticate(innovaMeLabHashSerial, ""))
        'Autenticazione
        Dim hash As String = ""
        Assert.IsTrue(DbHelper.GetInstance.Authenticate(innovaMeLabSerial, hash))
        Assert.AreEqual(hash, innovaMeLabHashSerial)
        Assert.IsTrue(DbHelper.GetInstance.AutenticateHashSerial(innovaMeLabHashSerial))
        'Controllo privilegi
        Assert.IsTrue(DbHelper.GetInstance.IsAdminHashSerial(innovaMeLabHashSerial))
    End Sub



    <TestMethod()> Public Sub AutenticateDunaGuidTest()
        Dim dunaSerial As String = "69e07c01-1420-4bfe-8174-7b76b7862919"
        Dim dunaHashSerial As String = "52BEB4C36A6B80673354E2730ECF1190B29AA09B"
        'L'autenticazione con l'hash deve fallire
        Assert.IsFalse(DbHelper.GetInstance.Authenticate(dunaHashSerial, ""))
        'Autenticazione
        Dim hash As String = ""
        Assert.IsTrue(DbHelper.GetInstance.Authenticate(dunaSerial, hash))
        Assert.AreEqual(hash, dunaHashSerial)
        Assert.IsTrue(DbHelper.GetInstance.AutenticateHashSerial(dunaHashSerial))
        'Controllo privilegi
        Assert.IsTrue(DbHelper.GetInstance.IsAdminHashSerial(dunaHashSerial))
    End Sub



    <TestMethod()> Public Sub AutenticateTestUserTest()
        Dim testUserSerial As String = "842ab08a-d1b2-4139-9f2c-d8df515768ec"
        Dim testUserHashSerial As String = "00F9436E10C81004F2266E3D60CA371A78734455"
        'L'autenticazione con l'hash deve fallire
        Assert.IsFalse(DbHelper.GetInstance.Authenticate(testUserHashSerial, ""))
        'Autenticazione
        Dim hash As String = ""
        Assert.IsTrue(DbHelper.GetInstance.Authenticate(testUserSerial, hash))
        Assert.AreEqual(hash, testUserHashSerial)
        Assert.IsTrue(DbHelper.GetInstance.AutenticateHashSerial(testUserHashSerial))
        'Controllo privilegi
        Assert.IsFalse(DbHelper.GetInstance.IsAdminHashSerial(testUserHashSerial))
    End Sub


    <TestMethod()> Public Sub GetUserIdTest()
        Dim userAdapter As New UserTableAdapter
        Dim firstUser As UserRow = userAdapter.GetData.Rows(0)
        Dim userID As Integer = firstUser.ID
        Dim userHash As String = firstUser.HashSerial
        Assert.AreEqual(userID, DbHelper.GetInstance.GetUserId(userHash))
    End Sub


#End Region


#Region " CRUD general "


    <TestMethod()> Public Sub ReadSizeTableTest()
        Dim sizeTableAdapter As New SizeTableAdapter
        Try
            Assert.IsTrue(sizeTableAdapter.GetData().Rows.Count > 0)
        Catch ex As Exception
            Throw ex
            Assert.IsTrue(False)
        End Try
    End Sub


    <TestMethod()> Public Sub InsertTemplateTest()
        Dim templateAdapter As New TemplateTableAdapter
        Dim preCount As Integer = templateAdapter.GetData.Rows.Count
        Dim result As Integer = templateAdapter.Insert(My.Application.Info.Title & " " & preCount, 2, 25, 2, 2, 2, INNOVAME_LAB_USER_ID, "", "")
        Assert.AreEqual(1, result)
    End Sub

    <TestMethod()> Public Sub InsertTemplateWithPathologyTest()
        Dim templateAdapter As New TemplateTableAdapter
        Dim templateId As String = My.Application.Info.Title & " " & templateAdapter.GetData.Rows.Count
        Dim result As Integer = templateAdapter.Insert(templateId, 3, 21, 2, 2, 2, INNOVAME_LAB_USER_ID, "", "")
        Assert.AreEqual(1, result)
        Dim pathologyAdapter As New PathologyTableAdapter
        Dim firstPathologyRow As PathologyRow = pathologyAdapter.GetData.Rows(0)
        Dim templatePathologyAdapter As New Template_PathologyTableAdapter
        result = templatePathologyAdapter.Insert(templateId, firstPathologyRow.ID)
        Assert.AreEqual(1, result)
    End Sub

    <TestMethod()> Public Sub DeleteTestTemplateTest()
        Dim templateAdapter As New TemplateTableAdapter
        'Prima aggiungo template per essere sicuri che ce ne siano da rimuovere
        Dim preCount As Integer = templateAdapter.GetData.Rows.Count
        Dim result As Integer = templateAdapter.Insert(My.Application.Info.Title & " " & preCount, 2, 25, 2, 2, 2, INNOVAME_LAB_USER_ID, "", "")
        Assert.AreEqual(1, result)
        result = templateAdapter.Insert(My.Application.Info.Title & " " & preCount + 1, 3, 21, 2, 2, 2, TEST_USER_ID, "", "")
        Assert.AreEqual(1, result)
        Dim templateTable As New TemplateDataTable
        Dim userAdapter As New UserTableAdapter
        For Each row As TemplateRow In templateAdapter.GetData.Rows
            If row.UserID = INNOVAME_LAB_USER_ID Or row.UserID = TEST_USER_ID Then
                'Controllo che i valori dei nomi dei file 3D non siano nulli
                Dim file3DLeft As String = Nothing
                Dim file3DRight As String = Nothing
                If Not DBNull.Value.Equals(row.Item(templateTable.File3DLeftColumn.ColumnName)) Then file3DLeft = row.File3DLeft
                If Not DBNull.Value.Equals(row.Item(templateTable.File3DRightColumn.ColumnName)) Then file3DRight = row.File3DRight
                'Query
                result = templateAdapter.Delete(row.ID, row.LastModelID, row.SizeID, row.ThicknessID, row.BottomTypeID, _
                                           row.VaultID, row.UserID, file3DLeft, file3DRight)
                Assert.AreEqual(1, result)
            End If
        Next
    End Sub


    ''' <summary>
    ''' Template con tutte le possibili combinazioni(anche non ammissibili a livello di filtri) accettate dal DB
    ''' Utile per testare ordinamento dei template
    ''' </summary>
    <TestMethod()>
    Public Sub WriteAllCombinationTemplateTest()

        Dim ds As New IdDataSet
        FillLastModelTable(ds)
        FillSizeTable(ds)
        FillThicknessTable(ds)
        FillBottomTypeTable(ds)
        FillVaultTable(ds)
        FillTemplateTable(ds)
        Dim templateAdapter As New TemplateTableAdapter

        'Pulisco tabella
        For Each t As TemplateRow In ds.Template
            templateAdapter.Delete(t.ID, t.LastModelID, t.SizeID, t.ThicknessID, t.BottomTypeID, t.VaultID, t.UserID, t.File3DLeft, t.File3DRight)
        Next
        Assert.AreEqual(0, templateAdapter.GetData.Rows.Count)
        templateAdapter.Update(ds.Template)

        'Ottimizzazione
        DbHelper.GetInstance().TemplateAdapterOpenConnection(templateAdapter)

        'Inserimento
        Dim counter As Integer = 1
        For i As Integer = 1 To ds.LastModel.Rows.Count
            For j As Integer = 1 To ds.Size.Rows.Count
                For k As Integer = 1 To ds.Thickness.Rows.Count
                    For v As Integer = 1 To ds.BottomType.Rows.Count
                        For w As Integer = 1 To ds.Vault.Rows.Count

                            Dim templateID As String = counter  '"test_" & counter
                            Dim lastModelId As Integer = ds.LastModel.Item(i Mod ds.LastModel.Rows.Count).ID
                            Dim sizeId As Integer = ds.Size.Item(j Mod ds.Size.Rows.Count).ID
                            Dim thicknessId As Integer = ds.Thickness.Item(k Mod ds.Thickness.Rows.Count).ID
                            Dim bottomId As Integer = ds.BottomType.Item(v Mod ds.BottomType.Rows.Count).ID
                            Dim vaultId As Integer = ds.Vault.Item(w Mod ds.Vault.Rows.Count).ID
                            templateAdapter.Insert(templateID, lastModelId, sizeId, thicknessId, bottomId, vaultId, 2, "", "")
                            counter += 1
                        Next
                    Next
                Next
            Next
        Next

        'Ottimizzazione
        DbHelper.GetInstance().TemplateAdapterCloseConnection(templateAdapter)

        Dim combination As Integer = ds.LastModel.Rows.Count * ds.Size.Rows.Count * ds.Thickness.Rows.Count * ds.BottomType.Rows.Count * ds.Vault.Rows.Count
        Assert.AreEqual(combination, templateAdapter.GetData.Rows.Count)
    End Sub

#End Region


#Region " Traduzioni "


    <TestMethod()> Public Sub TranslateTest()
        Thread.CurrentThread.CurrentCulture = New CultureInfo("it-IT")
        Assert.AreEqual("Uomo", DbHelper.GetInstance.Translate("Man"))
        Thread.CurrentThread.CurrentCulture = New CultureInfo("en")
        Assert.AreEqual("Man", DbHelper.GetInstance.Translate("Man"))
    End Sub

    <TestMethod()> Public Sub TranslateErrorTest()
        Assert.AreEqual(Nothing, DbHelper.GetInstance.Translate(Nothing))
    End Sub

    <TestMethod()> Public Sub TranslateError2Test()
        Assert.AreEqual("", DbHelper.GetInstance.Translate(""))
    End Sub

    <TestMethod()> Public Sub TranslateError3Test()
        Assert.AreEqual("aaaa", DbHelper.GetInstance.Translate("aaaa"))
    End Sub

#End Region


#Region " Pathology "


    <TestMethod()> Public Sub PathologyExistTest()
        Dim pathologyAdapter As New PathologyTableAdapter
        Dim pathologiesID As New List(Of Integer)
        For Each row As PathologyRow In pathologyAdapter.GetData.Rows
            pathologiesID.Add(row.ID)
        Next
        For Each pathologyID As Integer In pathologiesID
            Assert.IsNotNull(pathologyAdapter.GetData.FindByID(pathologyID))
        Next
        Assert.IsNull(pathologyAdapter.GetData.FindByID(-1))
    End Sub


#End Region


#Region " Patient "

    ''' <summary>
    ''' Inserimento paziente
    ''' </summary>
    <TestMethod()> Public Sub InsertPatientTest()
        Dim patientAdapter As New PatientTableAdapter
        Dim dateOfBirth As New Date(1982, 5, 18)
        Assert.AreEqual(1, patientAdapter.Insert("Alessandro", "Macchione", dateOfBirth))
    End Sub

    ''' <summary>
    ''' Inserimento paziente con ritorno dell'ID assegnato
    ''' </summary>
    <TestMethod()> Public Sub InsertPatientAndSetIdTest()
        Dim patientAdapter As New PatientTableAdapter
        'Inserisco almeno un paziente
        If patientAdapter.GetData.Rows.Count = 0 Then InsertPatientTest()
        'Salvo tutti gli id prima dell'inserimento
        Dim previousIds As New List(Of Integer)
        For Each patient As PatientRow In patientAdapter.GetData.Rows
            previousIds.Add(patient.ID)
        Next
        'Inserimento e controlli
        Dim newPatientId As Integer = -1
        Assert.IsTrue(DbHelper.GetInstance.InsertPatientAndSetId("name" & Date.Now.Ticks, "surname" & Date.Now.Ticks, Date.Now.Date, newPatientId))
        Assert.AreNotEqual(-1, newPatientId)
        Assert.IsFalse(previousIds.Contains(newPatientId))
        Assert.IsNotNull(patientAdapter.GetData.FindByID(newPatientId))
    End Sub

#End Region


End Class