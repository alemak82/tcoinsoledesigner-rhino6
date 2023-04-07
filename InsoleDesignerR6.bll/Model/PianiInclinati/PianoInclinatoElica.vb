Imports RMA.OpenNURBS
Imports InsoleDesigner.bll.IdElement3dManager
Imports InsoleDesigner.bll.IdLanguageManager
Imports InsoleDesigner.bll.IdAlias
Imports InsoleDesigner.bll.FactoryPianiInclinati


Public Class PianoInclinatoElica
    Inherits AbstractPianiInclinati
    Implements IOnSerializable
    Implements ICloneable


#Region " FIeld "


    Private _spessore2 As Double


#End Region


#Region " Constructor "

    Public Sub New(position As ePosizionePianiInclinato, spessore As Double)
        MyBase.New(eTipoPianoInclinato.elica, position, spessore)
        
        If Me.Position = ePosizionePianiInclinato.talloneLaterale_piantaMediale Then
            Tallone = New PianoInclinatoTallone(ePosizionePianiInclinato.laterale, Spessore)
            Pianta = New PianoInclinatoPianta(ePosizionePianiInclinato.mediale, Spessore)
        ElseIf Me.Position = ePosizionePianiInclinato.talloneMediale_piantaLaterale Then
            Tallone = New PianoInclinatoTallone(ePosizionePianiInclinato.mediale, Spessore)
            Pianta = New PianoInclinatoPianta(ePosizionePianiInclinato.laterale, Spessore)
        End If
    End Sub


#End Region


#Region " PROPERTY "

    Public Property Tallone() As AbstractPianiInclinati
    Public Property Pianta() As AbstractPianiInclinati

    ''' <summary>
    ''' In pianta speesore2
    ''' </summary>
    ''' <returns></returns>
    Public Property Spessore2() As Double
        Get
            Return _spessore2
        End Get
        Set(value As Double)
            _spessore2 = value
            Pianta.Spessore = _spessore2
        End Set
    End Property

#End Region


#Region " Overrides base "

    Public Overrides Function ToString() As String
        Return GetTypeName(Me.Type) & " " & GetPositionName(Me.Position) & " " & Me.Spessore & " - " & Me.Spessore2 & " mm"
    End Function

    Public Overrides Sub PulisciOggetiCostruzione()
        Tallone.PulisciOggetiCostruzione()
        Pianta.PulisciOggetiCostruzione()
    End Sub

    Public Overrides Sub DeleteFromDocument()
        Tallone.DeleteFromDocument()
        Pianta.DeleteFromDocument()
    End Sub

    Public Overrides Function IsInDoc() As Boolean
        Return Tallone.IsInDoc() And Pianta.IsInDoc()
    End Function

    Protected Overrides Function GetIdInizioBordo() As Guid
        Throw New NotImplementedException
    End Function

    Protected Overrides Function GetIdFineBordo() As Guid
        Throw New NotImplementedException
    End Function

#End Region


#Region " IClonable "

    Public Overrides Function Clone() As Object
        Dim res As New PianoInclinatoElica(Me.Position, Me.Spessore)
        res.Tallone = Me.Tallone.Clone()
        res.Pianta = Me.Pianta.Clone()
        res.Spessore2 = Me.Spessore2
        Return res     
    End Function

#End Region


#Region " Serializzazione/deserializzazione"

    Public Overrides Function Serialize(ByRef archive As OnBinaryArchive) As Boolean
        If Not MyBase.CommonSerialize(archive) Then Return False

        'double
        If Not archive.WriteDouble(_spessore2) Then Return False

        If Not Me.Tallone.Serialize(archive) Then Return False
        If Not Me.Pianta.Serialize(archive) Then Return False

        Return True
    End Function

    Public Overrides Function Deserialize(ByRef archive As OnBinaryArchive) As Boolean
        'double         
        If Not archive.ReadDouble(_spessore2) Then Return False

        If Not FactoryPianiInclinati.Deserialize(archive, Me.Tallone) Then Return False
        If Not FactoryPianiInclinati.Deserialize(archive, Me.Pianta) Then Return False

        Return True
    End Function

#End Region


End Class
