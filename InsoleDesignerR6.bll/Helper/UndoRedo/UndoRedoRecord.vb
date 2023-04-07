Imports RMA.Rhino
Imports InsoleDesigner.bll.IdElement3dManager
Imports InsoleDesigner.bll.IdAlias
Imports RMA.OpenNURBS


Public Class UndoRedoRecord
    'Implements IDisposable


#Region " Field "

    Private mEnglishCmdName As String
    Private mLayers As List(Of OnLayer)
    Private mObjects As List(Of MRhinoObject)
    Private mCurrentLayer As Integer
    'IdElement3dHelper
    Private mReferenceGuids(,) As Guid
    Private mFootCurvesLeft As SortedList(Of Double, Guid)
    Private mFootCurvesRight As SortedList(Of Double, Guid)
    Private mFootCurvesTrimSrfLeft As Dictionary(Of eTrimSurface, OnSurface)
    Private mFootCurvesTrimSrfRight As Dictionary(Of eTrimSurface, OnSurface)
    Private mAddictionsLeft As List(Of IdAddiction)
    Private mAddictionsRight As List(Of IdAddiction)
    Private mModelPressureMap As IdPressureMap
    Private mPianoInclinatoLeft As AbstractPianiInclinati
    Private mPianoInclinatoRight As AbstractPianiInclinati

#End Region


#Region " Constructor "

    Public Sub New(englishCmdName As String)
        Me.mEnglishCmdName = englishCmdName
        'Copio
        CloneRhinoDoc()
        CloneElement3dHelper()
    End Sub


    Private Sub CloneRhinoDoc()
        mCurrentLayer = RhUtil.RhinoApp.ActiveDoc.m_layer_table.CurrentLayerIndex
        mLayers = New List(Of OnLayer)
        mObjects = New List(Of MRhinoObject)
        Dim layers() As IRhinoLayer = {}
        RhUtil.RhinoApp.ActiveDoc.m_layer_table.GetSortedList(layers)
        For i As Int32 = 0 To layers.Length - 1
            Dim obj_list() As MRhinoObject = {}
            RhUtil.RhinoApp.ActiveDoc.LookupObject(layers(i), obj_list)
            For Each rhinoObj As MRhinoObject In obj_list
                mObjects.Add(rhinoObj.DuplicateRhinoObject)
            Next
            mLayers.Add(New OnLayer(layers(i)))
        Next
    End Sub


    Private Sub CloneElement3dHelper()
        Dim helper As IdElement3dManager = IdElement3dManager.GetInstance()
        Me.mReferenceGuids = MakeCopyOfReferences(helper.References())
        CloneElement3dHelper(eSide.left)
        CloneElement3dHelper(eSide.right)
        'Mappe pressione che gestisce internamente entrambi i side
        mModelPressureMap = helper.ModelPressureMap.Clone()
    End Sub

    Private Sub CloneElement3dHelper(ByVal side As eSide)
        Dim helper As IdElement3dManager = IdElement3dManager.GetInstance()
        If side = eSide.left Then
            Me.mFootCurvesLeft = MakeCopyOfFootCurves(helper.GetFootCurves(side))
            Me.mFootCurvesTrimSrfLeft = MakeCopyOfFootCurvesTrimSrf(helper.GetFootCurvesTrimSurface(side))
            Me.mAddictionsLeft = MakeCopyOfAddcition(helper.GetAddictions(side))
            Me.mPianoInclinatoLeft = MakeCopyOfPianoInclinato(side)
        Else
            Me.mFootCurvesRight = MakeCopyOfFootCurves(helper.GetFootCurves(side))
            Me.mFootCurvesTrimSrfRight = MakeCopyOfFootCurvesTrimSrf(helper.GetFootCurvesTrimSurface(side))
            Me.mAddictionsRight = MakeCopyOfAddcition(helper.GetAddictions(side))
            Me.mPianoInclinatoRight = MakeCopyOfPianoInclinato(side)
        End If
    End Sub


#End Region


