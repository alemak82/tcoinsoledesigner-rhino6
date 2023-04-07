Imports System.Math
Imports RMA.Rhino
Imports RMA.OpenNURBS
Imports System.Reflection


'**********************************************************
'*** Classe per la gestione di funzionalità geometriche ***
'**********************************************************

Public Class RhGeometry


    ''' <summary>
    ''' ESTRAZIONE DELLA CURVA DI BORDO DI UNA SUPERFICIE
    ''' </summary>
    ''' <param name="surfaceObjRef"></param>
    ''' <returns>UNICA CURVA UNITA O LA PIU' LUNGA(che dovrebbe essere anche la più esterna?!)</returns>
    ''' <remarks>http://wiki.mcneel.com/developer/sdksamples/dupborder</remarks>
    Public Shared Function ExtractSurfaceBorder(ByVal surfaceObjRef As MRhinoObjRef) As IOnCurve
        Dim edgeCurves As New List(Of IOnCurve)
        If surfaceObjRef Is Nothing Then Return Nothing
        Dim brep As IOnBrep = surfaceObjRef.Brep()
        If brep Is Nothing Then Return Nothing
        Dim edge_count As Integer = brep.m_E.Count()
        For i As Integer = 0 To edge_count - 1
            Dim edge As IOnBrepEdge = brep.m_E(i)
            If (edge.m_ti.Count() = 1 And edge.m_c3i >= 0) Then
                Dim curve As OnCurve = edge.DuplicateCurve()
                If (brep.m_T(edge.m_ti(0)).m_bRev3d) Then curve.Reverse()
                If (brep.m_T(edge.m_ti(0)).Face().m_bRev) Then curve.Reverse()
                edgeCurves.Add(curve)
            End If
        Next
        If edgeCurves.Count = 1 Then Return edgeCurves.Item(0)
        'Unisco eventuali curve multiple
        Dim tol As Double = RhUtil.RhinoApp.ActiveDoc.AbsoluteTolerance()
        Dim mergeCurves() As OnCurve = {}
        If (RhUtil.RhinoMergeCurves(edgeCurves.ToArray(), mergeCurves, tol) AndAlso mergeCurves.Length = 1) Then Return mergeCurves(0)
        If mergeCurves.Length = 0 Then Return Nothing
        'Se le curve non possono essere unite allora sono bordi distinti - al momento prendo la curva più lunga che solitamente è quella esterna
        Dim result As OnCurve = Nothing
        Dim maxLenght As Double = Double.MinValue
        For Each curve As OnCurve In mergeCurves
            Dim lenght As Double
            curve.GetLength(lenght)
            If lenght > maxLenght Then
                maxLenght = lenght
                result = curve
            End If
        Next
        Return result
    End Function





    ''' <summary>
    ''' Restituisce una porzione di curva chiusa compresa tra due punti
    ''' </summary>
    ''' <param name="curvaDaDividereChiusa"></param>
    ''' <param name="P1"></param>
    ''' <param name="P2"></param>
    ''' <param name="parteCorta"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function PorzioneCurva(ByVal curvaDaDividereChiusa As IOnCurve, ByVal P1 As IOn3dPoint, ByVal P2 As IOn3dPoint, Optional ByVal parteCorta As Boolean = True) As OnCurve
        If curvaDaDividereChiusa Is Nothing Then Return Nothing
        If Not curvaDaDividereChiusa.IsClosed Then Return Nothing
        Dim curva As OnCurve = curvaDaDividereChiusa.DuplicateCurve

        Dim tP1, tP2 As Double
        Dim curva1 As OnCurve = Nothing
        Dim curva2 As OnCurve = Nothing
        Dim lCurva1, lCurva2 As Double
        Dim res As Boolean = curva.GetClosestPoint(P1, tP1)
        res = res And curva.ChangeClosedCurveSeam(tP1)
        res = res And curva.GetClosestPoint(P2, tP2)
        res = res And curva.Split(tP2, curva1, curva2)
        curva.Dispose()
        If Not res Then Return Nothing
        If curva1 Is Nothing Then Return Nothing
        If curva2 Is Nothing Then Return Nothing
        curva1.GetLength(lCurva1)
        curva2.GetLength(lCurva2)

        If DirectCast(IIf(parteCorta, lCurva1 < lCurva2, lCurva1 > lCurva2), Boolean) Then
            curva2.Dispose()
            Return curva1
        Else
            curva1.Dispose()
            Return curva2
        End If
    End Function


    ''' <summary>
    ''' Restituisce una porzione di curva aperta separando con un punto
    ''' </summary>
    ''' <param name="curvaDaDividereAperta"></param>
    ''' <param name="p"></param>
    ''' <param name="parteCorta"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function PorzioneCurva(ByVal curvaDaDividereAperta As IOnCurve, ByVal p As IOn3dPoint, Optional ByVal parteCorta As Boolean = True) As OnCurve
        If curvaDaDividereAperta Is Nothing Then Return Nothing
        If curvaDaDividereAperta.IsClosed Then Return Nothing
        Dim curva As OnCurve = curvaDaDividereAperta.DuplicateCurve

        Dim tp As Double
        Dim curva1 As OnCurve = Nothing
        Dim curva2 As OnCurve = Nothing
        Dim lCurva1, lCurva2 As Double
        Dim res As Boolean = curva.GetClosestPoint(p, tp)
        res = res And curva.Split(tp, curva1, curva2)
        curva.Dispose()
        If Not res Then Return Nothing
        If curva1 Is Nothing Then Return Nothing
        If curva2 Is Nothing Then Return Nothing
        curva1.GetLength(lCurva1)
        curva2.GetLength(lCurva2)
        If DirectCast(IIf(parteCorta, lCurva1 < lCurva2, lCurva1 > lCurva2), Boolean) Then
            curva2.Dispose()
            Return curva1
        Else
            curva1.Dispose()
            Return curva2
        End If
    End Function


    ''' <summary>
    ''' Restituisce le due porzioni di una curva chiusa divisa nei punti con parametro t1 e t2
    ''' </summary>
    ''' <param name="curva"></param>
    ''' <param name="t1"></param>
    ''' <param name="t2"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function DividiCurvaIn2(ByVal curva As OnCurve, ByVal t1 As Double, ByVal t2 As Double) As OnCurve()
        If Not curva.IsClosed Then Return Nothing
        curva.ChangeClosedCurveSeam(t1)
        Dim curva1 As OnCurve = Nothing
        Dim curva2 As OnCurve = Nothing
        curva.Split(t2, curva1, curva2)
        Dim res(1) As OnCurve
        res(0) = curva1
        res(1) = curva2
        Return res
    End Function



    ''' <summary>
    ''' Restituisce un array di punti On3dPoint che dividono la curva in N parti
    ''' </summary>
    ''' <param name="curve"></param>
    ''' <param name="numParti"></param>
    ''' <param name="CreatePoints"></param>
    ''' <param name="strBaseName"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function SeparateCurve(ByVal curve As IOnCurve, ByVal numParti As Integer, Optional ByVal CreatePoints As Boolean = False, Optional ByVal strBaseName As String = "") As On3dPointArray
        If curve Is Nothing Then Return Nothing

        Dim pointAttributes As New MRhinoObjectAttributes
        Dim arrayPoints As New On3dPointArray(numParti)
        Dim domain As OnInterval = curve.Domain
        Dim delta As Double = domain.Length / numParti
        For i As Integer = 0 To CInt(IIf(curve.IsClosed, numParti - 1, numParti))
            arrayPoints.Append(curve.PointAt(domain.Min + i * delta))
            If CreatePoints Then
                pointAttributes.m_name = strBaseName & i + 1
                RhUtil.RhinoApp.ActiveDoc.AddPointObject(arrayPoints(i), pointAttributes)
            End If
        Next
        pointAttributes.Dispose()
        domain.Dispose()
        Return arrayPoints
    End Function



    ''' <summary>
    ''' Suddividi una curva in N parti considerando la curvatura; ritorna il punto iniziale della curva ma non quello finale
    ''' </summary>
    ''' <param name="curva"></param>
    ''' <param name="numSuddivisioni"></param>
    ''' <param name="peso">PESO = 0 la curvatura viene ignorata ; PESO > 0 la curvatura viene considerata</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function SuddividiCurva(ByVal curva As IOnCurve, ByVal numSuddivisioni As Integer, Optional ByVal peso As Double = 0.0) As On3dPointArray
        If curva Is Nothing Then Return Nothing
        Const SUDDIVISIONI_T As Integer = 1000
        Dim delta As Double = curva.Domain.Length / SUDDIVISIONI_T
        Dim integrale(SUDDIVISIONI_T - 1 + 1) As Double
        Dim lunghezza, curvatura, somma As Double
        integrale(0) = 0
        For i As Integer = 1 To SUDDIVISIONI_T - 1 + 1
            curvatura = curva.CurvatureAt(curva.Domain.Min + delta * i).Length
            curva.GetLength(lunghezza, 0.01, New OnInterval(curva.Domain.Min + delta * (i - 1), curva.Domain.Min + delta * i))
            somma += lunghezza + peso * curvatura
            integrale(i) = somma
        Next
        Dim avanzamento As Double = integrale(SUDDIVISIONI_T - 1 + 1) / numSuddivisioni
        Dim t(numSuddivisioni - 1) As Double
        t(0) = curva.Domain.Min
        For i As Integer = 1 To numSuddivisioni - 1          '<-- ciclo avanzamenti
            For j As Integer = 1 To SUDDIVISIONI_T - 1 + 1
                Dim tSup, tInf, intSup, intInf As Double
                If integrale(j) >= avanzamento * i Then
                    intSup = integrale(j)
                    intInf = integrale(j - 1)
                    tSup = curva.Domain.Min + j * delta
                    tInf = curva.Domain.Min + (j - 1) * delta
                    t(i) = tInf + (tSup - tInf) * (avanzamento * i - intInf) / (intSup - intInf)
                    Exit For
                End If
            Next
        Next
        Dim res As New On3dPointArray(numSuddivisioni)
        For i As Integer = 0 To numSuddivisioni - 1
            res.Append(curva.PointAt(t(i)))
        Next
        Return res
    End Function


    ''' <summary>
    ''' Suddividi una curva prevedendo i punti di tangenza ad 1/3 dagli estremi come tipico in Rhinoceros.
    ''' I punti di tangenza sono conteggiati in numSuddivisioni
    ''' </summary>
    ''' <param name="curva"></param>
    ''' <param name="numSuddivisioni"></param>
    ''' <returns>Ritorna il punto iniziale della curva ma non quello finale</returns>
    ''' <remarks></remarks>
    Public Shared Function SuddividiCurvaConPuntiTangenza(ByVal curva As IOnCurve, ByVal numSuddivisioni As Integer) As On3dPointArray
        Dim t(numSuddivisioni - 3) As Double        '2 in meno
        For i As Integer = 0 To numSuddivisioni - 3
            curva.GetNormalizedArcLengthPoint(i / (numSuddivisioni - 2), t(i))
        Next
        Dim tTangenzaIniziale As Double
        curva.GetNormalizedArcLengthPoint(0.33 / (numSuddivisioni - 2), tTangenzaIniziale)
        Dim tTangenzaFinale As Double
        curva.GetNormalizedArcLengthPoint(1 - 0.33 / (numSuddivisioni - 2), tTangenzaFinale)
        Dim res As New On3dPointArray(numSuddivisioni)
        For i As Integer = 0 To numSuddivisioni - 3
            res.Append(curva.PointAt(t(i)))
        Next
        res.Insert(1, curva.PointAt(tTangenzaIniziale))
        res.Append(curva.PointAt(tTangenzaFinale))
        Return res
    End Function


    ''' <summary>
    ''' Rimuove dei segmenti a una polilinea
    ''' </summary>
    ''' <param name="polilinea"></param>
    ''' <param name="indiciDaRimuovere"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function RimuoviSegmentiPolilinea(ByVal polilinea As IOnPolyline, ByVal indiciDaRimuovere As Arrayint) As List(Of OnPolyline)
        'Verifiche
        If polilinea Is Nothing Then Return Nothing
        If indiciDaRimuovere Is Nothing Then Return Nothing
        If indiciDaRimuovere.Count = 0 Then Return Nothing
        For i As Integer = 0 To indiciDaRimuovere.Count - 1
            If indiciDaRimuovere(i) > polilinea.SegmentCount - 1 Then Return Nothing
        Next

        Dim indici(indiciDaRimuovere.Count - 1) As Integer
        For i As Integer = 0 To indiciDaRimuovere.Count - 1
            indici(i) = indiciDaRimuovere(i)
        Next

        'Calcolo
        Dim res As New List(Of OnPolyline)
        res.Add(New OnPolyline)
        res(0).Append(polilinea.Item(0))
        For i As Integer = 0 To polilinea.SegmentCount - 1
            If Array.IndexOf(indici, i) >= 0 Then res.Add(New OnPolyline)
            res(res.Count - 1).Append(polilinea.Item(i + 1))
        Next
        For i As Integer = res.Count - 1 To 0 Step -1
            If res(i).Count < 2 Then res.RemoveAt(i)
        Next

        Return res
    End Function



    ''' <summary>
    ''' Calcola un punto intermedio su una curva
    ''' </summary>
    ''' <param name="curva"></param>
    ''' <param name="rapporto"></param>
    ''' <param name="t0"></param>
    ''' <param name="t1"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function PuntoIntermedio(ByVal curva As IOnCurve, ByVal rapporto As Double, Optional ByVal t0 As Double = Double.NegativeInfinity, Optional ByVal t1 As Double = Double.NegativeInfinity) As On3dPoint
        If curva Is Nothing Then Return Nothing
        Dim tResult As Double
        If Double.IsNegativeInfinity(t0) And Double.IsNegativeInfinity(t1) Then
            curva.GetNormalizedArcLengthPoint(rapporto, tResult, 0.001)
        Else
            If Double.IsNegativeInfinity(t0) Then t0 = curva.Domain.m_t(0)
            If Double.IsNegativeInfinity(t1) Then t1 = curva.Domain.m_t(1)
            Dim subDomain As New OnInterval(t0, t1)
            If Not curva.GetNormalizedArcLengthPoint(rapporto, tResult, 0.001, subDomain) Then
                tResult = subDomain.ParameterAt(rapporto)
            End If
            subDomain.Dispose()
        End If
        Return curva.PointAt(tResult)
    End Function


    ''' <summary>
    ''' Calcola un punto intermedio su una curva
    ''' </summary>
    ''' <param name="curva"></param>
    ''' <param name="rapporto"></param>
    ''' <param name="puntoT0"></param>
    ''' <param name="puntoT1"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function PuntoIntermedio(ByVal curva As IOnCurve, ByVal rapporto As Double, ByVal puntoT0 As IOn3dPoint, ByVal puntoT1 As IOn3dPoint) As On3dPoint
        If curva Is Nothing Then Return Nothing
        Dim t0, t1 As Double
        curva.GetClosestPoint(puntoT0, t0)
        curva.GetClosestPoint(puntoT1, t1)
        Dim subDomain As New OnInterval(t0, t1)
        Dim tResult As Double
        If t1 < t0 Then rapporto = 1 - rapporto
        curva.GetNormalizedArcLengthPoint(rapporto, tResult, 0.00000001, subDomain)
        subDomain.Dispose()
        Return curva.PointAt(tResult)
    End Function



    ''' <summary>
    ''' Crea una curva di terzo grado aperta a partire da un array di CV
    ''' </summary>
    ''' <param name="CV"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function CreaCurvaDaCV(ByVal CV As On3dPointArray) As OnNurbsCurve
        Dim curva As New OnNurbsCurve(3, True, 4, CV.Count)
        For i As Integer = 0 To CV.Count - 1
            curva.SetCV(i, CV(i))
        Next
        curva.MakeClampedUniformKnotVector()
        Return curva
    End Function

    ''' <summary>
    ''' Overload
    ''' </summary>
    ''' <param name="CVs"></param>
    ''' <returns></returns>
    Public Shared Function CreaCurvaDaCV(ByVal CVs As IList(Of On3dPoint)) As OnNurbsCurve
        Dim points As New On3dPointArray
        For Each point As On3dPoint In CVs
            points.Append(point)
        Next
        Dim result As OnNurbsCurve = CreaCurvaDaCV(points)
        points.Dispose()
        Return result
    End Function



    ''' <summary>
    ''' Crea una superfice NURBS a partire dai CV
    ''' </summary>
    ''' <param name="CV"></param>
    ''' <param name="ordineU"></param>
    ''' <param name="ordineV"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function CreaSuperficeDaCV(ByVal CV(,) As On3dPoint, Optional ByVal ordineU As Integer = 4, Optional ByVal ordineV As Integer = 4) As OnNurbsSurface
        Dim superficie As New OnNurbsSurface(3, True, ordineU, ordineV, CV.GetUpperBound(0) + 1, CV.GetUpperBound(1) + 1)
        For i As Integer = 0 To CV.GetUpperBound(0)
            For j As Integer = 0 To CV.GetUpperBound(1)
                superficie.SetCV(i, j, CV(i, j))
            Next
        Next
        superficie.MakeClampedUniformKnotVector(0)
        superficie.MakeClampedUniformKnotVector(1)
        Return superficie
    End Function


    ''' <summary>
    ''' Crea una curva di terzo grado chiusa a partire da un array di CV
    ''' </summary>
    ''' <param name="CV"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function CreaCurvaPeriodica(ByVal CV As On3dPointArray) As OnNurbsCurve
        'Imposta una curva chiusa senza assegnare i CV
        Dim curva As New OnNurbsCurve(3, False, 4, CV.Count + 2)
        'Assegna i CV compresi i duplicati
        curva.SetCV(0, CV(CV.Count - 2))
        For i As Integer = 0 To CV.Count - 1
            curva.SetCV(i + 1, CV(i))
        Next
        curva.SetCV(CV.Count + 1, CV(1))
        curva.MakePeriodicUniformKnotVector()
        Return curva
    End Function


    ''' <summary>
    ''' Crea una curva periodica interpolando i punti di passaggio 
    ''' </summary>
    ''' <param name="interpolatingPoints"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function CreaCurvaInterpolantePeriodica(ByVal interpolatingPoints As IOn3dPointArray) As OnNurbsCurve
        Return RhUtil.RhinoInterpCurve(3, interpolatingPoints, Nothing, Nothing, 3)
    End Function


    ''' <summary>
    ''' Crea una curva aperta interpolando i punti di passaggio 
    ''' </summary>
    ''' <param name="interpolatingPoints"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function CreaCurvaInterpolanteAperta(ByVal interpolatingPoints As IOn3dPointArray) As OnNurbsCurve
        Return RhUtil.RhinoInterpCurve(3, interpolatingPoints, Nothing, Nothing, 0)
    End Function


    ''' <summary>
    ''' Apre una poliLineCurve chiusa rimuovendo il segmento più lungo
    ''' </summary>
    ''' <param name="poliLinea"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function ApriPolilinea(ByRef poliLinea As OnCurve) As Boolean
        If poliLinea Is Nothing Then Return False
        If Not poliLinea.IsClosed Then Return False
        Dim plinePoints As New On3dPointArray
        If poliLinea.IsPolyline(plinePoints) = 0 Then Return False

        Dim poliCurva As New OnPolylineCurve(plinePoints)
        Dim nodi() As Double = Nothing
        If Not poliCurva.GetSpanVector(nodi) Then Return False

        'Ricava la distanza massima tra i nodi di una polilinea
        Dim maxDistanzaNodi As Double = Double.NegativeInfinity
        Dim indexMaxDistanzaNodi As Integer = -1
        Dim punto0 As On3dPoint
        Dim punto1 As On3dPoint
        For j As Integer = 0 To nodi.GetUpperBound(0) - 1
            punto0 = poliCurva.PointAt(nodi(j))
            punto1 = poliCurva.PointAt(nodi(j + 1))
            If punto0.DistanceTo(punto1) > maxDistanzaNodi Then
                maxDistanzaNodi = punto0.DistanceTo(punto1)
                indexMaxDistanzaNodi = j
            End If
        Next

        'Verifica se la distanza tra il primo e l'ultimo nodo è maggiore di quella calcolata in precedenza
        punto0 = poliCurva.PointAt(nodi(nodi.GetUpperBound(0)))
        punto1 = poliCurva.PointAt(nodi(0))
        If punto0.DistanceTo(punto1) > maxDistanzaNodi Then
            maxDistanzaNodi = punto0.DistanceTo(punto1)
            indexMaxDistanzaNodi = nodi.GetUpperBound(0)
        End If

        'Separa la curva nel nodo con la distanza maggiore dal prossimo
        If Not poliCurva.ChangeClosedCurveSeam(nodi(indexMaxDistanzaNodi)) Then Return False
        poliCurva.m_pline.Remove(0)

        'Assegna il risulato ed esci
        poliLinea.Dispose()
        poliLinea = New OnPolylineCurve(poliCurva.m_pline)
        poliCurva.Dispose()
        Return True
    End Function


    ''' <summary>
    ''' Chiude una curva aperta creando un segmento di giunzione tra punto iniziale e finale
    ''' </summary>
    ''' <param name="curva"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function ChiudiCurva(ByVal curva As IOnCurve) As OnCurve
        If curva Is Nothing Then Return Nothing
        If curva.IsClosed Then Return curva.DuplicateCurve

        Dim inputCurves(1) As OnCurve
        inputCurves(0) = curva.DuplicateCurve
        Dim puntoIniziale As On3dPoint = curva.PointAtStart
        Dim puntoFinale As On3dPoint = curva.PointAtEnd
        inputCurves(1) = New OnLineCurve(puntoIniziale, puntoFinale)
        Dim outputCurves(0) As OnCurve
        RhUtil.RhinoMergeCurves(inputCurves, outputCurves)
        inputCurves(0).Dispose()
        inputCurves(1).Dispose()
        If outputCurves Is Nothing Then Return Nothing
        If outputCurves.GetLength(0) <> 1 Then Return Nothing
        Return outputCurves(0)
    End Function


    ' ''' <summary>
    ' ''' ATTENZIONE IN CERTI CASI NON MEGLIO IDENTIFICATI IL RISULTATO E' ERRATO
    ' ''' Congiungi gli estremi di un insieme di curve con segmenti di lunghezza massima assegnata
    ' ''' </summary>
    ' ''' <param name="curves"></param>
    ' ''' <param name="maxLunghezzaLegame"></param>
    ' ''' <returns></returns>
    ' ''' <remarks></remarks>
    'Public Shared Function CongiungiCurve(ByVal curves() As IOnCurve, ByVal maxLunghezzaLegame As Double) As OnCurve()
    '    If curves Is Nothing Then Return Nothing
    '    If curves.GetLength(0) = 0 Then Return Nothing

    '    'Caso con una sola curva: se è chiusa ritorna la curva altrimenti richiama la funzione ChiudiCurva. Il risultato sarà sempre una sola curva chiusa
    '    If curves.GetLength(0) = 1 Then
    '        Dim outputCurve(0) As OnCurve
    '        If curves(0).IsClosed Then
    '            outputCurve(0) = curves(0).DuplicateCurve
    '        Else
    '            outputCurve(0) = ChiudiCurva(curves(0))
    '        End If
    '        Return outputCurve
    '    End If

    '    'Array di punti che a coppie rappresentano il punto iniziale e finale delle polilinee
    '    Dim puntiEstremi As New On3dPointArray
    '    For i As Integer = 0 To curves.GetUpperBound(0)
    '        puntiEstremi.Append(curves(i).PointAtStart)
    '        puntiEstremi.Append(curves(i).PointAtEnd)
    '    Next

    '    'Crea legami estremi e li ordina per distanza
    '    Dim legamiEstremi As New RhLinks
    '    For i As Integer = 0 To puntiEstremi.Count - 1
    '        For j As Integer = (i + 1 + ((i + 1) Mod 2)) To puntiEstremi.Count - 1
    '            legamiEstremi.Add(New RhLink(i, j, puntiEstremi(i).DistanceTo(puntiEstremi(j))))
    '        Next
    '    Next
    '    legamiEstremi.Sort()

    '    'Crea linee estremi
    '    Dim connessioniEstremi(puntiEstremi.Count - 1) As Integer
    '    Dim lineeEstremi As New ArrayList
    '    For i As Integer = 0 To legamiEstremi.Count - 1
    '        Dim legameEstremi As RhLink = legamiEstremi(i)
    '        If connessioniEstremi(legameEstremi.Indice0) < 1 AndAlso connessioniEstremi(legameEstremi.Indice1) < 1 Then
    '            connessioniEstremi(legameEstremi.Indice0) += 1
    '            connessioniEstremi(legameEstremi.Indice1) += 1
    '            lineeEstremi.Add(New OnLineCurve(puntiEstremi(legameEstremi.Indice0), puntiEstremi(legameEstremi.Indice1)))
    '        End If
    '    Next

    '    'Elimina le linee troppo lunghe (stesso parametro individuato in precedenza)
    '    For i As Integer = lineeEstremi.Count - 1 To 0 Step -1
    '        If DirectCast(lineeEstremi(i), OnLineCurve).m_line.Length > maxLunghezzaLegame Then
    '            lineeEstremi.RemoveAt(i)
    '        End If
    '    Next

    '    'Unisci le linee a formare policurve e ritorna un array con le polycurve chiuse
    '    Dim countIniziale As Integer = curves.GetUpperBound(0) + 1
    '    ReDim Preserve curves(countIniziale - 1 + lineeEstremi.Count)
    '    For i As Integer = 0 To lineeEstremi.Count - 1
    '        curves(countIniziale + i) = DirectCast(lineeEstremi(i), OnLineCurve)
    '    Next
    '    Dim outputCurvesEstremi(0) As OnCurve
    '    RhUtil.RhinoMergeCurves(curves, outputCurvesEstremi)
    '    Return outputCurvesEstremi
    'End Function


    ''' <summary>
    ''' Crea un insieme di polilinee che collega i punti di un array con lunghezza massima assegnata dei segmenti
    ''' </summary>
    ''' <param name="punti"></param>
    ''' <param name="maxLunghezzaLinea"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function CreaLineeDaPunti(ByVal punti As ArrayOn3dPoint, Optional ByVal maxLunghezzaLinea As Double = 10) As OnCurve()

        'Crea legami tra un punto e gli altri successivi; poi ordina per distanza
        Dim legami As New RhLinks
        For i As Integer = 0 To punti.Count - 1
            For j As Integer = i + 1 To punti.Count - 1 'crea dei legami tra un punto e gli altri successivi 
                legami.Add(New RhLink(i, j, punti(i).DistanceTo(punti(j))))
            Next
        Next
        legami.Sort()

        'Per ogni nodo crea due linee: una lo unisce al successivo, l'altra al precedente
        Dim connessioni(punti.Count - 1) As Integer
        Dim linee As New ArrayList
        For i As Integer = 0 To legami.Count - 1
            Dim legame As RhLink = legami(i)
            'Un nodo può avere al massimo due connessioni
            If connessioni(legame.Indice0) < 2 AndAlso connessioni(legame.Indice1) < 2 Then
                connessioni(legame.Indice0) += 1
                connessioni(legame.Indice1) += 1
                linee.Add(New OnLineCurve(punti(legame.Indice0), punti(legame.Indice1)))
            End If
        Next

        'Elimina le linee troppo lunghe
        For i As Integer = linee.Count - 1 To 0 Step -1
            If DirectCast(linee(i), OnLineCurve).m_line.Length > maxLunghezzaLinea Then
                linee.RemoveAt(i)
            End If
        Next

        'Unisci le linee a formare polilinee
        Dim inputCurves() As IOnCurve = DirectCast(linee.ToArray(GetType(OnLineCurve)), IOnCurve())
        Dim outputCurves(0) As OnCurve
        RhUtil.RhinoMergeCurves(inputCurves, outputCurves)

        'Rimuovi il segmento più lungo per ogni polilinea chiusa
        For i As Integer = 0 To outputCurves.GetUpperBound(0)
            ApriPolilinea(outputCurves(i))
        Next

        Return outputCurves
    End Function



    ''' <summary>
    ''' Calcola l'angolo [0, PI] tra due vettori nello spazio 
    ''' </summary>
    ''' <param name="dir1"></param>
    ''' <param name="dir2"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function Angolo(ByVal dir1 As IOn3dVector, ByVal dir2 As IOn3dVector) As Double
        Return Acos(OnUtil.ON_DotProduct(dir1, dir2) / (dir1.Length * dir2.Length))
    End Function


    ''' <summary>
    ''' Calcola la BoundingBox di un oggetto IRhinoObjRef
    ''' </summary>
    ''' <param name="objRef"></param>
    ''' <param name="piano"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function CalcolaBBox(ByVal objRef As IRhinoObjRef, ByVal piano As IOnPlane) As OnBoundingBox
        Dim oggetti(0) As IRhinoObject
        oggetti(0) = objRef.Object()
        Dim bbox As OnBoundingBox = New OnBoundingBox
        RhUtil.RhinoGetTightBoundingBox(oggetti, bbox, False, New OnPlane(piano), False)
        Return bbox
    End Function



    ''' <summary>
    ''' Usa lo script di Rhino per calcolare la MeshOutline
    ''' </summary>
    ''' <param name="meshObjRef"></param>
    ''' <param name="nomeVista"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function EsegueMeshOutline(ByVal meshObjRef As MRhinoObjRef, ByVal nomeVista As String) As OnNurbsCurve
        Dim nomeVistaAttiva As String = RhUtil.RhinoApp.ActiveView.ActiveViewport.Name
        RhUtil.RhinoApp.RunScript("_SetView _World " & nomeVista, 1)

        'Esegue il comando MESH_OUTLINE
        meshObjRef.Object.Select(True, False, True, True, True, True)
        RhUtil.RhinoApp.RunScript("_MeshOutline _enter", 0)
        RhUtil.RhinoApp.RunScript("_SelLast _enter", 0)
        Dim getObj As New MRhinoGetObject
        getObj.GetObjects(0, 0)

        'Imposta il risultato
        Dim result As OnNurbsCurve = Nothing
        If getObj.ObjectCount = 1 Then result = getObj.Object(0).Curve.NurbsCurve

        'Cancella oggetti e ritorna
        For i As Integer = 0 To getObj.ObjectCount - 1
            RhUtil.RhinoApp.ActiveDoc.DeleteObject(getObj.Object(i))
        Next
        getObj.Dispose()
        RhUtil.RhinoApp.RunScript("_SetView _World " & nomeVistaAttiva, 1)
        RhUtil.RhinoApp.ActiveDoc.Redraw()
        Return result
    End Function



    ''' <summary>
    ''' Section a mesh object with a sectionPlane. Return a policurve joining all section curves spans
    ''' </summary>
    ''' <param name="sectionPlane"></param>
    ''' <param name="meshObjRef"></param>
    ''' <returns></returns>
    ''' <remarks>modificato l'orientamento del piano di sezione per rendere coerente con MeshSectionLongest. Produce una inversione della curva sezionata</remarks>
    Public Shared Function MeshSection(ByVal sectionPlane As IOnPlane, ByVal meshObjRef As MRhinoObjRef, Optional ByVal maxLunghezzaLegame As Double = Double.PositiveInfinity) As OnCurve()
        'Setta il piano di riferimento
        Dim yAxis As New On3dVector(sectionPlane.zaxis)
        yAxis.Reverse()
        Dim cPlaneSezione As New OnPlane(sectionPlane.origin, sectionPlane.xaxis, yAxis)
        yAxis.Dispose()
        Dim view As MRhinoViewport = RhUtil.RhinoApp.ActiveView.MainViewport
        Dim backUpPlane As New OnPlane(view.ConstructionPlane.m_plane)
        Dim cPlane As New On3dmConstructionPlane(view.ConstructionPlane)
        cPlane.m_plane = cPlaneSezione
        view.PushConstructionPlane(cPlane)

        'Crea sezioni
        meshObjRef.Object.Select(True, False, True, True, True, True)
        RhUtil.RhinoApp.RunScript("_Section 0,0,0 1,0,0 _enter", 0)
        cPlane.m_plane = backUpPlane
        view.PushConstructionPlane(cPlane)
        backUpPlane.Dispose()
        cPlane.Dispose()
        cPlaneSezione.Dispose()
        Dim getObj As New MRhinoGetObject
        getObj.EnablePostSelect(False)
        getObj.GetObjects(0, 0)

        Dim curves(getObj.ObjectCount - 1) As IOnCurve
        For i As Integer = 0 To curves.GetUpperBound(0)
            curves(i) = getObj.Object(i).Curve
        Next
        Dim result() As OnCurve = {}
        RhUtil.RhinoMergeCurves(curves, result, maxLunghezzaLegame)

        'Cancella oggetti e ritorna
        For i As Integer = 0 To getObj.ObjectCount - 1
            RhUtil.RhinoApp.ActiveDoc.DeleteObject(getObj.Object(i))
        Next
        getObj.Dispose()
        RhUtil.RhinoApp.ActiveDoc.Redraw()
        Return result
    End Function


    ''' <summary>
    ''' Crea una sezione di un oggetto mesh chiudendo le polilinee ottenute con il parametro "maxLinkLength".
    ''' Seleziona quindi la curva più lunga se ce ne è più di una.
    ''' Se questa è aperta prova a chiuderla creando un segmento tra l'estremo finale e quello iniziale nel caso in cui la distanza sia inferiore a "maxRepairDistance"
    ''' </summary>
    ''' <param name="sectionPlane"></param>
    ''' <param name="meshObjRef"></param>
    ''' <param name="maxLinkLength"></param>
    ''' <param name="maxRepairDistance"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function MeshSectionOnlyLongestCurve(ByVal sectionPlane As IOnPlane, ByVal meshObjRef As MRhinoObjRef, Optional ByVal maxLinkLength As Double = Double.PositiveInfinity, Optional ByVal maxRepairDistance As Double = 10.0) As OnCurve
        Dim sectionCurves As OnCurve() = MeshSection(sectionPlane, meshObjRef, maxLinkLength)
        Return GetLongestCurve(sectionCurves, maxRepairDistance)
    End Function


    ''' <summary>
    ''' Seleziona quindi la curva più lunga se ce ne è più di una.
    ''' Se questa è aperta prova a chiuderla creando un segmento tra l'estremo finale e quello iniziale nel caso in cui la distanza sia inferiore a "distanzaMaxPerRiparazioneManuale"    
    ''' </summary>
    ''' <param name="curves"></param>
    ''' <param name="maxDistanzaPerRiparazioneManuale"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function GetLongestCurve(ByVal curves As OnCurve(), ByVal maxDistanzaPerRiparazioneManuale As Double) As OnCurve
        If curves Is Nothing Then Return Nothing
        Dim result As OnCurve
        Select Case curves.Length
            Case 0 : Return Nothing
            Case 1 : result = curves(0)
            Case Else
                Dim maxLength As Double
                Dim maxLengthCurveIndex As Integer = 0
                curves(0).GetLength(maxLength)
                For i As Integer = 1 To curves.GetUpperBound(0)
                    Dim tmpLength As Double
                    curves(i).GetLength(tmpLength)
                    If tmpLength > maxLength Then
                        maxLength = tmpLength
                        maxLengthCurveIndex = i
                    End If
                Next
                result = curves(maxLengthCurveIndex)
        End Select

        If Not result.IsClosed Then
            If result.PointAtStart.DistanceTo(result.PointAtEnd) < maxDistanzaPerRiparazioneManuale Then
                Dim closedResult As OnCurve = ChiudiCurva(result)
                result.Dispose()
                result = closedResult
            End If
        End If
        Return result
    End Function


    ''' <summary>
    ''' Section a mesh object with a sectionPlane. Return a reference to longest section curve
    ''' </summary>
    ''' <param name="sectionPlane"></param>
    ''' <param name="meshObjRef"></param>
    ''' <param name="sectionName"></param>
    ''' <returns></returns>
    ''' <remarks>The section is oriented as sectionPlane x axis</remarks>
    Public Shared Function MeshSectionLongest(ByVal sectionPlane As OnPlane, ByVal meshObjRef As MRhinoObjRef, Optional ByVal sectionName As String = "") As MRhinoObjRef
        Dim cPlaneSezione As New OnPlane(sectionPlane.origin, sectionPlane.xaxis, sectionPlane.zaxis * (-1))
        Dim view As MRhinoViewport = RhUtil.RhinoApp.ActiveView.MainViewport
        Dim backUpPlane As New OnPlane(view.ConstructionPlane.m_plane)
        Dim cPlane As New On3dmConstructionPlane(view.ConstructionPlane)
        cPlane.m_plane = cPlaneSezione
        view.PushConstructionPlane(cPlane)
        meshObjRef.Object.Select(True, False, True, True, True, True)
        RhUtil.RhinoApp.RunScript("_Section 0,0,0 1,0,0 _enter", 0)
        Dim getObj As New MRhinoGetObject
        getObj.EnablePostSelect(False)
        getObj.GetObjects(0, -1)
        cPlane.m_plane = backUpPlane
        view.PushConstructionPlane(cPlane)
        backUpPlane.Dispose()
        cPlane.Dispose()
        cPlaneSezione.Dispose()
        'Controlla se il risultato è di almeno un elemento; in caso contrario esce
        If getObj.ObjectCount = 0 Then
            getObj.Dispose()
            Return Nothing
        End If

        'Select the longest section
        Dim longestSectionIndex As Integer
        Select Case getObj.ObjectCount
            Case 0
                Return Nothing
            Case 1
                longestSectionIndex = 0
            Case Else
                Dim maxSectionLength As Double
                getObj.Object(0).Curve.NurbsCurve.GetLength(maxSectionLength)
                For i As Integer = 1 To getObj.ObjectCount - 1
                    Dim length As Double
                    getObj.Object(i).Curve.GetLength(length)
                    If length > maxSectionLength Then
                        longestSectionIndex = i
                        maxSectionLength = length
                    End If
                Next
                For i As Integer = 0 To getObj.ObjectCount - 1
                    If i <> longestSectionIndex Then RhUtil.RhinoApp.ActiveDoc.DeleteObject(getObj.Object(i))
                Next
        End Select

        If sectionName <> "" Then
            Dim newCurveAttributes As New MRhinoObjectAttributes(getObj.Object(longestSectionIndex).Object.Attributes)
            newCurveAttributes.m_name = sectionName
            RhUtil.RhinoApp.ActiveDoc.ModifyObjectAttributes(getObj.Object(longestSectionIndex), newCurveAttributes)
            newCurveAttributes.Dispose()
        End If
        RhUtil.RhinoApp.ActiveDoc.UnselectAll()
        Dim res As MRhinoObjRef = getObj.Object(longestSectionIndex)
        getObj.Dispose()
        Return res
    End Function




    ''' <summary>
    ''' Interseca due mesh e ritorna una policurve che congiunge tutte le curve di intersezione
    ''' </summary>
    ''' <param name="meshRef1"></param>
    ''' <param name="meshRef2"></param>
    ''' <param name="maxLinkLength"></param>
    ''' <returns></returns>
    ''' <remarks>modificato l'orientamento del piano di sezione per rendere coerente con MeshSectionLongest. Produce una inversione della curva sezionata</remarks>
    Public Shared Function MeshMeshIntersection(ByVal meshRef1 As MRhinoObjRef, ByVal meshRef2 As MRhinoObjRef, Optional ByVal maxLinkLength As Double = Double.PositiveInfinity) As OnCurve()
        'Crea le curve di intersezione
        meshRef1.Object.Select(True, False, True, True, True, True)
        meshRef2.Object.Select(True, False, True, True, True, True)
        RhUtil.RhinoApp.RunScript("_MeshIntersect", 0)
        Dim getObj As New MRhinoGetObject
        getObj.EnablePostSelect(False)
        getObj.GetObjects(0, 0)

        Dim curves(getObj.ObjectCount - 1) As IOnCurve
        For i As Integer = 0 To curves.GetUpperBound(0)
            curves(i) = getObj.Object(i).Curve
        Next
        Dim result() As OnCurve = {}
        RhUtil.RhinoMergeCurves(curves, result, maxLinkLength)

        'Cancella oggetti e ritorna
        For i As Integer = 0 To getObj.ObjectCount - 1
            RhUtil.RhinoApp.ActiveDoc.DeleteObject(getObj.Object(i))
        Next
        getObj.Dispose()
        RhUtil.RhinoApp.ActiveDoc.Redraw()
        Return result
    End Function


    ''' <summary>
    ''' Interseca due mesh chiudendo le polilinee ottenute con il parametro "maxLinkLength".
    ''' Seleziona quindi la curva più lunga se ce ne è più di una.
    ''' Se questa è aperta prova a chiuderla creando un segmento tra l'estremo finale e quello iniziale nel caso in cui la distanza sia inferiore a "maxRepairDistance"
    ''' </summary>
    ''' <param name="meshRef1"></param>
    ''' <param name="meshRef2"></param>
    ''' <param name="maxLinkLength"></param>
    ''' <param name="maxRepairDistance"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function MeshMeshIntersectionOnlyLongestCurve(ByVal meshRef1 As MRhinoObjRef, ByVal meshRef2 As MRhinoObjRef, Optional ByVal maxLinkLength As Double = Double.PositiveInfinity, Optional ByVal maxRepairDistance As Double = 10.0) As OnCurve
        Dim intersectionCurves As OnCurve() = MeshMeshIntersection(meshRef1, meshRef2, maxLinkLength)
        Return GetLongestCurve(intersectionCurves, maxRepairDistance)
    End Function



    ''' <summary>
    ''' Ritorna i punti a curvatura più elevata di una polilinea
    ''' </summary>
    ''' <param name="polilinea"></param>
    ''' <param name="numeroPunti">Numero punti da ritornare</param>
    ''' <param name="distanzaDiRicerca">Minima distanza fra due punti ritornati</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function EstraiPuntiMassimaCurvatura(ByVal polilinea As IOnCurve, ByVal numeroPunti As Integer, ByVal distanzaDiRicerca As Double) As On3dPointArray
        'Estrazione punti
        Dim punti As New On3dPointArray
        If polilinea.IsPolyline(punti) = 0 Then
            punti.Dispose()
            Return Nothing
        End If
        If polilinea.IsClosed Then punti.Remove(punti.Count - 1)

        'Calcolo delle curvature
        Dim curvature(punti.Count - 1) As Double
        Dim keysCurvature(punti.Count - 1) As Integer
        For i As Integer = 1 To punti.Count - 1 - 1
            Dim cerchio As New OnCircle(punti(i - 1), punti(i), punti(i + 1))
            If cerchio Is Nothing Then
                curvature(i) = 0
            Else
                curvature(i) = 1 / cerchio.radius
                curvature(i) *= (punti(i - 1).DistanceTo(punti(i)) + punti(i).DistanceTo(punti(i + 1))) / cerchio.Diameter
            End If
            keysCurvature(i) = i
            cerchio.Dispose()
        Next
        If polilinea.IsClosed Then
            'Punto iniziale
            Dim cerchio As New OnCircle(punti(punti.Count - 1), punti(0), punti(1))
            If cerchio Is Nothing Then
                curvature(0) = 0
            Else
                curvature(0) = 1 / cerchio.radius
                curvature(0) *= (punti(punti.Count - 1).DistanceTo(punti(0)) + punti(0).DistanceTo(punti(1))) / cerchio.Diameter
            End If
            keysCurvature(0) = 0
            cerchio.Dispose()

            'Punto finale
            cerchio = New OnCircle(punti(punti.Count - 2), punti(punti.Count - 1), punti(0))
            If cerchio Is Nothing Then
                curvature(punti.Count - 1) = 0
            Else
                curvature(punti.Count - 1) = 1 / cerchio.radius
                curvature(punti.Count - 1) *= (punti(punti.Count - 2).DistanceTo(punti(punti.Count - 1)) + punti(punti.Count - 1).DistanceTo(punti(0))) / cerchio.Diameter
            End If
            keysCurvature(punti.Count - 1) = punti.Count - 1
            cerchio.Dispose()
        End If

        'Ordinamento delle curvature
        Array.Sort(curvature, keysCurvature)

        'Ricerca dei punti risultato
        Dim numeroPuntiDaAnalizzare As Integer = numeroPunti
        If numeroPuntiDaAnalizzare > punti.Count Then numeroPuntiDaAnalizzare = punti.Count
        Dim result As On3dPointArray = Nothing
        Do
            If Not result Is Nothing Then result.Dispose()
            result = New On3dPointArray
            For i As Integer = 0 To numeroPuntiDaAnalizzare - 1
                Dim indice As Integer = keysCurvature(keysCurvature.GetUpperBound(0) - i)
                result.Append(punti(indice))
            Next

            'Raggruppa
            If distanzaDiRicerca > 0 Then
                Dim ridondanza As New Arrayint
                For i As Integer = 0 To result.Count - 1
                    ridondanza.Append(1)
                Next
                For i As Integer = result.Count - 1 To 1 Step -1
                    For j As Integer = i - 1 To 0 Step -1
                        If result(i).DistanceTo(result(j)) < distanzaDiRicerca Then
                            result(j) = MeanPosition(result(i), ridondanza(i), result(j), ridondanza(j))
                            ridondanza(j) += ridondanza(i)
                            ridondanza.Remove(i)
                            result.Remove(i)
                            Exit For
                        End If
                    Next
                Next
                ridondanza.Dispose()
            End If

            numeroPuntiDaAnalizzare += 1
        Loop Until result.Count = numeroPunti Or numeroPuntiDaAnalizzare > punti.Count

        'Riporta i punti sulla curva
        For i As Integer = 0 To result.Count - 1
            Dim t As Double
            If polilinea.GetClosestPoint(result(i), t) Then
                result(i) = polilinea.PointAt(t)
            End If
        Next

        Return result
    End Function



    ''' <summary>
    ''' Calcola la media pesata fra due punti
    ''' </summary>
    ''' <param name="position1"></param>
    ''' <param name="weight1"></param>
    ''' <param name="position2"></param>
    ''' <param name="weight2"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function MeanPosition(ByVal position1 As IOn3dPoint, ByVal weight1 As Double, ByVal position2 As IOn3dPoint, ByVal weight2 As Double) As On3dPoint
        Dim factor1, factor2 As Double
        If weight1 = 0 And weight2 = 0 Then
            factor1 = 1
            factor2 = 1
        Else
            factor1 = weight1 / (weight1 + weight2)
            factor2 = weight2 / (weight1 + weight2)
        End If
        Dim res As New On3dPoint(position1)
        res *= factor1
        Dim element2 As New On3dPoint(position2)
        element2 *= factor2
        res += element2
        element2.Dispose()
        Return res
    End Function


    ''' <summary>
    ''' Calcola la media pesata fra due vettori
    ''' </summary>
    ''' <param name="vector1"></param>
    ''' <param name="weight1"></param>
    ''' <param name="vector2"></param>
    ''' <param name="weight2"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function MeanDirection(ByVal vector1 As IOn3dVector, ByVal weight1 As Double, ByVal vector2 As IOn3dVector, ByVal weight2 As Double) As On3dPoint
        Dim factor1, factor2 As Double
        If weight1 = 0 And weight2 = 0 Then
            factor1 = 1
            factor2 = 1
        Else
            factor1 = weight1 / (weight1 + weight2)
            factor2 = weight2 / (weight1 + weight2)
        End If
        Dim res As New On3dVector(vector1)
        res *= factor1
        Dim element2 As New On3dVector(vector2)
        element2 *= factor2
        res += element2
        element2.Dispose()
        'Return res
        ''CORREZIONE PER FAR COMPILARE
        Return New On3dPoint(res.x, res.y, res.z)
    End Function



    ''' <summary>
    ''' Recupera i 6 punti estremi lungo gli assi X,Y,Z globali della mesh così come si trova nello spazio
    ''' </summary>
    ''' <param name="mesh"></param>
    ''' <param name="minX"></param>
    ''' <param name="maxX"></param>
    ''' <param name="minY"></param>
    ''' <param name="maxY"></param>
    ''' <param name="minZ"></param>
    ''' <param name="maxZ"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function MeshFindExtremePoints(ByVal mesh As IOnMesh, ByRef minX As On3dPoint, ByRef maxX As On3dPoint, ByRef minY As On3dPoint, ByRef maxY As On3dPoint, ByRef minZ As On3dPoint, ByRef maxZ As On3dPoint) As Boolean
        If mesh Is Nothing Then Return False
        Dim vertici As IOn3fPointArray = mesh.m_V
        Dim minIndexX, maxIndexX As Integer
        Dim minValueX As Single = Single.MaxValue
        Dim maxValueX As Single = Single.MinValue
        Dim minIndexY, maxIndexY As Integer
        Dim minValueY As Single = Single.MaxValue
        Dim maxValueY As Single = Single.MinValue
        Dim minIndexZ, maxIndexZ As Integer
        Dim minValueZ As Single = Single.MaxValue
        Dim maxValueZ As Single = Single.MinValue

        For i As Integer = 0 To vertici.Count - 1
            If vertici(i).x > maxValueX Then
                maxValueX = vertici(i).x
                maxIndexX = i
            End If
            If vertici(i).x < minValueX Then
                minValueX = vertici(i).x
                minIndexX = i
            End If
            If vertici(i).y > maxValueY Then
                maxValueY = vertici(i).y
                maxIndexY = i
            End If
            If vertici(i).y < minValueY Then
                minValueY = vertici(i).y
                minIndexY = i
            End If
            If vertici(i).z > maxValueZ Then
                maxValueZ = vertici(i).z
                maxIndexZ = i
            End If
            If vertici(i).z < minValueZ Then
                minValueZ = vertici(i).z
                minIndexZ = i
            End If
        Next
        minX = New On3dPoint(vertici(minIndexX).x, vertici(minIndexX).y, vertici(minIndexX).z)
        maxX = New On3dPoint(vertici(maxIndexX).x, vertici(maxIndexX).y, vertici(maxIndexX).z)
        minY = New On3dPoint(vertici(minIndexY).x, vertici(minIndexY).y, vertici(minIndexY).z)
        maxY = New On3dPoint(vertici(maxIndexY).x, vertici(maxIndexY).y, vertici(maxIndexY).z)
        minZ = New On3dPoint(vertici(minIndexZ).x, vertici(minIndexZ).y, vertici(minIndexZ).z)
        maxZ = New On3dPoint(vertici(maxIndexZ).x, vertici(maxIndexZ).y, vertici(maxIndexZ).z)
        Return True
    End Function


    ''' <summary>
    ''' Recupera i 4 punti estremi lungo gli assi X e Y della curva così come si trova nel piano
    ''' </summary>
    ''' <param name="curve"></param>
    ''' <param name="minX"></param>
    ''' <param name="maxX"></param>
    ''' <param name="minY"></param>
    ''' <param name="maxY"></param>
    ''' <param name="plane">Se nothing gli estremi vengono determinati rispetto agli assi globali</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function CurveFindExtremePoints(ByVal curve As IOnCurve, ByRef minX As On3dPoint, ByRef maxX As On3dPoint, ByRef minY As On3dPoint, ByRef maxY As On3dPoint, Optional ByVal plane As IOnPlane = Nothing, Optional ByVal tolerance As Double = 0.1) As Boolean
        If curve Is Nothing Then Return False
        If tolerance = 0 Then Return False

        'Crea la curva traformata
        Dim trasformedCurve As OnCurve = curve.DuplicateCurve
        Dim xForm As OnXform = Nothing
        If Not plane Is Nothing Then
            xForm = New OnXform
            xForm.Rotation(plane, OnPlane.World_xy)
            trasformedCurve.Transform(xForm)
        End If

        'Trova gli estremi
        Dim length As Double
        trasformedCurve.GetLength(length)
        Dim subdivisions As Integer = CInt(length / tolerance) + 1
        Dim domain As OnInterval = trasformedCurve.Domain

        Dim tMinX, tMaxX As Double
        Dim minValueX As Double = Double.MaxValue
        Dim maxValueX As Double = Double.MinValue
        Dim tMinY, tMaxY As Double
        Dim minValueY As Double = Double.MaxValue
        Dim maxValueY As Double = Double.MinValue

        For i As Integer = 0 To subdivisions
            Dim t As Double = trasformedCurve.Domain.m_t(0) + i * trasformedCurve.Domain.Length / subdivisions
            Dim point As On3dPoint = trasformedCurve.PointAt(t)
            If point.x > maxValueX Then
                maxValueX = point.x
                tMaxX = t
            End If
            If point.x < minValueX Then
                minValueX = point.x
                tMinX = t
            End If
            If point.y > maxValueY Then
                maxValueY = point.y
                tMaxY = t
            End If
            If point.y < minValueY Then
                minValueY = point.y
                tMinY = t
            End If
            point.Dispose()
        Next
        domain.Dispose()

        If Not minX Is Nothing Then minX.Dispose()
        minX = trasformedCurve.PointAt(tMinX)
        If Not maxX Is Nothing Then maxX.Dispose()
        maxX = trasformedCurve.PointAt(tMaxX)
        If Not minY Is Nothing Then minY.Dispose()
        minY = trasformedCurve.PointAt(tMinY)
        If Not maxY Is Nothing Then maxY.Dispose()
        maxY = trasformedCurve.PointAt(tMaxY)

        'Ristrasforma i punti e ritorna
        If Not plane Is Nothing Then
            xForm.Rotation(OnPlane.World_xy, plane)
            minX.Transform(xForm)
            maxX.Transform(xForm)
            minY.Transform(xForm)
            maxY.Transform(xForm)
            xForm.Dispose()
        End If
        trasformedCurve.Dispose()
        Return True
    End Function



    ''' <summary>
    ''' Visualizza una linea a video di lunghezza impostabile lungo la direzione specificata. se non viene passato il punto di applicazione viene presa l'origine globale
    ''' </summary>
    ''' <param name="direzione"></param>
    ''' <param name="puntoApplicazione"></param>
    ''' <param name="lunghezza"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function VisualizzaDirezione(ByVal direzione As IOn3dVector, Optional ByVal puntoApplicazione As IOn3dPoint = Nothing, Optional ByVal lunghezza As Double = 200.0) As MRhinoCurveObject
        Dim puntoA As On3dPoint
        If puntoApplicazione Is Nothing Then
            puntoA = New On3dPoint(OnPlane.World_xy.origin)
        Else
            puntoA = New On3dPoint(puntoApplicazione)
        End If
        Dim tmpDirezione As New On3dVector(direzione)
        tmpDirezione.Unitize()
        Dim tmpLinea As New OnLine(puntoA, puntoA + tmpDirezione * lunghezza)
        tmpDirezione.Dispose()
        Return RhUtil.RhinoApp.ActiveDoc.AddCurveObject(tmpLinea)
    End Function



    ''' <summary>
    ''' Crea un piano data l'origine e due angoli con i piani XZ e XY
    ''' </summary>
    ''' <param name="origin"></param>
    ''' <param name="angoloXZ"></param>
    ''' <param name="angoloXY"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function CreaPiano(ByVal origin As IOn3dPoint, ByVal angoloXZ As Double, ByVal angoloXY As Double) As OnPlane
        Dim xAxis As New On3dVector(Math.Cos(angoloXY * Math.PI / 180), Math.Sin(angoloXY * Math.PI / 180), 0)
        Dim yAxis As New On3dVector(Math.Cos(angoloXZ * Math.PI / 180), 0, Math.Sin(angoloXZ * Math.PI / 180))
        Dim plane As New OnPlane(origin, xAxis, yAxis)
        xAxis.Dispose()
        yAxis.Dispose()
        Return plane
    End Function


    ''' <summary>
    ''' Crea una superficie planare a partire da un piano e dalle estensioni rispetto l'origine
    ''' </summary>
    ''' <param name="plane"></param>
    ''' <param name="xMin"></param>
    ''' <param name="yMin"></param>
    ''' <param name="xMax"></param>
    ''' <param name="yMax"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function CreatePlanarSurface(ByVal plane As IOnPlane, ByVal xMin As Double, ByVal yMin As Double, ByVal xMax As Double, ByVal yMax As Double) As OnSurface
        If plane Is Nothing Then Return Nothing
        Dim planeCopy As New OnPlane(plane)
        Dim lineStart As On3dPoint = (planeCopy.xaxis * xMin) + plane.origin
        Dim lineEnd As On3dPoint = (planeCopy.xaxis * xMax) + plane.origin
        Dim lineCurve As New OnLineCurve(lineStart, lineEnd)
        Dim translation As On3dVector = planeCopy.yaxis * yMin
        lineCurve.Translate(translation)
        translation.Dispose()
        Dim result As New OnSumSurface
        result.Create(lineCurve, planeCopy.yaxis * (yMax - yMin))
        lineCurve.Dispose()
        result.SetDomain(0, xMin, xMax)
        result.SetDomain(1, yMin, yMax)
        lineEnd.Dispose()
        lineStart.Dispose()
        planeCopy.Dispose()
        Return result
    End Function


    ''' <summary>
    ''' Ritorna OnPlane da un piano del tipo MRhinoObjRef
    ''' </summary>
    ''' <param name="refPiano"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function RecuperaOnPlane(ByVal refPiano As MRhinoObjRef) As OnPlane
        Dim origin As New On3dPoint
        Dim du As New On3dVector
        Dim dv As New On3dVector
        If Not refPiano.Surface.Ev1Der(0, 0, origin, du, dv) Then Return Nothing
        Return New OnPlane(origin, du, dv)
    End Function


    ''' <summary>
    ''' Sposta l'estremo di una curva su un punto
    ''' </summary>
    ''' <param name="curva"></param>
    ''' <param name="punto"></param>
    ''' <param name="puntoTangenza"></param>
    ''' <param name="zonaRicercaTangenza">Lunghezza della zona di analisi per la ricerca della tangenza</param>
    ''' <param name="numeroCVAggiunti"></param>
    ''' <param name="zonaModifica">In percentuale la zona di modifica della curva</param>
    ''' <returns></returns>
    ''' <remarks>ALESSANDRO MACCHIONE - DA TESTARE RICHIAMA IL SUO OVERLOAD FATTO PER TCO</remarks>
    Public Shared Function SpostaEstremoCurvaSuPunto(ByRef curva As OnNurbsCurve, ByVal punto As IOn3dPoint, ByVal puntoTangenza As IOn3dPoint, ByVal zonaRicercaTangenza As Double, Optional ByVal numeroCVAggiunti As Integer = 0, Optional ByVal zonaModifica As Double = 0.0) As Boolean

        ''If curva.IsClosed Then Return False

        Const NUMERO_SUDDIVISIONI As Integer = 1000
        ''Const COS_MIN_TANGENTE As Double = 0.997        'Corrisponde ad un angolo di 4,44°

        '*** Recupera l'estremo della curva più vicino al punto ***
        Dim lunghezzaCurva As Double
        curva.GetLength(lunghezzaCurva)
        Dim delta As Double = curva.Domain.Length / NUMERO_SUDDIVISIONI
        Dim distPInizio As Double = curva.PointAtStart.DistanceTo(punto)
        Dim distPFine As Double = curva.PointAtEnd.DistanceTo(punto)
        Dim estremo As On3dPoint
        Dim tEstremo As Double
        Dim tZonaRicercaTangente As Double
        If (distPInizio < distPFine) Then
            estremo = curva.PointAtStart
            tEstremo = curva.Domain.Min
            curva.GetNormalizedArcLengthPoint(zonaRicercaTangenza / lunghezzaCurva, tZonaRicercaTangente)
            Return SpostaEstremoCurvaSuPunto(curva, eEstremo.startPoint, punto, puntoTangenza, zonaRicercaTangenza, numeroCVAggiunti, zonaModifica)
        Else
            estremo = curva.PointAtEnd
            tEstremo = curva.Domain.Max
            delta *= -1
            curva.GetNormalizedArcLengthPoint(1 - zonaRicercaTangenza / lunghezzaCurva, tZonaRicercaTangente)
            Return SpostaEstremoCurvaSuPunto(curva, eEstremo.endPoint, punto, puntoTangenza, zonaRicercaTangenza, numeroCVAggiunti, zonaModifica)
        End If



        ''*** Calcola il piano dove giace la curva e il punto proiezione ***
        'If puntoTangenza Is Nothing Then
        '    Dim intervallo As New OnInterval(tEstremo + 0.2 * (tZonaRicercaTangente - tEstremo), tZonaRicercaTangente)
        '    Dim piano As OnPlane = PianoOsculatoreMedio(curva, intervallo)
        '    puntoTangenza = piano.ClosestPointTo(punto)
        'End If

        ''### DEBUG 'RhUtil.RhinoApp.ActiveDoc.AddPointObject(curva.PointAt(tZonaRicercaTangente))
        ''### DEBUG 'RhUtil.RhinoApp.ActiveDoc.AddSurfaceObject(Rh.RhGeometry.VisualizzaPiano(piano, -10, -30, 10, 30))

        ''*** Calcola il punto di tangenza con il punto proiezione ***
        ''lineaTangente contiene un valore valido solo se la tangente viene individuata
        'Dim lineaTangente As New OnLine
        'Dim tTangenza As Double = Double.NaN
        'For i As Integer = 1 To NUMERO_SUDDIVISIONI
        '    Dim t As Double = tEstremo + i * delta
        '    If delta > 0 Then
        '        If t > tZonaRicercaTangente Then Exit For
        '    Else
        '        If t < tZonaRicercaTangente Then Exit For
        '    End If
        '    Dim tmpPunto As On3dPoint = curva.PointAt(t)
        '    Dim tangente As On3dVector = curva.TangentAt(t)
        '    If delta < 0 Then tangente *= -1
        '    lineaTangente.from = New On3dPoint(puntoTangenza)
        '    lineaTangente.to = tmpPunto
        '    If OnUtil.ON_DotProduct(tangente, lineaTangente.Tangent) > COS_MIN_TANGENTE Then
        '        tTangenza = t
        '        lineaTangente.from = New On3dPoint(punto)
        '        Exit For
        '    End If
        'Next

        ''Caso in cui la tangente non è stata trovata e la zona di modifica è nulla
        'If Double.IsNaN(tTangenza) Then
        '    If zonaModifica = 0 And numeroCVAggiunti > 0 Then
        '        tTangenza = tEstremo + delta
        '        lineaTangente.from = New On3dPoint(punto)
        '        lineaTangente.to = estremo
        '    End If
        'End If

        ''Correzione di tTangenza per il secondo e penultimo CV
        'If Not Double.IsNaN(tTangenza) Then
        '    If delta > 0 Then
        '        If tTangenza < curva.GrevilleAbcissa(1) Then tTangenza = curva.GrevilleAbcissa(1) + delta
        '    Else
        '        If tTangenza > curva.GrevilleAbcissa(curva.CVCount - 2) Then tTangenza = curva.GrevilleAbcissa(curva.CVCount - 2) + delta
        '    End If
        'End If


        ''*** Modifica la curva ***

        ''*** Il punto di tangenza non è stato trovato ***
        'If Double.IsNaN(tTangenza) Then

        '    'Sposta estremo mantenendo lo stesso numero di CV
        '    Dim CV As New On3dPoint
        '    Dim traslPuntoEstremo As On3dVector = New On3dPoint(punto) - estremo
        '    Dim tZonaInfluenza As Double
        '    If delta > 0 Then
        '        tZonaInfluenza = tEstremo + curva.Domain.Length * zonaModifica
        '    Else
        '        tZonaInfluenza = tEstremo - curva.Domain.Length * zonaModifica
        '    End If
        '    Dim tGreville As Double
        '    For i As Integer = 0 To curva.CVCount - 1
        '        tGreville = curva.GrevilleAbcissa(i)
        '        If delta > 0 Then
        '            Dim smorzamento As Double = (tZonaInfluenza - tGreville) / (tZonaInfluenza - tEstremo)
        '            If tGreville < tZonaInfluenza Then
        '                curva.GetCV(i, CV)
        '                CV += traslPuntoEstremo * smorzamento
        '                curva.SetCV(i, CV)
        '            End If
        '        Else
        '            Dim smorzamento As Double = (tZonaInfluenza - tGreville) / (tZonaInfluenza - tEstremo)
        '            If tGreville > tZonaInfluenza Then
        '                curva.GetCV(i, CV)
        '                CV += traslPuntoEstremo * smorzamento
        '                curva.SetCV(i, CV)
        '            End If
        '        End If
        '    Next

        '    'Aggiunta di CV 
        '    If numeroCVAggiunti > 0 Then

        '        'Calcolo suddivisioni zona di influenza
        '        Dim ultimoIndiceDaSpostare As Integer = -1
        '        Dim porzioneInfluenza As Double
        '        Dim suddivisioniInfluenza As Double
        '        If delta > 0 Then
        '            For i As Integer = 0 To curva.CVCount - 1
        '                If curva.GrevilleAbcissa(i) < tZonaInfluenza Then
        '                    ultimoIndiceDaSpostare = i
        '                Else
        '                    Exit For
        '                End If
        '            Next
        '            porzioneInfluenza = (tZonaInfluenza - curva.GrevilleAbcissa(ultimoIndiceDaSpostare)) / (curva.GrevilleAbcissa(ultimoIndiceDaSpostare + 1) - curva.GrevilleAbcissa(ultimoIndiceDaSpostare))
        '            suddivisioniInfluenza = porzioneInfluenza + ultimoIndiceDaSpostare + numeroCVAggiunti - 1
        '        Else
        '            For i As Integer = curva.CVCount - 1 To 0 Step -1
        '                If curva.GrevilleAbcissa(i) > tZonaInfluenza Then
        '                    ultimoIndiceDaSpostare = i
        '                Else
        '                    Exit For
        '                End If
        '            Next
        '            porzioneInfluenza = (tZonaInfluenza - curva.GrevilleAbcissa(ultimoIndiceDaSpostare)) / (curva.GrevilleAbcissa(ultimoIndiceDaSpostare - 1) - curva.GrevilleAbcissa(ultimoIndiceDaSpostare))
        '            suddivisioniInfluenza = porzioneInfluenza + curva.CVCount - 1 - ultimoIndiceDaSpostare + numeroCVAggiunti - 1
        '        End If

        '        'Crea punti
        '        Dim puntiInterpolanti As New On3dPointArray
        '        If delta > 0 Then
        '            puntiInterpolanti.Append(curva.PointAt(curva.GrevilleAbcissa(0)))
        '            For i As Integer = 1 To CInt(Int(suddivisioniInfluenza))
        '                puntiInterpolanti.Append(curva.PointAt(i * tZonaInfluenza / suddivisioniInfluenza))
        '            Next
        '            For i As Integer = ultimoIndiceDaSpostare + 1 To curva.CVCount - 2 - 1
        '                puntiInterpolanti.Append(curva.PointAt(curva.GrevilleAbcissa(i)))
        '            Next
        '            puntiInterpolanti.Append(curva.PointAt(curva.GrevilleAbcissa(curva.CVCount - 1)))

        '        Else
        '            puntiInterpolanti.Append(curva.PointAt(curva.GrevilleAbcissa(0)))
        '            For i As Integer = 2 To ultimoIndiceDaSpostare - 1
        '                puntiInterpolanti.Append(curva.PointAt(curva.GrevilleAbcissa(i)))
        '            Next
        '            For i As Integer = 1 To CInt(Int(suddivisioniInfluenza))
        '                Dim t As Double = tZonaInfluenza + i * (curva.Domain.m_t(1) - tZonaInfluenza) / suddivisioniInfluenza
        '                puntiInterpolanti.Append(curva.PointAt(t))
        '            Next
        '            puntiInterpolanti.Append(curva.PointAt(curva.GrevilleAbcissa(curva.CVCount - 1)))
        '        End If

        '        'Crea la nuova curva
        '        'Dim curvaCopia As OnNurbsCurve = RhUtil.RhinoInterpCurve(3, puntiInterpolanti, Nothing, Nothing, 1)
        '        Dim curvaCopia As OnNurbsCurve = RhUtil.RhinoInterpCurve(3, puntiInterpolanti, Nothing, Nothing, 0)
        '        puntiInterpolanti.Dispose()
        '        If delta > 0 Then
        '            For i As Integer = ultimoIndiceDaSpostare + 2 To curva.CVCount - 1
        '                curva.GetCV(i, CV)
        '                curvaCopia.SetCV(i + numeroCVAggiunti, CV)
        '            Next
        '        Else
        '            For i As Integer = 0 To ultimoIndiceDaSpostare - 2
        '                curva.GetCV(i, CV)
        '                curvaCopia.SetCV(i, CV)
        '            Next
        '        End If
        '        curva.Dispose()
        '        curva = curvaCopia
        '    End If


        '    '*** Il punto di tangenza è stato trovato ***
        'Else
        '    'Sposta sulla linea tangente mantenendo stessi CV
        '    If numeroCVAggiunti = 0 Then
        '        Dim tGreville As Double
        '        For i As Integer = 0 To curva.CVCount - 1
        '            tGreville = curva.GrevilleAbcissa(i)
        '            If delta > 0 Then
        '                If tGreville < tTangenza Then
        '                    Dim tLinea As Double = (tGreville - tEstremo) / (tTangenza - tEstremo)
        '                    curva.SetCV(i, lineaTangente.PointAt(tLinea))
        '                End If
        '            Else
        '                If tGreville > tTangenza Then
        '                    Dim tLinea As Double = (tGreville - tEstremo) / (tTangenza - tEstremo)
        '                    curva.SetCV(i, lineaTangente.PointAt(tLinea))
        '                End If
        '            End If
        '        Next

        '        'Sposta sulla linea tangente aggiungendo CV
        '    Else
        '        'Calcolo suddivisioni linea tangente
        '        Dim ultimoIndiceDaSpostare As Integer = -1
        '        Dim porzioneTangenza As Double
        '        Dim suddivisioniTangente As Double
        '        If delta > 0 Then
        '            For i As Integer = 0 To curva.CVCount - 1
        '                If curva.GrevilleAbcissa(i) < tTangenza Then
        '                    ultimoIndiceDaSpostare = i
        '                Else
        '                    Exit For
        '                End If
        '            Next
        '            porzioneTangenza = (tTangenza - curva.GrevilleAbcissa(ultimoIndiceDaSpostare)) / (curva.GrevilleAbcissa(ultimoIndiceDaSpostare + 1) - curva.GrevilleAbcissa(ultimoIndiceDaSpostare))
        '            suddivisioniTangente = porzioneTangenza + ultimoIndiceDaSpostare + numeroCVAggiunti - 1
        '        Else
        '            For i As Integer = curva.CVCount - 1 To 0 Step -1
        '                If curva.GrevilleAbcissa(i) > tTangenza Then
        '                    ultimoIndiceDaSpostare = i
        '                Else
        '                    Exit For
        '                End If
        '            Next
        '            porzioneTangenza = (tTangenza - curva.GrevilleAbcissa(ultimoIndiceDaSpostare)) / (curva.GrevilleAbcissa(ultimoIndiceDaSpostare - 1) - curva.GrevilleAbcissa(ultimoIndiceDaSpostare))
        '            suddivisioniTangente = porzioneTangenza + curva.CVCount - 1 - ultimoIndiceDaSpostare + numeroCVAggiunti - 1
        '        End If

        '        'Istanzia la nuova curva
        '        Dim nuoviCV As New On3dPointArray
        '        Dim CV As New On3dPoint
        '        If delta > 0 Then
        '            nuoviCV.Append(lineaTangente.PointAt(0))
        '            nuoviCV.Append(lineaTangente.PointAt(0.33 / suddivisioniTangente))
        '            For i As Integer = 1 To CInt(Int(suddivisioniTangente))
        '                nuoviCV.Append(lineaTangente.PointAt(i / suddivisioniTangente))
        '            Next
        '            For i As Integer = ultimoIndiceDaSpostare + 1 To curva.CVCount - 1
        '                curva.GetCV(i, CV)
        '                nuoviCV.Append(CV)
        '            Next

        '        Else
        '            For i As Integer = 0 To ultimoIndiceDaSpostare - 1
        '                curva.GetCV(i, CV)
        '                nuoviCV.Append(CV)
        '            Next
        '            For i As Integer = CInt(Int(suddivisioniTangente)) To 1 Step -1
        '                nuoviCV.Append(lineaTangente.PointAt(i / suddivisioniTangente))
        '            Next
        '            nuoviCV.Append(lineaTangente.PointAt(0.33 / suddivisioniTangente))
        '            nuoviCV.Append(lineaTangente.PointAt(0))
        '        End If
        '        curva.Dispose()
        '        curva = Rh.RhGeometry.CreaCurvaDaCV(nuoviCV)
        '        nuoviCV.Dispose()
        '    End If
        'End If

        Return True
    End Function


    ''' <summary>
    ''' Vedi metodo "SpostaEstremoCurvaSuPunto"
    ''' </summary>
    Public Enum eEstremo
        startPoint = 0
        endPoint
    End Enum

    ''' <summary>
    ''' Spostal'estremo scelto di una curva su un punto
    ''' </summary>
    ''' <param name="curva"></param>
    ''' <param name="estremo">Enumerativo che specifica se spostare il punto iniziale o finale della curva</param>
    ''' <param name="nuovoPunto"></param>
    ''' <param name="puntoTangenza"></param>
    ''' <param name="zonaRicercaTangenza">Lunghezza della zona di analisi per la ricerca della tangenza</param>
    ''' <param name="numeroCVAggiunti"></param>
    ''' <param name="zonaModifica">In percentuale la zona di modifica della curva</param>
    ''' <returns></returns>
    Public Shared Function SpostaEstremoCurvaSuPunto(ByRef curva As OnNurbsCurve, ByVal estremo As eEstremo, ByVal nuovoPunto As IOn3dPoint, ByVal puntoTangenza As IOn3dPoint, ByVal zonaRicercaTangenza As Double, Optional ByVal numeroCVAggiunti As Integer = 0, Optional ByVal zonaModifica As Double = 0.0) As Boolean

        If curva.IsClosed Then Return False

        Const NUMERO_SUDDIVISIONI As Integer = 1000
        Const COS_MIN_TANGENTE As Double = 0.997        'Corrisponde ad un angolo di 4,44°

        '*** Recupera l'estremo della curva in base al tipo passato ***

        Dim puntoOriginale As On3dPoint
        Dim tPuntoOriginale As Double
        Dim lunghezzaCurva As Double
        curva.GetLength(lunghezzaCurva)
        Dim delta As Double = curva.Domain.Length / NUMERO_SUDDIVISIONI
        Dim distPInizio As Double = curva.PointAtStart.DistanceTo(nuovoPunto)
        Dim distPFine As Double = curva.PointAtEnd.DistanceTo(nuovoPunto)

        Dim tZonaRicercaTangente As Double
        If (estremo = eEstremo.startPoint) Then
            puntoOriginale = curva.PointAtStart
            tPuntoOriginale = curva.Domain.Min
            curva.GetNormalizedArcLengthPoint(zonaRicercaTangenza / lunghezzaCurva, tZonaRicercaTangente)
        Else
            puntoOriginale = curva.PointAtEnd
            tPuntoOriginale = curva.Domain.Max
            delta *= -1
            curva.GetNormalizedArcLengthPoint(1 - zonaRicercaTangenza / lunghezzaCurva, tZonaRicercaTangente)
        End If


        '*** Calcola il piano dove giace la curva e il punto proiezione ***
        If puntoTangenza Is Nothing Then
            Dim intervallo As New OnInterval(tPuntoOriginale + 0.2 * (tZonaRicercaTangente - tPuntoOriginale), tZonaRicercaTangente)
            Dim piano As OnPlane = PianoOsculatoreMedio(curva, intervallo)
            puntoTangenza = piano.ClosestPointTo(nuovoPunto)
        End If

        '### DEBUG 'RhUtil.RhinoApp.ActiveDoc.AddPointObject(curva.PointAt(tZonaRicercaTangente))
        '### DEBUG 'RhUtil.RhinoApp.ActiveDoc.AddSurfaceObject(Rh.RhGeometry.VisualizzaPiano(piano, -10, -30, 10, 30))

        '*** Calcola il punto di tangenza con il punto proiezione ***
        'lineaTangente contiene un valore valido solo se la tangente viene individuata
        Dim lineaTangente As New OnLine
        Dim tTangenza As Double = Double.NaN
        For i As Integer = 1 To NUMERO_SUDDIVISIONI
            Dim t As Double = tPuntoOriginale + i * delta
            If delta > 0 Then
                If t > tZonaRicercaTangente Then Exit For
            Else
                If t < tZonaRicercaTangente Then Exit For
            End If
            Dim tmpPunto As On3dPoint = curva.PointAt(t)
            Dim tangente As On3dVector = curva.TangentAt(t)
            If delta < 0 Then tangente *= -1
            lineaTangente.from = New On3dPoint(puntoTangenza)
            lineaTangente.to = tmpPunto
            If OnUtil.ON_DotProduct(tangente, lineaTangente.Tangent) > COS_MIN_TANGENTE Then
                tTangenza = t
                lineaTangente.from = New On3dPoint(nuovoPunto)
                Exit For
            End If
        Next

        'Caso in cui la tangente non è stata trovata e la zona di modifica è nulla
        If Double.IsNaN(tTangenza) Then
            If zonaModifica = 0 And numeroCVAggiunti > 0 Then
                tTangenza = tPuntoOriginale + delta
                lineaTangente.from = New On3dPoint(nuovoPunto)
                lineaTangente.to = puntoOriginale
            End If
        End If

        'Correzione di tTangenza per il secondo e penultimo CV
        If Not Double.IsNaN(tTangenza) Then
            If delta > 0 Then
                If tTangenza < curva.GrevilleAbcissa(1) Then tTangenza = curva.GrevilleAbcissa(1) + delta
            Else
                If tTangenza > curva.GrevilleAbcissa(curva.CVCount - 2) Then tTangenza = curva.GrevilleAbcissa(curva.CVCount - 2) + delta
            End If
        End If

        '*** Modifica la curva ***

        '*** Il punto di tangenza non è stato trovato ***
        If Double.IsNaN(tTangenza) Then

            'Sposta estremo mantenendo lo stesso numero di CV
            Dim CV As New On3dPoint
            Dim traslPuntoEstremo As On3dVector = New On3dPoint(nuovoPunto) - puntoOriginale
            Dim tZonaInfluenza As Double
            If delta > 0 Then
                tZonaInfluenza = tPuntoOriginale + curva.Domain.Length * zonaModifica
            Else
                tZonaInfluenza = tPuntoOriginale - curva.Domain.Length * zonaModifica
            End If
            Dim tGreville As Double
            For i As Integer = 0 To curva.CVCount - 1
                tGreville = curva.GrevilleAbcissa(i)
                If delta > 0 Then
                    Dim smorzamento As Double = (tZonaInfluenza - tGreville) / (tZonaInfluenza - tPuntoOriginale)
                    If tGreville < tZonaInfluenza Then
                        curva.GetCV(i, CV)
                        CV += traslPuntoEstremo * smorzamento
                        curva.SetCV(i, CV)
                    End If
                Else
                    Dim smorzamento As Double = (tZonaInfluenza - tGreville) / (tZonaInfluenza - tPuntoOriginale)
                    If tGreville > tZonaInfluenza Then
                        curva.GetCV(i, CV)
                        CV += traslPuntoEstremo * smorzamento
                        curva.SetCV(i, CV)
                    End If
                End If
            Next
            'Aggiunta di CV 
            If numeroCVAggiunti > 0 Then
                'Calcolo suddivisioni zona di influenza
                Dim ultimoIndiceDaSpostare As Integer = -1
                Dim porzioneInfluenza As Double
                Dim suddivisioniInfluenza As Double
                If delta > 0 Then
                    For i As Integer = 0 To curva.CVCount - 1
                        If curva.GrevilleAbcissa(i) < tZonaInfluenza Then
                            ultimoIndiceDaSpostare = i
                        Else
                            Exit For
                        End If
                    Next
                    porzioneInfluenza = (tZonaInfluenza - curva.GrevilleAbcissa(ultimoIndiceDaSpostare)) / (curva.GrevilleAbcissa(ultimoIndiceDaSpostare + 1) - curva.GrevilleAbcissa(ultimoIndiceDaSpostare))
                    suddivisioniInfluenza = porzioneInfluenza + ultimoIndiceDaSpostare + numeroCVAggiunti - 1
                Else
                    For i As Integer = curva.CVCount - 1 To 0 Step -1
                        If curva.GrevilleAbcissa(i) > tZonaInfluenza Then
                            ultimoIndiceDaSpostare = i
                        Else
                            Exit For
                        End If
                    Next
                    porzioneInfluenza = (tZonaInfluenza - curva.GrevilleAbcissa(ultimoIndiceDaSpostare)) / (curva.GrevilleAbcissa(ultimoIndiceDaSpostare - 1) - curva.GrevilleAbcissa(ultimoIndiceDaSpostare))
                    suddivisioniInfluenza = porzioneInfluenza + curva.CVCount - 1 - ultimoIndiceDaSpostare + numeroCVAggiunti - 1
                End If
                'Crea punti
                Dim puntiInterpolanti As New On3dPointArray
                If delta > 0 Then
                    puntiInterpolanti.Append(curva.PointAt(curva.GrevilleAbcissa(0)))
                    For i As Integer = 1 To CInt(Int(suddivisioniInfluenza))
                        puntiInterpolanti.Append(curva.PointAt(i * tZonaInfluenza / suddivisioniInfluenza))
                    Next
                    For i As Integer = ultimoIndiceDaSpostare + 1 To curva.CVCount - 2 - 1
                        puntiInterpolanti.Append(curva.PointAt(curva.GrevilleAbcissa(i)))
                    Next
                    puntiInterpolanti.Append(curva.PointAt(curva.GrevilleAbcissa(curva.CVCount - 1)))

                Else
                    puntiInterpolanti.Append(curva.PointAt(curva.GrevilleAbcissa(0)))
                    For i As Integer = 2 To ultimoIndiceDaSpostare - 1
                        puntiInterpolanti.Append(curva.PointAt(curva.GrevilleAbcissa(i)))
                    Next
                    For i As Integer = 1 To CInt(Int(suddivisioniInfluenza))
                        Dim t As Double = tZonaInfluenza + i * (curva.Domain.m_t(1) - tZonaInfluenza) / suddivisioniInfluenza
                        puntiInterpolanti.Append(curva.PointAt(t))
                    Next
                    puntiInterpolanti.Append(curva.PointAt(curva.GrevilleAbcissa(curva.CVCount - 1)))
                End If

                'Crea la nuova curva
                'Dim curvaCopia As OnNurbsCurve = RhUtil.RhinoInterpCurve(3, puntiInterpolanti, Nothing, Nothing, 1)
                Dim curvaCopia As OnNurbsCurve = RhUtil.RhinoInterpCurve(3, puntiInterpolanti, Nothing, Nothing, 0)
                puntiInterpolanti.Dispose()
                If delta > 0 Then
                    For i As Integer = ultimoIndiceDaSpostare + 2 To curva.CVCount - 1
                        curva.GetCV(i, CV)
                        curvaCopia.SetCV(i + numeroCVAggiunti, CV)
                    Next
                Else
                    For i As Integer = 0 To ultimoIndiceDaSpostare - 2
                        curva.GetCV(i, CV)
                        curvaCopia.SetCV(i, CV)
                    Next
                End If
                curva.Dispose()
                curva = curvaCopia
            End If
            '*** Il punto di tangenza è stato trovato ***
        Else
            'Sposta sulla linea tangente mantenendo stessi CV
            If numeroCVAggiunti = 0 Then
                Dim tGreville As Double
                For i As Integer = 0 To curva.CVCount - 1
                    tGreville = curva.GrevilleAbcissa(i)
                    If delta > 0 Then
                        If tGreville < tTangenza Then
                            Dim tLinea As Double = (tGreville - tPuntoOriginale) / (tTangenza - tPuntoOriginale)
                            curva.SetCV(i, lineaTangente.PointAt(tLinea))
                        End If
                    Else
                        If tGreville > tTangenza Then
                            Dim tLinea As Double = (tGreville - tPuntoOriginale) / (tTangenza - tPuntoOriginale)
                            curva.SetCV(i, lineaTangente.PointAt(tLinea))
                        End If
                    End If
                Next

                'Sposta sulla linea tangente aggiungendo CV
            Else
                'Calcolo suddivisioni linea tangente
                Dim ultimoIndiceDaSpostare As Integer = -1
                Dim porzioneTangenza As Double
                Dim suddivisioniTangente As Double
                If delta > 0 Then
                    For i As Integer = 0 To curva.CVCount - 1
                        If curva.GrevilleAbcissa(i) < tTangenza Then
                            ultimoIndiceDaSpostare = i
                        Else
                            Exit For
                        End If
                    Next
                    porzioneTangenza = (tTangenza - curva.GrevilleAbcissa(ultimoIndiceDaSpostare)) / (curva.GrevilleAbcissa(ultimoIndiceDaSpostare + 1) - curva.GrevilleAbcissa(ultimoIndiceDaSpostare))
                    suddivisioniTangente = porzioneTangenza + ultimoIndiceDaSpostare + numeroCVAggiunti - 1
                Else
                    For i As Integer = curva.CVCount - 1 To 0 Step -1
                        If curva.GrevilleAbcissa(i) > tTangenza Then
                            ultimoIndiceDaSpostare = i
                        Else
                            Exit For
                        End If
                    Next
                    porzioneTangenza = (tTangenza - curva.GrevilleAbcissa(ultimoIndiceDaSpostare)) / (curva.GrevilleAbcissa(ultimoIndiceDaSpostare - 1) - curva.GrevilleAbcissa(ultimoIndiceDaSpostare))
                    suddivisioniTangente = porzioneTangenza + curva.CVCount - 1 - ultimoIndiceDaSpostare + numeroCVAggiunti - 1
                End If

                'Istanzia la nuova curva
                Dim nuoviCV As New On3dPointArray
                Dim CV As New On3dPoint
                If delta > 0 Then
                    nuoviCV.Append(lineaTangente.PointAt(0))
                    nuoviCV.Append(lineaTangente.PointAt(0.33 / suddivisioniTangente))
                    For i As Integer = 1 To CInt(Int(suddivisioniTangente))
                        nuoviCV.Append(lineaTangente.PointAt(i / suddivisioniTangente))
                    Next
                    For i As Integer = ultimoIndiceDaSpostare + 1 To curva.CVCount - 1
                        curva.GetCV(i, CV)
                        nuoviCV.Append(CV)
                    Next

                Else
                    For i As Integer = 0 To ultimoIndiceDaSpostare - 1
                        curva.GetCV(i, CV)
                        nuoviCV.Append(CV)
                    Next
                    For i As Integer = CInt(Int(suddivisioniTangente)) To 1 Step -1
                        nuoviCV.Append(lineaTangente.PointAt(i / suddivisioniTangente))
                    Next
                    nuoviCV.Append(lineaTangente.PointAt(0.33 / suddivisioniTangente))
                    nuoviCV.Append(lineaTangente.PointAt(0))
                End If
                curva.Dispose()
                curva = RhGeometry.CreaCurvaDaCV(nuoviCV)
                nuoviCV.Dispose()
            End If
        End If
        Return True
    End Function


    ''' <summary>
    ''' Calcola il piano osculatore mediando il calcolo nell'intervallo specificato
    ''' </summary>
    ''' <param name="curva"></param>
    ''' <param name="dominio"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function PianoOsculatoreMedio(ByVal curva As IOnCurve, ByVal dominio As IOnInterval) As OnPlane
        If curva Is Nothing Then Return Nothing
        If dominio Is Nothing Then Return Nothing

        Dim suddivisioni As Integer = 10
        Dim pianoBase As New OnPlane
        If Not curva.FrameAt(dominio.Mid, pianoBase) Then
            Return Nothing
        End If
        Dim normaleComune As New On3dVector(pianoBase.Normal)
        Dim xComune As New On3dVector(pianoBase.xaxis)
        For i As Integer = 0 To suddivisioni - 1
            Dim piano As New OnPlane
            If Not curva.FrameAt(dominio.m_t(0) + i * dominio.Length / suddivisioni, piano) Then
                Return Nothing
            End If
            If OnUtil.ON_DotProduct(normaleComune, piano.Normal) > 0 Then
                normaleComune += piano.Normal
                xComune += piano.xaxis
            Else
                normaleComune -= piano.Normal
                xComune -= piano.xaxis
            End If
            piano.Dispose()
        Next
        normaleComune.Unitize()
        xComune.Unitize()
        Dim yComune As On3dVector = OnUtil.ON_CrossProduct(normaleComune, xComune)
        Dim res As OnPlane = New OnPlane(curva.PointAt(dominio.Mid), xComune, yComune)
        Return res
    End Function



    ''' <summary>
    ''' Esegue il rebuild di Rhino
    ''' </summary>
    ''' <param name="curva"></param>
    ''' <param name="numeroDiPunti"></param>
    ''' <param name="grado"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function EsegueRebuild(ByVal curva As IOnCurve, ByVal numeroDiPunti As Integer, ByVal grado As Integer) As IOnCurve
        Dim curveObj As MRhinoObject = RhUtil.RhinoApp.ActiveDoc.AddCurveObject(curva)
        Dim curveObjRef As New MRhinoObjRef(curveObj)
        Dim result As IOnCurve = EsegueRebuild(curveObjRef, numeroDiPunti, grado)
        curveObjRef.Dispose()
        curveObj.Dispose()
        Return result
    End Function


    ''' <summary>
    ''' Esegue il rebuild di Rhino
    ''' </summary>
    ''' <param name="curvaObjRef"></param>
    ''' <param name="numeroDiPunti"></param>
    ''' <param name="grado"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function EsegueRebuild(ByVal curvaObjRef As MRhinoObjRef, ByVal numeroDiPunti As Integer, ByVal grado As Integer) As IOnCurve
        curvaObjRef.Object.Select(True, False, True, True, True, True)
        RhUtil.RhinoApp.RunScript("-_Rebuild _p=" & numeroDiPunti & "  _d=" & grado & " _e=y _c=n _enter", 0)
        curvaObjRef.Dispose()

        Dim objRef As MRhinoObjRef = Nothing
        Dim getObj As New MRhinoGetObject
        getObj.GetObjects(0, 0)
        Select Case getObj.ObjectCount
            Case 0
                'Return Nothing
            Case 1
                objRef = getObj.Object(0)
            Case Else
                'Return Nothing
        End Select
        getObj.Dispose()

        If objRef Is Nothing Then
            Return Nothing
        Else
            Dim result As IOnCurve = objRef.Curve
            RhUtil.RhinoApp.ActiveDoc.DeleteObject(objRef)
            objRef.Dispose()
            Return result
        End If
    End Function



    ''' <summary>
    ''' EsegueFitCurva
    ''' </summary>
    ''' <param name="curvaObjRef"></param>
    ''' <param name="tolleranza"></param>
    ''' <param name="grado"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function EsegueFitCurva(ByVal curvaObjRef As MRhinoObjRef, ByVal tolleranza As Double, ByVal grado As Integer) As OnCurve
        Dim strTolleranza As String = ""
        RhUtil.RhinoFormatNumber(tolleranza, strTolleranza)

        curvaObjRef.Object.Select(True, False, True, True, True, True)
        RhUtil.RhinoApp.RunScript("-_FitCrv _d=y _e=" & grado & " " & strTolleranza & " _enter", 0)
        curvaObjRef.Dispose()

        Dim objRef As MRhinoObjRef = Nothing
        Dim getObj As New MRhinoGetObject
        getObj.GetObjects(0, 0)
        Select Case getObj.ObjectCount
            Case 0
                'Return Nothing
            Case 1
                objRef = getObj.Object(0)
            Case Else
                'Return Nothing
        End Select
        getObj.Dispose()

        If objRef Is Nothing Then
            Return Nothing
        Else
            Dim result As OnCurve = objRef.Curve.NurbsCurve
            RhUtil.RhinoApp.ActiveDoc.DeleteObject(objRef)
            objRef.Dispose()
            Return result
        End If
    End Function


    ''' <summary>
    ''' EsegueFitCurva
    ''' </summary>
    ''' <param name="curva"></param>
    ''' <param name="tolleranza"></param>
    ''' <param name="grado"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function EsegueFitCurva(ByVal curva As IOnCurve, ByVal tolleranza As Double, ByVal grado As Integer) As OnCurve
        Dim curveObj As MRhinoObject = RhUtil.RhinoApp.ActiveDoc.AddCurveObject(curva)
        Dim curveObjRef As New MRhinoObjRef(curveObj)
        Dim result As OnCurve = EsegueFitCurva(curveObjRef, tolleranza, grado)
        curveObjRef.Dispose()
        curveObj.Dispose()
        Return result
    End Function


    ''' <summary>
    ''' EseguiFairCurva
    ''' </summary>
    ''' <param name="curva"></param>
    ''' <param name="tolleranza"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function EseguiFairCurva(ByVal curva As IOnCurve, ByVal tolleranza As Double) As OnCurve
        Dim arg As New MArgsRhinoFair
        arg.SetInputCurve(curva)
        arg.SetTolerance(tolleranza)
        'arg.SetIterationCount(5)
        Dim curvaResult As OnCurve = RhUtil.RhinoFairCurve(arg)
        'Caso incontrato con una polilinea con due punti di controllo(curva dritta)
        If curvaResult Is Nothing Then Return curva.NurbsCurve
        arg.Dispose()
        Return curvaResult.NurbsCurve
    End Function



    'Return euler orientation angles by X, Y and Z order
    Public Shared Function EulerAnglesXYZ(ByVal plane As IOnPlane) As Double()
        If plane Is Nothing Then Return Nothing
        If Not plane.IsValid Then Return Nothing

        'Z rotation
        Dim anglesXYZ(2) As Double
        Dim planeCopy As New OnPlane(plane)
        If planeCopy.xaxis.IsParallelTo(OnPlane.World_xy.zaxis) <> 0 Then
            anglesXYZ(2) = Math.Atan2(-planeCopy.yaxis.x, planeCopy.yaxis.y)
        Else
            Dim projectedXAxis As New On3dVector(planeCopy.xaxis.x, planeCopy.xaxis.y, 0)
            projectedXAxis.Unitize()
            anglesXYZ(2) = Math.Atan2(projectedXAxis.y, projectedXAxis.x)
            projectedXAxis.Dispose()
        End If
        Dim rotation As New OnXform
        rotation.Rotation(-anglesXYZ(2), OnPlane.World_xy.zaxis, OnPlane.World_xy.origin)
        planeCopy.Transform(rotation)

        'Y rotation
        anglesXYZ(1) = Math.Atan2(-planeCopy.xaxis.z, planeCopy.xaxis.x)
        rotation.Rotation(-anglesXYZ(1), OnPlane.World_xy.yaxis, OnPlane.World_xy.origin)
        planeCopy.Transform(rotation)
        rotation.Dispose()

        'X rotation
        anglesXYZ(0) = Math.Atan2(planeCopy.yaxis.z, planeCopy.yaxis.y)
        planeCopy.Dispose()

        Return anglesXYZ
    End Function


    'Return euler orientation angles by Z, Y and X order
    Public Shared Function EulerAnglesZYX(ByVal plane As IOnPlane) As Double()
        If plane Is Nothing Then Return Nothing
        If Not plane.IsValid Then Return Nothing

        'X rotation
        Dim anglesZYX(2) As Double
        Dim planeCopy As New OnPlane(plane)
        If planeCopy.zaxis.IsParallelTo(OnPlane.World_xy.xaxis) <> 0 Then
            anglesZYX(2) = Math.Atan2(planeCopy.yaxis.z, planeCopy.yaxis.x)
        Else
            Dim projectedZAxis As New On3dVector(0, planeCopy.zaxis.y, planeCopy.zaxis.z)
            projectedZAxis.Unitize()
            anglesZYX(2) = Math.Atan2(-projectedZAxis.y, projectedZAxis.z)
            projectedZAxis.Dispose()
        End If
        Dim rotation As New OnXform
        rotation.Rotation(-anglesZYX(2), OnPlane.World_xy.xaxis, OnPlane.World_xy.origin)
        planeCopy.Transform(rotation)

        'Y rotation
        anglesZYX(1) = -Math.Atan2(-planeCopy.zaxis.x, planeCopy.zaxis.z)
        rotation.Rotation(-anglesZYX(1), OnPlane.World_xy.yaxis, OnPlane.World_xy.origin)
        planeCopy.Transform(rotation)
        rotation.Dispose()

        'Z rotation
        anglesZYX(0) = Math.Atan2(planeCopy.xaxis.y, planeCopy.xaxis.x)
        planeCopy.Dispose()

        Return anglesZYX
    End Function



    ''' <summary>
    ''' Compute a plane with optimal orientation approximating a whole brepFace or a portion of it
    ''' </summary>
    ''' <param name="brepFace">Face to be approximate</param>
    ''' <param name="plane">Output plane</param>
    ''' <param name="subUInterval">Restrict interpolation to surface U subdomain</param>
    ''' <param name="subVInterval">Restrict interpolation to surface V subdomain</param>
    ''' <param name="minSubdivisionCount">Minimum U and V subdivision count for interpolating points</param>
    ''' <param name="maxSubdivisionCount">Maximum U and V subdivision count for interpolating points</param>
    ''' <param name="sampledPoints">Output array of points used for interpolation</param>
    ''' <param name="sampledNormals">Output array of normals at sampledPoints used for interpolation</param>
    ''' <param name="optimizeRotation">Decide if rotating the plane of PI/2 to align bounding box to x axis</param>
    ''' <returns>true if successiful</returns>
    Public Shared Function GetFaceInterpolantPlane(ByVal brepFace As IOnBrepFace, ByRef plane As OnPlane, Optional ByVal subUInterval As IOnInterval = Nothing, Optional ByVal subVInterval As IOnInterval = Nothing, Optional ByVal minSubdivisionCount As Integer = 0, Optional ByVal maxSubdivisionCount As Integer = Integer.MaxValue, Optional ByRef sampledPoints As On3dPointArray = Nothing, Optional ByRef sampledNormals As On3dVectorArray = Nothing, Optional ByVal optimizeRotation As Boolean = True) As Boolean
        If brepFace Is Nothing Then Return False

        'Create border loop
        Dim outerLoop As OnBrepLoop = brepFace.OuterLoop
        Dim outerLoopCurve As New OnPolyCurve(outerLoop.TrimCount)
        For i As Integer = 0 To outerLoop.TrimCount - 1
            outerLoopCurve.Append(outerLoop.Trim(i).DuplicateCurve)
        Next

        'Create inner loops array
        Dim innerLoopCurve() As OnPolyCurve = Nothing
        For i As Integer = 1 To brepFace.LoopCount - 1
            If brepFace.Loop(i).m_type = IOnBrepLoop.TYPE.inner Then
                If innerLoopCurve Is Nothing Then
                    ReDim innerLoopCurve(0)
                    innerLoopCurve(0) = New OnPolyCurve(brepFace.Loop(i).TrimCount)
                Else
                    ReDim Preserve innerLoopCurve(innerLoopCurve.GetUpperBound(0) + 1)
                    innerLoopCurve(innerLoopCurve.GetUpperBound(0)) = New OnPolyCurve(brepFace.Loop(i).TrimCount)
                End If
                For j As Integer = 0 To brepFace.Loop(i).TrimCount - 1
                    innerLoopCurve(innerLoopCurve.GetUpperBound(0)).Append(brepFace.Loop(i).Trim(j).DuplicateCurve)
                Next
            End If
        Next

        'Compute optimal surface subdivisions
        If subUInterval Is Nothing Then subUInterval = New OnInterval(outerLoop.m_pbox.Min.x, outerLoop.m_pbox.Max.x)
        If subVInterval Is Nothing Then subVInterval = New OnInterval(outerLoop.m_pbox.Min.y, outerLoop.m_pbox.Max.y)
        outerLoop.Dispose()
        Dim nurbsSurface As OnNurbsSurface = brepFace.NurbsSurface
        Dim subdivisionsUCount As Integer = 0
        Dim subdivisionsVCount As Integer = 0
        Dim gu() As Double = Nothing
        Dim gv() As Double = Nothing
        nurbsSurface.GetGrevilleAbcissae(0, gu)
        nurbsSurface.GetGrevilleAbcissae(1, gv)
        For i As Integer = 0 To gu.GetUpperBound(0)
            If subUInterval.Includes(gu(i)) Then subdivisionsUCount += 1
        Next
        If subdivisionsUCount < minSubdivisionCount Then subdivisionsUCount = minSubdivisionCount
        If subdivisionsUCount > maxSubdivisionCount Then subdivisionsUCount = maxSubdivisionCount
        For i As Integer = 0 To gv.GetUpperBound(0)
            If subVInterval.Includes(gv(i)) Then subdivisionsVCount += 1
        Next
        If subdivisionsVCount < minSubdivisionCount Then subdivisionsVCount = minSubdivisionCount
        If subdivisionsVCount > maxSubdivisionCount Then subdivisionsVCount = maxSubdivisionCount

        'Compute mid point and mean normal
        Dim tolerance As Double = subUInterval.Length / 100
        If subVInterval.Length / 100 < tolerance Then tolerance = subVInterval.Length / 100
        Dim pointArray As New On3dPointArray
        Dim normalArray As New On3dVectorArray
        Dim point As New On3dPoint
        Dim midPoint As New On3dPoint(0, 0, 0)
        Dim du As New On3dVector
        Dim dv As New On3dVector
        Dim meanDu As New On3dVector(0, 0, 0)
        Dim meanDv As New On3dVector(0, 0, 0)
        Dim meanNormal As New On3dVector(0, 0, 0)
        Dim pointCount As Integer = 0
        For i As Integer = 0 To subdivisionsUCount
            Dim u As Double = subUInterval.m_t(0) + i * subUInterval.Length / subdivisionsUCount
            For j As Integer = 0 To subdivisionsVCount
                Dim v As Double = subVInterval.m_t(0) + j * subVInterval.Length / subdivisionsVCount
                Dim point2D As New On3dPoint(u, v, 0)
                If RhUtil.RhinoPointInPlanarClosedCurve(point2D, outerLoopCurve, OnPlane.World_xy, tolerance) > 0 Then
                    Dim isPointOutDomain As Boolean = False
                    If Not innerLoopCurve Is Nothing Then
                        For z As Integer = 0 To innerLoopCurve.GetUpperBound(0)
                            If RhUtil.RhinoPointInPlanarClosedCurve(point2D, innerLoopCurve(z), OnPlane.World_xy, tolerance) > 0 Then
                                isPointOutDomain = True
                                Exit For
                            End If
                        Next
                    End If
                    If Not isPointOutDomain Then
                        nurbsSurface.Ev1Der(u, v, point, du, dv)
                        midPoint += point
                        pointArray.Append(point)
                        du.Unitize()
                        dv.Unitize()
                        If brepFace.m_bRev Then dv.Reverse()
                        normalArray.Append(OnUtil.ON_CrossProduct(du, dv))
                        meanNormal += normalArray.Last
                        meanDu += du
                        meanDv += dv
                        pointCount += 1
                    End If
                End If
            Next
        Next
        If pointCount = 0 Then Return False

        'Compute mean values
        midPoint *= 1 / pointCount
        meanNormal.Unitize()
        meanDu.Unitize()
        meanDv.Unitize()

        'Create output plane
        If Not plane Is Nothing Then plane.Dispose()
        Dim dirY As On3dVector = OnUtil.ON_CrossProduct(meanNormal, meanDu)
        Dim result As New OnPlane(midPoint, meanDu, dirY)
        Dim crossProduct As On3dVector = OnUtil.ON_CrossProduct(dirY, meanDv)
        Dim sinCorrectionAngle As Double = crossProduct.Length
        crossProduct.Dispose()
        result.Rotate(-Math.Asin(sinCorrectionAngle) / 2, result.Normal)

        'Optimise rotation on bounding box
        Dim xFormWorldToLocal As New OnXform
        xFormWorldToLocal.ChangeBasis(OnPlane.World_xy, result)
        Dim bbox As New OnBoundingBox
        pointArray.GetTightBoundingBox(bbox, 0, xFormWorldToLocal)
        xFormWorldToLocal.Dispose()
        Dim bboxCenter As On3dPoint = bbox.Center
        bboxCenter.z = 0
        Dim newOrigin As On3dPoint = RhCoordinates.CoordinateLocalToWorld(result, bboxCenter)
        result.SetOrigin(newOrigin)
        newOrigin.Dispose()
        bboxCenter.Dispose()
        If optimizeRotation Then
            If (bbox.m_max.x - bbox.m_min.x) < (bbox.m_max.y - bbox.m_min.y) Then
                result.Rotate(Math.PI / 2, result.zaxis)
            End If
        End If
        bbox.Dispose()
        dirY.Dispose()

        'Exiting and disposing
        If Not sampledPoints Is Nothing Then
            sampledPoints.Dispose()
            sampledPoints = pointArray
        Else
            pointArray.Dispose()
        End If
        If Not sampledNormals Is Nothing Then
            sampledNormals.Dispose()
            sampledNormals = normalArray
        Else
            normalArray.Dispose()
        End If
        plane = result
        nurbsSurface.Dispose()
        outerLoopCurve.Dispose()
        If Not innerLoopCurve Is Nothing Then
            For z As Integer = 0 To innerLoopCurve.GetUpperBound(0)
                innerLoopCurve(z).Dispose()
            Next
        End If

        Return True
    End Function


    Public Shared Function IsPointContainedInTrimmedDomain(ByVal brepFace As IOnBrepFace, ByVal s As Double, ByVal t As Double) As Boolean
        If brepFace Is Nothing Then Return False

        'Create border loop
        Dim outerLoop As OnBrepLoop = brepFace.OuterLoop
        Dim outerLoopCurve As New OnPolyCurve(outerLoop.TrimCount)
        For i As Integer = 0 To outerLoop.TrimCount - 1
            outerLoopCurve.Append(outerLoop.Trim(i).DuplicateCurve)
        Next

        'Create inner loops list
        Dim innerLoopCurves As New List(Of OnPolyCurve)
        For i As Integer = 1 To brepFace.LoopCount - 1
            If brepFace.Loop(i).m_type = IOnBrepLoop.TYPE.inner Then
                Dim innerLoopCurve As New OnPolyCurve(brepFace.Loop(i).TrimCount)
                For j As Integer = 0 To brepFace.Loop(i).TrimCount - 1
                    innerLoopCurve.Append(brepFace.Loop(i).Trim(j).DuplicateCurve)
                Next
                innerLoopCurves.Add(innerLoopCurve)
            End If
        Next

        'Check point
        Dim point2D As New On3dPoint(s, t, 0)
        Dim result As Boolean = False
        If RhUtil.RhinoPointInPlanarClosedCurve(point2D, outerLoopCurve, OnPlane.World_xy) > 0 Then
            result = True
            For z As Integer = 0 To innerLoopCurves.Count - 1
                If RhUtil.RhinoPointInPlanarClosedCurve(point2D, innerLoopCurves(z), OnPlane.World_xy) > 0 Then
                    result = False
                    Exit For
                End If
            Next
        End If

        'Exiting
        outerLoopCurve.Dispose()
        For i As Integer = 0 To innerLoopCurves.Count - 1
            innerLoopCurves(i).Dispose()
        Next
        point2D.Dispose()

        Return result
    End Function



    ''' <summary>
    ''' Get a wireframe outline of a viewport frustum
    ''' </summary>
    ''' <param name="viewport"></param>
    ''' <param name="lines"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function GetViewportOutline(ByVal viewport As IOnViewport, ByRef lines As List(Of OnLine), ByVal showOrigin As Boolean, ByVal showNearPlane As Boolean) As Boolean
        If viewport Is Nothing Then Return False
        If lines Is Nothing Then lines = New List(Of OnLine)

        'Camera location
        If showOrigin Then
            Dim locationCamera As New On3dPoint
            Dim xCamera As New On3dVector
            Dim yCamera As New On3dVector
            Dim zCamera As New On3dVector
            viewport.GetCameraFrame(locationCamera, xCamera, yCamera, zCamera)
            lines.Add(New OnLine(locationCamera, locationCamera + xCamera * 10))
            lines.Add(New OnLine(locationCamera, locationCamera + yCamera * 20))
            lines.Add(New OnLine(locationCamera, locationCamera + zCamera * 30))
            locationCamera.Dispose()
            xCamera.Dispose()
            yCamera.Dispose()
            zCamera.Dispose()
        End If

        'Far
        Dim leftTopFar As New On3dPoint
        Dim rightTopFar As New On3dPoint
        Dim leftBottomFar As New On3dPoint
        Dim rightBottomFar As New On3dPoint
        viewport.GetFarRect(leftBottomFar, rightBottomFar, leftTopFar, rightTopFar)
        lines.Add(New OnLine(leftTopFar, rightTopFar))
        lines.Add(New OnLine(rightTopFar, rightBottomFar))
        lines.Add(New OnLine(rightBottomFar, leftBottomFar))
        lines.Add(New OnLine(leftBottomFar, leftTopFar))

        'Near
        Dim leftTopNear As New On3dPoint
        Dim rightTopNear As New On3dPoint
        Dim leftBottomNear As New On3dPoint
        Dim rightBottomNear As New On3dPoint
        viewport.GetNearRect(leftBottomNear, rightBottomNear, leftTopNear, rightTopNear)
        If showNearPlane Then
            lines.Add(New OnLine(leftTopNear, rightTopNear))
            lines.Add(New OnLine(rightTopNear, rightBottomNear))
            lines.Add(New OnLine(rightBottomNear, leftBottomNear))
            lines.Add(New OnLine(leftBottomNear, leftTopNear))
        End If

        'Bordi
        lines.Add(New OnLine(leftTopNear, leftTopFar))
        lines.Add(New OnLine(rightTopNear, rightTopFar))
        lines.Add(New OnLine(leftBottomNear, leftBottomFar))
        lines.Add(New OnLine(rightBottomNear, rightBottomFar))

        leftTopFar.Dispose()
        rightTopFar.Dispose()
        leftBottomFar.Dispose()
        rightBottomFar.Dispose()
        leftTopNear.Dispose()
        rightTopNear.Dispose()
        leftBottomNear.Dispose()
        rightBottomNear.Dispose()
        Return True
    End Function



    ''' <summary>
    ''' Get a Wedge Brep of a viewport frustum
    ''' </summary>
    ''' <param name="viewport"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function GetViewportWedge(ByVal viewport As IOnViewport) As OnBrep
        If viewport Is Nothing Then Return Nothing
        Dim corners(7) As On3dPoint

        Dim leftTopNear As New On3dPoint
        Dim rightTopNear As New On3dPoint
        Dim leftBottomNear As New On3dPoint
        Dim rightBottomNear As New On3dPoint
        viewport.GetNearRect(leftBottomNear, rightBottomNear, leftTopNear, rightTopNear)
        corners(0) = leftBottomNear
        corners(1) = leftTopNear
        corners(2) = rightTopNear
        corners(3) = rightBottomNear

        Dim leftTopFar As New On3dPoint
        Dim rightTopFar As New On3dPoint
        Dim leftBottomFar As New On3dPoint
        Dim rightBottomFar As New On3dPoint
        viewport.GetFarRect(leftBottomFar, rightBottomFar, leftTopFar, rightTopFar)
        corners(4) = leftBottomFar
        corners(5) = leftTopFar
        corners(6) = rightTopFar
        corners(7) = rightBottomFar

        Dim result As OnBrep = OnUtil.ON_BrepBox(corners)
        For i As Integer = 0 To corners.GetUpperBound(0)
            corners(i).Dispose()
        Next
        Return result
    End Function


    ''' <summary>
    ''' Create a list of lines shorter than maxLineLength which connect an array of points
    ''' </summary>
    ''' <param name="points"></param>
    ''' <param name="maxLineLength"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function CreateLinesFromPoints(ByVal points As ArrayOn3dPoint, Optional ByVal maxLineLength As Double = 10, Optional ByVal onlyOneConnection As Boolean = False) As List(Of OnLine)

        'Create links between a point and the next ones; then sort by distance
        Dim links As New RhLinks
        For i As Integer = 0 To points.Count - 1
            For j As Integer = i + 1 To points.Count - 1
                links.Add(New RhLink(i, j, points(i).DistanceTo(points(j))))
            Next
        Next
        links.Sort()

        'Per each point create two lines at maximum
        Dim connections(points.Count - 1) As Integer
        Dim result As New List(Of OnLine)
        Dim numberOfConnections As Integer = 2
        If onlyOneConnection Then numberOfConnections = 1
        For i As Integer = 0 To links.Count - 1
            Dim link As RhLink = links(i)
            If connections(link.Indice0) < numberOfConnections AndAlso connections(link.Indice1) < numberOfConnections Then
                connections(link.Indice0) += 1
                connections(link.Indice1) += 1
                If (points(link.Indice0).DistanceTo(points(link.Indice1)) <= maxLineLength) Then
                    result.Add(New OnLine(points(link.Indice0), points(link.Indice1)))
                End If
            End If
        Next
        Return result
    End Function


    ''' <summary>
    ''' Join a list of lines in polylines with tolerance
    ''' </summary>
    ''' <param name="lines"></param>
    ''' <param name="tolerance"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function JoinLines(ByVal lines As List(Of OnLine), ByVal tolerance As Double) As List(Of OnPolyline)
        If lines Is Nothing Then Return Nothing

        'Create list of line end points
        Dim points(2 * lines.Count - 1) As IOn3dPoint
        Dim isPointAdded(2 * lines.Count - 1) As Boolean
        For i As Integer = 0 To lines.Count - 1
            points(2 * i) = lines(i).from
            points(2 * i + 1) = lines(i).to
            isPointAdded(2 * i) = False
            isPointAdded(2 * i + 1) = False
        Next

        'Create links between line end points if sufficiently near
        Dim firstLinkIndexes As New List(Of Integer)
        Dim secondLinkIndexes As New List(Of Integer)
        For i As Integer = 0 To points.Length - 1
            Dim firstJIndex As Integer
            If i Mod 2 = 0 Then
                firstJIndex = i + 2
            Else
                firstJIndex = i + 1
            End If
            For j As Integer = firstJIndex To points.Length - 1
                If points(i).DistanceTo(points(j)) <= tolerance Then
                    firstLinkIndexes.Add(i)
                    secondLinkIndexes.Add(j)
                End If
            Next
        Next

        'Create list of free ends
        Dim isFreeEnd(2 * lines.Count - 1) As Boolean
        For i As Integer = 0 To isFreeEnd.Length - 1
            If firstLinkIndexes.IndexOf(i) < 0 AndAlso secondLinkIndexes.IndexOf(i) < 0 Then
                isFreeEnd(i) = True
            Else
                isFreeEnd(i) = False
            End If
        Next

        'Manage open polylines
        Dim result As New List(Of OnPolyline)
        For i As Integer = 0 To isFreeEnd.Length - 1
            If isFreeEnd(i) Then
                Dim polyline As New OnPolyline
                Dim startIndex As Integer = i
                polyline.Append(points(startIndex))
                isPointAdded(startIndex) = True
                Do
                    Dim otherEndIndex As Integer
                    If startIndex Mod 2 = 0 Then
                        otherEndIndex = startIndex + 1
                    Else
                        otherEndIndex = startIndex - 1
                    End If
                    polyline.Append(points(otherEndIndex))
                    isPointAdded(otherEndIndex) = True
                    If isFreeEnd(otherEndIndex) Then
                        isFreeEnd(otherEndIndex) = False
                        Exit Do
                    Else
                        Dim linkIndex As Integer = firstLinkIndexes.IndexOf(otherEndIndex)
                        If linkIndex < 0 Then
                            linkIndex = secondLinkIndexes.IndexOf(otherEndIndex)
                            startIndex = firstLinkIndexes(linkIndex)
                        Else
                            startIndex = secondLinkIndexes(linkIndex)
                        End If
                        isPointAdded(startIndex) = True
                    End If
                Loop
                result.Add(polyline)
            End If
        Next

        'Manage closed polylines
        For i As Integer = 0 To isPointAdded.Length - 1
            If Not isPointAdded(i) Then
                Dim polyline As New OnPolyline
                Dim startIndex As Integer = i
                polyline.Append(points(startIndex))
                isPointAdded(startIndex) = True
                Do
                    Dim otherEndIndex As Integer
                    If startIndex Mod 2 = 0 Then
                        otherEndIndex = startIndex + 1
                    Else
                        otherEndIndex = startIndex - 1
                    End If
                    polyline.Append(points(otherEndIndex))
                    isPointAdded(otherEndIndex) = True
                    Dim linkIndex As Integer = firstLinkIndexes.IndexOf(otherEndIndex)
                    If linkIndex < 0 Then
                        linkIndex = secondLinkIndexes.IndexOf(otherEndIndex)
                        startIndex = firstLinkIndexes(linkIndex)
                    Else
                        startIndex = secondLinkIndexes(linkIndex)
                    End If
                    isPointAdded(startIndex) = True
                Loop Until (startIndex = i)
                result.Add(polyline)
            End If
        Next
        Return result
    End Function



    ''' <summary>
    ''' Join and order a list of curves in polycurves with tolerance
    ''' </summary>
    ''' <param name="curves"></param>
    ''' <param name="tolerance"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function JoinCurves(ByVal curves As List(Of IOnCurve), ByVal tolerance As Double) As List(Of OnPolyCurve)
        If curves Is Nothing Then Return Nothing
        If curves.Count = 0 Then Return Nothing
        For i As Integer = 0 To curves.Count - 1
            If curves(i) Is Nothing Then Return Nothing
        Next

        'Create list of curve end points
        Dim points(2 * curves.Count - 1) As On3dPoint
        Dim isPointAdded(2 * curves.Count - 1) As Boolean
        For i As Integer = 0 To curves.Count - 1
            points(2 * i) = curves(i).PointAtStart
            points(2 * i + 1) = curves(i).PointAtEnd
            isPointAdded(2 * i) = False
            isPointAdded(2 * i + 1) = False
        Next

        'Create links between curve end points if sufficiently near
        Dim firstLinkIndexes As New List(Of Integer)
        Dim secondLinkIndexes As New List(Of Integer)
        For i As Integer = 0 To points.Length - 1
            Dim firstJIndex As Integer
            If i Mod 2 = 0 Then
                firstJIndex = i + 2
            Else
                firstJIndex = i + 1
            End If
            For j As Integer = firstJIndex To points.Length - 1
                If points(i).DistanceTo(points(j)) <= tolerance Then
                    firstLinkIndexes.Add(i)
                    secondLinkIndexes.Add(j)
                End If
            Next
        Next

        'Create list of free ends
        Dim isFreeEnd(2 * curves.Count - 1) As Boolean
        For i As Integer = 0 To isFreeEnd.Length - 1
            If firstLinkIndexes.IndexOf(i) < 0 AndAlso secondLinkIndexes.IndexOf(i) < 0 Then
                isFreeEnd(i) = True
            Else
                isFreeEnd(i) = False
            End If
        Next

        'Manage open polycurves
        Dim result As New List(Of OnPolyCurve)
        For i As Integer = 0 To isFreeEnd.Length - 1
            If isFreeEnd(i) Then
                Dim polycurve As New OnPolyCurve
                Dim startIndex As Integer = i
                isPointAdded(startIndex) = True
                Do
                    Dim otherEndIndex As Integer
                    If startIndex Mod 2 = 0 Then
                        otherEndIndex = startIndex + 1
                    Else
                        otherEndIndex = startIndex - 1
                    End If
                    Dim curve As OnCurve = curves(otherEndIndex \ 2).DuplicateCurve
                    If otherEndIndex Mod 2 = 0 Then curve.Reverse()
                    polycurve.Append(curve)
                    isPointAdded(otherEndIndex) = True
                    If isFreeEnd(otherEndIndex) Then
                        isFreeEnd(otherEndIndex) = False
                        Exit Do
                    Else
                        Dim linkIndex As Integer = firstLinkIndexes.IndexOf(otherEndIndex)
                        If linkIndex < 0 Then
                            linkIndex = secondLinkIndexes.IndexOf(otherEndIndex)
                            startIndex = firstLinkIndexes(linkIndex)
                        Else
                            startIndex = secondLinkIndexes(linkIndex)
                        End If
                        isPointAdded(startIndex) = True
                    End If
                Loop
                result.Add(polycurve)
            End If
        Next

        'Manage closed polycurves
        For i As Integer = 0 To isPointAdded.Length - 1
            If Not isPointAdded(i) Then
                Dim polycurve As New OnPolyCurve
                Dim startIndex As Integer = i
                isPointAdded(startIndex) = True
                Do
                    Dim otherEndIndex As Integer
                    If startIndex Mod 2 = 0 Then
                        otherEndIndex = startIndex + 1
                    Else
                        otherEndIndex = startIndex - 1
                    End If
                    Dim curve As OnCurve = curves(otherEndIndex \ 2).DuplicateCurve
                    If otherEndIndex Mod 2 = 0 Then curve.Reverse()
                    polycurve.Append(curve)
                    isPointAdded(otherEndIndex) = True
                    Dim linkIndex As Integer = firstLinkIndexes.IndexOf(otherEndIndex)
                    If linkIndex < 0 Then
                        linkIndex = secondLinkIndexes.IndexOf(otherEndIndex)
                        startIndex = firstLinkIndexes(linkIndex)
                    Else
                        startIndex = secondLinkIndexes(linkIndex)
                    End If
                    isPointAdded(startIndex) = True
                Loop Until (startIndex = i)
                result.Add(polycurve)
            End If
        Next
        Return result
    End Function



    ''' <summary>
    ''' Join a list of polylines in polylines with tolerance
    ''' </summary>
    ''' <param name="polylines"></param>
    ''' <param name="tolerance"></param>
    ''' <returns></returns>
    ''' <remarks>Tratta la polylinea come una unica linea. Usare JoinCurves trasformando le Polyline in PolylineCurve</remarks>
    Public Shared Function JoinPolylines(ByVal polylines As List(Of IOnPolyline), ByVal tolerance As Double) As List(Of OnPolyline)
        If polylines Is Nothing Then Return Nothing

        'Create list of line end points
        Dim points(2 * polylines.Count - 1) As IOn3dPoint
        Dim isPointAdded(2 * polylines.Count - 1) As Boolean
        For i As Integer = 0 To polylines.Count - 1
            points(2 * i) = polylines(i).First
            points(2 * i + 1) = polylines(i).Last
            isPointAdded(2 * i) = False
            isPointAdded(2 * i + 1) = False
        Next

        'Create links between line end points if sufficiently near
        Dim firstLinkIndexes As New List(Of Integer)
        Dim secondLinkIndexes As New List(Of Integer)
        For i As Integer = 0 To points.Length - 1
            Dim firstJIndex As Integer
            If i Mod 2 = 0 Then
                firstJIndex = i + 2
            Else
                firstJIndex = i + 1
            End If
            For j As Integer = firstJIndex To points.Length - 1
                If points(i).DistanceTo(points(j)) <= tolerance Then
                    firstLinkIndexes.Add(i)
                    secondLinkIndexes.Add(j)
                End If
            Next
        Next

        'Create list of free ends
        Dim isFreeEnd(2 * polylines.Count - 1) As Boolean
        For i As Integer = 0 To isFreeEnd.Length - 1
            If firstLinkIndexes.IndexOf(i) < 0 AndAlso secondLinkIndexes.IndexOf(i) < 0 Then
                isFreeEnd(i) = True
            Else
                isFreeEnd(i) = False
            End If
        Next

        'Manage open polylines
        Dim result As New List(Of OnPolyline)
        For i As Integer = 0 To isFreeEnd.Length - 1
            If isFreeEnd(i) Then
                Dim polyline As New OnPolyline
                Dim startIndex As Integer = i
                polyline.Append(points(startIndex))
                isPointAdded(startIndex) = True
                Do
                    Dim otherEndIndex As Integer
                    If startIndex Mod 2 = 0 Then
                        otherEndIndex = startIndex + 1
                    Else
                        otherEndIndex = startIndex - 1
                    End If
                    polyline.Append(points(otherEndIndex))
                    isPointAdded(otherEndIndex) = True
                    If isFreeEnd(otherEndIndex) Then
                        isFreeEnd(otherEndIndex) = False
                        Exit Do
                    Else
                        Dim linkIndex As Integer = firstLinkIndexes.IndexOf(otherEndIndex)
                        If linkIndex < 0 Then
                            linkIndex = secondLinkIndexes.IndexOf(otherEndIndex)
                            startIndex = firstLinkIndexes(linkIndex)
                        Else
                            startIndex = secondLinkIndexes(linkIndex)
                        End If
                        isPointAdded(startIndex) = True
                    End If
                Loop
                result.Add(polyline)
            End If
        Next

        'Manage closed polylines
        For i As Integer = 0 To isPointAdded.Length - 1
            If Not isPointAdded(i) Then
                Dim closedPolyline As New OnPolyline
                Dim startIndex As Integer = i
                closedPolyline.Append(points(startIndex))
                isPointAdded(startIndex) = True
                Do
                    Dim otherEndIndex As Integer
                    If startIndex Mod 2 = 0 Then
                        otherEndIndex = startIndex + 1
                    Else
                        otherEndIndex = startIndex - 1
                    End If
                    closedPolyline.Append(points(otherEndIndex))
                    isPointAdded(otherEndIndex) = True
                    Dim linkIndex As Integer = firstLinkIndexes.IndexOf(otherEndIndex)
                    If linkIndex < 0 Then
                        linkIndex = secondLinkIndexes.IndexOf(otherEndIndex)
                        startIndex = firstLinkIndexes(linkIndex)
                    Else
                        startIndex = secondLinkIndexes(linkIndex)
                    End If
                    isPointAdded(startIndex) = True
                Loop Until (startIndex = i)
                result.Add(closedPolyline)
            End If
        Next
        Return result
    End Function



    Public Shared Function FilletPolyline(ByVal polyline As IOnPolyline, ByVal radius As Double) As OnPolyCurve
        If polyline Is Nothing Then Return Nothing
        Dim result As New OnPolyCurve
        If radius = 0 Then
            result.Append(New OnPolylineCurve(polyline))
            Return result
        End If

        Dim previousPoint As New On3dPoint(polyline(0))
        For i As Integer = 1 To polyline.Count - 2
            Dim dir0 As On3dVector = polyline.SegmentTangent(i - 1)
            Dim dir1 As On3dVector = polyline.SegmentTangent(i)
            Dim sum As On3dVector = dir0 + dir1
            Dim bisec As On3dVector = dir1 - dir0
            Dim done As Boolean = False

            If Not sum.IsTiny And Not bisec.IsTiny Then
                bisec.Unitize()
                Dim angle As Double = Math.Acos(OnUtil.ON_DotProduct(bisec, dir1))
                Dim distancePR As Double = radius / Math.Tan(angle)
                If previousPoint.DistanceTo(polyline(i)) > distancePR Then
                    If polyline(i).DistanceTo(polyline(i + 1)) > distancePR Then
                        Dim distanceQ As Double = radius / Math.Sin(angle) - radius
                        Dim P As On3dPoint = New On3dPoint(polyline(i)) + dir0 * (-distancePR)
                        Dim Q As On3dPoint = New On3dPoint(polyline(i)) + bisec * distanceQ
                        Dim R As On3dPoint = New On3dPoint(polyline(i)) + dir1 * distancePR
                        Dim arc As New OnArc(P, Q, R)
                        If previousPoint.DistanceTo(P) > 0.001 Then
                            Dim line As New OnLine(previousPoint, P)
                            result.Append(New OnLineCurve(line))
                            line.Dispose()
                        End If
                        result.Append(New OnArcCurve(arc))
                        arc.Dispose()
                        previousPoint.Dispose()
                        previousPoint = R
                        P.Dispose()
                        Q.Dispose()
                        done = True
                    End If
                End If
            End If
            If Not done Then
                Dim line As New OnLine(previousPoint, polyline(i))
                result.Append(New OnLineCurve(line))
                line.Dispose()
                previousPoint.Dispose()
                previousPoint = New On3dPoint(polyline(i))
            End If
            sum.Dispose()
            bisec.Dispose()
            dir0.Dispose()
            dir1.Dispose()
        Next

        Dim lastLine As New OnLine(previousPoint, polyline.Last)
        result.Append(New OnLineCurve(lastLine))
        lastLine.Dispose()
        previousPoint.Dispose()
        Return result
    End Function


    Public Shared Function SamplePolycurve(ByVal polycurve As IOnPolyCurve, ByVal sampleDistance As Double) As On3dPointArray
        If polycurve Is Nothing Then Return Nothing
        If sampleDistance <= 0 Then Return Nothing

        Dim result As New On3dPointArray
        Dim curveLength As Double
        polycurve.GetLength(curveLength)
        Dim segmentCount As Integer = CInt(curveLength / sampleDistance) + 1
        Dim t As Double
        For i As Integer = 0 To segmentCount
            polycurve.GetNormalizedArcLengthPoint(i / segmentCount, t)
            result.Append(polycurve.PointAt(t))
        Next
        Return result
    End Function



    Public Shared Function VisualizzaPianoConAssi(ByVal plane As IOnPlane, Optional ByVal planeName As String = "", Optional ByVal xAxisName As String = "", Optional ByVal yAxisName As String = "") As MRhinoSurfaceObject
        Const xMin As Double = -75
        Const xMax As Double = 75
        Const yMin As Double = -75
        Const yMax As Double = 75
        Dim refXaxis As New MRhinoObjRef(RhGeometry.VisualizzaDirezione(plane.xaxis, plane.origin))
        Dim refYaxis As New MRhinoObjRef(RhGeometry.VisualizzaDirezione(plane.yaxis, plane.origin))
        Dim tmpPLane As OnSurface = RhGeometry.CreatePlanarSurface(plane, xMin, yMin, xMax, yMax)
        Dim refPlane As MRhinoSurfaceObject = RhUtil.RhinoApp.ActiveDoc.AddSurfaceObject(tmpPLane)
        'Imposta i nomi se presenti
        If (planeName <> "") Then RhDocument.ImpostaNomeAdOggetto(refPlane, planeName)
        If (xAxisName <> "") Then RhDocument.ImpostaNomeAdOggetto(refXaxis, xAxisName)
        If (yAxisName <> "") Then RhDocument.ImpostaNomeAdOggetto(refYaxis, yAxisName)
        'Ritorna il piano
        Return refPlane
    End Function


    Public Shared Function IntersecaCurvaConPiano(ByVal curve As IOnCurve, ByVal plane As IOnPlane) As On3dPointArray
        Dim surfacePlane As OnSurface = RhGeometry.CreatePlanarSurface(plane, -100000, -100000, 100000, 100000)
        Return IntersecaCurvaConSuperfice(curve, surfacePlane)
    End Function


    Public Shared Function IntersecaCurvaConSuperfice(ByVal curve As IOnCurve, ByVal surface As IOnSurface) As On3dPointArray
        Dim result As New ArrayOnX_EVENT
        curve.IntersectSurface(surface, result)
        If result.Count = 0 Then Return Nothing
        'Costruice l'array di punti da ritornare
        Dim arrayResult As New On3dPointArray
        For i As Integer = 0 To result.Count - 1
            If result.Item(i).IsPointEvent Then arrayResult.Append(result.Item(i).m_pointA(0))
        Next
        Return arrayResult
    End Function



    Public Shared Function MeshFillHoles(ByVal meshObjRef As MRhinoObjRef) As Boolean
        Try
            meshObjRef.Object.Select(True, False, True, True, True, True)
            RhUtil.RhinoApp.RunScript("_FillMeshHoles _enter", 0)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function


    Public Shared Function MeshOutline(ByVal meshObjRef As MRhinoObjRef, ByVal referenceView As RhViewport.eViewportNames, Optional ByVal fitTolerance As Double = 0.0) As OnNurbsCurve
        Try
            'Calcola gli ID delle view: quella corrente e quella da attivare
            Dim activeViewportCameraDirection As IOn3dVector = RhUtil.RhinoApp.ActiveView.ActiveViewport.m_v.m_vp.CameraDirection
            Dim referenceViewportCameraDirection As IOn3dVector = RhViewport.ViewportGetCameraDirection(referenceView)

            'Verifica che la view attiva sia esattamente quella richiesta; in caso contrario la setta attiva
            If Not (activeViewportCameraDirection Is referenceViewportCameraDirection) Then
                Dim viewIsCorrect As Boolean = RhViewport.ViewportActivate(referenceView)
                If Not viewIsCorrect Then
                    Debug.Print("Vista non impostata corettamente. Impossibile continuare con il comando MeshOutline")
                    Return Nothing
                End If
            End If

            'Esegue il comando MESH_OUTLINE
            meshObjRef.Object.Select(True, False, True, True, True, True)
            RhUtil.RhinoApp.RunScript("_MeshOutline _enter", 0)
            RhUtil.RhinoApp.RunScript("_Sellast _enter", 0)
            Dim getObj As New MRhinoGetObject
            getObj.GetObjects(0, 0)

            'Imposta il risultato
            Dim result As OnNurbsCurve = Nothing
            If getObj.ObjectCount = 1 Then
                result = getObj.Object(0).Curve.NurbsCurve
            Else
                Return Nothing
            End If

            'Cancella oggetti e ritorna
            For i As Integer = 0 To getObj.ObjectCount - 1
                RhUtil.RhinoApp.ActiveDoc.DeleteObject(getObj.Object(i))
            Next
            getObj.Dispose()

            'Se necessario esegue il FIT con la tolleranza indicata; altrimenti ritorna la curva così com'è
            If (fitTolerance <> 0) Then
                result = EsegueFitCurva(result, fitTolerance, 3).NurbsCurve
            End If

            'Se necessario risetta la vista attiva e ritorna il risultato
            If Not (activeViewportCameraDirection Is referenceViewportCameraDirection) Then RhViewport.ViewportActivate(activeViewportCameraDirection)
            RhUtil.RhinoApp.ActiveDoc.Redraw()
            Return result
        Catch ex As Exception
            Return Nothing
        End Try
    End Function



    '##################### DA RIVEDERE #####################
    Public Shared Function CalcolaTangenteSuCurva(ByVal curve As IOnCurve, ByVal pointNearStart As IOn3dPoint, ByVal pointNearEnd As IOn3dPoint) As OnNurbsCurve
        Dim objCurve As MRhinoObject = Nothing
        Dim objCurveRef As MRhinoObjRef = Nothing
        Dim result As OnNurbsCurve = Nothing
        Dim getObj As MRhinoGetObject = Nothing
        Try
            objCurve = RhUtil.RhinoApp.ActiveDoc.AddCurveObject(curve)
            objCurveRef = New MRhinoObjRef(objCurve)
            'Seleziona la curva aggiunta e esegue lo script
            objCurve.Select(True, False, True, True, True, True)
            RhUtil.RhinoApp.RunScript("_Line _Tangent " & RhCoordinates.C3dPointToString(pointNearStart) & " " & RhCoordinates.C3dPointToString(pointNearEnd) & " _enter", 0)
            'Raccoglie il risultato
            getObj = New MRhinoGetObject
            getObj.EnablePostSelect(False)
            getObj.GetObjects(0, -1)
            'Controlla se il risultato è di un elemento; in caso contrario esce
            If getObj.ObjectCount <> 1 Then
                Throw New Exception("Anomalia riscontrata in " & New StackFrame().GetMethod().DeclaringType.FullName & "." & MethodBase.GetCurrentMethod().Name)
            Else
                result = getObj.Object(0).Curve.NurbsCurve
            End If
            Return result

        Catch ex As Exception
            Return Nothing

        Finally
            If objCurveRef IsNot Nothing Then
                RhUtil.RhinoApp.ActiveDoc.DeleteObject(objCurveRef)
                objCurveRef.Dispose()
            End If
            For i As Integer = 0 To getObj.ObjectCount - 1
                RhUtil.RhinoApp.ActiveDoc.DeleteObject(getObj.Object(i))
            Next
            getObj.Dispose()
            RhUtil.RhinoApp.ActiveDoc.Redraw()
        End Try
    End Function



    ''' <summary>
    ''' Find aligned most prominent points of a planar curve using a gravitational principle
    ''' </summary>
    ''' <param name="planarCurve">Input planar curve</param>
    ''' <param name="direction">Approaching direction that is the opposite of the gravitational field in world coordinates</param>
    ''' <param name="prominentPoints">Output points</param>
    ''' <param name="curveSubDomain">Restrict gravitational effect to a curve sub-domain</param>
    ''' <param name="tolerance">Tolerance used to find tangent to curve</param>
    ''' <param name="maxIterations">Max number of iteration to find result</param>
    ''' <param name="prominentPointsMinDistance">Min distance to separate result points. If 0 prominentPointsMinDistance= 100 * tolerance</param>
    ''' <param name="sampleCount">Number of points to sample the curve. If 0 curve is sampled every 10*tolerance</param>
    ''' <returns>true if successifully</returns>
    ''' <remarks>The algorithm compute a line which is laid on the curve from an approaching direction</remarks>
    Public Shared Function MostProminentPoints(ByVal planarCurve As IOnCurve, ByVal direction As IOn3dVector, ByRef prominentPoints As On3dPointArray, Optional ByVal curveSubDomain As IOnInterval = Nothing, Optional ByVal tolerance As Double = 0.1, Optional ByVal maxIterations As Integer = 10000, Optional ByVal prominentPointsMinDistance As Double = 0, Optional ByVal sampleCount As Integer = 0) As Boolean
        If planarCurve Is Nothing Then Return False
        If direction Is Nothing Then Return False

        'Initial plane orientation
        Dim curve As OnCurve = planarCurve.DuplicateCurve
        Dim curvePlane As New OnPlane
        If Not planarCurve.IsPlanar(curvePlane) Then
            If Not curvePlane Is Nothing Then curvePlane.Dispose()
            curve.Dispose()
            Return False
        End If
        If curvePlane.zaxis.IsParallelTo(direction) <> 0 Then
            curvePlane.Dispose()
            curve.Dispose()
            Return False
        End If
        Dim xFormCurvePlane As New OnXform
        xFormCurvePlane.Rotation(curvePlane, OnUtil.On_xy_plane)
        curve.Transform(xFormCurvePlane)

        'Direction of gravitational field
        Dim normalDirection As New On3dVector(direction)
        normalDirection.Transform(xFormCurvePlane)
        xFormCurvePlane.Dispose()
        normalDirection.z = 0
        normalDirection.Unitize()
        Dim angle As Double = Math.Atan2(normalDirection.y, normalDirection.x)
        Dim directionXForm As New OnXform
        directionXForm.Rotation(Math.PI / 2 - angle, OnUtil.On_zaxis, OnUtil.On_origin)
        curve.Transform(directionXForm)
        directionXForm.Dispose()

        'Sample curve and get center of mass
        If sampleCount = 0 Then     'Default samples count
            Dim curveLength As Double
            curve.GetLength(curveLength)
            sampleCount = CInt(0.1 * curveLength / tolerance)
        End If
        Dim points As New On3dPointArray(sampleCount)
        If curveSubDomain Is Nothing Then curveSubDomain = curve.Domain
        Dim massCenter As New On3dPoint
        Dim massCenterPointsCount As Integer = 0
        Dim pt As On3dPoint
        For i As Integer = 0 To sampleCount - 1 'Includes also last point
            Dim t As Double = curve.Domain.m_t(0) + i * curve.Domain.Length / (sampleCount - 1)
            pt = curve.PointAt(t)
            points.Append(pt)
            If curveSubDomain.Includes(t) Then
                massCenter += pt
                massCenterPointsCount += 1
            End If
            pt.Dispose()
        Next
        massCenter *= 1 / massCenterPointsCount

        'Get boundingbox diagonal length
        Dim bbox As New OnBoundingBox
        points.GetTightBoundingBox(bbox)
        Dim diagonalVector As On3dVector = bbox.Diagonal
        Dim diagonal As Double = diagonalVector.Length
        diagonalVector.Dispose()
        bbox.Dispose()
        Dim theta As Double = tolerance / diagonal

        ' Main loop
        Dim globalXForm As New OnXform
        globalXForm.Identity()

        Dim equilibrium As Integer
        Dim iterationCount As Integer = 0
        Dim previousEquilibrium As Integer = 0
        Dim exitIterationCount As Integer = maxIterations
        Do
            'Get extreme points
            Dim extremeIndex1 As Integer = -1
            Dim extremeIndex2 As Integer = -1
            GetExtremePoints(points, extremeIndex1, extremeIndex2)
            Dim extreme1 As On3dPoint = points(extremeIndex1)
            Dim extreme2 As On3dPoint = points(extremeIndex2)
            equilibrium = ReachedEquilibrium(extreme1, extreme2, massCenter, tolerance)
            If equilibrium <> 0 Then
                Dim xFormRotation As New OnXform
                xFormRotation.Rotation(equilibrium * theta, OnUtil.On_zaxis, extreme1)
                points.Transform(xFormRotation)
                massCenter.Transform(xFormRotation)
                globalXForm = xFormRotation * globalXForm
                xFormRotation.Dispose()
            End If

            Dim xFormTranslation As New OnXform
            xFormTranslation.Translation(0, -extreme1.y, 0)
            globalXForm = xFormTranslation * globalXForm
            points.Transform(xFormTranslation)
            massCenter.Transform(xFormTranslation)
            xFormTranslation.Dispose()
            If iterationCount > 0 And exitIterationCount = maxIterations Then
                If previousEquilibrium <> equilibrium Then 'Refine position
                    exitIterationCount = iterationCount + 10
                    theta /= 10
                End If
            End If
            previousEquilibrium = equilibrium
            iterationCount += 1
        Loop Until equilibrium = 0 Or iterationCount = exitIterationCount
        massCenter.Dispose()

        ' Find extremes points
        Dim extremePoints As New On3dPointArray
        For i As Integer = 0 To points.Count - 1
            If points(i).y <= tolerance Then extremePoints.Append(points(i))
        Next
        points.Dispose()

        'Group extreme points
        If prominentPointsMinDistance = 0 Then
            prominentPointsMinDistance = 100 * tolerance
        End If
        Dim redundancy As New Arrayint
        For i As Integer = 0 To extremePoints.Count - 1
            redundancy.Append(1)
        Next
        For i As Integer = extremePoints.Count - 1 To 1 Step -1
            For j As Integer = i - 1 To 0 Step -1
                If extremePoints(i).DistanceTo(extremePoints(j)) < 100 * tolerance Then
                    extremePoints(j) = MeanPosition(extremePoints(i), redundancy(i), extremePoints(j), redundancy(j))
                    redundancy(j) += redundancy(i)
                    redundancy.Remove(i)
                    extremePoints.Remove(i)
                    Exit For
                End If
            Next
        Next

        'Send extreme points to y=0 and transform back
        curve.Transform(globalXForm)
        globalXForm.Dispose()
        Dim tExtremePoint As Double
        For i As Integer = 0 To extremePoints.Count - 1
            extremePoints(i).y = 0
            curve.GetClosestPoint(extremePoints(i), tExtremePoint)
            extremePoints(i).Dispose()
            extremePoints(i) = planarCurve.PointAt(tExtremePoint)
        Next
        curve.Dispose()

        If Not prominentPoints Is Nothing Then prominentPoints.Dispose()
        prominentPoints = extremePoints
        Return True
    End Function


    Private Shared Function GetExtremePoints(ByVal points As IOn3dPointArray, ByRef extremeIndex1 As Integer, ByRef extremeIndex2 As Integer) As Boolean
        If points Is Nothing Then Return False
        If points.Count < 2 Then Return False
        extremeIndex1 = -1
        extremeIndex2 = -1
        Dim yMin As Double = Double.MaxValue
        For i As Integer = 0 To points.Count - 1
            If points(i).y < yMin Then
                yMin = points(i).y
                extremeIndex2 = extremeIndex1
                extremeIndex1 = i
            End If
        Next
        'Manage case extreme2 was not found
        If extremeIndex2 = -1 Then
            yMin = Double.MaxValue
            For i As Integer = 0 To points.Count - 1
                If i <> extremeIndex1 Then
                    If points(i).y < yMin Then
                        yMin = points(i).y
                        extremeIndex2 = i
                    End If
                End If
            Next
        End If
        Return True
    End Function


    ' Return 0 if equilibrium is reached
    ' Return 1 if a counterclockwise rotation (positive) is needed
    ' Return -1 if a clockwise rotation (negative) is needed
    Private Shared Function ReachedEquilibrium(ByVal extreme1 As IOn3dPoint, ByVal extreme2 As IOn3dPoint, ByVal centerMass As IOn3dPoint, ByVal tolerance As Double) As Integer
        Dim xMin As Double
        Dim xMax As Double
        If extreme1.x <= extreme2.x Then
            xMin = extreme1.x
            xMax = extreme2.x
        Else
            xMin = extreme2.x
            xMax = extreme1.x
        End If
        If centerMass.x >= xMin And centerMass.x <= xMax Then
            If extreme2.y - extreme1.y < tolerance Then Return 0
        End If
        If centerMass.x < extreme1.x Then
            Return 1
        Else
            Return -1
        End If
    End Function


    ''' <summary>
    ''' ### MARCO ###
    ''' Get a planar convex hull of a planar points array
    ''' </summary>
    ''' <param name="points">Input planar points array</param>
    ''' <param name="sampleCount">Number of points the planarCurve is sampled</param>
    ''' <param name="planarityTolerance">Tolerance to compute planarCurve plane</param>
    Public Shared Function GetConvexHull(ByVal points As On3dPointArray, ByVal pointsPlane As OnPlane, Optional ByVal sampleCount As Integer = 250, Optional ByVal planarityTolerance As Double = 10.0) As OnPolyline
        'Transform points
        Dim xForm As New OnXform
        xForm.Rotation(pointsPlane, OnUtil.On_xy_plane)
        points.Transform(xForm)

        'Sort points
        points.QuickSort(True)

        'Upper side
        Dim upperIndexes As New List(Of Integer)
        upperIndexes.Add(0)
        Dim maxTangentIndex As Integer = -1
        Dim maxYIndex As Integer = -1
        Do
            Dim maxTangent As Double = Double.NegativeInfinity
            Dim maxYValue As Double = Double.NegativeInfinity
            Dim lastUpperIndex As Integer = upperIndexes(upperIndexes.Count - 1)
            maxTangentIndex = -1
            maxYIndex = -1
            For i As Integer = lastUpperIndex + 1 To points.Count - 1
                If points(i).x = points(lastUpperIndex).x Then
                    If points(i).y > maxYValue Then
                        maxYValue = points(i).y
                        maxYIndex = i
                    End If
                Else
                    Dim tangent As Double = (points(i).y - points(lastUpperIndex).y) / (points(i).x - points(lastUpperIndex).x)
                    If tangent > maxTangent Then
                        maxTangent = tangent
                        maxTangentIndex = i
                    End If
                End If
            Next
            If maxTangentIndex <> -1 Then
                If maxYIndex <> -1 And maxYValue > points(lastUpperIndex).y Then
                    upperIndexes.Add(maxYIndex)
                Else
                    upperIndexes.Add(maxTangentIndex)
                End If
            Else
                If maxYIndex = -1 Then Exit Do
                upperIndexes.Add(maxYIndex)
            End If
        Loop Until (upperIndexes(upperIndexes.Count - 1) = points.Count - 1)

        'Lower side
        Dim lowerIndexes As New List(Of Integer)
        lowerIndexes.Add(0)
        Dim minTangentIndex As Integer = -1
        Dim minYIndex As Integer = -1
        Do
            Dim minTangent As Double = Double.PositiveInfinity
            Dim minYValue As Double = Double.PositiveInfinity
            Dim lastLowerIndex As Integer = lowerIndexes(lowerIndexes.Count - 1)
            minTangentIndex = -1
            minYIndex = -1
            For i As Integer = lastLowerIndex + 1 To points.Count - 1
                If points(i).x = points(lastLowerIndex).x Then
                    If points(i).y < minYValue Then
                        minYValue = points(i).y
                        minYIndex = i
                    End If
                Else
                    Dim tangent As Double = (points(i).y - points(lastLowerIndex).y) / (points(i).x - points(lastLowerIndex).x)
                    If tangent < minTangent Then
                        minTangent = tangent
                        minTangentIndex = i
                    End If
                End If
            Next
            If minTangentIndex <> -1 Then
                If minYIndex <> -1 And minYValue < points(lastLowerIndex).y Then
                    lowerIndexes.Add(minYIndex)
                Else
                    lowerIndexes.Add(minTangentIndex)
                End If
            Else
                lowerIndexes.Add(minYIndex)
            End If
        Loop Until (lowerIndexes(lowerIndexes.Count - 1) = points.Count - 1)

        'Create poliline
        Dim result As New OnPolyline
        For i As Integer = 0 To upperIndexes.Count - 1
            result.Append(points(upperIndexes(i)))
        Next
        For i As Integer = lowerIndexes.Count - 2 To 0 Step -1
            result.Append(points(lowerIndexes(i)))
        Next

        xForm.Rotation(OnUtil.On_xy_plane, pointsPlane)
        result.Transform(xForm)

        'Disposing
        pointsPlane.Dispose()
        xForm.Dispose()
        points.Dispose()

        Return result
    End Function


    ''' <summary>
    ''' Get a planar convect hull of a generic planar curve
    ''' </summary>
    ''' <param name="planarCurve">Input planar curve</param>
    ''' <param name="sampleCount">Number of points the planarCurve is sampled</param>
    ''' <param name="planarityTolerance">Tolerance to compute planarCurve plane</param>
    Public Shared Function GetConvexHull(ByVal planarCurve As IOnCurve, Optional ByVal sampleCount As Integer = 250, Optional ByVal planarityTolerance As Double = 10.0) As OnPolyline
        'Definisci piano di appartenenza
        Dim curvePlane As New OnPlane
        If Not planarCurve.IsPlanar(curvePlane, planarityTolerance) Then
            curvePlane.Dispose()
            Return Nothing
        End If

        'Sample curve
        Dim points As New On3dPointArray(sampleCount)
        Dim curveDomain As OnInterval = planarCurve.Domain
        Dim subdivisionDomainLenght As Double = 0
        If planarCurve.IsClosed Then
            subdivisionDomainLenght = curveDomain.Length / sampleCount
        Else
            subdivisionDomainLenght = curveDomain.Length / (sampleCount - 1)
        End If
        For i As Integer = 0 To sampleCount - 1
            Dim t As Double = curveDomain.m_t(0) + i * subdivisionDomainLenght
            Dim point As On3dPoint = planarCurve.PointAt(t)
            points.Append(point)
            point.Dispose()
        Next

        'Add discontinuity points
        Dim tDiscontinuity As Double
        Dim t0 As Double = curveDomain.m_t(0)
        Dim nextDiscuntinuityFound As Boolean = False
        Do
            If planarCurve.GetNextDiscontinuity(IOn.continuity.C1_continuous, t0, curveDomain.m_t(1), tDiscontinuity) Then
                Dim point As On3dPoint = planarCurve.PointAt(tDiscontinuity)
                Dim index As Integer
                If Not points.GetClosestPoint(point, index, 0.001) Then points.Append(point)
                point.Dispose()
                t0 = tDiscontinuity
                nextDiscuntinuityFound = True
            Else
                nextDiscuntinuityFound = False
            End If
        Loop While nextDiscuntinuityFound
        If planarCurve.IsClosed Then
            If Not planarCurve.IsContinuous(IOn.continuity.C1_locus_continuous, curveDomain.m_t(1)) Then
                Dim point As On3dPoint = planarCurve.PointAtStart
                Dim index As Integer
                If Not points.GetClosestPoint(point, index, 0.001) Then points.Append(point)
                point.Dispose()
            End If
        End If
        curveDomain.Dispose()

        'Transform points
        Dim xForm As New OnXform
        xForm.Rotation(curvePlane, OnUtil.On_xy_plane)
        points.Transform(xForm)

        'Sort points
        points.QuickSort(True)

        'Upper side
        Dim upperIndexes As New List(Of Integer)
        upperIndexes.Add(0)
        Dim maxTangentIndex As Integer = -1
        Dim maxYIndex As Integer = -1
        Do
            Dim maxTangent As Double = Double.NegativeInfinity
            Dim maxYValue As Double = Double.NegativeInfinity
            Dim lastUpperIndex As Integer = upperIndexes(upperIndexes.Count - 1)
            maxTangentIndex = -1
            maxYIndex = -1
            For i As Integer = lastUpperIndex + 1 To points.Count - 1
                If points(i).x = points(lastUpperIndex).x Then
                    If points(i).y > maxYValue Then
                        maxYValue = points(i).y
                        maxYIndex = i
                    End If
                Else
                    Dim tangent As Double = (points(i).y - points(lastUpperIndex).y) / (points(i).x - points(lastUpperIndex).x)
                    If tangent > maxTangent Then
                        maxTangent = tangent
                        maxTangentIndex = i
                    End If
                End If
            Next
            If maxTangentIndex <> -1 Then
                If maxYIndex <> -1 And maxYValue > points(lastUpperIndex).y Then
                    upperIndexes.Add(maxYIndex)
                Else
                    upperIndexes.Add(maxTangentIndex)
                End If
            Else
                If maxYIndex = -1 Then Exit Do
                upperIndexes.Add(maxYIndex)
            End If
        Loop Until (upperIndexes(upperIndexes.Count - 1) = points.Count - 1)

        'Lower side
        Dim lowerIndexes As New List(Of Integer)
        lowerIndexes.Add(0)
        Dim minTangentIndex As Integer = -1
        Dim minYIndex As Integer = -1
        Do
            Dim minTangent As Double = Double.PositiveInfinity
            Dim minYValue As Double = Double.PositiveInfinity
            Dim lastLowerIndex As Integer = lowerIndexes(lowerIndexes.Count - 1)
            minTangentIndex = -1
            minYIndex = -1
            For i As Integer = lastLowerIndex + 1 To points.Count - 1
                If points(i).x = points(lastLowerIndex).x Then
                    If points(i).y < minYValue Then
                        minYValue = points(i).y
                        minYIndex = i
                    End If
                Else
                    Dim tangent As Double = (points(i).y - points(lastLowerIndex).y) / (points(i).x - points(lastLowerIndex).x)
                    If tangent < minTangent Then
                        minTangent = tangent
                        minTangentIndex = i
                    End If
                End If
            Next
            If minTangentIndex <> -1 Then
                If minYIndex <> -1 And minYValue < points(lastLowerIndex).y Then
                    lowerIndexes.Add(minYIndex)
                Else
                    lowerIndexes.Add(minTangentIndex)
                End If
            Else
                lowerIndexes.Add(minYIndex)
            End If
        Loop Until (lowerIndexes(lowerIndexes.Count - 1) = points.Count - 1)

        'Create poliline
        Dim result As New OnPolyline
        For i As Integer = 0 To upperIndexes.Count - 1
            result.Append(points(upperIndexes(i)))
        Next
        For i As Integer = lowerIndexes.Count - 2 To 0 Step -1
            result.Append(points(lowerIndexes(i)))
        Next

        xForm.Rotation(OnUtil.On_xy_plane, curvePlane)
        result.Transform(xForm)

        'Disposing
        curvePlane.Dispose()
        xForm.Dispose()
        points.Dispose()

        Return result
    End Function




    'Suddividi una superfice in porzioni di una data dimensione
    Public Shared Sub SubdivideBrepFace(ByVal brepFace As IOnBrepFace, ByRef tu() As Double, ByRef tv() As Double, ByVal maxUArcLength As Double, ByVal maxVArcLength As Double, Optional ByVal maxUNormalVariation As Double = -1, Optional ByVal maxVNormalVariation As Double = -1)
        If brepFace Is Nothing Then Exit Sub

        'Calcola parametri Greville interni alla boundingbox dell'outer loop
        Dim nurbsSurface As OnNurbsSurface = brepFace.NurbsSurface
        Dim outerLoop As IOnBrepLoop = brepFace.Loop(0)

        Dim gu() As Double = Nothing
        nurbsSurface.GetGrevilleAbcissae(0, gu)
        Dim guInner As New List(Of Double)
        guInner.Add(outerLoop.m_pbox.Min.x)
        For i As Integer = 0 To gu.GetUpperBound(0)
            If gu(i) > outerLoop.m_pbox.Min.x AndAlso gu(i) < outerLoop.m_pbox.Max.x Then
                guInner.Add(gu(i))
            End If
        Next
        guInner.Add(outerLoop.m_pbox.Max.x)

        Dim gv() As Double = Nothing
        nurbsSurface.GetGrevilleAbcissae(1, gv)
        Dim gvInner As New List(Of Double)
        gvInner.Add(outerLoop.m_pbox.Min.y)
        For i As Integer = 0 To gv.GetUpperBound(0)
            If gv(i) > outerLoop.m_pbox.Min.y AndAlso gv(i) < outerLoop.m_pbox.Max.y Then
                gvInner.Add(gv(i))
            End If
        Next
        gvInner.Add(outerLoop.m_pbox.Max.y)

        'Crea suddivisioni in U
        Dim resultTu As New List(Of Double)
        resultTu.Add(outerLoop.m_pbox.Min.x)
        Dim nextTu As Double
        Do
            nextTu = Double.PositiveInfinity
            For i As Integer = 0 To gvInner.Count - 1
                Dim isoCurve As OnCurve = nurbsSurface.IsoCurve(0, gvInner(i))
                Dim isoCurveLength As Double
                isoCurve.GetLength(isoCurveLength)
                Dim initialExternalLength As Double
                Dim isocurveDomain As OnInterval = isoCurve.Domain
                Dim initialExternalLengthInterval As New OnInterval(isocurveDomain.m_t(0), resultTu(0))
                isoCurve.GetLength(initialExternalLength, 0.0001, initialExternalLengthInterval)
                initialExternalLengthInterval.Dispose()
                Dim normalisedArcLength As Double = (initialExternalLength + resultTu.Count * maxUArcLength) / isoCurveLength
                Dim tmpTu As Double
                If Not isoCurve.GetNormalizedArcLengthPoint(normalisedArcLength, tmpTu) Then
                    tmpTu = outerLoop.m_pbox.Max.x
                End If
                If tmpTu < nextTu Then nextTu = tmpTu
                If maxUNormalVariation >= 0 Then
                    Dim variationInterval As New OnInterval(resultTu(resultTu.Count - 1), isocurveDomain.m_t(1))
                    tmpTu = GetParameterAtNormalVariation(brepFace, True, gvInner(i), maxUNormalVariation, variationInterval)
                    variationInterval.Dispose()
                    If tmpTu < nextTu Then nextTu = tmpTu
                End If
                isocurveDomain.Dispose()
                isoCurve.Dispose()
            Next
            If nextTu > outerLoop.m_pbox.Max.x Then nextTu = outerLoop.m_pbox.Max.x
            If resultTu(resultTu.Count - 1) = nextTu Then Exit Do ' Condizione di errore
            resultTu.Add(nextTu)
        Loop While nextTu < outerLoop.m_pbox.Max.x
        tu = resultTu.ToArray

        'Crea suddivisioni in V
        Dim resultTv As New List(Of Double)
        resultTv.Add(outerLoop.m_pbox.Min.y)
        Dim nextTv As Double
        Do
            nextTv = Double.PositiveInfinity
            For i As Integer = 0 To guInner.Count - 1
                Dim isoCurve As OnCurve = nurbsSurface.IsoCurve(1, guInner(i))
                Dim isoCurveLength As Double
                isoCurve.GetLength(isoCurveLength)
                Dim initialExternalLength As Double
                Dim isocurveDomain As OnInterval = isoCurve.Domain
                Dim initialExternalLengthInterval As New OnInterval(isocurveDomain.m_t(0), resultTv(0))
                isoCurve.GetLength(initialExternalLength, 0.0001, initialExternalLengthInterval)
                Dim normalisedArcLength As Double = (initialExternalLength + resultTv.Count * maxVArcLength) / isoCurveLength
                Dim tmpTv As Double
                If Not isoCurve.GetNormalizedArcLengthPoint(normalisedArcLength, tmpTv) Then
                    tmpTv = outerLoop.m_pbox.Max.y
                End If
                If tmpTv < nextTv Then nextTv = tmpTv
                If maxVNormalVariation >= 0 Then
                    Dim variationInterval As New OnInterval(resultTv(resultTv.Count - 1), isocurveDomain.m_t(1))
                    tmpTv = GetParameterAtNormalVariation(brepFace, False, guInner(i), maxVNormalVariation, variationInterval)
                    variationInterval.Dispose()
                    If tmpTv < nextTv Then nextTv = tmpTv
                End If
                isocurveDomain.Dispose()
                isoCurve.Dispose()
            Next
            If nextTv > outerLoop.m_pbox.Max.y Then nextTv = outerLoop.m_pbox.Max.y
            If resultTv(resultTv.Count - 1) = nextTv Then Exit Do
            resultTv.Add(nextTv)
        Loop While nextTv < outerLoop.m_pbox.Max.y
        tv = resultTv.ToArray

        nurbsSurface.Dispose()
    End Sub


    Public Shared Function GetParameterAtTangentVariation(ByVal curve As IOnCurve, ByVal tangentVariation As Double, Optional ByVal subDomain As IOnInterval = Nothing) As Double
        Dim nurbsCurve As OnNurbsCurve = curve.NurbsCurve
        Dim subdivisionCount As Integer = 30 * (nurbsCurve.CVCount - 1) + 1 'Questo garantisce un numero congruo di punti analizzati
        Dim variationAngle As Double = -1
        Dim tangents As New On3dVectorArray
        Dim t0 As Double, t1 As Double
        If subDomain Is Nothing Then
            nurbsCurve.GetDomain(t0, t1)
        Else
            t0 = subDomain.m_t(0)
            t1 = subDomain.m_t(1)
        End If
        Dim maxAngleIndex As Integer = -1
        For i As Integer = 0 To subdivisionCount
            Dim t As Double = t0 + i * (t1 - t0) / subdivisionCount
            Dim tangent As On3dVector = nurbsCurve.TangentAt(t)
            For j As Integer = 0 To tangents.Count - 1
                Dim angle As Double = Math.Acos(OnUtil.ON_DotProduct(tangent, tangents(j)))
                If angle > variationAngle Then variationAngle = angle
            Next
            tangents.Append(tangent)
            tangent.Dispose()
            If variationAngle > tangentVariation Then
                maxAngleIndex = i - 1
                Exit For
            End If
        Next
        tangents.Dispose()
        nurbsCurve.Dispose()
        If maxAngleIndex = -1 Then Return t1
        Return t0 + maxAngleIndex * (t1 - t0) / subdivisionCount
    End Function


    Public Shared Function GetParameterAtNormalVariation(ByVal brep As IOnBrepFace, ByVal followU As Boolean, ByVal otherIsocurveParameter As Double, ByVal normalVariation As Double, Optional ByVal subDomain As IOnInterval = Nothing) As Double
        Dim nurbsCurve As OnNurbsCurve = Nothing
        If followU Then
            nurbsCurve = brep.IsoCurve(0, otherIsocurveParameter).NurbsCurve
        Else
            nurbsCurve = brep.IsoCurve(1, otherIsocurveParameter).NurbsCurve
        End If
        Dim subdivisionCount As Integer = 30 * (nurbsCurve.CVCount - 1) + 1 'Questo garantisce un numero congruo di punti analizzati
        Dim variationAngle As Double = -1
        Dim normals As New On3dVectorArray
        Dim t0 As Double, t1 As Double
        If subDomain Is Nothing Then
            nurbsCurve.GetDomain(t0, t1)
        Else
            t0 = subDomain.m_t(0)
            t1 = subDomain.m_t(1)
        End If
        Dim maxAngleIndex As Integer = -1
        For i As Integer = 0 To subdivisionCount
            Dim t As Double = t0 + i * (t1 - t0) / subdivisionCount
            Dim normal As On3dVector = Nothing
            If followU Then
                normal = brep.NormalAt(t, otherIsocurveParameter)
            Else
                normal = brep.NormalAt(otherIsocurveParameter, t)
            End If
            For j As Integer = 0 To normals.Count - 1
                Dim angle As Double = Math.Acos(OnUtil.ON_DotProduct(normal, normals(j)))
                If angle > variationAngle Then variationAngle = angle
            Next
            normals.Append(normal)
            normal.Dispose()
            If variationAngle > normalVariation Then
                maxAngleIndex = i - 1
                Exit For
            End If
        Next
        normals.Dispose()
        nurbsCurve.Dispose()
        If maxAngleIndex = -1 Then Return t1
        Return t0 + maxAngleIndex * (t1 - t0) / subdivisionCount
    End Function


    ''' <summary>
    ''' Calcola l'indice del punto estremo di un array di punti lungo una determinata direzione
    ''' </summary>
    ''' <param name="points"></param>
    ''' <param name="direction"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function ExtremePointDirection(ByVal points As IOn3dPointArray, ByVal direction As IOn3dVector) As Integer
        If points Is Nothing Then Return -1
        If points.Count = 0 Then Return -1
        Dim indice As Integer
        Dim punteggioMax As Double = Double.MinValue
        Dim punteggio As Double
        For i As Integer = 0 To points.Count - 1
            Dim vettorePunto As New On3dVector(points(i))
            punteggio = OnUtil.ON_DotProduct(direction, vettorePunto)
            If punteggio > punteggioMax Then
                indice = i
                punteggioMax = punteggio
            End If
        Next
        Return indice
    End Function



    ''' <summary>
    ''' Calcola (se esiste) il punto su una curva che è la proiezione da un punto esterno lungo una certa direzione
    ''' </summary>
    ''' <param name="punto">Punto da cui si proietta</param>
    ''' <param name="direzione">Direzione lungo la quale si proietta</param>
    ''' <param name="curva">Curva dove calcolare il punto</param>
    ''' <param name="tolleranzaAngolare">Angolo in radianti di errore tollerato sulla direzione di proiezione</param>
    ''' <returns>Ritorna il punto o Nothing se non esiste alcun punto nella tolleranza impostata</returns>
    ''' <remarks></remarks>
    Public Shared Function ProiettaPuntoSuCurvaLungoDirezione(ByVal curva As IOnNurbsCurve, ByVal punto As IOn3dPoint, ByVal direzione As IOn3dVector, Optional ByVal tolleranzaAngolare As Double = 0.2) As On3dPoint
        Const numDivisioni As Integer = 1000
        Dim projectionDirection As New On3dVector(direzione)
        projectionDirection.Unitize()
        Dim tmpPunto As On3dPoint
        Dim t, tRes As Double
        Dim cosenoMax As Double = -1
        Dim delta As Double = curva.Domain.Length / numDivisioni
        For k As Integer = 0 To numDivisioni
            t = curva.Domain.Min + delta * k
            tmpPunto = curva.PointAt(t)
            Dim currentDirection As On3dVector = tmpPunto - punto
            currentDirection.Unitize()
            Dim coseno As Double = OnUtil.ON_DotProduct(currentDirection, projectionDirection)
            If coseno > cosenoMax Then
                cosenoMax = coseno
                tRes = t
            End If
        Next
        If cosenoMax >= Math.Cos(tolleranzaAngolare) Then
            Return curva.PointAt(tRes)
        Else
            Return Nothing
        End If
    End Function



    Public Shared Function IntersectBezierCurves(ByVal bezier0 As IOnBezierCurve, ByVal bezier1 As IOnBezierCurve, ByVal tolerance As Double) As On3dPointArray
        'Crea vettore CV
        Dim cv0(11) As Double
        Dim point As New On3dPoint
        For i As Integer = 0 To 3
            bezier0.GetCV(i, point)
            cv0(3 * i) = point.x
            cv0(3 * i + 1) = point.y
            cv0(3 * i + 2) = point.z
        Next
        Dim cv1(11) As Double
        For i As Integer = 0 To 3
            bezier1.GetCV(i, point)
            cv1(3 * i) = point.x
            cv1(3 * i + 1) = point.y
            cv1(3 * i + 2) = point.z
        Next
        point.Dispose()

        'Suddividi le bezier usando De Casteljau
        'Per ottenere il valore di tolerance sull'intersezione la curva va suddivisa
        ' con una tolleranza inferiore di radice di 2
        Dim subdivisions0 As New List(Of Double())
        Dim tSubdivisions0 As New List(Of Double)
        SubdivideBezier(cv0, subdivisions0, 0, 1, tSubdivisions0, tolerance / Math.Sqrt(2))
        Dim subdivisions1 As New List(Of Double())
        Dim tSubdivisions1 As New List(Of Double)
        SubdivideBezier(cv1, subdivisions1, 0, 1, tSubdivisions1, tolerance / Math.Sqrt(2))

        'Calcola le intersezioni
        Dim result As New On3dPointArray
        Dim pointStart0 As New On3dPoint
        Dim pointEnd0 As New On3dPoint
        Dim pointStart1 As New On3dPoint
        Dim pointEnd1 As New On3dPoint
        Dim line0 As New OnLine
        Dim line1 As New OnLine
        For i As Integer = 0 To subdivisions0.Count - 1
            cv0 = subdivisions0(i)
            pointStart0.Set(cv0(0), cv0(1), cv0(2))
            pointEnd0.Set(cv0(9), cv0(10), cv0(11))
            line0.Create(pointStart0, pointEnd0)
            For j As Integer = 0 To subdivisions1.Count - 1
                cv1 = subdivisions1(j)
                pointStart1.Set(cv1(0), cv1(1), cv1(2))
                pointEnd1.Set(cv1(9), cv1(10), cv1(11))
                line1.Create(pointStart1, pointEnd1)
                Dim a As Double, b As Double
                If OnUtil.ON_Intersect(line0, line1, a, b) Then
                    If a >= 0 And a <= 1 And b >= 0 And b <= 1 Then
                        Dim pt As On3dPoint = line0.PointAt(a)
                        result.Append(pt)
                        pt.Dispose()
                    End If
                End If
            Next
        Next
        line0.Dispose()
        line1.Dispose()
        pointStart0.Dispose()
        pointEnd0.Dispose()
        pointStart1.Dispose()
        pointEnd1.Dispose()
        Return result
    End Function


    Public Shared Function IntersectBezierCurves(ByVal beziers0 As List(Of OnBezierCurve), ByVal beziers1 As List(Of OnBezierCurve), ByVal tolerance As Double) As On3dPointArray
        Dim result As New On3dPointArray
        Dim bbox0 As New OnBoundingBox
        Dim bbox1 As New OnBoundingBox
        For i As Integer = 0 To beziers0.Count - 1
            beziers0(i).GetBoundingBox(bbox0)
            For j As Integer = 0 To beziers1.Count - 1
                beziers1(j).GetBoundingBox(bbox1)
                If bbox0.MinimumDistanceTo(bbox1) < tolerance Then
                    Dim spanIntersections As On3dPointArray = IntersectBezierCurves(beziers0(i), beziers1(j), tolerance)
                    If Not spanIntersections Is Nothing Then
                        For si As Integer = 0 To spanIntersections.Count - 1
                            If result.Search(spanIntersections(si)) = -1 Then
                                result.Append(spanIntersections(si))
                            End If
                        Next
                        spanIntersections.Dispose()
                    End If
                End If
            Next
        Next
        bbox0.Dispose()
        bbox1.Dispose()
        Return result
    End Function


    Public Shared Function GetClosestPoint(ByVal bezier As IOnBezierCurve, ByVal point As IOn3dPoint, ByRef t As Double, ByVal tolerance As Double) As Boolean
        If bezier Is Nothing Then Return False

        'Crea vettore CV
        Dim cv(11) As Double
        Dim pt As New On3dPoint
        For i As Integer = 0 To 3
            bezier.GetCV(i, pt)
            cv(3 * i) = pt.x
            cv(3 * i + 1) = pt.y
            cv(3 * i + 2) = pt.z
        Next
        pt.Dispose()

        'Suddividi la bezier usando De Casteljau
        'Per ottenere il valore di tolerance sull'intersezione la curva va suddivisa
        ' con una tolleranza inferiore di radice di 2
        Dim subdivisions As New List(Of Double())
        Dim tSubdivisions As New List(Of Double)
        SubdivideBezier(cv, subdivisions, 0, 1, tSubdivisions, tolerance / Math.Sqrt(2))

        Dim polyline As New OnPolyline()
        Dim subdivision() As Double
        For i As Integer = 0 To subdivisions.Count - 1
            subdivision = subdivisions(i)
            polyline.Append(subdivision(0), subdivision(1), subdivision(2))
        Next
        subdivision = subdivisions(subdivisions.Count - 1)
        polyline.Append(subdivision(9), subdivision(10), subdivision(11))

        Dim tPolyline As Double
        Dim result As Boolean = polyline.ClosestPointTo(point, tPolyline)
        Dim subDivisionIndex As Integer = CInt(Int(tPolyline))
        If subDivisionIndex = subdivisions.Count Then
            t = subDivisionIndex
        ElseIf subDivisionIndex = subdivisions.Count - 1 Then
            Dim subdivisionRatio As Double = tPolyline - subDivisionIndex
            Dim t0 As Double = tSubdivisions(subDivisionIndex)
            t = t0 + subdivisionRatio * (1 - t0)
        Else
            Dim subdivisionRatio As Double = tPolyline - subDivisionIndex
            Dim t0 As Double = tSubdivisions(subDivisionIndex)
            Dim t1 As Double = tSubdivisions(subDivisionIndex + 1)
            t = t0 + subdivisionRatio * (t1 - t0)
        End If
        polyline.Dispose()
        Return result
    End Function


    Public Shared Function GetClosestPoint(ByVal beziers As List(Of OnBezierCurve), ByVal point As IOn3dPoint, ByRef curveIndex As Integer, ByRef tCurve As Double, ByVal tolerance As Double, Optional ByVal maximumDistance As Double = 0.0) As Boolean
        If beziers Is Nothing OrElse beziers.Count = 0 Then Return False
        If point Is Nothing Then Return False
        Dim processedBeziers As List(Of OnBezierCurve)
        If maximumDistance <= 0 Then
            processedBeziers = beziers
        Else
            processedBeziers = New List(Of OnBezierCurve)
            Dim bbox As New OnBoundingBox
            For i As Integer = 0 To beziers.Count - 1
                If beziers(i).GetBoundingBox(bbox) Then
                    If Not bbox.IsFartherThan(maximumDistance, point) Then processedBeziers.Add(beziers(i))
                End If
            Next
            bbox.Dispose()
        End If

        Dim minDistance As Double = Double.PositiveInfinity
        Dim minDistanceIndex As Integer = -1
        Dim minDistanceT As Double = -1
        For i As Integer = 0 To processedBeziers.Count - 1
            Dim t As Double
            If GetClosestPoint(processedBeziers(i), point, t, tolerance) Then
                Dim closestPoint As On3dPoint = processedBeziers(i).PointAt(t)
                Dim distance As Double = closestPoint.DistanceTo(point)
                closestPoint.Dispose()
                If distance < minDistance Then
                    minDistance = distance
                    minDistanceIndex = i
                    minDistanceT = t
                End If
            End If
        Next

        If minDistanceIndex <> -1 Then
            tCurve = minDistanceT
            curveIndex = beziers.IndexOf(processedBeziers(minDistanceIndex))
            Return True
        End If
        Return False
    End Function



    Public Shared Function GetApproximatingPolyline(ByVal curve As IOnCurve, ByVal sampleCount As Integer) As OnPolyline
        If curve Is Nothing Then Return Nothing
        Dim result As New OnPolyline()
        Dim t As Double
        For i As Integer = 0 To sampleCount - 1
            Dim s As Double = i / (sampleCount - 1)
            curve.GetNormalizedArcLengthPoint(s, t)
            Dim point As On3dPoint = curve.PointAt(t)
            result.Append(point)
            point.Dispose()
        Next
        Return result
    End Function



    ''' <summary>
    ''' La funzione non produce un risultato corretto se i pesi dei punti di controllo sono diversi da 1 (curve razionali)
    ''' </summary>
    ''' <param name="curve"></param>
    ''' <param name="cordalTolerance">Tolleranza cordale di approssimazione</param>
    ''' <returns></returns>
    ''' <remarks>Archi di cerchio ed ellissi vengono preventivamente convertiti in curve di terzo gardo</remarks>
    Public Shared Function GetApproximatingPolyline(ByVal curve As IOnCurve, ByVal cordalTolerance As Double) As OnPolyline
        If curve Is Nothing Then Return Nothing
        'Definisco una tolleranza di riconoscimento della geometria.
        'Tale valore andrebbe passato alla funzione?
        Dim tolerance As Double = cordalTolerance / 10
        Dim result As New OnPolyline()

        If curve.IsKindOf(OnPolyCurve.m_ON_PolyCurve_class_id) Then
            Dim polycurve As IOnPolyCurve = OnPolyCurve.ConstCast(curve)
            Dim segments() As IOnCurve = polycurve.SegmentCurves
            For i As Integer = 0 To segments.GetUpperBound(0)
                Dim segmentPolyline As OnPolyline = GetApproximatingPolyline(segments(i), cordalTolerance)
                For j As Integer = 0 To segmentPolyline.Count - 2
                    result.Append(segmentPolyline(j))
                Next
                segmentPolyline.Dispose()
            Next
            Dim endPoint As On3dPoint = polycurve.PointAtEnd()
            result.Append(endPoint)
            endPoint.Dispose()

        ElseIf curve.IsKindOf(OnArcCurve.m_ON_ArcCurve_class_id) Then
            Dim arcCurve As IOnArcCurve = OnArcCurve.ConstCast(curve)
            If arcCurve.Radius > 0.0 Then
                Dim theta As Double = 0.0
                If arcCurve.Radius > cordalTolerance Then
                    theta = 2 * Math.Acos(1 - cordalTolerance / arcCurve.Radius)
                    theta /= 2       '<-- per uniformare con il comportamento delle nurbscurve
                    If (theta > 22.5 * Math.PI / 180) Then theta = 22.5 * Math.PI / 180
                Else
                    theta = 22.5 * Math.PI / 180
                End If
                Dim arcDomain As OnInterval = arcCurve.m_arc.DomainRadians
                Dim subdivisions As Integer
                If theta > 0.001 Then
                    subdivisions = CInt(Decimal.Ceiling(CDec(arcDomain.Length / theta)))
                Else
                    subdivisions = 1
                End If
                For i As Integer = 0 To subdivisions
                    Dim t As Double = arcDomain.m_t(0) + i * arcDomain.Length / subdivisions
                    Dim point As On3dPoint = arcCurve.m_arc.PointAt(t)
                    result.Append(point)
                    point.Dispose()
                Next
                arcDomain.Dispose()
            End If

        ElseIf curve.IsKindOf(OnLineCurve.m_ON_LineCurve_class_id) Then
            Dim lineCurve As IOnLineCurve = OnLineCurve.ConstCast(curve)
            result.Append(lineCurve.m_line.from)
            result.Append(lineCurve.m_line.to)

        ElseIf curve.IsKindOf(OnPolylineCurve.m_ON_PolylineCurve_class_id) Then
            Dim polylineCurve As IOnPolylineCurve = OnPolylineCurve.ConstCast(curve)
            For i As Integer = 0 To polylineCurve.SpanCount
                result.Append(polylineCurve.m_pline(i))
            Next

        ElseIf curve.IsKindOf(OnNurbsCurve.m_ON_NurbsCurve_class_id) Then
            Dim nurbscurve As IOnNurbsCurve = OnNurbsCurve.ConstCast(curve)
            Dim plane As New OnPlane
            Dim arc As New OnArc
            Dim ellipse As New OnEllipse

            If nurbscurve.IsArc(Nothing, arc, tolerance) Then   'Passo nothing come "plane" per non effettuare il test di planarità
                Dim arcCurve As New OnArcCurve(arc)
                result = GetApproximatingPolyline(arcCurve, cordalTolerance)
                arcCurve.Dispose()

            ElseIf nurbscurve.IsLinear(tolerance) Then
                Dim startPoint As On3dPoint = nurbscurve.PointAtStart
                Dim endPoint As On3dPoint = nurbscurve.PointAtEnd
                result.Append(startPoint)
                result.Append(endPoint)
                startPoint.Dispose()
                endPoint.Dispose()

            Else
                If nurbscurve.IsRational Then
                    Dim nonRationalNurbsCurve As OnNurbsCurve = MakeNonRational(nurbscurve, 100)
                    result = GetNurbsCurveApproximatingPolyline(nonRationalNurbsCurve, cordalTolerance)
                    nonRationalNurbsCurve.Dispose()
                Else
                    result = GetNurbsCurveApproximatingPolyline(nurbscurve, cordalTolerance)
                End If
            End If
            plane.Dispose()
            arc.Dispose()
            ellipse.Dispose()
        End If

        'Chiudi la polilinea se in tolleranza
        If result.Count > 0 Then
            If result(0).DistanceTo(result(result.Count - 1)) < tolerance Then
                result(result.Count - 1).Set(result(0).x, result(0).y, result(0).z)
            End If
        End If

        Return result
    End Function



    ''' <summary>
    ''' Trasforma una curva razionale in non razionale campionandola uniformemente
    ''' </summary>
    ''' <param name="rationalCurve"></param>
    ''' <param name="sampleCount"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function MakeNonRational(ByVal rationalCurve As IOnCurve, ByVal sampleCount As Integer) As OnNurbsCurve
        If rationalCurve Is Nothing Then Return Nothing
        If sampleCount < 3 Then sampleCount = 3
        Dim domain As OnInterval = rationalCurve.Domain

        Dim samples As New On3dPointArray
        For i As Integer = 0 To sampleCount - 1
            Dim point As On3dPoint = rationalCurve.PointAt(domain.ParameterAt(i / (sampleCount - 1)))
            samples.Append(point)
            point.Dispose()
        Next
        domain.Dispose()

        Dim spline As RhSplineCurve
        If rationalCurve.IsClosed() And samples.Count > 3 Then
            samples.Remove()    'Il punto rimosso viene riaggiunto dalla CuSplineCurve con coordinate identiche
            spline = New RhSplineCurve(samples, RhSplineCurve.eKnotsMode.uniform, RhSplineCurve.eEndPointsStyle.periodic)
        Else
            spline = New RhSplineCurve(samples, RhSplineCurve.eKnotsMode.uniform, RhSplineCurve.eEndPointsStyle.parabolic)
        End If
        Dim result As OnNurbsCurve = spline.ConvertToNurbsCurve()
        spline.Dispose()
        samples.Dispose()
        Return result
    End Function



    ''' <summary>
    ''' Calcola l'approssimazione di una curva strettamente OnNurbsCurve in polilinea
    ''' </summary>
    ''' <param name="nurbscurve"></param>
    ''' <param name="tolerance">Tolleranza cordale di approssimazione</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Shared Function GetNurbsCurveApproximatingPolyline(ByVal nurbscurve As IOnNurbsCurve, ByVal tolerance As Double) As OnPolyline
        If nurbscurve Is Nothing Then Return Nothing
        If Not nurbscurve.IsKindOf(OnNurbsCurve.m_ON_NurbsCurve_class_id) Then Return Nothing

        Dim result As New OnPolyline()
        Dim bezier As New OnBezierCurve
        For k As Integer = 0 To nurbscurve.KnotCount - 1        '<-- ciclo fino a KnotCount per prendere in considerazione tutti gli span
            If nurbscurve.ConvertSpanToBezier(k, bezier) Then
                Dim cv(3 * bezier.Order - 1) As Double
                Dim pt As New On3dPoint
                For i As Integer = 0 To bezier.Order - 1
                    bezier.GetCV(i, pt)
                    cv(3 * i) = pt.x
                    cv(3 * i + 1) = pt.y
                    cv(3 * i + 2) = pt.z
                Next
                pt.Dispose()

                Dim subdivisions As New List(Of Double())
                Dim tSubdivisions As New List(Of Double)
                SubdivideBezier(cv, subdivisions, 0, 1, tSubdivisions, tolerance)

                Dim subdivision() As Double
                For i As Integer = 0 To subdivisions.Count - 1
                    subdivision = subdivisions(i)
                    result.Append(subdivision(0), subdivision(1), subdivision(2))
                Next
            End If
        Next
        Dim endPoint As On3dPoint = nurbscurve.PointAtEnd
        result.Append(endPoint)
        endPoint.Dispose()
        bezier.Dispose()
        Return result
    End Function


    ''' <summary>
    ''' Suddivide una Bezier usando l'algoritmo di De Casteljau
    ''' </summary>
    ''' <param name="cv"></param>
    ''' <param name="subdivisions"></param>
    ''' <param name="tStart"></param>
    ''' <param name="tEnd"></param>
    ''' <param name="tSubdivisions"></param>
    ''' <param name="tolerance"></param>
    ''' <remarks></remarks>
    Private Shared Sub SubdivideBezier(ByRef cv() As Double, ByRef subdivisions As List(Of Double()), ByRef tStart As Double, ByRef tEnd As Double, ByVal tSubdivisions As List(Of Double), ByVal tolerance As Double)
        Dim order As Integer = cv.GetLength(0) \ 3
        Dim cvSx(3 * order - 1) As Double
        Dim cvDx(3 * order - 1) As Double
        Array.Copy(cv, cvSx, 3 * order)
        Array.Copy(cv, cvDx, 3 * order)
        OnUtil.ON_EvaluatedeCasteljau(3, order, 0, 3, cvSx, 0.5)
        OnUtil.ON_EvaluatedeCasteljau(3, order, 1, 3, cvDx, 0.5)
        If IsLinearBezier(cvSx, tolerance) Then
            subdivisions.Add(cvSx)
            tSubdivisions.Add(tStart)
        Else
            SubdivideBezier(cvSx, subdivisions, tStart, (tStart + tEnd) / 2, tSubdivisions, tolerance)
        End If
        If IsLinearBezier(cvDx, tolerance) Then
            subdivisions.Add(cvDx)
            tSubdivisions.Add((tStart + tEnd) / 2)
        Else
            SubdivideBezier(cvDx, subdivisions, (tStart + tEnd) / 2, tEnd, tSubdivisions, tolerance)
        End If
    End Sub


    ''' <summary>
    ''' Verifica se una Bezier è lineare sotto una certa tolleranza
    ''' </summary>
    ''' <param name="cv"></param>
    ''' <param name="tolerance"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Shared Function IsLinearBezier(ByVal cv() As Double, ByVal tolerance As Double) As Boolean
        If cv Is Nothing Then Return False
        Dim order As Integer = cv.GetLength(0) \ 3
        Dim points As New On3dPointArray
        For i As Integer = 0 To order - 1
            points.Append(cv(3 * i), cv(3 * i + 1), cv(3 * i + 2))
        Next
        Dim line As New OnLine(points(0), points(order - 1))
        Dim result As Boolean = True
        For i As Integer = 1 To order - 2
            If line.DistanceTo(points(i)) > tolerance Then
                result = False
                Exit For
            End If
        Next
        points.Dispose()
        line.Dispose()
        Return result
    End Function








    Public Shared Function GetLoopAxis(ByVal loopCurve As IOnCurve, ByRef meanPoint As On3dPoint, ByRef meanNormal As On3dVector) As Boolean
        Const SUBDIVISION_COUNT As Integer = 50

        'Calcolo punto medio
        Dim curveDomain As OnInterval = loopCurve.Domain
        If meanPoint Is Nothing Then meanPoint = New On3dPoint
        meanPoint.Set(0, 0, 0)
        For j As Integer = 0 To SUBDIVISION_COUNT - 1
            Dim t As Double = curveDomain.m_t(0) + j * curveDomain.Length / SUBDIVISION_COUNT
            Dim point As On3dPoint = loopCurve.PointAt(t)
            meanPoint += point
            point.Dispose()
        Next
        meanPoint /= SUBDIVISION_COUNT

        'Calcolo asse medio
        If meanNormal Is Nothing Then meanNormal = New On3dVector
        meanNormal.Set(0, 0, 0)
        For j As Integer = 0 To SUBDIVISION_COUNT - 1
            Dim t As Double = curveDomain.m_t(0) + j * curveDomain.Length / SUBDIVISION_COUNT
            Dim point As On3dPoint = loopCurve.PointAt(t)
            Dim tangent As On3dVector = loopCurve.TangentAt(t)
            Dim vector As On3dVector = point - meanPoint
            Dim normal As On3dVector = OnUtil.ON_CrossProduct(tangent, vector)
            normal.Unitize()
            meanNormal += normal
            normal.Dispose()
            vector.Dispose()
            tangent.Dispose()
            point.Dispose()
        Next
        meanNormal /= SUBDIVISION_COUNT
        meanNormal.Unitize()  '<--- Serve perchè credo la tangente a volte può essere nulla

        curveDomain.Dispose()
        Return True
    End Function


    ''' <summary>
    ''' Divide un array di punti in clusters di raggio assegnato
    ''' </summary>
    ''' <param name="points"></param>
    ''' <param name="clustersDistance">clustersDistance è la distanza fra i clusters</param>
    ''' <returns></returns>
    ''' <remarks>Utilizza l'algoritmo k-means per affinare la determinazione dei clusters</remarks>
    Public Shared Function GetClusters(ByVal points As IOn3dPointArray, ByVal clustersDistance As Double, ByVal unitizeCentroids As Boolean) As List(Of On3dPointArray)
        Dim sortedPoints As New On3dPointArray(points)
        sortedPoints.QuickSort(False)      '<-- Aiuta ad uniformare il comportamento nell'estrazione dei seeds

        'Crea la lista dei seeds
        Dim initialCentroids As New On3dPointArray(sortedPoints)
        Dim redundancy As New List(Of Integer)
        For i As Integer = 0 To initialCentroids.Count - 1
            redundancy.Add(1)
        Next
        For i As Integer = initialCentroids.Count - 1 To 1 Step -1
            For j As Integer = i - 1 To 0 Step -1
                If initialCentroids(i).DistanceTo(initialCentroids(j)) <= clustersDistance Then
                    Dim newJPoint As On3dPoint = RhGeometry.MeanPosition(initialCentroids(i), redundancy(i), initialCentroids(j), redundancy(j))
                    If unitizeCentroids Then
                        Dim newJVector As New On3dVector(newJPoint)
                        newJVector.Unitize()
                        newJPoint.Set(newJVector)
                        newJVector.Dispose()
                    End If
                    initialCentroids(j).Set(newJPoint)
                    newJPoint.Dispose()
                    redundancy(j) += redundancy(i)
                    redundancy.RemoveAt(i)
                    initialCentroids.Remove(i)
                    Exit For
                End If
            Next
        Next

        'Crea i clusters iniziali
        Dim result As List(Of On3dPointArray) = RhGeometry.GetClusters(sortedPoints, initialCentroids)

        'Affina i clusters tramite l'algoritmo k-means
        Dim centroids As On3dPointArray = Nothing
        Dim initialDistance As Double = Double.PositiveInfinity
        Dim distanceVariation As Double = Double.PositiveInfinity
        Do
            centroids = New On3dPointArray
            For i As Integer = 0 To result.Count - 1
                Dim centroid As On3dPoint = RhGeometry.GetCentroid(result(i))
                If unitizeCentroids Then
                    Dim centroidVector As New On3dVector(centroid)
                    centroidVector.Unitize()
                    centroid.Set(centroidVector)
                    centroidVector.Dispose()
                End If
                centroids.Append(centroid)
                centroid.Dispose()
                result(i).Dispose()
            Next
            result.Clear()
            result = RhGeometry.GetClusters(sortedPoints, centroids)

            Dim currentDistance As Double = RhGeometry.GetCentroidsDistance(initialCentroids, centroids)
            distanceVariation = Math.Abs(initialDistance - currentDistance)
            initialDistance = currentDistance

            initialCentroids.Dispose()
            initialCentroids = centroids
        Loop Until distanceVariation < 1
        centroids.Dispose()
        sortedPoints.Dispose()
        Return result
    End Function


    ''' <summary>
    ''' Divide un array di punti in clusters partendo da un array di seeds
    ''' </summary>
    ''' <param name="points"></param>
    ''' <param name="seeds"></param>
    ''' <returns></returns>
    ''' <remarks>Alcuni seed potrebbero non generare alcun cluster</remarks>
    Public Shared Function GetClusters(ByVal points As IOn3dPointArray, ByVal seeds As On3dPointArray) As List(Of On3dPointArray)
        Dim result As New List(Of On3dPointArray)
        For i As Integer = 0 To seeds.Count - 1
            result.Add(New On3dPointArray)
        Next
        Dim closestCentroidIndex As Integer
        For i As Integer = 0 To points.Count - 1
            If seeds.GetClosestPoint(points(i), closestCentroidIndex) Then
                result(closestCentroidIndex).Append(points(i))
            End If
        Next
        For i As Integer = result.Count - 1 To 0 Step -1
            If result(i).Count = 0 Then
                result(i).Dispose()
                result.RemoveAt(i)
            End If
        Next
        Return result
    End Function


    ''' <summary>
    ''' Get the centroid of an array of points
    ''' </summary>
    ''' <param name="cluster"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function GetCentroid(ByVal cluster As IOn3dPointArray) As On3dPoint
        If cluster Is Nothing Then Return Nothing
        If cluster.Count = 0 Then Return Nothing
        Dim result As New On3dPoint
        For i As Integer = 0 To cluster.Count - 1
            result += cluster(i)
        Next
        result /= cluster.Count
        Return result
    End Function


    Private Shared Function GetCentroidsDistance(ByVal centroids0 As IOn3dPointArray, ByVal centroids1 As IOn3dPointArray) As Double
        If centroids0 Is Nothing Then Return 0
        If centroids1 Is Nothing Then Return 0
        If centroids0.Count <> centroids1.Count Then Return 0
        Dim result As Double = 0
        For i As Integer = 0 To centroids0.Count - 1
            result += centroids0(i).DistanceTo(centroids1(i))
        Next
        Return result
    End Function


    'ROBERTO: Nuova funzione
    Public Shared Function IntersectPolylinePlane(ByVal polyline As IOnPolyline, ByVal plane As IOnPlane) As Double()
        If polyline Is Nothing Then Return Nothing
        If plane Is Nothing Then Return Nothing
        Dim t As Double
        Dim line As OnLine
        Dim result As New List(Of Double)
        For i As Integer = 0 To polyline.Count - 2
            line = New OnLine(polyline(i), polyline(i + 1))
            If OnUtil.ON_Intersect(line, plane, t) Then
                If t >= 0 And t <= 1 Then
                    result.Add(i + t)
                End If
            End If
            line.Dispose()
        Next
        'Elimino gli eventuali doppi (lavorare in tolleranza?)
        For i As Integer = result.Count - 1 To 1 Step -1
            If result(i) = result(i - 1) Then result.RemoveAt(i)
        Next
        Return result.ToArray
    End Function


    'ROBERTO: Nuova funzione

    ''' <summary>
    ''' Calcola il punto più vicino ad una polilinea
    ''' </summary>
    ''' <param name="polyline"></param>
    ''' <param name="point"></param>
    ''' <returns></returns>
    ''' <remarks>La funzione è necessaria per evitare un baco di OpenNURBS</remarks>
    Public Shared Function PolylineClosestPoint(ByVal polyline As IOnPolyline, ByVal point As IOn3dPoint) As On3dPoint
        If polyline Is Nothing Then Return Nothing
        If point Is Nothing Then Return Nothing
        Dim t As Double
        If Not polyline.ClosestPointTo(point, t) Then Return Nothing
        Dim result As On3dPoint = Nothing
        If t = polyline.Count - 1 Then
            result = New On3dPoint(polyline.Last)
        Else
            result = polyline.PointAt(t)
        End If
        Return result
    End Function


    'ROBERTO: Nuova funzione

    ''' <summary>
    ''' Data una polylinea sul piano XY esegue il test di contenimento di un punto
    ''' </summary>
    ''' <param name="polyline">La polilinea deve giacere nel piano XY ed essere chiusa</param>
    ''' <param name="x"></param>
    ''' <param name="y"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function IsInClosedPolyline(ByVal polyline As IOnPolyline, ByVal x As Double, ByVal y As Double) As Boolean
        Dim intersectionCount As Integer = 0
        Dim pt0 As IOn3dPoint = Nothing
        Dim pt1 As IOn3dPoint = Nothing
        For i As Integer = 0 To polyline.Count - 2
            Dim i1 As Integer = i + 1
            If i1 = polyline.Count - 1 Then i1 = 0

            If Math.Abs(polyline(i1).y - polyline(i).y) > 0.000001 Then
                If polyline(i1).y > polyline(i).y Then
                    pt0 = polyline(i)
                    pt1 = polyline(i1)
                Else
                    pt0 = polyline(i1)
                    pt1 = polyline(i)
                End If

                If (y > pt0.y) And (y <= pt1.y) Then
                    Dim xIntersection As Double = pt0.x + (y - pt0.y) * (pt1.x - pt0.x) / (pt1.y - pt0.y)
                    If x <= xIntersection Then intersectionCount += 1
                End If
            End If
        Next
        Return (intersectionCount Mod 2) = 1
    End Function



    ''' <summary>
    ''' Seziona una superfice con un piano
    ''' </summary>
    ''' <param name="plane"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function IntersectSurfaceWithPlane(ByVal surface As IOnSurface, ByVal plane As IOnPlane) As OnCurve()
        If surface Is Nothing Then Return Nothing
        If plane Is Nothing Then Return Nothing

        Dim surfaceBBox As New OnBoundingBox
        Dim xform As New OnXform
        xform.ChangeBasis(OnUtil.On_xy_plane, plane)
        surface.GetTightBoundingBox(surfaceBBox, 0, xform)
        xform.Dispose()

        Dim planarSurface As OnSurface = RhGeometry.CreatePlanarSurface(plane, surfaceBBox.m_min.x, surfaceBBox.m_min.y, surfaceBBox.m_max.x, surfaceBBox.m_max.y)
        Dim planeBrep As New OnBrep()
        planeBrep.Create(planarSurface)

        Dim sectionCurves() As OnCurve = Nothing
        Dim sectionPoints As New On3dPointArray
        Dim surfaceBrep As OnBrep = surface.BrepForm
        RhUtil.RhinoIntersectBreps(surfaceBrep, planeBrep, RhUtil.RhinoApp.ActiveDoc.AbsoluteTolerance, sectionCurves, sectionPoints)
        surfaceBrep.Dispose()
        planeBrep.Dispose()
        Return sectionCurves
    End Function



    ''' <summary>
    ''' Esegue uno smoothing di una curva
    ''' </summary>
    ''' <param name="curve">Curva di cui eseguire lo smoothing</param>
    ''' <param name="axis">0 per l'asse X, 1 per l'asse Y e 2 per l'asse Z</param>
    ''' <param name="factor">Fattore di smoothing</param>
    ''' <param name="plane">Piano di riferimento. Può essere nothing</param>
    ''' <param name="moveOpenCurveEndPoints">Stabilisce se muovere gli estremi delle curve aperte</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function SmoothCurve(ByVal curve As OnNurbsCurve, ByVal axis As Integer, ByVal factor As Double, ByVal plane As IOnPlane, ByVal moveOpenCurveEndPoints As Boolean) As Boolean
        If curve Is Nothing Then Return False
        If axis < 0 Or axis > 2 Then Return False

        'Esprimi la curva nel sistema locale
        Dim xform As New OnXform(1)
        If plane IsNot Nothing Then
            xform.ChangeBasis(OnUtil.On_xy_plane, plane)
            curve.Transform(xform)
        End If

        'Carico i vecchi cv
        Dim cv As New On3dPoint
        Dim cvs(3 * curve.CVCount - 1) As Double
        For i As Integer = 0 To curve.CVCount - 1
            curve.GetCV(i, cv)
            cvs(3 * i) = cv.x
            cvs(3 * i + 1) = cv.y
            cvs(3 * i + 2) = cv.z
        Next
        Dim newCvs(cvs.GetUpperBound(0)) As Double
        Array.Copy(cvs, newCvs, cvs.GetLength(0))

        'Esegui lo smooth
        If curve.IsClosed Then
            For i As Integer = 1 To curve.CVCount - 3
                Dim j As Integer = 3 * i + axis
                newCvs(j) = cvs(j) + 0.5 * factor * (cvs(j + 3) - cvs(j)) + 0.5 * factor * (cvs(j - 3) - cvs(j))
            Next
            newCvs(axis) = newCvs(3 * (curve.CVCount - 3) + axis)
            newCvs(3 * (curve.CVCount - 2) + axis) = newCvs(3 + axis)
            newCvs(3 * (curve.CVCount - 1) + axis) = newCvs(6 + axis)
        Else
            If moveOpenCurveEndPoints Then
                Dim j As Integer = axis
                newCvs(j) = cvs(j) + factor * (cvs(j + 3) - cvs(j))
                j = 3 * (curve.CVCount - 1) + axis
                newCvs(j) = cvs(j) + factor * (cvs(j - 3) - cvs(j))
            End If
            For i As Integer = 1 To curve.CVCount - 2
                Dim j As Integer = 3 * i + axis
                newCvs(j) = cvs(j) + 0.5 * factor * (cvs(j + 3) - cvs(j)) + 0.5 * factor * (cvs(j - 3) - cvs(j))
            Next
        End If

        'Aggiorno i cv
        For i As Integer = 0 To curve.CVCount - 1
            cv.x = newCvs(3 * i)
            cv.y = newCvs(3 * i + 1)
            cv.z = newCvs(3 * i + 2)
            curve.SetCV(i, cv)
        Next
        cv.Dispose()

        'Esprimi la curva nel sistema globale
        If plane IsNot Nothing Then
            xform.ChangeBasis(plane, OnUtil.On_xy_plane)
            curve.Transform(xform)
        End If
        Return True
    End Function


    ''' <summary>
    ''' Elimina la componente z dei punti nell'array
    ''' </summary>
    ''' <param name="pointsArray"></param>
    ''' <remarks></remarks>
    Public Shared Function ProjectPoints(ByVal pointsArray As On3dPointArray) As On3dPointArray
        Dim res As New On3dPointArray
        For i As Integer = 0 To pointsArray.Count - 1
            Dim newPoint As New On3dPoint(pointsArray(i))
            newPoint.z = 0
            res.Append(newPoint)
        Next
        Return res
    End Function


    ''' <summary>
    ''' Stesso risultato del comando _Explode
    ''' </summary>
    ''' <param name="polysurfaceRef"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function GetPolysurfacePart(ByVal polysurfaceRef As MRhinoObjRef) As MRhinoObjRefArray
        Dim result As New MRhinoObjRefArray
        Dim edges As New List(Of MRhinoObject)
        For i As Integer = 0 To polysurfaceRef.Geometry.BrepForm.m_E.Count - 1
            edges.Add(RhUtil.RhinoApp.ActiveDoc.AddCurveObject(polysurfaceRef.Geometry.BrepForm.m_E(i)))
        Next
        '#If DEBUG Then
        '        IdTestMacchioneCmd.AddDocumentToDebug(polysurfaceRef.Geometry.BrepForm, "originale")
        '#End If
        RhUtil.RhinoApp.RunScript("_SelNone", 0)
        polysurfaceRef.Object.Select(True, True)
        Dim stringaSplit As String = "-_Split"
        For Each edge As MRhinoObject In edges
            stringaSplit &= " _SelID " & edge.Attributes.m_uuid.ToString
        Next
        stringaSplit &= " _Enter"
        RhUtil.RhinoApp.RunScript(stringaSplit, 1)
        RhUtil.RhinoApp.RunScript("_SelNone", 0)
        RhUtil.RhinoApp.RunScript("_SelLast", 0)
        Dim getObjects As New MRhinoGetObject
        getObjects.SetGeometryFilter(IRhinoGetObject.GEOMETRY_TYPE_FILTER.surface_object Or IRhinoGetObject.GEOMETRY_TYPE_FILTER.polysrf_object)
        getObjects.GetObjects(0, Integer.MaxValue)
        For i As Integer = 0 To getObjects.ObjectCount - 1
            result.Append(getObjects.Object(i))
        Next
        'Pulizia
        For Each edgeObj As MRhinoObject In edges
            RhUtil.RhinoApp.ActiveDoc.PurgeObject(edgeObj)
        Next
        edges.Clear()
        getObjects.Dispose()
        Return result
    End Function


    ''' <summary>
    ''' Estrae la curva di bordo di una superficie trimmata
    ''' </summary>
    ''' <param name="uuid"></param>
    ''' <returns>Ritorna una curva unica altrimenti nothing</returns>
    ''' <remarks></remarks>
    Public Shared Function CurvaDiBordoUnicaSrfTrimmata(ByVal uuid As Guid) As OnCurve
        Dim objref As New MRhinoObjRef(uuid)
        If objref Is Nothing Then Return Nothing
        Dim obj As IRhinoObject = objref.Object
        objref.Dispose()
        'Estraggo curva di bordo superficie superiore
        Dim inputCurves As New List(Of OnCurve)
        For Each edge As OnBrepEdge In obj.Geometry.BrepForm.m_E
            inputCurves.Add(edge.NurbsCurve.DuplicateCurve())
        Next
        Dim output() As OnCurve = {}
        RhUtil.RhinoMergeCurves(inputCurves.ToArray(), output)
        'If output.Length <> 1 Then Return Nothing
        'Return output(0)
        Dim result As OnCurve = Nothing
        If output.Length = 1 Then Return output(0)
        'Se vengono trovate diverse curve ritorno quella più esterna
        Dim maxX As Double = Double.MinValue
        Dim maxY As Double = Double.MinValue
        Dim maxZ As Double = Double.MinValue
        Dim minX As Double = Double.MaxValue
        Dim minY As Double = Double.MaxValue
        Dim minZ As Double = Double.MaxValue
        For i As Integer = 0 To output.Length - 1
            Dim curve As OnCurve = output(i)
            If (curve.BoundingBox.m_max.x > maxX And curve.BoundingBox.m_max.y > maxY And curve.BoundingBox.m_max.z > maxZ) And
               (curve.BoundingBox.m_min.x < minX And curve.BoundingBox.m_min.y < minY And curve.BoundingBox.m_min.z < minZ) Then
                result = curve
                maxX = curve.BoundingBox.m_max.x
                maxY = curve.BoundingBox.m_max.y
                maxZ = curve.BoundingBox.m_max.z
            End If
        Next
        Return result
    End Function

    Public Shared Function FindNearestCV(ByRef testPoint As On3dPoint, ByRef surface As IOnSurface) As On3dPoint
        Dim result As On3dPoint = Nothing
        Dim minDistance As Double = Double.MaxValue
        Dim cv0 As Integer = surface.NurbsSurface.m_cv_count(0)
        Dim cv1 As Integer = surface.NurbsSurface.m_cv_count(1)
        'Dim insoleCV(cv0 - 1, cv1 - 1) As On3dPoint
        For i As Integer = 0 To cv0 - 1
            For j As Integer = 0 To cv1 - 1
                Dim point As New On3dPoint
                surface.NurbsSurface.GetCV(i, j, point)
                If testPoint.DistanceTo(point) < minDistance Then
                    minDistance = testPoint.DistanceTo(point)
                    result = New On3dPoint(testPoint)
                End If
                point.Dispose()
            Next
        Next
        Return result
    End Function


    ''' <summary>
    ''' Inverte una curva
    ''' </summary>
    ''' <param name="rhinoOgj"></param>
    ''' <remarks></remarks>
    Public Shared Sub FlipCurve(ByRef rhinoOgj As MRhinoObject)
        RhUtil.RhinoApp.RunScript("_SelNone", 0)
        RhUtil.RhinoApp.RunScript("_SelId " & rhinoOgj.Attributes.m_uuid.ToString(), 0)
        RhUtil.RhinoApp.RunScript("-_Flip _Enter", 0)
        RhUtil.RhinoApp.RunScript("_SelLast", 0)
        Dim getObjectUpper As New MRhinoGetObject
        getObjectUpper.SetGeometryFilter(IRhinoGetObject.GEOMETRY_TYPE_FILTER.curve_object)
        If getObjectUpper.GetObjects(0, 1) = IRhinoGet.result.object Then
            'Per sicurezza riprendo il nuovo riferimento che potrebbe essere cambiato
            rhinoOgj = RhUtil.RhinoApp.ActiveDoc.LookupObject(getObjectUpper.Object(0).Object.Attributes.m_uuid)
        End If
    End Sub


    ''' <summary>
    ''' Esegue lo split di una curva attraverso un'array di punti e ritorna gli spezzoni di curva
    ''' </summary>
    ''' <param name="curve">Curva da trimmare</param>
    ''' <param name="splittingPoints">Punti di taglio</param>
    ''' <returns>Spezzoni di curva trimmata</returns>
    ''' <remarks>Se c'è un solo punto di intersezione non vengono tornate curve tagliate</remarks>
    Public Shared Function SplitCurve(curve As IOnCurve, splittingPoints As On3dPointArray) As OnCurveArray
        Dim result As New OnCurveArray
        Dim curveIntersectionParameters As New List(Of Double)
        Dim points As New On3dPointArray
        For Each point As On3dPoint In splittingPoints
            points.Append(point)
        Next
        If Not curve.IsClosed Then
            points.Append(curve.PointAtStart)
            If Not curve.IsClosed Then points.Append(curve.PointAtEnd)
        End If
        For i As Integer = 0 To points.Count - 1
            Dim t As Double = 0 'Parametro sulla curva relativo al punto d'intersezione fra la curva e la superficie
            If Not curve.GetClosestPoint(points(i), t) Then Continue For
            curveIntersectionParameters.Add(t)
        Next
        curveIntersectionParameters.Sort()
        For i As Integer = 0 To curveIntersectionParameters.Count - 2
            Dim domain As OnInterval = Nothing
            If curve.IsClosed Then
                If i = curveIntersectionParameters.Count - 2 Then
                    domain = New OnInterval(curveIntersectionParameters(i + 1), curveIntersectionParameters(0))
                    result.Append(OnUtil.ON_TrimCurve(curve, domain))
                End If
            End If
            domain = New OnInterval(curveIntersectionParameters(i), curveIntersectionParameters(i + 1))
            result.Append(OnUtil.ON_TrimCurve(curve, domain))
        Next
        Return result
    End Function


    ''' <summary>
    ''' NON AGGIUNGERE ALLE UTILS, ESISTE GIA'
    ''' </summary>
    ''' <param name="curves"></param>
    ''' <returns></returns>
    Public Shared Function GetLongestCurve(ByVal curves As OnCurveArray) As OnCurve
        Dim maxLenght As Double = Double.MinValue
        Dim tempIntSplitCurve As OnCurve = curves(0)
        For Each curve As OnCurve In curves
            Dim lenght As Double = 0
            curve.GetLength(lenght)
            If lenght > maxLenght Then
                tempIntSplitCurve = curve
                maxLenght = lenght
            End If
        Next
        Return tempIntSplitCurve
    End Function


        ''' <summary>
    ''' Restituisce la curva con la Z minore
    ''' </summary>
    Public Shared Function GetLowestCurve(ByVal curves As OnCurveArray) As OnCurve
        Dim result As OnCurve = Nothing
        Dim currentMinZ As Double = Double.MaxValue
        For Each curve As OnCurve In curves
            Dim curveMinZ As Double = curve.BoundingBox().m_min.z
            If curveMinZ < currentMinZ Then
                result = curve
                currentMinZ = curveMinZ
            End If
        Next
        Return result
    End Function


    
    ''' <summary>
    ''' Split una curva attraverso una superficie
    ''' </summary>
    ''' <param name="curveToSplit">Curva da splittare</param>
    ''' <param name="splittingSurface">Superficie di taglio</param>
    ''' <returns>Elenco di curve splittare</returns>
    ''' <remarks></remarks>
    Public Shared Function SplitCurveBySurface(curveToSplit As IOnCurve, splittingSurface As IOnSurface, ByRef intersectionPoints As On3dPointArray) As OnCurveArray
        Dim intersectionEvents As New ArrayOnX_EVENT 'Contiene l'elenco dei punti d'intersezione
        curveToSplit.IntersectSurface(splittingSurface, intersectionEvents)
        'Si instanziano i punti di intersezione
        intersectionPoints = New On3dPointArray
        If intersectionEvents.Count > 0 Then  'Se il numero di intersezioni è maggiore di 1
            For Each intersectionEvent As OnX_EVENT In intersectionEvents
                intersectionPoints.Append(intersectionEvent.m_pointA(0))
            Next
        End If
        'Elenco di curve splittate in base ai punti di intersezione
        Return SplitCurve(curveToSplit, intersectionPoints)
    End Function


      ''' <summary>
    ''' Restituisce la curva più centrata rispetto a quella originale
    ''' </summary>
    ''' <param name="curves">Curve tra le quali cercare la più centrata</param>
    ''' <param name="oldCurve">Curva originale da cui derivano le curve dell'array</param>
    ''' <returns>Curva più centrata</returns>
    Public Shared Function GetCenteredCurve(ByVal curves As OnCurveArray, ByVal oldCurve As IOnCurve) As OnCurve
        Dim result As OnCurve = Nothing
        Dim centerOldCurve As On3dPoint = oldCurve.BoundingBox().Center()
        Dim minCenterDistance As Double = Double.MaxValue
        For Each curve As OnCurve In curves
