Imports RMA.OpenNURBS
Imports RhinoUtils
Imports RMA.Rhino
Imports RhinoUtils.RhGeometry
Imports RhinoUtils.RhDebug
Imports InsoleDesigner.bll.IdElement3dManager
Imports InsoleDesigner.bll.IdLanguageManager
Imports InsoleDesigner.bll.IdAlias
Imports InsoleDesigner.bll.FactoryPianiInclinati


Public Class PianoInclinatoTotale
    Inherits AbstractPianiInclinati
    Implements IOnSerializable
    Implements ICloneable


#Region " Constructor "

    Public Sub New(position As ePosizionePianiInclinato, spessore As Double)
        MyBase.New(eTipoPianoInclinato.totale, position, spessore)
    End Sub

#End Region


#Region " PROPERTY "

    Private Property IdPointCT() As Guid
    Private Property IdPointBT() As Guid
    Private Property IdPointCT1() As Guid    
    Private Property IdPointBT1() As Guid

#End Region


#Region " Passi procedura "

    ''' <summary>
    ''' CURVE DA PUNTI DI CONTROLLO P1-E1 - VEDI procedura 7
    ''' </summary>
    ''' <remarks>Usato script perchè non riesco con la funzione RhGeometry.CreaCurvaDaCV()</remarks>
    Public Sub SetBordoInterno(side As eSide)
        Dim pointP1 = New MRhinoObjRef(IdPointP1).Point().point
        Dim pointBT = New MRhinoObjRef(IdPointC).Point().point
        Dim pointE = New MRhinoObjRef(IdPointE).Point().point
        Dim pointE1 = New MRhinoObjRef(IdPointE1).Point().point
        Dim getObjects As New MRhinoGetObject
        App.RunScript("_SelNone", 0)
        Dim scriptCommand = IIf(RhinoLanguageSetting() = elanguage.Italian, "-_Curve _G 3 C=No", "-_Curve _D 3 P=No").ToString()        
        scriptCommand = scriptCommand & " " & pointP1.x.ToString().Replace(",",".") & "," & pointP1.y.ToString().Replace(",",".") & "," & pointP1.z.ToString().Replace(",",".")
        scriptCommand = scriptCommand & " " & pointBT.x.ToString().Replace(",",".") & "," & pointBT.y.ToString().Replace(",",".") & "," & pointBT.z.ToString().Replace(",",".")
        scriptCommand = scriptCommand & " _Enter"
        App.RunScript(scriptCommand, 0)
        App.RunScript("_SelLast", 0)
        getObjects.SetGeometryFilter(IRhinoGetObject.GEOMETRY_TYPE_FILTER.curve_object)
        getObjects.GetObjects(0, Integer.MaxValue)
        If getObjects.ObjectCount <> 1 Then Throw New Exception("Impossibile creare la curca P1-BT")
        Dim curveObjRefP1_BT = New MRhinoObjRef(getObjects.Object(0))
'#If DEBUG
'        AddDocumentToDebug(curveObjRefP1_BT.Curve().DuplicateCurve(), "curveObjRefP1_BT")
'#End If
        App.RunScript("_SelNone", 0)
        scriptCommand = IIf(RhinoLanguageSetting() = elanguage.Italian, "-_Curve _G 3 C=No", "-_Curve _D 3 P=No").ToString()
        scriptCommand = scriptCommand & " " & pointBT.x.ToString().Replace(",",".") & "," & pointBT.y.ToString().Replace(",",".") & "," & pointBT.z.ToString().Replace(",",".")
        scriptCommand = scriptCommand & " " & pointE.x.ToString().Replace(",",".") & "," & pointE.y.ToString().Replace(",",".") & "," & pointE.z.ToString().Replace(",",".")
        scriptCommand = scriptCommand & " " & pointE1.x.ToString().Replace(",",".") & "," & pointE1.y.ToString().Replace(",",".") & "," & pointE1.z.ToString().Replace(",",".")
        scriptCommand = scriptCommand & " _Enter"
        App.RunScript(scriptCommand, 0)
        App.RunScript("_SelLast", 0)
        getObjects.ClearObjects()
        getObjects.SetGeometryFilter(IRhinoGetObject.GEOMETRY_TYPE_FILTER.curve_object)
        getObjects.GetObjects(0, Integer.MaxValue)
        If getObjects.ObjectCount <> 1 Then Throw New Exception("Impossibile creare la curva BT-E1")
        Dim curveObjRefBT_E1 = New MRhinoObjRef(getObjects.Object(0))
