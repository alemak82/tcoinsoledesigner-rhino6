Imports System.IO
Imports RMA.OpenNURBS
Imports InsoleDesigner.bll.IdElement3dManager
Imports InsoleDesigner.bll.IdAddiction
Imports InsoleDesigner.bll.IdMetbarAddiction
Imports InsoleDesigner.bll.IdCutoutToTalAddiction
Imports InsoleDesigner.bll.IdCutoutPartialAddiction
Imports InsoleDesigner.bll.IdHorseShoeTotalAddiction
Imports InsoleDesigner.bll.IdHorseShoePartialAddiction
Imports InsoleDesigner.bll.AbstractCutoutCommons


Public Class IdAddictionFactory


    Public Shared Function Create(ByVal side As eSide, ByVal type As IdAddiction.eAddictionType, ByVal model As IdAddiction.eAddictionModel,
                                 Optional ByVal size As IdAddiction.eAddictionSize = IdAddiction.eAddictionSize.none, Optional ByVal direction As AbstractCutoutCommons.eCutoutDirection = AbstractCutoutCommons.eCutoutDirection.none) As IdAddiction
        Dim result As IdAddiction = Nothing

        Select Case type
            Case IdAddiction.eAddictionType.metatarsalBar
                result = New IdMetbarAddiction(side, type, model, size)
            Case IdAddiction.eAddictionType.archSupprt
                result = New IdArchSupportAddiction(side, type, model, size)
            Case IdAddiction.eAddictionType.cutout
                If model = IdAddiction.eAddictionModel.cutoutTotal Then
                    result = New IdCutoutToTalAddiction(side, direction)
                ElseIf model = IdAddiction.eAddictionModel.cutoutPartial Then
                    result = New IdCutoutPartialAddiction(side, direction)
                End If
            Case IdAddiction.eAddictionType.horseShoe
                If model = IdAddiction.eAddictionModel.horseShoeTotal Then
                    result = New IdHorseShoeTotalAddiction(side)
                Else
                    result = New IdHorseShoePartialAddiction(side)
                End If
            Case IdAddiction.eAddictionType.metatarsalDome, IdAddiction.eAddictionType.olive
                result = New IdBaseAddiction(side, type, model, size)
        End Select

        Return result
    End Function



    ''' <summary>
    ''' Da usare solo per la deserializzazione - VEDI IdAddiction.Serialize()
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function DeserializeAddiction(ByRef archive As OnBinaryArchive, ByRef addiction As IdAddiction) As Boolean

        'Stringhe
        Dim text As String = String.Empty
        Dim side As eSide
        If Not archive.ReadString(text) Then Return False
        If Not eSide.TryParse(text, side) Then Return False
        Dim type As IdAddiction.eAddictionType
        If Not archive.ReadString(text) Then Return False
        If Not IdAddiction.eAddictionType.TryParse(text, type) Then Return False
        Dim model As IdAddiction.eAddictionModel
        If Not archive.ReadString(text) Then Return False
        If Not IdAddiction.eAddictionModel.TryParse(text, model) Then Return False
        Dim size As IdAddiction.eAddictionSize
        If Not archive.ReadString(text) Then Return False
        If Not IdAddiction.eAddictionSize.TryParse(text, size) Then Return False

        If type = IdAddiction.eAddictionType.cutout Then
            Dim direction As AbstractCutoutCommons.eCutoutDirection
            If Not archive.ReadString(text) Then Return False
            If Not IdAddiction.eAddictionModel.TryParse(text, direction) Then Return False
            addiction = Create(side, type, model, size, direction)
        Else
            addiction = Create(side, type, model, size)
        End If

        'UUID       
        If Not archive.ReadUuid(addiction.SurfaceID) Then Return False
        If Not archive.ReadUuid(addiction.BlendSurfaceID) Then Return False

        'BREP
        Dim onobj As OnObject = New OnBrep()
        If Not CBool(archive.ReadObject(onobj)) Then Return False
        addiction.BackupInsoleSurface(IdAddiction.eAddictionBkSrf.top) = OnBrep.Cast(onobj).Duplicate
        onobj.Dispose()

        ''CAMPI CUSTOM DEI DIVERSI TIPI DI SCARICO
        If Not addiction.Deserialize(archive) Then Return False

        Return True
    End Function


    Public Shared Function NeedBlend(ByVal type As IdAddiction.eAddictionType, ByVal model As IdAddiction.eAddictionModel) As Boolean
        Select Case type
            Case IdAddiction.eAddictionType.olive, IdAddiction.eAddictionType.metatarsalDome
                Return True
                'Case eAddictionType.cutout, eAddictionType.horseShoe
                '    If model = eAddictionModel.horseShoePartial Or model = eAddictionModel.cutoutPartial Then Return True
            Case Else
                Return False
        End Select
        Return False
    End Function


End Class
