Imports RMA.OpenNURBS

'**************************************************************
'*** Gestione della clusterizzazione di direzioni spaziali ***
'**************************************************************

''' <summary>
''' Classe che contiene i dati del singolo settore sferico
''' </summary>
''' <remarks></remarks>
Public Class RhSectorData
    Implements IComparable

    Public weightSum As Double
    Public weightedPointsSum As On3dPoint
    Public weightedDirectionsSum As On3dVector

    Public Sub New()
        weightSum = 0
        weightedPointsSum = New On3dPoint(0, 0, 0)
        weightedDirectionsSum = New On3dVector(0, 0, 0)
    End Sub

    Public Function MeanPoint() As On3dPoint
        If weightSum = 0 Then Return weightedPointsSum
        Return weightedPointsSum * (1 / weightSum)
    End Function

    Public Function MeanDirection() As On3dVector
        If weightSum = 0 Then Return weightedDirectionsSum
        Dim res As On3dVector = weightedDirectionsSum * (1 / weightSum)
        res.Unitize()
        Return res
    End Function

    Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo
        Return Me.weightSum.CompareTo(DirectCast(obj, RhSectorData).weightSum)
    End Function

End Class



''' <summary>
''' Classe che esprime una collection di DatiSettore
''' </summary>
''' <remarks></remarks>
Public Class RhSectorsData
    Inherits List(Of RhSectorData)

    Private Sub New()
    End Sub

    Public Sub New(ByVal sectorCount As Integer)
        MyBase.New(sectorCount)
    End Sub


    ''' <summary>
    ''' Ordina la collection in base al peso crescente
    ''' </summary>
    ''' <remarks>Ritorna un array di indici</remarks>
    Public Function SortedIndexesByWeight() As Integer()
        Dim sortedList As New RhSectorsData(Me.Count)
        For i As Integer = 0 To Me.Count - 1
            sortedList.Add(Me(i))
        Next
        sortedList.Sort()
        Dim result(Me.Count - 1) As Integer
        For i As Integer = 0 To Me.Count - 1
            result(i) = Me.IndexOf(sortedList(i))
        Next
        Return result
    End Function

End Class



