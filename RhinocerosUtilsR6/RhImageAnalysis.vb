Imports System.Drawing
Imports RMA.OpenNURBS
Imports System.Drawing.Drawing2D


'***************************************************************
'*** Classe per l'analisi di immagini tramite istogrammi HSV ***
'***************************************************************

Public Class RhImageAnalysis


#Region " Campi "

    Dim mImage As Color(,)
    Dim mIstogrammaTonalitÓ() As Integer
    Dim mIstogrammaLuminositÓ() As Integer
    Dim mIstogrammaSaturazione() As Integer
    Dim mIstogrammaTonalitÓPesata() As Single
    Dim mIstogrammaSaturazioneMediaSuTonalitÓ() As Single
    Dim mIstogrammaLuminositÓMediaSuTonalitÓ() As Single
    ''' <summary>
    ''' Contiene i valori minimi e massimi del range di interesse
    ''' Se (mMinRange minore mMaxRange) l'intervallo Ŕ [mMinRange, mMaxRange]
    ''' Se (mMinRange maggiore mMaxRange) l'intervallo Ŕ [mMinRange, 359] U [0, mMaxRange]
    ''' </summary>
    ''' <remarks></remarks>
    Dim mMinRange As Integer
    Dim mMaxRange As Integer


#End Region


#Region " Costruttori "

    Private Sub New()
    End Sub


    Public Sub New(ByVal image As Color(,))
        Me.mImage = image
    End Sub

#End Region


