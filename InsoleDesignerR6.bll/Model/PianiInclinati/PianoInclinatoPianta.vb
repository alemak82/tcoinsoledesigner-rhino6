Imports RMA.OpenNURBS
Imports RhinoUtils
Imports RMA.Rhino
Imports RhinoUtils.RhGeometry
Imports RhinoUtils.RhDebug
Imports InsoleDesigner.bll.IdElement3dManager
Imports InsoleDesigner.bll.IdLanguageManager
Imports InsoleDesigner.bll.IdAlias
Imports InsoleDesigner.bll.FactoryPianiInclinati


Public Class PianoInclinatoPianta
    Inherits AbstractPianiInclinati
    Implements IOnSerializable
    Implements ICloneable


#Region " Constructor "

    Public Sub New(position As ePosizionePianiInclinato, spessore As Double)
        MyBase.New(eTipoPianoInclinato.pianta, position, spessore)
    End Sub

#End Region


#Region " PROPERTY "

    Private Property IdPointBT() As Guid
    Private Property IdPointF() As Guid
    Private Property IdPointBT1() As Guid
    Private Property IdPointF1() As Guid

#End Region


#Region " Passi procedura "

    ''' <summary>
    ''' BORDO ESTERNO PROIETTATO - VEDI procedura 8-12
    ''' </summary>
    Public Sub SetBordoEsternoProiettato()   
        'CURVE A1_B1 E B1_F1
        Dim curveToTrimObjRef = New MRhinoObjRef(IdBordoEsternoSuperioreCrv)
        Dim curveToTrim = curveToTrimObjRef.Curve().DuplicateCurve()
        Dim extrusionLenght = curveToTrim.BoundingBox().Diagonal().Length() * 2 ' 10
        Dim pointB1 = New MRhinoObjRef(IdPointB1).Point().point
        Dim zMaxB1 = New On3dPoint(pointB1.x, pointB1.y, pointB1.z + extrusionLenght)
        Dim zMinB1 = New On3dPoint(pointB1.x, pointB1.y, pointB1.z - extrusionLenght)
        Dim lineB1 As New OnLineCurve(zMaxB1, zMinB1)                  
        Dim trimmingSurfaceB1 = RhUtil.RhinoExtrudeCurveStraight(lineB1, New On3dVector(0,1,0), extrusionLenght)
        RhUtil.RhinoExtendSurface(trimmingSurfaceB1, IOnSurface.ISO.N_iso, extrusionLenght, True)
        RhUtil.RhinoExtendSurface(trimmingSurfaceB1, IOnSurface.ISO.S_iso, extrusionLenght, True)
        RhUtil.RhinoExtendSurface(trimmingSurfaceB1, IOnSurface.ISO.W_iso, extrusionLenght, True)
        RhUtil.RhinoExtendSurface(trimmingSurfaceB1, IOnSurface.ISO.E_iso, extrusionLenght, True) 
        Dim intersectionPoints As New On3dPointArray      
        Dim curves As OnCurveArray = RhGeometry.SplitCurveBySurface(curveToTrim, trimmingSurfaceB1, intersectionPoints)
        If curves Is Nothing OrElse curves.Count() <> 2 Then
            RhDebug.AddDocumentToDebug(trimmingSurfaceB1, "trimmingSurfaceB1") 
            Throw New Exception("Impossibile trovare la curva A1-B1")
        End If
        Dim curvaA1_B1 = curves.Cast(Of OnCurve)().OrderBy(Function(c) c.BoundingBox().Min().x).First().DuplicateCurve()
        Dim curvaB1_F1 = curves.Cast(Of OnCurve)().OrderBy(Function(c) c.BoundingBox().Min().x).Last().DuplicateCurve()

        'CURVE B1_E1 E E1_F1
        Dim pointE1 = New MRhinoObjRef(IdPointE1).Point().point
        Dim zMaxE1 = New On3dPoint(pointE1.x, pointE1.y, pointE1.z + extrusionLenght)
        Dim zMinE1 = New On3dPoint(pointE1.x, pointE1.y, pointE1.z - extrusionLenght)
        Dim lineE1 As New OnLineCurve(zMaxE1, zMinE1)     
        Dim trimmingSurfaceE1 = RhUtil.RhinoExtrudeCurveStraight(lineE1, New On3dVector(0,1,0), extrusionLenght)
        RhUtil.RhinoExtendSurface(trimmingSurfaceE1, IOnSurface.ISO.N_iso, extrusionLenght, True)
        RhUtil.RhinoExtendSurface(trimmingSurfaceE1, IOnSurface.ISO.S_iso, extrusionLenght, True)
        RhUtil.RhinoExtendSurface(trimmingSurfaceE1, IOnSurface.ISO.W_iso, extrusionLenght, True)
        RhUtil.RhinoExtendSurface(trimmingSurfaceE1, IOnSurface.ISO.E_iso, extrusionLenght, True)
        curves = RhGeometry.SplitCurveBySurface(curvaB1_F1, trimmingSurfaceE1, intersectionPoints)   
        If curves Is Nothing OrElse curves.Count() <> 2 Then
            Throw New Exception("Impossibile trovare la curva E1-F1")
        End If
        Dim curvaB1_E1 = curves.Cast(Of OnCurve)().OrderBy(Function(c) c.BoundingBox().Min().x).First().DuplicateCurve()
        Dim curvaE1_F1 = curves.Cast(Of OnCurve)().OrderBy(Function(c) c.BoundingBox().Min().x).Last().DuplicateCurve()

        'CURVA P1-A1 PROIETTATA CON 11 PUNTI      
        Dim curvaA1_B1_proiettata = GetCurveBy11Points(curvaA1_B1, True)

        'PUNTI D1-E1 PROIETTATA CON 11 PUNTI
        Dim curvaE1_F1_proiettata = GetCurveBy11Points(curvaE1_F1, False)
        'CURVA A1_D1 TRASLATA
        Dim curvaB1_E1_proiettata = curvaB1_E1.DuplicateCurve()
        Dim xform As New OnXform
        xform.Translation(0, 0, -Me.Spessore)
        curvaB1_E1_proiettata.Transform(xform)
'#If DEBUG
'        RhDebug.AddDocumentToDebug(curvaA1_B1_proiettata, "curvaA1_B1_proiettata")
'        RhDebug.AddDocumentToDebug(curvaB1_E1_proiettata, "curvaB1_E1_proiettata")
'        RhDebug.AddDocumentToDebug(curvaE1_F1_proiettata, "curvaE1_F1_proiettata")
'#End If
        Dim inputCurves = {curvaA1_B1_proiettata, curvaB1_E1_proiettata, curvaE1_F1_proiettata}
        Dim joinedCurves() As OnCurve = {}
        If Not RhUtil.RhinoMergeCurves(inputCurves, joinedCurves)
            Throw New Exception("Impossibile unire le curve del bordo esterno inferiore")
        End If
        If joinedCurves.Length <> 1
            Throw New Exception("Impossibile unire le curve del bordo esterno inferiore")
        End If
        IdBordoEsternoInferioreCrv = Doc.AddCurveObject(joinedCurves(0)).Attributes().m_uuid

        'ESTRUDO LE 4 CURVE
        Dim bordoEsterno = New MRhinoObjRef(IdBordoEsternoSuperioreCrv).Curve().DuplicateCurve()
        extrusionLenght *= 10
        Dim extrudedCurvaA1_B1_proiettata = RhUtil.RhinoExtrudeCurveStraight(curvaA1_B1_proiettata, New On3dVector(0,1,0), extrusionLenght)
        Dim extrudedCurvaB1_E1_proiettata = RhUtil.RhinoExtrudeCurveStraight(curvaB1_E1_proiettata, New On3dVector(0,1,0), extrusionLenght)
        Dim extrudedCurvaE1_F1_proiettata = RhUtil.RhinoExtrudeCurveStraight(curvaE1_F1_proiettata, New On3dVector(0,1,0), extrusionLenght)
        Dim extrudedCurvaBordoEsterno = RhUtil.RhinoExtrudeCurveStraight(bordoEsterno, New On3dVector(0,1,0), extrusionLenght)
        xform.Identity()
        xform.Translation(0,-extrusionLenght/2,0)
        extrudedCurvaA1_B1_proiettata.Transform(xform)
        extrudedCurvaB1_E1_proiettata.Transform(xform)
        extrudedCurvaE1_F1_proiettata.Transform(xform)
        extrudedCurvaBordoEsterno.Transform(xform)
        Dim A1_B1SrfObj = Doc.AddSurfaceObject(extrudedCurvaA1_B1_proiettata)
        Dim B1_E1SrfObj = Doc.AddSurfaceObject(extrudedCurvaB1_E1_proiettata)
        Dim E1_F1SrfObj = Doc.AddSurfaceObject(extrudedCurvaE1_F1_proiettata)                                        
        Dim extrudedCurvaBordoEsternoObj = Doc.AddSurfaceObject(extrudedCurvaBordoEsterno)

        'UNISCO LE SUPERFICI
        App.RunScript("_SelNone", 0)
        A1_B1SrfObj.Select(True, True)
        B1_E1SrfObj.Select(True, True)
        E1_F1SrfObj.Select(True, True)
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

        'Dispose
        intersectionPoints.Dispose()
        getObjects.Dispose()
        intersectionSrfObj.Dispose()
        A1_B1SrfObj.Dispose()
        B1_E1SrfObj.Dispose()
        E1_F1SrfObj.Dispose()
        extrudedCurvaBordoEsternoObj.Dispose()
        bordoBbox.Dispose()
        plane.Dispose()
        planeSurface.Dispose()
        curveToTrimObjRef.Dispose()
        curveToTrim.Dispose()
    End Sub

    ''' <summary>
    ''' CURVE DA PUNTI DI CONTROLLO P1-E1 - VEDI procedura 7
    ''' </summary>
    ''' <remarks>Usato script perchè non riesco con la funzione RhGeometry.CreaCurvaDaCV()</remarks>
    Public Sub SetBordoInterno(side As eSide)
        Dim pointA1 = New MRhinoObjRef(IdPointA1).Point().point
        Dim pointA = New MRhinoObjRef(IdPointA).Point().point
        Dim pointC = New MRhinoObjRef(IdPointC).Point().point    
        Dim pointBT = New MRhinoObjRef(IdPointBT).Point().point   
        Dim pointF = New MRhinoObjRef(IdPointF).Point().point     
        Dim pointF1 = New MRhinoObjRef(IdPointF1).Point().point     
        Dim getObjects As New MRhinoGetObject
        App.RunScript("_SelNone", 0)
        Dim scriptCommand = IIf(RhinoLanguageSetting() = elanguage.Italian, "-_Curve _G 3 C=No", "-_Curve _D 3 P=No").ToString()
        scriptCommand = scriptCommand & " " & pointA1.x.ToString().Replace(",",".") & "," & pointA1.y.ToString().Replace(",",".") & "," & pointA1.z.ToString().Replace(",",".")
        scriptCommand = scriptCommand & " " & pointA.x.ToString().Replace(",",".") & "," & pointA.y.ToString().Replace(",",".") & "," & pointA.z.ToString().Replace(",",".")
        scriptCommand = scriptCommand & " " & pointC.x.ToString().Replace(",",".") & "," & pointC.y.ToString().Replace(",",".") & "," & pointC.z.ToString().Replace(",",".")
        scriptCommand = scriptCommand & " _Enter"
        App.RunScript(scriptCommand, 0)
        App.RunScript("_SelLast", 0)
        getObjects.SetGeometryFilter(IRhinoGetObject.GEOMETRY_TYPE_FILTER.curve_object)
        getObjects.GetObjects(0, Integer.MaxValue)
        If getObjects.ObjectCount <> 1 Then Throw New Exception("Impossibile creare la curca A1-C")
        Dim curveObjRefA1_C = New MRhinoObjRef(getObjects.Object(0))
'#If DEBUG
'        AddDocumentToDebug(curveObjRefA1_C.Curve().DuplicateCurve(), "curveObjRefA1_C")
'#End If
        App.RunScript("_SelNone", 0)
        scriptCommand = IIf(RhinoLanguageSetting() = elanguage.Italian, "-_Curve _G 3 C=No", "-_Curve _D 3 P=No").ToString()
        scriptCommand = scriptCommand & " " & pointC.x.ToString().Replace(",",".") & "," & pointC.y.ToString().Replace(",",".") & "," & pointC.z.ToString().Replace(",",".")
        scriptCommand = scriptCommand & " " & pointBT.x.ToString().Replace(",",".") & "," & pointBT.y.ToString().Replace(",",".") & "," & pointBT.z.ToString().Replace(",",".")
        scriptCommand = scriptCommand & " _Enter"
        App.RunScript(scriptCommand, 0)
        App.RunScript("_SelLast", 0)
        getObjects.ClearObjects()
        getObjects.SetGeometryFilter(IRhinoGetObject.GEOMETRY_TYPE_FILTER.curve_object)
        getObjects.GetObjects(0, Integer.MaxValue)
        If getObjects.ObjectCount <> 1 Then Throw New Exception("Impossibile creare la curva C-BT")
        Dim curveObjRefC_BT = New MRhinoObjRef(getObjects.Object(0))
'#If DEBUG
'        AddDocumentToDebug(curveObjRefC_BT.Curve().DuplicateCurve(), "curveObjRefC_BT")
'#End If
        App.RunScript("_SelNone", 0)
        scriptCommand = IIf(RhinoLanguageSetting() = elanguage.Italian, "-_Curve _G 3 C=No", "-_Curve _D 3 P=No").ToString()
        scriptCommand = scriptCommand & " " & pointBT.x.ToString().Replace(",",".") & "," & pointBT.y.ToString().Replace(",",".") & "," & pointBT.z.ToString().Replace(",",".")
        scriptCommand = scriptCommand & " " & pointF.x.ToString().Replace(",",".") & "," & pointF.y.ToString().Replace(",",".") & "," & pointF.z.ToString().Replace(",",".")
        scriptCommand = scriptCommand & " " & pointF1.x.ToString().Replace(",",".") & "," & pointF1.y.ToString().Replace(",",".") & "," & pointF1.z.ToString().Replace(",",".")
        scriptCommand = scriptCommand & " _Enter"
        App.RunScript(scriptCommand, 0)
        App.RunScript("_SelLast", 0)
        getObjects.ClearObjects()
        getObjects.SetGeometryFilter(IRhinoGetObject.GEOMETRY_TYPE_FILTER.curve_object)
        getObjects.GetObjects(0, Integer.MaxValue)
        If getObjects.ObjectCount <> 1 Then Throw New Exception("Impossibile creare la curva BT-F1")
        Dim curveObjRefBT_F1 = New MRhinoObjRef(getObjects.Object(0))
'#If DEBUG
'        AddDocumentToDebug(curveObjRefBT_F1.Curve().DuplicateCurve(), "curveObjRefBT_F1")
'#End If
        App.RunScript("_SelNone", 0)
        curveObjRefA1_C.Object().Select(True, True)
        curveObjRefC_BT.Object().Select(True, True)
        curveObjRefBT_F1.Object().Select(True, True)
        App.RunScript("_Join", 0)
        App.RunScript("_SelNone", 0)
        App.RunScript("_SelLast", 0)
        getObjects.ClearObjects()
        getObjects.SetGeometryFilter(IRhinoGetObject.GEOMETRY_TYPE_FILTER.curve_object)
        getObjects.GetObjects(0, Integer.MaxValue)
        If getObjects.ObjectCount <> 1 Then Throw New Exception("Impossibile unire le curve A1-C e C-BT e BT-F1")
        IdBordoInternoCrv = getObjects.Object(0).m_uuid
        App.RunScript("_SelNone", 0)
        getObjects.Dispose()
        curveObjRefA1_C.Dispose()
        curveObjRefC_BT.Dispose()
        curveObjRefBT_F1.Dispose()
        
        'Proiezione sul plantare
        ProjectBordoInterno(side)
    End Sub

    ''' <summary>
    ''' BORDO ESTERNO PIANO INCLINATO - VEDI procedura 6
    ''' </summary>   
    Public Sub SetBordoEsterno(side As eSide)
        Dim insoleBottomSrfObjref = Element3dManager.GetRhinoObjRef(eReferences.insoleBottomSurface, side)       
        Dim insoleBottomBorder As IOnCurve = RhGeometry.ExtractSurfaceBorder(insoleBottomSrfObjref)
        'SRF CHE PASSA PER IL PUNTO A
        Dim pointA = New MRhinoObjRef(IdPointA).Point().point
        Dim extrusionLenght = insoleBottomBorder.BoundingBox().Diagonal().Length()
        Dim lineA As New OnLineCurve(New On3dPoint(pointA.x, pointA.y, pointA.z + extrusionLenght), New On3dPoint(pointA.x, pointA.y, pointA.z - extrusionLenght))        
        Dim vectorAxis = GetInsoleBottomAxisVector()
        vectorAxis.Unitize()
        Dim xform As New OnXform
        Dim rotationAngle = Math.PI/2
        If side = eSide.right And Position = ePosizionePianiInclinato.laterale Or side = eSide.left And Position = ePosizionePianiInclinato.mediale Then rotationAngle = -rotationAngle 
        xform.Rotation(rotationAngle, OnUtil.On_zaxis, pointA)
        vectorAxis.Transform(xform)        
        Dim extrusionSrf1 = RhUtil.RhinoExtrudeCurveStraight(lineA, New On3dVector(vectorAxis.x, vectorAxis.y, 0), extrusionLenght)
        Dim extrusionSrfObj1 = Doc.AddSurfaceObject(extrusionSrf1)
        'SRF CHE PASSA PER IL PUNTO F
        Dim pointF = New MRhinoObjRef(IdPointF).Point().point
        Dim lineF As New OnLineCurve(New On3dPoint(pointF.x, pointF.y, pointF.z + extrusionLenght), New On3dPoint(pointF.x, pointF.y, pointF.z - extrusionLenght))
        vectorAxis = GetInsoleBottomAxisVector()
        vectorAxis.Unitize()
        xform.Identity()
        xform.Rotation(rotationAngle, OnUtil.On_zaxis, pointF)
        vectorAxis.Transform(xform)
        Dim extrusionSrf2 = RhUtil.RhinoExtrudeCurveStraight(lineF, New On3dVector(vectorAxis.x, vectorAxis.y, 0), extrusionLenght)
        Dim extrusionSrfObj2 = Doc.AddSurfaceObject(extrusionSrf2)
        'SPLIT
        Dim curvaDiBordo = Doc.AddCurveObject(insoleBottomBorder)
        Dim objToTrimId As String = curvaDiBordo.Attributes().m_uuid.ToString()
        Dim trimmerId1 As String = extrusionSrfObj1.Attributes.m_uuid.ToString()
        Dim trimmerId2 As String = extrusionSrfObj2.Attributes.m_uuid.ToString()
        App.RunScript("_SelNone", 0)
        Dim splitcommand As String = "-_Split _SelID " & objToTrimId & " _Enter _SelId " & trimmerId1 & " _SelId " & trimmerId2 & " _Enter"
        App.RunScript(splitcommand, 0)
        'Leggo le curve risultanti
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
        lineA.Dispose()
        lineF.Dispose()
        extrusionSrf1.Dispose()
        extrusionSrf2.Dispose()
        getObjects.Dispose()
        extrusionSrfObj1.Dispose()
        extrusionSrfObj2.Dispose()
        App.RunScript("_SelNone", 0)
    End Sub

    ''' <summary>
    ''' PROIETTO PUNTI SUL BORDO - VEDI procedura 5.2
    ''' </summary>
    Public Sub ProjectPoints(side As eSide)
        Dim insoleBottomSrfObjref = Element3dManager.GetRhinoObjRef(eReferences.insoleBottomSurface, side)  
        Dim insoleBottomBorder As IOnCurve = RhGeometry.ExtractSurfaceBorder(insoleBottomSrfObjref)
        IdPointA1 = ProjectPoint(side, insoleBottomBorder, New MRhinoObjRef(IdPointA).Point().point).Attributes().m_uuid
        IdPointB1 = ProjectPoint(side, insoleBottomBorder, New MRhinoObjRef(IdPointB).Point().point).Attributes().m_uuid
        IdPointC1 = ProjectPoint(side, insoleBottomBorder, New MRhinoObjRef(IdPointC).Point().point).Attributes().m_uuid
        IdPointD1 = ProjectPoint(side, insoleBottomBorder, New MRhinoObjRef(IdPointD).Point().point).Attributes().m_uuid
        IdPointE1 = ProjectPoint(side, insoleBottomBorder, New MRhinoObjRef(IdPointE).Point().point).Attributes().m_uuid
        IdPointF1 = ProjectPoint(side, insoleBottomBorder, New MRhinoObjRef(IdPointF).Point().point).Attributes().m_uuid
        IdPointBT1 = ProjectPoint(side, insoleBottomBorder, New MRhinoObjRef(IdPointBT).Point().point).Attributes().m_uuid
        IdPointCV1 = ProjectPoint(side, insoleBottomBorder, New MRhinoObjRef(IdPointCV).Point().point).Attributes().m_uuid
    End Sub

    ''' <summary>
    ''' PUNTI A B C D E - VEDI procedura 5.1
    ''' </summary>
    Public Sub Set_A_B_C_D_E_F()
        Dim pointCV = New MRhinoObjRef(IdPointCV).Point().point
        Dim pointBT = New MRhinoObjRef(IdPointBT).Point().point
        Dim pointP1 = New MRhinoObjRef(IdPointP2).Point().point
        Dim pointP2 = New MRhinoObjRef(IdPointP2).Point().point
        'Dim L1 = pointCV.DistanceTo(pointBT)
        'Dim L2 = pointBT.DistanceTo(pointP2)       
        Dim t as Double
        Dim insoleBottomAxis = New MRhinoObjRef(IdInsoleBottomAxisCrv).Curve().DuplicateCurve()   

        Dim vectorCV_P1 = New On3dPoint(pointP1) - New On3dPoint(pointCV)
        vectorCV_P1.Unitize()
        Dim nearPoint = New On3dPoint(pointCV.x - vectorCV_P1.x * 10, pointCV.y - vectorCV_P1.y * 10, pointCV.z - vectorCV_P1.z * 10)
        insoleBottomAxis.GetClosestPoint(nearPoint, t)
        Dim pointA = insoleBottomAxis.PointAt(t)
        IdPointA = Doc.AddPointObject(pointA).Attributes.m_uuid

        Dim vectorL1 = New On3dPoint(pointBT) - New On3dPoint(pointCV)
        nearPoint = New On3dPoint(pointCV.x + vectorL1.x /5, pointCV.y + vectorL1.y /5, pointCV.z + vectorL1.z /5)
        insoleBottomAxis.GetClosestPoint(nearPoint, t)
        Dim pointB = insoleBottomAxis.PointAt(t)
        IdPointB = Doc.AddPointObject(pointB).Attributes.m_uuid

        nearPoint = New On3dPoint(vectorL1.x /10 + pointB.x, vectorL1.y /10 + pointB.y, vectorL1.z /10 + pointB.z)
        insoleBottomAxis.GetClosestPoint(nearPoint, t)
        Dim pointC = insoleBottomAxis.PointAt(t)
        IdPointC = Doc.AddPointObject(pointC).Attributes.m_uuid

        Dim vectorL2 = New On3dPoint(pointP2) - New On3dPoint(pointBT) 
        nearPoint = New On3dPoint(pointBT.x + vectorL2.x /4, pointBT.y + vectorL2.y /4, pointBT.z + vectorL2.z /4)
        insoleBottomAxis.GetClosestPoint(nearPoint, t)
        Dim pointD = insoleBottomAxis.PointAt(t)
        IdPointD = Doc.AddPointObject(pointD).Attributes.m_uuid

        nearPoint = New On3dPoint(pointD.x + vectorL2.x /5, pointD.y + vectorL2.y /5, pointD.z + vectorL2.z /5)
        insoleBottomAxis.GetClosestPoint(nearPoint, t)
        Dim pointE = insoleBottomAxis.PointAt(t)
        IdPointE = Doc.AddPointObject(pointE).Attributes.m_uuid
        
        nearPoint = New On3dPoint(pointE.x + vectorL2.x * 35/100, pointE.y + vectorL2.y * 35/100, pointE.z + vectorL2.z * 35/100)
        insoleBottomAxis.GetClosestPoint(nearPoint, t)
        Dim pointF = insoleBottomAxis.PointAt(t)
        IdPointF = Doc.AddPointObject(pointF).Attributes.m_uuid       
    End Sub

    ''' <summary>
    ''' SELEZIONE PUNTI BT E CV CON IMPOSTAZIONE SNAP - VEDI procedura 4
    ''' </summary>
    Public Sub Set_BT_CV()
        MRhinoView.EnableDrawing(True)
        Doc.Redraw()

        RhUtil.RhinoApp().RunScript("_SelNone", 0)
        If RhinoLanguageSetting() = elanguage.English Then
            RhUtil.RhinoApp().RunScript("-_Osnap _E=Off _N=On _P=Off _M=Off _C=Off _I=Off _R=Off _T=Off _Q=Off _K=Off _V=Off _Enter", 0)
        ElseIf RhinoLanguageSetting() = elanguage.Italian Then
            RhUtil.RhinoApp().RunScript("-_Osnap _F=Off _V=On _P=Off _U=Off _C=Off _I=Off _E=Off _T=Off _Q=Off _N=Off _R=Off _Enter", 0)
        End If
        'MESSAGGIO UTENTE PER CREARE PUNTO BT E CV
        IdLanguageManager.PromptUserMessage("Selezionare i punti BT(battuta metatarsale) e CV(istmo del piede) e tasto Enter")
        'COMANDO
        App.RunScript("_Points", 0)
        'SALVO PUNTI BT E CV
        App.RunScript("_SelLast", 0)
        Dim pointList As New Dictionary(Of Guid, IOn3dPoint)
        Dim getObjects As New MRhinoGetObject
        getObjects.SetGeometryFilter(IRhinoGetObject.GEOMETRY_TYPE_FILTER.point_object)
        getObjects.GetObjects(0, Integer.MaxValue)
        If getObjects.ObjectCount <> 2 Then
            Throw New Exception("Numero di punti selezionati errato")
        End If
        For i = 0 To getObjects.ObjectCount - 1
            pointList.Add(getObjects.Object(i).m_uuid, getObjects.Object(i).Point.point)
        Next
        App.RunScript("_SelNone", 0)        
        IdPointCV = pointList.OrderBy(Function(keyValue) keyValue.Value.x).Select(Function(keyValue) keyValue.Key).First()
        IdPointBT = pointList.OrderBy(Function(keyValue) keyValue.Value.x).Select(Function(keyValue) keyValue.Key).Last()

        'RIMPOSTO IL PIANO DI COSTRUZIONE DEFAULT
        RhViewport.SetNewConstructionPlane(OnPlane.World_xy)
        Doc.Redraw()
        MRhinoView.EnableDrawing(False)
    End Sub


#End Region


#Region " Overrides base "

    Public Overrides Sub PulisciOggetiCostruzione()       
        PulisciOggetiCostruzioneComuni()    
         
        Doc.DeleteObject(New MRhinoObjRef(IdPointBT()))               
        Doc.DeleteObject(New MRhinoObjRef(IdPointF()))               
        Doc.DeleteObject(New MRhinoObjRef(IdPointBT1()))
        Doc.DeleteObject(New MRhinoObjRef(IdPointF1()))               
    End Sub

    Public Overrides Sub DeleteFromDocument()
        Doc.DeleteObject(New MRhinoObjRef(IdSuperficieFinale()))
    End Sub

    Public Overrides Function IsInDoc() As Boolean
        Dim obj = Doc.LookupDocumentObject(IdSuperficieFinale, True)
        Return obj IsNot Nothing
    End Function

    Protected Overrides Function GetIdInizioBordo() As Guid
        Return IdPointA1
    End Function

    Protected Overrides Function GetIdFineBordo() As Guid
        Return IdPointF1
    End Function


#End Region


#Region " IClonable "

    Public Overrides Function Clone() As Object
        Dim res As New PianoInclinatoPianta(Me.Position, Me.Spessore)
        res.IdSuperficieFinale = New Guid(IdSuperficieFinale.ToString())
        Return res
    End Function

#End Region


#Region " Serializzazione/deserializzazione"

    Public Overrides Function Serialize(ByRef archive As OnBinaryArchive) As Boolean
        If Not MyBase.CommonSerialize(archive) Then Return False

        If Not archive.WriteUuid(IdSuperficieFinale) Then Return False

        Return True
    End Function

    Public Overrides Function Deserialize(ByRef archive As OnBinaryArchive) As Boolean
        Dim uuid As New Guid
        If Not archive.ReadUuid(uuid) Then Return False
        If RhUtil.RhinoApp.ActiveDoc.LookupDocumentObject(uuid, True) IsNot Nothing Then Me.IdSuperficieFinale = New Guid(uuid.ToString)

        Return True
    End Function

#End Region


End Class
