Public Class ProgressManager


  Private Shared mInstance As ProgressManager
  Private loader As FrmProgress

  
  Private Sub New()    
  End Sub


  Public Shared Function GetInstance() As ProgressManager
    If mInstance Is Nothing Then
      mInstance = New ProgressManager()
    End If
    Return mInstance
  End Function


  Public Sub Show
    #If Not DEBUG
        loader = New FrmProgress
        loader.Show()
    #End If    
  End Sub

  Public Sub Close
    #If Not DEBUG
      If loader IsNot Nothing Then loader.Dispose()
    #End If  
  End Sub


End Class
