Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Globalization
Imports System.IO
Imports RMA.OpenNURBS
Imports RMA.Rhino
Imports System.Math
Imports System.Windows.Forms
Imports InsoleDesigner.bll.IdAlias
Imports RhinoUtils
Imports RhinoUtilsino.DocObjects


Public Class IdPressureMapUtils


  Public Const PressureNoisePercentage As Double = 10 'Livello di rumore accettabile in un'analisi (i punti con pressione inferiore a questa soglia vengono tagliati)
  Public Const INITIAL_TRANSLATION_Z As Integer = -100


#Region " LETTURE FILE TESTUALI "

       ''' <summary>
    ''' Salvo il valore di pressione minimo e massimo
    ''' </summary>
    ''' <returns></returns>
    Public Shared Sub LeggiMatricePedadaDuna(fileName As String, byref minP As Double, byref maxP As Double) 
        If Not File.Exists(fileName) Then Exit Sub

        Dim columnSensors As New List(Of Int32)
        Dim rowSensors As New List(Of Int32)
        Dim pValues As New List(Of List(Of Double))
        'Prima lettura per memorizzazione parametri
        Dim provider As New NumberFormatInfo()
        provider.NumberDecimalSeparator = ","
        provider.NumberGroupSeparator = "."
        Dim flatList As New List(Of Double)
        Dim delimiters() As String = {"[", "]", ControlChars.Tab, " "}
        Using fileReader As New StreamReader(fileName)
            Do While (Not fileReader.EndOfStream)
                Dim line = fileReader.ReadLine()
                If Not line.StartsWith("[") And Not line.StartsWith("kPa") Then Continue Do

                If line.StartsWith("kPa") Then
                    Dim columns = line.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).ToList()
                    columns.RemoveAt(0)
                    columnSensors = columns.Select(Function(x) CInt(x)).ToList()
                    Continue Do
                End If

                Dim tokens = line.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).ToList()
                Dim rowNumber = CInt(tokens.Item(0))
                rowSensors.Add(rowNumber)
                tokens.RemoveAt(0)
                Dim values = tokens.Select(Function(x) Convert.ToDouble(x, provider)).ToList()
                pValues.Add(values)
                flatList.AddRange(values)
            Loop
            fileReader.Close()
        End Using
        maxP = flatList.Max()
        minP = flatList.Where(Function(p) p > 0).Min()       
    End Sub

    Public Shared Sub LeggiMatricePedadaAbbaBTS(fileName As String, byref minP As Double, byref maxP As Double) 
        If Not File.Exists(fileName) Then Exit Sub
        Dim flatList As New List(Of Double)
        Dim delimiters() As String = {ControlChars.Tab, " "}
        Using fileReader As New StreamReader(fileName)
            Do While (Not fileReader.EndOfStream)
                Dim line = fileReader.ReadLine()
                Dim tokens = line.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).ToList()
                Dim values = tokens.Select(Function(x) Convert.ToDouble(x)).ToList()
                flatList.AddRange(values)
            Loop
            fileReader.Close()
        End Using
        maxP = flatList.Max()
        minP = flatList.Where(Function(p) p > 0).Min()
    End Sub

#End Region


