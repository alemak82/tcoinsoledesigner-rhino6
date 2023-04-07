Imports RMA.OpenNURBS
Imports RMA.Rhino
Imports InsoleDesigner.bll.IdAlias
Imports InsoleDesigner.bll.IdElement3dManager
Imports InsoleDesigner.bll.IdLanguageManager
Imports BrepUtil
Imports System.Reflection
Imports RhinoUtils
Imports RhinoUtils.RhGeometry


Public Class IdGeometryUtils


  Public Enum eFilletSide
    inner
    outer
  End Enum

  ''' <summary>
  ''' Crea il raccordo tra due curve lineari non parallele
  ''' </summary>
  ''' <param name="curve1">Curva 1 convertibile il OnLineCurve</param>
  ''' <param name="curve2">Curva 2 convertibile il OnLineCurve</param>
  ''' <param name="plane">Piano in comune tra le due curve(Es se le curve hanno Z costante piano XY)</param>
  ''' <param name="radius">Raggio di raccordo(raggio dell'arco di circonferenza usato per il raccordo)</param> 
  ''' <param name="side">indica se calcolare il raccordo dove le curve sono più vicine o si intersecano(INNER) oppure dall'altro verso(OUTER)</param>   
  ''' <returns>Curva che raccorda in tangenza curve1 e curve2</returns>
  ''' <remarks></remarks>
  Public Shared Function FilletLineCurve(ByVal curve1 As OnCurve, ByVal curve2 As OnCurve, ByVal plane As OnPlane, ByVal radius As Double, Optional ByVal side As eFilletSide = eFilletSide.inner) As OnNurbsCurve
    ''CONTROLLI
    If Not curve1.IsPlanar(plane) Or Not curve2.IsPlanar(plane) Then Return Nothing
    If Not curve1.IsLinear() Or Not curve2.IsLinear() Then Return Nothing
    Dim lineCurve1 As New OnLineCurve(curve1.PointAtStart, curve1.PointAtEnd)
    Dim lineCurve2 As New OnLineCurve(curve2.PointAtStart, curve2.PointAtEnd)
    'Creo la superficie per trovre il punto di intersezione estrudendo la curva estesa        
    Dim extendCrv1 As OnCurve = lineCurve1.Duplicate()
    Dim extendCrv2 As OnCurve = lineCurve2.Duplicate()
    RhUtil.RhinoExtendCurve(extendCrv1, IRhinoExtend.Type.Line, 0, Int16.MaxValue)
    RhUtil.RhinoExtendCurve(extendCrv1, IRhinoExtend.Type.Line, 1, Int16.MaxValue)
    RhUtil.RhinoExtendCurve(extendCrv2, IRhinoExtend.Type.Line, 0, Int16.MaxValue)
    RhUtil.RhinoExtendCurve(extendCrv2, IRhinoExtend.Type.Line, 1, Int16.MaxValue)
    '#If DEBUG Then
    '        AddDocumentToDebug(extendCrv1, "extendCrv1")
    '        AddDocumentToDebug(extendCrv2, "extendCrv2")
    '#End If
    Dim curvesMaxDistance As Double = curve1.BoundingBox.MaximumDistanceTo(curve2.BoundingBox)
    Dim extrusionSrf1 As OnSurface = RhUtil.RhinoExtrudeCurveStraight(extendCrv1, plane.Normal, curvesMaxDistance * 2)
    Dim xform As New OnXform
    xform.Translation(-plane.Normal.x * curvesMaxDistance, -plane.Normal.y * curvesMaxDistance, -plane.Normal.z * curvesMaxDistance)
    extrusionSrf1.Transform(xform)
    '#If DEBUG Then
    '        AddDocumentToDebug(extrusionSrf1, "extrusionSrf1")
    '#End If
    Dim points As On3dPointArray = RhGeometry.IntersecaCurvaConSuperfice(extendCrv2, extrusionSrf1)
    If points Is Nothing OrElse points.Count <> 1 Then
      '#If DEBUG Then
      '            MsgBox("Impossibile trovare il punto di intersezione tra le curve, aumentare <extensionFactor>")
      '#End If
      Return Nothing
    End If
    'Vertice di intersezione delle curve da raccordare
    Dim intersectionCurves As New On3dPoint(points.Item(0))
    '#If DEBUG Then
    '        AddDocumentToDebug(intersectionCurves, "vertexToFillet")
    '#End If
    'Trovo i vettori che rappresentano le curve
    Dim vectorCrv1, vectorCrv2 As On3dVector
    If lineCurve1.PointAtStart.DistanceTo(intersectionCurves) > lineCurve1.PointAtEnd.DistanceTo(intersectionCurves) Then
      vectorCrv1 = New On3dVector(lineCurve1.PointAtStart - lineCurve1.PointAtEnd)
    Else
      vectorCrv1 = New On3dVector(lineCurve1.PointAtEnd - lineCurve1.PointAtStart)
    End If
    If lineCurve2.PointAtStart.DistanceTo(intersectionCurves) > lineCurve2.PointAtEnd.DistanceTo(intersectionCurves) Then
      vectorCrv2 = New On3dVector(lineCurve2.PointAtStart - lineCurve2.PointAtEnd)
    Else
      vectorCrv2 = New On3dVector(lineCurve2.PointAtEnd - lineCurve2.PointAtStart)
    End If
    If CBool(vectorCrv1.IsParallelTo(vectorCrv2)) Then Return Nothing
    ''Faccio prodotto scalare tra i vettori delle curve, divido per le lunghezze, faccio arcoseno e divido per 2 per trovare l'angolo
    ''A.B = |A|*|B|*cos(Ø)
    Dim lenghtL1, lenghtL2 As Double
    lineCurve1.GetLength(lenghtL1)
    lineCurve2.GetLength(lenghtL2)
    Dim angleL1L2 As Double = Math.Acos(OnUtil.ON_DotProduct(vectorCrv1, vectorCrv2) / (lenghtL1 * lenghtL2))
    If Not angleL1L2 < Math.PI Then
      MsgBox(IdLanguageManager.GetInstance.Message(238), MsgBoxStyle.Exclamation, My.Application.Info.Title)
      Return Nothing
    End If
    'DISTANZA TRA IL VERTICE DA RACCORDARE E IL CENTRO DEL CERCHIO PER IL RACCORDO = IPOTENUSA DEL TRIANGOLO
    Dim intersectionCurves_filletCenter As Double = radius / Math.Sin(angleL1L2 / 2)
    'DISTANZA TRA IL VERTICE DA RACCORDARE E IL PUNTO DI INTERSEZIONE CURVA-RACCORDO
    Dim halfFilletAngle As Double = Math.PI / 2 - angleL1L2 / 2
    Dim curveToTrim As Double = intersectionCurves_filletCenter * Math.Sin(halfFilletAngle)
    'CALCOLO I PUNTI DI INTERSEZIONE CURVA-RACCORDO
    vectorCrv1.Unitize()
    vectorCrv2.Unitize()
    xform.Identity()
    xform.Scale(intersectionCurves, curveToTrim)
    vectorCrv1.Transform(xform)
    vectorCrv2.Transform(xform)
    Dim pointToTrimL1 As New On3dPoint(vectorCrv1.x + intersectionCurves.x, vectorCrv1.y + intersectionCurves.y, vectorCrv1.z + intersectionCurves.z)
    Dim pointToTrimL2 As New On3dPoint(vectorCrv2.x + intersectionCurves.x, vectorCrv2.y + intersectionCurves.y, vectorCrv2.z + intersectionCurves.z)
    '#If DEBUG Then
    '        AddDocumentToDebug(pointToTrimL1, "pointToTrimL1")
    '        AddDocumentToDebug(pointToTrimL2, "pointToTrimL2")
    '#End If
    'Controllo che il raccordo sia fattibile con il raggio specificato
    Dim t As Double
    If Not curve1.GetClosestPoint(pointToTrimL1, t, RhUtil.RhinoApp.ActiveDoc.AbsoluteTolerance) Then
      '#If DEBUG Then
      '            MsgBox("Valore del raccordo non ammissibile")
      '#End If            
      Return Nothing
    End If
    If Not curve2.GetClosestPoint(pointToTrimL2, t, RhUtil.RhinoApp.ActiveDoc.AbsoluteTolerance) Then
      '#If DEBUG Then
      '            MsgBox("Valore del raccordo non ammissibile")
      '#End If
      Return Nothing
    End If
    'Punto medio dei punti di intersezione curve-raccordo
    Dim pointToTrimL1_pointToTrimL2 As New OnLineCurve(pointToTrimL1, pointToTrimL2)
    Dim midPointL1L2 As On3dPoint = pointToTrimL1_pointToTrimL2.PointAt(pointToTrimL1_pointToTrimL2.Domain().Mid())
    '#If DEBUG Then
    '        AddDocumentToDebug(pointToTrimL1_pointToTrimL2, "pointToTrimL1_pointToTrimL2")
    '        AddDocumentToDebug(midPointL1L2, "midPoint")
    '#End If
    'Vettore che rappresenta la bisettrice dell'angolo da raccordare
    Dim bisector As New On3dVector(midPointL1L2 - intersectionCurves)
    bisector.Unitize()
    xform.Identity()
    xform.Scale(intersectionCurves, intersectionCurves_filletCenter)
    bisector.Transform(xform)
    Dim filletCenter As New On3dPoint(bisector.x + intersectionCurves.x, bisector.y + intersectionCurves.y, bisector.z + intersectionCurves.z)
    '#If DEBUG Then
    '        AddDocumentToDebug(filletCenter, "filletCenter")
    '#End If
    'Calcolo il terzo punto per creare l'arco
    Dim intersectionCurves_midPointArc As Double
    If side = eFilletSide.inner Then
      intersectionCurves_midPointArc = intersectionCurves_filletCenter - radius
    Else
      intersectionCurves_midPointArc = intersectionCurves_filletCenter + radius
    End If
    xform.Identity()
    bisector.Unitize()
    xform.Scale(intersectionCurves, intersectionCurves_midPointArc)
    bisector.Transform(xform)
    Dim thirdPoint As New On3dPoint(bisector.x + intersectionCurves.x, bisector.y + intersectionCurves.y, bisector.z + intersectionCurves.z)
    '#If DEBUG Then
    '        AddDocumentToDebug(thirdPoint, "thirdPointDistance")
    '#End If
    Dim fillet As New OnArc(pointToTrimL1, thirdPoint, pointToTrimL2)
    'Dispose
    pointToTrimL1.Dispose()
    pointToTrimL2.Dispose()
    pointToTrimL1_pointToTrimL2.Dispose()
    midPointL1L2.Dispose()
    bisector.Dispose()
    filletCenter.Dispose()
    thirdPoint.Dispose()
    extendCrv1.Dispose()
    extendCrv2.Dispose()
    extrusionSrf1.Dispose()
    xform.Dispose()
    points.Dispose()
    intersectionCurves.Dispose()
    'Risultato
    Dim result As New OnNurbsCurve
    fillet.GetNurbForm(result)
    '#If DEBUG Then
    '        AddDocumentToDebug(result, "result")
    '#End If
    Return result
  End Function

  ''' <summary>
  ''' Esegue iterativamente il comando _Smooth di Rhino sulla superficie superiore del plantare e controlla se ci sono intersezioni
  ''' con le altre superfici, se sì fa rollback
  ''' </summary>
  ''' <param name="side"></param>
  ''' <param name="smoothIteration">Numero di volte di eseguzione dello script _Smooth</param>
  ''' <param name="smoothFactor">Parametro passato allo script _Smooth</param>
  ''' <param name="chekcAlsoLateraleSrf">Decide se controllare l'eventuale intersezione anche con la superficie laterale(con la inferiore sempre)</param>
  Public Shared Sub SmoothTopSurface(side As IdElement3dManager.eSide, smoothIteration As Integer, smoothFactor As Double,
                                     Optional chekcAlsoLateraleSrf As Boolean = true)
    Try
      '#If DEBUG Then
      '            RhDebug.AddDocumentToDebug(Helper.GetRhinoObjRef(eReferences.insoleTopSurface, side).Surface, "bk pre-smooth")
      '#End If
      RhUtil.RhinoApp.ActiveDoc.BeginWaitCursor()
      RhLayer.RendiCorrenteLayer(GetLayerName(side, IdElement3dManager.eLayerType.insole))
      'Recupero le superfici
      Dim insoleLateralBrep As IOnBrep = Element3dManager.GetRhinoObjRef(IdElement3dManager.eReferences.insoleLateralSurface, side).Brep()
      Dim insoleBottomBrep As IOnBrep = Element3dManager.GetRhinoObjRef(IdElement3dManager.eReferences.insoleBottomSurface, side).Brep()
      If insoleLateralBrep Is Nothing Or insoleBottomBrep Is Nothing Then Exit Sub
      'Inizio interazioni
      Dim nextIteration As Integer = 1
      Dim intersectionIsValid As Boolean = True
      While nextIteration <= smoothIteration And intersectionIsValid
        Dim insoleTopRef = Element3dManager.GetRhinoObjRef(IdElement3dManager.eReferences.insoleTopSurface, side)
        'FACCIO UNA COPIA DI BACKUP DELLA SRF TOP                     
        Dim backupsrfTop As OnBrep = insoleTopRef.Brep.BrepForm()
        'SMOOTH DELLA SUPERFICIE
        insoleTopRef.Object.Select(True, True)
        If RhinoLanguageSetting() = elanguage.English Then
          App.RunScript("-_Smooth _S=" & smoothFactor.ToString.Replace(",", ".") & " _C=World _X=Y _Y=Y _Z=Y _F=Y _Enter", 0)    '_X=N _Y=N 
        Else
          App.RunScript("-_Smooth _F=" & smoothFactor.ToString.Replace(",", ".") & " _S=Assoluto _X=S _Y=S _Z=Y _I=S _Enter", 0) '_X=N _Y=N
        End If
        App.RunScript("_SelNone", 0)
        'CONTROLLO CHE NON CI SIA ALCUNA INTERSEZIONE CON LA SRF INFERIORE
        Dim insoleTopBrep As IOnBrep = insoleTopRef.Brep
        If insoleTopBrep Is Nothing Then Exit Sub
        Dim curves() As OnCurve = {}
        Dim points As New On3dPointArray
        RhUtil.RhinoIntersectBreps(insoleTopBrep, insoleBottomBrep, Doc.AbsoluteTolerance, curves, points)
        If curves.Length > 0 Or points.Count > 0 Then intersectionIsValid = False
        If intersectionIsValid And chekcAlsoLateraleSrf Then
          'INTERSEZIONE CON LA SRF LATERALE(SE PUNTO CALCOLO DISTANZA, SE CURVA DISTANZA DA N PUNTI DEL DOMINIO)       
          RhUtil.RhinoIntersectBreps(insoleTopBrep, insoleLateralBrep, Doc.AbsoluteTolerance, curves, points)
          Dim insoleTopBorder As IOnCurve = RhGeometry.ExtractSurfaceBorder(insoleTopRef)
          intersectionIsValid = ValidIntersection(points, curves, insoleTopBorder)
        End If
        'SE TROVO INTERSEZIONI NON VALIDE BUTTO VIA LA SRF TOP E LA SOSTITUISCO CON QUELLA DI BACKUP
        If Not intersectionIsValid Then
          '#If DEBUG Then
          '                    RhDebug.AddDocumentToDebug(insoleTopRef.Brep, "superficie intersecante")
          '                    MsgBox("Trovata intersezione nel processo di Smooth della superficie superiore del plantare(vedi layer DEBUG)", MsgBoxStyle.Information)
          '#End If
          Doc.DeleteObject(insoleTopRef)
          Element3dManager.SetRhinoObj(IdElement3dManager.eReferences.insoleTopSurface, side, Doc.AddBrepObject(backupsrfTop).Attributes.m_uuid)
          App.RunScript("_SelNone", 0)
        End If
        nextIteration += 1
        'Dispose
        points.Dispose()
        backupsrfTop.Dispose()
        insoleTopRef.Dispose()
      End While
    Catch ex As Exception
      MsgBox(LanguageManager.Message(163), MsgBoxStyle.Exclamation)
      IdLanguageManager.PromptError(ex.Message)
    Finally
      RhUtil.RhinoApp.ActiveDoc.EndWaitCursor()
    End Try
  End Sub


  ''' <summary>
  ''' Verifica se le intersezioni con la superficie superiore del plantare corrispondono alla curva di bordo della stessa
  ''' </summary>
  ''' <param name="points">Eventuali punti di intersezione</param>
  ''' <param name="curves">Eventuali curve di intersezione</param>
  ''' <param name="borderCurve">Curva di bordo della superficie superiore del plantare</param>
  ''' <returns></returns>
  Private Shared Function ValidIntersection(ByRef points As On3dPointArray, ByRef curves() As OnCurve, ByRef borderCurve As IOnCurve) As Boolean
'#If DEBUG Then
'    RhDebug.AddDocumentToDebug(borderCurve, "borderCurve")
'#End If
    ''VALORE IMPOSTATO EMPIRICAMENTE DOPO DIVERSE PROVE
    Dim maxDistance As Double = 0.2
    'Per i punti controllo la distanza minima dalla curva di bordo
    For Each point As On3dPoint In points
      Dim domain As Double
      borderCurve.GetClosestPoint(point, domain)
      Dim testPoint As On3dPoint = borderCurve.PointAt(domain)
      If point.DistanceTo(testPoint) > maxDistance Then
        '#If DEBUG Then
        '                RhDebug.AddDocumentToDebug(point, "point")
        '                RhDebug.AddDocumentToDebug(testPoint, "testPoint")
        '#End If
        Return False
      End If
      testPoint.Dispose()
    Next
    'Per le curve divido il dominio della curva in N, prendo il puunto e calcolo distanza con la curva di bordo
    For Each curve As OnCurve In curves
      Dim interval As Integer = 4
      Dim minDomain As Double = curve.Domain.Min
      Dim maxDomain As Double = curve.Domain.Max
      Dim gap As Double = (maxDomain - minDomain) / interval
      For i As Integer = 0 To interval
        Dim domain As Double = minDomain + gap * i
        If i = interval Then domain = maxDomain
        Dim point As On3dPoint = curve.PointAt(domain)
        borderCurve.GetClosestPoint(point, domain)
        Dim testPoint As On3dPoint = borderCurve.PointAt(domain)
        If point.DistanceTo(testPoint) > maxDistance Then
'#If DEBUG Then
'          RhDebug.AddDocumentToDebug(curve, "intersection curve" & i)
'          RhDebug.AddDocumentToDebug(point, "point")
'          RhDebug.AddDocumentToDebug(testPoint, "testPoint")
'#End If
          Return False
        End If
        point.Dispose()
        testPoint.Dispose()
      Next
    Next
    Return True
  End Function



  ''' <summary>
  ''' Esegue offset di un array di curve e ritorna la curva congiungente
  ''' </summary>
  ''' <param name="curves"></param>
  ''' <param name="offsetDistance"></param>
  ''' <returns></returns>
  Public Shared Function JoinAndOffsetCurves(ByVal curves() As OnCurve, ByVal offsetDistance As Double, ByVal rebuildTolerance As Double,
                                             Optional ByVal joinTolerance As Double = 1) As OnCurve
    If curves Is Nothing Then Return Nothing
    'JOIN
    'Dim joinedCurves() As OnCurve = CongiungiCurve(curves, joinTolerance)
    Dim joinedCurves() As OnCurve = {}
    If Not RhUtil.RhinoMergeCurves(curves, joinedCurves) Then
      MsgBox(LanguageManager.Message(238), MsgBoxStyle.Exclamation, My.Application.Info.Title)
      Return Nothing
    End If
    '#If DEBUG Then
    '        'AddDocumentToDebug(joinedCurves, "joinedCurves")
    '        If joinedCurves Is Nothing Then
    '            MsgBox("DEBUG: L'unione delle curve di profilo prima dell'offset è fallita")
    '        Else
    '            If joinedCurves.Length <> 1 Then
    '                MsgBox("DEBUG: L'unione delle curve di profilo prima dell'offset ha prodotto il seguente numero di elementi: " & joinedCurves.Length)
    '            End If
    '        End If
    '#End If
    If joinedCurves Is Nothing OrElse joinedCurves.Length <> 1 Then
      MsgBox(LanguageManager.Message(239), MsgBoxStyle.Exclamation, My.Application.Info.Title)
      Return Nothing
    End If
    'REBUILD
    Dim preOffsetCurve As IOnCurve = Nothing
    Dim preRebuildCurve As MRhinoCurveObject = RhUtil.RhinoApp.ActiveDoc.AddCurveObject(joinedCurves(0))
    preRebuildCurve.Select(True)
    Dim toleranceString As String = rebuildTolerance.ToString().Replace(",", ".")
    Dim rebuildCommand As String = "_RebuildCrvNonUniform _RequestedTolerance=" & toleranceString & " _MaxPointCount=30 _Quarters=_No _DeleteInput=_Yes _Enter"
    RhUtil.RhinoApp().RunScript(rebuildCommand, 0)
    Dim getObj As New MRhinoGetObject
    getObj.SetGeometryFilter(IRhinoGetObject.GEOMETRY_TYPE_FILTER.curve_object)
    If getObj.GetObjects(0, 1) = IRhinoGet.result.object Then
      'Controllo che l'oggetto selezionato non sia l'altra curva, accade se l'utente ha interrotto il disegno della seconda curva
      Dim selectedObjRef As MRhinoObjRef = getObj.Object(0)
      preOffsetCurve = selectedObjRef.Curve.DuplicateCurve()
      RhUtil.RhinoApp.ActiveDoc.DeleteObject(selectedObjRef)
      selectedObjRef.Dispose()
    End If
    If preOffsetCurve Is Nothing Then
      MsgBox(LanguageManager.Message(240))
      Return Nothing
    End If
    'OFFSET CURVA
    If Not preOffsetCurve.IsClosed Then preOffsetCurve = RhGeometry.ChiudiCurva(preOffsetCurve)
    'Dim resultCurve As OnCurve = RhinoOffsetCurve(preOffsetCurve, offsetDistance)
    Dim resultCurve As OnCurve = RhGeometry.ManualOffsetCurve(preOffsetCurve, offsetDistance)
    If resultCurve Is Nothing Then Return Nothing
    'Garantisco che il risultato sia una curva chiusa
    If resultCurve.IsClosed Then
      Return resultCurve
    Else
      Return RhGeometry.ChiudiCurva(resultCurve)
    End If
  End Function



  ''' <summary>
  ''' INSTABILE - Restituisce una curva di offset rispetto a quella originale fatto in direzione Y negativa e // in XZ
  ''' </summary>
  ''' <param name="originalCurve"></param>
  ''' <param name="offsetDistance"></param>
  ''' <returns></returns>
  ''' <remarks>Nonostante i vari sforzi ad oggi 20/11/2015 a volte la funzione crea problemi forse dipende dal piano di costruzione ma NON verificato</remarks>
  Public Shared Function RhinoOffsetCurve(ByRef originalCurve As IOnCurve, ByVal offsetDistance As Double) As OnCurve
    If originalCurve Is Nothing Then Return Nothing
    'Curva chiusa in precedenza
    'If Not originalCurve.IsClosed() Then originalCurve = ChiudiCurva(originalCurve)
    Dim originalCurveObj As MRhinoObject = RhUtil.RhinoApp.ActiveDoc.AddCurveObject(originalCurve)
    Dim originalObjRef As New MRhinoObjRef(originalCurveObj.Attributes.m_uuid)
    'Trovo punto con X minima
    Dim minX = New On3dPoint
    Dim maxX As New On3dPoint
    Dim minY As New On3dPoint
    Dim maxY As New On3dPoint
    RhGeometry.CurveFindExtremePoints(originalCurve, minX, maxX, minY, maxY)
    'Calcolo punto target per l'offset considerando che la normale del punto è anti // all'asse X
    Dim offsetPoint As New On3dPoint(minY.x, (minY.y - offsetDistance), minY.z)
    Dim pointString As String = offsetPoint.x.ToString().Replace(",", ".") & "," & offsetPoint.y.ToString().Replace(",", ".") & "," & offsetPoint.z.ToString().Replace(",", ".")
    RhUtil.RhinoApp().RunScript("_SelNone", 0)
    originalCurveObj.Select(True, True)
    If RhinoLanguageSetting() = elanguage.English Then
      RhUtil.RhinoApp().RunScript("-_Offset Tolerance 1 ThroughPoint " & pointString & " _Enter", 0)
    ElseIf RhinoLanguageSetting() = elanguage.Italian Then
      RhUtil.RhinoApp().RunScript("-_Offset Tolleranza 1 AttraversoPunto " & pointString & " _Enter", 0)
    End If
    RhUtil.RhinoApp().RunScript("_SelNone", 0)
    RhUtil.RhinoApp().RunScript("_SelLast", 0)
    'originalCurveObj.Select(False, True)
    Dim offsetSplittedCurves As New List(Of OnCurve)
    'Salvo le curve rsultanti dall'offset
    Dim getObj As New MRhinoGetObject
    getObj.SetGeometryFilter(IRhinoGetObject.GEOMETRY_TYPE_FILTER.curve_object)
    If getObj.GetObjects(0, Integer.MaxValue) = IRhinoGet.result.object Then
      For i As Int32 = 0 To getObj.ObjectCount() - 1
        Dim curveObjRef As MRhinoObjRef = getObj.Object(i)
        If (curveObjRef.Object.IsSelected() = 1 Or curveObjRef.Object.IsSelected() = 2) And (curveObjRef.m_uuid <> originalObjRef.m_uuid) Then
          offsetSplittedCurves.Add(curveObjRef.Curve.DuplicateCurve())
          RhUtil.RhinoApp.ActiveDoc.DeleteObject(curveObjRef)
        End If
        curveObjRef.Dispose()
      Next
    End If
    If offsetSplittedCurves.Count = 0 Then Return Nothing
    Dim finalCurves() As OnCurve = {}
    If Not RhUtil.RhinoMergeCurves(offsetSplittedCurves.ToArray(), finalCurves) Then Return Nothing
    '#If DEBUG Then
    '        AddDocumentToDebug(minY, "Origin point")
    '        AddDocumentToDebug(offsetPoint, "Target point")
    '        AddDocumentToDebug(originalCurve, "Preoffset curve")
    '        AddDocumentToDebug(offsetSplittedCurves, "PostOffset curve")
    '        AddDocumentToDebug(finalCurves, "Merge offset curve")
    '#End If        
    RhUtil.RhinoApp.ActiveDoc.DeleteObject(originalObjRef)
    If finalCurves.Length = 0 Then Return Nothing
    'Elimino curve infinitesime rilevate in DEBUG - prendo la più lunga
    Dim result As OnCurve = Nothing
    Dim maxLenght As Double = Double.MinValue
    For Each curve As OnCurve In finalCurves
      Dim lenght As Double
      curve.GetLength(lenght)
      If lenght > maxLenght Then
        result = curve
        maxLenght = lenght
      End If
    Next
    '#If DEBUG Then
    '        AddDocumentToDebug(result, "Offset longest curve")
    '#End If
    originalObjRef.Dispose()
    offsetPoint.Dispose()
    minX.Dispose()
    maxX.Dispose()
    minY.Dispose()
    maxY.Dispose()
    Return result
  End Function



  Public Shared Sub SplitFootCurves(ByVal curveToTrim As OnCurve, ByVal trimmingSurface As OnSurface)
    Dim curveToTrimObject As New MRhinoGetObject
    Dim intersectionPoints As On3dPointArray = Nothing
    RhUtil.RhinoApp.RunScript("_SelNone _Enter", 0)
    Dim curves As OnCurveArray = RhGeometry.SplitCurveBySurface(curveToTrim, trimmingSurface, intersectionPoints)

    Try
      Dim curvesToRemove As New List(Of OnCurve)
      For i As Integer = 0 To curves.Count - 1
        Dim curve As OnCurve = curves(i)
        Dim minimumBoundingBoxLimit As On3dPoint = curve.BoundingBox.m_min
        If curve.BoundingBox.m_max.z <= curve.BoundingBox.m_min.z Then minimumBoundingBoxLimit = curve.BoundingBox.m_max

        If i = curves.Count - 1 Then
          If Math.Abs(minimumBoundingBoxLimit.z - intersectionPoints(i - 1).z) <= RhUtil.RhinoApp.ActiveDoc.AbsoluteTolerance Then
            curvesToRemove.Add(curve) 'Si aggiunge la curva alla lista curves se non esiste una curva con la stessa lunghezza e dominio
          End If
        Else
          If Math.Abs(minimumBoundingBoxLimit.z - intersectionPoints(i).z) <= RhUtil.RhinoApp.ActiveDoc.AbsoluteTolerance Then
            curvesToRemove.Add(curve) 'Si aggiunge la curva alla lista curves se non esiste una curva con la stessa lunghezza e dominio
          End If
        End If
      Next

      For i As Integer = curves.Count - 1 To 0 Step -1
        Dim removeCurve As Boolean = False
        For j As Integer = 0 To curvesToRemove.Count - 1
          If curvesToRemove(j).InternalPointer = curves(i).InternalPointer Then
            removeCurve = True
            Exit For
          End If
        Next
        If removeCurve Then curves.Remove(i)
      Next

      ''Elenco delle curve trimmate da disegnare
      Dim curvesToDraw As New OnCurveArray

      curvesToDraw = curves

      'Si elimina la curva originale
      RhUtil.RhinoApp.ActiveDoc.DeleteObject(New MRhinoObjRef(curveToTrimObject.Object(0).m_uuid), True, True)

      For Each curve As OnCurve In curvesToDraw
        RhUtil.RhinoApp.ActiveDoc.AddCurveObject(curve)
      Next
    Catch ex As Exception
      MsgBox(LanguageManager.Message(234), MsgBoxStyle.Exclamation, My.Application.Info.Title)
      IdLanguageManager.PromptError(ex.Message)
    End Try
  End Sub



  Public Enum eDirectionCheck
    bottom                  'rivolta verso il basso
    lateral360              'indica una superficie richiusa su se stessa
    top                     'rivolta verso l'alto
    positiveX               'rivolta verso X positiva
    negativeX               'rivolta verso X negativa
    positiveY               'rivolta verso Y positiva
    negativeY               'rivolta verso Y negativa 
    cutoutBlendSrfNegY      'tagliata e punto massimo in X rivolta verso Y negativa
    cutoutBlendSrfPosY      'tagliata e punto massimo in X rivolta verso Y positiva
  End Enum

  ''' <summary>
  ''' CONTROLLO SE LA SUPERFICIE E' RIVOLTA VERSO Z NEGATIVO E IN CASO LA GIRO      
  ''' </summary>
  ''' <param name="objRef"></param>
  ''' <param name="direction">VEDI COMMENTI SU DEFINIZIONE ENUM</param>
  Public Shared Function CheckSurfaceDirection(ByRef objRef As MRhinoObjRef, ByVal direction As eDirectionCheck) As Boolean
    If objRef Is Nothing Then
      Throw New Exception(LanguageManager.Message(237))
      Return False
    End If
    Dim objSurface As IOnSurface = objRef.Brep.m_S.Item(0)
    If objSurface Is Nothing Then
      Throw New Exception(LanguageManager.Message(241))
      Return False
    End If
    Dim brepFace As IOnBrepFace = objRef.Brep.m_F(0)
    If brepFace Is Nothing Then
      Throw New Exception(LanguageManager.Message(242))
      Return False
    End If
    Dim domainX As OnInterval = brepFace.Domain(0) 'objSurface.Domain(0)
    Dim domainY As OnInterval = brepFace.Domain(1) 'objSurface.Domain(1)
    Dim normal As On3dVector = Nothing
    Dim domain1, domain2 As Double
    Dim needFlip As Boolean = False
    Select Case direction
      Case eDirectionCheck.bottom
        domain1 = domainX.Mid
        domain2 = domainY.Mid
        normal = brepFace.NormalAt(domain1, domain2)
        If (normal.z > 0 And Not brepFace.m_bRev) Or (normal.z < 0 And brepFace.m_bRev) Then needFlip = True
      Case eDirectionCheck.lateral360
        'Attenzione in questo caso non uso un punto medio del dominio ma il minimo che sta nel tallone
        domain1 = domainX.Min
        domain2 = domainY.Min
        normal = brepFace.NormalAt(domain1, domain2)
        If (normal.x > 0 And Not brepFace.m_bRev) Or (normal.x < 0 And brepFace.m_bRev) Then needFlip = True
      Case eDirectionCheck.top
        domain1 = domainX.Mid
        domain2 = domainY.Mid
        normal = brepFace.NormalAt(domain1, domain2)
        If (normal.z < 0 And Not brepFace.m_bRev) Or (normal.z > 0 And brepFace.m_bRev) Then needFlip = True
      Case eDirectionCheck.positiveX
        domain1 = domainX.Mid
        domain2 = domainY.Mid
        normal = brepFace.NormalAt(domain1, domain2)
        If (normal.x < 0 And Not brepFace.m_bRev) Or (normal.x > 0 And brepFace.m_bRev) Then needFlip = True
      Case eDirectionCheck.negativeX
        domain1 = domainX.Mid
        domain2 = domainY.Mid
        normal = brepFace.NormalAt(domain1, domain2)
        If (normal.x > 0 And Not brepFace.m_bRev) Or (normal.x < 0 And brepFace.m_bRev) Then needFlip = True
      Case eDirectionCheck.positiveY
        domain1 = domainX.Mid
        domain2 = domainY.Mid
        normal = brepFace.NormalAt(domain1, domain2)
        If (normal.y < 0 And Not brepFace.m_bRev) Or (normal.y > 0 And brepFace.m_bRev) Then needFlip = True
      Case eDirectionCheck.negativeY
        domain1 = domainX.Mid
        domain2 = domainY.Mid
        normal = brepFace.NormalAt(domain1, domain2)
        If (normal.y > 0 And Not brepFace.m_bRev) Or (normal.y < 0 And brepFace.m_bRev) Then needFlip = True
      Case eDirectionCheck.cutoutBlendSrfNegY, eDirectionCheck.cutoutBlendSrfPosY
        Dim bbox As OnBoundingBox = CalculatingTightBoundingBoxeSplitBrep.CalculateBbox(brepFace.Brep)
        brepFace.GetClosestPoint(bbox.m_max, domain1, domain2)
        bbox.Dispose()
        normal = brepFace.NormalAt(domain1, domain2)
        If direction = eDirectionCheck.cutoutBlendSrfPosY Then
          If (normal.y < 0 And Not brepFace.m_bRev) Or (normal.y > 0 And brepFace.m_bRev) Then needFlip = True
        Else
          If (normal.y > 0 And Not brepFace.m_bRev) Or (normal.y < 0 And brepFace.m_bRev) Then needFlip = True
        End If
    End Select
    '#If DEBUG Then
    '        AddDocumentToDebug(brepFace.PointAt(domain1, domain2), "testFlipPoint")
    '        AddDocumentToDebug(brepFace.Brep, "preflip")
    '#End If
    If needFlip Then
      '#If DEBUG Then            
      '            MsgBox("fatto flip " & direction.ToString)
      '#End If
      RhUtil.RhinoApp().RunScript("_SelNone", 0)
      objRef.Object.Select(True, True)
      RhUtil.RhinoApp().RunScript("_Flip", 0)
    End If
    RhUtil.RhinoApp().RunScript("_SelNone", 0)
    'Dispose
    domainX.Dispose()
    domainY.Dispose()
    normal.Dispose()
    Return True
  End Function



End Class
