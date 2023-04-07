Imports RMA.OpenNURBS
Imports RMA.Rhino


'**************************************************************************************************
'*** Classe che definisce il proprio DisplayConduit per la visualizzazione di oggetti OpenNURBS ***
'*** e gestisce la lista degli oggetti da mostrare con gli attributi di visualizzazione         ***
'**************************************************************************************************

Public Class RhDisplayConduit
    Inherits MRhinoDisplayConduit


#Region " Fields & Initialization "

    Dim mObjectList As List(Of Object)
    Dim mColorList As List(Of UInteger)
    Dim mTransparencyList As List(Of Double)
    Dim mWireDensityList As List(Of Integer)
    Dim mMaterialList As List(Of OnMaterial)
    Dim mDrawForegroundList As List(Of Boolean)
    Dim mGroupIdList As List(Of Integer)

    Dim mBoundingBox As OnBoundingBox

    Public Sub New()
        MyBase.New(New MSupportChannels(MSupportChannels.SC_CALCBOUNDINGBOX Or MSupportChannels.SC_DRAWFOREGROUND Or MSupportChannels.SC_POSTDRAWOBJECTS), True)
        mObjectList = New List(Of Object)
        mColorList = New List(Of UInteger)
        mTransparencyList = New List(Of Double)
        mWireDensityList = New List(Of Integer)
        mMaterialList = New List(Of OnMaterial)
        mDrawForegroundList = New List(Of Boolean)
        mGroupIdList = New List(Of Integer)
        mBoundingBox = New OnBoundingBox
    End Sub

#End Region