#Region " Gestione immagini e conversioni colore<-->pressione"

  ''' <summary>
  ''' Una volta noti i valori massimi e minimi di pressione e di colori(HSV) dal colore ricavo la pressione
  ''' </summary>
  ''' <param name="color">Colore da cui ricavare il valore di pressione</param>
  ''' <param name="rapportoGapPrecalcolato">PER OTTIMIZZAZIONE: (MaxValPressure(side) - MinValPressure(side)) / (MaxValHue(side) - MinValHue(side))</param>  
  ''' <returns></returns>
  Public Shared Function GetPressureFromColor(color As Color, rapportoGapPrecalcolato As Double, maxValHue As Double, minValPressure As Double) As Double
    Dim hueCrescenti = -(color.GetHue() - maxValHue)
    Return hueCrescenti * rapportoGapPrecalcolato + minValPressure
  End Function

  ''' <summary>
  ''' Una volta noti i valori massimi e minimi di pressione e di colori(HSV) dalla pressione ricavo il colore
  ''' </summary>
  ''' <param name="pressureVal">Pressione da cui ricavare il colore</param>
  ''' <param name="rapportoGapPrecalcolato">PER OTTIMIZZAZIONE: (MaxValPressure(side) - MinValPressure(side)) / (MaxValHue(side) - MinValHue(side))</param>
  ''' <returns></returns>
  Public Shared Function GetColorFromPressure(pressureVal As Double, rapportoGapPrecalcolato As Double, maxValHue As Double, minValPressure As Double) As Color
    ''Posso considerare H=pressureVal, S=V=1 e fare conversione HSV_to_RGB -> http://www.alvyray.com/Papers/hsv2rgb.htm
    'Dim resOnColor As OnColor = New OnColor()
    ''--------------FROM Rhino SDK Documentation--------------------
    ''Hue() returns an angle in the range 0 to 2*pi
    ''0 red,     'pi/3 = yellow,     '2*pi/3 = green,    'pi cyan,   '4 * pi / 3 = blue,                 
    ''5 * pi / 3 = magenta,  -> NON USATO    
    ''2 * pi = red)          -> NON USATO
    ''--------------------------------------------------------------      
    If pressureVal < 0.1 Then
      'Serve per il bianco e il rumore
      'resOnColor.SetHSV(1, 0, 1)
      Return Color.White
    Else      
      Dim hueCrescenteInGradi As Double = (pressureVal - minValPressure) / rapportoGapPrecalcolato
      Dim hueDecrescenteInGradi = (maxValHue - hueCrescenteInGradi)            
      'Dim hueInRadianti = (hueDecrescenteInGradi * Math.PI) / 180
      'OTTIMIZZAZIONE CALCOLI
      Dim hueInRadianti = hueDecrescenteInGradi * 0.01745329252
      Dim resOnColor As New OnColor
      resOnColor.SetHSV(hueInRadianti, 1, 1)      
      Return Color.FromArgb(resOnColor.Red, resOnColor.Green, resOnColor.Blue)               
    End If    
  End Function

  ''' <summary>
  ''' Elimina i colori non necessati dalla bitmap(assi e numeri vari )
  ''' </summary>
  ''' <param name="originalBitmap"></param>
  ''' <returns></returns>
  Public Shared Function PulisciMappaPressioneBitmap(originalBitmap As Bitmap) As Bitmap
    Dim result As New Bitmap(originalBitmap.Width, originalBitmap.Height, Imaging.PixelFormat.Format24bppRgb)    
    'For x = 0 To originalBitmap.Width - 1
    '  For y = 0 To originalBitmap.Height - 1
    '    Dim color = originalBitmap.GetPixel(x, y)
    '    Dim vicini = GetNearColors(x, y, originalBitmap)
    '    If IsValidColor(color) AndAlso vicini.All(Function(c) IsValidColor(c)) Then          
    '      result.SetPixel(x,y,color)
    '    Else
    '      result.SetPixel(x,y,System.Drawing.Color.White)          
    '    End If
    '  Next
    'Next
    ''VERSIONE OTTIMIZZATA
    Dim matrix(originalBitmap.width, originalBitmap.height) As Boolean  
    For x = 0 To originalBitmap.width - 1
      For y = 0 To originalBitmap.height - 1
       matrix(x,y) = IsValidColor(originalBitmap.GetPixel(x, y))
      Next
    Next
    For x = 0 To originalBitmap.Width - 1
      For y = 0 To originalBitmap.Height - 1        
        If matrix(x,y) AndAlso AreNearColorValid(matrix, x,y, originalBitmap.width, originalBitmap.height) Then
          result.SetPixel(x, y, originalBitmap.GetPixel(x, y))
        Else
          result.SetPixel(x, y, System.Drawing.Color.White)
        End If
      Next
    Next
'#If DEBUG Then
'    result.Save("C:\Users\Utente\Desktop\PulisciMappaPressioneBitmap.bmp", Imaging.ImageFormat.Bmp)
'#End If
    Return result
  End Function

  Private Shared Function AreNearColorValid(ByRef matrix(,) As Boolean, x As Integer, y As Integer, width As Integer, height As integer) As Boolean
    If x-1 > 0 And y-1 > 0 Then If Not matrix(x-1,y-1) Then Return False
    If y-1 > 0 Then If Not matrix(x,y-1) Then Return False
    If x+1 < width-1 And y-1 > 0 Then If Not matrix(x+1,y-1) Then Return False
    If x-1 > 0 Then If Not matrix(x-1,y) Then Return False
    If x+1 < width-1 Then If Not matrix(x+1,y) Then Return False
    If x-1 > 0 And y+1 < height-1 Then If Not matrix(x-1,y+1) Then Return False
    If y+1 < height-1 Then If Not matrix(x,y+1) Then Return False
    If x+1 < width-1 And y+1 < height-1 Then If Not matrix(x+1,y+1) Then return false
    Return true
  End Function

  Private Shared Function IsValidColor(color As System.Drawing.Color) As Boolean
    If color.GetSaturation() < 1.0 Then Return False
    If color.R = color.G And color.G = color.B Then Return False    
    If color.R < 15 And color.G < 15 And color.B < 15 Then Return False    
    Return True
  End Function

  ''' <summary>
  ''' Restituisce i colori dei pixel vicini(intorno da 3 a 7)
  ''' </summary>
  ''' <param name="x"></param>
  ''' <param name="y"></param>
  ''' <param name="image"></param>
  ''' <returns></returns>
  Private Shared Function GetNearColors(x As Integer, y As Integer, image As Bitmap) As List(Of Color)
    Dim res As New List(Of Color)
    If x-1 > 0 And y-1 > 0 Then res.Add(image.GetPixel(x-1,y-1))
    If y-1 > 0 Then res.Add(image.GetPixel(x,y-1))
    If x+1 < image.Width-1 And y-1 > 0 Then res.Add(image.GetPixel(x+1,y-1))
    If x-1 > 0 Then res.Add(image.GetPixel(x-1,y))
    If x+1 < image.Width-1 Then res.Add(image.GetPixel(x+1,y))
    If x-1 > 0 And y+1 < image.Height-1 Then res.Add(image.GetPixel(x-1,y+1))
    If y+1 < image.Height-1 Then res.Add(image.GetPixel(x,y+1))
    If x+1 < image.Width-1 And y+1 < image.Height-1 Then res.Add(image.GetPixel(x+1,y+1))
    return res
  End Function

  Private Shared Sub GetBBoxImage(image As Bitmap, byref minX As Integer, byref maxX As Integer, byref minY As Integer, byref maxY As Integer)
    Dim xCoords AS New List(Of Integer)
    Dim yCoords AS New List(Of Integer)
    For x = 0 To image.Width - 1
      For y = 0 To image.Height - 1
        Dim color = image.GetPixel(x, y)
        If Not (color.R > 250 And color.G > 250 And color.B > 250) Then
          xCoords.Add(x)
          yCoords.Add(y)
        End If
      Next
    Next
    minX = xCoords.Min()
    maxX = xCoords.Max()
    minY = yCoords.Min()
    maxY = yCoords.Max()
  End Sub

  Public Shared Function GetCenteredBitmap(originalBitmap As Bitmap) As Bitmap    
    Dim minX, maxX, minY, maxY As Integer
    GetBBoxImage(originalBitmap, minX, maxX, minY, maxY)
    'Nuova immagine con sfondo impostato
    Dim result As New Bitmap(originalBitmap.Width, originalBitmap.Height, Imaging.PixelFormat.Format24bppRgb)
    For x = 0 To originalBitmap.Width - 1
      For y = 0 To originalBitmap.Height - 1
        result.SetPixel(x, y, System.Drawing.Color.White)        
      Next
    Next
    Dim xColorate = maxX - minX
    Dim yColorate = maxY - minY
    Dim bordoX = CInt(Math.Truncate((originalBitmap.Width - xColorate) / 2))
    Dim bordoY = CInt(Math.Truncate((originalBitmap.Height - yColorate) / 2)) 
    For x = minX To maxX
      For y = minY To maxY
        Dim color = originalBitmap.GetPixel(x, y)
        result.SetPixel(x + bordoX - minX, y + bordoY - minY, color)
      Next
    Next
