Imports System.IO
Imports RMA.OpenNURBS
Imports InsoleDesigner.bll.IdAlias
Imports InsoleDesigner.bll.IdAddiction
Imports InsoleDesigner.bll.AbstractPianiInclinati
Imports System.Reflection
Imports RhinoUtils
Imports RMA.Rhino


Public Class IdElement3dManager
    Implements IOnSerializable


#Region " ENUM "

    Public Enum eSide
        left = 0
        right = 1
    End Enum

    ''' <summary>
    ''' ATTENZIONE: per compatibilità file salvati in vecchie versioni aggiungere nuove voci sempre in fondo, mai in mezzo
    ''' </summary>
    Public Enum eReferences As Integer
        footMesh = 0
        lastTopSurface
        lastLateralSurface
        lastBottomSurface
        lastTotalSurface
        userInternalUpperCurve
        userExternalUpperCurve
        finalUpperCurve
        sweepCuttingSurface
        tcoProfileSurface
        tcoProfileCurve
        manufacturingLateralSurface
        manufacturingBottomSurface
        insoleTopSurface
        insoleTopBlendSurface
        insoleLateralSurface
        insoleBottomBlendSurface
        insoleBottomSurface
        insoleFinalSurface
        manufacturingTopSurface
        ''aggiungere qui!!!
    End Enum

    Public Enum eLayerType
        root = 0
        foot
        last
        insole
        manufacturing
        addiction
        ruler
        sottopiede
        TcoProfile
        template
        pressureMap
        pianoInclinato
    End Enum

    Public Enum eTrimSurface
        topInt
        topExt
    End Enum


#End Region


#Region " Field "

    Private Shared mInstance As IdElement3dManager
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


    Private Sub New()
        ResetReferences()
        mFootCurvesLeft = New SortedList(Of Double, Guid)
        mFootCurvesRight = New SortedList(Of Double, Guid)
        mFootCurvesTrimSrfLeft = New Dictionary(Of eTrimSurface, OnSurface)
        mFootCurvesTrimSrfRight = New Dictionary(Of eTrimSurface, OnSurface)
        mAddictionsLeft = New List(Of IdAddiction)
        mAddictionsRight = New List(Of IdAddiction)
        mModelPressureMap = New IdPressureMap
    End Sub

    Public Shared Function GetInstance() As IdElement3dManager
        If mInstance Is Nothing Then
            mInstance = New IdElement3dManager()
        End If
        Return mInstance
    End Function

#End Region


#Region " Riferimenti gloable "


    Public Function GetRhinoObj(ByVal refType As eReferences, ByVal side As eSide) As MRhinoObject
        Dim index As Integer = CInt(refType)
        Dim id As Guid = mReferenceGuids(index, side)
        Return RhUtil.RhinoApp.ActiveDoc.LookupObject(id)
    End Function

    Public Function GetRhinoObjRef(ByVal refType As eReferences, ByVal side As eSide) As MRhinoObjRef
        If ObjectExist(refType, side) Then
            Dim index As Integer = CInt(refType)
            Dim id As Guid = mReferenceGuids(index, side)
            Return New MRhinoObjRef(id)
        End If
        Return Nothing
    End Function

    Public Function GetRhinoObjID(ByVal refType As eReferences, ByVal side As eSide) As Guid
        Dim index As Integer = CInt(refType)
        Return mReferenceGuids(index, side)
    End Function

    ''' <summary>
    ''' Imposta il nuovo guid dell'oggetto
    ''' </summary>
    ''' <param name="refType"></param>
    ''' <param name="side"></param>
    ''' <param name="uuid"></param>
    ''' <remarks></remarks>
    Public Sub SetRhinoObj(ByVal refType As eReferences, ByVal side As eSide, ByVal uuid As Guid, Optional ByVal deleteOld As Boolean = False)
        Dim index As Integer = CInt(refType)
        'Cancello vecchio
        If deleteOld Then
            Dim oldId As Guid = Me.mReferenceGuids(index, side)
            If RhUtil.RhinoApp.ActiveDoc.LookupDocumentObject(oldId, True) IsNot Nothing Then
                RhUtil.RhinoApp.ActiveDoc.DeleteObject(New MRhinoObjRef(oldId))
            End If
        End If
        'Aggiorno
        Me.mReferenceGuids(index, side) = uuid
        SetObjectName(refType, side, uuid)
    End Sub


    Public Function ObjectExist(ByVal refType As eReferences) As Boolean
        Return ObjectExist(refType, eSide.left) Or ObjectExist(refType, eSide.right)
    End Function

    ''' <summary>
    ''' Controlla che l'oggetto esista nel documento
    ''' </summary>
    Public Function ObjectExist(ByVal refType As eReferences, ByVal side As eSide) As Boolean
        Dim index As Integer = CInt(refType)
        Dim id As Guid = mReferenceGuids(index, side)
        If RhUtil.RhinoApp.ActiveDoc.LookupObject(id) Is Nothing Then Return False
        Return True
    End Function

    ''' <summary>
    ''' Cicla tutti gli oggetti salvati per trovare il relativo side
    ''' </summary>
    ''' <param name="uuid"></param>
    ''' <returns>Ritorna -1 se non trova il lato</returns>
    ''' <remarks></remarks>
    Public Function FindObjectSide(ByVal uuid As Guid) As eSide
        For i As Integer = 0 To CInt((mReferenceGuids.Length / 2)) - 1
            For j As Integer = 0 To 1
                If mReferenceGuids(i, j) = uuid Then
                    Return CType(j, eSide)
                End If
            Next
        Next
        'Return eSide.left
        Return CType(-1, eSide)
    End Function

    ''' <summary>
    ''' Cicla tutti gli oggetti salvati per trovare il relativo tipo
    ''' </summary>
    ''' <param name="uuid"></param>
    ''' <returns>Ritorna -1 se non lo trova</returns>
    ''' <remarks></remarks>
    Public Function FindObjectType(ByVal uuid As Guid) As eReferences
        For i As Integer = 0 To CInt((mReferenceGuids.Length / 2)) - 1
            For j As Integer = 0 To 1
                If mReferenceGuids(i, j) = uuid Then
                    Return CType(i, eReferences)
                End If
            Next
        Next
        'Return eSide.left
        Return CType(-1, eReferences)
    End Function

    Public Sub ShowObject(ByVal refType As eReferences, ByVal side As eSide, ByVal visible As Boolean)
        If GetRhinoObjRef(refType, side) Is Nothing Then Exit Sub
        If visible Then
            Doc.ShowObject(GetRhinoObjRef(refType, side), True)
        Else
            Doc.HideObject(GetRhinoObjRef(refType, side), True)
        End If
    End Sub

    Public Sub ReferenceSwitchType(ByVal fromType As eReferences, ByVal toType As eReferences, ByVal side As eSide)
        SetRhinoObj(toType, side, GetRhinoObjID(fromType, side), False)
        ClearRhinoObjRef(fromType, side)
    End Sub

    ''' <summary>
    ''' Controlla che l'id sia già presente tra i reference
    ''' </summary>
    Public Function ReferenceExist(ByVal uuid As Guid) As Boolean
        For Each id As Guid In mReferenceGuids
            If id = uuid Then Return True
        Next
        Return False
    End Function

    Public Sub ClearRhinoObjRef(ByVal refType As eReferences, ByVal side As eSide)
        Dim index As Integer = CInt(refType)
        mReferenceGuids(index, side) = Guid.Empty
    End Sub

    Public Sub ClearRhinoObjSideRef(ByVal side As eSide)
        For i As Integer = 0 To [Enum].GetValues(GetType(eReferences)).Length - 1
            mReferenceGuids(i, side) = Guid.Empty
        Next
    End Sub

    Public Sub ResetReferences()
        ReDim Me.mReferenceGuids([Enum].GetValues(GetType(eReferences)).Length - 1, 1)
    End Sub

    Public ReadOnly Property References() As Guid(,)
        Get
            Return mReferenceGuids
        End Get
    End Property


