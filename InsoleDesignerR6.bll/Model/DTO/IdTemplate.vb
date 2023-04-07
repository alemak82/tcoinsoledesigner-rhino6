Imports InsoleDesigner.bll.IdElement3dManager
Imports InsoleDesigner.bll.IdAddiction
Imports InsoleDesigner.bll.AbstractCutoutCommons
Imports ORM
Imports ORM.IdDataSet
Imports ORM.IdDataSetTableAdapters
Imports System.Reflection


Public Class IdTemplate


#Region " Enum "



#End Region


#Region " Field "

    Public TemplateID As String
    Public LastModelID As Integer
    Public SizeID As Integer
    Public ThicknessID As Integer
    Public BottomTypeID As Integer
    Public VaultID As Integer
    Public UserID As Integer
    Public PatientID As Integer
    Private mTemplate3DFileNameLeft As String
    Private mTemplate3DFileNameRight As String
    Public Pathologies As List(Of Integer)

#End Region


#Region " Constructor "

    Public Sub New()
        SetDefault()
    End Sub

    Public Sub New(lastModelId As Integer, sizeId As Integer, thicknessId As Integer, bottomTypeId As Integer, vaultId As Integer)
        SetDefault()
        Me.LastModelID = lastModelId
        Me.SizeID = sizeId
        Me.ThicknessID = thicknessId
        Me.BottomTypeID = bottomTypeId
        Me.VaultID = vaultId
    End Sub

    Private Sub SetDefault()
        LastModelID = -1
        SizeID = -1
        ThicknessID = -1
        BottomTypeID = -1
        VaultID = -1
        UserID = -1
        PatientID = -1
        mTemplate3DFileNameLeft = ""
        mTemplate3DFileNameRight = ""
        Pathologies = New List(Of Integer)
    End Sub

#End Region


#Region " Property "


    Public Property File3dName(ByVal side As IdElement3dManager.eSide) As String
        Get
            If side = IdElement3dManager.eSide.left Then
                Return mTemplate3DFileNameLeft
            Else
                Return mTemplate3DFileNameRight
            End If
        End Get
        Set(value As String)
            If side = IdElement3dManager.eSide.left Then
                mTemplate3DFileNameLeft = value
            Else
                mTemplate3DFileNameRight = value
            End If
        End Set
    End Property

    ''' <summary>
    ''' Il percorso finale è composto da:
    ''' 1. La directory di esecuzione corrente + 
    ''' 2. Il percorso relativo del file che a sua volta dipende dal tipo di licenza e il nome del file vero e proprio
    ''' </summary>
    ''' <param name="side"></param>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property File3dFullPath(ByVal side As IdElement3dManager.eSide) As String
        Get
            If String.IsNullOrEmpty(File3dName(side)) Then Return ""
            Return LibraryManager.TemplateFilePath(UserID, File3dName(side))
        End Get
    End Property

#End Region


#Region " READ DB "




#End Region


#Region " WRITE DB "


    Public Function WriteInDB() As Boolean
        'Controlli        
        If LastModelID < 1 Or SizeID < 1 Or ThicknessID < 1 Or BottomTypeID < 1 Or VaultID < 1 Then Return False
        If String.IsNullOrEmpty(mTemplate3DFileNameLeft) And String.IsNullOrEmpty(mTemplate3DFileNameRight) Then Return False
        Dim pathologyAdapter As New PathologyTableAdapter
        For Each pathologyID As Integer In Pathologies
            If pathologyAdapter.GetData.FindByID(pathologyID) Is Nothing Then Return False
        Next
        If UserID < 1 Then Return False

        Try
            TemplateID = Guid.NewGuid.ToString
            Dim templateAdapter As New TemplateTableAdapter
            If templateAdapter.Insert(TemplateID, LastModelID, SizeID, ThicknessID, BottomTypeID, VaultID, UserID, mTemplate3DFileNameLeft, mTemplate3DFileNameRight) <> 1 Then
                Return False
            End If

            'Aggiungo eventuali patologie 
            Dim templatePathologyAdapter As New Template_PathologyTableAdapter
            For Each pathologyID As Integer In Pathologies
                If templatePathologyAdapter.Insert(TemplateID, pathologyID) <> 1 Then
                    'Se l'inserimento fallisce rimuovo il template appena inserito -> per impostazione il DB eliminerà le eventuali relative patologie inserite
                    ''DOVRE USARE templateAdapter.Transaction.Rollback...
                    templateAdapter.Delete(TemplateID, LastModelID, SizeID, ThicknessID, BottomTypeID, VaultID, UserID, mTemplate3DFileNameLeft, mTemplate3DFileNameRight)
                    Return False
                End If
            Next

            'Aggiungo eventuale utente
            If PatientID <> -1 Then
                Dim templatePatientAdapter As New Template_PatientTableAdapter
                templatePatientAdapter.Insert(TemplateID, PatientID)
            End If

            Return True
        Catch ex As Exception
            IdLanguageManager.PromptError(ex.Message)
            Return False
        End Try

    End Function


