Imports RMA.OpenNURBS
Imports RhinoUtilsino.Geometry
Imports RMA.Rhino
Imports InsoleDesigner.bll.IdElement3dManager
Imports InsoleDesigner.bll.IdLanguageManager
Imports InsoleDesigner.bll.IdAlias
Imports InsoleDesigner.bll.FactoryPianiInclinati
Imports RhinoUtils


Public MustInherit Class AbstractPianiInclinati
    Implements IOnSerializable
    Implements ICloneable


#Region " Field "

    Protected Const PLANE_DISTANCE As Double = 7
    Public Property Type As FactoryPianiInclinati.eTipoPianoInclinato
    Public Property Position As FactoryPianiInclinati.ePosizionePianiInclinato
    Public Property Spessore As Double

#End Region


#Region " Constructor "

    Protected Sub New(type As eTipoPianoInclinato, position As ePosizionePianiInclinato, spessore As Double)
        Me.Type = type
        Me.Position = position
        Me.Spessore = spessore
    End Sub

#End Region


#Region " PROPERTY "

    Protected Property IdPointP1() As Guid
    Protected Property IdPointP2() As Guid
    Protected Property IdPointCV() As Guid
    Protected Property IdPointA() As Guid
    Protected Property IdPointB() As Guid
    Protected Property IdPointC() As Guid
    Protected Property IdPointD() As Guid
    Protected Property IdPointE() As Guid
    Protected Property IdPointCV1() As Guid
    Protected Property IdPointA1() As Guid
    Protected Property IdPointB1() As Guid
    Protected Property IdPointC1() As Guid
    Protected Property IdPointD1() As Guid
    Protected Property IdPointE1() As Guid

    Protected Property IdInsoleBottomAxisCrv() As Guid
    
    Protected Property IdBordoInternoCrv() As Guid
    Protected Property IdBordoEsternoSuperioreCrv() As Guid
    Protected Property IdBordoEsternoInferioreCrv() As Guid

    Protected Property IdTrimmerBordoEsternoSrf() As Guid
    Protected Property IdSrfLateraleFinale() As Guid
    Protected Property IdSrfInferioreFinale() As Guid
    Protected Property IdSuperficieFinale() As Guid

#End Region


#Region " Funzioni astratte "

    Public MustOverride  Sub PulisciOggetiCostruzione()
    Public MustOverride  Sub DeleteFromDocument()
    Public MustOverride  Function IsInDoc() As Boolean
    Protected MustOverride Function GetIdInizioBordo() As Guid
    Protected MustOverride Function GetIdFineBordo() As Guid

#End Region