#End Region


#Region " Mesh piede "


    Public Sub ShowFoot(ByVal visible As Boolean, ByVal side As eSide)
        RhLayer.RendiVisibileLayer(GetLayerName(side, eLayerType.foot), visible)
    End Sub

    ''' <summary>
    ''' Visualizza o nasconde i piedi
    ''' </summary>
    Public Sub ShowFoot(ByVal visible As Boolean)
        ShowFoot(visible, eSide.left)
        ShowFoot(visible, eSide.right)
    End Sub

#End Region


#Region " Forme scarpa "


    Public Sub ShowLast(ByVal visible As Boolean, ByVal side As eSide)
        'If ObjectExist(eReferences.lastLateralSurface, side) Then
        '    Dim objRef As MRhinoObjRef = GetRhinoObjRef(eReferences.lastLateralSurface, side)
        '    If visible Then
        '        RhUtil.RhinoApp.ActiveDoc.ShowObject(objRef, True)
        '    Else
        '        RhUtil.RhinoApp.ActiveDoc.HideObject(objRef, True)
        '    End If
        '    objRef.Dispose()
        'End If
        'If ObjectExist(eReferences.lastBottomSurface, side) Then
        '    Dim objRef As MRhinoObjRef = GetRhinoObjRef(eReferences.lastBottomSurface, side)
        '    If visible Then
        '        RhUtil.RhinoApp.ActiveDoc.ShowObject(objRef, True)
        '    Else
        '        RhUtil.RhinoApp.ActiveDoc.HideObject(objRef, True)
        '    End If
        '    objRef.Dispose()
        'End If
        'If ObjectExist(eReferences.lastTotalSurface, side) Then
        '    Dim objRef As MRhinoObjRef = GetRhinoObjRef(eReferences.lastTotalSurface, side)
        '    If visible Then
        '        RhUtil.RhinoApp.ActiveDoc.ShowObject(objRef, True)
        '    Else
        '        RhUtil.RhinoApp.ActiveDoc.HideObject(objRef, True)
        '    End If
        '    objRef.Dispose()
        'End If
        RhLayer.RendiVisibileLayer(GetLayerName(side, eLayerType.last), visible)
    End Sub

    Public Sub ShowLast(ByVal visible As Boolean)
        ShowLast(visible, eSide.left)
        ShowLast(visible, eSide.right)
    End Sub

    Public Sub DeleteLateralLast(ByVal side As eSide)
        If Not ObjectExist(eReferences.lastLateralSurface, side) Then Exit Sub
        Dim id As Guid = GetRhinoObjID(eReferences.lastLateralSurface, side)
        If Not RhUtil.RhinoApp.ActiveDoc.LookupObject(id) Is Nothing Then RhUtil.RhinoApp.ActiveDoc.DeleteObject(New MRhinoObjRef(id), True, True)
    End Sub

#End Region


#Region " Lavorazione manuale "


    Public Sub ShowLavorazioneManuale(ByVal visible As Boolean, ByVal side As eSide)
        RhLayer.RendiVisibileLayer(GetLayerName(side, eLayerType.manufacturing), visible)
    End Sub

    Public Sub ShowLavorazioneManuale(ByVal visible As Boolean)
        ShowLavorazioneManuale(visible, eSide.left)
        ShowLavorazioneManuale(visible, eSide.right)
    End Sub


#End Region


#Region " Curve di sezione del piede "

    ''' <summary>
    ''' Esiste almeno un ID salvato e tramite l'ID stesso si recupera un oggetto presente nel documento
    ''' </summary>
    ''' <param name="side"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function FootCurvesExists(ByVal side As eSide) As Boolean
        If side = eSide.left Then
            If mFootCurvesLeft.Count > 0 Then
                For Each uuid As Guid In mFootCurvesLeft.Values
                    If RhUtil.RhinoApp.ActiveDoc.LookupObject(uuid) IsNot Nothing Then Return True
                Next
                Return False
            End If
        Else
            If mFootCurvesRight.Count > 0 Then
                For Each uuid As Guid In mFootCurvesRight.Values
                    If RhUtil.RhinoApp.ActiveDoc.LookupObject(uuid) IsNot Nothing Then Return True
                Next
                Return False
            End If
        End If
        Return False
    End Function

    Public Function FootCurvesExists() As Boolean
        If FootCurvesExists(eSide.left) Then Return True
        If FootCurvesExists(eSide.right) Then Return True
        Return False
    End Function

    Public Function GetFootCurves(ByVal side As eSide) As SortedList(Of Double, Guid)
        If side = eSide.left Then
            Return mFootCurvesLeft
        Else
            Return mFootCurvesRight
        End If
    End Function

    Public Sub SetFootCurves(ByVal side As eSide, ByVal curves As SortedList(Of Double, Guid))
        If side = eSide.left Then
            mFootCurvesLeft = curves
        Else
            mFootCurvesRight = curves
        End If
        For Each position As Double In curves.Keys
            SetFootCurveName(side, position)
        Next
    End Sub

    ''' <summary>
    ''' Aggiunge l'Id e cancella l'eventuale vecchia curva dal Doc
    ''' </summary>
    ''' <param name="side"></param>
    ''' <param name="position"></param>
    ''' <param name="uuid"></param>
    ''' <remarks></remarks>
    Public Sub AddFootCurve(ByVal side As eSide, ByVal position As Double, ByVal uuid As Guid)
        If side = eSide.left Then
            If mFootCurvesLeft.ContainsKey(position) Then RemoveFootCurveRef(side, position)
            mFootCurvesLeft.Add(position, uuid)
        Else
            If mFootCurvesRight.ContainsKey(position) Then RemoveFootCurveRef(side, position)
            mFootCurvesRight.Add(position, uuid)
        End If
        SetFootCurveName(side, position)
    End Sub

    Public Sub ReplaceFootCurve(ByVal side As eSide, ByVal position As Double, ByVal uuid As Guid)
        AddFootCurve(side, position, uuid)
    End Sub

    ''' <summary>
    ''' NON USARE QUESTO METODO NEI CICLI!
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub ReplaceFootCurve(ByVal side As eSide, ByVal oldGuid As Guid, ByVal newGuid As Guid)
        Dim targetList As SortedList(Of Double, Guid) = mFootCurvesLeft
        If side = eSide.right Then targetList = mFootCurvesRight
        If targetList.ContainsValue(oldGuid) Then
            Dim index As Integer = targetList.IndexOfValue(oldGuid)
            Dim key As Double = targetList.Keys.Item(index)
            targetList.Item(key) = newGuid
            SetFootCurveName(side, key)
        End If
    End Sub

    ''' <summary>
    ''' I nomi sono stati impostati vuoti per lasciare libertà all'utente. Ma sono impostati nel momento in cui vengono salvati i template
    ''' per successivo riconoscimento. Appena importati gli oggetti vengono nuovamente impostati con nome vuoto
    ''' </summary>
    ''' <param name="side"></param>
    ''' <param name="position"></param>
    Public Sub SetFootCurveName(ByVal side As eSide, ByVal position As Double)
        Dim targetId As Guid = Nothing
        If side = eSide.left Then
            targetId = mFootCurvesLeft.Item(position)
        Else
            targetId = mFootCurvesRight.Item(position)
        End If
        Dim rhinoObjInterface As IRhinoObject = RhUtil.RhinoApp.ActiveDoc.LookupObject(targetId)
        If rhinoObjInterface Is Nothing Then Exit Sub
        Dim objAttr As New MRhinoObjectAttributes(rhinoObjInterface.Attributes)
        objAttr.m_name = "" 'GetFootCurveName(side, position)
        RhUtil.RhinoApp.ActiveDoc.ModifyObjectAttributes(New MRhinoObjRef(rhinoObjInterface.Attributes.m_uuid), objAttr)
        objAttr.Dispose()
    End Sub

    Public Sub RemoveFootCurveRef(ByVal side As eSide, ByVal position As Double, Optional ByVal alsoDeleteInDoc As Boolean = True)
        Dim uuid As Guid = Nothing
        If side = eSide.left Then
            If mFootCurvesLeft.ContainsKey(position) Then
                uuid = mFootCurvesLeft.Item(position)
                mFootCurvesLeft.Remove(position)
            End If
        Else
            If mFootCurvesRight.ContainsKey(position) Then
                uuid = mFootCurvesRight.Item(position)
                mFootCurvesRight.Remove(position)
            End If
        End If
        If alsoDeleteInDoc AndAlso RhUtil.RhinoApp.ActiveDoc.LookupObject(uuid) IsNot Nothing Then RhUtil.RhinoApp.ActiveDoc.DeleteObject(New MRhinoObjRef(uuid), True, True)
    End Sub

    Public Sub RemoveFootCurveRef(ByVal uuid As Guid, Optional ByVal alsoDeleteInDoc As Boolean = True)
        Dim side As eSide = GetFootCurveSide(uuid)
        Dim position As Double = GetFootCurvePosition(side, uuid)
        If position > -1 Then RemoveFootCurveRef(side, position, alsoDeleteInDoc)
    End Sub

    Public Sub DeleteFootCurves(ByVal side As eSide, Optional ByVal alsoDeleteInDoc As Boolean = True)
        If alsoDeleteInDoc Then
            For Each uuid As Guid In GetFootCurves(side).Values
                If RhUtil.RhinoApp.ActiveDoc.LookupObject(uuid) IsNot Nothing Then
                    RhUtil.RhinoApp.ActiveDoc.DeleteObject(New MRhinoObjRef(uuid), True, True)
                End If
            Next
        End If
        ClearFootCurvesRef(side)
    End Sub

    Public Sub DeleteAllFootCurves(Optional ByVal alsoDeleteInDoc As Boolean = True)
        DeleteFootCurves(eSide.left, alsoDeleteInDoc)
        DeleteFootCurves(eSide.right, alsoDeleteInDoc)
    End Sub

    Private Sub ClearFootCurvesRef(ByVal side As eSide)
        If side = eSide.left Then
            mFootCurvesLeft.Clear()
        Else
            mFootCurvesRight.Clear()
        End If
    End Sub

    Public Sub ShowFootCurve(ByVal visible As Boolean, ByVal side As eSide)
        If Not FootCurvesExists(side) Then Exit Sub
        For Each uuid As Guid In GetFootCurves(side).Values
            Dim objRef As New MRhinoObjRef(uuid)
            If objRef IsNot Nothing AndAlso objRef.Object IsNot Nothing Then
                If visible Then
                    RhUtil.RhinoApp.ActiveDoc.ShowObject(objRef, True)
                Else
                    RhUtil.RhinoApp.ActiveDoc.HideObject(objRef, True)
                End If
            End If
            objRef.Dispose()
        Next
    End Sub

    Public Sub ShowFootCurve(ByVal visible As Boolean)
        ShowFootCurve(visible, eSide.left)
        ShowFootCurve(visible, eSide.right)
    End Sub


    Public Function FootCurveBbox(ByVal side As eSide) As OnBoundingBox
        Dim result As New OnBoundingBox()
        If side = eSide.left Then
            For Each uuid As Guid In mFootCurvesLeft.Values
                Dim objRef As New MRhinoObjRef(uuid)
                If objRef IsNot Nothing AndAlso objRef.Object IsNot Nothing AndAlso objRef.Curve() IsNot Nothing Then
                    result.Union(objRef.Curve().BoundingBox)
                End If
            Next
        Else
            For Each uuid As Guid In mFootCurvesRight.Values
                Dim objRef As New MRhinoObjRef(uuid)
                If objRef IsNot Nothing AndAlso objRef.Object IsNot Nothing AndAlso objRef.Curve() IsNot Nothing Then
                    result.Union(objRef.Curve().BoundingBox)
                End If
            Next
        End If
        Return result
    End Function


    Public Function IsFootCurve(ByVal side As eSide, ByVal uuid As Guid) As Boolean
        If side = eSide.left Then
            If mFootCurvesLeft.ContainsValue(uuid) Then Return True
        Else
            If mFootCurvesRight.ContainsValue(uuid) Then Return True
        End If
        Return False
    End Function

    Public Function IsFootCurve(ByVal uuid As Guid) As Boolean
        Return IsFootCurve(eSide.left, uuid) Or IsFootCurve(eSide.right, uuid)
    End Function

    Public Function GetFootCurveSide(ByVal uuid As Guid) As eSide
        If Not IsFootCurve(uuid) Then Return CType(-1, eSide)
        If IsFootCurve(eSide.left, uuid) Then Return eSide.left
        If IsFootCurve(eSide.right, uuid) Then Return eSide.right
        Return CType(-1, eSide)
    End Function

    Public Function GetFootCurvePosition(ByVal side As eSide, ByVal uuid As Guid) As Double
        Dim result As Double = -1
        If Not IsFootCurve(side, uuid) Then Return result
        Dim targetList As SortedList(Of Double, Guid) = mFootCurvesLeft
        If side = eSide.right Then targetList = mFootCurvesRight
        For Each pair As KeyValuePair(Of Double, Guid) In targetList
            If pair.Value = uuid Then result = pair.Key
        Next
        Return result
    End Function

