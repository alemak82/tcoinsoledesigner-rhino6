Imports System.IO
Imports ORM
Imports ORM.DataBindingHelper


Public Class LibraryManager





  Public Enum eDirectoryLibrary
    addiction
    ruler
    tcoProfile
    last
    deformationTable
    'template - gestione separata perchè ha sottodirectory
  End Enum


  Private Const MAIN_LIBRARY_DIRECTORY As String = "Library"
  Private Const ADDICTION_LIBRARY_DIRECTORY As String = "Addiction"
  Private Const RULER_LIBRARY_DIRECTORY As String = "Ruler"
  Private Const TCO_PROFILE_LIBRARY_DIRECTORY As String = "TcoProfile"
  Private Const LAST_LIBRARY_DIRECTORY As String = "Last"
  Private Const DEFORM_TABLE_LIBRARY_DIRECTORY As String = "DeformationTable"

  Private Const TEMPLATE_LIBRARY_DIRECTORY As String = "Template"
  'I template sono concettualmente (e anche come persistenza) divisi in 2 tipi:
  'Quelli preinstallati e non cancellabili gestiti da Duna
  'Quelli custom gestiti dall'utente
  Private Const STATIC_TEMPLATE_3D_SUBDIRECTORY As String = "Static"
  Private Const CUSTOM_TEMPLATE_3D_SUBDIRECTORY As String = "Custom"
  Public Const TEMPLATE_3D_EXTENSION As String = ".igs"
  Public Const DEFORM_TABLE_EXTENSION As String = ".csv"


  Public Shared Function GetDirectory(ByVal directoryType As eDirectoryLibrary) As String
    Dim baseDir As String = Path.Combine(My.Application.Info.DirectoryPath, MAIN_LIBRARY_DIRECTORY)
    Dim subDir As String = ""
    Select Case directoryType
      Case eDirectoryLibrary.addiction
        subDir = ADDICTION_LIBRARY_DIRECTORY
      Case eDirectoryLibrary.ruler
        subDir = RULER_LIBRARY_DIRECTORY
      Case eDirectoryLibrary.tcoProfile
        subDir = TCO_PROFILE_LIBRARY_DIRECTORY
      Case eDirectoryLibrary.last
        subDir = LAST_LIBRARY_DIRECTORY
      Case eDirectoryLibrary.deformationTable
        subDir = DEFORM_TABLE_LIBRARY_DIRECTORY
      Case Else
        subDir = ""
    End Select
    Return Path.Combine(baseDir, subDir)
  End Function



  ''' <summary>
  ''' La directory di lettura/scrittura dipende dal tipo di licenza dell'utente associato al template
  ''' </summary>
  ''' <param name="userID">In caso di lettura è l'id dell'utente che ha creato il template, in scrittura dell'utente corrente</param>
  ''' <returns></returns>
  Public Shared Function TemplateDirectoryPath(ByVal userID As Integer) As String
    Dim baseDir As String = Path.Combine(Path.Combine(My.Application.Info.DirectoryPath, MAIN_LIBRARY_DIRECTORY), TEMPLATE_LIBRARY_DIRECTORY)
    Dim dataSet As IdDataSet = New IdDataSet
    FillUserTable(dataSet)
    If dataSet.User.FindByID(userID).Admin Then
      Return Path.Combine(baseDir, STATIC_TEMPLATE_3D_SUBDIRECTORY)
    Else
      Return Path.Combine(baseDir, CUSTOM_TEMPLATE_3D_SUBDIRECTORY)
    End If
  End Function


  ''' <summary>
  ''' Il percorso di scrittura dipende dal tipo di licenza
  ''' </summary>
  ''' <param name="userID"></param>
  ''' <param name="filename"></param>
  ''' <returns></returns>
  Public Shared Function TemplateFilePath(ByVal userID As Integer, ByVal filename As String) As String
    Return Path.Combine(TemplateDirectoryPath(userID), filename & TEMPLATE_3D_EXTENSION)
  End Function


  Public Shared Function GetDeformTableFiles() As IEnumerable(Of FileInfo)    
    Dim dir As New DirectoryInfo(GetDirectory(LibraryManager.eDirectoryLibrary.deformationTable) )    
    Return dir.GetFiles().Where(Function(x) x.FullName.Contains("deformation table")).ToList()
  End Function

End Class