#Region " Property "

    ''' <summary>
    ''' Contiene l'istogramma della tonalitÓ
    ''' </summary>
    ''' <remarks></remarks>
    Public ReadOnly Property IstogrammaTonalitÓ() As Integer()
        Get
            Return mIstogrammaTonalitÓ
        End Get
    End Property


    ''' <summary>
    ''' Contiene l'istogramma della luminositÓ
    ''' </summary>
    ''' <remarks></remarks>    
    Public ReadOnly Property IstogrammaLuminositÓ() As Integer()
        Get
            Return mIstogrammaLuminositÓ
        End Get
    End Property


    ''' <summary>
    ''' Contiene l'istogramma della saturazione
    ''' </summary>
    ''' <remarks></remarks>    
    Public ReadOnly Property IstogrammaSaturazione() As Integer()
        Get
            Return mIstogrammaSaturazione
        End Get
    End Property


    ''' <summary>
    ''' Contiene l'istogramma della tonalitÓ media pesata sulla luminositÓ
    ''' Usato per la valutazione del range ottimale di tonalitÓ dalla funzione CalcolaRangeRistrettoTonalitÓ
    ''' </summary>
    ''' <remarks></remarks>    
    Public ReadOnly Property IstogrammaTonalitÓPesata() As Single()
        Get
            Return mIstogrammaTonalitÓPesata
        End Get
    End Property


    ''' <summary>
    ''' Contiene l'istogramma (per ogni valore di tonalitÓ) della saturazione media
    ''' Usato per la determinazione della soglia ottimale di saturazione
    ''' </summary>
    ''' <remarks></remarks>    
    Public ReadOnly Property IstogrammaSaturazioneMediaSuTonalitÓ() As Single()
        Get
            Return mIstogrammaSaturazioneMediaSuTonalitÓ
        End Get
    End Property


    ''' <summary>
    ''' Contiene l'istogramma (per ogni valore di tonalitÓ) della luminositÓ media
    ''' Usato per la determinazione della soglia ottimale di luminositÓ
    ''' </summary>
    ''' <remarks></remarks>    
    Public ReadOnly Property IstogrammaLuminositÓMediaSuTonalitÓ() As Single()
        Get
            Return mIstogrammaLuminositÓMediaSuTonalitÓ
        End Get
    End Property

    ''' <summary>
    ''' Ritorna la larghezza del range di interesse
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property LarghezzaRange() As Integer
        Get
            If mMinRange <= mMaxRange Then
                Return mMaxRange - mMinRange + 1
            Else
                Return mMaxRange + 360 - mMinRange + 1
            End If
        End Get
    End Property

    ''' <summary>
    ''' Ritorna il centro geometrico del range di interesse
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property CentroRange() As Integer
        Get
            If mMinRange <= mMaxRange Then
                Return (mMaxRange + mMinRange) \ 2
            Else
                Return ((mMinRange + mMaxRange + 360) \ 2) Mod 360
            End If
        End Get
    End Property

#End Region


#Region " Metodi "


    ''' <summary>
    ''' Setta il range di interesse a partire da un centro e una larghezza desiderata
    ''' </summary>
    ''' <param name="centroRange"></param>
    ''' <param name="larghezzaRange">180 Ŕ il valore ottimale</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function SetRange(ByVal centroRange As Integer, Optional ByVal larghezzaRange As Integer = 180) As Boolean
        mMinRange = (centroRange - larghezzaRange \ 2) Mod 360
        If mMinRange < 0 Then mMinRange += 360
        mMaxRange = (centroRange + larghezzaRange \ 2) Mod 360
        If mMaxRange < 0 Then mMaxRange += 360
        Return True
    End Function


    ''' <summary>
    ''' Calcola gli istogrammi HVS
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CalcolaIstogrammi() As Boolean

        'Inizializza i vettori
        If Me.mIstogrammaTonalitÓ Is Nothing Then ReDim Me.mIstogrammaTonalitÓ(359)
        If Me.mIstogrammaSaturazione Is Nothing Then ReDim Me.mIstogrammaSaturazione(99)
        If Me.mIstogrammaLuminositÓ Is Nothing Then ReDim Me.mIstogrammaLuminositÓ(99)
        For i As Integer = 0 To 359
            mIstogrammaTonalitÓ(i) = 0
        Next
        For i As Integer = 0 To 99
            mIstogrammaSaturazione(i) = 0
            mIstogrammaLuminositÓ(i) = 0
        Next

        'Costruisci gli istogrammi
        For i As Integer = 0 To mImage.GetUpperBound(0)
            For j As Integer = 0 To mImage.GetUpperBound(1)
                Dim colore As System.Drawing.Color = mImage(i, j)

                Dim hue As Integer = CInt(colore.GetHue())
                If hue < 1 Then hue = 1
                If hue > mIstogrammaTonalitÓ.Length + 1 Then hue = mIstogrammaTonalitÓ.Length
                mIstogrammaTonalitÓ(hue - 1) += 1

                Dim saturation As Integer = CInt(colore.GetSaturation() * 100)
                If saturation < 1 Then saturation = 1
                If saturation > mIstogrammaSaturazione.Length + 1 Then saturation = mIstogrammaSaturazione.Length
                mIstogrammaSaturazione(saturation - 1) += 1

                Dim brightness As Integer = CInt(colore.GetBrightness() * 100)
                If brightness < 1 Then brightness = 1
                If brightness > mIstogrammaLuminositÓ.Length + 1 Then brightness = mIstogrammaLuminositÓ.Length
                mIstogrammaLuminositÓ(brightness - 1) += 1
            Next
        Next
        Return True
    End Function


    ''' <summary>
    ''' Calcola gli istogrammi per il range di interesse
    ''' 
    ''' </summary>
    ''' <param name="applicaPesoCosinuisodale">Se true viene applicato un filtro passabanda pari a COS</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CalcolaIstogrammiRange(Optional ByVal applicaPesoCosinuisodale As Boolean = True) As Boolean

        'Inizializza i vettori
        If Me.mIstogrammaTonalitÓ Is Nothing Then ReDim Me.mIstogrammaTonalitÓ(359)
        If Me.mIstogrammaTonalitÓPesata Is Nothing Then ReDim Me.mIstogrammaTonalitÓPesata(359)
        If Me.mIstogrammaSaturazioneMediaSuTonalitÓ Is Nothing Then ReDim Me.mIstogrammaSaturazioneMediaSuTonalitÓ(359)
        If Me.mIstogrammaLuminositÓMediaSuTonalitÓ Is Nothing Then ReDim Me.mIstogrammaLuminositÓMediaSuTonalitÓ(359)
        For i As Integer = 0 To 359
            mIstogrammaTonalitÓ(i) = 0
            mIstogrammaTonalitÓPesata(i) = 0
            mIstogrammaSaturazioneMediaSuTonalitÓ(i) = 0
            mIstogrammaLuminositÓMediaSuTonalitÓ(i) = 0
        Next

        'Costruisci gli istogrammi
        For i As Integer = 0 To mImage.GetUpperBound(0)
            For j As Integer = 0 To mImage.GetUpperBound(1)
                Dim colore As System.Drawing.Color = mImage(i, j)
                Dim hue As Integer = CInt(colore.GetHue())
                If hue <> 0 Then    'Non considero i pixel grigi (hue = 0 Ŕ grigio, no rosso!)
                    If IsContained(hue, mMinRange, mMaxRange) Then
                        If hue > mIstogrammaTonalitÓPesata.Length + 1 Then hue = mIstogrammaTonalitÓPesata.Length

                        'Calcolo del peso
                        Dim peso As Double = 1.0
                        If applicaPesoCosinuisodale Then
                            If mMinRange < mMaxRange Then
                                peso = Math.Cos(Math.PI * (hue - CentroRange) / LarghezzaRange)
                            Else
                                If CentroRange > mMinRange Then
                                    If hue > mMinRange Then
                                        peso = Math.Cos(Math.PI * (hue - CentroRange) / LarghezzaRange)
                                    Else
                                        peso = Math.Cos(Math.PI * (hue + 360 - CentroRange) / LarghezzaRange)
                                    End If
                                Else
                                    If hue < mMaxRange Then
                                        peso = Math.Cos(Math.PI * (hue - CentroRange) / LarghezzaRange)
                                    Else
                                        peso = Math.Cos(Math.PI * (360 - hue + CentroRange) / LarghezzaRange)
                                    End If
                                End If
                            End If
                        End If

                        'Calcolo istogrammi
                        mIstogrammaTonalitÓ(hue - 1) += 1
                        mIstogrammaSaturazioneMediaSuTonalitÓ(hue - 1) += colore.GetSaturation()
                        mIstogrammaLuminositÓMediaSuTonalitÓ(hue - 1) += colore.GetBrightness()

                        'Calcolo istogramma della tonalitÓ pesata sulla luminositÓ
                        mIstogrammaTonalitÓPesata(hue - 1) += CSng(peso * colore.GetBrightness())
                    End If
                End If
            Next
        Next

        'Calcola la media dove necessario
        For i As Integer = 0 To 359
            If mIstogrammaTonalitÓ(i) <> 0 Then
                mIstogrammaLuminositÓMediaSuTonalitÓ(i) /= mIstogrammaTonalitÓ(i)
                mIstogrammaSaturazioneMediaSuTonalitÓ(i) /= mIstogrammaTonalitÓ(i)
                mIstogrammaTonalitÓPesata(i) /= mIstogrammaTonalitÓ(i)
            End If
        Next
        Return True
    End Function


    ''' <summary>
    ''' Verifica che un valore di tonalitÓ sia contenuto nella finestra di interesse
    ''' </summary>
    ''' <param name="value"></param>
    ''' <param name="minRange"></param>
    ''' <param name="maxRange"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function IsContained(ByVal value As Double, ByVal minRange As Double, ByVal maxRange As Double) As Boolean
        value = value Mod 360
        If value < 0 Then value += 360
        If minRange < maxRange Then
            Return value >= minRange And value <= maxRange
        Else
            Return value >= minRange Or value <= maxRange
        End If
    End Function


    ''' <summary>
    ''' Ritorna un range [inizio,fine] tale che la probabilitÓ cumulata
    ''' della tonalitÓ unitaria pesata sulla luminositÓ sia pari a probabilitÓCumulata
    ''' </summary>
    ''' <param name="inizio"></param>
    ''' <param name="fine"></param>
    ''' <param name="probabilitÓCumulata">0.75 Ŕ il valore ottimale</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CalcolaRangeRistrettoTonalitÓ(ByRef inizio As Integer, ByRef fine As Integer, Optional ByVal probabilitÓCumulata As Double = 0.75) As Boolean
        If mIstogrammaTonalitÓPesata Is Nothing Then Return False

        'La funzione integrale Ŕ definita da 0 a LarghezzaRange-1
        Dim integraleTonalitÓ(LarghezzaRange - 1) As Single

        'Il valoreMedioIntegrale rappresenta il valore medio pesato che divide l'area sottesa in due parti uguali
        Dim valoreMedioIntegrale As Integer
        Dim inizioIntegrale, fineIntegrale As Integer

        '*** Istogramma continuo da mMinRange a mMaxRange ***
        If mMinRange < mMaxRange Then

            ' Calcolo valoreMedioIntegrale
            integraleTonalitÓ(0) = mIstogrammaTonalitÓPesata(mMinRange)
            For i As Integer = mMinRange + 1 To mMaxRange
                integraleTonalitÓ(i - mMinRange) = integraleTonalitÓ(i - mMinRange - 1) + mIstogrammaTonalitÓPesata(i)
            Next
            For i As Integer = 0 To LarghezzaRange - 1
                If integraleTonalitÓ(i) > integraleTonalitÓ(LarghezzaRange - 1) / 2 Then
                    valoreMedioIntegrale = i
                    Exit For
                End If
            Next

            'Calcolo valori di inizio e fine integrazione tali che l'area sottesa sia pari a probabilitÓCumulata
            Dim maxDistanza As Integer = valoreMedioIntegrale
            If LarghezzaRange - valoreMedioIntegrale - 1 > maxDistanza Then maxDistanza = LarghezzaRange - valoreMedioIntegrale - 1
            For i As Integer = 1 To maxDistanza
                inizioIntegrale = valoreMedioIntegrale - i
                If inizioIntegrale < 0 Then inizioIntegrale = 0
                fineIntegrale = valoreMedioIntegrale + i
                If fineIntegrale > LarghezzaRange - 1 Then fineIntegrale = LarghezzaRange - 1
                If integraleTonalitÓ(fineIntegrale) - integraleTonalitÓ(inizioIntegrale) > probabilitÓCumulata * integraleTonalitÓ(LarghezzaRange - 1) Then
                    inizio = inizioIntegrale + mMinRange
                    fine = fineIntegrale + mMinRange
                    Exit For
                End If
            Next

            '*** Istogramma discontinuo da mMaxRange a 359 e da 0 a mMinRange ***
        Else

            ' Calcolo valoreMedioIntegrale
            integraleTonalitÓ(0) = mIstogrammaTonalitÓPesata(mMinRange)
            For i As Integer = mMinRange + 1 To 359
                integraleTonalitÓ(i - mMinRange) = integraleTonalitÓ(i - mMinRange - 1) + mIstogrammaTonalitÓPesata(i)
            Next
            For i As Integer = 0 To mMaxRange
                integraleTonalitÓ(360 - mMinRange + i) = integraleTonalitÓ(360 - mMinRange + i - 1) + mIstogrammaTonalitÓPesata(i)
            Next
            For i As Integer = 0 To LarghezzaRange - 1
                If integraleTonalitÓ(i) > integraleTonalitÓ(LarghezzaRange - 1) / 2 Then
                    valoreMedioIntegrale = i
                    Exit For
                End If
            Next

            'Calcolo valori di inizio e fine integrazione tali che l'area sottesa sia pari a probabilitÓCumulata
            Dim maxDistanza As Integer = valoreMedioIntegrale
            If LarghezzaRange - valoreMedioIntegrale - 1 > maxDistanza Then maxDistanza = LarghezzaRange - valoreMedioIntegrale - 1
            For i As Integer = 1 To maxDistanza
                inizioIntegrale = valoreMedioIntegrale - i
                If inizioIntegrale < 0 Then inizioIntegrale = 0
                fineIntegrale = valoreMedioIntegrale + i
                If fineIntegrale > LarghezzaRange - 1 Then fineIntegrale = LarghezzaRange - 1
                If integraleTonalitÓ(fineIntegrale) - integraleTonalitÓ(inizioIntegrale) > probabilitÓCumulata * integraleTonalitÓ(LarghezzaRange - 1) Then
                    inizio = (inizioIntegrale + mMinRange) Mod 360
                    fine = (fineIntegrale + mMinRange) Mod 360
                    Exit For
                End If
            Next
        End If
        Return True
    End Function


    ''' <summary>
    ''' Calcola i valori medi di LuminositÓ e Saturazione in un certo range
    ''' </summary>
    ''' <param name="inizio"></param>
    ''' <param name="fine"></param>
    ''' <param name="luminositÓMedia"></param>
    ''' <param name="saturazioneMedia"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CalcolaMediaIstogrammiVS(ByVal inizio As Integer, ByVal fine As Integer, ByRef luminositÓMedia As Double, ByRef saturazioneMedia As Double) As Boolean
        If Me.mIstogrammaLuminositÓMediaSuTonalitÓ Is Nothing Then Return False
        If Me.mIstogrammaSaturazioneMediaSuTonalitÓ Is Nothing Then Return False

        '*** Istogramma continuo da mMinRange a mMaxRange ***
        If inizio < fine Then
            Dim integraleLuminositÓ(fine - inizio) As Single
            Dim integraleSaturazione(fine - inizio) As Single
            integraleLuminositÓ(0) = mIstogrammaLuminositÓMediaSuTonalitÓ(inizio)
            integraleSaturazione(0) = mIstogrammaSaturazioneMediaSuTonalitÓ(inizio)
            For i As Integer = inizio + 1 To fine
                integraleLuminositÓ(i - inizio) = integraleLuminositÓ(i - inizio - 1) + mIstogrammaLuminositÓMediaSuTonalitÓ(i)
                integraleSaturazione(i - inizio) = integraleSaturazione(i - inizio - 1) + mIstogrammaSaturazioneMediaSuTonalitÓ(i)
            Next
            luminositÓMedia = integraleLuminositÓ(fine - inizio) / (fine - inizio + 1)
            saturazioneMedia = integraleSaturazione(fine - inizio) / (fine - inizio + 1)

            '*** Istogramma discontinuo da mMaxRange a 359 e da 0 a mMinRange ***
        Else
            Dim integraleLuminositÓ(360 - inizio + fine) As Single
            Dim integraleSaturazione(360 - inizio + fine) As Single
            integraleLuminositÓ(0) = mIstogrammaLuminositÓMediaSuTonalitÓ(inizio)
            integraleSaturazione(0) = mIstogrammaSaturazioneMediaSuTonalitÓ(inizio)
            For i As Integer = inizio + 1 To 359
                integraleLuminositÓ(i - inizio) = integraleLuminositÓ(i - inizio - 1) + mIstogrammaLuminositÓMediaSuTonalitÓ(i)
                integraleSaturazione(i - inizio) = integraleSaturazione(i - inizio - 1) + mIstogrammaSaturazioneMediaSuTonalitÓ(i)
            Next
            For i As Integer = 0 To fine
                integraleLuminositÓ(360 - inizio + i) = integraleLuminositÓ(360 - inizio + i - 1) + mIstogrammaLuminositÓMediaSuTonalitÓ(i)
                integraleSaturazione(360 - inizio + i) = integraleSaturazione(360 - inizio + i - 1) + mIstogrammaSaturazioneMediaSuTonalitÓ(i)
            Next
            luminositÓMedia = integraleLuminositÓ(360 - inizio + fine) / (360 - inizio + fine + 1)
            saturazioneMedia = integraleSaturazione(360 - inizio + fine) / (360 - inizio + fine + 1)
        End If
        Return True
    End Function


    ''' <summary>
    ''' Calcola il massimo di un array
    ''' </summary>
    ''' <param name="array"></param>
    ''' <param name="maxIndex"></param>
    ''' <param name="maxValue"></param>
    ''' <param name="ignoreFirstElement"></param>
    ''' <remarks></remarks>
    Public Shared Sub Max(ByVal array() As Integer, ByRef maxIndex As Integer, Optional ByRef maxValue As Integer = 0, Optional ByVal ignoreFirstElement As Boolean = True)
        If array Is Nothing Then Exit Sub
        Dim firstIndex As Integer = 0
        If ignoreFirstElement Then firstIndex = 1
        maxIndex = -1
        maxValue = Integer.MinValue
        For i As Integer = firstIndex To array.GetUpperBound(0)
            If array(i) > maxValue Then
                maxValue = array(i)
                maxIndex = i
            End If
        Next
    End Sub


    ''' <summary>
    ''' Restituisce una bitmap a partire da quella passata in ingresso, ma con i bordi netti.
    ''' </summary>
    ''' <param name="originalBitmap">Bitmap originale con i bordi non netti</param>
    ''' <param name="tollerance">La tolleranza deve essere un valore compreso tra 1 e 254</param>
    ''' <returns>Bitmap creata a partire da quella originale ma con i bordi netti</returns>
    Public Shared Function CleanDirtyWhite(ByVal originalBitmap As Bitmap, Optional ByVal tollerance As Int32 = 125) As Bitmap

        If tollerance > 254 Or tollerance < 1 Then tollerance = 125

        Dim bitmapWidth As Int32 = originalBitmap.Width
        Dim bitmapHeight As Int32 = originalBitmap.Height
        Dim resultBitmap As Bitmap = DirectCast(originalBitmap.Clone(), Bitmap)
        'Il colore del bordo in RGB Ŕ identificabile
        ' perchŔ ha contemporaneamentetutte le componenti R,G,B con un valore alto.
        ' Ci˛ Ŕ dovuto dal fatto che la linea di equazione R=G=B corrisponde ai livelli di grigio. Ovvero quando R=G=B=0 il colore Ŕ nero,
        ' quando R=G=B=255 il colore Ŕ bianco, con tutti i valori intermedi R=G=B si hanno sfumature di grigio.
        Dim originalColor As Color
        For i As Int32 = 0 To originalBitmap.Width - 1
            For j As Int32 = 0 To originalBitmap.Height - 1
                originalColor = originalBitmap.GetPixel(i, j)
                If originalColor.R > tollerance And originalColor.G > tollerance And originalColor.B > tollerance Then
                    resultBitmap.SetPixel(i, j, Color.White)
                End If
            Next
        Next

        Return resultBitmap
    End Function


    ''' <summary>
    ''' Calcola il colore di una curva isobara dopo aver specificato l'indice della curva ed il numero di livelli da calcolare
    ''' </summary>
    ''' <param name="index"></param>
    ''' <param name="levels"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function LevelColorOfIsobar(ByVal index As Integer, ByVal levels As Integer) As Color
        Dim hue As Double = ((240 * index) / (levels - 1)) * Math.PI / 180
        Dim resOnColor As New OnColor
        resOnColor.SetHSV(hue, 1, 1)
        Return Color.FromArgb(resOnColor.Red, resOnColor.Green, resOnColor.Blue)
    End Function


    ''' <summary>
    ''' Dato un valore della pressione calcola il colore per rappresentarlo
    ''' </summary>
    ''' <param name="pressureVal">Valore da rappresentare</param>
    ''' <param name="minVal">Valore minimo della pressione</param>
    ''' <param name="maxVal">Valore massimo della pressione</param>
    ''' <returns></returns>
    Public Shared Function ConvertPressureToColor(ByVal pressureVal As Double, ByVal minVal As Double, ByVal maxVal As Double) As Color
        'Posso considerare H=pressureVal, S=V=1 e fare conversione HSV_to_RGB -> http://www.alvyray.com/Papers/hsv2rgb.htm
        Dim resOnColor As OnColor = New OnColor()

        '--------------FROM Rhino SDK Documentation--------------------
        'Hue() returns an angle in the range 0 to 2*pi
        '0 red,     'pi/3 = yellow,     '2*pi/3 = green,    'pi cyan,   '4 * pi / 3 = blue,                 
        '5 * pi / 3 = magenta,  -> NON USATO    
        '2 * pi = red)          -> NON USATO
        '--------------------------------------------------------------

        If pressureVal = 0 Then
            'Serve per il bianco e il rumore
            resOnColor.SetHSV(1, 0, 1)
        Else
            Dim hue As Double = 4 / 3 * Math.PI - ((pressureVal / (maxVal + 1)) * 4 / 3 * Math.PI)
            resOnColor.SetHSV(hue, 1, 1)
        End If

        Dim resColor As Color = Color.FromArgb(resOnColor.Red, resOnColor.Green, resOnColor.Blue)
        Return resColor
    End Function


    ''' <summary>
    ''' Restituisce una bitmap interpolata con l'algoritmo "HighQualityBicubic"
    ''' </summary>
    ''' <param name="miniBitmap"></param>
    ''' <param name="scaledBitmapWidth"></param>
    ''' <param name="scaledBitmapHeight"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function BitmapInterpolation(ByVal miniBitmap As Bitmap, ByVal scaledBitmapWidth As Int32, ByVal scaledBitmapHeight As Int32) As Bitmap
        ''Interpolazione per ottenere una bitmap ingrandita (le dimensioni delle bitmap scalata sono fisse)
        Dim targetBitmap As Bitmap = Nothing
        targetBitmap = New Bitmap(miniBitmap, scaledBitmapHeight, scaledBitmapWidth)
        Dim targetGraphic As Graphics = Graphics.FromImage(targetBitmap)
        targetGraphic.InterpolationMode = InterpolationMode.HighQualityBicubic
        targetGraphic.SmoothingMode = SmoothingMode.None
        targetGraphic.DrawImage(miniBitmap, New Rectangle(0, 0, scaledBitmapHeight, scaledBitmapWidth), _
                                0, 0, miniBitmap.Width, miniBitmap.Height, GraphicsUnit.Pixel)
        targetGraphic.Dispose()
        targetGraphic = Nothing
        Return targetBitmap
    End Function



#End Region


End Class
