Imports RMA.OpenNURBS


''' <summary>
''' Classe per la gestione del Matching di elementi geometrici
''' </summary>
''' <remarks></remarks>
Public Class RhMatching

    'Calcola la trasformazione che mappa curvaMobile sopra la curvaFissa con il metodo del matching
    Public Shared Function MatchingCurve(ByVal curvaFissa As IOnCurve, ByVal curvaMobile As OnCurve, Optional ByVal numeroCampioni As Integer = 40, Optional ByVal tolleranza As Double = 0.001, Optional ByRef numeroIterazioni As Integer = 0, Optional ByRef deviazioneMax As Double = 0.0) As OnXform
        'Suddividi curva mobile
        Dim puntiMobili As New ArrayOn3dPoint

        Dim dominio As OnInterval = curvaMobile.Domain
        For i As Integer = 0 To numeroCampioni - 1
            Dim tMobile As Double = dominio.m_t(0) + i * (dominio.m_t(1) - dominio.m_t(0)) / (numeroCampioni - 1)
            Dim puntoMobile As On3dPoint = curvaMobile.PointAt(tMobile)
            puntiMobili.Append(puntoMobile)
        Next

        Dim xFormTotale As New OnXform
        xFormTotale.Identity()
        deviazioneMax = 0.0
        Dim deviazioneMaxPreviousStep As Double = 0.0

        'Ciclo trasformazioni
        numeroIterazioni = 0
        Do
            Dim puntiFissi As New ArrayOn3dPoint
            For i As Integer = 0 To numeroCampioni - 1
                Dim tFisso As Double
                curvaFissa.GetClosestPoint(puntiMobili(i), tFisso)
                puntiFissi.Append(curvaFissa.PointAt(tFisso))
            Next

            deviazioneMaxPreviousStep = deviazioneMax
            deviazioneMax = 0.0
            Dim xform As OnXform = CalcolaStepTrasformazione(puntiFissi, puntiMobili, deviazioneMax)
            xFormTotale = xform * xFormTotale

            puntiFissi.Dispose()
            numeroIterazioni += 1
        Loop Until Math.Abs(deviazioneMaxPreviousStep - deviazioneMax) < tolleranza

        puntiMobili.Dispose()
        curvaMobile.Dispose()
        Return xFormTotale
    End Function


    Public Shared Function CalcolaStepTrasformazione(ByVal puntiFissi As IArrayOn3dPoint, ByRef puntiMobili As ArrayOn3dPoint, Optional ByRef deviazioneMax As Double = 0.0) As OnXform
        'Verifiche iniziali
        If puntiFissi Is Nothing Then Return Nothing
        If puntiMobili Is Nothing Then Return Nothing
        If puntiFissi.Count < 3 Then Return Nothing
        If puntiMobili.Count < 3 Then Return Nothing
        If puntiFissi.Count <> puntiMobili.Count Then Return Nothing

        'Scrivi il sistema
        Dim numeroPunti As Integer = puntiMobili.Count
        Dim A As New RhMatrice(3 * numeroPunti - 1, 5)
        Dim b As New RhMatrice(3 * numeroPunti - 1, 0)
        For i As Integer = 0 To numeroPunti - 1
            A(3 * i, 0) = 1
            A(3 * i, 1) = 0
            A(3 * i, 2) = 0
            A(3 * i, 3) = 0
            A(3 * i, 4) = puntiMobili(i).z
            A(3 * i, 5) = -puntiMobili(i).y

            A(3 * i + 1, 0) = 0
            A(3 * i + 1, 1) = 1
            A(3 * i + 1, 2) = 0
            A(3 * i + 1, 3) = -puntiMobili(i).z
            A(3 * i + 1, 4) = 0
            A(3 * i + 1, 5) = puntiMobili(i).x

            A(3 * i + 2, 0) = 0
            A(3 * i + 2, 1) = 0
            A(3 * i + 2, 2) = 1
            A(3 * i + 2, 3) = puntiMobili(i).y
            A(3 * i + 2, 4) = -puntiMobili(i).x
            A(3 * i + 2, 5) = 0

            b(3 * i, 0) = puntiFissi(i).x - puntiMobili(i).x
            b(3 * i + 1, 0) = puntiFissi(i).y - puntiMobili(i).y
            b(3 * i + 2, 0) = puntiFissi(i).z - puntiMobili(i).z
        Next

        'Risolvi
        Dim inversaA_MP As RhMatrice = A.InversaMoorePenrose
        Dim soluzione As RhMatrice = RhMatrice.Prodotto(inversaA_MP, b)

        'Trasforma
        Dim vettoreRotazione As New On3dVector(soluzione(3, 0), soluzione(4, 0), soluzione(5, 0))
        Dim angolo As Double = vettoreRotazione.Length
        vettoreRotazione.Unitize()
        Dim xFormRotazione As New OnXform
        xFormRotazione.Rotation(angolo, vettoreRotazione, OnPlane.World_xy.origin)

        Dim traslazione As New On3dVector(soluzione(0, 0), soluzione(1, 0), soluzione(2, 0))
        Dim xFormTraslazione As New OnXform
        xFormTraslazione.Translation(traslazione)

        Dim res As OnXform = xFormTraslazione * xFormRotazione

        'Calcola deviazione massima
        deviazioneMax = 0.0
        For i As Integer = 0 To puntiMobili.Count - 1
            puntiMobili(i).Transform(res)
            If puntiFissi(i).DistanceTo(puntiMobili(i)) > deviazioneMax Then
                deviazioneMax = puntiFissi(i).DistanceTo(puntiMobili(i))
            End If
        Next

        Return res
    End Function

End Class
