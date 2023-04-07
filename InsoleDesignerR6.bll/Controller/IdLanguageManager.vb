Imports System.Threading
Imports System.Globalization
Imports InsoleDesigner.bll.IdElement3dManager
Imports InsoleDesigner.bll.IdAddiction
Imports System.Reflection
Imports RMA.Rhino
Imports CommonsUtils.Maintenance


Public Class IdLanguageManager


#Region " ENUM "

    ''' <summary>
    ''' Craeta per gestire la lingua di Rhino che usa un intero e poi usata per gestire la lingua nel plugin
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum elanguage
        Unsupported = 0
        Italian
        English
    End Enum


#End Region


#Region " Field "

    Private Shared mInstance As IdLanguageManager
    Private mPluginLanguage As elanguage

#End Region


#Region " Constructor "


    ''' <summary>
    ''' Inizializzazione campi privati
    ''' </summary>
    Private Sub New()
        'All'avvio la lingua settata corrisponde a quell dell'ultimo avvio
        SetStartupLanguage()
    End Sub


    Public Shared Function GetInstance() As IdLanguageManager
        If mInstance Is Nothing Then
            mInstance = New IdLanguageManager()
        End If
        Return mInstance
    End Function


#End Region


#Region " Gestione lingua Rhino e plugin "


    Public Shared Function RhinoLanguageSetting() As elanguage
        Dim settings As MRhinoAppSettings = RhUtil.RhinoApp.AppSettings
        Dim appearance As IRhinoAppAppearanceSettings = settings.AppearanceSettings()
        Dim id As Integer = CType(appearance.m_language_identifier, Integer)
        Dim culture As New CultureInfo(id)
        If culture.Name = "it-IT" Then
            Return elanguage.Italian
        ElseIf culture.Name = "en-EN" Or culture.Name = "en-US" Then
            Return elanguage.English
        Else
            Return elanguage.Unsupported
        End If
    End Function


    Private Sub SetStartupLanguage()

        Dim language As elanguage = elanguage.Unsupported

        'Cerco la lingua usata all'ultima esecuzione
        elanguage.TryParse(My.Settings.LastPluginLanguage, language)

        'Se è la prima esecuzione del software uso la lingua di Rhino
        If language = elanguage.Unsupported Then language = RhinoLanguageSetting()

        'In caso estremo impongo inglese
        If language = elanguage.Unsupported Then language = elanguage.English

        PluginLanguage = language
    End Sub

    ''' <summary>
    ''' Impostando la cultura le traduzioni dei form dipendono dai relativi file *.resx e persiste l'impostazione
    ''' </summary>
    ''' <remarks></remarks>
    Public Property PluginLanguage() As elanguage
        Get
            Return mPluginLanguage
        End Get
        Set(value As elanguage)
            mPluginLanguage = value
            'Salvo per avvio successivo
            My.Settings.LastPluginLanguage = mPluginLanguage.ToString()
            My.Settings.Save()
            'Aggiorno cultura applicazione
            Me.UpdateCultureGUI(mPluginLanguage)
        End Set
    End Property


    Private Sub UpdateCultureGUI(ByVal language As elanguage)
        Dim cultureName As String = "en"
        Select Case language
            Case elanguage.Italian
                cultureName = "it-IT"
            Case elanguage.English
                cultureName = "en"
        End Select
        Thread.CurrentThread.CurrentCulture = New CultureInfo(cultureName)
        My.Application.ChangeUICulture(cultureName)
    End Sub


#End Region


#Region " Scarichi "


    ''' <summary>
    ''' Leggo dai resources localizzati(ita-eng ecc...)
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function AddictionTypeName(ByVal type As eAddictionType) As String
        Select Case type
            Case eAddictionType.metatarsalBar
                Return My.Resources.AddctionTypeMetatarsalBar
            Case eAddictionType.archSupprt
                Return My.Resources.AddctionTypeArchSupport
            Case eAddictionType.cutout
                Return My.Resources.AddctionTypeCutout
            Case eAddictionType.horseShoe
                Return My.Resources.AddctionTypeHorseshoeOffload
            Case eAddictionType.metatarsalDome
                Return My.Resources.AddctionTypeMetatarsalDome
            Case eAddictionType.olive
                Return My.Resources.AddctionTypeOlive
            Case Else
                Return ""
        End Select
    End Function


#End Region


#Region " Utils "


    Public Function TranslateSide(ByVal side As eSide) As String
        If side = eSide.left Then
            Select Case PluginLanguage
                Case elanguage.Italian
                    Return "sinistro"
                Case elanguage.English
                    Return "left"
                Case Else
                    Return "left"
            End Select
        Else
            Select Case PluginLanguage
                Case elanguage.Italian
                    Return "destro"
                Case elanguage.English
                    Return "right"
                Case Else
                    Return "right"
            End Select
        End If
    End Function


#End Region


#Region " Error and message "


#Region " Campi dei messaggi "

    Private mSide As eSide
    Private mText As String
    Private mNumber As Integer
    Private mCoordinate As Double
    Private mAddictionType As eAddictionType
    Private mAddictionType2 As eAddictionType

#End Region


#Region " Overload per gestire parametri nei messaggi "


    ''' <summary>
    ''' Metodo principale che richiama la funzione che corrisponde alla lingua corrente
    ''' </summary>
    ''' <param name="index"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Message(ByVal index As Integer) As String
        Dim res = ""
        Select Case PluginLanguage
            Case elanguage.Unsupported
                res = GetMessageENG(index)
            Case elanguage.Italian
                res = GetMessageITA(index)
            Case elanguage.English
                res = GetMessageENG(index)

                '...ALTRE LINGUE

            Case Else
                res = GetMessageENG(index)
                If String.IsNullOrEmpty(res) Or String.IsNullOrWhiteSpace(res) Then res = GetMessageITA(index)
        End Select
        If String.IsNullOrEmpty(res) Or String.IsNullOrWhiteSpace(res) Then res = "Warnign generic"
        RhUtil.RhinoApp.ActiveDoc.EndWaitCursor()
        Return res
    End Function


    Public Function Message(ByVal index As Integer, ByVal side As eSide) As String
        Me.mSide = side
        Return Message(index)
    End Function

    Public Function Message(ByVal index As Integer, ByVal text As String) As String
        Me.mText = text
        Return Message(index)
    End Function

    Public Function Message(ByVal index As Integer, ByVal coordinate As Double) As String
        'Approssimo a 3 cifre dopo la virgola
        Me.mCoordinate = Math.Round(coordinate, 3)
        Return Message(index)
    End Function

    Public Function Message(ByVal index As Integer, ByVal coordinate As Double, ByVal number As Integer) As String
        Me.mNumber = number
        Return Message(index, coordinate)
    End Function

    Public Function Message(ByVal index As Integer, ByVal side As eSide, ByVal coordinate As Double) As String
        Me.mSide = side
        Return Message(index, coordinate)
    End Function

    Public Function Message(ByVal index As Integer, ByVal addictionType As eAddictionType) As String
        Me.mAddictionType = addictionType
        Return Message(index)
    End Function

    Public Function Message(ByVal index As Integer, ByVal addictionType As eAddictionType, ByVal addictionType2 As eAddictionType) As String
        Me.mAddictionType = addictionType
        Me.mAddictionType2 = addictionType2
        Return Message(index)
    End Function


#End Region


#Region " Traduzioni "


    ''' <summary>
    ''' Messaggi in italiano che eventualmente usano i campi impostati
    ''' </summary>
    ''' <param name="index">indice univoco del messaggio</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function GetMessageITA(ByVal index As Integer) As String

        Try

            Select Case index

        'MESSAGGI CON PARAMETRI
                Case 0 : Return "Lo scarico " & AddictionTypeName(mAddictionType) & " deve essere il primo scarico applicato"
                Case 1 : Return "Gli scarichi " & AddictionTypeName(mAddictionType) & " e " & AddictionTypeName(mAddictionType2) & " sono incompatibili"
                Case 2 : Return "Impossibile creare il profilo TCO lato " & TranslateSide(mSide)
                Case 3 : Return "Impossibile leggere il numero di righe della curva " & TranslateSide(mSide)
                Case 4 : Return "Lato " & TranslateSide(mSide) & " - tagliare la superficie laterale con il profilo TCO ?"
                Case 5 : Return "Lato " & TranslateSide(mSide) & ": curve esistenti, proseguo?"
                Case 6 : Return "Tutti gli oggetti lato " & TranslateSide(mSide) & " verranno cancellati, proseguire?"
                Case 7 : Return "Impossibile salvare le geometrie del lato " & TranslateSide(mSide)
                Case 8 : Return "Impossibile salvare il file template lato " & TranslateSide(mSide)
                Case 9 : Return "Impossibile sostituire la curva in posizione " & mCoordinate & " lato " & TranslateSide(mSide) & " con una rettilinea"
                Case 10 : Return "Impossibile ricostruire il tallone lato " & TranslateSide(mSide)
                Case 11 : Return "Impossibile trovare il file " & mText
                Case 12 : Return "Impossibile proiettare le curve del " & AddictionTypeName(mAddictionType)
                Case 13 : Return GetMessageITA(131) & " " & AddictionTypeName(mAddictionType)
                Case 14 : Return GetMessageITA(136) & " " & AddictionTypeName(mAddictionType)
                Case 15 : Return "Curve esterne in X rispetto alla superficie superiore del lato " & TranslateSide(mSide) & ", posizionare lo scarico correttamente."
                Case 16 : Return "Curve esterne in Y rispetto alla superficie superiore del lato " & TranslateSide(mSide) & ", posizionare lo scarico correttamente."
                Case 17 : Return "Curve esterne rispetto alla superficie superiore del lato " & TranslateSide(mSide) & ", posizionare lo scarico correttamente."
                Case 18 : Return "Chiudere il form '" & mText & "' per salvare i dati di Insole Designer"
                Case 19 : Return "Impossibile creare la curva lato " & TranslateSide(mSide)
                Case 20 : Return "Controllare la curva di sezione in posizione X=" & mCoordinate & " che interseca la forma in " & mNumber & "punti"
                Case 21 : Return "Impossibile raccordare le curve perchè più distanti di " & mCoordinate & "mm"
                Case 22 : Return "Formato del file " & mText & " non corretto"
                Case 23 : Return "Header del file " & mText & " non corretto"
                Case 24 : Return "Chiudere il form '" & mText & "' prima di proseguire"
                Case 25 : Return "Impossibile riconoscere le curve di base dello scarico, controllare il file '" & mText & "'"
                Case 26 : Return "Impossibile riconoscere le curve superiori dello scarico, controllare il file '" & mText & "'"
                Case 27 : Return "Impossibile trovare il file del template lato " & TranslateSide(mSide)
                Case 28 : Return "Impossibile allineare lato " & TranslateSide(mSide) & ": mesh del piede e plantare non trovati"
                Case 29 : Return "Nessun piano inclinato trovato lato " & TranslateSide(mSide)
                Case 30 : Return "Piano inclinato esistente lato " & TranslateSide(mSide) & ", proseguire e sovrascrivere?"
                Case 31 : Return ""
                Case 32 : Return ""
                Case 33 : Return ""




                    ''MESSAGGI SENZA PARAMETRI
                Case 100 : Return "importato correttamente"
                Case 101 : Return "non importato"
                Case 102 : Return "Salvataggio dei dati Insole Designer completato"
                Case 103 : Return "Importazione dei dati Insole Designer completata"
                Case 104 : Return "Undo terminato"
                Case 105 : Return "Redo terminato"
                Case 106 : Return "Inserimento scarico terminato"
                Case 107 : Return "Creazione sottopiede terminata"
                Case 108 : Return "Salvataggio template terminato"
                Case 109 : Return "Rimozione dell'ultimo scarico terminata"
                Case 110 : Return "Ricostruzione del tallone terminata"
                Case 111 : Return "Apertura template terminata"
                Case 112 : Return "Specchiatura terminata"
                Case 113 : Return "Import da Last Designer terminato"
                Case 114 : Return "Caricamento file terminato"
                Case 115 : Return "Unione delle superfici terminata"
                Case 116 : Return "Curve di sezione del piede terminate"
                Case 117 : Return "Taglio con profilo TCO terminato"
                Case 118 : Return "Creazione guide del plantare terminata"
                Case 119 : Return "Creazione plantare terminata"
                Case 120 : Return "Allineamento terminato"
                Case 121 : Return "Caricamento profili TCO terminato"
                Case 122 : Return "Inserimento righello terminato"
                Case 123 : Return "Correzione automatica terminata"
                Case 124 : Return "Impossibile trovare le superfici del plantare"
                Case 125 : Return "Impossibile posizionare lo scarico"
                Case 126 : Return "Impossibile eseguire l'import dello scarico"
                Case 127 : Return "Scarico non trovato"
                Case 128 : Return "Superficie superiore del plantare non trovata"
                Case 129 : Return "Una o più curve intersecano la superficie, posizionare lo scarico correttamente"
                Case 130 : Return "Scarico di tipo errato"
                Case 131 : Return "Impossibile eseguire la raccordatura dello scarico"
                Case 132 : Return "Impossibile proiettare la barra metatarsale"
                Case 133 : Return "Impossibile creare la superficie dello scarico"
                Case 134 : Return "Impossibile raccordare lo scarico"
                Case 135 : Return "Impossibile creare il raccordo dello scarico"
                Case 136 : Return "Impossibile applicare il"
                Case 137 : Return "Curve dello scarico non trovate"
                Case 138 : Return "Curve al di sotto della superficie, posizionare lo scarico correttamente"
                Case 139 : Return "Impossibile proiettare lo scarico"
                Case 140 : Return "Impossibile inserire il righello!"
                Case 141 : Return "Impossibile inserire il profilo TCO!"
                Case 142 : Return "Impossibile creare la superficie di loft"
                Case 143 : Return "Impossibile creare la superficie di intersezione"
                Case 144 : Return "Impossibile intersecare le superfici"
                Case 145 : Return "Superficie di loft non creata correttmente"
                Case 146 : Return "Impossibile estendere posteriormente la superficie di loft"
                Case 147 : Return "Impossibile estendere anteriormente la superficie di loft"
                Case 148 : Return "Impossibile ottenere una curva unica"
                Case 149 : Return "Impossibile creare la curva con i punti letti nel file"
                Case 150 : Return "Mesh o curve del piede non trovate"
                Case 151 : Return "Curve del plantare non trovate"
                Case 152 : Return "Forma non trovata"
                Case 153 : Return "Impossibile creare il plantare!"
                Case 154 : Return "Impossibile creare la superficie superiore del plantare"
                Case 155 : Return "E' stata rilevata un'intersezione tra la superficie superiore e inferiore del plantare"
                Case 156 : Return "Split della forma non riuscito"
                Case 157 : Return "Creazione della polisuperficie per la lavorazione manuale non riuscita."
                Case 158 : Return "Impossibile tagliare la superficie laterale con il profilo TCO"
                Case 159 : Return "Impossibile creare il cilindro per il taglio"
                Case 160 : Return "Impossibile creare lo smusso della supercifie laterale del plantare"
                Case 161 : Return "Impossibile creare lo smusso della supercifie superiore del plantare"
                Case 162 : Return "Impossibile creare lo smusso della supercifie laterale della lavorazione manuale"
                Case 163 : Return "Impossibile portare a termine la lisciatura della superficie superiore del plantare"
                Case 164 : Return "Curve di sezione del piede non trovate"
                Case 165 : Return "Impossibile creare le guide del plantare!"
                Case 166 : Return "Attenzione non è stato possibile aggiornare la curva di profilo superiore"
                Case 167 : Return "Non è stato possibile raccordare una o più curve del piede, verranno eliminate"
                Case 168 : Return "Completamento delle curve del piede non riuscito"
                Case 169 : Return "Impossibile trovare il punto esterno della curva di sezione per lo sweep della superficie di taglio"
                Case 170 : Return "Non è stato trovato il punto esterno della curva di sezione per lo sweep della superficie di taglio"
                Case 171 : Return "Impossibile creare la superficie di taglio"
                Case 172 : Return "Impossibile trovare l'intersezione tra la superficie di taglio e la forma"
                Case 173 : Return "Impossibile chiudere la curva di intersezione tra la superficie di taglio e la forma"
                Case 174 : Return "Sono state rilevate ed eliminate curve del piede mal posizionate rispetto al piano XY"
                Case 175 : Return "Superfici non trovate, comando annullato"
                Case 176 : Return "Impossibile eseguire lo Split delle superfici con il profilo TCO"
                Case 177 : Return "Forma non trovata, esco."
                Case 178 : Return "Elimino le curve esistenti?"
                Case 179 : Return "Curva non valida perchè più corta della forma!"
                Case 180 : Return "L'intersezione tra la curva esterna e la forma non è continua"
                Case 181 : Return "L'intersezione tra la curva interna e la forma non è continua."
                Case 182 : Return "Curva di contorno del plantare non valida"
                Case 183 : Return "Impossibile tagliare la curva interna nel tallone"
                Case 184 : Return "Impossibile tagliare la curva interna in punta"
                Case 185 : Return "Impossibile tagliare la curva esterna nel tallone!"
                Case 186 : Return "Impossibile tagliare la curva esterna in punta!"
                Case 187 : Return "Impossibile creare le curve di raccordo"
                Case 188 : Return "Mesh del piede non trovato"
                Case 189 : Return "Impossibile trovare l'intersezione tra il piede e i piani"
                Case 190 : Return "Creazione della superficie di raccordo superiore non riuscita"
                Case 191 : Return "Unione delle superfici non riuscita"
                Case 192 : Return "Impossibile eseguire il caricamento"
                Case 193 : Return "Impossibile caricare la forma, numero di superfici trovate non corretto"
                Case 194 : Return "Impossibile eseguire l'import da Last Designer"
                Case 195 : Return "Impossibile duplicare le superfici della forma"
                Case 196 : Return "Superficie superiore della forma non trovata"
                Case 197 : Return "Impossibile duplicare la mesh del piede"
                Case 198 : Return "Impossibile eseguire la specchiatura"
                Case 199 : Return "Impossibile eseguire la specchiatura in presenza di scarichi"
                Case 200 : Return "Impossibile trovare tutti gli elementi necessari per la specchiatura"
                Case 201 : Return "Proseguire e sovrascrivere gli oggetti presenti nel documento?"
                Case 202 : Return "Impossibile aprire il template"
                Case 203 : Return "Impossibile ricostruire il tallone"
                Case 204 : Return "Nessuno scarico trovato"
                Case 205 : Return "Impossibile ripristinare le superfici del plantare"
                Case 206 : Return "Salvataggio template completato"
                Case 207 : Return "Impossibile salvare il template"
                Case 208 : Return "Impossibile creare il sottopiede"
                Case 209 : Return "Procedura non completata esco ed elimino lo scarico?"
                Case 210 : Return "Impossibile eseguire il resize automatico dello scarico"
                Case 211 : Return "Prego inserire una taglia"
                Case 212 : Return "Licenza non valida"
                Case 213 : Return "Elimino la curva di profilo?"
                Case 214 : Return "Impossibile disegnare la curva"
                Case 215 : Return "Impossibile creare la curva di profilo"
                Case 216 : Return "Correzione automatica delle curve fallita"
                Case 217 : Return "Curva di profilo modificata correttamente"
                Case 218 : Return "Curva di profilo sostituita correttamente"
                Case 219 : Return "Impossibile salvare il nuovo template"
                Case 220 : Return "Impossibile estendere la curva L1"
                Case 221 : Return "Impossibile estendere la curva L2"
                Case 222 : Return "Impossibile creare il raccordo richiesto"
                Case 223 : Return "Raccordatura L1-L2 fallita"
                Case 224 : Return "Impossibile eseguire la raccordatura del cutout"
                Case 225 : Return "Impossibile eseguire la raccordatura del ferro di cavallo"
                Case 226 : Return "Impossibile eseguire la raccordatura del ferro di cavallo"
                Case 227 : Return "Impossibile estendere la curva L3"
                Case 228 : Return "Impossibile estendere la curva L4"
                Case 229 : Return "Il valore inserito è maggiore della dimensione dello scarico"
                Case 230 : Return "Impossibile creare l'anteprima della raccordatura del cutout"
                Case 231 : Return "Impossibile creare l'anteprima della raccordatura del ferro di cavallo"
                Case 232 : Return "Interrompere il comando corrente prima di continuare"
                Case 233 : Return "Attualmente è in esecuzione un altro comando."
                Case 234 : Return "Impossibile eseguire lo split delle curve del piede"
                Case 235 : Return "Impossibile caricare il plug-in Insole Designer"
                Case 236 : Return "Database non trovato"
                Case 237 : Return "Impossibile controllare il verso della superficie, l'oggetto è nullo"
                Case 238 : Return "Angolo tra le curve maggiore di 180°"
                Case 239 : Return "Impossibile unire la curva di base del plantare"
                Case 240 : Return "Impossibile fare il rebuild della curva di base del plantare"
                Case 241 : Return "Impossibile controllare il verso della superficie, la superficie è nulla"
                Case 242 : Return "Impossibile controllare il verso della superficie, il relativo Brep è nullo"
                Case 243 : Return "Rilevata intersezione tra lato destro e sinistro, traslo il sinistro"
                Case 244 : Return "Impossibile ripristinare il documento"
                Case 245 : Return "Impossibile trovare la superficie laterale del plantare"
                Case 246 : Return "Impossibile trovare la superficie inferiore del plantare"
                Case 247 : Return "Tipologia di scarico errata per la proiezione"
                Case 248 : Return "Curva di base esterna del supporto volta non trovata"
                Case 249 : Return "Impossibile eseguire lo split della curva di profilo del plantare"
                Case 250 : Return "Impossibile proiettare la curva interna del supporto volta sul plantare"
                Case 251 : Return "Nessuna curva risultante dalla proiezione della curva interna del supporto volta sul plantare"
                Case 252 : Return "Impossibile ricavare una curva dalla proiezione della curva anteriore del supporto volta"
                Case 253 : Return "Estrusione dalle curve longitudinali fallita"
                Case 254 : Return "Estensione della superficie estrusa dalle curve longitudinali fallita"
                Case 255 : Return "Rilevata curva longitudinale esterna al plantare"
                Case 256 : Return "Estensione della superficie estrusa dalle curve trasversali fallita"
                Case 257 : Return "Estensione della superficie estrusa dalle curve longitudinali fallita"
                Case 258 : Return "Impossibile trovare un punto estremo di una curva interna"
                Case 259 : Return "Impossibile trovare un punto estremo di una curva esterna"
                Case 260 : Return "Impossibile proiettare i punti delle curve superiori sulla superficie del plantare"
                Case 261 : Return "Una o più curve superiori longitudinali della superficie escono dal plantare"
                Case 262 : Return "Impossibile proiettare lo scarico perchè la superficie superiore non è stata trovata"
                Case 263 : Return "Controllare la posizione della barra metatarsale prima di procedere con la proiezione"
                Case 264 : Return "Impossibile estrudere le curve denominate 'A' e 'D' secondo la procedura per proiettare lo scarico"
                Case 265 : Return "Impossibile intersecare il plantare con superficie derivata dall'estrusione della curva denominata 'A'"
                Case 266 : Return "Impossibile intersecare il plantare con superficie derivata dall'estrusione della curva denominata 'D'"
                Case 267 : Return "Impossibile estrarre il bordo della superficie superiore del pantare"
                Case 268 : Return "Errore nello split del plantare con superficie derivata dall'estrusione della curva denominata 'A'"
                Case 269 : Return "Errore nello split del plantare con superficie derivata dall'estrusione della curva denominata 'D'"
                Case 270 : Return "Estrusione dalle curve longitudinali fallita"
                Case 271 : Return "Estensione della superficie estrusa dalle curve longitudinali fallita"
                Case 272 : Return "Estensione della superficie estrusa dalle curve trasversali fallita"
                Case 273 : Return "Impossibile trovare un punto estremo di una curva trasversale"
                Case 274 : Return "Impossibile trovare un punto estremo di una curva trasversale"
                Case 275 : Return "Errore nel proiezione dei punti delle curve superiori sulla superficie del plantare"
                Case 276 : Return "Una o più curve superiori longitudinali della superficie escono dal plantare"
                Case 277 : Return "Impossibile identificare il terzo punto per il comando Splop"
                Case 278 : Return "Impossibile eseguire lo split della curva L1"
                Case 279 : Return "Impossibile eseguire lo split della curva L2"
                Case 280 : Return "Impossibile eseguire lo split della curva L3"
                Case 281 : Return "Impossibile eseguire lo split della curva L4"
                Case 282 : Return "Impossibile unire le curve del ferro di cavallo"
                Case 283 : Return "Impossibile creare una curva unica del ferro di cavallo"
                Case 284 : Return "Curve di riferimento non trovate"
                Case 285 : Return "Impossibile trovare le curve cutout"
                Case 286 : Return "Impossibile creare la superficie del cutout"
                Case 287 : Return "Impossibile trovare la superficie superiore del plantare"
                Case 288 : Return "Superficie dello scarico non trovata"
                Case 289 : Return "Superficie laterale del plantare non trovata"
                Case 290 : Return "Superficie inferiore del plantare non trovata"
                Case 291 : Return "Impossibile trovare la superficie estrusa del ferro di cavallo"
                Case 292 : Return "Impossibile trovare la copia della siperficie superiore del plantare"
                Case 293 : Return "Impossibile estrarre la curva di bordo della supericie copiata"
                Case 294 : Return "Impossibile eseguire l'offset della curva di bordo della supericie copiata"
                Case 295 : Return "Impossibile tagliare la superficie superiore del plantare"
                Case 296 : Return "Impossibile tagliare la superficie superiore del plantare copiata"
                Case 297 : Return "Impossibile recuperare la superficie di sweep"
                Case 298 : Return "Scarico non trovato"
                Case 299 : Return "Tipologia di scarico errata per la proiezione"
                Case 300 : Return "Impossibile eseguire lo split della superficie superiore del plantare"
                Case 301 : Return "Superfici di estrusione non trovate"
                Case 302 : Return "Errore nel primo split del plantare per l'inserimento dello scarico con relativo smusso"
                Case 303 : Return "Errore nel secondo split del plantare per l'inserimento dello scarico con relativo smusso"
                Case 304 : Return "Impossibile ricreare la superficie della barra metatarsale unita al plantare"
                Case 305 : Return "Impossibile creare la curva di bordo dello scarico"
                Case 306 : Return "Errore rebuild curva di bordo dello scarico"
                Case 307 : Return "Errore creazione cilindro per il raccordo"
                Case 308 : Return "Errore: lo scarico non è adiacente al plantare"
                Case 309 : Return "Errore nell'intersezione tra lo scarico e il cilindro per lo smusso"
                Case 310 : Return "Errore nello split del plantare per l'inserimento dello scarico con relativo smusso"
                Case 311 : Return "Errore nello split dello scarico per lo smusso"
                Case 312 : Return "Errore nel rebuild delle curve per lo sweep a due binari: una delle due curve è nulla"
                Case 313 : Return "Errore nello sweep2 per la creazione dello smusso"
                Case 314 : Return "Impossibile tagliare la superficie superiore del plantare"
                Case 315 : Return "Impossibile estrarre le curve di bordo della superficie superiore"
                Case 316 : Return "Impossibile estrarre le curve di bordo della superficie inferiore"
                Case 317 : Return "Impossibile tagliare la superficie laterale del plantare"
                Case 318 : Return "Impossibile unire la superficie laterale dopo lo Split"
                Case 319 : Return "Impossibile estrarre le curve di bordo della superficie laterale"
                Case 320 : Return "Impossibile unire le curve di bordo"
                Case 321 : Return "Numero delle curve di bordo non corretto"
                Case 322 : Return "Impossibile eseguire lo Split della superficie di taglio"
                Case 323 : Return "Impossibile trovare la superficie estrusa del cutout"
                Case 324 : Return "Impossibile trovare la copia della siperficie superiore del plantare"
                Case 325 : Return "Impossibile estrarre la curva di bordo della supericie copiata"
                Case 326 : Return "Impossibile eseguire l'offset della curva di bordo della supericie copiata"
                Case 327 : Return "Impossibile creare la superficie dello scarico cutout parziale"
                Case 328 : Return "Impossibile calcolare le curve laterali del raccordo del cutout"
                Case 329 : Return "Impossibile calcolare la curva superiore del raccordo del cutout"
                Case 330 : Return "Impossibile calcolare la curva inferiore del raccordo del cutout"
                Case 331 : Return "Impossibile unire le curve del raccordo in una curva unica"
                Case 332 : Return "Impossibile calcolare le curve laterali del raccordo del ferro di cavallo"
                Case 333 : Return "Impossibile calcolare la curva superiore del raccordo del ferro di cavallo"
                Case 334 : Return "Impossibile calcolare la curva inferiore del raccordo del ferro di cavallo"
                Case 335 : Return "Errore nel rebuild delle curve per lo sweep a due binari: curve non trovate"
                Case 336 : Return "Impossibile unire le curve del raccordo del cutout"
                Case 337 : Return "Impossibile unire le curve del raccordo del derro di cavallo"
                Case 338 : Return "Impossibile creare la directory dei template, verificare che si disponga dei permessi necessari"
                Case 339 : Return "Selezionare una superficie di partenza"
                Case 340 : Return "Superficie selezionata non valida"
                Case 341 : Return "Impossibile estrarre il bordo della superficie di partenza"
                Case 342 : Return "Impossibile intersecate la superficie longitudinale con quella di partenza"
                Case 343 : Return "Intersezione tra la superficie longitudinale con quella di partenza non corretta"
                Case 344 : Return "Impossibile intersecate una superficie trasversale con quella di partenza"
                Case 345 : Return "Intersezione tra la superficie trasversale con quella di partenza non corretta"
                Case 346 : Return "Impossibile eseguire lo split delle curve trasversali con la superficie XZ"
                Case 347 : Return "Impossibile eseguire lo split della curva longitudinale con i punti di intersezione"
                Case 348 : Return "Impossibile creare il backup delle superfici del plantare"
                Case 349 : Return "Curve parallele non valide"
                Case 350 : Return "Raccordatura L1-L3 fallita"
                Case 351 : Return "Raccordatura L2-L4 fallita"
                Case 352 : Return "Impossibile eseguire la prima suddivisione della superficie laterale del plantare"
                Case 353 : Return "Impossibile eseguire la seconda suddivisione della superficie laterale del plantare"
                Case 354 : Return "Impossibile creare la superficie proiettata del cutout"
                Case 355 : Return "Impossibile creare la superficie finale del cutout"
                Case 356 : Return "Impossibile ricreare la superficie laterale del plantare"
                Case 357 : Return "Impossibile tagliare la curva di base del supporto volta"
                Case 358 : Return "Impossibile trovare la superficie estrusa del cutout"
                Case 359 : Return "Impossibile estrarre la curva di bordo della superficie estrusa"
                Case 360 : Return "Impossibile identificare la curva L1"
                Case 361 : Return "Impossibile identificare la curva L2"
                Case 362 : Return "Intersezione anomala tra le superfici estruse da L1 e L2"
                Case 363 : Return "Impossibile intersecare le superfici estruse da L1 e L2"
                Case 364 : Return "Impossibile tagliare la curva L1 con il centro di rotazione"
                Case 365 : Return "La profondità del cutout è eccessiva: la superficie di taglio interseca quella inferiroe del plantare"
                Case 366 : Return "Impossibile recuperare le curve L1 e L2"
                Case 367 : Return "Impossibile trovare la curva di bordo della superficie di trim del cutout"
                Case 368 : Return "Impossibile trovare la curva di bordo unita della superficie di trim del cutout"
                Case 369 : Return "Impossibile eseguire il primo Split della curva di cutout proiettata"
                Case 370 : Return "Impossibile eseguire il secondo Split della curva di cutout proiettata"
                Case 371 : Return "Impossibile eseguire il terzo Split della curva di cutout proiettata"
                Case 372 : Return "Impossibile ricavare il binario interno per lo Sweep"
                Case 373 : Return "Impossibile creare la superficie con lo Sweep"
                Case 374 : Return "La profondità del cutout è eccessiva: la superficie di taglio interseca quella inferiroe del plantare"
                Case 375 : Return "Impossibile trovare la curva di bordo della superficie di trim"
                Case 376 : Return "Impossibile trovare la curva di bordo unita della superficie di trim"
                Case 377 : Return "Impossibile trovare il centro della serie polare"
                Case 378 : Return "Impossibile trovare i punti di intersezione tra la curva di bordo del plantare e la superficie estrusa del ferro di cavallo"
                Case 379 : Return "Punti di intersezione tra la curva di bordo del plantare e la superficie estrusa del ferro di cavallo errati"
                Case 380 : Return "Impossibile ricavare il binario interno per lo Sweep"
                Case 381 : Return "Impossibile estendere la curva origine della serie polare"
                Case 382 : Return "Impossibile eseguire il primo Split della curva del ferro di cavallo proiettata"
                Case 383 : Return "Impossibile eseguire il secondo Split della curva del ferro di cavallo proiettata"
                Case 384 : Return "Impossibile eseguire il terzo Split della curva del ferro di cavallo proiettata"
                Case 385 : Return "Mappa di pressione non trovata"
                Case 386 : Return "Licenza scaduta"
                Case 387 : Return "Impossibile proiettare la mappa di pressione"
                Case 388 : Return "Impossibile deformare la superficie superiore del plantare in base alla tabella scelta"
                Case 389 : Return "Impossibile leggere i valori di pressione"
                Case 390 : Return "Importazione dei dati Insole Designer fallita"
                Case 391 : Return "Comando piani inclinati terminato"
                Case 392 : Return "Comando piani inclinati fallito"
                Case 393 : Return ""



            End Select

        Catch ex As Exception
            PromptError(ex.Message)
        End Try

        Return "Errore!"
    End Function


    ''' <summary>
    ''' Messaggi in inglese che eventualmente usano i campi impostati
    ''' </summary>
    ''' <param name="index">indice univoco del messaggio</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function GetMessageENG(ByVal index As Integer) As String
        Try
            Select Case index
                Case 0 : Return "Addiction " & AddictionTypeName(mAddictionType) & " must be the first one applied"
                Case 1 : Return "Addictions " & AddictionTypeName(mAddictionType) & " and " & AddictionTypeName(mAddictionType2) & " are incompatible"
                Case 2 : Return "Can not create the TCO profile side " & TranslateSide(mSide)
                Case 3 : Return "Can not read the row number of the curve " & TranslateSide(mSide)
                Case 4 : Return "Side " & TranslateSide(mSide) & " - split lataral surface by TCO profile ?"
                Case 5 : Return "Side " & TranslateSide(mSide) & ": curves already exist, continue?"
                Case 6 : Return "All objects of " & TranslateSide(mSide) & "side will be deleted, continue?"
                Case 7 : Return "Impossible to save the geometry side " & TranslateSide(mSide)
                Case 8 : Return "Impossible to save the template file of " & TranslateSide(mSide) & " side"
                Case 9 : Return "Impossible to replace the curve in position " & mCoordinate & " side " & TranslateSide(mSide) & " with straight curve"
                Case 10 : Return "Impossible to rebuild heel side " & TranslateSide(mSide)
                Case 11 : Return "File " & mText & " not found"
                Case 12 : Return "Impossible to project the curves of " & AddictionTypeName(mAddictionType)
                Case 13 : Return GetMessageITA(131) & " " & AddictionTypeName(mAddictionType)
                Case 14 : Return GetMessageITA(136) & " " & AddictionTypeName(mAddictionType)
                Case 15 : Return "Curves are external to X respect of upper surface of " & TranslateSide(mSide) & "side, position addiction properly"
                Case 16 : Return "Curves are external to Y respect of upper surface of " & TranslateSide(mSide) & "side, position addiction properly"
                Case 17 : Return "Curves are external of upper surface of side " & TranslateSide(mSide) & ", position addiction properly"
                Case 18 : Return "Please close form '" & mText & "' to save Insole Designer data"
                Case 19 : Return "Impossible to create the curve of " & TranslateSide(mSide) & " side"
                Case 20 : Return "Check the section curve in position X=" & mCoordinate & " that intersect the last in " & mNumber & "point(s)"
                Case 21 : Return "Impossible to fillet the curves because of distance greater of " & mCoordinate & "mm"
                Case 22 : Return "File format " & mText & " is wrong"
                Case 23 : Return "Header of file " & mText & " is wrong"
                Case 24 : Return "Please close form '" & mText & "' to continue"
                Case 25 : Return "Impossible recognise the base curves of addiction, check the file file '" & mText & "'"
                Case 26 : Return "Impossible recognise the upper curves of addiction, check the file file '" & mText & "'"
                Case 27 : Return ""
                Case 28 : Return "Impossible align pressure map side " & TranslateSide(mSide) & ": no foot mesh or insole found"
                Case 29 : Return "No Wedge found at " & TranslateSide(mSide) & " side" 
                Case 30 : Return "Wedge already exist side " & TranslateSide(mSide) & ", continue and overwrite?"
                Case 31 : Return ""
                Case 32 : Return ""
                Case 33 : Return ""



                    ''MESSAGGI SENZA PARAMETRI
                Case 100 : Return "correctly imported"
                Case 101 : Return "not imported"
                Case 102 : Return "Insole Designer data saving done"
                Case 103 : Return "Insole Designer data import done"
                Case 104 : Return "Undo completed"
                Case 105 : Return "Redo completed"
                Case 106 : Return "Addition done"
                Case 107 : Return "Footbed done"
                Case 108 : Return "Tamplate saved"
                Case 109 : Return "Last addition removed"
                Case 110 : Return "Heel reconstruction done"
                Case 111 : Return "Template loading completed"
                Case 112 : Return "Mirroring done"
                Case 113 : Return "Last Designer import done"
                Case 114 : Return "File loading completed"
                Case 115 : Return "Join surfaces done"
                Case 116 : Return "Cross-section curves done"
                Case 117 : Return "Bottom profile cutting done"
                Case 118 : Return "Insole guide curves completed"
                Case 119 : Return "Insole design completed"
                Case 120 : Return "Alignment completed"
                Case 121 : Return "TCO profile completed"
                Case 122 : Return "Ruler uploaded"
                Case 123 : Return "Automatic correction done"
                Case 124 : Return "Impossible to find Insole surfaces"
                Case 125 : Return "Impossible to place the unloading"
                Case 126 : Return "Impossible to import the unloading"
                Case 127 : Return "Unloading not found"
                Case 128 : Return "Insole upper surface not found"
                Case 129 : Return "One or more curves intersect the surface, please rearrange the addition"
                Case 130 : Return "Wrong addition type"
                Case 131 : Return "Unable to join the addition"
                Case 132 : Return "Impossible to project the metatarsal bar"
                Case 133 : Return "Impossible to create the addition surface"
                Case 134 : Return "Impossible to join the addition"
                Case 135 : Return "Impossible to create the joining surface for the addition"
                Case 136 : Return "Impossible to apply the"
                Case 137 : Return "Addition curves not found"
                Case 138 : Return "Curves below the surface, please rearrange the addition"
                Case 139 : Return "Impossible to project the addition"
                Case 140 : Return "Impossible to insert the ruler"
                Case 141 : Return "Impossible to insert TCO profile"
                Case 142 : Return "Impossible to create loft surface"
                Case 143 : Return "Impossible to create intersection surface"
                Case 144 : Return "Impossible to intersect surfaces"
                Case 145 : Return "Wrong loft surface"
                Case 146 : Return "Impossible to extend behind the loft surface"
                Case 147 : Return "Impossible to extend the loft surface in front"
                Case 148 : Return "Impossible to obtain a unique curve"
                Case 149 : Return "Impossible to create the curve with selected points"
                Case 150 : Return "Mesh or foot curves not found"
                Case 151 : Return "Insole curves not found"
                Case 152 : Return "Last not found"
                Case 153 : Return "Unable to generate the insole"
                Case 154 : Return "Unable to generate the upper surface of the insole"
                Case 155 : Return "Intersection between upper and bottom surface of the insole"
                Case 156 : Return "Wrong last split"
                Case 157 : Return "Profiled last not generated"
                Case 158 : Return "Unable to cut lateral surface with TCO profile"
                Case 159 : Return "Impossible to create the cutting figure"
                Case 160 : Return "Impossible to create the joining lateral surface"
                Case 161 : Return "Impossible to create the joining upper surface"
                Case 162 : Return "Impossible to create the lateral surface for profiled last"
                Case 163 : Return "Impossible to smooth insole upper surface"
                Case 164 : Return "Cross-section curves not found"
                Case 165 : Return "Unable to create guide curves"
                Case 166 : Return "Warning: ID unable to update upper profile curve"
                Case 167 : Return "Unable to join one or more foot curves. Curves will be deleted"
                Case 168 : Return "Foot curves not completed totally"
                Case 169 : Return "Unable to find vertex point on section curve"
                Case 170 : Return "Unable to find vertex point on section curve"
                Case 171 : Return "Impossible to create cutting surface"
                Case 172 : Return "Unable to find intersection between last and cutting surface"
                Case 173 : Return "Unable to join the splitting profile"
                Case 174 : Return "Bad positioned foot curves (XY plane) found and removed"
                Case 175 : Return "Surfaces not found, control canceled"
                Case 176 : Return "Impossible to split surfaces with TCO profile"
                Case 177 : Return "Last not found. Leaving"
                Case 178 : Return "Delete existing curves?"
                Case 179 : Return "Invalid: curve is shorter than the last"
                Case 180 : Return "Intersection between outer curve and the last is interrupted"
                Case 181 : Return "Intersection between inner curve and the last is interrupted"
                Case 182 : Return "Invalid insole boundary"
                Case 183 : Return "Impossible to cut inner curve at the heel"
                Case 184 : Return "Impossible to cut inner curve at the toe"
                Case 185 : Return "Impossible to outer curve at the heel"
                Case 186 : Return "Impossible to cut outer curve at the toe"
                Case 187 : Return "Impossible to create joining curves"
                Case 188 : Return "Foot mesh not found"
                Case 189 : Return "Intersection between foot and section planes failed"
                Case 190 : Return "Upper joining surface failed"
                Case 191 : Return "Surface joining failed"
                Case 192 : Return "Loading failed"
                Case 193 : Return "Last loading failed, incorrect surface number"
                Case 194 : Return ""
                Case 195 : Return ""
                Case 196 : Return ""
                Case 197 : Return ""
                Case 198 : Return ""
                Case 199 : Return ""
                Case 200 : Return ""
                Case 201 : Return ""
                Case 202 : Return ""
                Case 203 : Return ""
                Case 204 : Return ""
                Case 205 : Return ""
                Case 206 : Return ""
                Case 207 : Return ""
                Case 208 : Return ""
                Case 209 : Return ""
                Case 210 : Return ""
                Case 211 : Return ""
                Case 212 : Return ""
                Case 213 : Return ""
                Case 214 : Return ""
                Case 215 : Return ""
                Case 216 : Return ""
                Case 217 : Return ""
                Case 218 : Return ""
                Case 219 : Return ""
                Case 220 : Return ""
                Case 221 : Return ""
                Case 222 : Return ""
                Case 223 : Return ""
                Case 224 : Return ""
                Case 225 : Return ""
                Case 226 : Return ""
                Case 227 : Return ""
                Case 228 : Return ""
                Case 229 : Return ""
                Case 230 : Return ""
                Case 231 : Return ""
                Case 232 : Return ""
                Case 233 : Return ""
                Case 234 : Return ""
                Case 235 : Return ""
                Case 236 : Return ""
                Case 237 : Return ""
                Case 238 : Return ""
                Case 239 : Return ""
                Case 240 : Return ""
                Case 241 : Return ""
                Case 242 : Return ""
                Case 243 : Return ""
                Case 244 : Return ""
                Case 245 : Return ""
                Case 246 : Return ""
                Case 247 : Return ""
                Case 248 : Return ""
                Case 249 : Return ""
                Case 250 : Return ""
                Case 251 : Return ""
                Case 252 : Return ""
                Case 253 : Return ""
                Case 254 : Return ""
                Case 255 : Return ""
                Case 256 : Return ""
                Case 257 : Return ""
                Case 258 : Return ""
                Case 259 : Return ""
                Case 260 : Return ""
                Case 261 : Return ""
                Case 262 : Return ""
                Case 263 : Return ""
                Case 264 : Return ""
                Case 265 : Return ""
                Case 266 : Return ""
                Case 267 : Return ""
                Case 268 : Return ""
                Case 269 : Return ""
                Case 270 : Return ""
                Case 271 : Return ""
                Case 272 : Return ""
                Case 273 : Return ""
                Case 274 : Return ""
                Case 275 : Return ""
                Case 276 : Return ""
                Case 277 : Return ""
                Case 278 : Return ""
                Case 279 : Return ""
                Case 280 : Return ""
                Case 281 : Return ""
                Case 282 : Return ""
                Case 283 : Return ""
                Case 284 : Return ""
                Case 285 : Return ""
                Case 286 : Return ""
                Case 287 : Return ""
                Case 288 : Return ""
                Case 289 : Return ""
                Case 290 : Return ""
                Case 291 : Return ""
                Case 292 : Return ""
                Case 293 : Return ""
                Case 294 : Return ""
                Case 295 : Return ""
                Case 296 : Return ""
                Case 297 : Return ""
                Case 298 : Return ""
                Case 299 : Return ""
                Case 300 : Return ""
                Case 301 : Return ""
                Case 302 : Return ""
                Case 303 : Return ""
                Case 304 : Return ""
                Case 305 : Return ""
                Case 306 : Return ""
                Case 307 : Return ""
                Case 308 : Return ""
                Case 309 : Return ""
                Case 310 : Return ""
                Case 311 : Return ""
                Case 312 : Return ""
                Case 313 : Return ""
                Case 314 : Return ""
                Case 315 : Return ""
                Case 316 : Return ""
                Case 317 : Return ""
                Case 318 : Return ""
                Case 319 : Return ""
                Case 320 : Return ""
                Case 321 : Return ""
                Case 322 : Return ""
                Case 323 : Return ""
                Case 324 : Return ""
                Case 325 : Return ""
                Case 326 : Return ""
                Case 327 : Return ""
                Case 328 : Return ""
                Case 329 : Return ""
                Case 330 : Return ""
                Case 331 : Return ""
                Case 332 : Return ""
                Case 333 : Return ""
                Case 334 : Return ""
                Case 335 : Return ""
                Case 336 : Return ""
                Case 337 : Return ""
                Case 338 : Return ""
                Case 339 : Return ""
                Case 340 : Return ""
                Case 341 : Return ""
                Case 342 : Return ""
                Case 343 : Return ""
                Case 344 : Return ""
                Case 345 : Return ""
                Case 346 : Return ""
                Case 347 : Return ""
                Case 348 : Return ""
                Case 349 : Return ""
                Case 350 : Return ""
                Case 351 : Return ""
                Case 352 : Return ""
                Case 353 : Return ""
                Case 354 : Return ""
                Case 355 : Return ""
                Case 356 : Return ""
                Case 357 : Return ""
                Case 358 : Return ""
                Case 359 : Return ""
                Case 360 : Return ""
                Case 361 : Return ""
                Case 362 : Return ""
                Case 363 : Return ""
                Case 364 : Return ""
                Case 365 : Return ""
                Case 366 : Return ""
                Case 367 : Return ""
                Case 368 : Return ""
                Case 369 : Return ""
                Case 370 : Return ""
                Case 371 : Return ""
                Case 372 : Return ""
                Case 373 : Return ""
                Case 374 : Return ""
                Case 375 : Return ""
                Case 376 : Return ""
                Case 377 : Return ""
                Case 378 : Return ""
                Case 379 : Return ""
                Case 380 : Return ""
                Case 381 : Return ""
                Case 382 : Return ""
                Case 383 : Return ""
                Case 384 : Return ""
                Case 385 : Return "Pressure map not found"
                Case 386 : Return "License has expired"
                Case 387 : Return "Impossible project pressure map"
                Case 388 : Return "Impossible deform insole upper surface by choosen table"
                Case 389 : Return "Impossible read pressure values"
                Case 390 : Return "Insole Designer data import fail"
                Case 391 : Return "Wedge command completed"
                Case 392 : Return "Wedge command fail"
                Case 393 : Return ""


            End Select

        Catch ex As Exception
            PromptError(ex.Message)
        End Try

        Return "Error!"
    End Function


#End Region


#Region " DEBUG "


    '''' <summary>
    '''' Scrive nel prompt di Rhino e sulla finestra di debug il messaggio di errore con il metodo che lo ha intercettato l'eccezione
    '''' </summary>
    '''' <param name="errorMessage"></param>
    'Public Shared Sub PromptError(ByVal errorMessage As String)
    '    Dim callerString = ""
    '    Dim stackTrace = New StackTrace()
    '    If stackTrace.FrameCount > 1 Then
    '        Dim caller = stackTrace.GetFrame(1).GetMethod()
    '        If caller IsNot Nothing Then callerString = caller.DeclaringType.FullName & "." & caller.Name
    '    End If
    '    If String.IsNullOrEmpty(errorMessage) Then errorMessage = "Errore sconosciuto"
    '    Dim fullMessage As String = vbCrLf & callerString & " - " & errorMessage & vbCrLf
    '    RhUtil.RhinoApp.Print(fullMessage)
    '    Debug.Print(fullMessage)
    'End Sub

    Public Shared Sub PromptError(errorMessage As String)
        If String.IsNullOrEmpty(errorMessage) Then 
            Dim callerString = ""
            Dim stackTrace = New StackTrace()
            Dim caller = stackTrace.GetFrame(1).GetMethod()
            If caller IsNot Nothing Then callerString = caller.DeclaringType.FullName & "." & caller.Name
            errorMessage = callerString & " - " & "Errore sconosciuto"
        End If
         Dim fullMessage As String = vbCrLf & errorMessage & vbCrLf
        RhUtil.RhinoApp.Print(fullMessage)
        Debug.Print(fullMessage)
    End Sub

    ''' <summary>
    ''' Messaggio di notifica all'utente nel prompt di Rhino
    ''' </summary>
    ''' <param name="message"></param>
    Public Shared Sub PromptUserMessage(ByVal message As String)
        Dim title = IIf(My.Application.Info.Title.Contains(".bll"), My.Application.Info.Title.Replace(".bll", ""), My.Application.Info.Title)
        RhUtil.RhinoApp.Print(vbCrLf & vbCrLf & title & " - " & message & vbCrLf & vbCrLf)
    End Sub


#End Region


#End Region



End Class