#Region " Function "

    Private Sub UpdateBoundingBox()
        For i As Integer = 0 To mObjectList.Count - 1
            Select Case mObjectList(i).GetType.ToString
                Case GetType(On3dPoint).ToString
                    Dim point As On3dPoint = DirectCast(mObjectList(i), On3dPoint)
                    mBoundingBox.Set(point, True)

                Case GetType(OnLine).ToString
                    Dim line As OnLine = DirectCast(mObjectList(i), OnLine)
                    line.GetTightBoundingBox(mBoundingBox, 1)

                Case GetType(OnPolyline).ToString
                    Dim polyline As OnPolyline = DirectCast(mObjectList(i), OnPolyline)
                    polyline.GetTightBoundingBox(mBoundingBox, 1)

                Case GetType(OnBrep).ToString
                    Dim brep As OnBrep = DirectCast(mObjectList(i), OnBrep)
                    brep.GetTightBoundingBox(mBoundingBox, 1)

                Case GetType(OnMesh).ToString
                    Dim mesh As OnMesh = DirectCast(mObjectList(i), OnMesh)
                    mesh.GetTightBoundingBox(mBoundingBox, 1)

                Case GetType(OnTextDot).ToString
                    Dim dot As OnTextDot = DirectCast(mObjectList(i), OnTextDot)
                    mBoundingBox.Set(dot.point, True)
            End Select
        Next
    End Sub

    Private Sub InvalidateBoundingBox()
        If Not mBoundingBox Is Nothing Then mBoundingBox.Dispose()
        mBoundingBox = New OnBoundingBox
    End Sub


    ''' <summary>
    ''' Aggiunge un oggetto alla lista di visualizzazione
    ''' </summary>
    ''' <param name="constObj">Una copia di constObj verrà aggiunta alla lista</param>
    ''' <param name="color">Colore con cui disegnare. Nel caso di Brep, Mesh e Surface se material non è nothing il colore viene ignorato</param>
    ''' <param name="transparency">Grado di trasparenza compreso tra 0 e un massimo di 1</param>
    ''' <param name="wireDensity">Esprime lo spessore per la linea, la densità delle isocurve per Brep e Surface (-1 per assenza curve di bordo), se >=0 la presenza del wireframe nelle Mesh</param>
    ''' <param name="material">Materiale per la renderizzazione. Se nothing vengono usati color e transparency</param>
    ''' <param name="drawForeground">Stabilisce se l'oggetto deve essere disegnato in primo piano</param>
    ''' <param name="groupId">Attribuisce un ID del gruppo di appartenenza</param>
    ''' <remarks></remarks>
    Public Sub AddObject(ByVal constObj As Object, Optional ByVal color As IOnColor = Nothing, Optional ByVal transparency As Double = 0.0, Optional ByVal wireDensity As Integer = -1, Optional ByVal material As IOnMaterial = Nothing, Optional ByVal drawForeground As Boolean = False, Optional ByVal groupId As Integer = -1)
        If constObj Is Nothing Then Exit Sub
        Select Case constObj.GetType.ToString
            Case GetType(On3dPoint).ToString
                Dim point As IOn3dPoint = CType(constObj, IOn3dPoint)
                mObjectList.Add(New On3dPoint(point))

            Case GetType(OnLine).ToString
                Dim line As OnLine = DirectCast(constObj, OnLine)
                mObjectList.Add(New OnLine(line))

            Case GetType(OnPolyline).ToString
                Dim polyline As OnPolyline = DirectCast(constObj, OnPolyline)
                mObjectList.Add(New OnPolyline(polyline))

            Case GetType(OnCurve).ToString, GetType(OnPolyCurve).ToString, GetType(OnNurbsCurve).ToString
                Dim curve As OnCurve = DirectCast(constObj, OnCurve)
                Dim polyline As OnPolyline = RhGeometry.GetApproximatingPolyline(curve, 0.2)
                mObjectList.Add(polyline)

            Case GetType(OnBrep).ToString
                Dim brep As OnBrep = DirectCast(constObj, OnBrep)
                mObjectList.Add(New OnBrep(brep))

            Case GetType(OnSurface).ToString, GetType(OnSumSurface).ToString, GetType(OnNurbsSurface).ToString
                Dim surface As OnSurface = DirectCast(constObj, OnSurface)
                mObjectList.Add(surface.BrepForm)

            Case GetType(OnMesh).ToString
                Dim mesh As OnMesh = DirectCast(constObj, OnMesh)
                mObjectList.Add(New OnMesh(mesh))

            Case GetType(OnTextDot).ToString
                Dim dot As OnTextDot = DirectCast(constObj, OnTextDot)
                mObjectList.Add(New OnTextDot(dot))
        End Select

        If color Is Nothing Then
            mColorList.Add(Convert.ToUInt32(RGB(255, 255, 255)))
        Else
            mColorList.Add(Convert.ToUInt32(RGB(color.Red, color.Green, color.Blue)))
        End If
        mTransparencyList.Add(transparency)
        If material Is Nothing Then
            mMaterialList.Add(Nothing)
        Else
            mMaterialList.Add(New OnMaterial(material))
        End If
        mWireDensityList.Add(wireDensity)
        mDrawForegroundList.Add(drawForeground)
        mGroupIdList.Add(groupId)
        UpdateBoundingBox()
    End Sub


    ''' <summary>
    ''' Aggiunge un oggetto alla lista di visualizzazione
    ''' </summary>
    ''' <param name="constObj">Una copia di constObj verrà aggiunta alla lista</param>
    ''' <param name="index">Indice di inserimento nella lista</param>
    ''' <param name="color">Colore con cui disegnare. Nel caso di Brep, Mesh e Surface se material non è nothing il colore viene ignorato</param>
    ''' <param name="transparency">Grado di trasparenza compreso tra 0 e un massimo di 1</param>
    ''' <param name="wireDensity">Esprime lo spessore per la linea, polilinea o curva, la densità delle isocurve per Brep e Surface (-1 per assenza curve di bordo), se >=0 la presenza del wireframe nelle Mesh</param>
    ''' <param name="material">Materiale per la renderizzazione. Se nothing vengono usati color e transparency</param>
    ''' <remarks></remarks>
    Public Sub InsertObject(ByVal constObj As Object, ByVal index As Integer, Optional ByVal color As IOnColor = Nothing, Optional ByVal transparency As Double = 0.0, Optional ByVal wireDensity As Integer = -1, Optional ByVal material As IOnMaterial = Nothing, Optional ByVal drawForeground As Boolean = False, Optional ByVal groupId As Integer = -1)
        If constObj Is Nothing Then Exit Sub
        If index < 0 Or index > Me.mObjectList.Count Then Exit Sub
        Select Case constObj.GetType.ToString
            Case GetType(On3dPoint).ToString
                Dim point As IOn3dPoint = CType(constObj, IOn3dPoint)
                mObjectList.Insert(index, New On3dPoint(point))

            Case GetType(OnLine).ToString
                Dim line As OnLine = DirectCast(constObj, OnLine)
                mObjectList.Insert(index, New OnLine(line))

            Case GetType(OnPolyline).ToString
                Dim polyline As OnPolyline = DirectCast(constObj, OnPolyline)
                mObjectList.Insert(index, New OnPolyline(polyline))

            Case GetType(OnCurve).ToString, GetType(OnPolyCurve).ToString
                Dim curve As OnCurve = DirectCast(constObj, OnCurve)
                Dim polyline As OnPolyline = RhGeometry.GetApproximatingPolyline(curve, 60)
                mObjectList.Add(polyline)

            Case GetType(OnBrep).ToString
                Dim brep As OnBrep = DirectCast(constObj, OnBrep)
                mObjectList.Insert(index, New OnBrep(brep))

            Case GetType(OnSurface).ToString
                Dim surface As OnSurface = DirectCast(constObj, OnSurface)
                mObjectList.Insert(index, surface.BrepForm)

            Case GetType(OnMesh).ToString
                Dim mesh As OnMesh = DirectCast(constObj, OnMesh)
                mObjectList.Insert(index, New OnMesh(mesh))

            Case GetType(OnTextDot).ToString
                Dim dot As OnTextDot = DirectCast(constObj, OnTextDot)
                mObjectList.Insert(index, New OnTextDot(dot))
        End Select

        If color Is Nothing Then
            mColorList.Insert(index, Convert.ToUInt32(RGB(255, 255, 255)))
        Else
            mColorList.Insert(index, Convert.ToUInt32(RGB(color.Red, color.Green, color.Blue)))
        End If
        mTransparencyList.Insert(index, transparency)
        If material Is Nothing Then
            mMaterialList.Insert(index, Nothing)
        Else
            mMaterialList.Insert(index, New OnMaterial(material))
        End If
        mWireDensityList.Insert(index, wireDensity)
        mDrawForegroundList.Insert(index, drawForeground)
        mGroupIdList.Add(groupId)
        UpdateBoundingBox()
    End Sub


    Public Sub RemoveAt(ByVal index As Integer)
        If index < 0 Or index > mObjectList.Count - 1 Then Exit Sub
        Me.DisposeObject(index)
        mObjectList.RemoveAt(index)
        mColorList.RemoveAt(index)
        mTransparencyList.RemoveAt(index)
        mWireDensityList.RemoveAt(index)
        mMaterialList.RemoveAt(index)
        UpdateBoundingBox()
    End Sub


    Public Sub RemoveAtGroupId(ByVal groupId As Integer)
        For i As Integer = Me.mObjectList.Count - 1 To 0 Step -1
            If mGroupIdList(i) = groupId Then
                Me.DisposeObject(i)
                mObjectList.RemoveAt(i)
                mColorList.RemoveAt(i)
                mTransparencyList.RemoveAt(i)
                mWireDensityList.RemoveAt(i)
                mMaterialList.RemoveAt(i)
                mGroupIdList.RemoveAt(i)
            End If
        Next
        UpdateBoundingBox()
    End Sub

    Public ReadOnly Property ObjectCount() As Integer
        Get
            Return Me.mObjectList.Count
        End Get
    End Property


    Public Sub ClearObjectList()
        For i As Integer = 0 To mObjectList.Count - 1
            DisposeObject(i)
        Next
        mObjectList.Clear()
        mColorList.Clear()
        mTransparencyList.Clear()
        mMaterialList.Clear()
        mWireDensityList.Clear()
        mDrawForegroundList.Clear()
        mGroupIdList.Clear()
        InvalidateBoundingBox()
    End Sub


    Private Sub DisposeObject(index As Integer)
        If index < 0 Or index > mObjectList.Count - 1 Then Exit Sub
        Dim item As Object = mObjectList(index)
        If item IsNot Nothing Then
            Select Case item.GetType.ToString
                Case GetType(On3dPoint).ToString
                    Dim point As On3dPoint = DirectCast(item, On3dPoint)
                    point.Dispose()

                Case GetType(OnLine).ToString
                    Dim line As OnLine = DirectCast(item, OnLine)
                    line.Dispose()

                Case GetType(OnPolyline).ToString
                    Dim polyline As OnPolyline = DirectCast(item, OnPolyline)
                    polyline.Dispose()

                Case GetType(OnBrep).ToString
                    Dim brep As OnBrep = DirectCast(item, OnBrep)
                    brep.Dispose()

                Case GetType(OnMesh).ToString
                    Dim mesh As OnMesh = DirectCast(item, OnMesh)
                    mesh.Dispose()

                Case GetType(OnTextDot).ToString
                    Dim dot As OnTextDot = DirectCast(item, OnTextDot)
                    dot.Dispose()
            End Select
        End If
        If Not mMaterialList(index) Is Nothing Then mMaterialList(index).Dispose()
    End Sub


    Public ReadOnly Property MeshObjects() As List(Of IOnMesh)
        Get
            Dim result As New List(Of IOnMesh)
            For i As Integer = 0 To Me.mObjectList.Count - 1
                If Me.mObjectList(i).GetType.ToString = GetType(OnMesh).ToString Then
                    result.Add(CType(Me.mObjectList(i), IOnMesh))
                End If
            Next
            Return result
        End Get
    End Property

    Public ReadOnly Property BrepObjects() As List(Of IOnBrep)
        Get
            Dim result As New List(Of IOnBrep)
            For i As Integer = 0 To Me.mObjectList.Count - 1
                If Me.mObjectList(i).GetType.ToString = GetType(OnBrep).ToString Then
                    result.Add(CType(Me.mObjectList(i), IOnBrep))
                End If
            Next
            Return result
        End Get
    End Property

    Public ReadOnly Property LineObjects() As List(Of IOnLine)
        Get
            Dim result As New List(Of IOnLine)
            For i As Integer = 0 To Me.mObjectList.Count - 1
                If Me.mObjectList(i).GetType.ToString = GetType(OnLine).ToString Then
                    result.Add(CType(Me.mObjectList(i), IOnLine))
                End If
            Next
            Return result
        End Get
    End Property

    Public ReadOnly Property PolylineObjects() As List(Of IOnPolyline)
        Get
            Dim result As New List(Of IOnPolyline)
            For i As Integer = 0 To Me.mObjectList.Count - 1
                If Me.mObjectList(i).GetType.ToString = GetType(OnPolyline).ToString Then
                    result.Add(CType(Me.mObjectList(i), IOnPolyline))
                End If
            Next
            Return result
        End Get
    End Property

    Public ReadOnly Property PointObjects() As List(Of IOn3dPoint)
        Get
            Dim result As New List(Of IOn3dPoint)
            For i As Integer = 0 To Me.mObjectList.Count - 1
                If Me.mObjectList(i).GetType.ToString = GetType(On3dPoint).ToString Then
                    result.Add(CType(Me.mObjectList(i), IOn3dPoint))
                End If
            Next
            Return result
        End Get
    End Property

    Public ReadOnly Property TextDotObjects() As List(Of IOnTextDot)
        Get
            Dim result As New List(Of IOnTextDot)
            For i As Integer = 0 To Me.mObjectList.Count - 1
                If Me.mObjectList(i).GetType.ToString = GetType(OnTextDot).ToString Then
                    result.Add(CType(Me.mObjectList(i), IOnTextDot))
                End If
            Next
            Return result
        End Get
    End Property

#End Region


#Region " Drawing "

    Public Overrides Function ExecConduit(ByRef dp As RMA.Rhino.MRhinoDisplayPipeline, ByVal current_channel As UInteger, ByRef termination_flag As Boolean) As Boolean

        If current_channel = MSupportChannels.SC_CALCBOUNDINGBOX Then
            m_pChannelAttrs.m_BoundingBox.Union(mBoundingBox)

        Else
            For i As Integer = 0 To mObjectList.Count - 1
                If (Not Me.mDrawForegroundList(i) And current_channel = MSupportChannels.SC_POSTDRAWOBJECTS) Or _
                    (Me.mDrawForegroundList(i) And current_channel = MSupportChannels.SC_DRAWFOREGROUND) Then

                    Select Case mObjectList(i).GetType.ToString
                        Case GetType(On3dPoint).ToString
                            Dim point As On3dPoint = DirectCast(mObjectList(i), On3dPoint)
                            dp.DrawPoint(point)

                        Case GetType(OnLine).ToString
                            Dim line As OnLine = DirectCast(mObjectList(i), OnLine)
                            dp.DrawLine(line.from, line.to, mColorList(i), mWireDensityList(i))

                        Case GetType(OnPolyline).ToString
                            Dim polyline As OnPolyline = DirectCast(mObjectList(i), OnPolyline)
                            dp.DrawPolyline(polyline, mColorList(i), mWireDensityList(i))

                        Case GetType(OnBrep).ToString
                            Dim brep As OnBrep = DirectCast(mObjectList(i), OnBrep)
                            If mWireDensityList(i) >= 0 Then
                                dp.DrawBrep(brep, mColorList(i), mWireDensityList(i))
                            End If
                            If mMaterialList(i) Is Nothing Then
                                Dim material As New OnMaterial()
                                material.m_transparency = mTransparencyList(i)
                                material.m_diffuse = New OnColor(mColorList(i))
                                Dim pipelineMaterial As New RMA.Rhino.MDisplayPipelineMaterial(material)
                                dp.DrawShadedBrep(brep, pipelineMaterial)
                                pipelineMaterial.Dispose()
                                material.Dispose()
                            Else
                                Dim pipelineMaterial As New RMA.Rhino.MDisplayPipelineMaterial(mMaterialList(i))
                                dp.DrawShadedBrep(brep, pipelineMaterial)
                                pipelineMaterial.Dispose()
                            End If

                        Case GetType(OnMesh).ToString
                            Dim mesh As OnMesh = DirectCast(mObjectList(i), OnMesh)
                            If mWireDensityList(i) >= 0 Then
                                dp.DrawWireframeMesh(mesh, mColorList(i), True)
                            End If
                            If mesh.HasVertexColors Then
                                dp.DrawShadedMesh(mesh)
                            Else
                                If mMaterialList(i) Is Nothing Then
                                    Dim material As New OnMaterial()
                                    material.m_transparency = mTransparencyList(i)
                                    material.m_diffuse = New OnColor(mColorList(i))
                                    Dim pipelineMaterial As New RMA.Rhino.MDisplayPipelineMaterial(material)
                                    dp.DrawShadedMesh(mesh, pipelineMaterial)
                                    pipelineMaterial.Dispose()
                                    material.Dispose()
                                Else
                                    Dim pipelineMaterial As New RMA.Rhino.MDisplayPipelineMaterial(mMaterialList(i))
                                    dp.DrawShadedMesh(mesh, pipelineMaterial)
                                    pipelineMaterial.Dispose()
                                End If
                            End If

                        Case GetType(OnTextDot).ToString
                            Dim textColor As UInt32 = Convert.ToUInt32(RGB(255, 255, 255))
                            Dim dot As OnTextDot = DirectCast(mObjectList(i), OnTextDot)
                            dp.DrawDot(dot.m_point, dot.m_text, mColorList(i))
                    End Select
                End If
            Next
        End If
        Return True
    End Function

#End Region


#Region " Disposing "

    Protected Overrides Sub Dispose(ByVal Param As Boolean)
        Me.ClearObjectList()
        MyBase.Dispose(Param)
    End Sub

#End Region

End Class

