Imports System.Collections.Specialized
Imports System.Diagnostics.Eventing.Reader
Imports System.IO
Imports System.Linq
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Globalization
Imports RMA.OpenNURBS
Imports RMA.Rhino
Imports InsoleDesigner.bll.IdAlias
Imports InsoleDesigner.bll.IdElement3dManager
Imports InsoleDesigner.bll.IdPressureMapUtils
Imports System.IO.FileInfo


Public Class IdPressureMap        
    Implements ICloneable, IOnSerializableSide


    Private mMinPressureLeft As Double = 0
    Private mMaxPressureLeft As Double = 255
    Private mMinPressureRight As Double = 0
    Private mMaxPressureRight As Double = 255

    Private mMinHueLeft As Double = 360
    Private mMaxHueLeft As Double = 0
    Private mMinHueRight As Double = 360
    Private mMaxHueRight As Double = 0

    'formato intermedio comune a tutti i tipi di caricamento
    Private mNumericMapRight As On3dPointArray
    Private mNumericMapLeft As On3dPointArray

    'sono le mesh costruite a partire dalla mappa di pressione
    Private mBitmapMeshRightID As Guid
    Private mBitmapMeshLeftID As Guid

    Private mMaterialRight As OnMaterial
    Private mMaterialLeft As OnMaterial

    Private mTextureFileNameLeft As String
    Private mTextureFileNameRight As String

    Private mProjectionMeshRightID As Guid
    Private mProjectionMeshLeftID As Guid

    Private mDeformInsoleTables As List(Of DeformationTable)

    Private mSelectedDeformInsoleTableRight As String
    Private mSelectedDeformInsoleTableLeft As String

    'Private Const MinimumPercentageNoiseLevel As Int32 = 7

    'PER CONVENZIONE LA BITMAP INIZIALE E' DISPOSTA IN VERTICALE
    Public Const DEFAULT_BITMAP_WIDTH As Integer = 200
    Public Const DEFAULT_BITMAP_HEIGHT As Integer = 360

    Public Const MIN_VALID_SATURATION As Double = 0.5
    Public Const MIN_VALID_PRESSURE As Double = 0.01

    'Dimensione del piexel della bitmap, ottenuto empiricamente confrontando la scansione del piede per Duna ok 0.92
    Public Property PixelDimension As Double = 1 '0.92


    Public Sub New()
        mNumericMapRight = New On3dPointArray(0)
        mNumericMapLeft = New On3dPointArray(0)
        mMaterialRight = New OnMaterial
        mMaterialLeft = New OnMaterial
        mDeformInsoleTables = New List(Of DeformationTable)
    End Sub


