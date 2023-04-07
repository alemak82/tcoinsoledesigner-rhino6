Imports RMA.OpenNURBS
Imports RhinoUtilsino.Geometry
Imports RMA.Rhino
Imports RhinoUtils
Imports RhinoUtils.RhGeometry
Imports InsoleDesigner.bll.IdElement3dManager
Imports InsoleDesigner.bll.IdLanguageManager
Imports InsoleDesigner.bll.IdAlias
Imports InsoleDesigner.bll.AbstractPianiInclinati


Public Class FactoryPianiInclinati
    

    Public Enum eTipoPianoInclinato
        tallone = 0
        pianta
        totale
        elica
    End Enum

    Public Enum ePosizionePianiInclinato
        mediale = 0
        laterale
        talloneMediale_piantaLaterale
        talloneLaterale_piantaMediale
    End Enum


    Public Shared Function Create(type As eTipoPianoInclinato, position As ePosizionePianiInclinato, spessore As Double) As AbstractPianiInclinati        
        Select Case type
            Case eTipoPianoInclinato.tallone
                Return New PianoInclinatoTallone(position, spessore)
            Case eTipoPianoInclinato.pianta
                Return New PianoInclinatoPianta(position, spessore)
            Case eTipoPianoInclinato.totale
                Return New PianoInclinatoTotale(position, spessore)
            Case eTipoPianoInclinato.elica
                Return New PianoInclinatoElica(position, spessore)
            Case Else
                Return Nothing
        End Select
    End Function

    Public Shared Function Deserialize(ByRef archive As OnBinaryArchive, ByRef pianoInclinato As AbstractPianiInclinati) As Boolean
        Dim pianoInclinatoType As eTipoPianoInclinato = Nothing
        Dim pianoInclinatoPosition As ePosizionePianiInclinato = Nothing
        Dim pianoInclinatoSpessore As Double = 0
        If Not CommonDeserialize(archive, pianoInclinatoType, pianoInclinatoPosition, pianoInclinatoSpessore) Then Return False
        pianoInclinato = Create(pianoInclinatoType, pianoInclinatoPosition, pianoInclinatoSpessore)
        pianoInclinato.Deserialize(archive)
        Return True
    End Function


    #Region " TESTI "


    Public Shared Function GetPositionName(tipo As FactoryPianiInclinati.ePosizionePianiInclinato) As String
        Select Case tipo
            Case FactoryPianiInclinati.ePosizionePianiInclinato.mediale
                Return IIf(LanguageManager().PluginLanguage = elanguage.Italian, "Mediale", "Medial")
            Case FactoryPianiInclinati.ePosizionePianiInclinato.laterale                  
                Return IIf(LanguageManager().PluginLanguage = elanguage.Italian, "Laterale", "Lateral")
            Case FactoryPianiInclinati.ePosizionePianiInclinato.talloneMediale_piantaLaterale                  
                Return IIf(LanguageManager().PluginLanguage = elanguage.Italian, "Mediale al tallone laterale in pianta", "Heel medial sole lateral")
            Case FactoryPianiInclinati.ePosizionePianiInclinato.talloneLaterale_piantaMediale
                Return IIf(LanguageManager().PluginLanguage = elanguage.Italian, "Laterale al tallone mediale in pianta", "Heel lateral sole medial")
            Case Else
                Return Nothing
        End Select
    End Function

    Public Shared Function GetTypeName(tipo As FactoryPianiInclinati.eTipoPianoInclinato) As String
        Select Case tipo
            Case FactoryPianiInclinati.eTipoPianoInclinato.tallone
                Return IIf(LanguageManager().PluginLanguage = elanguage.Italian, "Tallone", "Heel")
            Case FactoryPianiInclinati.eTipoPianoInclinato.pianta                  
                Return IIf(LanguageManager().PluginLanguage = elanguage.Italian, "Pianta", "Sole")
            Case FactoryPianiInclinati.eTipoPianoInclinato.totale
                Return IIf(LanguageManager().PluginLanguage = elanguage.Italian, "Totale", "Total")
            Case FactoryPianiInclinati.eTipoPianoInclinato.elica
                Return IIf(LanguageManager().PluginLanguage = elanguage.Italian, "Elica", "Helix")
            Case Else
                Return Nothing
        End Select
    End Function


#End Region


End Class
