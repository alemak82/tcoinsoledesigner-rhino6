Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting



<TestClass()> Public Class IdPatientTest


    ''' <summary>
    ''' Output in formato breve del paziente
    ''' </summary>
    <TestMethod()> Public Sub GetPatientStringTest()
        Dim dateOfBirth As New Date(1982, 5, 18)
        Dim message As String = IdPatient.GetString("Alessandro", "Macchione", dateOfBirth)
        Assert.AreEqual("Alessandro Macchione 18/05/1982", message)
    End Sub


End Class