#End Region


#Region " Superfici plantare "

    Public Sub ShowInsole(ByVal visible As Boolean, ByVal side As eSide)
        RhLayer.RendiVisibileLayer(GetLayerName(side, eLayerType.insole), visible)
        If visible Then
            ShowObject(eReferences.insoleTopSurface, side, True)
            ShowObject(eReferences.insoleTopBlendSurface, side, True)
            ShowObject(eReferences.insoleLateralSurface, side, True)
            ShowObject(eReferences.insoleBottomBlendSurface, side, True)
            ShowObject(eReferences.insoleBottomSurface, side, True)
        End If
    End Sub

    Public Sub ShowInsole(ByVal visible As Boolean)
        ShowInsole(visible, eSide.left)
        ShowInsole(visible, eSide.right)
    End Sub

#End Region


#Region " Upper curve "


    Public Function UserUpperCurvesExists() As Boolean
        If ObjectExist(eReferences.userInternalUpperCurve, eSide.left) Then Return True
        If ObjectExist(eReferences.userInternalUpperCurve, eSide.right) Then Return True
        If ObjectExist(eReferences.userExternalUpperCurve, eSide.left) Then Return True
        If ObjectExist(eReferences.userExternalUpperCurve, eSide.right) Then Return True
        Return False
    End Function

    Public Sub ShowFinalUpperCurve(ByVal visible As Boolean, ByVal side As eSide)
        If Not ObjectExist(eReferences.finalUpperCurve, side) Then Exit Sub
        Dim objRef As MRhinoObjRef = GetRhinoObjRef(eReferences.finalUpperCurve, side)
        If visible Then
            RhUtil.RhinoApp.ActiveDoc.ShowObject(objRef, True)
        Else
            RhUtil.RhinoApp.ActiveDoc.HideObject(objRef, True)
        End If
        objRef.Dispose()
    End Sub

    Public Sub ShowFinalUpperCurve(ByVal visible As Boolean)
        ShowFinalUpperCurve(visible, eSide.left)
        ShowFinalUpperCurve(visible, eSide.right)
    End Sub

    Public Sub ShowUserUpperCurve(ByVal visible As Boolean, ByVal side As eSide)
        If ObjectExist(eReferences.userExternalUpperCurve, side) Then
            Dim objRef As MRhinoObjRef = GetRhinoObjRef(eReferences.userExternalUpperCurve, side)
            If visible Then
                RhUtil.RhinoApp.ActiveDoc.ShowObject(objRef, True)
            Else
                RhUtil.RhinoApp.ActiveDoc.HideObject(objRef, True)
            End If
            objRef.Dispose()
        End If
        If ObjectExist(eReferences.userInternalUpperCurve, side) Then
            Dim objRef As MRhinoObjRef = GetRhinoObjRef(eReferences.userInternalUpperCurve, side)
            If visible Then
                RhUtil.RhinoApp.ActiveDoc.ShowObject(objRef, True)
            Else
                RhUtil.RhinoApp.ActiveDoc.HideObject(objRef, True)
            End If
            objRef.Dispose()
        End If
    End Sub

    Public Sub DeleteUserUpperCurve(ByVal referenceType As eReferences, ByVal side As eSide)
        If ObjectExist(referenceType, side) Then
            RhUtil.RhinoApp.ActiveDoc.DeleteObject(GetRhinoObjRef(referenceType, side), True, True)
            ClearRhinoObjRef(referenceType, side)
        End If
    End Sub

    Public Sub DeleteUserUpperCurve(ByVal side As eSide)
        If ObjectExist(eReferences.userInternalUpperCurve, side) Then
            RhUtil.RhinoApp.ActiveDoc.DeleteObject(GetRhinoObjRef(eReferences.userInternalUpperCurve, side), True, True)
            ClearRhinoObjRef(eReferences.userInternalUpperCurve, side)
        End If
        If ObjectExist(eReferences.userExternalUpperCurve, side) Then
            RhUtil.RhinoApp.ActiveDoc.DeleteObject(GetRhinoObjRef(eReferences.userExternalUpperCurve, side), True, True)
            ClearRhinoObjRef(eReferences.userExternalUpperCurve, side)
        End If
    End Sub

    Public Sub DeleteUserUpperCurve()
        DeleteUserUpperCurve(eSide.left)
        DeleteUserUpperCurve(eSide.right)
    End Sub

    Public Sub DeleteFinalUpperCurve(ByVal side As eSide)
        If ObjectExist(eReferences.finalUpperCurve, side) Then
            RhUtil.RhinoApp.ActiveDoc.DeleteObject(GetRhinoObjRef(eReferences.finalUpperCurve, side), True, True)
            ClearRhinoObjRef(eReferences.finalUpperCurve, side)
        End If
    End Sub

    Public Sub DeleteFinalUpperCurve()
        DeleteFinalUpperCurve(eSide.left)
        DeleteFinalUpperCurve(eSide.right)
    End Sub

    Public Sub DeleteAllUpperCurves(ByVal side As eSide)
        DeleteUserUpperCurve(side)
        DeleteFinalUpperCurve(side)
    End Sub

    Public Sub DeleteAllUpperCurves()
        DeleteAllUpperCurves(eSide.left)
        DeleteAllUpperCurves(eSide.right)
    End Sub