#Region " Get/Set "

    Public Property MaxValPressure(side As IdElement3dManager.eSide) As Double
        Get
            If side = IdElement3dManager.eSide.left Then
                Return mMaxPressureLeft
            Else
                Return mMaxPressureRight
            End If
        End Get
        Set
            If side = IdElement3dManager.eSide.left Then
                mMaxPressureLeft = Value
            Else
                mMaxPressureRight = Value
            End If
        End Set
    End Property

    Public Property MinValPressure(side As IdElement3dManager.eSide) As Double
        Get
            If side = IdElement3dManager.eSide.left Then
                Return mMinPressureLeft
            Else
                Return mMinPressureRight
            End If
        End Get
        Set
            If side = IdElement3dManager.eSide.left Then
                mMinPressureLeft = Value
            Else
                mMinPressureRight = Value
            End If
        End Set
    End Property

    Public Property MaxValHue(side As IdElement3dManager.eSide) As Double
        Get
            If side = IdElement3dManager.eSide.left Then
                Return mMaxHueLeft
            Else
                Return mMaxHueRight
            End If
        End Get
        Set(value As Double)
            If side = IdElement3dManager.eSide.left Then
                mMaxHueLeft = value
            Else
                mMaxHueRight = value
            End If
        End Set
    End Property

    Public Property MinValHue(side As IdElement3dManager.eSide) As Double
        Get
            If side = IdElement3dManager.eSide.left Then
                Return mMinHueLeft
            Else
                Return mMinHueRight
            End If
        End Get
        Set(value As Double)
            If side = IdElement3dManager.eSide.left Then
                mMinHueLeft = value
            Else
                mMinHueRight = value
            End If
        End Set
    End Property

    Public Function GetBitmap(ByVal side As IdElement3dManager.eSide) As Bitmap
        'If side = IdElement3dManager.eSide.left Then
        '  Return mBitmapLeft
        'Else
        '  Return mBitmapRight
        'End If
        ''PROVO A PRENDERE DAL MATERIALE
        'Dim materiale = GetMaterial(side)
        'Return CType(Bitmap.FromFile(materiale.m_textures.Item(0).m_filename), Bitmap)
        'RICREO DA NumericMap PER NON DOVER SERIALIZZARE LA BITMAP CHE DA PROBLEMI IN SALVATAGGIO DOCUMENTO
        Return CreateBitmapFromNumericMap(side)
    End Function

    Public Function GetNumericMap(side As IdElement3dManager.eSide) As On3dPointArray
        If side = IdElement3dManager.eSide.left Then
            Return mNumericMapLeft
        Else
            Return mNumericMapRight
        End If
    End Function

    Public Function GetNumericMapPressurePoints(side As IdElement3dManager.eSide) As List(Of On3dPoint)
        Return Element3dManager.ModelPressureMap.GetNumericMap(side).Cast(Of On3dPoint)().Where(Function(p) p.z > MIN_VALID_PRESSURE).ToList()
    End Function

    Private Function GetBitmapMeshId(side As eSide) As Guid
        Return IIf(side = eSide.left, mBitmapMeshLeftID, mBitmapMeshRightID)
    End Function

    Public Function GetBitmapMeshObj(ByVal side As IdElement3dManager.eSide) As MRhinoObject
        If side = IdElement3dManager.eSide.left Then
            Return RhUtil.RhinoApp.ActiveDoc.LookupObject(mBitmapMeshLeftID)
        Else
            Return RhUtil.RhinoApp.ActiveDoc.LookupObject(mBitmapMeshRightID)
        End If
    End Function

    Public Function GetBitmapMeshObjRef(side As IdElement3dManager.eSide) As MRhinoObjRef
        If side = IdElement3dManager.eSide.left Then
            Return New MRhinoObjRef(mBitmapMeshLeftID)
        Else
            Return New MRhinoObjRef(mBitmapMeshRightID)
        End If
    End Function

    Public Function GetMaterial(ByVal side As IdElement3dManager.eSide) As OnMaterial
        If side = IdElement3dManager.eSide.left Then
            Return mMaterialLeft
        Else
            Return mMaterialRight
        End If
    End Function

    Private Property TextureOriginalFileName(ByVal side As IdElement3dManager.eSide) As String
        Get
            Return IIf(side = eSide.left, mTextureFileNameLeft, mTextureFileNameRight)
        End Get
        Set(value As String)
            If side = IdElement3dManager.eSide.left Then
            mTextureFileNameLeft = value
        Else
            mTextureFileNameRight = value
        End If
        End Set
    End Property

    Public Function GetProjectionMeshObj(ByVal side As IdElement3dManager.eSide) As MRhinoObject
        If side = IdElement3dManager.eSide.left Then
            Return RhUtil.RhinoApp.ActiveDoc.LookupObject(mProjectionMeshLeftID)
        Else
            Return RhUtil.RhinoApp.ActiveDoc.LookupObject(mProjectionMeshRightID)
        End If
    End Function

    Public Function GetProjectionMeshObjRef(ByVal side As IdElement3dManager.eSide) As MRhinoObjRef
        If side = IdElement3dManager.eSide.left Then
            Return New MRhinoObjRef(mProjectionMeshLeftID)
        Else
            Return New MRhinoObjRef(mProjectionMeshRightID)
        End If
    End Function

    Private Function GetProjectionMeshId(side As eSide) As Guid
        Return IIf(side = eSide.left, mProjectionMeshLeftID, mProjectionMeshRightID)
    End Function

    Private Sub SetNumericMap(ByVal side As IdElement3dManager.eSide, points As On3dPointArray)
        Dim target As On3dPointArray = IIf(side = IdElement3dManager.eSide.left, mNumericMapLeft, mNumericMapRight)
        target.Destroy()
        For Each point As On3dPoint In points
            target.Append(New On3dPoint(point))
        Next
    End Sub

    Public Sub UpdateNumericMapPointXY(side As IdElement3dManager.eSide, point As IOn3dPoint, index As Integer)
        Dim target As On3dPointArray = IIf(side = IdElement3dManager.eSide.left, mNumericMapLeft, mNumericMapRight)
        Dim pressure = target.Item(index).z
        'target.Item(index) = New On3dPoint(point.x, point.y, pressure)
        target.Item(index).x = point.x
        target.Item(index).y = point.y
    End Sub

    ''' <summary>
    ''' ATTENZIONE: differenza dei metodi Fill* non fa il clear della lista
    ''' </summary>
    Public Sub SetBitmapMeshID(ByVal side As IdElement3dManager.eSide, ByVal bitmapMeshID As Guid)
        If side = IdElement3dManager.eSide.left Then
            mBitmapMeshLeftID = bitmapMeshID
        Else
            mBitmapMeshRightID = bitmapMeshID
        End If
    End Sub

    ''' <summary>
    ''' ATTENZIONE: differenza dei metodi Fill* non fa il clear della lista
    ''' </summary>
    Public Sub SetMaterial(ByVal side As IdElement3dManager.eSide, ByVal material As OnMaterial)
        If side = IdElement3dManager.eSide.left Then
            mMaterialLeft = material
        Else
            mMaterialRight = material
        End If
    End Sub


    Public Sub SetProjectionMeshID(ByVal side As IdElement3dManager.eSide, ByVal id As Guid)
        If side = IdElement3dManager.eSide.left Then
            mProjectionMeshLeftID = id
        Else
            mProjectionMeshRightID = id
        End If
    End Sub


