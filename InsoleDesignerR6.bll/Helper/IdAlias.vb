Imports RhinoUtils


'****************************************************************
'*** Classe creata per compatibilità con vecchia impostazione ***
'****************************************************************


Public Class IdAlias
    Inherits RhAlias


    Public Shared Function Element3dManager() As IdElement3dManager
        Return IdElement3dManager.GetInstance()
    End Function

    Public Shared Function LanguageManager() As IdLanguageManager
        Return IdLanguageManager.GetInstance()
    End Function


End Class