''' <summary>
''' Classe che gestisce la suddivisione dello spazio in settori sferici
''' </summary>
''' <remarks>Le due calotte polari sono gestite come un unico settore</remarks>
Public Class RhSectors

    Dim mSectorsData As RhSectorsData

    Public Structure SectorCoordinates
        Dim Azimuth As Integer
        Dim Elevation As Integer
    End Structure

    Dim mMeridianCountPerQuadrant As Integer
    Public ReadOnly Property MeridianCountPerQuadrant() As Integer
        Get
            Return mMeridianCountPerQuadrant
        End Get
    End Property

    Dim mParallelCountPerQuadrant As Integer
    Public ReadOnly Property ParallelCountPerQuadrant() As Integer
        Get
            Return mParallelCountPerQuadrant
        End Get
    End Property


    ''' <summary>
    ''' Rendo il costruttore senza parametri privato
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub New()
    End Sub


    ''' <summary>
    ''' Costruttore che instanzia la classe correttamente
    ''' </summary>
    ''' <param name="meridianCountPerQuadrant"></param>
    ''' <param name="parallelCountPerQuadrant"></param>
    ''' <remarks></remarks>
    Public Sub New(ByVal meridianCountPerQuadrant As Integer, ByVal parallelCountPerQuadrant As Integer)
        Me.mMeridianCountPerQuadrant = meridianCountPerQuadrant
        Me.mParallelCountPerQuadrant = parallelCountPerQuadrant
        mSectorsData = New RhSectorsData(SectorCount)
        For i As Integer = 0 To SectorCount() - 1
            mSectorsData.Add(New RhSectorData)
        Next
    End Sub


    ''' <summary>
    ''' Ritorna il numero totale di settori
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function SectorCount() As Integer
        Dim comleteParallelsCount As Integer = 2 * mParallelCountPerQuadrant - 1
        Return 4 * mMeridianCountPerQuadrant * comleteParallelsCount + 2
    End Function


    ''' <summary>
    ''' Calcola le coordinate del settore di appartenza di un generico punto nello spazio
    ''' </summary>
    ''' <param name="position"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function FindSector(ByVal position As IOn3dPoint) As SectorCoordinates
        Dim direzione As New On3dVector(position)
        direzione.Unitize()
        Dim longitudine, latitudine As Double
        CartesianToSpherical(direzione, longitudine, latitudine)
        longitudine += Math.PI
        Dim res As SectorCoordinates
        res.Azimuth = CInt(Math.Round(longitudine / (0.5 * Math.PI / mMeridianCountPerQuadrant), 0))
        If res.Azimuth = 4 * mMeridianCountPerQuadrant Then res.Azimuth = 0
        res.Elevation = CInt(Math.Round(latitudine / (0.5 * Math.PI / mParallelCountPerQuadrant), 0))
        Return res
    End Function


    ''' <summary>
    ''' Calcola le coordinate del settore di appartenza di una generica direzione nello spazio
    ''' </summary>
    ''' <param name="direction"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function FindSector(ByVal direction As IOn3fVector) As SectorCoordinates
        Dim copyDirezione As New On3dVector(direction.x, direction.y, direction.z)
        copyDirezione.Unitize()
        Dim longitudine, latitudine As Double
        CartesianToSpherical(copyDirezione, longitudine, latitudine)
        longitudine += Math.PI
        Dim res As SectorCoordinates
        res.Azimuth = CInt(Math.Round(longitudine / (0.5 * Math.PI / mMeridianCountPerQuadrant), 0))
        If res.Azimuth = 4 * mMeridianCountPerQuadrant Then res.Azimuth = 0
        res.Elevation = CInt(Math.Round(latitudine / (0.5 * Math.PI / mParallelCountPerQuadrant), 0))
        Return res
    End Function


    ''' <summary>
    ''' Ritorna latitudine e longitudine di un vettore unitario
    ''' </summary>
    ''' <param name="vector">Vettore unitario</param>
    ''' <param name="azimuth">In radianti. Il risultato varia in [-PI, PI]</param>
    ''' <param name="elevation">In radianti. Il risultato varia in [-PI/2, PI/2]</param>
    ''' <remarks></remarks>
    Public Sub CartesianToSpherical(ByVal vector As IOn3dVector, ByRef azimuth As Double, ByRef elevation As Double)
        elevation = Math.Asin(vector.z)
        azimuth = Math.Atan2(vector.y, vector.x)
    End Sub


    ''' <summary>
    ''' Costruisce un vettore a partire dalla sua latitudine e longitudine
    ''' </summary>
    ''' <param name="azimuth">In radianti. Il valore varia in [-PI/2, PI/2]</param>
    ''' <param name="elevation">In radianti. Il valore varia in [-PI, PI]</param>
    ''' <param name="vector">Ritorna un vettore unitario</param>
    ''' <remarks></remarks>
    Public Sub SphericalToCartesian(ByVal azimuth As Double, ByVal elevation As Double, ByRef vector As On3dVector)
        If vector Is Nothing Then vector = New On3dVector
        vector.z = Math.Sin(elevation)
        vector.x = Math.Cos(elevation) * Math.Cos(azimuth)
        vector.y = Math.Cos(elevation) * Math.Sin(azimuth)
    End Sub


    ''' <summary>
    ''' Ritorna i dati di un determinato settore individuato dagli indici del parallelo e del meridiano
    ''' </summary>
    ''' <param name="parallelIndex">Se indiceParellelo corrisponde ad una calotta polare allora indiceMeridiano viene ignorato</param>
    ''' <param name="meridianIndex"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Sector(ByVal parallelIndex As Integer, ByVal meridianIndex As Integer) As RhSectorData
        If parallelIndex <= -mParallelCountPerQuadrant Then
            Return mSectorsData(0)
        End If
        If parallelIndex >= mParallelCountPerQuadrant Then
            Return mSectorsData(SectorCount() - 1)
        End If

        Dim index As Integer = 4 * mMeridianCountPerQuadrant * parallelIndex + meridianIndex
        index += 4 * mMeridianCountPerQuadrant * (mParallelCountPerQuadrant - 1)
        index += 1
        Return mSectorsData(index)
    End Function


    ''' <summary>
    ''' Ritorna i dati di un determinato settore
    ''' </summary>
    ''' <param name="coordinates"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Sector(ByVal coordinates As RhSectors.SectorCoordinates) As RhSectorData
        Return Sector(coordinates.Elevation, coordinates.Azimuth)
    End Function


    ''' <summary>
    ''' Ritorna i dati di un determinato settore individuato dal suo indice progessivo
    ''' </summary>
    ''' <param name="index"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Sector(ByVal index As Integer) As RhSectorData
        Return DirectCast(mSectorsData(index), RhSectorData)
    End Function


    ''' <summary>
    ''' Ritorna le coordinate di un settore dal suo indice
    ''' </summary>
    ''' <param name="index"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Coordinates(ByVal index As Integer) As SectorCoordinates
        Dim result As SectorCoordinates
        If index <= 0 Then
            result.Elevation = -Me.mParallelCountPerQuadrant
            result.Azimuth = 0
            Return result
        End If
        If index >= SectorCount() - 1 Then
            result.Elevation = Me.mParallelCountPerQuadrant
            result.Azimuth = 0
            Return result
        End If
        index -= 1
        result.Azimuth = index Mod (4 * mMeridianCountPerQuadrant)
        result.Elevation = (index \ (4 * mMeridianCountPerQuadrant)) - (mParallelCountPerQuadrant - 1)
        Return result
    End Function


    ''' <summary>
    ''' Ritorna le coordinate di un settore
    ''' </summary>
    ''' <param name="sector"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Coordinates(ByVal sector As RhSectorData) As SectorCoordinates
        Return Coordinates(mSectorsData.IndexOf(sector))
    End Function


    ''' <summary>
    ''' Calcola il centro geometrico di un settore
    ''' </summary>
    ''' <param name="indice"></param>
    ''' <param name="distanzaRadiale"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function SectorCenter(ByVal indice As Integer, ByVal distanzaRadiale As Double) As On3dPoint
        Dim coords As SectorCoordinates = Coordinates(indice)
        Dim punto As New On3dPoint(distanzaRadiale, 0, 0)
        punto.Rotate(-coords.Elevation * 0.5 * Math.PI / mParallelCountPerQuadrant, OnPlane.World_xy.yaxis, OnPlane.World_xy.origin)
        punto.Rotate(coords.Azimuth * 0.5 * Math.PI / mMeridianCountPerQuadrant, OnPlane.World_xy.zaxis, OnPlane.World_xy.origin)
        Return punto
    End Function


    ''' <summary>
    ''' Calcola il punto medio di un settore
    ''' </summary>
    ''' <param name="index"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function MeanPoint(ByVal index As Integer) As On3dPoint
        Return Sector(index).MeanPoint
    End Function


    ''' <summary>
    ''' Calcola la direzione media di un settore
    ''' </summary>
    ''' <param name="index"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function MeanDirection(ByVal index As Integer) As On3dVector
        Return Sector(index).MeanDirection
    End Function


    ''' <summary>
    ''' Corregge il peso trasformandolo in densità areale dello spicchio
    ''' </summary>
    ''' <remarks>Non credo che questa funzione abbia molto senso nel caso delle direzioni</remarks>
    Public Sub CorrectWeightBySectorEstension()
        Dim centerElevationCosine As Double
        For j As Integer = -mParallelCountPerQuadrant + 1 To mParallelCountPerQuadrant - 1
            centerElevationCosine = Math.Cos(j * Math.PI / (2 * mParallelCountPerQuadrant))
            For i As Integer = 0 To 4 * mMeridianCountPerQuadrant - 1
                Sector(i, j).weightSum /= centerElevationCosine
            Next
        Next
        centerElevationCosine = Math.Cos(Math.PI * (0.5 - 0.25 / mParallelCountPerQuadrant)) * 4 * mMeridianCountPerQuadrant / 2
        Sector(-mParallelCountPerQuadrant, 0).weightSum /= centerElevationCosine
        Sector(mParallelCountPerQuadrant, 0).weightSum /= centerElevationCosine
    End Sub


    ''' <summary>
    ''' Ritorna un array di indici di settori ordinati in base al peso
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function SortedIndexesByWeight() As Integer()
        Return Me.mSectorsData.SortedIndexesByWeight
    End Function

End Class