#End Region


#Region " Trimming surface delle curve di sezione "

    Public Sub SetFootCurvesTrimSurface(ByVal side As eSide, ByVal type As eTrimSurface, ByVal surface As OnSurface)
        If side = eSide.left Then
            If Not mFootCurvesTrimSrfLeft.ContainsKey(type) Then
                mFootCurvesTrimSrfLeft.Add(type, surface)
            Else
                mFootCurvesTrimSrfLeft.Item(type) = surface
            End If
        Else
            If Not mFootCurvesTrimSrfRight.ContainsKey(type) Then
                mFootCurvesTrimSrfRight.Add(type, surface)
            Else
                mFootCurvesTrimSrfRight.Item(type) = surface
            End If

        End If
    End Sub

    Public Function GetFootCurvesTrimSurface(ByVal side As eSide) As Dictionary(Of eTrimSurface, OnSurface)
        If side = eSide.left Then
            Return mFootCurvesTrimSrfLeft
        Else
            Return mFootCurvesTrimSrfRight
        End If
    End Function



    Public Function GetFootCurvesTrimSurface(ByVal side As eSide, ByVal type As eTrimSurface) As OnSurface
        If side = eSide.left Then
            If mFootCurvesTrimSrfLeft.ContainsKey(type) Then Return mFootCurvesTrimSrfLeft.Item(type)
        Else
            If mFootCurvesTrimSrfRight.ContainsKey(type) Then Return mFootCurvesTrimSrfRight.Item(type)
        End If
        Return Nothing
    End Function


    Public Sub ClearFootCurvesTrimSurface()
        mFootCurvesTrimSrfLeft.Clear()
        mFootCurvesTrimSrfRight.Clear()
    End Sub