#End Region


#Region " ESISTENZA OGGETTI "


    Public Function NumericMapExist() As Boolean
        Return NumericMapExist(IdElement3dManager.eSide.left) Or NumericMapExist(IdElement3dManager.eSide.right)
    End Function

    Public Function NumericMapExist(side As IdElement3dManager.eSide) As Boolean
        If side = IdElement3dManager.eSide.left Then
            Return Not IsNothing(mNumericMapLeft) AndAlso mNumericMapLeft.Count() > 0
        Else
            Return Not IsNothing(mNumericMapRight) AndAlso mNumericMapRight.Count() > 0
        End If
    End Function

    Public Function BitmapMeshExist() As Boolean
        Return BitmapMeshExist(IdElement3dManager.eSide.left) Or BitmapMeshExist(IdElement3dManager.eSide.right)
    End Function

    Public Function BitmapMeshExist(side As IdElement3dManager.eSide) As Boolean
        If side = IdElement3dManager.eSide.left Then
            Return Not IsNothing(RhUtil.RhinoApp.ActiveDoc.LookupObject(mBitmapMeshLeftID))
        Else
            Return Not IsNothing(RhUtil.RhinoApp.ActiveDoc.LookupObject(mBitmapMeshRightID))
        End If
    End Function

    Public Function ProjectionMeshExist() As Boolean
        Return ProjectionMeshExist(IdElement3dManager.eSide.left) Or ProjectionMeshExist(IdElement3dManager.eSide.right)
    End Function

    Public Function ProjectionMeshExist(side As IdElement3dManager.eSide) As Boolean
        If side = IdElement3dManager.eSide.left Then
            Return Not IsNothing(RhUtil.RhinoApp.ActiveDoc.LookupObject(mProjectionMeshLeftID))
        Else
            Return Not IsNothing(RhUtil.RhinoApp.ActiveDoc.LookupObject(mProjectionMeshRightID))
        End If
    End Function


#End Region


#Region " DELETE "


    Public Sub DeleteAll()
        DeleteAll(IdElement3dManager.eSide.left)
        DeleteAll(IdElement3dManager.eSide.right)
    End Sub

    Public Sub DeleteAll(side As IdElement3dManager.eSide)
        DeleteBitmapMesh(side)
        DeleteProjectionMesh(side)
        SetNumericMap(side, New On3dPointArray())
        SetMaterial(side, New OnMaterial())
    End Sub

    Public Sub DeleteBitmapMesh(side As IdElement3dManager.eSide)
        If Not BitmapMeshExist(side) Then Exit Sub
        Dim objRef = GetBitmapMeshObjRef(side)
        Doc.DeleteObject(objRef)
        objRef.Dispose()
        SetBitmapMeshID(side, New Guid())
    End Sub

    Public Sub DeleteProjectionMesh(side As IdElement3dManager.eSide)
        If Not ProjectionMeshExist(side) Then Exit Sub
        Dim objRef = GetProjectionMeshObjRef(side)
        Doc.DeleteObject(objRef)
        objRef.Dispose()
        SetProjectionMeshID(side, New Guid())
    End Sub

