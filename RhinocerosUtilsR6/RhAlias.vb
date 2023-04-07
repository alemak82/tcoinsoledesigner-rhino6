Imports Rhino
Imports RMA.Rhino


'****************************************************************
'*** Classe creata per compatibilità con vecchia impostazione ***
'****************************************************************


Public Class RhAlias

    ''' <summary>
    ''' VERSIONE ORIGINALE
    ''' </summary>
    Public Shared Function Doc() As MRhinoDoc 
        Return RhUtil.RhinoApp.ActiveDoc        
    End Function
    ''' <summary>
    ''' VERSIONE CON RhinoCommons
    ''' </summary>
    Public Shared Function Doc2() As RhinoDoc        
        Return RhinoDoc.ActiveDoc
    End Function


    Public Shared Function App() As MRhinoApp
        Return RhUtil.RhinoApp
    End Function

  
    Public Shared Function FormManager() As RhFormNonModalManager
        Return RhFormNonModalManager.GetInstance()
    End Function  


End Class