'#If DEBUG
'        AddDocumentToDebug(curveObjRefBT_E1.Curve().DuplicateCurve(), "curveObjRefBT_E1")
'#End If
        App.RunScript("_SelNone", 0)
        curveObjRefP1_BT.Object().Select(True, True)
        curveObjRefBT_E1.Object().Select(True, True)
        App.RunScript("_Join", 0)
        App.RunScript("_SelNone", 0)
        App.RunScript("_SelLast", 0)
        getObjects.ClearObjects()
        getObjects.SetGeometryFilter(IRhinoGetObject.GEOMETRY_TYPE_FILTER.curve_object)
        getObjects.GetObjects(0, Integer.MaxValue)
        If getObjects.ObjectCount <> 1 Then Throw New Exception("Impossibile unire le curve P1-C e C-E1")        
        IdBordoInternoCrv = getObjects.Object(0).m_uuid
        App.RunScript("_SelNone", 0)
        getObjects.Dispose()
        curveObjRefP1_BT.Dispose()
        curveObjRefBT_E1.Dispose()

        'Proiezione sul plantare
        ProjectBordoInterno(side)
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
        IdPointBT1 = ProjectPoint(side, insoleBottomBorder, New MRhinoObjRef(IdPointBT).Point().point).Attributes().m_uuid
        IdPointCT1 = ProjectPoint(side, insoleBottomBorder, New MRhinoObjRef(IdPointCT).Point().point).Attributes().m_uuid
        IdPointCV1 = ProjectPoint(side, insoleBottomBorder, New MRhinoObjRef(IdPointCV).Point().point).Attributes().m_uuid
    End Sub

    ''' <summary>
    ''' PUNTI A B C D E - VEDI procedura 5.1
    ''' </summary>
    Public Sub Set_A_B_C_D_E()
        Dim pointP1 = New MRhinoObjRef(IdPointP1).Point().point
        Dim pointCT = New MRhinoObjRef(IdPointCT).Point().point
        Dim pointBT = New MRhinoObjRef(IdPointBT).Point().point
        Dim pointP2 = New MRhinoObjRef(IdPointP2).Point().point
        'Dim L1 = pointP1.DistanceTo(pointCT)
        'Dim L2 = pointBT.DistanceTo(pointCV)            
        Dim t as Double
        Dim insoleBottomAxis = New MRhinoObjRef(IdInsoleBottomAxisCrv).Curve().DuplicateCurve()   

        Dim vectorL1 = New On3dPoint(pointCT) - New On3dPoint(pointP1)
        Dim nearPoint = New On3dPoint(pointP1.x + vectorL1.x *3/10, pointP1.y + vectorL1.y *3/10, pointP1.z + vectorL1.z *3/10)
        insoleBottomAxis.GetClosestPoint(nearPoint, t)
        Dim pointA = insoleBottomAxis.PointAt(t)
        IdPointA = Doc.AddPointObject(pointA).Attributes.m_uuid

        nearPoint = New On3dPoint(pointA.x + vectorL1.x / 10, pointA.y + vectorL1.y * 10, pointA.z + vectorL1.z * 10)
        insoleBottomAxis.GetClosestPoint(nearPoint, t)
        Dim pointB = insoleBottomAxis.PointAt(t)
        IdPointB = Doc.AddPointObject(pointB).Attributes.m_uuid

        Dim vectorL2 = New On3dPoint(pointP2) - New On3dPoint(pointBT)
        nearPoint = New On3dPoint(pointBT.x + vectorL2.x /4, pointBT.y + vectorL2.y /4, pointBT.z + vectorL2.z /4)
        insoleBottomAxis.GetClosestPoint(nearPoint, t)
        Dim pointC = insoleBottomAxis.PointAt(t)
        IdPointC = Doc.AddPointObject(pointC).Attributes.m_uuid

        nearPoint = New On3dPoint(pointC.x + vectorL2.x / 5, pointC.y + vectorL2.y / 5, pointC.z + vectorL2.z / 5)
        insoleBottomAxis.GetClosestPoint(nearPoint, t)
        Dim pointD = insoleBottomAxis.PointAt(t)
        IdPointD = Doc.AddPointObject(pointD).Attributes.m_uuid

        nearPoint = New On3dPoint(pointD.x + vectorL2.x * 35/100, pointD.y + vectorL2.y * 35/100, pointD.z + vectorL2.z * 35/100)
        insoleBottomAxis.GetClosestPoint(nearPoint, t)
        Dim pointE = insoleBottomAxis.PointAt(t)
        IdPointE = Doc.AddPointObject(pointE).Attributes.m_uuid
    End Sub

    ''' <summary>
    ''' SELEZIONE PUNTI BT E CT E CV CON IMPOSTAZIONE SNAP - VEDI procedura 4
    ''' </summary>
    Public Sub Set_BT_CT_CV()
        MRhinoView.EnableDrawing(True)
        Doc.Redraw()

        RhUtil.RhinoApp().RunScript("_SelNone", 0)
        If RhinoLanguageSetting() = elanguage.English Then
            RhUtil.RhinoApp().RunScript("-_Osnap _E=Off _N=On _P=Off _M=Off _C=Off _I=Off _R=Off _T=Off _Q=Off _K=Off _V=Off _Enter", 0)
        ElseIf RhinoLanguageSetting() = elanguage.Italian Then
            RhUtil.RhinoApp().RunScript("-_Osnap _F=Off _V=On _P=Off _U=Off _C=Off _I=Off _E=Off _T=Off _Q=Off _N=Off _R=Off _Enter", 0)
        End If
        'MESSAGGIO UTENTE PER CREARE PUNTO CT E CV
        IdLanguageManager.PromptUserMessage("Selezionare i punti CT(centro tallone) e CV(centro del volta) e BT(battuta metatarsale) e tasto Enter")
        'COMANDO
        App.RunScript("_Points", 0)
        'SALVO PUNTI CT E CV
        App.RunScript("_SelLast", 0)
        Dim pointList As New Dictionary(Of Guid, IOn3dPoint)
        Dim getObjects As New MRhinoGetObject
        getObjects.SetGeometryFilter(IRhinoGetObject.GEOMETRY_TYPE_FILTER.point_object)
        getObjects.GetObjects(0, Integer.MaxValue)
        If getObjects.ObjectCount <> 3 Then
            Throw New Exception("Numero di punti selezionati errato")
        End If
        For i = 0 To getObjects.ObjectCount - 1
            pointList.Add(getObjects.Object(i).m_uuid, getObjects.Object(i).Point.point)
        Next
        App.RunScript("_SelNone", 0)
        IdPointCT = pointList.OrderBy(Function(keyValue) keyValue.Value.x).Select(Function(keyValue) keyValue.Key).First()
        IdPointCV = pointList.OrderBy(Function(keyValue) keyValue.Value.x).Select(Function(keyValue) keyValue.Key).Take(2).Last()
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
        Doc.DeleteObject(New MRhinoObjRef(IdPointCT()))               
        Doc.DeleteObject(New MRhinoObjRef(IdPointBT1()))
        Doc.DeleteObject(New MRhinoObjRef(IdPointCT1()))
    End Sub

    Public Overrides Sub DeleteFromDocument()
        Doc.DeleteObject(New MRhinoObjRef(IdSuperficieFinale()))
    End Sub

    Public Overrides Function IsInDoc() As Boolean
        Dim obj = Doc.LookupDocumentObject(IdSuperficieFinale, True)
        Return obj IsNot Nothing
    End Function

    Protected Overrides Function GetIdInizioBordo() As Guid
        Return IdPointP1
    End Function

    Protected Overrides Function GetIdFineBordo() As Guid
        Return IdPointE1
    End Function


#End Region


#Region " IClonable "

    Public Overrides Function Clone() As Object
        Dim res As New PianoInclinatoTotale(Me.Position, Me.Spessore)
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