#Region " Passi procedura comuni "


    ''' <summary>
    ''' UNIONE FINALE - VEDI Procedura 21
    ''' </summary>
    Public Sub JoinFinale()               
        Dim srfLateraleFinaleObjRef = New MRhinoObjRef(IdSrfLateraleFinale)
        Dim srfInferioreFinaleObjRef = New MRhinoObjRef(IdSrfInferioreFinale)
        srfInferioreFinaleObjRef.Object().Select(True, True)
        srfLateraleFinaleObjRef.Object().Select(True, True)
        App.RunScript("_Join", 0)
        App.RunScript("_SelLast", 0)
        Dim getObjects As New MRhinoGetObject
        getObjects.SetGeometryFilter(IRhinoGetObject.GEOMETRY_TYPE_FILTER.surface_object Or IRhinoGetObject.GEOMETRY_TYPE_FILTER.polysrf_object)
        getObjects.GetObjects(0, Integer.MaxValue)
        If getObjects.ObjectCount <> 1 Then Throw New Exception("Impossibile unire le superfici finali del piano inclinato")
        IdSuperficieFinale = getObjects.Object(0).m_uuid                 
        getObjects.Dispose()
        srfLateraleFinaleObjRef.Dispose()
        srfInferioreFinaleObjRef.Dispose()
        App.RunScript("_SelNone", 0)
    End Sub

    ''' <summary>
    ''' CREAZIONE SEZIONI PER PIANO SUPERIORE - VEDI procedura 15-20
    ''' </summary>    
    Public Sub SetSrfInferioreFinale(side As eSide)
        Dim insoleBottomSrfObjref = Element3dManager.GetRhinoObjRef(eReferences.insoleBottomSurface, side)   
        Dim crvBordoInternoObjRef = New MRhinoObjRef(IdBordoInternoCrv)
        Dim crvBordoEsternoInferioreObjRef = New MRhinoObjRef(IdBordoEsternoInferioreCrv)
        Dim puntoInizioBordo = New MRhinoObjRef(GetIdInizioBordo()).Point().point
        Dim puntoFineBordo = New MRhinoObjRef(GetIdFineBordo()).Point().point
        Dim gapP1_E = puntoFineBordo.x - puntoInizioBordo.x
        Dim insoleBbox = insoleBottomSrfObjref.Object().BoundingBox()
        Dim counter = 1
        Dim delta = counter * PLANE_DISTANCE
        Dim puntiSezioni = New SortedList(Of Double, On3dPointArray)
        Dim points As On3dPointArray
        While delta < gapP1_E - PLANE_DISTANCE / 2
            Dim intersectionCurves() As OnCurve = {}
            Dim intersectionPoints As New On3dPointArray           
            Dim positionX = puntoInizioBordo.x + delta
            Dim planeSurface = GetCutterPlane(insoleBbox, positionX)
            'INTERSEZIONE CON BORDI
            points = New On3dPointArray
            RhUtil.RhinoCurveBrepIntersect(crvBordoInternoObjRef.Curve().DuplicateCurve(), planeSurface.BrepForm(), 0.1, intersectionCurves, intersectionPoints)
            If intersectionPoints.Count() <> 1 Then Throw New Exception("Impossibile trova punti intersezione per binari")
            points.Append(New On3dPoint(intersectionPoints(0)))
            intersectionPoints = New On3dPointArray
            RhUtil.RhinoCurveBrepIntersect(crvBordoEsternoInferioreObjRef.Curve().DuplicateCurve(), planeSurface.BrepForm(), 0.1, intersectionCurves, intersectionPoints)
            If intersectionPoints.Count() <> 1 Then Throw New Exception("Impossibile trova punti intersezione per binari")
            points.Append(New On3dPoint(intersectionPoints(0)))
            puntiSezioni.Add(positionX, points)
            counter += 1
            delta = counter * PLANE_DISTANCE
        End While
        'CREO SEZIONI
        Dim curveSezioni = New SortedList(Of Double, Guid)
        For Each pair As KeyValuePair(Of Double, On3dPointArray) In puntiSezioni
            Dim line As New OnLineCurve(pair.Value.Item(0), pair.Value.Item(1))
            Dim id = Doc.AddCurveObject(line).Attributes().m_uuid
            curveSezioni.Add(pair.Key, id)
        Next
        'SPLIT BINARI
        Dim xPosition = puntoInizioBordo.x + PLANE_DISTANCE / 2
        Dim cutterSrf = GetCutterPlane(insoleBbox, xPosition)
        Dim cutterSrfObj1 = Doc.AddSurfaceObject(cutterSrf)
        App.RunScript("_SelNone", 0)        
        crvBordoInternoObjRef.Object().Select(True, True)
        App.RunScript("-_Split _SelID " & cutterSrfObj1.Attributes.m_uuid.ToString() & " _Enter", 0)
        App.RunScript("_SelLast", 0)
        Dim getObjects As New MRhinoGetObject
        getObjects.SetGeometryFilter(IRhinoGetObject.GEOMETRY_TYPE_FILTER.curve_object)
        getObjects.GetObjects(0, Integer.MaxValue)
        If getObjects.ObjectCount <> 2 Then Throw New Exception("Impossibile eseguire split del bordo plantare estruso")
        Dim splittedSrf As New SortedList(Of Double, MRhinoObjRef)
        For i = 0 To getObjects.ObjectCount - 1
            Dim bbox As New OnBoundingBox
            getObjects.Object(i).Object().GetTightBoundingBox(bbox)
            splittedSrf.Add(bbox.Diagonal().Length(), getObjects.Object(i))
        Next
        curveSezioni.Add(xPosition, splittedSrf.Values.Item(0).m_uuid)
        Dim binario1ObjRef = splittedSrf.Values.Item(1)
        Doc.PurgeObject(cutterSrfObj1)

        xPosition = puntoFineBordo.x - PLANE_DISTANCE / 2
        cutterSrf = GetCutterPlane(insoleBbox, xPosition)
        Dim cutterSrfObj2 = Doc.AddSurfaceObject(cutterSrf)        
        App.RunScript("_SelNone", 0)
        crvBordoEsternoInferioreObjRef.Object().Select(True, True)
        App.RunScript("-_Split _SelID " & cutterSrfObj2.Attributes.m_uuid.ToString() & " _Enter", 0)
        App.RunScript("_SelLast", 0)
        getObjects.ClearObjects()
        getObjects.GetObjects(0, Integer.MaxValue)
        If getObjects.ObjectCount <> 2 Then Throw New Exception("Impossibile eseguire split del bordo plantare estruso")
        splittedSrf = New SortedList(Of Double, MRhinoObjRef)
        For i = 0 To getObjects.ObjectCount - 1
            Dim bbox As New OnBoundingBox
            getObjects.Object(i).Object().GetTightBoundingBox(bbox)
            splittedSrf.Add(bbox.Diagonal().Length(), getObjects.Object(i))
        Next
        curveSezioni.Add(xPosition, splittedSrf.Values.Item(0).m_uuid)
        Dim binario2ObjRef = splittedSrf.Values.Item(1)
        Doc.PurgeObject(cutterSrfObj2)
        'CREO SUPERFICIE
        App.RunScript("_SelNone", 0)
        binario1ObjRef.Object().Select(True, True)
        binario2ObjRef.Object().Select(True, True)
        For Each sezione As Guid In curveSezioni.Values
            Dim sel = New MRhinoObjRef(sezione)
            sel.Object().Select(True, True)
        Next
        Dim sweep2Cmd As String = "-_Sweep2 " 
        If RhinoLanguageSetting() = elanguage.English Then
            sweep2Cmd &= " _Enter Simplify=None MaintainHeight=No _Enter"
        Else
            sweep2Cmd &= " _Enter Semplifica=No MantieniAltezza=No _Enter"
        End If
        App.RunScript(sweep2Cmd, 0)
        App.RunScript("_SelLast", 0)
        getObjects.ClearObjects()
        getObjects.SetGeometryFilter(IRhinoGetObject.GEOMETRY_TYPE_FILTER.surface_object Or IRhinoGetObject.GEOMETRY_TYPE_FILTER.polysrf_object)
        getObjects.GetObjects(0, Integer.MaxValue)
        If getObjects.ObjectCount <> 1 Then Throw New Exception("Impossibile creare la superficie inferiore del piano inclinato")
        IdSrfInferioreFinale =  getObjects.Object(0).m_uuid
        App.RunScript("_SelNone", 0)
        'Pulizia
        Doc.DeleteObject(binario1ObjRef)
        Doc.DeleteObject(binario2ObjRef)
        For Each sezione As Guid In curveSezioni.Values
            Doc.DeleteObject(New MRhinoObjRef(sezione))            
        Next     
        insoleBottomSrfObjref.Dispose()
        binario1ObjRef.Dispose()
        binario2ObjRef.Dispose() 
        getObjects.Dispose()
    End Sub

    ''' <summary>
    ''' ESTRUSIONE MEZZO BORDO PLANTARE - VEDI procedura 13-14
    ''' </summary>    
    Public Sub SetSrfLateraleFinale(side As eSide)                     
        'ESTRUDO ASSE PLANTARE
        Dim insoleBottomAxisObjRef = New MRhinoObjRef(IdInsoleBottomAxisCrv)
        Dim insoleBottomAxis = insoleBottomAxisObjRef.Curve().DuplicateCurve()
        Dim extrusionLenght = insoleBottomAxis.BoundingBox().Diagonal().Length() * Spessore * 4
        RhUtil.RhinoExtendCurve(insoleBottomAxis, IRhinoExtend.Type.Line, 0, extrusionLenght)
        RhUtil.RhinoExtendCurve(insoleBottomAxis, IRhinoExtend.Type.Line, 1, extrusionLenght)
        Dim insoleBottomAxisSrf = RhUtil.RhinoExtrudeCurveStraight(insoleBottomAxis, New On3dVector(0, 0, 1), extrusionLenght)
        RhUtil.RhinoExtendSurface(insoleBottomAxisSrf, IOnSurface.ISO.N_iso, extrusionLenght, True)
        RhUtil.RhinoExtendSurface(insoleBottomAxisSrf, IOnSurface.ISO.S_iso, extrusionLenght, True)
        RhUtil.RhinoExtendSurface(insoleBottomAxisSrf, IOnSurface.ISO.W_iso, extrusionLenght, True)
        RhUtil.RhinoExtendSurface(insoleBottomAxisSrf, IOnSurface.ISO.E_iso, extrusionLenght, True)        
'#If DEBUG
    'RhDebug.AddDocumentToDebug(insoleBottomAxisSrf, "insoleBottomAxisSrf")
'#End If
        'SPLIT BORDO PLANTARE
        Dim insoleBottomSrfObjref = Element3dManager.GetRhinoObjRef(eReferences.insoleBottomSurface, side)    
        Dim insoleBottomBorder As IOnCurve = RhGeometry.ExtractSurfaceBorder(insoleBottomSrfObjref)
        Dim intersectionPoints = RhGeometry.IntersecaCurvaConSuperfice(insoleBottomBorder, insoleBottomAxisSrf)        
        Dim splittedCurves = RhGeometry.SplitCurve(insoleBottomBorder, intersectionPoints)
        If splittedCurves.Count() <> 2 Then Throw New Exception("Impossibile dividere il bordo del plantare")
        Dim borderCrvSplitted As OnCurve = Nothing
        If side = eSide.right And Position = ePosizionePianiInclinato.laterale Or side = eSide.left And Position = ePosizionePianiInclinato.mediale Then
            borderCrvSplitted = splittedCurves.Cast(Of OnCurve)().OrderBy(Function(c) c.BoundingBox().Max().y).First()
        Else 
            borderCrvSplitted = splittedCurves.Cast(Of OnCurve)().OrderBy(Function(c) c.BoundingBox().Max().y).Last()
        End If                
        Dim borderCrvSplittedSrf = RhUtil.RhinoExtrudeCurveStraight(borderCrvSplitted, New On3dVector(0, 0, 1), extrusionLenght)
        Dim xform As New OnXform
        xform.Translation(0, 0, -extrusionLenght / 2)
        borderCrvSplittedSrf.Transform(xform)
        Dim borderCrvSplittedSrfObj = Doc.AddSurfaceObject(borderCrvSplittedSrf)
        'SPLIT BORDO ESTRUSO
        Dim trimmerBordoEsternoSrfObjRef = New MRhinoObjRef(IdTrimmerBordoEsternoSrf)    
'#If DEBUG
'        RhDebug.AddDocumentToDebug(trimmerBordoEsternoSrfObjRef.Brep(), "trimmerBordoEsternoSrfObjRef")
'#End If    
        App.RunScript("_SelNone", 0)
        borderCrvSplittedSrfObj.Select(True, True)
        Dim splitCommand = "-_Split _SelID " & trimmerBordoEsternoSrfObjRef.m_uuid.ToString & " _Enter"
        App.RunScript(splitCommand, 0)
        App.RunScript("_SelLast", 0)
        Dim getObjects As New MRhinoGetObject
        getObjects.SetGeometryFilter(IRhinoGetObject.GEOMETRY_TYPE_FILTER.surface_object Or IRhinoGetObject.GEOMETRY_TYPE_FILTER.polysrf_object)
        getObjects.GetObjects(0, Integer.MaxValue)
        If getObjects.ObjectCount <> 2 Then Throw New Exception("Impossibile eseguire split del bordo plantare estruso")
        App.RunScript("_SelNone", 0)
        Dim splittedSrf As New SortedList(Of Double, MRhinoObjRef)
        For i = 0 To getObjects.ObjectCount - 1
            Dim bbox As New OnBoundingBox
            getObjects.Object(i).Object().GetTightBoundingBox(bbox)
            splittedSrf.Add(bbox.Volume(), getObjects.Object(i))
        Next
        IdSrfLateraleFinale = splittedSrf.Values.Item(0).m_uuid
        Doc.DeleteObject(splittedSrf.Values.Item(1))
        Doc.DeleteObject(trimmerBordoEsternoSrfObjRef)
        Doc.DeleteObject(insoleBottomAxisObjRef)
        'Dispose
        insoleBottomSrfObjref.Dispose()
        getObjects.Dispose()
        insoleBottomAxis.Dispose()
        insoleBottomAxisSrf.Dispose()
        borderCrvSplittedSrf.Dispose()
    End Sub

    ''' <summary>
    ''' BORDO ESTERNO PROIETTATO - VEDI procedura 8-12
    ''' </summary>
    Public Sub SetBordoEsternoProiettato_Tallone_Or_Totale()     
        'CURVE P1_A1 E A1_E1           
        Dim curveToTrimObjRef = New MRhinoObjRef(IdBordoEsternoSuperioreCrv)
        Dim curveToTrim = curveToTrimObjRef.Curve().DuplicateCurve()
        Dim extrusionLenght = curveToTrim.BoundingBox().Diagonal().Length() * 2 ' 10
        Dim pointA1 = New MRhinoObjRef(IdPointA1).Point().point
        Dim zMaxA1 = New On3dPoint(pointA1.x, pointA1.y, pointA1.z + extrusionLenght)
        Dim zMinA1 = New On3dPoint(pointA1.x, pointA1.y, pointA1.z - extrusionLenght)
        Dim lineA1 As New OnLineCurve(zMaxA1, zMinA1)      
        Dim trimmingSurfaceA1 = RhUtil.RhinoExtrudeCurveStraight(lineA1, New On3dVector(0,1,0), extrusionLenght)
        RhUtil.RhinoExtendSurface(trimmingSurfaceA1, IOnSurface.ISO.N_iso, extrusionLenght, True)
        RhUtil.RhinoExtendSurface(trimmingSurfaceA1, IOnSurface.ISO.S_iso, extrusionLenght, True)
        RhUtil.RhinoExtendSurface(trimmingSurfaceA1, IOnSurface.ISO.W_iso, extrusionLenght, True)
        RhUtil.RhinoExtendSurface(trimmingSurfaceA1, IOnSurface.ISO.E_iso, extrusionLenght, True)         
        Dim intersectionPoints As New On3dPointArray      
        Dim curves As OnCurveArray = RhGeometry.SplitCurveBySurface(curveToTrim, trimmingSurfaceA1, intersectionPoints)
        If curves Is Nothing OrElse curves.Count() <> 2 Then
            RhDebug.AddDocumentToDebug(trimmingSurfaceA1, "extrusionSrfA1") 
            Throw New Exception("Impossibile trovare la curva P1-A1")
        End If
        Dim curvaP1_A1 = curves.Cast(Of OnCurve)().OrderBy(Function(c) c.BoundingBox().Min().x).First().DuplicateCurve()
        Dim curvaA1_E1 = curves.Cast(Of OnCurve)().OrderBy(Function(c) c.BoundingBox().Min().x).Last().DuplicateCurve()

        'CURVE A1_D1 E D1_E1
        Dim pointD1 = New MRhinoObjRef(IdPointD1).Point().point
        Dim zMaxD1 = New On3dPoint(pointD1.x, pointD1.y, pointD1.z + extrusionLenght)
        Dim zMinD1 = New On3dPoint(pointD1.x, pointD1.y, pointD1.z - extrusionLenght)
        Dim lineD1 As New OnLineCurve(zMaxD1, zMinD1)           
        Dim trimmingSurfaceD1 = RhUtil.RhinoExtrudeCurveStraight(lineD1, New On3dVector(0,1,0), extrusionLenght)
        RhUtil.RhinoExtendSurface(trimmingSurfaceD1, IOnSurface.ISO.N_iso, extrusionLenght, True)
        RhUtil.RhinoExtendSurface(trimmingSurfaceD1, IOnSurface.ISO.S_iso, extrusionLenght, True)
        RhUtil.RhinoExtendSurface(trimmingSurfaceD1, IOnSurface.ISO.W_iso, extrusionLenght, True)
        RhUtil.RhinoExtendSurface(trimmingSurfaceD1, IOnSurface.ISO.E_iso, extrusionLenght, True)
        curves = RhGeometry.SplitCurveBySurface(curvaA1_E1, trimmingSurfaceD1, intersectionPoints)        
        If curves Is Nothing OrElse curves.Count() <> 2 Then
            Throw New Exception("Impossibile trovare la curva D1-E1")
        End If
        Dim curvaA1_D1 = curves.Cast(Of OnCurve)().OrderBy(Function(c) c.BoundingBox().Min().x).First().DuplicateCurve()
        Dim curvaD1_E1 = curves.Cast(Of OnCurve)().OrderBy(Function(c) c.BoundingBox().Min().x).Last().DuplicateCurve()

        'CURVA P1-A1 PROIETTATA CON 11 PUNTI      
        Dim curvaP1_A1_proiettata = GetCurveBy11Points(curvaP1_A1, True)

        'PUNTI D1-E1 PROIETTATA CON 11 PUNTI
        Dim curvaD1_E1_proiettata = GetCurveBy11Points(curvaD1_E1, False)

        'CURVA A1_D1 TRASLATA
        Dim curvaA1_D1_proiettata = curvaA1_D1.DuplicateCurve()
        Dim xform As New OnXform
        xform.Translation(0, 0, -Me.Spessore)
        curvaA1_D1_proiettata.Transform(xform)  
