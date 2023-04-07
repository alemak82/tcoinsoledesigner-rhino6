Imports System.IO
Imports RMA.OpenNURBS
Imports InsoleDesigner.bll.IdElement3dManager
Imports RhinoUtils
Imports RMA.Rhino


Public MustInherit Class IdAddiction
    Implements IOnSerializable
    Implements ICloneable
    'Implements IDisposable


#Region " Constant "

    Protected Const SURFACE_TOLERANCE As String = "0.001"

#End Region


#Region " ENUM "

    'CORRISPONDONO AI TAG DEI TAB IN FRMADDADDICTION
    Public Enum eAddictionType
        metatarsalBar = 0
        olive
        metatarsalDome 'goccia
        archSupprt
        cutout
        horseShoe
    End Enum

    'TUTTI I POSSIBILI MODELLI DA INSERIRE NEI TAG NEI CONTROLLI PICTUREBOX, LABEL E COMBOBOX
    'IL MODELLO DEVE CONTENERE IL CODICE CHE C'È NEL NOME DEL FILE
    Public Enum eAddictionModel
        metbar3450
        metbar3869
        metbar3870
        metDome
        olive3423
        olive3435
        olive3447
        olive6648
        archsupport3030
        cutoutTotal
        cutoutPartial
        horseShoeTotal
        horseShoePartial
    End Enum

    'VALIDA PER TUTTI GLI SCARICHI
    Public Enum eAddictionSize
        none = 0
        S
        M
        L
    End Enum


    Public Enum eAddictionBkSrf
        top
        lateral
        bottom
    End Enum

#End Region


#Region " Field "

    Public Overloads Property Side As eSide
    Public Property Type As eAddictionType
    Public Property Model As eAddictionModel
    Public Property CurvesID As List(Of Guid)
    Public Property CageEditID As Guid
    Protected mOriginFileName As String
    Protected mSurfaceID As Guid
    Protected mBlendSurfaceID As Guid
    Private mSize As eAddictionSize
    Private mBackupInsoleSurfaceTop As OnBrep
    Private mBackupInsoleSurfaceLateral As OnBrep
    Private mBackupInsoleSurfaceBottom As OnBrep


#End Region


#Region " Constructor "


    Protected Sub New()
        SetDefault()
    End Sub


    Public Sub New(ByVal side As eSide, ByVal type As eAddictionType, ByVal model As eAddictionModel, ByVal size As eAddictionSize)
        SetDefault()
        Me.Side = side
        Me.Type = type
        Me.Model = model
        Me.Size = size
        CurvesID = New List(Of Guid)
        mSurfaceID = Guid.Empty
        mBlendSurfaceID = Guid.Empty
        SetFileName()
    End Sub


    Protected Overridable Sub SetDefault()
        mSize = eAddictionSize.none
    End Sub


#End Region


