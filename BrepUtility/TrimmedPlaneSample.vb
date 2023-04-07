Imports RMA.Rhino
Imports RMA.OpenNURBS


''*********************************************************************************
''***                                                                           ***
''*** UTILIZZO: Dim brep As OnBrep = BrepUtility.MakeTrimmedBrepFace()          ***
''***                                                                           ***
''*********************************************************************************


Public Class TrimmedPlaneSample
    ' symbolic vertex index constants to make code more readable
    Const A As Integer = 0, B As Integer = 1, C As Integer = 2, D As Integer = 3, E As Integer = 4

    ' symbolic edge index constants to make code more readable
    Const AB As Integer = 0, BC As Integer = 1, AC As Integer = 2

    ' symbolic face index constants to make code more readable
    Const ABC_i As Integer = 0

    Private Shared Function CreateLinearCurve(from As On3dPoint, [to] As On3dPoint) As OnCurve
        ' creates a 3d line segment to be used as a 3d curve in a ON_Brep
        Dim c3d As OnCurve = New OnLineCurve(from, [to])
        If c3d IsNot Nothing Then
            c3d.SetDomain(0.0, 10.0)
        End If

        Return c3d
    End Function

    Private Shared Function CreateNurbsSurface(SW As On3dPoint, SE As On3dPoint, NE As On3dPoint, NW As On3dPoint) As OnSurface
        ' dimension (>= 1)
        ' not rational
        ' "u" order (>= 2)
        ' "v" order (>= 2)
        ' number of control vertices in "u" dir (>= order)
        ' number of control vertices in "v" dir (>= order)
        Dim pNurbsSurface As New OnNurbsSurface(3, False, 2, 2, 2, 2)
        ' corner CVs in counter clockwise order starting in the south west
        pNurbsSurface.SetCV(0, 0, SW)
        pNurbsSurface.SetCV(1, 0, SE)
        pNurbsSurface.SetCV(1, 1, NE)
        pNurbsSurface.SetCV(0, 1, NW)
        ' "u" knots
        pNurbsSurface.SetKnot(0, 0, 0.0)
        pNurbsSurface.SetKnot(0, 1, 1.0)
        ' "v" knots
        pNurbsSurface.SetKnot(1, 0, 0.0)
        pNurbsSurface.SetKnot(1, 1, 1.0)

        Return pNurbsSurface
    End Function

    ' index of start vertex
    ' index of end vertex
    Private Shared Sub CreateOneEdge(ByRef brep As OnBrep, vi0 As Integer, vi1 As Integer, c3i As Integer)
        ' index of 3d curve
        Dim v0 As OnBrepVertex = brep.m_V(vi0)
        Dim v1 As OnBrepVertex = brep.m_V(vi1)
        Dim edge As OnBrepEdge = brep.NewEdge(v0, v1, c3i)
        If edge IsNot Nothing Then
            edge.m_tolerance = 0.0
        End If
        ' this simple example is exact - for models with
        ' non-exact data, set tolerance as explained in
        ' definition of ON_BrepEdge.
    End Sub

    Private Shared Sub CreateEdges(ByRef brep As OnBrep)
        ' In this simple example, the edge indices exactly match the 3d
        ' curve indices.  In general,the correspondence between edge and
        ' curve indices can be arbitrary.  It is permitted for multiple
        ' edges to use different portions of the same 3d curve.  The 
        ' orientation of the edge always agrees with the natural 
        ' parametric orientation of the curve.

        'outer edges
        ' edge that runs from A to B
        CreateOneEdge(brep, A, B, AB)

        ' edge that runs from B to C
        CreateOneEdge(brep, B, C, BC)

        ' edge that runs from C to D
        CreateOneEdge(brep, A, C, AC)

    End Sub

    Private Shared Function CreateTrimmingCurve(s As OnSurface, side As Integer) As OnCurve
        ' 0 = SW to SE
        ' 1 = SE to NE
        ' 2 = NE to NW
        ' 3 = NW to SW
        ' A trimming curve is a 2d curve whose image lies in the surface's domain.
        ' The "active" portion of the surface is to the left of the trimming curve.
        ' An outer trimming loop consists of a simple closed curve running 
        ' counter-clockwise around the region it trims.
        ' An inner trimming loop consists of a simple closed curve running 
        ' clockwise around the region the hole.

        Dim from As New On2dPoint()
        Dim [to] As New On2dPoint()
        Dim u0 As Double = Double.NaN, u1 As Double = Double.NaN, v0 As Double = Double.NaN, v1 As Double = Double.NaN

        s.GetDomain(0, u0, u1)
        s.GetDomain(1, v0, v1)

        Select Case side
            Case 0
                ' SW to SE
                from.x = u0
                from.y = v0
                [to].x = u1
                [to].y = v0
                Exit Select
            Case 1
                ' diagonal
                from.x = u1
                from.y = v0
                [to].x = (u0 + u1) / 2
                [to].y = v1
                Exit Select
            Case 2
                ' diagonal
                from.x = (u0 + u1) / 2
                from.y = v1
                [to].x = u0
                [to].y = v0
                Exit Select
            Case Else
                Return Nothing
        End Select

        Dim c2d As OnCurve = New OnLineCurve(from, [to])
        If c2d IsNot Nothing Then
            c2d.SetDomain(0.0, 1.0)
        End If
        Return c2d
    End Function

    ' returns index of loop
    ' face loop is on
    ' Indices of corner vertices listed in A,B,C order
    ' index of first edge
    ' orientation of edge
    ' index second edgee
    ' orientation of edge
    ' index third edge
    Private Shared Function MakeTrimmingLoop(ByRef brep As OnBrep, ByRef face As OnBrepFace, v0 As Integer, v1 As Integer, v2 As Integer, e0 As Integer,
        e0_dir As Integer, e1 As Integer, e1_dir As Integer, e2 As Integer, e2_dir As Integer) As Integer
        ' orientation of edge
        Dim srf As OnSurface = brep.m_S(face.m_si)

        'Create new loop
        Dim [loop] As OnBrepLoop = brep.NewLoop(IOnBrepLoop.TYPE.outer, face)

        ' Create trimming curves running counter clockwise around the surface's domain.
        ' Note that trims of outer loops run counter clockwise while trims of inner loops (holes) run anti-clockwise.
        ' Also note that when trims locate on surface N,S,E or W ends, then trim_iso becomes N_iso, S_iso, E_iso and W_iso respectfully.  
        ' While if trim is parallel to surface N,S or E,W, then trim is becomes y_iso and x_iso respectfully.

        ' Start at the south side
        Dim c2 As OnCurve
        Dim c2i As Integer, ei As Integer = 0
        Dim bRev3d As Boolean = False
        Dim iso As IOnSurface.ISO = IOnSurface.ISO.not_iso

        For side As Integer = 0 To 2
            ' side: 0=south, 1=east, 2=north, 3=west

            c2 = CreateTrimmingCurve(srf, side)

            'Add trimming curve to brep trmming curves array
            c2i = brep.m_C2.Count()
            brep.m_C2.Append(c2)

            Select Case side
                Case 0
                    ' south
                    ei = e0
                    bRev3d = (e0_dir = -1)
                    iso = IOnSurface.ISO.S_iso
                    Exit Select
                Case 1
                    ' diagonal
                    ei = e1
                    bRev3d = (e1_dir = -1)
                    iso = IOnSurface.ISO.not_iso
                    Exit Select
                Case 2
                    ' diagonal
                    ei = e2
                    bRev3d = (e2_dir = -1)
                    iso = IOnSurface.ISO.not_iso
                    Exit Select
            End Select

            'Create new trim topology that references edge, direction reletive to edge, loop and trim curve geometry
            Dim edge As OnBrepEdge = brep.m_E(ei)
            Dim trim As OnBrepTrim = brep.NewTrim(edge, bRev3d, [loop], c2i)
            If trim IsNot Nothing Then
                trim.m_iso = iso
                trim.m_type = IOnBrepTrim.TYPE.boundary
                ' This one b-rep face, so all trims are boundary ones.
                trim.m_tolerance(0) = 0.0
                ' This simple example is exact - for models with non-exact
                ' data, set tolerance as explained in definition of ON_BrepTrim.
                trim.m_tolerance(1) = 0.0
            End If
        Next
        Return [loop].m_loop_index
    End Function

    ' index of 3d surface
    ' orientation of surface with respect to surfce
    ' Indices of corner vertices
    ' index of first edge
    ' orientation of edge
    ' index of second edge
    ' orientation of edge
    ' index of third edge
    Private Shared Sub MakeTrimmedFace(ByRef brep As OnBrep, si As Integer, s_dir As Integer, v0 As Integer, v1 As Integer, v2 As Integer,
        e0 As Integer, e0_dir As Integer, e1 As Integer, e1_dir As Integer, e2 As Integer, e2_dir As Integer)
        ' orientation of edge
        'Add new face to brep
        Dim face As OnBrepFace = brep.NewFace(si)

        'Create loop and trims for the face
        MakeTrimmingLoop(brep, face, v0, v1, v2, e0,
            e0_dir, e1, e1_dir, e2, e2_dir)

        'Set face direction relative to surface direction
        face.m_bRev = (s_dir = -1)
    End Sub

    Private Shared Sub CreateFace(ByRef brep As OnBrep, si As Integer)
        ' Index of face
        ' orientation of surface with respect to surface
        ' Indices of vertices
        ' Side edge and its orientation with respect to
        ' to the trimming curve.  (AB)
        ' Side edge and its orientation with respect to
        ' to the trimming curve.  (BC)
        ' Side edge and its orientation with respect to
        ' to the trimming curve   (AC)
        MakeTrimmedFace(brep, ABC_i, +1, A, B, C,
            AB, +1, BC, +1, AC, -1)
    End Sub

    Public Shared Function MakeTrimmedBrepFace() As OnBrep
        ' This example demonstrates how to construct a ON_Brep
        ' with the topology shown below.
        '
        '
        '    E-------C--------D
        '    |       /\       | 
        '    |      /  \      |
        '    |     /    \     |
        '    |    e2      e1  |     
        '    |   /        \   |    
        '    |  /          \  |  
        '    | /            \ |  
        '    A-----e0-------->B
        '
        '
        '  Things need to be defined in a valid brep:
        '   1- Vertices
        '   2- 3D Curves (geometry)
        '   3- Edges (topology - reference curve geometry)
        '   4- Surface (geometry)
        '   5- Faces (topology - reference surface geometry)
        '   6- Loops (2D parameter space of faces)
        '   4- Trims and 2D curves (2D parameter space of edges)
        '

        'Vertex points
        ' define the corners of the face with hole
        Dim point As On3dPoint() = New On3dPoint(4) {}
        point(0) = New On3dPoint(0.0, 0.0, 0.0)
        'point A = geometry for vertex 0 (and surface SW corner)
        point(1) = New On3dPoint(10.0, 0.0, 0.0)
        ' point B = geometry for vertex 1 (and surface SE corner)
        point(2) = New On3dPoint(5.0, 10.0, 0.0)
        ' point C = geometry for vertex 2
        point(3) = New On3dPoint(10.0, 10.0, 0.0)
        ' point D (surface NE corner)
        point(4) = New On3dPoint(0.0, 10.0, 0.0)
        ' point E (surface NW corner)
        ' Build the brep        
        Dim brep As New OnBrep()

        ' create four vertices of the outer edges
        Dim vi As Integer
        For vi = 0 To 2
            Dim v As OnBrepVertex = brep.NewVertex(point(vi))
            ' this simple example is exact - for models with
            ' non-exact data, set tolerance as explained in
            ' definition of ON_BrepVertex.
            v.m_tolerance = 0.0
        Next

        ' Create 3d curve geometry - the orientations are arbitrarily chosen
        ' so that the end vertices are in alphabetical order.
        brep.m_C3.Append(CreateLinearCurve(point(A), point(B)))
        ' line AB
        brep.m_C3.Append(CreateLinearCurve(point(B), point(C)))
        ' line BC
        brep.m_C3.Append(CreateLinearCurve(point(A), point(C)))
        ' line CD
        ' Create edge topology for each curve in the brep.
        CreateEdges(brep)

        ' Create 3d surface geometry - the orientations are arbitrarily chosen so
        ' that some normals point into the cube and others point out of the cube.
        brep.m_S.Append(CreateNurbsSurface(point(A), point(B), point(D), point(E)))
        ' ABCD
        ' Create face topology and 2d parameter space loops and trims.
        CreateFace(brep, ABC_i)

        Return brep
    End Function

End Class