#Region " Property "

    Public ReadOnly Property ReferenceGuid(ByVal refType As eReferences, ByVal side As eSide) As Guid
        Get
            Return New Guid(Me.mReferenceGuids(refType, side).ToString)
        End Get
    End Property

    Public ReadOnly Property ReferenceGuids() As Guid(,)
        Get
            Return MakeCopyOfReferences(Me.mReferenceGuids)
        End Get
    End Property

    Public ReadOnly Property FootCurvesTrimSurface(ByVal side As eSide) As Dictionary(Of eTrimSurface, OnSurface)
        Get
            If side = eSide.left Then
                Return MakeCopyOfFootCurvesTrimSrf(Me.mFootCurvesTrimSrfLeft)
            Else
                Return MakeCopyOfFootCurvesTrimSrf(Me.mFootCurvesTrimSrfRight)
            End If
        End Get
    End Property

    Public ReadOnly Property FootCurves(ByVal side As eSide) As SortedList(Of Double, Guid)
        Get
            If side = eSide.left Then
                Return MakeCopyOfFootCurves(Me.mFootCurvesLeft)
            Else
                Return MakeCopyOfFootCurves(Me.mFootCurvesRight)
            End If
        End Get
    End Property

    Public ReadOnly Property GetAddictions(ByVal side As eSide) As List(Of IdAddiction)
        Get
            If side = eSide.left Then
                Return MakeCopyOfAddcition(Me.mAddictionsLeft)
            Else
                Return MakeCopyOfAddcition(Me.mAddictionsRight)
            End If
        End Get
    End Property

    Public ReadOnly Property Layers() As List(Of OnLayer)
        Get
            Dim result As New List(Of OnLayer)
            For Each layer As OnLayer In mLayers
                result.Add(New OnLayer(layer))
            Next
            Return result
        End Get
    End Property

    Public ReadOnly Property CurrentLayer() As Integer
        Get
            Return mCurrentLayer
        End Get
    End Property

    Public ReadOnly Property Objects() As List(Of MRhinoObject)
        Get
            Dim result As New List(Of MRhinoObject)
            For Each rhinoobj As MRhinoObject In mObjects
                result.Add(rhinoobj.DuplicateRhinoObject)
            Next
            Return result
        End Get
    End Property

    Public ReadOnly Property EnglishCmdName() As String
        Get
            Return mEnglishCmdName
        End Get
    End Property

    Public ReadOnly Property GetModelPressureMap() As IdPressureMap
        Get
            Return mModelPressureMap.Clone()
        End Get
    End Property

    Public ReadOnly Property GetPianoInclinato(side As eSide) As AbstractPianiInclinati
        Get
            Dim target = IIf(side = eSide.left, mPianoInclinatoLeft, mPianoInclinatoRight)
            If target Is Nothing Then Return Nothing
            Return target.Clone()
        End Get
    End Property

#End Region


#Region " Copie "

    Private Shared Function MakeCopyOfReferences(ByRef references(,) As Guid) As Guid(,)
        Dim result(,) As Guid
        ReDim result([Enum].GetValues(GetType(eReferences)).Length - 1, 1)
        For i As Integer = 0 To [Enum].GetValues(GetType(eReferences)).Length - 1
            For j As Integer = 0 To [Enum].GetValues(GetType(eSide)).Length - 1
                result(i, j) = New Guid(references(i, j).ToString)
            Next
        Next
        Return result
    End Function

    Private Shared Function MakeCopyOfFootCurves(ByRef footCurves As SortedList(Of Double, Guid)) As SortedList(Of Double, Guid)
        Dim result As New SortedList(Of Double, Guid)
        For Each pair As KeyValuePair(Of Double, Guid) In footCurves
            result.Add(pair.Key, New Guid(pair.Value.ToString()))
        Next
        Return result
    End Function

    Private Shared Function MakeCopyOfFootCurvesTrimSrf(ByRef trimSurface As Dictionary(Of eTrimSurface, OnSurface)) As Dictionary(Of eTrimSurface, OnSurface)
        Dim result As New Dictionary(Of eTrimSurface, OnSurface)
        For Each pair As KeyValuePair(Of eTrimSurface, OnSurface) In trimSurface
            result.Add(pair.Key, pair.Value.DuplicateSurface)
        Next
        Return result
    End Function

    Private Shared Function MakeCopyOfAddcition(ByRef addictions As List(Of IdAddiction)) As List(Of IdAddiction)
        Dim result As New List(Of IdAddiction)
        For Each addiction As IdAddiction In addictions
            result.Add(CType(addiction.Clone(), IdAddiction))
        Next
        Return result
    End Function

    Private Shared Function MakeCopyOfPianoInclinato(side As eSide) As AbstractPianiInclinati
        If Element3dManager.PianoInclinatoExist(side) Then
            Return Element3dManager.GetPianoInclinato(side).Clone()
        Else 
            Return Nothing
        End If        
    End Function

