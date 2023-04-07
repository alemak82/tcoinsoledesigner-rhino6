Imports System.IO
Imports RMA.OpenNURBS
Imports InsoleDesigner.bll.IdElement3dManager



Public MustInherit Class AbstractProjectableAddiction
    Inherits IdAddiction


#Region " ENUM "

    Public Enum eSuperiorCurve
        longitudinal
        trasversal
        both
    End Enum

#End Region


#Region " Constructor "


    Public Sub New(ByVal side As eSide, ByVal type As eAddictionType, ByVal model As eAddictionModel, ByVal size As eAddictionSize)
        MyBase.New(side, type, model, size)
    End Sub


#End Region


#Region " Funzioni CAD "


    Public Overloads Function GetSuperiorCurvesId(ByVal superiorCurve As eSuperiorCurve) As List(Of Guid)
        Select Case superiorCurve
            Case eSuperiorCurve.longitudinal
                Return GetLongitudinalCurves()
            Case eSuperiorCurve.trasversal
                Return GetTrasversalCurves()
            Case eSuperiorCurve.both
                Return MyBase.GetSuperiorCurvesId()
            Case Else
                Return Nothing
        End Select
    End Function

    ''' <summary>
    ''' Le curve longitudinali hanno gli estremi sulle curve front e retro
    ''' </summary>
    ''' <returns>La chiave è la coordinata X del punto coincidente con la curva di base front</returns>
    Public Function GetLongitudinalCurves() As List(Of Guid)
        Dim result As New List(Of Guid)
        For Each uuid As Guid In Me.GetSuperiorCurvesId()
            If IsCurveLongitudinal(uuid) Then result.Add(uuid)
        Next
        Return result
    End Function

    ''' <summary>
    ''' Le curve trasversali hanno gli estremi sulle curve laterali
    ''' </summary>
    ''' <returns>La chiave è la coordinata Y del punto coincidente con le curve laterali</returns>
    Public Function GetTrasversalCurves() As List(Of Guid)
        Dim result As New List(Of Guid)
        For Each uuid As Guid In Me.GetSuperiorCurvesId()
            If Not IsCurveLongitudinal(uuid) Then result.Add(uuid)
        Next
        Return result
    End Function


    Public MustOverride Function IsCurveLongitudinal(ByVal uuid As Guid) As Boolean

   


#End Region


#Region " METODI IMPLEMENTATI "


    Public Overrides Sub DeleteBlendSrf()        
    End Sub

    Public Overrides Function IsBlendSrfInDocument() As Boolean
        Return False
    End Function

    Public Overrides Sub SelectBlendSrf()       
    End Sub



#End Region


End Class