#End Region


#Region " Sweep cutting surface della forma laterale "

    Public Sub ShowSweepCuttingSrf(ByVal visible As Boolean, ByVal side As eSide, Optional ByVal alsoLockUnlock As Boolean = True)
        If Not ObjectExist(eReferences.sweepCuttingSurface, side) Then Exit Sub
        Dim objRef As MRhinoObjRef = GetRhinoObjRef(eReferences.sweepCuttingSurface, side)
        objRef.Object.Select(True, True)
        If visible Then
            RhUtil.RhinoApp.ActiveDoc.ShowObject(objRef, True)
        Else
            RhUtil.RhinoApp.ActiveDoc.HideObject(objRef, False)
        End If
        RhUtil.RhinoApp.RunScript("_SelNone", 0)
        'SEMBRA CHE UN OGGETTO NON POSSA ESSERE SIA NASCOSTO CHE BLOCCATO - QUINDI IL CODICE SEGUENTE E' INUTILE
        If alsoLockUnlock Then
            If Not objRef.Object.IsVisible() And Not objRef.Object.IsLocked() Then
                objRef.Object.Select(True, True)
                RhUtil.RhinoApp.RunScript("_Lock", 0)
            End If
            If objRef.Object.IsVisible() And objRef.Object.IsLocked() Then
                objRef.Object.Select(True, True)
                RhUtil.RhinoApp.RunScript("_UnlockSelected ", 0)
            End If
            RhUtil.RhinoApp.RunScript("_SelNone", 0)
        End If
        objRef.Dispose()
    End Sub

    Public Sub ShowSweepCuttingSrf(ByVal visible As Boolean, Optional ByVal alsoLockUnlock As Boolean = True)
        ShowSweepCuttingSrf(visible, eSide.left, alsoLockUnlock)
        ShowSweepCuttingSrf(visible, eSide.right, alsoLockUnlock)
    End Sub

#End Region


