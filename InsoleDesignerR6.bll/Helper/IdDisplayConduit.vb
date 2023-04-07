Imports RhinoUtils
Imports RMA.OpenNURBS
Imports RMA.Rhino


'**************************************************************************************************
'*** Classe che definisce il proprio DisplayConduit per la visualizzazione di oggetti OpenNURBS ***
'*** e gestisce la lista degli oggetti da mostrare                                              ***
'**************************************************************************************************

Public Class IdDisplayConduit
    Inherits RhDisplayConduit  'MRhinoDisplayConduit


'#Region " Fields & Initialization "

'  Dim mObjectList As List(Of Object)
'  Dim mColorList As List(Of UInteger)
'  Dim mTransparencyList As List(Of Double)
'  Dim mWireDensityList As List(Of Integer)
'  Dim mMaterialList As List(Of OnMaterial)

'  Dim mBoundingBox As OnBoundingBox

'  Public Sub New()
'    MyBase.New(New MSupportChannels(MSupportChannels.SC_CALCBOUNDINGBOX Or MSupportChannels.SC_PREDRAWOBJECTS Or MSupportChannels.SC_DRAWFOREGROUND), True)
'    mObjectList = New List(Of Object)
'    mColorList = New List(Of UInteger)
'    mTransparencyList = New List(Of Double)
'    mWireDensityList = New List(Of Integer)
'    mMaterialList = New List(Of OnMaterial)
'    mBoundingBox = New OnBoundingBox
'  End Sub

'#End Region


'#Region " APPLICATION LOGIC "

'  Private Sub UpdateBoundingBox()
'    For i As Integer = 0 To mObjectList.Count - 1
'      Select Case mObjectList(i).GetType.ToString
'        Case GetType(On3dPoint).ToString
'          Dim point As On3dPoint = DirectCast(mObjectList(i), On3dPoint)
'          mBoundingBox.Set(point, True)

'        Case GetType(OnLine).ToString
'          Dim line As OnLine = DirectCast(mObjectList(i), OnLine)
'          line.GetTightBoundingBox(mBoundingBox, 1)

'        Case GetType(OnBrep).ToString
'          Dim brep As OnBrep = DirectCast(mObjectList(i), OnBrep)
'          brep.GetTightBoundingBox(mBoundingBox, 1)

'        Case GetType(OnMesh).ToString
'          Dim mesh As OnMesh = DirectCast(mObjectList(i), OnMesh)
'          mesh.GetTightBoundingBox(mBoundingBox, 1)

'        Case GetType(OnAnnotationTextDot).ToString
'          Dim dot As OnAnnotationTextDot = DirectCast(mObjectList(i), OnAnnotationTextDot)
'          mBoundingBox.Set(dot.point, True)
'      End Select
'    Next
'  End Sub

'  Private Sub InvalidateBoundingBox()
'    If Not mBoundingBox Is Nothing Then mBoundingBox.Dispose()
'    mBoundingBox = New OnBoundingBox
'  End Sub


'  ''' <summary>
'  ''' Aggiunge un oggetto alla lista di visualizzazione
'  ''' </summary>
'  ''' <param name="constObj">Una copia di constObj verrà aggiunta alla lista</param>
'  ''' <param name="color">Colore con cui disegnare. Nel caso di Brep, Mesh e Surface se material non è nothing il colore viene ignorato</param>
'  ''' <param name="transparency">Grado di trasparenza compreso tra 0 e un massimo di 1</param>
'  ''' <param name="wireDensity">Esprime lo spessore per la linea, la densità delle isocurve per Brep e Surface (-1 per assenza curve di bordo), se >0 la presenza del wireframe nelle Mesh</param>
'  ''' <param name="material">Materiale per la renderizzazione. Se nothing vengono usati color e transparency</param>
'  ''' <remarks></remarks>
'  Public Sub AddObject(ByVal constObj As Object, Optional ByVal color As IOnColor = Nothing, Optional ByVal transparency As Double = 0.0, Optional ByVal wireDensity As Integer = -1, Optional ByVal material As IOnMaterial = Nothing)
'    Select Case constObj.GetType.ToString
'      Case GetType(On3dPoint).ToString
'        Dim point As IOn3dPoint = CType(constObj, IOn3dPoint)
'        mObjectList.Add(New On3dPoint(point))

'      Case GetType(OnLine).ToString
'        Dim line As OnLine = DirectCast(constObj, OnLine)
'        mObjectList.Add(New OnLine(line))

'      Case GetType(OnBrep).ToString
'        Dim brep As OnBrep = DirectCast(constObj, OnBrep)
'        mObjectList.Add(New OnBrep(brep))

'      Case GetType(OnMesh).ToString
'        Dim mesh As OnMesh = DirectCast(constObj, OnMesh)
'        mObjectList.Add(New OnMesh(mesh))

'      Case GetType(OnAnnotationTextDot).ToString
'        Dim dot As OnAnnotationTextDot = DirectCast(constObj, OnAnnotationTextDot)
'        mObjectList.Add(New OnAnnotationTextDot(dot))
'    End Select

'    If color Is Nothing Then
'      mColorList.Add(Convert.ToUInt32(RGB(255, 255, 255)))
'    Else
'      mColorList.Add(Convert.ToUInt32(RGB(color.Red, color.Green, color.Blue)))
'    End If
'    mTransparencyList.Add(transparency)
'    If material Is Nothing Then
'      mMaterialList.Add(Nothing)
'    Else
'      mMaterialList.Add(New OnMaterial(material))
'    End If
'    mWireDensityList.Add(wireDensity)
'    UpdateBoundingBox()
'  End Sub


