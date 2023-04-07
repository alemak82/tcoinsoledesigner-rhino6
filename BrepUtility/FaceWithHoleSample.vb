Imports RMA.Rhino
Imports RMA.OpenNURBS



''*************************************************************************************
''***                                                                               ***
''*** UTILIZZO: Dim brep As OnBrep = BrepUtility.MakeBrepFace()                     ***
''***                                                                               ***
''*************************************************************************************



Public Class FaceWithHoleSample
        ' symbolic vertex index constants to make code more readable

        Const A As Integer = 0, B As Integer = 1, C As Integer = 2, D As Integer = 3, E As Integer = 4, F As Integer = 5,
            G As Integer = 6, H As Integer = 7

        ' symbolic edge index constants to make code more readable

        Const AB As Integer = 0, BC As Integer = 1, CD As Integer = 2, AD As Integer = 3, EF As Integer = 4, FG As Integer = 5,
            GH As Integer = 6, EH As Integer = 7

        ' symbolic face index constants to make code more readable
        Const ABCD As Integer = 0

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
            CreateOneEdge(brep, C, D, CD)

            ' edge that runs from A to D
            CreateOneEdge(brep, A, D, AD)

            'Inner Edges
            ' edge that runs from E to F
            CreateOneEdge(brep, E, F, EF)

            ' edge that runs from F to G
            CreateOneEdge(brep, F, G, FG)

            ' edge that runs from G to H
            CreateOneEdge(brep, G, H, GH)

            ' edge that runs from E to H
            CreateOneEdge(brep, E, H, EH)

        End Sub

        Private Shared Function CreateOuterTrimmingCurve(s As OnSurface, side As Integer) As OnCurve
            ' 0 = SW to SE
            ' 1 = SE to NE
            ' 2 = NE to NW
            ' 3 = NW to SW
            ' A trimming curve is a 2d curve whose image lies in the surface's domain.
            ' The "active" portion of the surface is to the left of the trimming curve.
            ' An outer trimming loop consists of a simple closed curve running 
            ' counter-clockwise around the region it trims.

            'In cases when trim curve is not easily defined in surface domain, 
            'use ON_Surface::Pullback only be careful about curve direction to ensure
            'loop trims run anti-clockwise for outer loop and clockwise for inner loop.

            'In this case, trim curves lie on the four edges of the surface
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
                    ' SE to NE
                    from.x = u1
                    from.y = v0
                    [to].x = u1
                    [to].y = v1
                    Exit Select
                Case 2
                    ' NE to NW
                    from.x = u1
                    from.y = v1
                    [to].x = u0
                    [to].y = v1
                    Exit Select
                Case 3
                    ' NW to SW
                    from.x = u0
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
        ' Indices of corner vertices listed in SW,SE,NW,NE order
        ' index of edge on south side of surface
        ' orientation of edge with respect to surface trim
        ' index of edge on south side of surface
        ' orientation of edge with respect to surface trim
        ' index of edge on south side of surface
        ' orientation of edge with respect to surface trim
        ' index of edge on south side of surface
        Private Shared Function MakeOuterTrimmingLoop(ByRef brep As OnBrep, ByRef face As OnBrepFace, vSWi As Integer, vSEi As Integer, vNEi As Integer, vNWi As Integer,
            eSi As Integer, eS_dir As Integer, eEi As Integer, eE_dir As Integer, eNi As Integer, eN_dir As Integer,
            eWi As Integer, eW_dir As Integer) As Integer
            ' orientation of edge with respect to surface trim
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

            For side As Integer = 0 To 3
                ' side: 0=south, 1=east, 2=north, 3=west

                c2 = CreateOuterTrimmingCurve(srf, side)

                'Add trimming curve to brep trmming curves array
                c2i = brep.m_C2.Count()
                brep.m_C2.Append(c2)

                Select Case side
                    Case 0
                        ' south
                        ei = eSi
                        bRev3d = (eS_dir = -1)
                        iso = IOnSurface.ISO.S_iso
                        Exit Select
                    Case 1
                        ' east
                        ei = eEi
                        bRev3d = (eE_dir = -1)
                        iso = IOnSurface.ISO.E_iso
                        Exit Select
                    Case 2
                        ' north
                        ei = eNi
                        bRev3d = (eN_dir = -1)
                        iso = IOnSurface.ISO.N_iso
                        Exit Select
                    Case 3
                        ' west
                        ei = eWi
                        bRev3d = (eW_dir = -1)
                        iso = IOnSurface.ISO.W_iso
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

        'Trim curves must run is clockwise direction
        Private Shared Function CreateInnerTrimmingCurve(s As OnSurface, side As Integer) As OnCurve
            ' 0 = near SE to SW
            ' 1 = near SW to NW
            ' 2 = near NW to NE
            ' 3 = near NE to SE
            ' A trimming curve is a 2d curve whose image lies in the surface's domain.
            ' The "active" portion of the surface is to the left of the trimming curve.
            ' An inner trimming loop consists of a simple closed curve running 
            ' clockwise around the region the hole.

            'In this case, trim curves lie with 0.2 domain distance from surface edge
            Dim from As New On2dPoint()
            Dim [to] As New On2dPoint()
            Dim u0 As Double = Double.NaN, u1 As Double = Double.NaN, v0 As Double = Double.NaN, v1 As Double = Double.NaN

            s.GetDomain(0, u0, u1)
            s.GetDomain(1, v0, v1)

            Dim udis As Double = 0.2 * (u1 - u0)
            Dim vdis As Double = 0.2 * (v1 - v0)

            u0 += udis
            u1 -= udis
            v0 += vdis
            v1 -= vdis

            Select Case side
                Case 0
                    ' near SE to SW
                    from.x = u1
                    from.y = v0
                    [to].x = u0
                    [to].y = v0
                    Exit Select
                Case 1
                    ' near SW to NW
                    from.x = u0
                    from.y = v0
                    [to].x = u0
                    [to].y = v1
                    Exit Select
                Case 2
                    ' near NW to NE
                    from.x = u0
                    from.y = v1
                    [to].x = u1
                    [to].y = v1
                    Exit Select
                Case 3
                    ' near NE to SE
                    from.x = u1
                    from.y = v1
                    [to].x = u1
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
        ' Indices of hole vertices
        ' index of edge close to south side of surface
        ' orientation of edge with respect to surface trim
        ' index of edge close to east side of surface
        ' orientation of edge with respect to surface trim
        ' index of edge close to north side of surface
        ' orientation of edge with respect to surface trim
        ' index of edge close to west side of surface
        Private Shared Function MakeInnerTrimmingLoop(ByRef brep As OnBrep, ByRef face As OnBrepFace, vSWi As Integer, vSEi As Integer, vNEi As Integer, vNWi As Integer,
            eSi As Integer, eS_dir As Integer, eEi As Integer, eE_dir As Integer, eNi As Integer, eN_dir As Integer,
            eWi As Integer, eW_dir As Integer) As Integer
            ' orientation of edge with respect to surface trim
            Dim srf As OnSurface = brep.m_S(face.m_si)
            'Create new inner loop
            Dim [loop] As OnBrepLoop = brep.NewLoop(IOnBrepLoop.TYPE.inner, face)

            ' Create trimming curves running counter clockwise around the surface's domain.
            ' Note that trims of outer loops run counter clockwise while trims of inner loops (holes) run clockwise.
            ' Also note that when trims locate on surface N,S,E or W ends, then trim_iso becomes N_iso, S_iso, E_iso and W_iso respectfully.  
            ' While if trim is parallel to surface N,S or E,W, then trim iso becomes y_iso and x_iso respectfully. 
            ' All other cases, iso is set to not_iso

            ' Start near the south side
            Dim c2 As OnCurve
            Dim c2i As Integer, ei As Integer = 0
            Dim bRev3d As Boolean = False
            Dim iso As IOnSurface.ISO = IOnSurface.ISO.not_iso

            For side As Integer = 0 To 3
                ' side: 0=near south(y_iso), 1=near west(x_iso), 2=near north(y_iso), 3=near east(x_iso)

                'Create trim 2d curve
                c2 = CreateInnerTrimmingCurve(srf, side)

                'Add trimming curve to brep trmming curves array
                c2i = brep.m_C2.Count()
                brep.m_C2.Append(c2)

                Select Case side
                    Case 0
                        ' near south
                        ei = eSi
                        bRev3d = (eS_dir = -1)
                        iso = IOnSurface.ISO.y_iso
                        Exit Select
                    Case 1
                        ' near west
                        ei = eEi
                        bRev3d = (eE_dir = -1)
                        iso = IOnSurface.ISO.x_iso
                        Exit Select
                    Case 2
                        ' near north
                        ei = eNi
                        bRev3d = (eN_dir = -1)
                        iso = IOnSurface.ISO.y_iso
                        Exit Select
                    Case 3
                        ' near east
                        ei = eWi
                        bRev3d = (eW_dir = -1)
                        iso = IOnSurface.ISO.x_iso
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

        Private Shared Sub CreateFace(ByRef brep As OnBrep, si As Integer)
            'Add new face to brep
            Dim face As OnBrepFace = brep.NewFace(si)

            'Create outer loop and trims for the face
            ' Indices of vertices listed in SW,SE,NW,NE order
            ' South side edge and its orientation with respect to
            ' to the trimming curve.  (AB)
            ' East side edge and its orientation with respect to
            ' to the trimming curve.  (BC)
            ' North side edge and its orientation with respect to
            ' to the trimming curve   (CD)
            ' West side edge and its orientation with respect to
            ' to the trimming curve   (AD)
            MakeOuterTrimmingLoop(brep, face, A, B, C, D,
                AB, +1, BC, +1, CD, +1,
                AD, -1)


            'Create loop and trims for the face
            ' Indices of hole vertices
            ' Parallel to south side edge and its orientation with respect to
            ' to the trimming curve.  (EF)
            ' Parallel to east side edge and its orientation with respect to
            ' to the trimming curve.  (FG)
            ' Parallel to north side edge and its orientation with respect to
            ' to the trimming curve   (GH)
            ' Parallel to west side edge and its orientation with respect to
            ' to the trimming curve   (EH)
            MakeInnerTrimmingLoop(brep, face, E, F, G, H,
                EF, +1, FG, +1, GH, +1,
                EH, -1)

            'Set face direction relative to surface direction
            face.m_bRev = False
        End Sub

        Public Shared Function MakeBrepFace() As OnBrep
            ' This example demonstrates how to construct a ON_Brep
            ' with the topology shown below.
            '
            '
            '   D---------e2-----C      
            '   |                |     
            '   |  G----e6---H   |
            '   |  |         |   |
            '   e3 e5        e7  |
            '   |  |         |   |
            '   |  F<---e4---E   |
            '   |                |
            '   A-------e0------>B
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
            Dim point As On3dPoint() = New On3dPoint(7) {}
            point(0) = New On3dPoint(0.0, 0.0, 0.0)
            point(1) = New On3dPoint(10.0, 0.0, 0.0)
            point(2) = New On3dPoint(10.0, 10.0, 0.0)
            point(3) = New On3dPoint(0.0, 10.0, 0.0)

            point(4) = New On3dPoint(8.0, 2.0, 0.0)
            point(5) = New On3dPoint(2.0, 2.0, 0.0)
            point(6) = New On3dPoint(2.0, 8.0, 0.0)
            point(7) = New On3dPoint(8.0, 8.0, 0.0)

            ' Build the brep        
            Dim brep As New OnBrep()

            ' create four vertices of the outer edges
            Dim vi As Integer
            For vi = 0 To 7
                Dim v As OnBrepVertex = brep.NewVertex(point(vi))
                ' this simple example is exact - for models with
                ' non-exact data, set tolerance as explained in
                ' definition of ON_BrepVertex.
                v.m_tolerance = 0.0
            Next

            ' Create 3d curve geometry of the outer boundary 
            ' The orientations are arbitrarily chosen
            ' so that the end vertices are in alphabetical order.
            brep.m_C3.Append(CreateLinearCurve(point(A), point(B)))
            ' line AB
            brep.m_C3.Append(CreateLinearCurve(point(B), point(C)))
            ' line BC
            brep.m_C3.Append(CreateLinearCurve(point(C), point(D)))
            ' line CD
            brep.m_C3.Append(CreateLinearCurve(point(A), point(D)))
            ' line AD
            ' Create 3d curve geometry of the hole 
            brep.m_C3.Append(CreateLinearCurve(point(E), point(F)))
            ' line EF
            brep.m_C3.Append(CreateLinearCurve(point(F), point(G)))
            ' line GH
            brep.m_C3.Append(CreateLinearCurve(point(G), point(H)))
            ' line HI
            brep.m_C3.Append(CreateLinearCurve(point(E), point(H)))
            ' line EI
            ' Create edge topology for each curve in the brep.
            CreateEdges(brep)

            ' Create 3d surface geometry - the orientations are arbitrarily chosen so
            ' that some normals point into the cube and others point out of the cube.
            brep.m_S.Append(CreateNurbsSurface(point(A), point(B), point(C), point(D)))
            ' ABCD
            ' Create face topology and 2d parameter space loops and trims.
            CreateFace(brep, ABCD)

            Return brep
        End Function

End Class


