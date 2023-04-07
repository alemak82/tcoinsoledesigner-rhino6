Imports System.Globalization
Imports System.Threading
Imports ORM
Imports ORM.IdDataSet
Imports ORM.IdDataSetTableAdapters


Public Class IdPatient


    Public Id As Integer
    Public Name As String
    Public Surname As String
    Public DateOfBirth As Date


    Public Sub New(ByVal name As String, ByVal surname As String, ByVal dateOfBirth As Date)
        Me.Name = name
        Me.Surname = surname
        Me.DateOfBirth = dateOfBirth
    End Sub



    ''' <summary>
    ''' Ritorna la stringa di rappresentazione di un paziente "Nome Cognome Data" con la data invariante rispetto lla cultura corrente
    ''' </summary>
    ''' <param name="name"></param>
    ''' <param name="surname"></param>
    ''' <param name="dateOfBirth"></param>
    ''' <returns></returns>
    Public Shared Function GetString(ByVal name As String, ByVal surname As String, ByVal dateOfBirth As Date) As String
        Dim bkCulture As CultureInfo = Thread.CurrentThread.CurrentCulture
        Thread.CurrentThread.CurrentCulture = New CultureInfo("it-IT")
        Dim result As String = name & " " & surname & " " & dateOfBirth.ToString("d")
        Thread.CurrentThread.CurrentCulture = bkCulture
        Return result
    End Function

    Public Shared Function GetString(ByVal patientRow As PatientRow) As String
        Return GetString(patientRow.Name, patientRow.Surname, patientRow.DateOfBirth)
    End Function

    Public Function GetString() As String
        Return GetString(Me.Name, Me.Surname, Me.DateOfBirth)
    End Function



End Class