#If DEBUG Then
            'AddDocumentToDebug(curve, "Curva i-esima pre-taglio verticale")
#End If
            Dim curveCenterProjection As New On3dPoint(curve.BoundingBox().Center().x, curve.BoundingBox().Center().y, centerOldCurve.z)
            Dim centerDistance As Double = curveCenterProjection.DistanceTo(centerOldCurve)
            If centerDistance < minCenterDistance Then
                result = curve
                minCenterDistance = centerDistance
            End If
            curveCenterProjection.Dispose()
        Next
        centerOldCurve.Dispose()
#If DEBUG Then
        'AddDocumentToDebug(result, "Curva + centrata dopo taglio verticale")
#End If
        Return result
    End Function


    
    Public Shared Function GetPolylineOfRhinoObject(ByVal rhinoObj As IRhinoObject) As OnPolyline
        Dim result As OnPolyline = Nothing
        If rhinoObj Is Nothing Then Return result

        Dim objRef As MRhinoObjRef = New MRhinoObjRef(rhinoObj.Attributes.m_uuid)
        Dim onCurve As IOnCurve = objRef.Curve()
        If onCurve Is Nothing Then Return result
        If onCurve.IsPolyline() >= 2 Then
            Dim pline_crv As IOnPolylineCurve = OnPolylineCurve.ConstCast(onCurve)
            result = New OnPolyline(pline_crv.m_pline)
        Else
            result = GetApproximatingPolyline(onCurve, 10)
        End If

        objRef.Dispose()
        Return (result)
    End Function



   
    ''' <summary>
    ''' Offset in XY di una curva che varia in XYZ 
    ''' </summary>
    ''' <param name="originalCurve"></param>
    ''' <param name="offsetDistance"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function ManualOffsetCurve(ByRef originalCurve As IOnCurve, ByVal offsetDistance As Double, Optional ByVal campioni As Integer = 1000) As OnCurve
        If originalCurve Is Nothing OrElse Not originalCurve.IsValid Then Return Nothing
#If DEBUG Then
        'AddDocumentToDebug(originalCurve, "pre offset curve")
#End If
        Dim newPoints As New On3dPointArray
        'Divido il dominio della curva per campionare N punti
        Dim domain As OnInterval = originalCurve.Domain
        Dim rotationAngle As Double = Math.PI / 2
        'Il verso di rotazione cambia a seconda del verso della curva -> quindi calcolo la tangente in minX
        Dim minX As New On3dPoint
        Dim maxX As New On3dPoint
        Dim minY As New On3dPoint
        Dim maxY As New On3dPoint
        RhGeometry.CurveFindExtremePoints(originalCurve, minX, maxX, minY, maxY)
        Dim t As Double
        originalCurve.GetClosestPoint(minX, t)
        Dim normal As On3dVector = originalCurve.TangentAt(t)
        If normal.y < 0 Then rotationAngle = -rotationAngle
        'Scorro punti nel dominio dell curva
        Dim delta As Double = (domain.Max - domain.Min) / campioni
        For i As Integer = 0 To 1000
            t = domain.Min + i * delta
            Dim oldPoint As On3dPoint = originalCurve.PointAt(t)
            normal = originalCurve.TangentAt(t)
            normal.z = 0
            normal.Rotate(rotationAngle, OnUtil.On_zaxis)
            normal.Unitize()
            Dim xform As New OnXform
            xform.Scale(oldPoint, offsetDistance)
            normal.Transform(xform)
            Dim newPoint As On3dPoint = normal + oldPoint
            newPoint.z = oldPoint.z
            newPoints.Append(newPoint)
#If DEBUG Then
            'AddDocumentToDebug(oldPoint, "oldPoint" & i)
            'AddDocumentToDebug(newPoint, "newPoint" & i)
#End If
            oldPoint.Dispose()
            newPoint.Dispose()
            xform.Dispose()
        Next
        'Ricreo curva
        Dim offsetCrv As OnNurbsCurve = RhUtil.RhinoInterpCurve(3, newPoints, Nothing, Nothing, 0)
        Dim result As OnNurbsCurve = RhUtil.RhinoRebuildCurve(offsetCrv, 3, 100)
#If DEBUG Then
        'AddDocumentToDebug(offsetCrv, "offsetCrv")
        'AddDocumentToDebug(result, "offsetCrvRebuilded")
#End If
        'Dispose
        domain.Dispose()
        normal.Dispose()
        newPoints.Dispose()
        Return result.DuplicateCurve()
    End Function




    Public Enum eCrossSectionStart
        maxX
        minX
        maxY
        minY
    End Enum


     ''' <summary>
    ''' Data la curva restituisce un numero di sezioni calcolate in base alla lunghezza della curva e al raccordo
    ''' </summary>
    ''' <param name="railCurveID"></param>
    ''' <param name="startPoint"></param>
    ''' <param name="crossSectionStart"></param>
    ''' <param name="blendRadius"></param>
    ''' <param name="numberOfItems">Se non viene specificato un numero > 0 il numero di sezioni viene calcolato</param>
    ''' <returns>ID DELLE CURVE DI SEZIONE AGGIUNTE AL DOCUMENTO</returns>
    ''' <remarks>Impostare numberOfItems solo se certi altrimenti viene calcolato un numero che rende il comando stabile in base ai test effettiati</remarks>
    Public Shared Function GetSweepCrossSection(ByVal railCurveID As Guid, ByRef startPoint As On3dPoint, ByVal crossSectionStart As eCrossSectionStart,
                                                ByVal blendRadius As Double, Optional ByVal numberOfItems As Integer = 0) As SortedList(Of Double, Guid)
        Dim result As New SortedList(Of Double, Guid)
        Dim maxX As New On3dPoint
        Dim minX As New On3dPoint
        Dim minY As New On3dPoint
        Dim maxY As New On3dPoint
        Dim railCurve As IOnCurve = New MRhinoObjRef(railCurveID).Curve
        'Se non è stato specificato un numero di sezioni viene calcolato
        If Not numberOfItems > 0 Then
            'In presenza di curvature accentuate del rail si rischia che due sezioni consecutive si intersechino con conseguente fallimento dello sweep
            'La distanza minima andrebbe calcolata in base alla curvatura ma in assenza di spigoli(che comunque farebbero fallire) empiricamente imposto una distanza minima
            Dim lenght As Double
            railCurve.GetLength(lenght)
            Dim minDistance As Double = blendRadius * 4
            Dim distance As Double = lenght / numberOfItems
            If distance < minDistance Then numberOfItems = CInt(lenght / minDistance)
            Dim minSection As Integer = 10
            Dim maxSection As Integer = 100
            If numberOfItems < minSection Then numberOfItems = minSection
            If numberOfItems > maxSection Then numberOfItems = maxSection
        End If
        'Cacolo punti iniziali
        RhGeometry.CurveFindExtremePoints(railCurve, minX, maxX, minY, maxY)
        Select Case crossSectionStart
            Case eCrossSectionStart.maxX
                startPoint = maxX
            Case eCrossSectionStart.minX
                startPoint = minX
            Case eCrossSectionStart.maxY
                startPoint = maxY
            Case eCrossSectionStart.minY
                startPoint = minY
        End Select
        'MaxX
        Dim tubeCenterPlane As New OnPlane(startPoint, OnUtil.On_xaxis, OnUtil.On_zaxis)
        Dim tubeCircleStart As New OnCircle(tubeCenterPlane, startPoint, blendRadius)
        Dim objTubeCircleStart As MRhinoObject = RhUtil.RhinoApp.ActiveDoc.AddCurveObject(tubeCircleStart)
        RhUtil.RhinoApp().RunScript("_SelNone", 0)
        objTubeCircleStart.Select(True, True)
        'Per rinforzare l'algoritmo passo come input allo script anche il parametro "BasePoint" che calcolo sulla rail curve vicino al primo cerchio
        Dim basePoint As String = startPoint.x.ToString.Replace(",", ".") & "," & startPoint.y.ToString.Replace(",", ".") & "," & startPoint.z.ToString.Replace(",", ".")
        'App.RunScript("-_ArrayCrv _SelID " & railCurveID.ToString() & "_O=Freeform _D 5", 0)
        RhUtil.RhinoApp().RunScript("-_ArrayCrv _B " & basePoint & " _SelID " & railCurveID.ToString() & " _O=Freeform " & numberOfItems, 0)
        RhUtil.RhinoApp().RunScript("_SelLast", 0)
        Dim getObjects As New MRhinoGetObject
        getObjects.SetGeometryFilter(IRhinoGetObject.GEOMETRY_TYPE_FILTER.curve_object)
        getObjects.GetObjects(0, Integer.MaxValue)
        Dim domain As Double
        For i As Integer = 0 To getObjects.ObjectCount - 1
            railCurve.GetClosestPoint(getObjects.Object(i).Curve.PointAt(0), domain)
            If Not result.ContainsKey(domain) Then result.Add(domain, getObjects.Object(i).m_uuid)
        Next
        ''Aggiungo quello originale
        'railCurve.GetClosestPoint(tubeCircleStart.PointAt(0), domain)
        'If Not result.ContainsKey(domain) Then result.Add(domain, objTubeCircleStart.Attributes.m_uuid)
        ''AVENDO AGGIUNTO IL PARAMETRO "basePoint" CREA SOLO PROBLEMI QUINDI LO RIMUOVO INVECE DI AGGIUNGERLO
        RhUtil.RhinoApp.ActiveDoc.PurgeObject(objTubeCircleStart)
        RhUtil.RhinoApp().RunScript("_SelNone", 0)
        getObjects.Dispose()
        objTubeCircleStart.Dispose()
        tubeCenterPlane.Dispose()
        tubeCircleStart.Dispose()
        Return result
    End Function



End Class