#Region " Addiction "


    Public Sub AddAddiction(ByVal addiction As IdAddiction)
        If Not GetAddictions(addiction.Side).Contains(addiction) Then GetAddictions(addiction.Side).Add(addiction)
    End Sub

    Public Function GetLastAddiction(ByVal side As eSide) As IdAddiction
        If GetAddictions(side).Count < 1 Then Return Nothing
        Dim lastIndex As Integer = GetAddictions(side).Count - 1
        Return GetAddictions(side).Item(lastIndex)
    End Function

    Public Function AddictionsExist(ByVal side As eSide) As Boolean
        For Each addiction As IdAddiction In GetAddictions(side)
            If addiction.IsBlendSrfInDocument Or addiction.IsSurfaceInDocument Or addiction.AreAllCurvesInDocument Then
                Return True
            End If
        Next
        Return False
    End Function

    ''' <summary>
    ''' Nel documento esiste almeno uno scarico(curve o superficie o raccordo)
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function AddictionsExist() As Boolean
        For Each addiction As IdAddiction In GetAddictions(eSide.left)
            If addiction.IsBlendSrfInDocument Or addiction.IsSurfaceInDocument Or addiction.AreAllCurvesInDocument Then Return True
        Next
        For Each addiction As IdAddiction In GetAddictions(eSide.right)
            If addiction.IsBlendSrfInDocument Or addiction.IsSurfaceInDocument Or addiction.AreAllCurvesInDocument Then Return True
        Next
        Return False
    End Function


    Public Function GetAddictions(ByVal side As eSide) As List(Of IdAddiction)
        If side = eSide.left Then
            Return mAddictionsLeft
        Else
            Return mAddictionsRight
        End If
    End Function

    Public Function GetAddictions(ByVal side As eSide, ByVal type As IdAddiction.eAddictionType) As List(Of IdAddiction)
        Return CType((From addiction In GetAddictions(side) Where (addiction.Type = type) Select addiction), List(Of IdAddiction))
    End Function

    ''' <summary>
    ''' Rimuove lo scarico dal documento e dalla lista
    ''' </summary>
    ''' <param name="addictionToRemove"></param>
    Public Sub RemoveAddiction(ByVal addictionToRemove As IdAddiction)
        'If GetAddictions(addictionToRemove.Side).Contains(addictionToRemove) Then GetAddictions(addictionToRemove.Side).Remove(addictionToRemove)        
        Dim index As Integer = -1
        For i As Integer = 0 To GetAddictions(addictionToRemove.Side).Count - 1
            If GetAddictions(addictionToRemove.Side).Item(i) Is addictionToRemove Then index = i
        Next
        If index > -1 Then GetAddictions(addictionToRemove.Side).RemoveAt(index)
        addictionToRemove.DeleteFromDocument()
    End Sub

    ''' <summary>
    ''' Clear della lista degli scarichi ma non dal Doc
    ''' </summary>
    Public Sub ClearAddictions(ByVal side As eSide, Optional ByVal alsoDeleteInDoc As Boolean = True)
        If alsoDeleteInDoc Then
            For Each addiction As IdAddiction In GetAddictions(side)
                addiction.DeleteFromDocument()
            Next
        End If
        GetAddictions(side).Clear()
    End Sub

    ''' <summary>
    ''' Clear delle liste degli scarichi ma non dal Doc
    ''' </summary>
    Public Sub ClearAddictions(Optional ByVal alsoDeleteInDoc As Boolean = True)
        GetAddictions(eSide.left).Clear()
        GetAddictions(eSide.right).Clear()
    End Sub


#End Region


#Region " Mappe pressione "


    Public Property ModelPressureMap() As IdPressureMap
        Get
            Return mModelPressureMap
        End Get
        Set
            mModelPressureMap = Value
        End Set
    End Property

    Public Sub ClearPressureMap()
        mModelPressureMap = New IdPressureMap
    End Sub

    Public Sub ShowPressureMap(ByVal visible As Boolean)
        ShowPressureMap(visible, eSide.left)
        ShowPressureMap(visible, eSide.right)
    End Sub

    Public Shared Sub ShowPressureMap(ByVal visible As Boolean, ByVal side As eSide)
        RhLayer.RendiVisibileLayer(GetLayerName(side, eLayerType.pressureMap), visible)
    End Sub

#End Region