'#If DEBUG
        'RhDebug.AddDocumentToDebug(curvaP1_A1_proiettata, "curvaP1_A1_proiettata")
        'RhDebug.AddDocumentToDebug(curvaA1_D1_proiettata, "curvaA1_D1_proiettata")
        'RhDebug.AddDocumentToDebug(curvaD1_E1_proiettata, "curvaD1_E1_proiettata")
'#End If      
        Dim inputCurves = {curvaP1_A1_proiettata, curvaA1_D1_proiettata, curvaD1_E1_proiettata}
        Dim joinedCurves() As OnCurve = {}
        If Not RhUtil.RhinoMergeCurves(inputCurves, joinedCurves) OrElse joinedCurves.Length <> 1
            RhDebug.AddDocumentToDebug(inputCurves, "inputCurves")
            RhDebug.AddDocumentToDebug(joinedCurves, "joinedCurves")
            Throw New Exception("Impossibile unire le curve del bordo esterno inferiore")
        End If       
        IdBordoEsternoInferioreCrv = Doc.AddCurveObject(joinedCurves(0)).Attributes().m_uuid

        'ESTRUDO LE 4 CURVE
        Dim bordoEsterno = New MRhinoObjRef(IdBordoEsternoSuperioreCrv).Curve().DuplicateCurve()
        extrusionLenght *= 10
        Dim extrudedCurvaP1_A1_proiettata = RhUtil.RhinoExtrudeCurveStraight(curvaP1_A1_proiettata, New On3dVector(0,1,0), extrusionLenght)
        Dim extrudedCurvaA1_D1_proiettata = RhUtil.RhinoExtrudeCurveStraight(curvaA1_D1_proiettata, New On3dVector(0,1,0), extrusionLenght)
        Dim extrudedCurvaD1_E1_proiettata = RhUtil.RhinoExtrudeCurveStraight(curvaD1_E1_proiettata, New On3dVector(0,1,0), extrusionLenght)
        Dim extrudedCurvaBordoEsterno = RhUtil.RhinoExtrudeCurveStraight(bordoEsterno, New On3dVector(0,1,0), extrusionLenght)
        xform.Identity()
        xform.Translation(0,-extrusionLenght/2,0)
        extrudedCurvaP1_A1_proiettata.Transform(xform)
        extrudedCurvaA1_D1_proiettata.Transform(xform)
        extrudedCurvaD1_E1_proiettata.Transform(xform)
        extrudedCurvaBordoEsterno.Transform(xform)
        Dim P1_A1SrfObj = Doc.AddSurfaceObject(extrudedCurvaP1_A1_proiettata)
        Dim A1_D1SrfObj = Doc.AddSurfaceObject(extrudedCurvaA1_D1_proiettata)
        Dim D1_E1SrfObj = Doc.AddSurfaceObject(extrudedCurvaD1_E1_proiettata)                                        
        Dim extrudedCurvaBordoEsternoObj = Doc.AddSurfaceObject(extrudedCurvaBordoEsterno)

        'UNISCO LE SUPERFICI
        App.RunScript("_SelNone", 0)
        P1_A1SrfObj.Select(True, True)
        A1_D1SrfObj.Select(True, True)
        D1_E1SrfObj.Select(True, True)
        extrudedCurvaBordoEsternoObj.Select(True, True)
        App.RunScript("_Join", 0)
        App.RunScript("_SelLast", 0)
        Dim getObjects As New MRhinoGetObject
        getObjects.SetGeometryFilter(IRhinoGetObject.GEOMETRY_TYPE_FILTER.surface_object Or IRhinoGetObject.GEOMETRY_TYPE_FILTER.polysrf_object)
        getObjects.GetObjects(0, Integer.MaxValue)
        If getObjects.ObjectCount <> 1 Then Throw New Exception("Impossibile unire le superfici di estrusione del bordo proiettato")
        Dim polySrfObj = getObjects.Object(0).Object()
        App.RunScript("_SelNone", 0)

        'AGGIUNGO PIANO PER INTERSEZIONE
        Dim bordoBbox = bordoEsterno.BoundingBox()
        Dim yCoordinate = bordoBbox.Center().y
        Dim lenght = bordoBbox.Diagonal().Length() * 2
        Dim originPoint = New On3dPoint(bordoBbox.Min().x, yCoordinate, bordoBbox.Min().z)
        Dim planePoint1 = New On3dPoint(bordoBbox.Max().x, yCoordinate, bordoBbox.Min().z)
        Dim planePoint2 = New On3dPoint(bordoBbox.Max().x, yCoordinate, bordoBbox.Max().z)
        Dim zDirection = New On3dVector(planePoint1 - originPoint)
        Dim yDirection = New On3dVector(planePoint2 - originPoint)
        Dim plane = New OnPlane(originPoint, yDirection, zDirection)
        Dim planeSurface As New OnPlaneSurface(plane)
        planeSurface.Extend(0, New OnInterval(-lenght, lenght))
        planeSurface.Extend(1, New OnInterval(-lenght, lenght))
        planeSurface.Extend(2, New OnInterval(-lenght, lenght))
        planeSurface.Extend(3, New OnInterval(-lenght, lenght))
        Dim intersectionSrfObj = Doc.AddSurfaceObject(planeSurface)

        'INTERSEZIONE
        App.RunScript("_SelNone", 0)
        polySrfObj.Select(True, True)
        intersectionSrfObj.Select(True, True)
        App.RunScript("_Intersect", 0)
        App.RunScript("_SelLast", 0)
        getObjects.ClearObjects()
        getObjects.SetGeometryFilter(IRhinoGetObject.GEOMETRY_TYPE_FILTER.curve_object)
        getObjects.GetObjects(0, Integer.MaxValue)
        If getObjects.ObjectCount <> 1 Then
            Throw New Exception("Impossibile intersecare la polysuperficie di estrusione del bordo proiettato con il piano X=0")
        End If
        Dim crvForCutDown = getObjects.Object(0).Curve().DuplicateCurve()

        Dim srfInferioreTaglioBordoEsterno = RhUtil.RhinoExtrudeCurveStraight(crvForCutDown, New On3dVector(0, 1, 0), extrusionLenght)
        xform.Identity()
        xform.Translation(0, -extrusionLenght / 2, 0)
        srfInferioreTaglioBordoEsterno.Transform(xform)
        IdTrimmerBordoEsternoSrf = Doc.AddSurfaceObject(srfInferioreTaglioBordoEsterno).Attributes().m_uuid

        'PULIZIA DOC
        Doc.DeleteObject(getObjects.Object(0))
        Doc.DeleteObject(curveToTrimObjRef)
        Doc.DeleteObject(New MRhinoObjRef(polySrfObj.Attributes().m_uuid))
        Doc.DeleteObject(New MRhinoObjRef(intersectionSrfObj.Attributes().m_uuid))
        App.RunScript("_SelNone", 0)

        'Dispose
        intersectionPoints.Dispose()
        getObjects.Dispose()
        intersectionSrfObj.Dispose()
        P1_A1SrfObj.Dispose()
        A1_D1SrfObj.Dispose()
        D1_E1SrfObj.Dispose()
        extrudedCurvaBordoEsternoObj.Dispose()
        bordoBbox.Dispose()
        plane.Dispose()
        planeSurface.Dispose()
        curveToTrimObjRef.Dispose()
        curveToTrim.Dispose()
    End Sub

    ''' <summary>
    ''' BORDO ESTERNO PIANO INCLINATO - VEDI procedura 6
    ''' </summary>   
    Public Sub SetBordoEsterno_Tallone_Or_Totale(side As eSide)
        Dim insoleBottomSrfObjref = Element3dManager.GetRhinoObjRef(eReferences.insoleBottomSurface, side)       
        Dim insoleBottomBorder As IOnCurve = RhGeometry.ExtractSurfaceBorder(insoleBottomSrfObjref)
        Dim pointE = New MRhinoObjRef(IdPointE).Point().point
        Dim extrusionLenght = insoleBottomBorder.BoundingBox().Diagonal().Length()
        Dim lineE As New OnLineCurve(New On3dPoint(pointE.x, pointE.y, pointE.z + extrusionLenght), New On3dPoint(pointE.x, pointE.y, pointE.z - extrusionLenght))
        'SRF CHE TAGLIA LA PARTE POSTERIORE
        Dim vectorAxis = GetInsoleBottomAxisVector()
        vectorAxis.Unitize()
        vectorAxis.Reverse()
        Dim extrusionSrf1 = RhUtil.RhinoExtrudeCurveStraight(lineE, New On3dVector(vectorAxis.x, vectorAxis.y, 0), extrusionLenght)
        Dim extrusionSrfObj1 = Doc.AddSurfaceObject(extrusionSrf1)        
        'SRF CHE TAGLIA LA PARTE LATERALE
        vectorAxis.Reverse()
        Dim xform As New OnXform
        Dim rotationAngle = Math.PI/2
        If side = eSide.right And Position = ePosizionePianiInclinato.laterale Or side = eSide.left And Position = ePosizionePianiInclinato.mediale Then rotationAngle = -rotationAngle        
        xform.Rotation(rotationAngle, OnUtil.On_zaxis, pointE)
        vectorAxis.Transform(xform)
        Dim extrusionSrf2 = RhUtil.RhinoExtrudeCurveStraight(lineE, New On3dVector(vectorAxis.x, vectorAxis.y, 0), extrusionLenght)
        Dim extrusionSrfObj2 = Doc.AddSurfaceObject(extrusionSrf2)
        Dim curvaDiBordo = Doc.AddCurveObject(insoleBottomBorder)
        Dim objToTrimId As String = curvaDiBordo.Attributes().m_uuid.ToString()
        Dim trimmerId1 As String = extrusionSrfObj1.Attributes.m_uuid.ToString()
        Dim trimmerId2 As String = extrusionSrfObj2.Attributes.m_uuid.ToString()
        App.RunScript("_SelNone", 0)
        Dim splitcommand As String = "-_Split _SelID " & objToTrimId & " _Enter _SelId " & trimmerId1 & " _SelId " & trimmerId2 & " _Enter"
        App.RunScript(splitcommand, 0)
        'LEGGO LE CURVE RISULTANTI
        App.RunScript("_SelLast", 0)
        Dim borderCurveSplittedObjRef As New List(Of MRhinoObjRef)
        Dim getObjects As New MRhinoGetObject
        getObjects.SetGeometryFilter(IRhinoGetObject.GEOMETRY_TYPE_FILTER.curve_object)
        If getObjects.GetObjects(0, Integer.MaxValue) = IRhinoGet.result.object Then
            If getObjects.ObjectCount <> 2 Then Throw New Exception("Numero di curve risultate dallo split errato")
            For i As Int32 = 0 To getObjects.ObjectCount() - 1
                borderCurveSplittedObjRef.Add(getObjects.Object(i))
            Next
        End If
        Dim res = borderCurveSplittedObjRef.OrderBy(Function(x) x.Object().BoundingBox().Diagonal()).First()
        IdBordoEsternoSuperioreCrv = res.m_uuid
'#If DEBUG
'        RhDebug.AddDocumentToDebug(res.Curve().DuplicateCurve(), "BordoEsternoSuperioreCrv")
'#End If
        'Pulisco il doc
        Doc.DeleteObject(borderCurveSplittedObjRef.OrderBy(Function(x) x.Object().BoundingBox().Diagonal()).Last(), True)
        Doc.DeleteObject(New MRhinoObjRef(extrusionSrfObj1.Attributes.m_uuid), True)
        Doc.DeleteObject(New MRhinoObjRef(extrusionSrfObj2.Attributes.m_uuid), True)
        'Dispose
        insoleBottomSrfObjref.Dispose()
        lineE.Dispose()
        extrusionSrf1.Dispose()
        extrusionSrf2.Dispose()
        getObjects.Dispose()
        extrusionSrfObj1.Dispose()
        extrusionSrfObj2.Dispose()
        App.RunScript("_SelNone", 0)
    End Sub

    ''' <summary>
    ''' PUNTI P1, P2 E ASSE DEL PIEDE - VEDI procedura 1-3
    ''' </summary>
    Public Sub Set_P1_P2_Asse(side As eSide)
        Dim insoleBottomSrfObjref = Element3dManager.GetRhinoObjRef(eReferences.insoleBottomSurface, side)    
        Dim insoleBottomBorder As IOnCurve = RhGeometry.ExtractSurfaceBorder(insoleBottomSrfObjref)
        Dim borderMinX = New On3dPoint
        Dim borderMaxX As New On3dPoint
        Dim borderMinY As New On3dPoint
        Dim borderMaxY As New On3dPoint
        RhGeometry.CurveFindExtremePoints(insoleBottomBorder, borderMinX, borderMaxX, borderMinY, borderMaxY)
        Dim pointP1 = New On3dPoint(borderMinX)
        Dim pointP1ObjRef As New MRhinoObjRef(Doc.AddPointObject(pointP1).Attributes.m_uuid)
        IdPointP1 = pointP1ObjRef.m_uuid
        Dim extrusionLenght = insoleBottomBorder.BoundingBox().Diagonal().Length()
        Dim insoleAxisXExtrusionCrv As New OnLineCurve(pointP1, New On3dPoint(pointP1.x + extrusionLenght, pointP1.y, pointP1.z))
        Dim extrusionResult = True
        'Per sicurezza estendo in tutte le direzioni       
        Dim insoleAxisXExtrusionSrf = RhUtil.RhinoExtrudeCurveStraight(insoleAxisXExtrusionCrv, New On3dVector(0, 0, 1), extrusionLenght)
        extrusionResult = extrusionResult And RhUtil.RhinoExtendSurface(insoleAxisXExtrusionSrf, IOnSurface.ISO.N_iso, extrusionLenght, True)
        extrusionResult = extrusionResult And RhUtil.RhinoExtendSurface(insoleAxisXExtrusionSrf, IOnSurface.ISO.S_iso, extrusionLenght, True)
        extrusionResult = extrusionResult And RhUtil.RhinoExtendSurface(insoleAxisXExtrusionSrf, IOnSurface.ISO.W_iso, extrusionLenght, True)
        extrusionResult = extrusionResult And RhUtil.RhinoExtendSurface(insoleAxisXExtrusionSrf, IOnSurface.ISO.E_iso, extrusionLenght, True)
        If Not extrusionResult Then
            Throw New Exception("Impossibile estendere la curva longitudinale del plantare inferiore")
        End If