#Region " Property "


    ''' <summary>
    ''' Lo scarico cutout non ha la taglia
    ''' </summary>
    Public Property Size() As eAddictionSize
        Get
            If Me.Type = eAddictionType.cutout Then
                Return eAddictionSize.none
            Else
                Return Me.mSize
            End If
        End Get
        Set(value As eAddictionSize)
            If Me.Type = eAddictionType.cutout Then
                Me.mSize = eAddictionSize.none
            Else
                Me.mSize = value
                SetFileName()
            End If
        End Set
    End Property


    Public ReadOnly Property OriginFileName() As String
        Get
            Return mOriginFileName
        End Get
    End Property



    Public Property SurfaceID() As Guid
        Get
            Return mSurfaceID
        End Get
        Set(value As Guid)
            mSurfaceID = value
            ClearName(mSurfaceID)
        End Set
    End Property

    Private Sub ClearName(ByVal uuid As Guid)
        If RhUtil.RhinoApp.ActiveDoc.LookupDocumentObject(mSurfaceID, True) IsNot Nothing Then
            Dim objref As New MRhinoObjRef(mSurfaceID)
            Dim objAttr As New MRhinoObjectAttributes(objref.Object.Attributes)
            objAttr.m_name = ""
            RhUtil.RhinoApp.ActiveDoc.ModifyObjectAttributes(objref, objAttr)
            objref.Dispose()
        End If
    End Sub

    ''' <summary>
    ''' (PROBLEMA RISOLTO)Property generale da NON usare nel caso della metbaar che ha overload specifico avendo due raccordi
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable Property BlendSurfaceID() As Guid
        Get
            If Me.Type <> eAddictionType.metatarsalBar Then
                Return mBlendSurfaceID
            Else
                Return Guid.Empty
            End If
        End Get
        Set(value As Guid)
            If Me.Type <> eAddictionType.metatarsalBar Then
                mBlendSurfaceID = value
                ClearName(mBlendSurfaceID)
            End If
        End Set
    End Property


    Public Property BackupInsoleSurface(ByVal backupSrf As eAddictionBkSrf) As IOnBrep
        Get
            Select Case backupSrf
                Case eAddictionBkSrf.top
                    Return mBackupInsoleSurfaceTop
                Case eAddictionBkSrf.lateral
                    Return mBackupInsoleSurfaceLateral
                Case eAddictionBkSrf.bottom
                    Return mBackupInsoleSurfaceBottom
                Case Else
                    Return Nothing
            End Select
        End Get
        Set(value As IOnBrep)
            Select Case backupSrf
                Case eAddictionBkSrf.top
                    mBackupInsoleSurfaceTop = value.BrepForm()
                Case eAddictionBkSrf.lateral
                    mBackupInsoleSurfaceLateral = value.BrepForm()
                Case eAddictionBkSrf.bottom
                    mBackupInsoleSurfaceBottom = value.BrepForm()
            End Select
        End Set
    End Property


#End Region


#Region " Nomi file "

    Public Shared Function GetFileSideString(ByVal side As eSide) As String
        If side = eSide.left Then
            Return "_Sn"
        Else
            Return "_Dx"
        End If
    End Function

    Public Shared Function GetFileTypeString(ByVal type As eAddictionType) As String
        Select Case type
            Case eAddictionType.metatarsalBar
                Return "BarraMetatarsale"
            Case eAddictionType.olive
                Return "Oliva"
            Case eAddictionType.metatarsalDome
                Return "Goccia"
            Case eAddictionType.archSupprt
                Return "SupportoVolta"
            Case eAddictionType.cutout
                Return "CutOut"
            Case eAddictionType.horseShoe
                Return "FerroDiCavallo"
            Case Else
                Return [Enum].GetName(GetType(eAddictionType), type)
        End Select
    End Function

    ''' <summary>
    ''' Il codice presente nel nome del file si ricava dalla stringa eAddictionModel
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function GetFileTypeModel(ByVal type As eAddictionType, ByVal model As eAddictionModel) As String
        Select Case type
            Case eAddictionType.metatarsalBar
                Return model.ToString().Replace("metbar", "_")
            Case eAddictionType.olive
                Return model.ToString().Replace("olive", "_")
            Case eAddictionType.metatarsalDome
                'C'è una sola goccia che non ha codice!
                Return ""
            Case eAddictionType.archSupprt
                Return model.ToString().Replace("archsupport", "_")
            Case eAddictionType.cutout
                'I due tipi di cutout hanno lo stesso file
                Return ""
            Case Else
                Return ""
        End Select
    End Function

    Private Shared Function GetFileSizeString(ByVal size As eAddictionSize) As String
        Select Case size
            Case eAddictionSize.S
                Return "_S"
            Case eAddictionSize.M
                Return "_M"
            Case eAddictionSize.L
                Return "_L"
            Case eAddictionSize.none
                Return ""
            Case Else
                Return ""
        End Select
    End Function

    Public Shared Function GetFileName(ByVal side As eSide, ByVal type As eAddictionType, ByVal model As eAddictionModel, ByVal size As eAddictionSize) As String
        Dim basePath As String = My.Application.Info.DirectoryPath
        'Assemblaggio stringhe
        Dim filename As String = ""
        filename = GetFileTypeString(type) & GetFileTypeModel(type, model) & GetFileSideString(side) & GetFileSizeString(size)
        Return Path.Combine(LibraryManager.GetDirectory(LibraryManager.eDirectoryLibrary.addiction), filename & ".3dm")
    End Function

    Protected Overridable Sub SetFileName()
        mOriginFileName = GetFileName(Me.Side, Me.Type, Me.Model, Me.Size)
#If DEBUG Then
        If Not File.Exists(mOriginFileName) Then MsgBox("Non esiste uno scarico con le caratteristiche richieste", MsgBoxStyle.Exclamation, My.Application.Info.Title)
#End If
    End Sub


#End Region


#Region " Metodi Overridable "

    ''' <summary>
    ''' Verifica che sussistano le condizioni per aggiungere lo scarico
    ''' </summary>
    ''' <param name="errorMessage"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable Function AddictionCanBeAdded(ByRef errorMessage As String) As Boolean
        Return True
    End Function

    Public Overridable Sub DeleteBlendSrf()
        If IsBlendSrfInDocument() Then RhUtil.RhinoApp.ActiveDoc.DeleteObject(New MRhinoObjRef(BlendSurfaceID))
    End Sub

    Public Overridable Function IsBlendSrfInDocument() As Boolean
        Return (RhUtil.RhinoApp.ActiveDoc.LookupObject(BlendSurfaceID) IsNot Nothing)
    End Function

    Public Overridable Sub SelectBlendSrf()
        If IsBlendSrfInDocument() Then
            RhUtil.RhinoApp().RunScript("_PointsOff", 0)
            Dim rhinoObjRef As New MRhinoObjRef(BlendSurfaceID)
            rhinoObjRef.Object.Select(True, True)
            rhinoObjRef.Dispose()
        End If
    End Sub


#End Region


#Region " CAD Method "

    ''' <summary>
    ''' Salva i Brep delle superfici del plantare necessarie per ogni tipo di scarico
    ''' </summary>
    ''' <remarks></remarks>
    Public Function BackupInsoleSurfaces() As Boolean
        Dim helper As IdElement3dManager = IdElement3dManager.GetInstance()
        If Not helper.ObjectExist(eReferences.insoleTopSurface, Me.Side) Then Return False
        If helper.GetRhinoObjRef(eReferences.insoleTopSurface, Me.Side).Geometry.BrepForm Is Nothing Then Return False
        'Faccio sempre backup della superficie superiore
        Me.BackupInsoleSurface(eAddictionBkSrf.top) = helper.GetRhinoObjRef(eReferences.insoleTopSurface, Me.Side).Geometry.BrepForm
        Select Case Me.Type
            Case eAddictionType.cutout, eAddictionType.horseShoe
                'Con cutout e ferro di cavallo faccio sempre backup della superficie laterale
                If Not helper.ObjectExist(eReferences.insoleLateralSurface, Me.Side) Then Return False
                If helper.GetRhinoObjRef(eReferences.insoleLateralSurface, Me.Side).Geometry.BrepForm Is Nothing Then Return False
                Me.BackupInsoleSurface(eAddictionBkSrf.lateral) = helper.GetRhinoObjRef(eReferences.insoleLateralSurface, Me.Side).Geometry.BrepForm
                If Me.Model = eAddictionModel.cutoutTotal Or Me.Model = eAddictionModel.horseShoeTotal Then
                    'Con cutout e ferro di cavallo parziali faccio anche backup della superficie inferiore
                    If Not helper.ObjectExist(eReferences.insoleBottomSurface, Me.Side) Then Return False
                    If helper.GetRhinoObjRef(eReferences.insoleBottomSurface, Me.Side).Geometry.BrepForm Is Nothing Then Return False
                    Me.BackupInsoleSurface(eAddictionBkSrf.bottom) = helper.GetRhinoObjRef(eReferences.insoleBottomSurface, Me.Side).Geometry.BrepForm
                End If
        End Select
        Return True
    End Function


    Public Sub ReplaceId(ByVal oldId As Guid, ByVal newId As Guid)
        Dim backupCurves(Me.CurvesID.Count - 1) As Guid
        Me.CurvesID.CopyTo(backupCurves)
        Dim index As Integer = Array.IndexOf(backupCurves, oldId)
        If index <> -1 Then
            Me.CurvesID.Clear()
            For i As Integer = 0 To backupCurves.Length - 1
                If i <> index Then
                    Me.CurvesID.Add(backupCurves(i))
                Else
                    Me.CurvesID.Add(newId)
                End If
            Next
        Else
            Me.CurvesID.Add(newId)
        End If
    End Sub

    Public Sub ParseAddictionId(ByVal layerName As String)
        CurvesID.Clear()
        For Each rhinoObj As MRhinoObject In RhLayer.GetLayerObjects(layerName)
            If rhinoObj.Geometry().ObjectType = IOn.object_type.curve_object Then
                CurvesID.Add(rhinoObj.Attributes.m_uuid)
            End If
        Next

    End Sub

    Public Sub ChangeLayer(ByVal newLayerName As String)
        RhUtil.RhinoApp().RunScript("_SelNone", 0)
        SelectCurves()
        SelectSurface()
        SelectBlendSrf()
        RhUtil.RhinoApp().RunScript("-_ChangeLayer """ & newLayerName & """", 0)
        RhUtil.RhinoApp().RunScript("_SelNone", 0)
        RhLayer.RendiCorrenteLayer(newLayerName)
    End Sub

    ''' <summary>
    ''' Funzione generica da NON usare con il cutout per il quale è stata ridefinita
    ''' </summary>
    ''' <param name="layerName"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable Function CreateSrfFromCurves(ByVal layerName As String) As Boolean
        If Not AreAllCurvesInDocument() Then Return False
        RhUtil.RhinoApp().RunScript("_SelNone", 0)
        SelectCurves()
        RhLayer.RendiCorrenteLayer(layerName)
        RhUtil.RhinoApp().RunScript("-_NetworkSrf EdgeTol=" & SURFACE_TOLERANCE & " InteriorTol=" & SURFACE_TOLERANCE & " _Enter", 0)
        Dim result As Boolean = RhUtil.RhinoApp().RunScript("_SelLast", 0)
        If Not result Then Return False
        Dim getObjects As New MRhinoGetObject
        getObjects.SetGeometryFilter(IRhinoGetObject.GEOMETRY_TYPE_FILTER.surface_object)
        getObjects.GetObjects(0, 1)
        If getObjects.ObjectCount <> 1 Then Return False
        SurfaceID = getObjects.Object(0).ObjectUuid
        RhUtil.RhinoApp().RunScript("_SelNone", 0)
        IdGeometryUtils.CheckSurfaceDirection(getObjects.Object(0), IdGeometryUtils.eDirectionCheck.top)
        Return True
    End Function

    Public Function IsSurfaceInDocument() As Boolean
        Return (RhUtil.RhinoApp.ActiveDoc.LookupObject(SurfaceID) IsNot Nothing)
    End Function


    Public Function AreAllCurvesInDocument() As Boolean
        For Each curveId As Guid In CurvesID
            If RhUtil.RhinoApp.ActiveDoc.LookupObject(curveId) Is Nothing Then Return False
        Next
        Return True
    End Function

    Public Sub SelectSurface()
        If IsSurfaceInDocument() Then
            RhUtil.RhinoApp().RunScript("_PointsOff", 0)
            Dim rhinoObjRef As New MRhinoObjRef(SurfaceID)
            rhinoObjRef.Object.Select(True, True)
            rhinoObjRef.Dispose()
        End If
    End Sub


    Public Overridable Sub SelectCurves()
        If AreAllCurvesInDocument() Then
            For Each curveId As Guid In CurvesID
                RhUtil.RhinoApp().RunScript("_PointsOff", 0)
                Dim rhinoObjRef As New MRhinoObjRef(curveId)
                rhinoObjRef.Object.Select(True, True)
                rhinoObjRef.Dispose()
            Next
        End If
    End Sub

    Public Function GetBbox() As OnBoundingBox
        Dim result As New OnBoundingBox()
        If Not IsSurfaceInDocument() And Not AreAllCurvesInDocument() Then Return result
        If IsSurfaceInDocument() Then
            Dim rhinoObjRef As New MRhinoObjRef(SurfaceID)
            'Per sicurezza dovrei usare GetTightBoundingBox() come fatto per le curve
            If Not rhinoObjRef.Surface Is Nothing Then Return rhinoObjRef.Surface.BoundingBox()
            If Not rhinoObjRef.Brep Is Nothing Then Return rhinoObjRef.Brep.BoundingBox()
        End If
        If AreAllCurvesInDocument() Then
            Dim bbox As New OnBoundingBox()
            For Each curveId As Guid In CurvesID
                Dim rhinoObjRef As New MRhinoObjRef(curveId)
                rhinoObjRef.Curve.GetTightBoundingBox(bbox)
                result.Union(bbox)
                rhinoObjRef.Dispose()
            Next
            bbox.Dispose()
        End If
        Return result
    End Function


    ''' <summary>
    ''' Dato che le curve di base sono complanari scarto le curve che hanno il massimo in Z della bbox al di sopra del minimo della bbox totale
    ''' </summary>
    ''' <returns></returns>
    Public Function GetBaseCurvesObjRef() As List(Of MRhinoObjRef)
        Dim result As New List(Of MRhinoObjRef)
        Dim delta As Double = 0.1
        For Each curveId As Guid In CurvesID
            Dim rhinoObjRef As New MRhinoObjRef(curveId)
            If rhinoObjRef IsNot Nothing AndAlso rhinoObjRef.Curve IsNot Nothing Then
                Dim currentCurveMaxZ As Double = rhinoObjRef.Curve().BoundingBox().m_max.z
                If GetBbox().m_min.z + delta > currentCurveMaxZ Then result.Add(rhinoObjRef)
            End If
        Next
        Return result
    End Function

    Public Function GetBaseCurves() As List(Of IOnCurve)
        Dim result As New List(Of IOnCurve)
        For Each objref As MRhinoObjRef In GetBaseCurvesObjRef()
            result.Add(objref.Curve())
        Next
        Return result
    End Function

    Public Function GetBaseCurvesId() As List(Of Guid)
        Dim result As New List(Of Guid)
        For Each objref As MRhinoObjRef In GetBaseCurvesObjRef()
            result.Add(objref.m_uuid)
        Next
        Return result
    End Function


    Public Overridable Function GetSuperiorCurvesId() As List(Of Guid)
        Dim result As New List(Of Guid)
        For Each uuid As Guid In CurvesID
            If Not GetBaseCurvesId.Contains(uuid) Then result.Add(uuid)
        Next
        Return result
    End Function



    ''' <summary>
    ''' Aggiunge il CageEdit al Doc tranne nel caso del CutOut e Ferro di cavallo(richiesta del cliente)
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub AddCageEdit()
        If Me.Type = eAddictionType.cutout Or Me.Type = eAddictionType.horseShoe Then Exit Sub
        RhUtil.RhinoApp().RunScript("_SelNone", 0)
        SelectCurves()
        Dim cageEditCmd As String = Nothing
        ''-----------------------------------------------------------------------------------------------------------------------------------------------
        'If LanguageHelper.RhinoLanguageSetting = elanguage.English Then
        '    cageEditCmd = "-_CageEdit _B _W _X=4 _Y=5 _Z=4 _D=3 _E=3 _G=3 _Enter _G _Enter"
        'Else
        '    cageEditCmd = "-_CageEdit _P _A _N=4 _U=5 _M=4 _D=3 _E=3 _G=3 _Enter _G _Enter"
        'End If
        cageEditCmd = "-_CageEdit _Boundingbox _World _XPointCount=8 _YPointCount=6 _ZPointCount=4 _XDegree=3 _YDegree=3 _ZDegree=3 _Enter _Global _Enter"
        ''------------------------------------------------------------------------------------------------------------------------------------------------
        RhUtil.RhinoApp().RunScript(cageEditCmd, 0)
        RhUtil.RhinoApp().RunScript("_PointsOff", 0)
        RhUtil.RhinoApp().RunScript("_SelLast", 0)
        Dim getObjects As New MRhinoGetObject
        getObjects.GetObjects(0, 1)
        For i As Integer = 0 To getObjects.ObjectCount - 1
            If getObjects.Object(i).GeometryType = IRhinoObject.GEOMETRY_TYPE.morph_control_object Then
                CageEditID = getObjects.Object(i).ObjectUuid
            End If
        Next
        RhUtil.RhinoApp().RunScript("_PointsOn", 0)
    End Sub

    Public Sub RemoveCageEdit()
        If CageEditID = Guid.Empty Then Exit Sub
        RhUtil.RhinoApp().RunScript("_SelNone", 0)
        RhUtil.RhinoApp().RunScript("_PointsOff", 0)
        Dim cageObjRef As New MRhinoObjRef(CageEditID)
        If cageObjRef IsNot Nothing Then RhUtil.RhinoApp.ActiveDoc.DeleteObject(cageObjRef, True, True)
        cageObjRef.Dispose()
        RhUtil.RhinoApp().RunScript("_SelNone", 0)
    End Sub

    Public Sub DeleteFromDocument()
        RemoveCageEdit()
        DeleteSrf()
        DeleteBlendSrf()
        DeleteAllCurves()
    End Sub

    Public Overridable Sub DeleteSrf()
        If IsSurfaceInDocument() Then RhUtil.RhinoApp.ActiveDoc.DeleteObject(New MRhinoObjRef(SurfaceID))
    End Sub

    ''' <summary>
    ''' Elimina le curve dal Doc e i riferimenti
    ''' </summary>
    ''' <remarks></remarks>
    Public Overridable Sub DeleteAllCurves()
        For Each curveId As Guid In CurvesID
            Dim curveObjRef As New MRhinoObjRef(curveId)
            If curveObjRef IsNot Nothing Then RhUtil.RhinoApp.ActiveDoc.DeleteObject(curveObjRef)
            curveObjRef.Dispose()
        Next
        Me.CurvesID.Clear()
    End Sub


#End Region


#Region " Serializzazione/deserializzazione"

    ''*********************************************************************************************************************************************************************
    ''*** LA SERIALIZZAZIONE E DESERIALIZZAZIONE FUNZIONANO IN MODI DIFFERENTI                                                                                          ***
    ''*** SERIALIZZAZIONE: VIENE RIDEFINITA DA OGNI CLASSE SPECIFICA CHE ALL'INIZIO CHIAMA QUELLA GENERICA PER I CAMPI COMUNI                                           ***
    ''*** DESERIALIZZAZIONE: I CAMPI COMUNI VENGONO FATTI DA IdAddictionFactory CHE PUO' ISTANZIARE LA CLASSE CORRETTA E POI RICHIAMA LE ULTERIORI DESERIALIZZAZIONI    ***
    ''*********************************************************************************************************************************************************************

    Protected Function CommonSerialize(ByRef archive As OnBinaryArchive) As Boolean
        'Stringhe
        If Not archive.WriteString(Me.Side.ToString) Then Return False
        If Not archive.WriteString(Me.Type.ToString) Then Return False
        If Not archive.WriteString(Me.Model.ToString) Then Return False
        If Not archive.WriteString(Me.Size.ToString) Then Return False
        If Me.Type = eAddictionType.cutout Then
            If Not archive.WriteString(DirectCast(Me, IdCutoutToTalAddiction).CutoutDirection.ToString) Then Return False
        End If

        'UUID
        If Not archive.WriteUuid(Me.SurfaceID) Then Return False
        If Not archive.WriteUuid(Me.BlendSurfaceID) Then Return False
        'BREP
        If Not archive.WriteObject(mBackupInsoleSurfaceTop) Then Return False

        Return True
    End Function


    Public MustOverride Function Serialize(ByRef archive As OnBinaryArchive) As Boolean Implements IOnSerializable.Serialize


    Public MustOverride Function Deserialize(ByRef archive As OnBinaryArchive) As Boolean Implements IOnSerializable.Deserialize



#End Region


#Region " IClonable "


    MustOverride Function Clone() As Object Implements ICloneable.Clone

    Protected Sub CloneCommonField(ByRef ohterAddiction As IdAddiction)
        ohterAddiction.SurfaceID = New Guid(Me.SurfaceID.ToString)
        ohterAddiction.CageEditID = New Guid(Me.CageEditID.ToString)
        For Each uuid As Guid In Me.CurvesID
            ohterAddiction.CurvesID.Add(New Guid(uuid.ToString))
        Next
        If Me.BackupInsoleSurface(eAddictionBkSrf.top) IsNot Nothing Then
            ohterAddiction.BackupInsoleSurface(eAddictionBkSrf.top) = Me.BackupInsoleSurface(eAddictionBkSrf.top).BrepForm.Duplicate()
        End If
        If Me.BackupInsoleSurface(eAddictionBkSrf.lateral) IsNot Nothing Then
            ohterAddiction.BackupInsoleSurface(eAddictionBkSrf.lateral) = Me.BackupInsoleSurface(eAddictionBkSrf.lateral).BrepForm.Duplicate()
        End If
        If Me.BackupInsoleSurface(eAddictionBkSrf.bottom) IsNot Nothing Then
            ohterAddiction.BackupInsoleSurface(eAddictionBkSrf.bottom) = Me.BackupInsoleSurface(eAddictionBkSrf.bottom).BrepForm.Duplicate()
        End If
    End Sub

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
    '        If mBackupInsoleSurfaceTop IsNot Nothing Then mBackupInsoleSurfaceTop.Dispose()
    '        If mBackupInsoleSurfaceLateral IsNot Nothing Then mBackupInsoleSurfaceLateral.Dispose()
    '        If mBackupInsoleSurfaceBottom IsNot Nothing Then mBackupInsoleSurfaceLateral.Dispose()
    '        ' TODO: impostare campi di grandi dimensioni su null.
    '    End If
    '    Me.disposedValue = True
    'End Sub

    ''TODO: eseguire l'override di Finalize() solo se Dispose(ByVal disposing As Boolean) dispone del codice per liberare risorse non gestite.
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
