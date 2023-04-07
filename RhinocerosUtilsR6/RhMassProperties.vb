Imports RMA.OpenNURBS


''' <summary>
''' Classe derivata da OnMassProperty che implememta funzioni aggiutive su momenti e direzioni principali di inerzia
''' </summary>
''' <remarks></remarks>
Public Class RhMassProperties
    Inherits OnMassProperties

    ''' <summary>
    ''' Funzione per il calcolo della funzione caratteristica le cui radici esprimono i momenti principali di inerzia
    ''' </summary>
    ''' <param name="x"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function FunzioneCaratteristica(ByVal x As Double) As Double
        Dim a As Double = m_ccs_xx - x
        Dim b As Double = m_ccs_xy
        Dim c As Double = m_ccs_zx
        Dim e As Double = m_ccs_yy - x
        Dim f As Double = m_ccs_yz
        Dim i As Double = m_ccs_zz - x
        Return a * e * i + b * f * c + c * b * f - e * c ^ 2 - a * f ^ 2 - i * b ^ 2
    End Function


    ''' <summary>
    ''' Calcola i momenti principali di inerzia
    ''' </summary>
    ''' <param name="momenti"></param>
    ''' <param name="tolleranza"></param>
    ''' <param name="numeroMomenti">Numero di momenti desiderati ordinati in senso crescente</param>
    ''' <returns></returns>
    ''' <remarks>Ricerca delle radici non sempre affidabile</remarks>
    Public Function MomentiPrincipaliInerzia(ByRef momenti As Arraydouble, Optional ByVal tolleranza As Double = 1, Optional ByVal numeroMomenti As Integer = 2) As Boolean
        If Not m_bValidSecondMoments Or Not m_bValidProductMoments Then Return False
        Const NUMERO_MAX_ITERAZIONI As Integer = 10000000

        If Not momenti Is Nothing Then momenti.Dispose()
        momenti = New Arraydouble()
        Dim ultimoPositivo As Boolean = FunzioneCaratteristica(0) > 0
        For k As Integer = 1 To NUMERO_MAX_ITERAZIONI
            Dim x As Double = 2 * k * tolleranza
            If (FunzioneCaratteristica(x) > 0) <> ultimoPositivo Then
                ultimoPositivo = Not ultimoPositivo
                Dim x1 As Double = x - 2 * tolleranza
                Dim y1 As Double = FunzioneCaratteristica(x1)
                Dim x2 As Double = x
                Dim y2 As Double = FunzioneCaratteristica(x2)
                momenti.Append(x1 + y1 * (x2 - x1) / (y1 - y2))
                If momenti.Count = numeroMomenti Then Exit For
            End If
        Next
        If momenti.Count < numeroMomenti Then Return False
        Return True
    End Function


    ''' <summary>
    ''' Calcolo delle direzioni principali di inerzia
    ''' </summary>
    ''' <param name="direzioniPrincipali"></param>
    ''' <param name="tolleranza"></param>
    ''' <param name="numeroMomenti">Numero di momenti desiderati ordinati in senso crescente</param>
    ''' <returns></returns>
    ''' <remarks>Ricerca delle radici non sempre affidabile</remarks>
    Public Function DirezioniPrincipaliInerzia(ByRef direzioniPrincipali As ArrayOn3dVector, Optional ByVal tolleranza As Double = 1, Optional ByVal numeroMomenti As Integer = 2) As Boolean
        Dim autovalori As Arraydouble = Nothing
        If Not MomentiPrincipaliInerzia(autovalori, tolleranza, numeroMomenti) Then Return False
        If Not direzioniPrincipali Is Nothing Then direzioniPrincipali.Dispose()
        direzioniPrincipali = New ArrayOn3dVector
        For k As Integer = 0 To autovalori.Count - 1
            direzioniPrincipali.Append(DirezioneMomento(autovalori(k)))
        Next
        Return True
    End Function


    ''' <summary>
    ''' Ritorna la matrice di inerzia in forma di OnXForm
    ''' </summary>
    ''' <param name="matrice"></param>
    ''' <returns></returns>
    ''' <remarks>[3,3]=1, il resto nullo</remarks>
    Public Function MatriceDiInerzia(ByRef matrice As OnXform) As Boolean
        If Not m_bValidSecondMoments Then Return False
        If Not matrice Is Nothing Then matrice.Dispose()

        Dim vettore1 As New On3dVector(m_ccs_xx, m_ccs_xy, m_ccs_zx)
        Dim vettore2 As New On3dVector(m_ccs_xy, m_ccs_yy, m_ccs_yz)
        Dim vettore3 As New On3dVector(m_ccs_zx, m_ccs_yz, m_ccs_zz)
        matrice = New OnXform(New On3dPoint(0, 0, 0), vettore1, vettore2, vettore3)
        If matrice.IsValid Then
            Return True
        Else
            matrice.Dispose()
            Return False
        End If
    End Function


    ''' <summary>
    ''' Calcolo del momento dominante utilizzando il metodo delle potenze
    ''' </summary>
    ''' <param name="momentoMassimo"></param>
    ''' <param name="direzioneMassima"></param>
    ''' <param name="tolleranza"></param>
    ''' <param name="passiMax"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function MomentoInerziaMassimo(ByRef momentoMassimo As Double, ByRef direzioneMassima As On3dVector, Optional ByVal tolleranza As Double = 0.01, Optional ByVal passiMax As Integer = 100) As Boolean
        Dim matrice As New OnXform
        If Not MatriceDiInerzia(matrice) Then Return False

        Dim vettoreInnesco As New On3dVector(1, 1, 1)
        If direzioneMassima Is Nothing Then direzioneMassima = New On3dVector
        direzioneMassima = vettoreInnesco / vettoreInnesco.Length
        Dim yTmp As New On3dVector
        Dim lambda As Double = 0
        momentoMassimo = 1
        Dim passi As Integer = 0
        Do
            yTmp = direzioneMassima
            lambda = momentoMassimo
            direzioneMassima = matrice * direzioneMassima
            momentoMassimo = yTmp * direzioneMassima / (yTmp * yTmp)
            direzioneMassima = direzioneMassima / direzioneMassima.Length
            passi += 1
        Loop While Math.Abs(momentoMassimo - lambda) > tolleranza And passi < passiMax
        matrice.Dispose()
        direzioneMassima *= -1
        If passi = passiMax And Math.Abs(momentoMassimo - lambda) > tolleranza Then Return False
        Return True
    End Function


    ''' <summary>
    ''' Calcolo utilizzando il metodo delle potenze inverse
    ''' </summary>
    ''' <param name="momentoMinimo"></param>
    ''' <param name="direzioneMinima"></param>
    ''' <param name="tolleranza"></param>
    ''' <param name="passiMax"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function MomentoInerziaMinimo(ByRef momentoMinimo As Double, ByRef direzioneMinima As On3dVector, Optional ByVal tolleranza As Double = 0.01, Optional ByVal passiMax As Integer = 100) As Boolean
        Dim matrice As New OnXform
        If Not MatriceDiInerzia(matrice) Then Return False
        Dim matriceInversa As OnXform = matrice.Inverse
        matrice.Dispose()

        Dim vettoreInnesco As New On3dVector(1, 1, 1)
        Dim y As On3dVector = vettoreInnesco / vettoreInnesco.Length
        Dim yTmp As New On3dVector
        Dim mup As Double = 0
        Dim mu As Double = 1
        Dim passi As Integer = 0
        Do
            mup = mu
            yTmp = y
            y = matriceInversa * y
            mu = yTmp * y / (yTmp * yTmp)
            y = y / y.Length
            passi += 1
        Loop While Math.Abs(1 / mu - 1 / mup) > tolleranza And passi < passiMax
        matriceInversa.Dispose()
        momentoMinimo = 1 / mu
        If passi = passiMax And Math.Abs(1 / mu - 1 / mup) > tolleranza Then Return False

        ' Calcola l'autovettore
        direzioneMinima = DirezioneMomento(momentoMinimo)
        Return True
    End Function


    ''' <summary>
    ''' Calcola la direzione principale dato il corrispondente momento di inerzia
    ''' </summary>
    ''' <param name="autovalore"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function DirezioneMomento(ByVal autovalore As Double) As On3dVector
        If autovalore = 0.0 Then Return Nothing
        Dim a As Double = m_ccs_xx - autovalore
        Dim b As Double = m_ccs_xy
        Dim c As Double = m_ccs_zx
        Dim e As Double = m_ccs_yy - autovalore
        Dim f As Double = m_ccs_yz
        Dim i As Double = m_ccs_zz - autovalore

        Dim direzione As New On3dVector
        If c = 0 And f = 0 Then 'Caso in cui z è nullo
            direzione.z = 0
            direzione.x = 1
            direzione.y = -a / b
        Else
            direzione.z = 1
            direzione.y = (a * f - b * c) * direzione.z / (b * b - a * e)
            direzione.x = -(b * direzione.y + c * direzione.z) / a
        End If
        direzione.Unitize()
        Return direzione
    End Function


    ''' <summary>
    ''' Piano formato dalle direzioni principali di inerzia
    ''' </summary>
    ''' <param name="pianoPrincipale"></param>
    ''' <param name="tolleranza">Tolleranza sul calcolo del momento id inerzia</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function PianoPrincipaleInerzia(ByRef pianoPrincipale As OnPlane, Optional ByVal tolleranza As Double = 0.1, Optional ByVal usaMomentiMinimoMassimo As Boolean = True) As Boolean
        Dim direzioniPrincipali As New ArrayOn3dVector
        If usaMomentiMinimoMassimo Then
            Dim momentoMin, momentoMax As Double
            Dim direzioneMin As New On3dVector
            Dim direzioneMax As New On3dVector
            If MomentoInerziaMinimo(momentoMin, direzioneMin, tolleranza) And Not MomentoInerziaMassimo(momentoMax, direzioneMax, tolleranza) Then
                direzioniPrincipali.Dispose()
                Return False
            End If
            direzioniPrincipali.Append(direzioneMin)
            direzioniPrincipali.Append(direzioneMax)
        Else
            If Not DirezioniPrincipaliInerzia(direzioniPrincipali, tolleranza, 2) Then
                direzioniPrincipali.Dispose()
                Return False
            End If
        End If
        If Not pianoPrincipale Is Nothing Then pianoPrincipale.Dispose()
        pianoPrincipale = New OnPlane(Centroid, direzioniPrincipali(0), direzioniPrincipali(1))
        Dim sinAngolo As Double = OnUtil.ON_CrossProduct(pianoPrincipale.yaxis, direzioniPrincipali(1)).Length
        Dim angolo As Double = Math.Asin(sinAngolo) / 2
        pianoPrincipale.Rotate(angolo, pianoPrincipale.Normal, pianoPrincipale.origin)
        Return pianoPrincipale.IsValid
    End Function

End Class
