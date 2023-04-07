Imports RMA.Rhino
Imports System.Windows.Forms
Imports System.Drawing.Point
Imports Rhino.UI
Imports Rhino.Display


''' <summary>
''' GESTORE MOUSE - VA ISTANZIATO E ABILITATO/DISABILITAO
''' </summary>
Public Class RhMouseManager
    Inherits MouseCallback


    Public Event MouseEndDrag(ByVal e As MouseCallbackEventArgs)
    Public Event MouseUp(ByVal e As MouseCallbackEventArgs)
    Public Event MouseDown(ByVal e As MouseCallbackEventArgs)
    Public Event MouseDoubleClick(ByVal e As MouseCallbackEventArgs)
    Public Event MouseMove(ByVal e As MouseCallbackEventArgs)
    Public Event MouseEnter(ByVal view As RhinoView)
    Public Event MouseHover(ByVal view As RhinoView)
    Public Event MouseLeave(ByVal view As RhinoView)

    Public LastDownPoint As Drawing.Point
    'valore scelto empiricamente per distinguere tra fine drag e semplice click
    Public Property MinimunDrag As Decimal = 2


    Public Sub New()
        MyBase.New()
        LastDownPoint = New Drawing.Point(0, 0)
    End Sub


    Protected Overrides Sub OnMouseDown(e As Rhino.UI.MouseCallbackEventArgs)
#If DEBUG Then
        RhUtil.RhinoApp.Print("MouseManager caught OnMouseDown" + vbCrLf)
#End If
        MyBase.OnMouseDown(e)
        LastDownPoint = New Drawing.Point(e.ViewportPoint.X, e.ViewportPoint.Y)
        RaiseEvent MouseDown(e)
    End Sub


    Protected Overrides Sub OnMouseUp(e As MouseCallbackEventArgs)
#If DEBUG Then
        RhUtil.RhinoApp.Print("MouseManager caught OnMouseUp" + vbCrLf)
#End If
        MyBase.OnMouseUp(e)
        If Math.Abs(LastDownPoint.X - e.ViewportPoint.X) < MinimunDrag And Math.Abs(LastDownPoint.Y - e.ViewportPoint.Y) < MinimunDrag Then
            RaiseEvent MouseUp(e)
        Else
            RaiseEvent MouseEndDrag(e)
        End If

    End Sub


    Protected Overrides Sub OnMouseDoubleClick(e As MouseCallbackEventArgs)
#If DEBUG Then
        RhUtil.RhinoApp.Print("MouseManager caught OnMouseDoubleClick" + vbCrLf)
#End If
        MyBase.OnMouseDoubleClick(e)
        RaiseEvent MouseDoubleClick(e)
    End Sub

    Protected Overrides Sub OnMouseMove(e As MouseCallbackEventArgs)
#If DEBUG Then
        RhUtil.RhinoApp.Print("MouseManager caught OnMouseMove" + vbCrLf)
#End If
        MyBase.OnMouseMove(e)
        RaiseEvent MouseMove(e)
    End Sub

    Protected Overrides Sub OnMouseEnter(view As RhinoView)
#If DEBUG Then
        RhUtil.RhinoApp.Print("MouseManager caught OnMouseEnter" + vbCrLf)
#End If
        MyBase.OnMouseEnter(view)
        RaiseEvent MouseEnter(view)
    End Sub

    Protected Overrides Sub OnMouseHover(view As RhinoView)
#If DEBUG Then
        RhUtil.RhinoApp.Print("MouseManager caught OnMouseHover" + vbCrLf)
#End If
        MyBase.OnMouseHover(view)
        RaiseEvent MouseHover(view)
    End Sub

    Protected Overrides Sub OnMouseLeave(view As RhinoView)
#If DEBUG Then
        RhUtil.RhinoApp.Print("MouseManager caught OnMouseLeave" + vbCrLf)
#End If
        MyBase.OnMouseLeave(view)
        RaiseEvent MouseLeave(view)
    End Sub



End Class