#End Region


#Region " Nomi oggetti nei template 3D "


#Region " Creazione "

    Public Shared Function TemplateObjectName(ByVal refType As IdElement3dManager.eReferences, ByVal side As IdElement3dManager.eSide) As String
        Dim result As String = ""
        Select Case refType
            Case IdElement3dManager.eReferences.footMesh
                result = "Foot Mesh"
            Case IdElement3dManager.eReferences.lastLateralSurface
                result = "Lateral Last"
            Case IdElement3dManager.eReferences.lastBottomSurface
                result = "Bottom Last"
            Case IdElement3dManager.eReferences.lastTotalSurface
                result = "Total Last"
            Case IdElement3dManager.eReferences.userInternalUpperCurve
                result = "User Int Upper Curve"
            Case IdElement3dManager.eReferences.userExternalUpperCurve
                result = "User Ext Upper Curve"
            Case IdElement3dManager.eReferences.finalUpperCurve
                result = "Offset Upper Curve"
            Case IdElement3dManager.eReferences.insoleTopSurface
                result = "Insole Top Surface"
            Case IdElement3dManager.eReferences.insoleLateralSurface
                result = "Insole Lateral Surface"
            Case IdElement3dManager.eReferences.insoleBottomSurface
                result = "Insole Bottom Surface"
            Case IdElement3dManager.eReferences.insoleFinalSurface
                result = "Insole Final Surface"
            Case Else
                result = ""
        End Select
        Return result.Trim() & " " & GetSideSuffix(side)
    End Function

    Public Shared Function TemplateFootCurveBaseName(ByVal side As IdElement3dManager.eSide) As String
        Return "Foot Section" & " " & GetSideSuffix(side)
    End Function

    Public Shared Function TemplateFootCurveName(ByVal side As IdElement3dManager.eSide, ByVal position As Double) As String
        Dim positionString As String = Convert.ToInt16(position).ToString
        Return TemplateFootCurveBaseName(side) & "_" & positionString.Trim()
    End Function

#End Region


#Region " Riconoscimento "


    Public Shared Function IsTemplateUpperCurveName(ByVal side As IdElement3dManager.eSide, ByVal name As String) As Boolean
        Return (name = TemplateObjectName(IdElement3dManager.eReferences.finalUpperCurve, side))
    End Function

    Public Shared Function IsTemplateFootSectionName(ByVal side As IdElement3dManager.eSide, ByVal name As String) As Boolean
        Return (name.StartsWith(TemplateFootCurveBaseName(side)))
    End Function

    ''' <summary>
    ''' Funzione a doppio utilizzo che dipende dal parametro refType: se specifico per insole controlla quello, altrimenti lo valorizza e cerca
    ''' una dei tre tipi
    ''' </summary>
    ''' <param name="side"></param>
    ''' <param name="name"></param>
    ''' <param name="refType"></param>
    ''' <returns></returns>
    Public Shared Function IsTemplateInsoleName(ByVal side As IdElement3dManager.eSide, ByVal name As String, ByRef refType As IdElement3dManager.eReferences) As Boolean
        If refType = IdElement3dManager.eReferences.insoleTopSurface Or refType = IdElement3dManager.eReferences.insoleLateralSurface Or refType = IdElement3dManager.eReferences.insoleBottomSurface Then
            If name = TemplateObjectName(refType, side) Then Return True
        Else
            refType = IdElement3dManager.eReferences.insoleTopSurface
            If name = TemplateObjectName(refType, side) Then Return True
            refType = IdElement3dManager.eReferences.insoleLateralSurface
            If name = TemplateObjectName(refType, side) Then Return True
            refType = IdElement3dManager.eReferences.insoleBottomSurface
            If name = TemplateObjectName(refType, side) Then Return True
        End If
        Return False
    End Function