'#If DEBUG Then
'    result.Save(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) & "\result.bmp", Imaging.ImageFormat.Bmp)
'#End If
    Return result
  End Function

  ''' <summary>
  ''' Data la mappa di pressione di entrambi i piedi ne ricava 2 separate
  ''' </summary>
  Public Shared Function SplitPressureMapImage(originalImagePath As String, ByRef leftImage As Bitmap, ByRef rightImage As Bitmap) As Boolean
    If Not File.Exists(originalImagePath) Then Return False    
    Dim originalImage = CType(Bitmap.FromFile(originalImagePath), Bitmap)
    Dim clearedBitmap = PulisciMappaPressioneBitmap(originalImage)
'#If DEBUG Then
'    clearedBitmap.Save(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) & "\clearedBitmap.bmp", Imaging.ImageFormat.Bmp)
'#End If
    Dim newWidth As Int32 = Convert.ToInt32(Math.Floor(clearedBitmap.Width / 2) ) 
    Dim newHeight As Int32 = clearedBitmap.Height    
    leftImage = New Bitmap(newWidth, newHeight, Imaging.PixelFormat.Format24bppRgb)
    rightImage = New Bitmap(newWidth, newHeight, Imaging.PixelFormat.Format24bppRgb)
    For x = 0 To newWidth - 1
      For y = 0 To newHeight - 1
        leftImage.SetPixel(x, y, clearedBitmap.GetPixel(x,y))      
        rightImage.SetPixel(x, y, clearedBitmap.GetPixel(x+newWidth,y))         
      Next
    Next      
'#If DEBUG Then
'    leftImage.Save(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) & "\leftImage.bmp", Imaging.ImageFormat.Bmp)
'    rightImage.Save(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) & "\rightImage.bmp", Imaging.ImageFormat.Bmp)
'#End If
    return true
  End Function

  ''' <summary>
  ''' Data la mappa di pressione centrata di un piede con rapporto altezza/larghezza diverso da 1,8 elimina il bianco in eccesso e la ridimensiona
  ''' </summary>  
  Public Shared Function ResizePressureMapImage(originalImage As Bitmap) As Bitmap        
    Dim proporzione = IdPressureMap.DEFAULT_BITMAP_HEIGHT / IdPressureMap.DEFAULT_BITMAP_WIDTH
    Dim newWidth As Integer = Math.Floor(originalImage.Height / proporzione)    
    Dim minBboxX, maxBboxX, minBboxY, maxBboxY As Integer
    GetBBoxImage(originalImage, minBboxX, maxBboxX, minBboxY, maxBboxY)
    If newWidth < maxBboxX - minBboxX Then Return Nothing   
    Dim deltaWidth = originalImage.Width - newWidth
    If deltaWidth Mod 2 <> 0 Then deltaWidth += 1  
    Dim colonneDaTagliare As Integer = Convert.ToInt32(deltaWidth/2)
    Dim result As New Bitmap(newWidth, originalImage.Height, Imaging.PixelFormat.Format24bppRgb)
    For x = 0 To result.Width-1
      For y = 0 To result.Height-1
        Dim color = originalImage.GetPixel(x+colonneDaTagliare, y)
        result.SetPixel(x, y, color)
      Next
    Next
