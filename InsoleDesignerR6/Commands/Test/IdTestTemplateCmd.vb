#If DEBUG Then


Imports System
Imports System.Collections.Generic
Imports Eto.Drawing
Imports Rhino
Imports Rhino.Commands
Imports Rhino.Geometry
Imports Rhino.Input
Imports Rhino.Input.Custom
Imports RhinoUtils
Imports RMA.OpenNURBS
Imports RMA.Rhino


Public Class IdTestTemplateCmd
    Inherits Command

    Shared _instance As IdTestTemplateCmd

    Public Sub New()
        ' Rhino only creates one instance of each command class defined in a
        ' plug-in, so it is safe to store a refence in a static field.
        _instance = Me
    End Sub

    '''<summary>The only instance of this command.</summary>
    Public Shared ReadOnly Property Instance() As IdTestTemplateCmd
        Get
            Return _instance
        End Get
    End Property

    '''<returns>The command name as it appears on the Rhino command line.</returns>
    Public Overrides ReadOnly Property EnglishName() As String
        Get
            Return "IdTestTemplateCmd"
        End Get
    End Property


    Protected Overrides Function RunCommand(ByVal doc As RhinoDoc, ByVal mode As RunMode) As Result
        '' ORIGINALE DA TEMPLATE
        'RhinoApp.WriteLine("The {0} command will add a line right now.", EnglishName)

        'Dim pt0 As Point3d
        'Using getPointAction As New GetPoint()
        '    getPointAction.SetCommandPrompt("Please select the start point")
        '    If getPointAction.[Get]() <> GetResult.Point Then
        '        RhinoApp.WriteLine("No start point was selected.")
        '        Return getPointAction.CommandResult()
        '    End If
        '    pt0 = getPointAction.Point()
        'End Using

        'Dim pt1 As Point3d
        'Using getPointAction As New GetPoint()
        '    getPointAction.SetCommandPrompt("Please select the end point")
        '    getPointAction.SetBasePoint(pt0, True)
        '    AddHandler getPointAction.DynamicDraw,
        '        Sub(sender, e) e.Display.DrawLine(pt0, e.CurrentPoint, System.Drawing.Color.DarkRed)
        '    If getPointAction.[Get]() <> GetResult.Point Then
        '        RhinoApp.WriteLine("No end point was selected.")
        '        Return getPointAction.CommandResult()
        '    End If
        '    pt1 = getPointAction.Point()
        'End Using

        'Dim line1 = New Line(pt0, pt1)
        'doc.Objects.AddLine(line1)
        'doc.Views.Redraw()
        'RhinoApp.WriteLine("The {0} command added one line to the document.", EnglishName)
        'RhinoApp.WriteLine("The distance between the two points is {0}.", line1.Length)



        ''USANDO RhinoCommon.dll
        'Dim aaa = New DocObjects.Layer()
        'aaa.Name = "provaaaa"
        'bll.IdAlias.Doc2.Layers.Add(aaa)

        'USANDO Rhino_DotNet.dll
        Dim layerTable As MRhinoLayerTable = RhUtil.RhinoApp().ActiveDoc().m_layer_table        
        Dim newLayer As New RMA.OpenNURBS.OnLayer()
        newLayer.SetLayerName("testaaaaaaaaaa")
        newLayer.SetColor(New OnColor(Drawing.Color.Red))
        layerTable.AddLayer(newLayer)
        ''SE SCOMMENO LA RIGA SOTTO NON FUNZIA INFATTI IN Rhino_DotNet.dll DI RHINO 6 IL METODO NON ESISTE
        'Dim layerIndex As Int32 = layerTable.FindLayer(newLayer.m_layer_id)



        Return Result.Success
    End Function

End Class

#End If