#End Region


#Region " Nomi scarichi "

    Public Const SEPARATOR As Char = "_"c
    Public Const SRF_TEMPLATE_PREFIX As String = "Addiction"
    Public Const BLEND_TEMPLATE_PREFIX As String = "FilletAddiction"
    Public Const BK_SRF_TEMPLATE_PREFIX As String = "BackupSrfAddiction"


#Region " Creazione "


    ''' <summary>
    ''' Formato: "Addiction" "_" tipo "_" model "_" size "_" direction "_" side "_" index
    ''' </summary>
    ''' <param name="index"></param>
    ''' <returns>  </returns>
    Public Shared Function SurfaceTemplateName(ByVal addiction As IdAddiction, ByVal index As Integer) As String
        ''assegno alle SRF degli scarichi
        Return SRF_TEMPLATE_PREFIX & SEPARATOR & TemplateType(addiction.Type) & SEPARATOR & TemplateModel(addiction.Model) & SEPARATOR &
            TemplateSize(addiction.Size) & SEPARATOR & TemplateDirection(addiction) & SEPARATOR & TemplateSideString(addiction.Side) & SEPARATOR & index.ToString()
    End Function

    Public Shared Function BlendSurfaceTemplateBaseName(ByVal side As IdElement3dManager.eSide) As String
        Return BLEND_TEMPLATE_PREFIX & SEPARATOR & TemplateSideString(side) & SEPARATOR
    End Function

    Public Shared Function BlendSurfaceTemplateName(ByVal addiction As IdAddiction, ByVal index As Integer) As String
        Return BlendSurfaceTemplateBaseName(addiction.Side) & index.ToString
    End Function

    ''' <summary>
    ''' FORMATO: "BackupSrfAddiction" "_" eAddictionBkSrf "_" side "_"
    ''' </summary>
    ''' <param name="side"></param>
    ''' <param name="backupSrf"></param>
    ''' <returns></returns>
    Public Shared Function BackupSurfaceTemplateBaseName(ByVal side As IdElement3dManager.eSide, ByVal backupSrf As IdAddiction.eAddictionBkSrf) As String
        Return BK_SRF_TEMPLATE_PREFIX & SEPARATOR & [Enum].GetName(GetType(IdAddiction.eAddictionBkSrf), backupSrf) & SEPARATOR & TemplateSideString(side) & SEPARATOR
    End Function

    ''' <summary>
    ''' FORMATO: "BackupSrfAddiction" "_" eAddictionBkSrf "_" side "_" index
    ''' </summary>
    ''' <param name="side"></param>
    ''' <param name="backupSrf"></param>
    ''' <param name="index"></param>
    ''' <returns></returns>
    Public Shared Function BackupSurfaceTemplateName(ByVal side As IdElement3dManager.eSide, ByVal backupSrf As IdAddiction.eAddictionBkSrf, ByVal index As Integer) As String
        Return BackupSurfaceTemplateBaseName(side, backupSrf) & index.ToString
    End Function


    Public Shared Function TemplateSideString(ByVal side As IdElement3dManager.eSide) As String
        'If side = eSide.left Then
        '    Return "L"
        'Else
        '    Return "R"
        'End If
        Return [Enum].GetName(GetType(IdElement3dManager.eSide), side)
    End Function

    Public Shared Function TemplateType(ByVal type As IdAddiction.eAddictionType) As String
        'Select Case type
        '    Case eAddictionType.metatarsalBar
        '        Return "Metatarsal Bar"
        '    Case eAddictionType.olive
        '        Return "Olive"
        '    Case eAddictionType.metatarsalDome
        '        Return "Metatarsal Dome"
        '    Case eAddictionType.archSupprt
        '        Return "Arch Support"
        '    Case eAddictionType.cutout
        '        Return "Cutout"
        '    Case eAddictionType.horseShoe
        '        Return "Horse Shoe"
        '    Case Else
        '        Return [Enum].GetName(GetType(eAddictionType), type)
        'End Select
        Return [Enum].GetName(GetType(IdAddiction.eAddictionType), type)
    End Function

    Public Shared Function TemplateModel(ByVal model As IdAddiction.eAddictionModel) As String
        'Select Case model
        '    Case eAddictionModel.metbar3450, eAddictionModel.metbar3869, eAddictionModel.metbar3870
        '        Return model.ToString().Replace("metbar", "")
        '    Case eAddictionModel.metDome
        '        Return "MetDome"
        '    Case eAddictionModel.olive3423, eAddictionModel.olive3435, eAddictionModel.olive3447, eAddictionModel.olive6648
        '        Return model.ToString().Replace("olive", "")
        '    Case eAddictionModel.archsupport3030
        '        Return model.ToString().Replace("archsupport", "")
        '    Case eAddictionModel.cutoutTotal, eAddictionModel.horseShoeTotal
        '        Return "Total"
        '    Case eAddictionModel.cutoutPartial, eAddictionModel.horseShoePartial
        '        Return "Partial"
        '    Case Else
        '        Return [Enum].GetName(GetType(eAddictionModel), model)
        'End Select
        Return [Enum].GetName(GetType(IdAddiction.eAddictionModel), model)
    End Function

    Private Shared Function TemplateSize(ByVal size As IdAddiction.eAddictionSize) As String
        'Select Case size
        '    Case eAddictionSize.Piccolo
        '        Return "S"
        '    Case eAddictionSize.Medio
        '        Return "M"
        '    Case eAddictionSize.Grande
        '        Return "L"
        '    Case eAddictionSize.none
        '        Return [Enum].GetName(GetType(eAddictionModel), eAddictionSize.none)
        '    Case Else
        '        Return [Enum].GetName(GetType(eAddictionModel), eAddictionSize.none)
        'End Select
        Return [Enum].GetName(GetType(IdAddiction.eAddictionSize), size)
    End Function

    Private Shared Function TemplateDirection(ByVal addiction As IdAddiction) As String
        If addiction.Type <> IdAddiction.eAddictionType.cutout Then Return [Enum].GetName(GetType(AbstractCutoutCommons.eCutoutDirection), AbstractCutoutCommons.eCutoutDirection.none)
        'Select Case DirectCast(addiction, IdCutoutToTalAddiction).CutoutDirection
        '    Case eCutoutDirection.external
        '        Return "Ext"
        '    Case eCutoutDirection.internal
        '        Return "Int"
        '    Case eCutoutDirection.none
        '        Return eCutoutDirection.none.ToString
        '    Case Else
        '        Return eCutoutDirection.none.ToString
        'End Select
        Return [Enum].GetName(GetType(AbstractCutoutCommons.eCutoutDirection), DirectCast(addiction, IdCutoutToTalAddiction).CutoutDirection)
    End Function