'#If DEBUG Then
'        RhDebug.AddDocumentToDebug(insoleAxisXExtrusionSrf, "insoleAxisXExtrusionSrf")
'#End If
        Dim intersectionCurves() As OnCurve = {}
        Dim intersectionPoints As New On3dPointArray
        'INTERSECO CURVA BORDO CON SRF ESTRUSA      
        If Not RhUtil.RhinoCurveBrepIntersect(insoleBottomBorder, insoleAxisXExtrusionSrf.BrepForm, 0.001, {}, intersectionPoints) Then
            Throw New Exception("Impossibile intersecare la superficie del plantare inferiore con la superficie estrusa dall'asse longitudinale")
        End If
        If intersectionPoints Is Nothing OrElse intersectionPoints.Count <> 2 Then
            Throw New Exception("Impossibile intersecare la superficie del plantare inferiore con la superficie estrusa dall'asse longitudinale")
        End If
        Dim pointP2 = intersectionPoints.Cast(Of On3dPoint)().OrderByDescending(Function(p) p.DistanceTo(pointP1)).First()        
        RhUtil.RhinoIntersectBreps(insoleAxisXExtrusionSrf.BrepForm, insoleBottomSrfObjref.Brep(), Doc.AbsoluteTolerance, intersectionCurves, intersectionPoints)
        If intersectionCurves.Length <> 1 Then
            Throw New Exception("Impossibile intersecare la superficie del plantare inferiore con la superficie estrusa dall'asse longitudinale")
        End If
        Dim insoleBottomAxisCrv1 = intersectionCurves(0).DuplicateCurve()
        'EVENTUALE ROTAZIONE DELL'UTENTE
        RhUtil.RhinoExtendCurve(insoleBottomAxisCrv1, IRhinoExtend.Type.Line, 0, 10)
        RhUtil.RhinoExtendCurve(insoleBottomAxisCrv1, IRhinoExtend.Type.Line, 1, 10)
        Dim insoleBottomAxisCrvObj = Doc.AddCurveObject(insoleBottomAxisCrv1)
        IdInsoleBottomAxisCrv = insoleBottomAxisCrvObj.Attributes().m_uuid        
        Dim centerRotation = pointP1.x.ToString().Replace(",", ".") & "," & pointP1.y .ToString().Replace(",", ".") & "," & pointP1.z.ToString().Replace(",", ".")
        Dim p2Rotation = pointP2.x.ToString().Replace(",", ".") & "," & pointP2.y.ToString().Replace(",", ".") & "," & pointP2.z.ToString().Replace(",", ".")
        App.RunScript("_SelNone", 0)
        insoleBottomAxisCrvObj.Select(true, true)
        Dim rotateCmd = "-_Rotate _C=_N " & centerRotation & " " & p2Rotation
        App.RunScript(rotateCmd, 0)
        App.RunScript("_SelNone", 0)        
        'ESTRUDO CURVA RUOTATA
        Dim insoleAxisRotatedObjRef = New MRhinoObjRef(IdInsoleBottomAxisCrv)        
        Dim insoleAxisXExtrusionRotatedSrf = RhUtil.RhinoExtrudeCurveStraight(insoleAxisRotatedObjRef.Curve(), New On3dVector(0, 0, 1), extrusionLenght)
        RhUtil.RhinoExtendSurface(insoleAxisXExtrusionRotatedSrf, IOnSurface.ISO.N_iso, extrusionLenght, True)
        RhUtil.RhinoExtendSurface(insoleAxisXExtrusionRotatedSrf, IOnSurface.ISO.S_iso, extrusionLenght, True)
        RhUtil.RhinoExtendSurface(insoleAxisXExtrusionRotatedSrf, IOnSurface.ISO.W_iso, extrusionLenght, True)
        RhUtil.RhinoExtendSurface(insoleAxisXExtrusionRotatedSrf, IOnSurface.ISO.E_iso, extrusionLenght, True)              
        Doc.PurgeObject(insoleBottomAxisCrvObj)
        Doc.DeleteObject(insoleAxisRotatedObjRef)
        'INTERSECO CURVA BORDO CON SRF ESTRUSA RUOTATA PER AGGIORNARE P2     
        intersectionPoints = New On3dPointArray
        If Not RhUtil.RhinoCurveBrepIntersect(insoleBottomBorder, insoleAxisXExtrusionRotatedSrf.BrepForm, 0.001, {}, intersectionPoints) Then
            Throw New Exception("Impossibile intersecare la superficie del plantare inferiore con la superficie estrusa dall'asse longitudinale ruotato")
        End If
        If intersectionPoints Is Nothing OrElse intersectionPoints.Count <> 2 Then
            Throw New Exception("Impossibile intersecare la superficie del plantare inferiore con la superficie estrusa dall'asse longitudinale ruotato")
        End If
        pointP2 = intersectionPoints.Cast(Of On3dPoint)().OrderByDescending(Function(p) p.DistanceTo(pointP1)).First()        
        Dim pointP2ObjRef = New MRhinoObjRef(Doc.AddPointObject(pointP2).Attributes.m_uuid)
        IdPointP2 = pointP2ObjRef.m_uuid
        'INTERSECO PLANTARE CON SRF ESTRUSA RUOTATA PER AGGIORNARE ASSE PLANTARE        
        intersectionCurves = {}
        App.RunScript("_SelNone", 0)
        RhUtil.RhinoIntersectBreps(insoleAxisXExtrusionRotatedSrf.BrepForm, insoleBottomSrfObjref.Brep(), Doc.AbsoluteTolerance, intersectionCurves, intersectionPoints)
        If intersectionCurves.Length <> 1 Then
            Throw New Exception("Impossibile intersecare la superficie del plantare inferiore con la superficie estrusa dall'asse longitudinale ruotato")
        End If               
        IdInsoleBottomAxisCrv = Doc.AddCurveObject(intersectionCurves(0).DuplicateCurve()).Attributes().m_uuid
        App.RunScript("_SelNone", 0)
        'Dispose
        insoleAxisXExtrusionCrv.Dispose()
        insoleAxisXExtrusionSrf.Dispose()
        intersectionPoints.Dispose()
        insoleBottomSrfObjref.Dispose()
        insoleBottomAxisCrvObj.Dispose()        
    End Sub