#End Region


#Region " TABELLE DEFORMAZIONE PLANTARE "

    Public ReadOnly Property GetDeformInsoleTables() As List(Of DeformationTable)
        Get
            Return mDeformInsoleTables
        End Get
    End Property

    Public ReadOnly Property GetSelectedDeformInsoleTable(side As IdElement3dManager.eSide) As DeformationTable
        Get
            If side = IdElement3dManager.eSide.left Then
                If String.IsNullOrEmpty(mSelectedDeformInsoleTableLeft) Then Return Nothing
                Return mDeformInsoleTables.FirstOrDefault(Function(x) x.Name = mSelectedDeformInsoleTableLeft)
            Else
                If String.IsNullOrEmpty(mSelectedDeformInsoleTableRight) Then Return Nothing
                Return mDeformInsoleTables.FirstOrDefault(Function(x) x.Name = mSelectedDeformInsoleTableRight)
            End If
        End Get
    End Property

    Public Function LoadDeformTables() As Boolean
        mDeformInsoleTables.Clear()
        'LEGGO FILES
        For Each fileInfo In LibraryManager.GetDeformTableFiles()
            Dim lines As New List(Of String)
            Try
                Using reader As New StreamReader(fileInfo.FullName)
                    While Not reader.EndOfStream
                        lines.Add(reader.ReadLine())
                    End While
                    reader.Close()
                End Using
            Catch ex As Exception
                MsgBox("Impossibile leggere il file '" & fileInfo.FullName & "' controllare che non sia aperto e riprovare")
            End Try
            If Not lines.Any() Then Continue For
            'Creo le tabelle deformazione      
            Dim deformTable As New DeformationTable
            deformTable.Name = fileInfo.Name.Replace(fileInfo.Extension, "")
            For Each line In lines
                If line.StartsWith("#") Then Continue For
                Dim items = line.Split(New String() {";"}, StringSplitOptions.None)
                If items.Length <> 3 Then Exit For
                Dim minP, maxP, deform As Double
                If Not Double.TryParse(items(0), minP) Then Exit For
                If Not Double.TryParse(items(1), maxP) Then Exit For
                If Not Double.TryParse(items(2), deform) Then Exit For
                deformTable.AddInterval(minP, maxP, deform)
            Next
            mDeformInsoleTables.Add(deformTable)
        Next
        Return mDeformInsoleTables.Any()
    End Function

    Public Sub SetSelectedDeformInsoleTable(side As IdElement3dManager.eSide, name As String)
        If side = IdElement3dManager.eSide.left Then
            mSelectedDeformInsoleTableLeft = name
        Else
            mSelectedDeformInsoleTableRight = name
        End If
    End Sub

#End Region