#End Region


#Region " Riconoscimento "

    Public Shared Function IsAddictionSrfTemplateName(ByVal side As IdElement3dManager.eSide, ByVal name As String) As Boolean
        Return (name.StartsWith(SRF_TEMPLATE_PREFIX))
    End Function

    ''' <summary>
    '''  Formato "Addiction" "_" tipo "_" model "_" size "_" direction "_" side "_" index
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function ParseTemplateAddictionName(ByVal side As IdElement3dManager.eSide, ByVal name As String, ByRef type As IdAddiction.eAddictionType, ByRef model As IdAddiction.eAddictionModel,
                                                      ByRef size As IdAddiction.eAddictionSize, ByRef direction As AbstractCutoutCommons.eCutoutDirection, ByRef index As Integer) As Boolean

        Dim token() As String = name.Split(SEPARATOR)
        If Not IdAddiction.eAddictionType.TryParse(token(1), type) Then Return False
        If Not IdAddiction.eAddictionModel.TryParse(token(2), model) Then Return False
        If Not IdAddiction.eAddictionSize.TryParse(token(3), size) Then Return False
        If Not AbstractCutoutCommons.eCutoutDirection.TryParse(token(4), direction) Then Return False
        Dim readSide As IdElement3dManager.eSide
        If Not IdElement3dManager.eSide.TryParse(token(5), readSide) Then Return False
        If readSide <> side Then Return False
        If Not Integer.TryParse(token(6), index) Then Return False
        Return True
    End Function


    Public Shared Function IsAddictionBlendTemplateName(ByVal side As IdElement3dManager.eSide, ByVal name As String) As Boolean
        Return (name.StartsWith(BLEND_TEMPLATE_PREFIX))
    End Function

    Public Shared Function GetIndexBlendSrfFromName(ByVal side As IdElement3dManager.eSide, ByVal name As String) As Integer
        Dim result As Integer
        Integer.TryParse(name.Replace(BlendSurfaceTemplateBaseName(side), ""), result)
        Return result
    End Function

    ''' <summary>
    ''' Prima controllo il prefisso, poi il tipo specifico di backup surface e ritorno il tipo
    ''' </summary>
    ''' <param name="side"></param>
    ''' <param name="name"></param>
    ''' <param name="srfBkType"></param>
    ''' <returns></returns>
    Public Shared Function IsAddictionBKSrfTemplateName(ByVal side As IdElement3dManager.eSide, ByVal name As String, ByRef srfBkType As IdAddiction.eAddictionBkSrf) As Boolean
        'Controllo prima parte
        If Not name.StartsWith(BK_SRF_TEMPLATE_PREFIX) Then Return False
        'Controllo tipo di backup
        Dim partialName As String = name.Replace(BK_SRF_TEMPLATE_PREFIX & SEPARATOR, "")
        srfBkType = IdAddiction.eAddictionBkSrf.top
        If partialName.StartsWith([Enum].GetName(GetType(IdAddiction.eAddictionBkSrf), srfBkType)) Then Return True
        srfBkType = IdAddiction.eAddictionBkSrf.lateral
        If partialName.StartsWith([Enum].GetName(GetType(IdAddiction.eAddictionBkSrf), srfBkType)) Then Return True
        srfBkType = IdAddiction.eAddictionBkSrf.bottom
        If partialName.StartsWith([Enum].GetName(GetType(IdAddiction.eAddictionBkSrf), srfBkType)) Then Return True
        Return False
    End Function

    ''' <summary>
    ''' "BackupSrfAddiction" "_" eAddictionBkSrf "_" side "_" index
    ''' </summary>
    ''' <param name="side"></param>
    ''' <param name="name"></param>
    ''' <param name="srfBkType"></param>
    ''' <returns></returns>
    Public Shared Function GetIndexAddictionBkSrfFromName(ByVal side As IdElement3dManager.eSide, ByVal name As String, ByVal srfBkType As IdAddiction.eAddictionBkSrf) As Integer
        'Prendo la stringa da rimuovere
        Dim indexString As String = name.Replace(BackupSurfaceTemplateBaseName(side, srfBkType), "")
        Dim result As Integer
        Integer.TryParse(indexString, result)
        Return result
    End Function