'  Public Sub ClearObjectList()
'    For i As Integer = 0 To mObjectList.Count - 1
'      Select Case mObjectList(i).GetType.ToString
'        Case GetType(On3dPoint).ToString
'          Dim point As On3dPoint = DirectCast(mObjectList(i), On3dPoint)
'          point.Dispose()

'        Case GetType(OnLine).ToString
'          Dim line As OnLine = DirectCast(mObjectList(i), OnLine)
'          line.Dispose()

'        Case GetType(OnBrep).ToString
'          Dim brep As OnBrep = DirectCast(mObjectList(i), OnBrep)
'          brep.Dispose()

'        Case GetType(OnMesh).ToString
'          Dim mesh As OnMesh = DirectCast(mObjectList(i), OnMesh)
'          mesh.Dispose()

'        Case GetType(OnAnnotationTextDot).ToString
'          Dim dot As OnAnnotationTextDot = DirectCast(mObjectList(i), OnAnnotationTextDot)
'          dot.Dispose()
'      End Select
'    Next
'    mObjectList.Clear()
'    mColorList.Clear()
'    mTransparencyList.Clear()
'    For i As Integer = 0 To mMaterialList.Count - 1
'      If Not mMaterialList(i) Is Nothing Then mMaterialList(i).Dispose()
'    Next
'    mMaterialList.Clear()
'    mWireDensityList.Clear()
'    InvalidateBoundingBox()
'  End Sub

'#End Region


'#Region " Drawing "

'  Public Overrides Function ExecConduit(ByRef dp As RMA.Rhino.MRhinoDisplayPipeline, ByVal current_channel As UInteger, ByRef termination_flag As Boolean) As Boolean

'    If current_channel = MSupportChannels.SC_CALCBOUNDINGBOX Then
'      m_pChannelAttrs.m_BoundingBox.Union(mBoundingBox)

'    ElseIf current_channel = MSupportChannels.SC_PREDRAWOBJECTS Then
'      For i As Integer = 0 To mObjectList.Count - 1
'        Select Case mObjectList(i).GetType.ToString
'          Case GetType(On3dPoint).ToString
'            Dim point As On3dPoint = DirectCast(mObjectList(i), On3dPoint)
'            dp.DrawPoint(point)

'          Case GetType(OnLine).ToString
'            Dim line As OnLine = DirectCast(mObjectList(i), OnLine)
'            dp.DrawLine(line.from, line.to, mColorList(i), mWireDensityList(i))

'          Case GetType(OnBrep).ToString, GetType(OnSurface).ToString
'            Dim brep As OnBrep
'            If mObjectList(i).GetType.ToString = GetType(OnSurface).ToString Then
'              brep = DirectCast(mObjectList(i), OnSurface).BrepForm
'            Else
'              brep = DirectCast(mObjectList(i), OnBrep)
'            End If
'            If mWireDensityList(i) >= 0 Then
'              dp.DrawBrep(brep, mColorList(i), mWireDensityList(i))
'            End If
'            If mMaterialList(i) Is Nothing Then
'              Dim material As New OnMaterial()
'              material.m_transparency = mTransparencyList(i)
'              material.m_diffuse = New OnColor(mColorList(i))
'              Dim pipelineMaterial As New RMA.Rhino.MDisplayPipelineMaterial(material)
'              dp.DrawShadedBrep(brep, pipelineMaterial)
'              pipelineMaterial.Dispose()
'              material.Dispose()
'            Else
'              Dim pipelineMaterial As New RMA.Rhino.MDisplayPipelineMaterial(mMaterialList(i))
'              dp.DrawShadedBrep(brep, pipelineMaterial)
'              pipelineMaterial.Dispose()
'            End If

'          Case GetType(OnMesh).ToString
'            Dim mesh As OnMesh = DirectCast(mObjectList(i), OnMesh)
'            If mWireDensityList(i) >= 0 Then
'              dp.DrawWireframeMesh(mesh, mColorList(i), True)
'            End If
'            If mesh.HasVertexColors Then
'              dp.DrawShadedMesh(mesh)
'            Else
'              If mMaterialList(i) Is Nothing Then
'                Dim material As New OnMaterial()
'                material.m_transparency = mTransparencyList(i)
'                material.m_diffuse = New OnColor(mColorList(i))
'                Dim pipelineMaterial As New RMA.Rhino.MDisplayPipelineMaterial(material)
'                dp.DrawShadedMesh(mesh, pipelineMaterial)
'                pipelineMaterial.Dispose()
'                material.Dispose()
'              Else
'                Dim pipelineMaterial As New RMA.Rhino.MDisplayPipelineMaterial(mMaterialList(i))
'                dp.DrawShadedMesh(mesh, pipelineMaterial)
'                pipelineMaterial.Dispose()
'              End If
'            End If
'        End Select
'      Next

'    ElseIf current_channel = MSupportChannels.SC_DRAWFOREGROUND Then
'      For i As Integer = 0 To mObjectList.Count - 1
'        Select Case mObjectList(i).GetType.ToString
'          Case GetType(OnAnnotationTextDot).ToString
'            Dim textColor As UInt32 = Convert.ToUInt32(RGB(255, 255, 255))
'            Dim dot As OnAnnotationTextDot = DirectCast(mObjectList(i), OnAnnotationTextDot)
'            dp.DrawDot(dot.point, dot.m_text, mColorList(i))
'        End Select
'      Next
'    End If

'    Return True
'  End Function

'#End Region


End Class