#End Region


#Region "IDisposable Support"
    'Private disposedValue As Boolean ' Per rilevare chiamate ridondanti

    '' IDisposable
    'Protected Overridable Sub Dispose(disposing As Boolean)
    '    If Not Me.disposedValue Then
    '        If disposing Then
    '            ' TODO: eliminare stato gestito (oggetti gestiti).
    '        End If

    '        ' TODO: liberare risorse non gestite (oggetti non gestiti) ed eseguire l'override del seguente Finalize().
    '        'Layers
    '        While mLayers.Count > 0
    '            Dim item As OnLayer = mLayers.Item(0)
    '            mLayers.RemoveAt(0)
    '            If item IsNot Nothing Then item.Dispose()
    '        End While
    '        'Oggetti Rhino
    '        While mObjects.Count > 0
    '            Dim item As MRhinoObject = mObjects.Item(0)
    '            mObjects.RemoveAt(0)
    '            If item IsNot Nothing Then item.Dispose()
    '        End While
    '        'Trim Surface
    '        While mFootCurvesTrimSrfLeft.Count > 0
    '            Dim key As eTrimSurface = mFootCurvesTrimSrfLeft.Keys(0)
    '            Dim item As OnSurface = mFootCurvesTrimSrfLeft.Item(key)
    '            mFootCurvesTrimSrfLeft.Remove(key)
    '            If item IsNot Nothing Then item.Dispose()
    '        End While
    '        While mFootCurvesTrimSrfRight.Count > 0
    '            Dim key As eTrimSurface = mFootCurvesTrimSrfRight.Keys(0)
    '            Dim item As OnSurface = mFootCurvesTrimSrfRight.Item(key)
    '            mFootCurvesTrimSrfRight.Remove(key)
    '            If item IsNot Nothing Then item.Dispose()
    '        End While
    '        'Scarichi
    '        While mAddictionsLeft.Count() > 0
    '            Dim item As IdAddiction = mAddictionsLeft.Item(0)
    '            mAddictionsLeft.RemoveAt(0)
    '            If item IsNot Nothing Then item.Dispose()
    '        End While
    '        While mAddictionsRight.Count() > 0
    '            Dim item As IdAddiction = mAddictionsRight.Item(0)
    '            mAddictionsRight.RemoveAt(0)
    '            If item IsNot Nothing Then item.Dispose()
    '        End While

    '        ' TODO: impostare campi di grandi dimensioni su null.
    '    End If
    '    Me.disposedValue = True
    'End Sub

    '' TODO: eseguire l'override di Finalize() solo se Dispose(ByVal disposing As Boolean) dispone del codice per liberare risorse non gestite.
    'Protected Overrides Sub Finalize()
    '    ' Non modificare questo codice. Inserire il codice di pulizia in Dispose(ByVal disposing As Boolean).
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    '' Questo codice è aggiunto da Visual Basic per implementare in modo corretto il modello Disposable.
    'Public Sub Dispose() Implements IDisposable.Dispose
    '    ' Non modificare questo codice. Inserire il codice di pulizia in Dispose(ByVal disposing As Boolean).
    '    Dispose(True)
    '    GC.SuppressFinalize(Me)
    'End Sub
#End Region



End Class