#End Region


#Region " Utils "

    ''' <summary>
    ''' Una volta creata la curva relativa al bordo partendo dai punti, va proiettata sulla srf inferiore del plantare
    ''' </summary>
    Protected Sub ProjectBordoInterno(side As eSide)     
        'ESTRUDO CURVA   
        Dim crvBordoInternoObjRef = New MRhinoObjRef(IdBordoInternoCrv)               
        Dim crvBordoInternoSrf = RhUtil.RhinoExtrudeCurveStraight(crvBordoInternoObjRef.Curve(), New On3dVector(0, 0, 1), 10)
        RhUtil.RhinoExtendSurface(crvBordoInternoSrf, IOnSurface.ISO.N_iso, 100, True)
        RhUtil.RhinoExtendSurface(crvBordoInternoSrf, IOnSurface.ISO.S_iso, 100, True)
        RhUtil.RhinoExtendSurface(crvBordoInternoSrf, IOnSurface.ISO.W_iso, 100, True)
        RhUtil.RhinoExtendSurface(crvBordoInternoSrf, IOnSurface.ISO.E_iso, 100, True)
'#If DEBUG Then
'        RhDebug.AddDocumentToDebug(crvBordoInternoSrf, "crvBordoInternoSrf")
'#End If

        'ELIMINO DAL DOC VECCHIA CURVA
        Doc.DeleteObject(crvBordoInternoObjRef)

        'INTERSECO COL PLANTARE
        Dim insoleBottomSrfObjref = Element3dManager.GetRhinoObjRef(eReferences.insoleBottomSurface, side)    
        Dim curves() As OnCurve = Nothing
        Dim points As New On3dPointArray
        If Not RhUtil.RhinoIntersectBreps(insoleBottomSrfObjref.Brep, crvBordoInternoSrf.BrepForm, 0.001, curves, points) Then
            Throw New Exception("Intersezione tra brep plantare e bordo interno fallita")
        End If
        If curves.Length <> 1 Then Throw New Exception("Intersezione tra brep plantare e bordo interno fallita, numero curve non corretto")
        Dim crvBordoInternoNew As OnCurve = curves(0).DuplicateCurve()
'#If DEBUG Then
'        RhDebug.AddDocumentToDebug(crvBordoInternoNew, "crvBordoInternoNew")
'#End If

        'SOVRASCRIVO IdBordoInternoCrv
        IdBordoInternoCrv = Doc.AddCurveObject(crvBordoInternoNew).Attributes().m_uuid
    End Sub

    Protected Function GetInsoleBottomAxisVector() As On3dVector
        Dim p1 = New MRhinoObjRef(IdPointP1).Point().point
        Dim p2 = New MRhinoObjRef(IdPointP2).Point().point
        Return New On3dPoint(p2) - New On3dPoint(p1)                
    End Function

    Private Function GetCutterPlane(insoleBbox As OnBoundingBox, positionX As Double) As OnPlaneSurface
        Dim lenght = insoleBbox.Diagonal().Length()
         Dim originPoint = New On3dPoint(positionX, insoleBbox.Min().y - lenght, insoleBbox.Min().z -lenght)
            Dim planePoint1 = New On3dPoint(positionX, insoleBbox.Max().y + lenght, insoleBbox.Min().z -lenght)
            Dim planePoint2 = New On3dPoint(positionX, insoleBbox.Max().y + lenght, insoleBbox.Max().z +lenght)
        Dim yDirection = New On3dVector(planePoint1 - originPoint)
        Dim zDirection = New On3dVector(planePoint2 - originPoint)
        Dim plane = New OnPlane(originPoint, yDirection, zDirection)
        Dim planeSurface As New OnPlaneSurface(plane)
        planeSurface.Extend(0, New OnInterval(-lenght, lenght * 2))
        planeSurface.Extend(1, New OnInterval(-lenght, lenght * 2))
        planeSurface.Extend(2, New OnInterval(-lenght, lenght * 2))
        planeSurface.Extend(3, New OnInterval(-lenght, lenght * 2))        
'#If DEBUG
'        RhDebug.AddDocumentToDebug(planeSurface, "planeSurface")
'#End If
        Return planeSurface
    End Function


    Protected Function GetCurveBy11Points(curve As IOnCurve, decrescente As Boolean) As OnCurve
        Dim t1, t2 As Double
        If Not curve.GetDomain(t1, t2) Then
            Throw New Exception("Impossibile trovare i punti di P1-A1")
        End If
        Dim points11 As New On3dPointArray
        Dim delta = (Math.Max(t1, t2) - Math.Min(t1, t2)) / 10
        For i = 0 To 10
            Dim domain = t1 + delta * i
            Dim p = curve.PointAt(domain)
            points11.Append(p)
        Next
        Dim points11Ordinate = points11.Cast(Of On3dPoint)().OrderBy(Function(p) p.x).ToList()
        Dim points11Traslated As New SortedList(Of Double, On3dPoint)
        For i = 0 To points11.Count() - 1
            Dim traslazioneZ = i * (Me.Spessore / 10)
            If Not decrescente Then traslazioneZ = Me.Spessore - i * (Me.Spessore / 10)
            Dim newP As New On3dPoint(points11Ordinate.Item(i).x, points11Ordinate.Item(i).y, points11Ordinate.Item(i).z - traslazioneZ)
            points11Traslated.Add(newP.x, newP)
'#If DEBUG
'            RhDebug.AddDocumentToDebug(newP, "p_"&i)
'#End If            
        Next        
        Return RhGeometry.CreaCurvaDaCV(points11Traslated.Values)
    End Function


    ''' <summary>
    ''' Dato un punto notevole interno imposta il relativo sul bordo esterno
    ''' </summary>
    ''' <param name="insoleBottomBorder"></param>
    ''' <param name="point"></param>    
    Protected Function ProjectPoint(side As eSide, insoleBottomBorder As IOnCurve, point As IOn3dPoint) As MRhinoObject
        Dim extrusionLenght = insoleBottomBorder.BoundingBox().Diagonal().Length()
        Dim line As New OnLineCurve(New On3dPoint(point.x, point.y, point.z + extrusionLenght), New On3dPoint(point.x, point.y, point.z - extrusionLenght))
        'LA DIREZIONE DIPENDE SE è MEDIALE O LATERALE E DAL SIDE      
        Dim vectorAxis = GetInsoleBottomAxisVector()
        vectorAxis.Unitize()
        Dim xform As New OnXform
        Dim rotationAngle = Math.PI/2
        If side = eSide.right And Position = ePosizionePianiInclinato.laterale Or side = eSide.left And Position = ePosizionePianiInclinato.mediale Then rotationAngle = -rotationAngle        
        xform.Rotation(rotationAngle, OnUtil.On_zaxis, point)
        vectorAxis.Transform(xform)
        Dim direction = New On3dVector(vectorAxis.x, vectorAxis.y, 0)
        Dim extrusionSrf = RhUtil.RhinoExtrudeCurveStraight(line, direction, extrusionLenght)
        Dim intersectionCurves() As OnCurve = {}
        Dim intersectionPoints As New On3dPointArray
        If Not RhUtil.RhinoCurveBrepIntersect(insoleBottomBorder, extrusionSrf.BrepForm, 0.001, intersectionCurves, intersectionPoints) Then
            Throw New Exception("Impossibile eseguire la proiezione dei punti notevoli sul bordo")
        End If
        If intersectionPoints.Count() <> 1 Then
            Throw New Exception("Impossibile eseguire la proiezione dei punti notevoli sul bordo")
        End If
        Dim resObj = Doc.AddPointObject(intersectionPoints(0))
        intersectionPoints.Dispose()
        direction.Dispose()
        xform.Dispose()
        vectorAxis.Dispose()
        Return resObj
    End Function


    Protected Sub PulisciOggetiCostruzioneComuni()
        Doc.DeleteObject(New MRhinoObjRef(IdPointP1()))
        Doc.DeleteObject(New MRhinoObjRef(IdPointP2()))
        Doc.DeleteObject(New MRhinoObjRef(IdPointCV()))
        Doc.DeleteObject(New MRhinoObjRef(IdPointA()))
        Doc.DeleteObject(New MRhinoObjRef(IdPointB()))
        Doc.DeleteObject(New MRhinoObjRef(IdPointC()))
        Doc.DeleteObject(New MRhinoObjRef(IdPointD()))
        Doc.DeleteObject(New MRhinoObjRef(IdPointE()))
        Doc.DeleteObject(New MRhinoObjRef(IdPointCV1()))
        Doc.DeleteObject(New MRhinoObjRef(IdPointA1()))
        Doc.DeleteObject(New MRhinoObjRef(IdPointB1()))
        Doc.DeleteObject(New MRhinoObjRef(IdPointC1()))
        Doc.DeleteObject(New MRhinoObjRef(IdPointD1()))
        Doc.DeleteObject(New MRhinoObjRef(IdPointE1()))
    End Sub


    Public Shadows Overridable Function ToString() As String
        Return GetTypeName(Me.Type) & " " & GetPositionName(Me.Position) & " " & Me.Spessore & " mm"
    End Function