#End Region


#End Region


#End Region


#Region " Util "


    Public Shared Function CheckAllGeometryExist(ByVal side As IdElement3dManager.eSide) As Boolean
        If Not IdElement3dManager.GetInstance.FootCurvesExists(side) Then Return False
        If Not IdElement3dManager.GetInstance.ObjectExist(IdElement3dManager.eReferences.finalUpperCurve, side) Then Return False
        If Not IdElement3dManager.GetInstance.ObjectExist(IdElement3dManager.eReferences.insoleTopSurface, side) Then Return False
        If Not IdElement3dManager.GetInstance.ObjectExist(IdElement3dManager.eReferences.insoleLateralSurface, side) Then Return False
        If Not IdElement3dManager.GetInstance.ObjectExist(IdElement3dManager.eReferences.insoleBottomSurface, side) Then Return False
        Return True
    End Function

    Public Shared Function CheckAnyGeometryExist(ByVal side As IdElement3dManager.eSide) As Boolean
        If IdElement3dManager.GetInstance.FootCurvesExists(side) Then Return True
        If IdElement3dManager.GetInstance.ObjectExist(IdElement3dManager.eReferences.finalUpperCurve, side) Then Return True
        If IdElement3dManager.GetInstance.ObjectExist(IdElement3dManager.eReferences.insoleTopSurface, side) Then Return True
        If IdElement3dManager.GetInstance.ObjectExist(IdElement3dManager.eReferences.insoleLateralSurface, side) Then Return True
        If IdElement3dManager.GetInstance.ObjectExist(IdElement3dManager.eReferences.insoleBottomSurface, side) Then Return True
        Return False
    End Function


#End Region



End Class