#Region " Piani inclinati "


    Public Sub SetPianoInclinato(side As eSide, pianoInclinato As AbstractPianiInclinati)
        If side = eSide.left Then
            mPianoInclinatoLeft = pianoInclinato
        Else
            mPianoInclinatoRight = pianoInclinato
        End If
    End Sub

    Public Function GetPianoInclinato(side As eSide) As AbstractPianiInclinati
        If side = eSide.left Then
            Return mPianoInclinatoLeft
        Else
            Return mPianoInclinatoRight
        End If
    End Function

    Public Function PianoInclinatoExists() As Boolean
        Return PianoInclinatoExist(eSide.left) And PianoInclinatoExist(eSide.right)
    End Function

    Public Function PianoInclinatoExist(side As eSide) As Boolean
        Dim target = GetPianoInclinato(side)
        Return target IsNot Nothing AndAlso target.IsInDoc()
    End Function

    Public Sub ClearPianoInclinato(side As eSide, Optional ByVal alsoDeleteInDoc As Boolean = True)
        Dim pianoInclinato = GetPianoInclinato(side)
        If pianoInclinato IsNot Nothing And alsoDeleteInDoc Then pianoInclinato.DeleteFromDocument()
        SetPianoInclinato(side, Nothing)
    End Sub

    Public Sub ClearPianoInclinato(Optional ByVal alsoDeleteInDoc As Boolean = True)
        ClearPianoInclinato(eSide.left, alsoDeleteInDoc)
        ClearPianoInclinato(eSide.right, alsoDeleteInDoc)
    End Sub

    Public Sub ShowPianiInclinati(ByVal visible As Boolean, ByVal side As eSide)
        RhLayer.RendiVisibileLayer(GetLayerName(side, eLayerType.pianoInclinato), visible)
    End Sub

    ''' <summary>
    ''' Visualizza o nasconde i piedi
    ''' </summary>
    Public Sub ShowPianiInclinati(ByVal visible As Boolean)
        ShowFoot(visible, eSide.left)
        ShowFoot(visible, eSide.right)
    End Sub


#End Region


#Region " Utils "


    Public ReadOnly Property OtherSide(ByVal side As eSide) As eSide
        Get
            If side = eSide.left Then
                Return eSide.right
            Else
                Return eSide.left
            End If
        End Get
    End Property

    Public Sub ResetAll()
        ResetReferences()
        DeleteAllFootCurves(False)
        ClearFootCurvesTrimSurface()
        ClearAddictions(False)
        ClearPressureMap()
        ClearPianoInclinato(False)
    End Sub


#End Region


#Region " Nomi Layer e Oggetti Rhino - TUTTI NON DIPENDENTI DALLA LINGUA PER MAGGIORE COMPATIBILITA' "

    ''' <summary>
    ''' Restituisci il nome del layer richiesto
    ''' </summary>
    ''' <remarks>NB: per evitare problemi di versioni create con una lingua a riaperte con una diversa i layer sono SOLO in inglese</remarks>
    Public Shared Function GetLayerName(ByVal side As eSide, ByVal type As eLayerType) As String
        Dim result As String = Nothing

        Select Case type
            Case eLayerType.foot
                result = "Foot"
            Case eLayerType.insole
                result = "Insole"
            Case eLayerType.last
                result = "Last"
            Case eLayerType.addiction
                result = "Addiction"
            Case eLayerType.root
                result = "Insole Designer"
            Case eLayerType.ruler
                result = "Ruler"
            Case eLayerType.manufacturing
                result = "Manufacturing"
            Case eLayerType.sottopiede
                result = "Sole"
            Case eLayerType.TcoProfile
                result = "TCO profile"
            Case eLayerType.template
                result = "Template"
            Case eLayerType.pressureMap
                result = "Pressure Map"
            Case eLayerType.pianoInclinato
                result = "Wedge"
            Case Else
                result = type.ToString()
        End Select

        Return result.Trim() & " " & GetSideSuffix(side)
    End Function


    ''' <summary>
    ''' I nomi sono stati impostati vuoti per lasciare libertà all'utente. Ma sono impostati nel momento in cui vengono salvati i template
    ''' per successivo riconoscimento. Appena importati gli oggetti vengono nuovamente impostati con nome vuoto
    ''' </summary>
    ''' <param name="refType"></param>
    ''' <param name="side"></param>
    ''' <returns></returns>
    Public Shared Function GetObjectName(ByVal refType As eReferences, ByVal side As eSide) As String
        'Dim result As String = ""
        'Select Case refType
        '    Case eReferences.footMesh
        '        result = "Foot Mesh"
        '    Case eReferences.lastLateralSurface
        '        result = "Lateral Last"
        '    Case eReferences.lastBottomSurface
        '        result = "Bottom Last"
        '    Case eReferences.lastTotalSurface
        '        result = "Total Last"
        '    Case eReferences.userInternalUpperCurve
        '        result = "User Int Upper Curve"
        '    Case eReferences.userExternalUpperCurve
        '        result = "User Ext Upper Curve"
        '    Case eReferences.finalUpperCurve
        '        result = "Offset Upper Curve"
        '    Case eReferences.insoleTopSurface
        '        result = "Insole Top Surface"
        '    Case eReferences.insoleLateralSurface
        '        result = "Insole Lateral Surface"
        '    Case eReferences.insoleBottomSurface
        '        result = "Insole Bottom Surface"
        '    Case eReferences.insoleFinalSurface
        '        result = "Insole Final Surface"
        '    Case Else
        '        result = ""
        'End Select
        'Return result.Trim() & " " & GetSideSuffix(side)
        Return ""
    End Function


    Public Shared Sub SetObjectName(ByVal refType As eReferences, ByVal side As eSide, ByVal uuid As Guid)
        Dim rhinoObjInterface As IRhinoObject = RhUtil.RhinoApp.ActiveDoc.LookupObject(uuid)
        If rhinoObjInterface IsNot Nothing Then SetObjectName(refType, side, rhinoObjInterface)
    End Sub


    Public Shared Sub SetObjectName(ByVal refType As eReferences, ByVal side As eSide, ByRef rhinoObj As IRhinoObject)
        Dim objAttr As New MRhinoObjectAttributes(rhinoObj.Attributes)
        objAttr.m_name = GetObjectName(refType, side)
        RhUtil.RhinoApp.ActiveDoc.ModifyObjectAttributes(New MRhinoObjRef(rhinoObj.Attributes.m_uuid), objAttr)
        objAttr.Dispose()
    End Sub


    Public Shared Function GetSideSuffix(ByVal side As eSide) As String
        If side = eSide.left Then
            Return "L"
        ElseIf side = eSide.right Then
            Return "R"
        Else
            Return ""
        End If
    End Function


#End Region