#End Region


#Region " IClonable "

    Public MustOverride Function Clone() As Object Implements ICloneable.Clone


#End Region


#Region " Serializzazione/deserializzazione"

    Protected Function CommonSerialize(ByRef archive As OnBinaryArchive) As Boolean
        'integer
        If Not archive.WriteInt(Type) Then Return False
        If Not archive.WriteInt(Position) Then Return False

        'double
        If Not archive.WriteDouble(Spessore) Then Return False      

        Return True
    End Function

    Public Shared Function CommonDeserialize(ByRef archive As OnBinaryArchive, ByRef type As FactoryPianiInclinati.eTipoPianoInclinato, byref position As FactoryPianiInclinati.ePosizionePianiInclinato, ByRef spessore As Double) As Boolean
        'integer      
        Dim typeCount As Integer = -1
        If Not archive.ReadInt(typeCount) Then Return False
        type = typeCount
        Dim positionCount As Integer = -1
        If Not archive.ReadInt(positionCount) Then Return False
        position = positionCount
        
        'double         
        If Not archive.ReadDouble(spessore) Then Return False

        Return True
    End Function

    Public MustOverride Function Serialize(ByRef archive As OnBinaryArchive) As Boolean Implements IOnSerializable.Serialize

    Public MustOverride Function Deserialize(ByRef archive As OnBinaryArchive) As Boolean Implements IOnSerializable.Deserialize


#End Region





End Class
