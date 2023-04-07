Imports System.IO
Imports CommonsUtils.Security
Imports ORM.IdDataSet
Imports ORM.IdDataSetTableAdapters
Imports ORM.DataBindingHelper
Imports System.Reflection
Imports System.Threading
Imports System.Data.OleDb


Public Class DbHelper


#Region " Field "

    Private Shared mInstance As DbHelper
    Private mLanguageTable As New LanguageDataTable

#End Region


#Region " Constructor "


    ''' <summary>
    ''' Inizializzazione campi privati
    ''' </summary>
    Private Sub New()
        'DEVE ESERE LA PRIMA RIGA DI CODICE DEL COSTRUTTORE - Imposto la directory superiore - necessario per sviluppo plugin
        SetDataDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))

        'Carico la tabella delle traduzioni nel dataset
        FillLanguageTable(mLanguageTable)
    End Sub


    Public Shared Function GetInstance() As DbHelper
        If mInstance Is Nothing Then
            mInstance = New DbHelper()
        End If
        Return mInstance
    End Function


#End Region


#Region " DB path "


    Public Function GetDbSubPath() As String
        'Adapter di una tabella a caso (ma che rimarrà sicuramente)
        Dim sizeTableAdapter As New SizeTableAdapter
        Return sizeTableAdapter.Connection.DataSource.Replace("|DataDirectory|", "")
    End Function


    ''' <summary>
    ''' Imposta la directory superiore da cui viene concatenata DataSource (chiamata dal costruttore)
    ''' Questa impostazione non è necessaria se l'applicazione eseguita(.exe) è un progetto della soluzione
    ''' E' essenziale se si sta sviluppando un plugin(TCO Insole Designer) perchè altrimenti la directory principale è quella dell'eseguibile(Rhinoceros)
    ''' </summary>
    ''' <param name="baseDirectory"></param>
    ''' <remarks></remarks>
    Public Sub SetDataDirectory(ByVal baseDirectory As String)
        AppDomain.CurrentDomain.SetData("DataDirectory", baseDirectory)
        Debug.Print(AppDomain.CurrentDomain.GetData("DataDirectory").ToString)
    End Sub


    ''' <summary>
    ''' Controlla che il file database sia disponibile localmente per il progetto attualmente in eseguzione
    ''' </summary>
    ''' <returns></returns>
    Public Function DatabaseFileExist() As Boolean
        If AppDomain.CurrentDomain Is Nothing Then Return False
        If AppDomain.CurrentDomain.GetData("DataDirectory") Is Nothing Then Return False

        'Adapter di una tabella a caso (ma che rimarrà sicuramente)
        Dim sizeTableAdapter As New SizeTableAdapter

        'Controllo esistenza file Access
        If sizeTableAdapter.Connection.DataSource.Contains("|DataDirectory|") Then
            Dim subPath As String = sizeTableAdapter.Connection.DataSource.Replace("|DataDirectory|", "")
            If Not File.Exists(AppDomain.CurrentDomain.GetData("DataDirectory").ToString & subPath) Then Return False
        End If

        sizeTableAdapter.Dispose()
        Return True
    End Function


    Public Function DatabaseFileExist(ByVal parentDirectory As String) As Boolean
        'Adapter di una tabella a caso
        Dim sizeTableAdapter As New SizeTableAdapter

        'Controllo esistenza file Access
        If sizeTableAdapter.Connection.DataSource.Contains("|DataDirectory|") Then
            Dim subPath As String = sizeTableAdapter.Connection.DataSource.Replace("|DataDirectory|", "")
            If Not File.Exists(parentDirectory & subPath) Then Return False
        End If

        sizeTableAdapter.Dispose()
        Return True
    End Function


#End Region