#Region " Serializzazione/deserializzazione"


    Public Function IHaveDataToWrite() As Boolean
        Try
            'Ciclo tutti i reftype con e almeno uno deve esistere
            For Each refType As eReferences In System.Enum.GetValues(GetType(eReferences))
                If ObjectExist(refType) Then Return True
            Next

            If FootCurvesExists(eSide.left) Then Return True
            If FootCurvesExists(eSide.right) Then Return True

            'Mi limito a controllare che le superfici siano state salvate
            If mFootCurvesTrimSrfLeft.Count > 0 Then Return True
            If mFootCurvesTrimSrfRight.Count > 0 Then Return True

            If AddictionsExist() Then Return True

            If PianoInclinatoExists() Then Return True

            Return False
        Catch ex As Exception
            IdLanguageManager.PromptError(ex.Message)
            Return False
        End Try
    End Function

    Public Function Serialize(ByRef archive As OnBinaryArchive) As Boolean Implements IOnSerializable.Serialize      
        Try
            If Not Serialize(archive, eSide.left) Then Return False
            If Not Serialize(archive, eSide.right) Then Return False
            Return True
        Catch ex As Exception
            IdLanguageManager.PromptError(ex.Message)
            Return False
        End Try

    End Function

    ''' <summary>
    ''' Serializza gli oggetti di un lato
    ''' </summary>  
    Private Function Serialize(ByRef archive As OnBinaryArchive, ByVal side As eSide) As Boolean

        'Lista oggetti globali - precedo con numero per compatibilità versioni successive
        'Ovviamente i campi nuovi non potranno essere letti da versioni vecchie
        If Not archive.WriteInt([Enum].GetValues(GetType(eReferences)).Length) Then Return False
        For Each refType As eReferences In System.Enum.GetValues(GetType(eReferences))
            Dim uuid As Guid = Me.GetRhinoObjID(refType, side)
            If Not archive.WriteUuid(uuid) Then Return False
        Next

        'Curve di profilo
        If Not archive.WriteInt(GetFootCurves(side).Count) Then Return False
        For Each pair As KeyValuePair(Of Double, Guid) In GetFootCurves(side)
            If Not archive.WriteDouble(pair.Key) Then Return False
            If Not archive.WriteUuid(pair.Value) Then Return False
        Next

        'Superfici di taglio delle curve di sezione - le SRF sono 0 oppure 2 ma la lista potrebbe contenere 2 nothing che non posso essere serializzati!!!
        If GetFootCurvesTrimSurface(side).Count > 0 AndAlso
            GetFootCurvesTrimSurface(side, eTrimSurface.topExt) IsNot Nothing Or GetFootCurvesTrimSurface(side, eTrimSurface.topInt) IsNot Nothing Then
            If Not archive.WriteInt(2) Then Return False
            If Not archive.WriteObject(GetFootCurvesTrimSurface(side, eTrimSurface.topExt).NurbsSurface) Then Return False
            If Not archive.WriteObject(GetFootCurvesTrimSurface(side, eTrimSurface.topInt).NurbsSurface) Then Return False
        Else
            If Not archive.WriteInt(0) Then Return False
        End If

        'Scarichi - tramite lambda scrivo solo quelli del side corretto
        Dim addicionCount As Integer = GetAddictions(side).Count()
        If Not archive.WriteInt(addicionCount) Then Return False
        For Each addiction As IdAddiction In GetAddictions(side)
            If Not addiction.Serialize(archive) Then Return False
        Next

        'Mappe pressione che gestisce internamente entrambi i side
        If Not mModelPressureMap.Serialize(archive, side) Then Return False

        'Piani inclinati
        Dim pianoInclinatoExist As Boolean = Me.PianoInclinatoExist(side)
        If Not archive.WriteBool(pianoInclinatoExist) Then Return False
        If pianoInclinatoExist Then
            If Not GetPianoInclinato(side).Serialize(archive) Then Return False
        End If

        Return True
    End Function


    Public Function Deserialize(ByRef archive As OnBinaryArchive) As Boolean Implements IOnSerializable.Deserialize      
        Try
            If Not Deserialize(archive, eSide.left) Then Return False
            If Not Deserialize(archive, eSide.right) Then Return False
            Return True
        Catch ex As Exception
            IdLanguageManager.PromptError(ex.Message)
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Deserializza gli oggetti di un lato
    ''' </summary> 
    Private Function Deserialize(ByRef archive As OnBinaryArchive, ByVal side As eSide) As Boolean

        'Lista oggetti globali
        Dim rhinoRefCount As Integer = -1
        If Not archive.ReadInt(rhinoRefCount) Then Return False
        For i As Integer = 0 To rhinoRefCount - 1
            Dim uuid As New Guid
            If Not archive.ReadUuid(uuid) Then Return False
            'Per compatibilità tra diverse versioni controllo che i non sia maggiore del numero dei tipi di ref di questa versione
            If i < [Enum].GetValues(GetType(eReferences)).Length Then
                If RhUtil.RhinoApp.ActiveDoc.LookupDocumentObject(uuid, True) IsNot Nothing Then
                    'Cast sicuro dato il controllo precedente
                    Dim refType As eReferences = DirectCast(i, eReferences)
                    Me.SetRhinoObj(refType, side, uuid)
                End If
            End If
        Next

        'Curve di profilo
        Dim footCurvesCount As Integer = -1
        If Not archive.ReadInt(footCurvesCount) Then Return False
        For i As Integer = 0 To footCurvesCount - 1
            Dim position As Double
            If Not archive.ReadDouble(position) Then Return False
            Dim uuid As New Guid
            If Not archive.ReadUuid(uuid) Then Return False
            If RhUtil.RhinoApp.ActiveDoc.LookupDocumentObject(uuid, True) IsNot Nothing Then AddFootCurve(side, position, uuid)
        Next

        'Superfici di taglio delle curve di sezione        
        Dim footCurvesTrimSrfCount As Integer = -1
        If Not archive.ReadInt(footCurvesTrimSrfCount) Then Return False
        If footCurvesTrimSrfCount = 2 Then
            'Dai sorgenti ho letto che bisogna usare ReadObject con WriteObject e NON ReadOnObject
            Dim onobj As OnObject = New OnNurbsSurface()
            If Not CBool(archive.ReadObject(onobj)) Then Return False
            SetFootCurvesTrimSurface(side, eTrimSurface.topExt, OnSurface.Cast(onobj).DuplicateSurface)
            onobj = New OnNurbsSurface()
            If Not CBool(archive.ReadObject(onobj)) Then Return False
            SetFootCurvesTrimSurface(side, eTrimSurface.topInt, OnSurface.Cast(onobj).DuplicateSurface)
            onobj.Dispose()
        End If

        'Scarichi
        Dim addicionCount As Integer = -1
        If Not archive.ReadInt(addicionCount) Then Return False
        For i As Integer = 0 To addicionCount - 1
            Dim addiction As IdAddiction = Nothing
            If Not IdAddictionFactory.DeserializeAddiction(archive, addiction) Then Return False
            AddAddiction(addiction)
        Next

        'Mappe pressione che gestisce internamente entrambi i side
        'Non do fallimento se non ci sono i dati per compatibilità con versioni precedenti
        mModelPressureMap.Deserialize(archive, side)

        'Piani inclinati
        Dim pianoInclinatoExist = False
        'non do fallimento se non ci sono i dati per compatibilità con versioni precedenti
        If Not archive.ReadBool(pianoInclinatoExist) Then Return True
        If pianoInclinatoExist Then
            Dim pianoInclinato As AbstractPianiInclinati = Nothing
            If Not FactoryPianiInclinati.Deserialize(archive, pianoInclinato) Then Return False
            SetPianoInclinato(side, pianoInclinato)
        End If

        Return True
    End Function


#End Region



End Class
