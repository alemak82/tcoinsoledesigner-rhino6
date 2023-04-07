Imports RMA.Rhino
Imports RMA.OpenNURBS




'Question
'I am having a problem With getting a tight bounding box Of a trimmed Brep Using C#. I have a trimmed surface which I Then split into smaller pieces. 
'I Then want To Get Each face Of this New Brep And Get the bounding box Of Each. 
'However, Each Of the resulting bounding boxes bounds the extent Of the original Brep And Not the individual faces. 
'If I extract the edge Loop, this bounds correctly, however.

'Answer
'Tight bounding boxes For surfaces And Breps are calculated With runtime information, such As display curves And render meshes. 
'Thus, When you cook up your own Brep, which Do Not have display curves And render meshes, you only Get the bounding boxes Of the untrimmed surfaces.
'One way round this Is To Call RhUtil.RhinoGetTightBoundingBox. This Function requires Rhino objects that are In the document. This Is probably Not too convenient In your situation.
'Another alternative Is To created meshes, one For Each Brep face, And Then calculate the bounding box Of Each mesh. This should give you accurate results.





Public Class CalculatingTightBoundingBoxeSplitBrep



    Public Shared Function CalculateBbox(ByRef split_brep As IOnBrep) As OnBoundingBox
        If split_brep Is Nothing Then Return Nothing

        Dim mp As IOnMeshParameters = RhUtil.RhinoApp.ActiveDoc.Properties().RenderMeshSettings()
        Dim mesh_list As OnMesh() = New OnMesh(-1) {}
        Dim mesh_count As Integer = split_brep.CreateMesh(mp, mesh_list)

        Dim result As New OnBoundingBox()
        For i As Integer = 0 To mesh_list.Length - 1
            Dim bbox As New OnBoundingBox()
            If mesh_list(i).GetTightBoundingBox(bbox) Then
                result.Union(bbox)
                Dim box_corners As On3dPoint() = New On3dPoint(7) {}

                ''DISEGNA BBOX SUL DOC------------------------------------------------------------------------------
                'box_corners(0) = bbox.Corner(0, 0, 0)
                'box_corners(1) = bbox.Corner(1, 0, 0)
                'box_corners(2) = bbox.Corner(1, 1, 0)
                'box_corners(3) = bbox.Corner(0, 1, 0)
                'box_corners(4) = bbox.Corner(0, 0, 1)
                'box_corners(5) = bbox.Corner(1, 0, 1)
                'box_corners(6) = bbox.Corner(1, 1, 1)
                'box_corners(7) = bbox.Corner(0, 1, 1)

                'Dim rect As On3dPoint() = New On3dPoint(4) {}
                'Dim line As New OnLine()

                'Dim box_type As Integer = ClassifyBBox(box_corners, rect, line)
                '' returns 0=box, 1=rectangle, 2=line, 3=point
                '' BoundingBox failed. The bounding box is a point.
                'If box_type = 3 Then
                '    ' BoundingBox failed. The bounding box is a line.
                'ElseIf box_type = 2 Then
                'ElseIf box_type = 1 Then
                '    Dim polyline As New OnPolyline()
                '    polyline.Append(rect(0))
                '    polyline.Append(rect(1))
                '    polyline.Append(rect(2))
                '    polyline.Append(rect(3))
                '    polyline.Append(rect(4))
                '    RhUtil.RhinoApp.ActiveDoc.AddCurveObject(polyline)
                'Else
                '    ' box_type == 0
                '    Dim brep_box As OnBrep = OnUtil.ON_BrepBox(box_corners)
                '    If brep_box IsNot Nothing Then RhUtil.RhinoApp.ActiveDoc.AddBrepObject(brep_box)
                'End If
                ''-------------------------------------------------------------------------------------------------------
            End If
        Next

        Return result
    End Function


    ''SERVE SOLO SE VOGLIO DISEGNARE LA BBOX SUL DOC
    'Private Shared Function ClassifyBBox(box_corners As On3dPoint(), ByRef rect As On3dPoint(), ByRef line As OnLine) As Integer
    '    Const FLT_EPSILON As Double = 0.0000001192093F
    '    Dim numflat As Integer = 0
    '    Dim xflat As Boolean = False, yflat As Boolean = False, zflat As Boolean = False

    '    If FLT_EPSILON > box_corners(0).DistanceTo(box_corners(1)) Then
    '        numflat += 1
    '        xflat = True
    '    End If

    '    If FLT_EPSILON > box_corners(0).DistanceTo(box_corners(3)) Then
    '        numflat += 1
    '        yflat = True
    '    End If

    '    If FLT_EPSILON > box_corners(0).DistanceTo(box_corners(4)) Then
    '        numflat += 1
    '        zflat = True
    '    End If

    '    If numflat = 2 Then
    '        line.from = box_corners(0)
    '        If Not xflat Then
    '            line.[to] = box_corners(1)
    '        ElseIf Not yflat Then
    '            line.[to] = box_corners(3)
    '        Else
    '            line.[to] = box_corners(4)
    '        End If
    '    ElseIf numflat = 1 Then
    '        rect(0) = box_corners(0)
    '        rect(4) = box_corners(0)
    '        If xflat Then
    '            rect(1) = box_corners(4)
    '            rect(2) = box_corners(7)
    '            rect(3) = box_corners(3)
    '        ElseIf yflat Then
    '            rect(1) = box_corners(1)
    '            rect(2) = box_corners(5)
    '            rect(3) = box_corners(4)
    '        Else
    '            ' zflat
    '            rect(1) = box_corners(1)
    '            rect(2) = box_corners(2)
    '            rect(3) = box_corners(3)
    '        End If
    '    End If

    '    Return numflat
    'End Function





End Class
