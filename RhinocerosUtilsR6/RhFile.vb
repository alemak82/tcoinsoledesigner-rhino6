Imports RMA.Rhino
Imports RMA.OpenNURBS
Imports System.Drawing


'*****************************************
'*** Classe per la lettura formati CAD ***
'*****************************************

Public Class RhFile

    ''' <summary>
    ''' Legge un file in formato "*.vrm" contenente la definizione di una mesh e di una texture
    ''' </summary>
    ''' <param name="fileName"></param>
    ''' <param name="immagine"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function LeggiVRML(ByVal fileName As String, ByRef immagine(,) As Color) As OnMesh
        Dim file As New System.IO.StreamReader(fileName)
        Dim lineaCorrente As String = file.ReadLine
        lineaCorrente = lineaCorrente.Trim()
        If Not lineaCorrente = "#VRML V2.0 utf8" Then
            RhUtil.RhinoApp.Print("VRML file version is not valid" & vbCrLf)
            Return Nothing
        End If
        lineaCorrente = file.ReadLine
        lineaCorrente = lineaCorrente.Trim()
        If Not lineaCorrente = "# Polygon Editing Tool" Then
            RhUtil.RhinoApp.Print("File not created by Konica-Minolta Polygon Editing Tool software" & vbCrLf)
            Return Nothing
        End If

        Do
            If file.EndOfStream Then
                RhUtil.RhinoApp.Print("File saved without any image" & vbCrLf)
                Return Nothing
            End If
            lineaCorrente = file.ReadLine
        Loop Until lineaCorrente.IndexOf("image") >= 0

        Dim larghezzaImmagine, altezzaImmagine As Integer
        lineaCorrente = lineaCorrente.Trim()
        Dim s() As String = lineaCorrente.Split
        larghezzaImmagine = CInt(s(1))
        altezzaImmagine = CInt(s(2))

        'Crea l'immagine        
        RhUtil.RhinoApp.Wait(300)
        ReDim immagine(larghezzaImmagine - 1, altezzaImmagine - 1)
        Dim numeroLineePerRiga As Integer = larghezzaImmagine \ 8
        For j As Integer = 0 To altezzaImmagine - 1
            For h As Integer = 0 To numeroLineePerRiga - 1
                lineaCorrente = file.ReadLine
                lineaCorrente = lineaCorrente.Trim()
                Dim c() As String = lineaCorrente.Split
                For i As Integer = 0 To c.GetUpperBound(0)
                    immagine(8 * h + i, j) = Color.FromArgb(Convert.ToInt32(c(i), 16))
                Next
            Next
        Next

        'Leggi le coordinate delle texture
        Do
            lineaCorrente = file.ReadLine
        Loop Until lineaCorrente.IndexOf("TextureCoordinate") >= 0
        file.ReadLine()     'scorri 1 linee (1 è vuota)
        Dim textureCoordsArray As New On2fPointArray
        Dim esci As Boolean = False
        Do
            lineaCorrente = file.ReadLine
            esci = Not lineaCorrente.EndsWith(",")
            lineaCorrente = lineaCorrente.Replace(",", "")
            lineaCorrente = lineaCorrente.Replace(".", ",")
            s = lineaCorrente.Split()
            Dim textureCoords As New On2fPoint(CSng(s(0)), CSng(s(1)))
            textureCoordsArray.Append(textureCoords)
        Loop Until esci

        'Leggi le coordinate dei vertici
        Do
            lineaCorrente = file.ReadLine
        Loop Until lineaCorrente.IndexOf("Coordinate {") >= 0
        file.ReadLine()     'scorri 1 linea
        Dim vertexArray As New On3fPointArray
        Do
            lineaCorrente = file.ReadLine
            esci = Not lineaCorrente.EndsWith(",")
            lineaCorrente = lineaCorrente.Replace(",", "")
            lineaCorrente = lineaCorrente.Replace(".", ",")
            s = lineaCorrente.Split()
            Dim vertex As New On3fPoint(CSng(s(0)), CSng(s(1)), CSng(s(2)))
            vertexArray.Append(vertex)
        Loop Until esci

        'Leggi le faccettine
        Dim meshPlane As OnMesh = RMA.Rhino.RhUtil.RhinoMeshPlane(OnPlane.World_xy, New OnInterval(0, 1), New OnInterval(0, 1), 1, 1)
        Dim facetToCopy As IOnMeshFace = meshPlane.m_F(0)
        Do
            lineaCorrente = file.ReadLine
        Loop Until lineaCorrente.IndexOf("coordIndex [") >= 0
        Dim facetsArray As New ArrayOnMeshFace
        Do
            lineaCorrente = file.ReadLine
            esci = Not lineaCorrente.EndsWith(",")
            lineaCorrente = lineaCorrente.Replace(",", "")
            s = lineaCorrente.Split()
            Dim facet As New OnMeshFace(facetToCopy)
            facet.vi(0) = CInt(s(0))
            facet.vi(1) = CInt(s(1))
            facet.vi(2) = CInt(s(2))
            If s.GetUpperBound(0) = 3 Then
                facet.vi(3) = CInt(s(2))
            Else
                facet.vi(3) = CInt(s(3))
            End If
            facetsArray.Append(facet)
        Loop Until esci
        file.Close()

        'Calcola colore per punti dove non è definito
        Dim analisi As New RhImageAnalysis(immagine)
        analisi.CalcolaIstogrammi()
        Dim maxIndexTonalità As Integer
        RhImageAnalysis.Max(analisi.IstogrammaTonalità, maxIndexTonalità)
        Dim maxIndexSaturazione As Integer
        RhImageAnalysis.Max(analisi.IstogrammaSaturazione, maxIndexSaturazione)
        Dim maxIndexLuminosità As Integer
        RhImageAnalysis.Max(analisi.IstogrammaLuminosità, maxIndexLuminosità)

        Dim coloreRiempimento As OnColor = New OnColor
        coloreRiempimento.SetHSV(maxIndexTonalità * Math.PI / 180, maxIndexSaturazione / 100, maxIndexLuminosità / 100)

        'Crea i colori
        If textureCoordsArray.Count > vertexArray.Count Then
            textureCoordsArray.SetCapacity(vertexArray.Count)
        End If
        If textureCoordsArray.Count < vertexArray.Count Then
            Dim mancanti As Integer = vertexArray.Count - textureCoordsArray.Count
            textureCoordsArray.SetCapacity(vertexArray.Count)
            For i As Integer = 0 To mancanti - 1
                textureCoordsArray.Append(New On2fPoint(-1, -1))    'Coordinate Texture non definite
            Next
        End If

        Dim coloriArray As New ArrayOnColor(textureCoordsArray.Count)
        For i As Integer = 0 To textureCoordsArray.Count - 1
            Dim coord As On2fPoint = textureCoordsArray(i)
            Dim x As Integer = CInt(Math.Round(larghezzaImmagine * coord.x))
            Dim y As Integer = CInt(Math.Round(altezzaImmagine * coord.y))
            If x > larghezzaImmagine - 1 Then x = larghezzaImmagine - 1
            If y > altezzaImmagine - 1 Then y = altezzaImmagine - 1
            If coord.x <= 0 And coord.y <= 0 Then               'Coordinate Texture nulle o non definite
                coloriArray.Append(coloreRiempimento)
            Else
                coloriArray.Append(New OnColor(immagine(x, y)))
            End If
        Next

        'Crea la mesh
        Dim res As New OnMesh
        res.m_F = facetsArray
        res.m_V = vertexArray
        res.m_C = coloriArray
        res.Compact()
        res.ConvertQuadsToTriangles()
        If Not res.IsValid Then
            RhUtil.RhinoApp.Print("Mesh is not valid" & vbCrLf)
            Return Nothing
        End If
        Return res
    End Function


    ''' <summary>
    ''' Legge un file in formato "*.mgf" contenente la definizione di una mesh i cui vertici sono colorati
    ''' </summary>
    ''' <param name="fileName"></param>
    ''' <param name="immagine"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function LeggiMGF(ByVal fileName As String, ByRef immagine(,) As Color) As OnMesh
        Dim file As New System.IO.StreamReader(fileName)
        Dim lineaCorrente As String = file.ReadLine
        lineaCorrente = lineaCorrente.Trim()
        Do
            lineaCorrente = file.ReadLine
        Loop Until lineaCorrente.IndexOf("color") >= 0
        Dim numeroRighe As Integer = CInt(file.ReadLine)
        Dim s() As String = lineaCorrente.Split

        Dim vertexArray As New On3fPointArray
        Dim coloriArray As New ArrayOnColor

        'Crea l'immagine
        'RhUtil.RhinoApp.Print("Attendere caricamento della formella..." & vbCrLf)
        RhUtil.RhinoApp.Wait(300)

        For i As Integer = 0 To numeroRighe - 1
            lineaCorrente = file.ReadLine
            lineaCorrente = lineaCorrente.Replace(".", ",")
            s = lineaCorrente.Split()
            Dim vertex As New On3fPoint(CSng(s(0)), CSng(s(1)), CSng(s(2)))
            vertexArray.Append(vertex)
            Dim color As New OnColor(CInt(Convert.ToDouble(s(3)) * 255), CInt(Convert.ToDouble(s(4)) * 255), CInt(Convert.ToDouble(s(5)) * 255))
            coloriArray.Append(color)
        Next

        ' Ricrea l'immagine
        ReDim immagine(0, coloriArray.Count - 1)
        For i As Integer = 0 To coloriArray.Count - 1
            immagine(0, i) = coloriArray(i)
        Next

        'Leggi le faccettine
        Dim meshPlane As OnMesh = RMA.Rhino.RhUtil.RhinoMeshPlane(OnPlane.World_xy, New OnInterval(0, 1), New OnInterval(0, 1), 1, 1)
        Dim facetToCopy As IOnMeshFace = meshPlane.m_F(0)

        numeroRighe = CInt(file.ReadLine)
        Dim facetsArray As New ArrayOnMeshFace

        For i As Integer = 0 To numeroRighe - 1
            Dim numeroVertici As Integer = CInt(file.ReadLine)
            lineaCorrente = file.ReadLine
            s = lineaCorrente.Split()
            Dim facet As New OnMeshFace(facetToCopy)
            facet.vi(0) = CInt(s(0)) - 1
            facet.vi(1) = CInt(s(1)) - 1
            facet.vi(2) = CInt(s(2)) - 1
            If numeroVertici = 4 Then
                facet.vi(3) = CInt(s(3)) - 1
            Else
                facet.vi(3) = CInt(s(2)) - 1
            End If
            facetsArray.Append(facet)
        Next

        file.Close()

        'Crea la mesh
        Dim res As New OnMesh
        res.m_F = facetsArray
        res.m_V = vertexArray
        res.m_C = coloriArray
        res.Compact()
        res.ConvertQuadsToTriangles()
        If Not res.IsValid Then
            RhUtil.RhinoApp.Print("Mesh is not valid" & vbCrLf)
            Return Nothing
        End If
        Return res

    End Function

    ''' <summary>
    ''' Esporta un oggetto salvando in un altro file di Rhino; se non viene specificato il parametro "fileName" viene proposta la DirectoryBrowser 
    ''' </summary>
    ''' <param name="rhinoObj"></param>
    ''' <param name="fileName"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function ExportSelectedObject(ByVal rhinoObj As IRhinoObject, Optional ByVal fileName As String = "") As Boolean
        Try
            rhinoObj.Select(True, False, True, True, True, True)
            If (fileName = "") Then
                RhUtil.RhinoApp.RunScript("-_Export _enter", 0)
            Else
                RhUtil.RhinoApp.RunScript("-_Export """ & fileName & """ _enter", 0)
            End If
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Shared Function ExportSelectedObject(ByVal rhinoObjRef As IRhinoObjRef, Optional ByVal fileName As String = "") As Boolean
        Return ExportSelectedObject(rhinoObjRef.Object, fileName)
    End Function


    Public Shared Function ExportSelectedObjects(ByVal rhinoObjs() As IRhinoObject, Optional ByVal fileName As String = "") As Boolean
        Try
            For i As Integer = 0 To rhinoObjs.GetUpperBound(0)
                rhinoObjs(i).Select(True, False, True, True, True, True)
            Next
            If (fileName = "") Then
                RhUtil.RhinoApp.RunScript("-_Export _b _enter", 0)
            Else
                RhUtil.RhinoApp.RunScript("-_Export """ & fileName & """ _enter", 0)
            End If
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Shared Function ExportSelectedObjects(ByVal rhinoObjRefs() As IRhinoObjRef, Optional ByVal fileName As String = "") As Boolean
        Dim objs(rhinoObjRefs.GetUpperBound(0)) As IRhinoObject
        For i As Integer = 0 To rhinoObjRefs.GetUpperBound(0)
            objs(i) = rhinoObjRefs(i).Object
        Next
        Return ExportSelectedObjects(objs, fileName)
    End Function



End Class
