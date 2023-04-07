Imports RMA.OpenNURBS
Imports RMA.Rhino

'***********************************************************************************************************
'*** 06-12-2008 ROBERTO RAFFAELI: Creata questa classe per gestire la selezione di un punto in superfice ***
'***********************************************************************************************************

Public Class RhGetObject
    Inherits MRhinoGetObject

    'Usare questa funzione a posto di MRhinoGetPoint::Point() per selezionare in superficie
    Public Function FrontPoint() As On3dPoint
        Dim getZBuffer As New MRhinoZBuffer
        Dim zValue As Double = getZBuffer.ZValue(Me.Point2d.X, Me.Point2d.Y)
        getZBuffer.Dispose()
        If zValue = 1 Then Return Nothing
        Dim puntoWorld As New On3dPoint(Me.Point2d.X, Me.Point2d.Y, zValue)
        Dim viewport As IOnViewport = RhUtil.RhinoApp.ActiveView.ActiveViewport.VP
        Dim xForm As New OnXform()
        viewport.GetXform(IOn.coordinate_system.screen_cs, IOn.coordinate_system.world_cs, xForm)
        puntoWorld.Transform(xForm)
        xForm.Dispose()
        Return puntoWorld
    End Function

End Class


Public Class RhGetPoint
    Inherits MRhinoGetPoint

    'Usare questa funzione a posto di MRhinoGetPoint::Point() per selezionare in superficie
    Public Function FrontPoint() As On3dPoint
        Dim getZBuffer As New MRhinoZBuffer
        Dim zValue As Double = getZBuffer.ZValue(Me.Point2d.X, Me.Point2d.Y)
        getZBuffer.Dispose()
        If zValue = 1 Then Return Nothing
        Dim puntoWorld As New On3dPoint(Me.Point2d.X, Me.Point2d.Y, zValue)
        Dim viewport As IOnViewport = RhUtil.RhinoApp.ActiveView.ActiveViewport.VP
        Dim xForm As New OnXform()
        viewport.GetXform(IOn.coordinate_system.screen_cs, IOn.coordinate_system.world_cs, xForm)
        puntoWorld.Transform(xForm)
        xForm.Dispose()
        Return puntoWorld
    End Function

End Class