#Region " Funzioni di conversione oggetti del Model "


    ''' <summary>
    ''' Crea la matrice numerica in base all'immagine e ai valori massimo e minimo di pressione
    ''' </summary>
    ''' <param name="side"></param>
    ''' <param name="tempBitmap"></param>
    Public Sub FillNumericMapFromBitmap(side As IdElement3dManager.eSide, tempBitmap As Bitmap)
        '#If DEBUG
        '    tempBitmap.Save("C:\Users\Utente\Desktop\FillNumericMapFromBitmap.bmp", Imaging.ImageFormat.Bmp)
        '#End If
        Dim scaledBitmapWidth As Integer = DEFAULT_BITMAP_WIDTH
        Dim scaledBitmapHeight As Integer = DEFAULT_BITMAP_HEIGHT
        'Dim cleanBitmap, targetBitmap As Bitmap
        ''Pulisco dalla bitmap il bianco "sporco" presente al contorno
        'cleanBitmap = RhImageAnalysis.CleanDirtyWhite(tempBitmap, 99)  
        'targetBitmap = New Bitmap(cleanBitmap, scaledBitmapWidth, scaledBitmapHeight)
        Dim targetBitmap As Bitmap = New Bitmap(tempBitmap, scaledBitmapWidth, scaledBitmapHeight)
        '#If DEBUG
        '    targetBitmap.Save("C:\Users\Utente\Desktop\preFillNumericMapFromBitmap.bmp", Imaging.ImageFormat.Bmp)
        '#End If
        'NUOVA VERSIONE         
        For i As Int32 = 0 To scaledBitmapWidth - 1
            For j As Int32 = 0 To scaledBitmapHeight - 1
                Dim pixel = targetBitmap.GetPixel(i, j)
                If (pixel.GetSaturation >= MIN_VALID_SATURATION) Then
                    Dim hue = pixel.GetHue()
                    If hue < MinValHue(side) Then MinValHue(side) = hue
                    If hue > MaxValHue(side) Then MaxValHue(side) = hue
                End If
            Next
        Next
        Dim rapportoGapPrecalcolato = (MaxValPressure(side) - MinValPressure(side)) / (MaxValHue(side) - MinValHue(side))

        'Scorro pixel bitmap
        Dim pressureValue As Double
        Dim on3dPointArray As New On3dPointArray
        Dim bitmapColor As Color
        Dim scostamentoX = DEFAULT_BITMAP_WIDTH - DEFAULT_BITMAP_WIDTH * PixelDimension
        For i As Int32 = 0 To scaledBitmapWidth - 1
            For j As Int32 = 0 To scaledBitmapHeight - 1
                bitmapColor = targetBitmap.GetPixel(i, j)
                'Non va considerato GetHue() quando è nel bianco
                If (bitmapColor.GetSaturation < MIN_VALID_SATURATION) Then
                    pressureValue = 0
                Else
                    pressureValue = GetPressureFromColor(bitmapColor, rapportoGapPrecalcolato, MaxValHue(side), MinValPressure(side))
                End If
                'on3dPointArray.Append(i, j, pressureValue)        
                on3dPointArray.Append(i * PixelDimension + scostamentoX, j * PixelDimension, pressureValue)
            Next
        Next
        on3dPointArray.Rotate(Math.PI, OnUtil.On_yaxis, New On3dPoint(scaledBitmapWidth / 2, 0, 0))
        For i As Integer = 0 To on3dPointArray.Count - 1
            on3dPointArray(i).z = Math.Abs(on3dPointArray(i).z)
        Next
        '    'BITMAP SCALATA
        '    If side = IdElement3dManager.eSide.left Then
        '      mBitmapLeft = targetBitmap
        '      mNumericMapLeft = on3dPointArray
        '    Else
        '      mBitmapRight = targetBitmap
        '      mNumericMapRight = on3dPointArray
        '    End If    
        '#If DEBUG Then
        '    targetBitmap.Save("C:\Users\Utente\Desktop\targetBitmap.bmp", Imaging.ImageFormat.Bmp)
        '#End If
        SetNumericMap(side, on3dPointArray)
    End Sub


    ''' <summary>
    ''' Funzione che genera una mesh rettangolare le cui dimensioni sono le stesse della mappa di pressione, con un vertice all'origine.
    ''' L'orientamento è coerente con la nuvola di punti relativa alle pressioni
    ''' Sulla mesh viene applicata una texture.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CreatePressureMesh(ByVal side As IdElement3dManager.eSide) As OnMesh
        Dim mesh As New OnMesh()
        Dim presureMap As Bitmap = Me.GetBitmap(side)
        '#If DEBUG Then
        '    presureMap.Save("C:\Users\Utente\Desktop\CreatePressureMesh.bmp", Imaging.ImageFormat.Bmp)
        '#End If
        mesh.m_V.Append(presureMap.Width * PixelDimension, presureMap.Height * PixelDimension, 0)
        mesh.m_V.Append(0, presureMap.Height * PixelDimension, 0)
        mesh.m_V.Append(0, 0, 0)
        mesh.m_V.Append(presureMap.Width * PixelDimension, 0, 0)
        mesh.m_F.AppendQuad(0, 1, 2, 3)

        If presureMap.Height > presureMap.Width Then
            mesh.m_T.Append(0, 0)
            mesh.m_T.Append(1, 0)
            mesh.m_T.Append(1, 1)
            mesh.m_T.Append(0, 1)
        Else
            mesh.m_T.Append(1, 0)
            mesh.m_T.Append(1, 1)
            mesh.m_T.Append(0, 1)
            mesh.m_T.Append(0, 0)
        End If

        'Materiale
        Dim material As New OnMaterial()
        material.m_material_name = "pressure map " & side.ToString()
        material.m_diffuse.SetRGB(255, 255, 255)  '<-- Dà luminosità all'immagine
        material.m_transparency = 0.5 '0.1        '<-- Leggera trasparenza migliora resa immagine (aumentata x aiutare allineamento manuale)
        material.m_shine = 0.0                    '<-- Proibisce le riflessioni della texture
        'Dim textureIndex As Integer = material.AddTexture(bitmapFileName, IOnTexture.TYPE.bitmap_texture)
        ''PIU' CORRETTO SALVARE IN DIRECTORY TEMP LA BITMAP CREATA E USARE QUELLA INVECE DI QUELLA ORIGINALE
        Dim tempFile = Path.GetTempFileName()
        presureMap.Save(tempFile, ImageFormat.Bmp)
        Dim textureIndex As Integer = material.AddTexture(tempFile, IOnTexture.TYPE.bitmap_texture)
        SetMaterial(side, material)

        Return mesh
    End Function

    Private Function CreateBitmapFromNumericMap(ByVal side As IdElement3dManager.eSide) As Bitmap
        Dim numericMap As On3dPointArray = GetNumericMap(side)
        ''LE DIMENSIONI 200X360 SONO STATE FISSATE E NON DEVONO ESSERE MODIFICATE PERCHE' RELATIVE ALL'On3dPointArray  
        Dim resBitmap As New Bitmap(DEFAULT_BITMAP_WIDTH, DEFAULT_BITMAP_HEIGHT, Imaging.PixelFormat.Format24bppRgb)
        '#If DEBUG Then
        '    resBitmap.Save("C:\Users\Utente\Desktop\CreateBitmapFromNumericMap1.bmp", Imaging.ImageFormat.Bmp)
        '#End If
        Dim rapportoGapPrecalcolato = (MaxValPressure(side) - MinValPressure(side)) / (MaxValHue(side) - MinValHue(side))
        Dim on3dPoint As On3dPoint
        For i = 0 To resBitmap.Width - 1
            For j = 0 To resBitmap.Height - 1
                on3dPoint = numericMap.Item(i * (resBitmap.Height) + j)
                Dim color = GetColorFromPressure(on3dPoint.z, rapportoGapPrecalcolato, MaxValHue(side), MinValPressure(side))
                resBitmap.SetPixel(i, j, color)
            Next
        Next
        '#If DEBUG Then
        '    resBitmap.Save("C:\Users\Utente\Desktop\CreateBitmapFromNumericMap2.bmp", Imaging.ImageFormat.Bmp)
        '#End If
        Return resBitmap
    End Function

    Private Sub UpdatePressureMapMesh(ByRef material As OnMaterial, filename As String, side As eSide)
        Dim texture = material.m_textures.Item(0)
        texture.m_filename = filename
        Dim bitmapMeshObjRef = GetBitmapMeshObjRef(side)
        If Doc.LookupDocumentObject(bitmapMeshObjRef.m_uuid, False) Is Nothing Then Exit Sub        
        Dim materialIndex As Integer = Doc.m_material_table.AddMaterial(material)
        Dim attributes As New MRhinoObjectAttributes(bitmapMeshObjRef.Object().Attributes)
        attributes.m_material_index = materialIndex
        attributes.SetMaterialSource(IOn.object_material_source.material_from_object)
        RhUtil.RhinoApp.ActiveDoc.ModifyObjectAttributes(bitmapMeshObjRef, attributes)
        bitmapMeshObjRef.Dispose()
    End Sub

#End Region


#Region " Serializzazione/deserializzazione"


    Public Function Serialize(ByRef archive As OnBinaryArchive, side As eSide) As Boolean Implements IOnSerializableSide.Serialize
        'double
        If Not archive.WriteDouble(MinValPressure(side)) Then Return False
        If Not archive.WriteDouble(MaxValPressure(side)) Then Return False
        If Not archive.WriteDouble(MinValHue(side)) Then Return False
        If Not archive.WriteDouble(MaxValHue(side)) Then Return False

        'UUID
        If Not archive.WriteUuid(GetBitmapMeshId(side)) Then Return False        
        If Not archive.WriteUuid(GetProjectionMeshId(side)) Then Return False

        'OnMaterial 
        Dim material = GetMaterial(side)
        If Not archive.WriteObject(material) Then Return False 
        Dim textureExist = material.m_textures.Count() > 0   
        If Not archive.WriteBool(textureExist) Then Return False       
        If textureExist  
            Dim texture = material.m_textures.Item(0)
            Dim bitmap = Drawing.Bitmap.FromFile(texture.m_filename)                       
            Dim converter As New ImageConverter
            Dim buffer() As Byte = converter.ConvertTo(bitmap, GetType(Byte()))
            If Not archive.WriteCompressedBuffer(buffer) Then Return False                                
        End If                    

        'On3dPointArray
        If Not archive.WriteInt(GetNumericMap(side).Count()) Then Return False
        For Each p As On3dPoint In GetNumericMap(side)
            archive.WritePoint(p)
        Next

        Return True
    End Function  


    Public Function Deserialize(ByRef archive As OnBinaryArchive, side As eSide) As Boolean Implements IOnSerializableSide.Deserialize
        'double
        Dim minPressure As Double
        If Not archive.ReadDouble(minPressure) Then Return False
        MinValPressure(side) = minPressure
        Dim maxPressure As Double
        If Not archive.ReadDouble(maxPressure) Then Return False
        MaxValPressure(side) = maxPressure
        Dim minHue As Double
        If Not archive.ReadDouble(minHue) Then Return False
        MinValHue(side) = minHue
        Dim maxHue As Double
        If Not archive.ReadDouble(maxHue) Then Return False
        MaxValHue(side) = maxHue

        'UUID
        Dim uuid As New Guid
        If Not archive.ReadUuid(uuid) Then Return False
        If RhUtil.RhinoApp.ActiveDoc.LookupDocumentObject(uuid, True) IsNot Nothing Then SetBitmapMeshID(side, New Guid(uuid.ToString))      
        uuid = New Guid
        If Not archive.ReadUuid(uuid) Then Return False
        If RhUtil.RhinoApp.ActiveDoc.LookupDocumentObject(uuid, True) IsNot Nothing Then SetProjectionMeshID(side, New Guid(uuid.ToString))

        'OnMaterial   
        Dim onobj As OnObject = New OnMaterial()
        If Not CBool(archive.ReadObject(onobj)) Then Return False
        Dim material = OnMaterial.Cast(onobj).Duplicate()       
        Dim textureExist = False        
        If Not archive.ReadBool(textureExist) Then Return False
        If textureExist
            Dim buffer() As Byte = {}
            If Not archive.ReadCompressedBuffer(buffer, True) Then Return False
            Dim stream as new MemoryStream(buffer)
            Dim bitmap = Drawing.Bitmap.FromStream(stream)            
            Dim tempFile = Path.GetTempFileName()
            bitmap.Save(tempFile, Imaging.ImageFormat.Bmp)
            UpdatePressureMapMesh(material, tempFile, side)
        End If
        SetMaterial(side, material)

        'On3dPointArray
        Dim points As Integer = -1
        If Not archive.ReadInt(points) Then Return False
        Dim numerciMap As New On3dPointArray(points)
        For i As Integer = 0 To points - 1
            Dim p As New On3dPoint
            If Not archive.ReadPoint(p) Then Return False            
            numerciMap.Append(p)
        Next
        SetNumericMap(side, numerciMap)

        Return True
    End Function



#End Region


#Region " IClonable "

    Public Function Clone() As Object Implements ICloneable.Clone
        Dim res As New IdPressureMap
        res.mMaxPressureLeft = mMaxPressureLeft
        res.mMaxPressureRight = mMaxPressureRight
        res.mMinPressureLeft = mMinPressureLeft
        res.mMinPressureRight = mMinPressureRight
        res.mMinHueLeft = mMinHueLeft
        res.mMaxHueLeft = mMaxHueLeft
        res.mMinHueRight = mMinHueRight
        res.mMaxHueRight = mMaxHueRight
        res.mBitmapMeshLeftID = New Guid(mBitmapMeshLeftID.ToString())
        res.mBitmapMeshRightID = New Guid(mBitmapMeshRightID.ToString())
        res.mProjectionMeshLeftID = New Guid(mProjectionMeshLeftID.ToString())
        res.mProjectionMeshRightID = New Guid(mProjectionMeshRightID.ToString())
        res.mMaterialLeft = New OnMaterial(mMaterialLeft)
        res.mMaterialRight = New OnMaterial(mMaterialRight)
        For Each p As On3dPoint In mNumericMapLeft
            res.mNumericMapLeft.Append(p)
        Next
        For Each p As On3dPoint In mNumericMapRight
            res.mNumericMapRight.Append(p)
        Next
        Return res
    End Function

#End Region



End Class