#Region " Authentication - per sicurezza queste funzioni lavorano tutte direttamente du DB senza usare il dataset(cache) "


    ''' <summary>
    ''' In fase di autenticazione confronto l'hash del seriale e se l'account è attivo
    ''' </summary>
    ''' <param name="serialCode"></param>
    ''' <returns></returns>
    Public Function Authenticate(ByVal serialCode As String, ByRef hash As String) As Boolean
        If String.IsNullOrEmpty(serialCode) Then Return False
        'Creo hash
        Dim hashSerialCode As String = Cryptograph.CreateHash(serialCode)
        If String.IsNullOrEmpty(hashSerialCode) Then Return False
        'Se l'autenticazione è andata a buon fine ritorno l'hash per memorizzarlo
        If AutenticateHashSerial(hashSerialCode) Then
            hash = hashSerialCode
            Return True
        End If
        Return False
    End Function


    Public Function AutenticateHashSerial(ByVal hashSerialCode As String) As Boolean
        Try
            'Confronto
            Dim userAdapter As New UserTableAdapter
            For Each row As UserRow In userAdapter.GetData()
                'Confronto hash e verifico che l'account sia assegnato
                If row.HashSerial = hashSerialCode And row.Used Then
                    'Controllo data di scadenza
                    Return (Date.Compare(row.ExpirationDate, Date.Now) >= 0)
                End If
            Next
            Return False
        Catch ex As Exception
            Debug.Print("Errore nell'autenticazione dell'utente" & vbCrLf & vbCrLf & ex.Message)
            Throw New Exception(ex.Message)
        End Try
    End Function


    'Private Function IsAdminUser(ByVal serialCode As String) As Boolean
    '    If Not Authenticate(serialCode, "") Then Return False
    '    'Creo hash
    '    Dim hashSerialCode As String = UtPassword.CreateHash(serialCode)
    '    If String.IsNullOrEmpty(hashSerialCode) Then Return False
    '    Return IsAdminHashSerial(hashSerialCode)
    'End Function


    Public Function IsAdminHashSerial(ByVal hashSerialCode As String) As Boolean
        If Not AutenticateHashSerial(hashSerialCode) Then Return False
        Try
            'Confronto
            Dim userAdapter As New UserTableAdapter
            For Each row As UserRow In userAdapter.GetData()
                If row.HashSerial = hashSerialCode Then Return (row.Admin)
            Next
            Return False
        Catch ex As Exception
            Debug.Print("Errore nella verifica dei privilegi dell'utente" & vbCrLf & vbCrLf & ex.Message)
            Throw New Exception(ex.Message)
        End Try
    End Function


    Public Function GetUserId(ByVal hashSerialCode As String) As Integer
        If Not AutenticateHashSerial(hashSerialCode) Then Return -1
        Try
            'Confronto
            Dim userAdapter As New UserTableAdapter
            For Each row As UserRow In userAdapter.GetData()
                If row.HashSerial = hashSerialCode Then Return (row.ID)
            Next
            Return -1
        Catch ex As Exception
            Debug.Print("Errore nella ricerca dell'identificativo utente" & vbCrLf & vbCrLf & ex.Message)
            Throw New Exception(ex.Message)
        End Try
    End Function


#End Region


#Region " Traduzioni "


    'Metodo per aggiornare la cultura corrente in modo da rimanere allineati con thread principale
    'Non usato perchè imposto direttamente il "Thread.CurrentThread.CurrentCulture"
    Public Sub UpdateCulture(ByVal cultureName As String)
        My.Application.ChangeCulture(cultureName)
    End Sub

    ''Per rendere la funzione più robusta oltre alla cultura corrente ritorna anche inglese e italiano
    Private Function GetCultureColumn() As String()
        'Return My.Application.Culture.ThreeLetterISOLanguageName
        Dim current As String = Thread.CurrentThread.CurrentCulture.ThreeLetterISOLanguageName.ToUpper()
        Return New String() {current, "ENG", "ITA"}
    End Function


    Public Function Translate(ByVal idLanguage As String) As String
        If String.IsNullOrEmpty(idLanguage) Then Return idLanguage
        Try
            Dim languageAdapter As New LanguageTableAdapter
            'Trovo la riga corrispondente
            Dim row As LanguageRow = mLanguageTable.FindByID(idLanguage)
            If row IsNot Nothing Then
                'Trovo la traduzione migliore possibile
                For Each cultureColumn As String In GetCultureColumn()
                    If row.Item(cultureColumn) IsNot Nothing Then
                        Return row.Item(cultureColumn).ToString()
                    End If
                Next
            End If
            languageAdapter.Dispose()
            'Se non trovo niente ritorno il parametro passato (discutibile)
            Return idLanguage
        Catch ex As Exception
            Throw New Exception(ex.Message)
        End Try
    End Function


#End Region


#Region " Patient "


    Public Function ValidateNewPatient(name As String, surname As String, dateOfBirth As Date, ByRef patientTable As PatientDataTable) As Boolean
        For Each patient As PatientRow In patientTable.Rows
            If patient.Name = name And patient.Surname = surname And patient.DateOfBirth = dateOfBirth Then Return False
        Next
        Return True
    End Function


    ''' <summary>
    ''' Scrive il paziente sul DB e assegna l'ID
    ''' </summary>
    Public Function InsertPatientAndSetId(ByVal name As String, ByVal surname As String, ByVal dateOfBirth As Date, ByRef newPatientId As Integer) As Boolean
        Dim patientAdapter As New PatientTableAdapter
        Dim patientTable As PatientDataTable = patientAdapter.GetData
        If Not DbHelper.GetInstance.ValidateNewPatient(name, surname, dateOfBirth, patientTable) Then Return False
        Dim connection As New OleDbConnection(patientAdapter.Connection.ConnectionString)
        Dim cmd As OleDbCommand = patientAdapter.Adapter.InsertCommand
        Try
            cmd.Parameters.Item(patientTable.NameColumn.ColumnName).Value = name
            cmd.Parameters.Item(patientTable.SurnameColumn.ColumnName).Value = surname
            cmd.Parameters.Item(patientTable.DateOfBirthColumn.ColumnName).Value = dateOfBirth
            cmd.Connection = connection
            connection.Open()
            cmd.ExecuteNonQuery()
            cmd.Parameters.Clear()
            cmd.CommandText = "Select @@Identity"
            newPatientId = CInt(cmd.ExecuteScalar())
        Catch retrieveSymbolIndexException As SqlClient.SqlException
            Console.WriteLine(retrieveSymbolIndexException.ToString())
            Return False
        Catch ex2 As Exception
            Console.WriteLine("Error is : " + ex2.StackTrace)
            Return False
        Finally
            If connection.State <> ConnectionState.Closed Then connection.Close()
            If cmd IsNot Nothing Then cmd.Dispose()
            If connection IsNot Nothing Then connection.Dispose()
        End Try
        patientAdapter.Dispose()
        patientTable.Dispose()
        Return True
    End Function


#End Region


#Region " Utils "


    Public Sub TemplateAdapterOpenConnection(ByRef adapter As TemplateTableAdapter)
        If adapter.Connection.State <> ConnectionState.Open Then adapter.Connection.Open()
    End Sub

    Public Sub TemplateAdapterCloseConnection(ByRef adapter As TemplateTableAdapter)
        If adapter.Connection.State <> ConnectionState.Closed Then adapter.Connection.Close()
    End Sub


#End Region


End Class