'#If DEBUG Then
'    result.Save(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) & "\resized.bmp", Imaging.ImageFormat.Bmp)
'#End If
    Return result
  End Function


#End Region


#Region " Funzioni Geometriche "


  ''' <summary>
  ''' Calcola il sistema di riferimento del piede espresso come un piano, dove
  ''' L'origine è il punto estremo in prossimità del tallone
  ''' L'asse X è l'asse del piede che dal tallone va verso le dita
  ''' L'asse Y è un asse perpendicolare ad X e giace nel piano in cui sono definiti i punti
  ''' </summary>
  ''' <param name="pointsArray"></param>
  ''' <param name="centreMetatarsalPoint"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Shared Function FootReferencePlane(ByVal pointsArray As On3dPointArray, Optional ByVal centreMetatarsalPoint As On3dPoint = Nothing) As OnPlane
    'livello di rumore dei punti, sopra il quale i punti vengono eliminati
    Dim originalPoints As On3dPointArray = IdPressureMapUtils.FilteredPoints(pointsArray, IdPressureMapUtils.MaxPressurePoint(pointsArray).z, PressureNoisePercentage)    
    Dim ProjectedPoints As On3dPointArray = IdPressureMapUtils.ProjectPoints(originalPoints)
    Dim prominentPoint As On3dPoint = CalculateRearProminentPoint(ProjectedPoints)
    '-------------------------------------------------------------------------------------

    '--- Calcolo della retta interpolante ---
    'Dim line As OnLine = FpFunctions.LineThroughtPoints(ProjectedPoints)
    Dim line As New OnLine
    RMA.Rhino.RhUtil.RhinoFitLineToPoints(ProjectedPoints, line)
    '---------------------------------------

    '--- Calcolo asse del piede ---
    Dim footAxis As On3dVector = line.Direction
    footAxis.Unitize()
    If footAxis Is Nothing Then Return Nothing
    Dim yAxisLocalFootPlane As On3dVector = OnUtil.ON_CrossProduct(OnUtil.On_zaxis, footAxis)
    '-------------------------------------------------------

    '--- Calcolo punti metatarsali ---
    Dim externalMetatarsalPoints As On3dPointArray = IdPressureMapUtils.ExternalPoints(ProjectedPoints, yAxisLocalFootPlane)
    Dim firstMetatarsal As On3dPoint = externalMetatarsalPoints(0)
    Dim fifthMetatarsal As On3dPoint = externalMetatarsalPoints(1)
    Dim mediumMetatarsalPoint As On3dPoint = (firstMetatarsal + fifthMetatarsal) / 2
    '----------------------------------

    '--- Calcolo punti sul tallone ---
    Dim externalLongitudinalPoints As On3dPointArray = IdPressureMapUtils.ExternalPoints(ProjectedPoints, footAxis)
    'viene ricalcolato il punto più prominente in un sistema orientato rispetto l'asse del piede
    'il punto sul tallone è quello più distante dai metatarsi
    If mediumMetatarsalPoint.DistanceTo(externalLongitudinalPoints(0)) >= mediumMetatarsalPoint.DistanceTo(externalLongitudinalPoints(1)) Then
      prominentPoint = externalLongitudinalPoints(0)
    Else
      prominentPoint = externalLongitudinalPoints(1)
    End If

    'L'asse del piede ottenuto dalla retta interpolante potrebbe non avere il verso settato correttamente coerentemente al peide
    'Di seguito viene scelto il verso corretto
    '-----------------------------------------------------------------------
    Dim tempFootAxis As New On3dVector(mediumMetatarsalPoint - prominentPoint)
    If OnUtil.ON_DotProduct(footAxis, tempFootAxis) < 0 Then
      footAxis.Reverse()
    End If
    '-----------------------------------------------------------------------

    Dim res As New OnPlane(prominentPoint, footAxis, OnUtil.ON_CrossProduct(OnUtil.On_zaxis, footAxis))
    Return res
  End Function

  ''' <summary>
  ''' Calcola il punto più promimente in vicinanza del tallone, considerando che l'asse del piede sia lungo l'asse x
  ''' </summary>
  ''' <param name="pointsArray"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Private Shared Function CalculateRearProminentPoint(ByVal pointsArray As On3dPointArray) As On3dPoint
    Dim res As New On3dPoint(Double.MaxValue, 0, 0)
    For i As Integer = 0 To pointsArray.Count - 1
      If pointsArray(i).x < res.x Then res = pointsArray(i)
    Next
    Return res
  End Function


  ''' <summary>
  ''' Calcola i punti estremi dell'array lungo una direzione specificata
  ''' </summary>
  ''' <param name="pointsArray"></param>
  ''' <param name="direction"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Shared Function ExternalPoints(ByVal pointsArray As On3dPointArray, ByVal direction As On3dVector) As On3dPointArray
    'si crea un sistema di riferimento locale con asse z lungo la direzione specificata dall'utente
    Dim localReferenceSystem As New OnPlane(OnUtil.On_origin, direction)

    Dim initialTransformZ As New OnXform
    initialTransformZ.Translation(0,0, INITIAL_TRANSLATION_Z)
    localReferenceSystem.Transform(initialTransformZ)

    Dim localPoints As On3dPointArray = TransformPointsWorldToLocal(pointsArray, localReferenceSystem)

    Dim minPoint As New On3dPoint(0, 0, Double.MaxValue)
    Dim maxPoint As New On3dPoint(0, 0, Double.MinValue)

    For i As Integer = 0 To localPoints.Count - 1
      If localPoints(i).z > maxPoint.z Then maxPoint = localPoints(i)
      If localPoints(i).z < minPoint.z Then minPoint = localPoints(i)
    Next

    minPoint = RhCoordinates.CoordinateLocalToWorld(localReferenceSystem, minPoint)
    maxPoint = RhCoordinates.CoordinateLocalToWorld(localReferenceSystem, maxPoint)

    Dim res As New On3dPointArray
    res.Append(minPoint)
    res.Append(maxPoint)
    Return res
  End Function

  ''' <summary>
  ''' Trasforma un on3dPointArray dal sistema di riferimento Globale al Locale
  ''' </summary>
  ''' <param name="pointsArray"></param>
  ''' <param name="refPlane"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Shared Function TransformPointsWorldToLocal(ByVal pointsArray As On3dPointArray, ByVal refPlane As OnPlane) As On3dPointArray
    Dim localPoints As New On3dPointArray
    For i As Integer = 0 To pointsArray.Count - 1
      Dim localPointTemp As New On3dPoint
      localPointTemp = RhCoordinates.CoordinateWorldToLocal(refPlane, pointsArray(i))
      Dim localPoint As New On3dPoint(localPointTemp.x, localPointTemp.y, localPointTemp.z)
      localPoints.Append(localPoint)
    Next
    Return localPoints
  End Function


  ''' <summary>
  ''' Calcola una mesh relativa alla suola del piede
  ''' </summary>
  ''' <param name="footMesh"></param>
  ''' <param name="soleHeight"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Shared Function FootSoleMesh(ByVal footMesh As IOnMesh, ByVal soleHeight As Double) As OnMesh
    Dim res = New OnMesh(footMesh)

    Dim facets As ArrayOnMeshFace = New ArrayOnMeshFace
    For i = 0 To footMesh.m_F.Count() - 1
      Dim facet As IOnMeshFace = footMesh.m_F.Item(i)
      Dim toBeAdded As Boolean = True
      For j = 0 To 3
        Dim facetVi As Integer = facet.vi(j)
        If footMesh.m_V().Item(facetVi).z < soleHeight Then 'AndAlso footMesh.m_N(i).z < 0 Then
          toBeAdded = toBeAdded And True
        Else
          toBeAdded = toBeAdded And False
        End If
      Next
      If toBeAdded Then facets.Append(facet)
    Next
    res.m_F = facets
    res.Compact()
    Return res
  End Function


  ''' <summary>
  ''' Calcola un on3dPointarray eliminando quelli con pressione inferiore al rumore
  ''' </summary>
  ''' <param name="pointsArray"></param>
  ''' <param name="PressureNoisePercentage">livello di rumore dei punti, sopra il quale i punti vengono eliminati</param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Shared Function FilteredPoints(ByVal pointsArray As On3dPointArray, ByVal maxPressure As Double, ByVal PressureNoisePercentage As Double) As On3dPointArray
    Dim res As New On3dPointArray()
    For i As Integer = 0 To pointsArray.Count - 1
      res.Append(New On3dPoint(pointsArray(i).x, pointsArray(i).y, pointsArray(i).z))
    Next

    For i As Integer = res.Count - 1 To 0 Step -1
      If (res(i).z / maxPressure) * 100 < PressureNoisePercentage Then
        res.Remove(i)
      End If
    Next
    Return res
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
  ''' Punto relativo alla massima pressione
  ''' </summary>
  ''' <param name="pointArray"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Shared Function MaxPressurePoint(ByVal pointArray As On3dPointArray) As On3dPoint
    Dim res As New On3dPoint(0, 0, 0)
    For i As Integer = 0 To pointArray.Count - 1
      If pointArray(i).z > res.z Then res = pointArray(i)
    Next
    Return res    
  End Function

  Public Shared Sub AddPressureMapMeshToDoc(side As IdElement3dManager.eSide, bitmapMesh As OnMesh)
    Dim material As OnMaterial = Element3dManager.ModelPressureMap.GetMaterial(side)
    Dim materialIndex As Integer = Doc.m_material_table.AddMaterial(material)
    Dim attributes As New On3dmObjectAttributes()
    attributes.m_material_index = materialIndex
    attributes.SetMaterialSource(IOn.object_material_source.material_from_object)  
    Dim bitmapMeshObj As MRhinoMeshObject = Doc.AddMeshObject(bitmapMesh, attributes)
    Element3dManager.ModelPressureMap.SetBitmapMeshID(side, bitmapMeshObj.Attributes().m_uuid)
    attributes.Dispose()
    'Per qualche ignoto motivo finisce sul layer sbagliato
    RhUtil.RhinoApp().RunScript("_SelNone", 0)
    bitmapMeshObj.Select(True, True)
    RhUtil.RhinoApp().RunScript("-_ChangeLayer """ & Element3dManager.GetLayerName(side, IdElement3dManager.eLayerType.pressureMap)  & """", 0)
    RhUtil.RhinoApp().RunScript("_SelNone", 0)
  End Sub


#End Region


#Region " Funzioni NON usate "


  ''' <summary>
  ''' Calcola il baricentro del piede, considerando la massa direttamente proporzionale alla pressione e quindi al valore z del punto
  ''' </summary>
  ''' <param name="pointsArray"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Shared Function CalculateFootBarycentre(ByVal pointsArray As On3dPointArray) As On3dPoint
    Dim xG As Double = 0
    Dim yG As Double = 0

    Dim mass As Double = 0
    For i As Integer = 0 To pointsArray.Count - 1
      mass += pointsArray(i).z
    Next

    For i As Integer = 0 To pointsArray.Count - 1
      xG += pointsArray(i).z * pointsArray(i).x
    Next
    xG /= mass

    For i As Integer = 0 To pointsArray.Count - 1
      yG += pointsArray(i).z * pointsArray(i).y
    Next
    yG /= mass

    Return New On3dPoint(xG, yG, 0)
  End Function

  Public Shared Function ResizeMatrix(ByVal originalMatrix()() As Double, ByRef finalMatrix()() As Double,
                                      ByRef originalRowNumber As Int32, ByRef originalColumnNumber As Int32,
                                      ByRef finalRowNumber As Int32, ByRef finalColumnNumber As Int32) As Boolean
    Dim result As Boolean = False
    Dim bufferList As New List(Of List(Of Double))

    'calcolare quante righe e quante colonne devo levare: righe 53-45=8 - colonne 53-25=28
    Dim totalRowToRemove As Int32 = originalRowNumber - finalRowNumber
    Dim totalColumnToRemove As Int32 = originalColumnNumber - finalColumnNumber
    If totalRowToRemove < 1 Or totalColumnToRemove < 1 Then Return False

    Try
      'Ridimensiono la matrice risultante, controllo che tutti i valori siano numerici e li inserisco in una lista
      ReDim finalMatrix(finalRowNumber - 1)
      For i As Int32 = 0 To originalRowNumber - 1
        If i < finalRowNumber Then ReDim finalMatrix(i)(finalColumnNumber - 1)
        bufferList.Add(New List(Of Double))
        For j As Int32 = 0 To originalColumnNumber - 1
          Dim parseResult As Int32
          If Not Integer.TryParse(originalMatrix(i)(j), parseResult) Then Return False
          bufferList.Item(i).Insert(j, parseResult)
        Next
      Next

      'salvare l'indice della riga in cui iniziano e finiscono i valori di pressione
      Dim indexOfFirstValRow As Int32 = -1
      Dim indexOfLastValRow As Int32 = -1
      For i As Int32 = 0 To originalRowNumber - 1
        For Each val As Int32 In bufferList.Item(i)
          If val > 0 Then
            If indexOfFirstValRow < 0 Then indexOfFirstValRow = i
            indexOfLastValRow = i
            Exit For
          End If
        Next
      Next

      'salvare l'indice della colonna in cui iniziano e finiscono i valori di pressione
      Dim indexOfFirstValColumn As Int32 = -1
      Dim indexOfLastValColumn As Int32 = -1
      For j As Int32 = 0 To originalColumnNumber - 1
        For i As Int32 = 0 To originalRowNumber - 1
          If bufferList.Item(i).Item(j) > 0 Then
            If indexOfFirstValColumn < 0 Then indexOfFirstValColumn = j
            indexOfLastValColumn = j
            Exit For
          End If
        Next
      Next

      'calcolare quante righe da levare in testa e in coda
      Dim startZeroRowNumber As Int32 = indexOfFirstValRow
      Dim endZeroRowNumber As Int32 = originalRowNumber - indexOfLastValRow + 1
      Dim deltaZeroRow As Int32 = startZeroRowNumber - endZeroRowNumber
      Dim startRowToRemove As Int32
      Dim endRowToRemove As Int32
      Dim residueRow As Int32 = 0
      If deltaZeroRow = 0 Then
        startRowToRemove = CInt(totalRowToRemove / 2)
        endRowToRemove = CInt(totalRowToRemove / 2)
        If (startRowToRemove + endRowToRemove) < totalRowToRemove Then endRowToRemove += 1
      ElseIf deltaZeroRow > 0 Then
        If deltaZeroRow > totalRowToRemove Then
          startRowToRemove = totalRowToRemove
          endRowToRemove = 0
        Else
          residueRow = totalRowToRemove - deltaZeroRow
          startRowToRemove = CInt(residueRow / 2)
          endRowToRemove = CInt(residueRow / 2) + deltaZeroRow
          If (startRowToRemove + endRowToRemove) < totalRowToRemove Then endRowToRemove += 1
        End If
      ElseIf deltaZeroRow < 0 Then
        If -deltaZeroRow > totalRowToRemove Then
          endRowToRemove = totalRowToRemove
          startRowToRemove = 0
        Else
          residueRow = totalRowToRemove + deltaZeroRow
          startRowToRemove = CInt(residueRow / 2)
          endRowToRemove = CInt(residueRow / 2) - deltaZeroRow
          If (startRowToRemove + endRowToRemove) < totalRowToRemove Then endRowToRemove += 1
        End If
      End If

      'Rimozione delle righe
      For i As Int32 = 0 To startRowToRemove - 1
        bufferList.RemoveAt(0)
      Next
      For j As Int32 = 0 To endRowToRemove - 1
        bufferList.RemoveAt(bufferList.Count - 1)
      Next

      'calcolare quante colonne da levare in testa e in coda
      Dim startZeroColumnNumber As Int32 = indexOfFirstValColumn
      Dim endZeroColumnNumber As Int32 = originalColumnNumber - indexOfLastValColumn + 1
      Dim deltaZeroColumn As Int32 = startZeroColumnNumber - endZeroColumnNumber
      Dim startColumnToRemove As Int32
      Dim endColumnToRemove As Int32
      Dim residueColumn As Int32 = 0
      If deltaZeroColumn = 0 Then
        startColumnToRemove = CInt(totalColumnToRemove / 2)
        endColumnToRemove = CInt(totalColumnToRemove / 2)
        If (startColumnToRemove + endColumnToRemove) < totalColumnToRemove Then endColumnToRemove += 1
      ElseIf deltaZeroColumn > 0 Then
        If deltaZeroColumn > totalColumnToRemove Then
          startColumnToRemove = totalColumnToRemove
          endColumnToRemove = 0
        Else
          residueColumn = totalColumnToRemove - deltaZeroColumn
          startColumnToRemove = CInt(residueColumn / 2)
          endColumnToRemove = CInt(residueColumn / 2) + deltaZeroColumn
          If (startColumnToRemove + endColumnToRemove) < totalColumnToRemove Then endColumnToRemove += 1
        End If
      ElseIf deltaZeroColumn < 0 Then
        If -deltaZeroColumn > totalColumnToRemove Then
          endColumnToRemove = totalColumnToRemove
          startColumnToRemove = 0
        Else
          residueColumn = totalColumnToRemove + deltaZeroColumn
          startColumnToRemove = CInt(residueColumn / 2)
          endColumnToRemove = CInt(residueColumn / 2) - deltaZeroColumn
          If (startColumnToRemove + endColumnToRemove) < totalColumnToRemove Then endColumnToRemove += 1
        End If
      End If

      'Rimozione delle colonne
      For Each item As List(Of Double) In bufferList
        For i As Int32 = 0 To startColumnToRemove - 1
          item.RemoveAt(0)
        Next
        For j As Int32 = 0 To endColumnToRemove - 1
          item.RemoveAt(item.Count - 1)
        Next
      Next

      'Metto risultato nell'array
      Dim rowIndex As Int32 = 0
      For Each item As List(Of Double) In bufferList
        finalMatrix(rowIndex) = item.ToArray()
        rowIndex += 1
      Next

      result = True

    Catch ex As Exception
      Return False
    End Try

    Return result
  End Function

  ''' <summary>
  ''' Trasforma un on3dPointArray dal sistema di riferimento Locale al Globale 
  ''' </summary>
  ''' <param name="pointsArray"></param>
  ''' <param name="refPlane"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Shared Function TransformPointsLocalToWorld(ByVal pointsArray As On3dPointArray, ByVal refPlane As OnPlane) As On3dPointArray
    Dim localPoints As New On3dPointArray
    For i As Integer = 0 To pointsArray.Count - 1
      Dim localPointTemp As New On3dPoint
      localPointTemp = RhCoordinates.CoordinateLocalToWorld(refPlane, pointsArray(i))
      Dim localPoint As New On3dPoint(localPointTemp.x, localPointTemp.y, localPointTemp.z)
      localPoints.Append(localPoint)
    Next
    Return localPoints
  End Function

  ''' <summary>
  ''' Calcola una mesh a partire ad un array di punti
  ''' </summary>
  ''' <param name="pointsArray"></param>
  ''' <param name="samplingDensityPlusNoise"></param>
  ''' <param name="doc"></param>
  ''' <param name="removeOriginalPoints"></param>
  ''' <param name="removeMesh"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Shared Function MeshFromPoints(ByVal pointsArray As On3dPointArray, ByVal samplingDensityPlusNoise As Double, ByVal doc As MRhinoDoc, Optional ByVal removeOriginalPoints As Boolean = True, Optional ByRef removeMesh As Boolean = True) As OnMesh
    RhUtil.RhinoApp.RunScript("_SelNone", 0)
    Dim pointCloudRhino As MRhinoPointCloudObject = doc.AddPointCloudObject(pointsArray)
    pointCloudRhino.Select(True)
    Dim valueString As String = samplingDensityPlusNoise.ToString.Replace(",", ".")
    RhUtil.RhinoApp.RunScript("_MeshFromPoints _SamplingDensityPlusNoise=" & valueString & " _AutoAdjustGrid=Yes _NunOfCountorningGridCell=100 _enter", 0)
    If removeOriginalPoints Then doc.DeleteObject(New MRhinoObjRef(pointCloudRhino.Attributes.m_uuid), True)
    RhUtil.RhinoApp.RunScript("_SelNone _Enter", 0)
    RhUtil.RhinoApp.RunScript("_SelLast _Enter", 0)
    Dim getObj As New MRhinoGetObject
    getObj.GetObjects(0, 1)
    Dim resultMesh As IOnMesh = getObj.Object(0).Mesh
    Dim res As New OnMesh(resultMesh)
    If removeMesh Then doc.DeleteObject(getObj.Object(0))
    Return res
  End Function

  Public Shared Function InterpolatePressure(x As Integer, y As Integer, scaledXY As List(Of List(Of Double))) As Double
    Dim sum As Double = scaledXY.Item(x).Item(y)
    Dim count As Integer = 1    
    If x - 1 >= 0 And y - 1 >= 0 Then
      sum += scaledXY.Item(x - 1).Item(y - 1)
      count += 1
    End If
    If y - 1 >= 0 Then
      sum += scaledXY.Item(x).Item(y - 1)
      count += 1
    End If
    If x + 1 < scaledXY.Count And y - 1 >= 0 Then
      sum += scaledXY.Item(x + 1).Item(y - 1)
      count += 1
    End If
    If x - 1 >= 0 Then
      sum += scaledXY.Item(x - 1).Item(y)
      count += 1
    End If          
    If x + 1 < scaledXY.Count Then
      sum += scaledXY.Item(x + 1).Item(y)
      count += 1
    End If
    If x - 1 >= 0 And y + 1 < scaledXY.Item(0).Count Then
      sum += scaledXY.Item(x - 1).Item(y + 1)
      count += 1
    End If
    If y + 1 < scaledXY.Item(0).Count Then
      sum += scaledXY.Item(x).Item(y + 1)
      count += 1
    End If
    If x + 1 < scaledXY.Count And y + 1 < scaledXY.Item(0).Count Then
      sum += scaledXY.Item(x + 1).Item(y + 1)
      count += 1
    End If
    Return sum / count
  End Function
  
  Public Shared Function ConvertGrayscaleToColor(ByVal value As Integer) As Color
    If value >= 0 And value <= 255 Then
      If value = 255 Then
        Return Color.White
      Else
        Dim resOnColor As OnColor = New OnColor
        Dim hue As Double = value * (240 / 255)
        resOnColor.SetHSV(hue * (Math.PI / 180), 1, 1)
        Return Color.FromArgb(resOnColor.Red, resOnColor.Green, resOnColor.Blue)
      End If
    Else
      Return Nothing
    End If
  End Function


  Public Shared Function ConvertColorToGrayscale(ByVal color As Color) As Integer
    Dim hue As Double = color.GetHue()
    'Return CInt((hue / 240) * 255) ''OTTIMIZZO
    Return CInt(hue * 1.0625)
  End Function

  Public Shared Function ConvertGrayScaleToPressure(ByVal grayScaleValue As Int32, ByVal minVal As Double, ByVal maxVal As Double) As Double
    Return maxVal - grayScaleValue * ((maxVal - minVal) / 255)
  End Function

   Public Shared Function ConvertPressureToGrayScale(ByVal pressureVal As Double, ByVal minVal As Double, ByVal maxVal As Double) As Int32
    Return CInt(-(255 / (maxVal - minVal)) * (pressureVal - maxVal))
  End Function



#End Region


End Class
