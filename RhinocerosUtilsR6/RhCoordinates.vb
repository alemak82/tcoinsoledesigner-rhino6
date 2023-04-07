Imports RMA.Rhino
Imports RMA.OpenNURBS


'*****************************************************************
'*** Classe per la gestione delle trasformazioni di coordinate ***
'*****************************************************************

Public Class RhCoordinates

    ''' <summary>
    ''' Ritorna le coordinate di un punto come stringa per RhinoScript
    ''' </summary>
    ''' <param name="point"></param>
    ''' <param name="addParentesis"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function C3dPointToString(ByVal point As RMA.OpenNURBS.IOn3dPoint, Optional ByVal addParentesis As Boolean = False) As String
        Dim strTemp As String = ""
        RhUtil.RhinoFormatPoint(point, strTemp)
        If addParentesis Then
            Return "(" & strTemp & ")"
        Else
            Return strTemp
        End If
    End Function


    ''' <summary>
    ''' Esegue la conversione delle coordinate di un punto
    ''' </summary>
    ''' <param name="localPlane"></param>
    ''' <param name="localPoint"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function CoordinateLocalToWorld(ByVal localPlane As IOnPlane, ByVal localPoint As IOn3dPoint) As On3dPoint
        Dim xFormLocalToWorld As New OnXform
        xFormLocalToWorld.ChangeBasis(OnPlane.World_xy.xaxis, OnPlane.World_xy.yaxis, OnPlane.World_xy.zaxis, localPlane.xaxis, localPlane.yaxis, localPlane.zaxis)
        Dim res As On3dPoint = New On3dPoint(localPoint) * xFormLocalToWorld + New On3dVector(localPlane.origin)
        xFormLocalToWorld.Dispose()
        Return res
    End Function


    ''' <summary>
    ''' Esegue la conversione delle coordinate di un vettore
    ''' </summary>
    ''' <param name="localPlane"></param>
    ''' <param name="localVector"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function CoordinateLocalToWorld(ByVal localPlane As IOnPlane, ByVal localVector As IOn3dVector) As On3dVector
        Dim xFormLocalToWorld As New OnXform
        xFormLocalToWorld.ChangeBasis(OnPlane.World_xy.xaxis, OnPlane.World_xy.yaxis, OnPlane.World_xy.Normal, localPlane.xaxis, localPlane.yaxis, localPlane.zaxis)
        Dim res As On3dVector = New On3dVector(localVector.x, localVector.y, localVector.z) * xFormLocalToWorld
        xFormLocalToWorld.Dispose()
        Return res
    End Function



    '''' <summary>
    '''' Esegue la conversione delle coordinate di un punto
    '''' </summary>
    '''' <param name="localPlane"></param>
    '''' <param name="worldPoint"></param>
    '''' <returns></returns>
    '''' <remarks></remarks>
    'Public Shared Function CoordinateWorldToLocal(ByVal localPlane As IOnPlane, ByVal worldPoint As IOn3dPoint) As On3dPoint
    '    Dim xFormWorldToLocal As New OnXform
    '    xFormWorldToLocal.ChangeBasis(localPlane.xaxis, localPlane.yaxis, localPlane.zaxis, OnPlane.World_xy.xaxis, OnPlane.World_xy.yaxis, OnPlane.World_xy.zaxis)
    '    Dim res As On3dPoint = (New On3dPoint(worldPoint) - New On3dVector(localPlane.origin)) * xFormWorldToLocal
    '    xFormWorldToLocal.Dispose()
    '    Return res
    'End Function


    '''' <summary>
    '''' Esegue la conversione delle coordinate di un vettore
    '''' </summary>
    '''' <param name="localPlane"></param>
    '''' <param name="worldVector"></param>
    '''' <returns></returns>
    '''' <remarks></remarks>
    'Public Shared Function CoordinateWorldToLocal(ByVal localPlane As IOnPlane, ByVal worldVector As On3fVector) As On3dVector
    '    Dim xFormWorldToLocal As New OnXform
    '    xFormWorldToLocal.ChangeBasis(localPlane.xaxis, localPlane.yaxis, localPlane.zaxis, OnPlane.World_xy.xaxis, OnPlane.World_xy.yaxis, OnPlane.World_xy.zaxis)
    '    Dim res As On3dVector = New On3dVector(worldVector.x, worldVector.y, worldVector.z) * xFormWorldToLocal
    '    xFormWorldToLocal.Dispose()
    '    Return res
    'End Function


    '''' <summary>
    '''' Esegue la conversione delle coordinate di un vettore
    '''' </summary>
    '''' <param name="localPlane"></param>
    '''' <param name="worldVector"></param>
    '''' <returns></returns>
    '''' <remarks></remarks>
    'Public Shared Function CoordinateWorldToLocal(ByVal localPlane As IOnPlane, ByVal worldVector As IOn3dVector) As On3dVector
    '    Dim xFormWorldToLocal As New OnXform
    '    xFormWorldToLocal.ChangeBasis(localPlane.xaxis, localPlane.yaxis, localPlane.zaxis, OnPlane.World_xy.xaxis, OnPlane.World_xy.yaxis, OnPlane.World_xy.zaxis)
    '    Dim res As On3dVector = New On3dVector(worldVector) * xFormWorldToLocal
    '    xFormWorldToLocal.Dispose()
    '    Return res
    'End Function







    ''' <summary>
    ''' Esegue la conversione delle coordinate di un punto
    ''' </summary>
    ''' <param name="localPlane"></param>
    ''' <param name="worldPoint"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function CoordinateWorldToLocal(ByVal localPlane As IOnPlane, ByVal worldPoint As IOn3dPoint) As On3dPoint
        Dim xFormWorldToLocal As New OnXform
        xFormWorldToLocal.ChangeBasis(localPlane.xaxis, localPlane.yaxis, localPlane.zaxis, OnPlane.World_xy.xaxis, OnPlane.World_xy.yaxis, OnPlane.World_xy.zaxis)
        Dim res As On3dPoint = (New On3dPoint(worldPoint) - New On3dVector(localPlane.origin)) * xFormWorldToLocal
        xFormWorldToLocal.Dispose()
        Return res
    End Function


    ''' <summary>
    ''' Esegue la conversione delle coordinate di un vettore
    ''' </summary>
    ''' <param name="localPlane"></param>
    ''' <param name="worldVector"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function CoordinateWorldToLocal(ByVal localPlane As IOnPlane, ByVal worldVector As On3fVector) As On3dVector
        Dim xFormWorldToLocal As New OnXform
        xFormWorldToLocal.ChangeBasis(localPlane.xaxis, localPlane.yaxis, localPlane.zaxis, OnPlane.World_xy.xaxis, OnPlane.World_xy.yaxis, OnPlane.World_xy.zaxis)
        Dim res As On3dVector = New On3dVector(worldVector.x, worldVector.y, worldVector.z) * xFormWorldToLocal
        xFormWorldToLocal.Dispose()
        Return res
    End Function


    'Roberto: qui credo sia sbagliata perchè moltiplicare (vettore * xForm) è diverso dall'applicare Transform
    'Roberto: però pare che funziona! Approfondire

    ''' <summary>
    ''' Esegue la conversione delle coordinate di un vettore
    ''' </summary>
    ''' <param name="localPlane"></param>
    ''' <param name="worldVector"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function CoordinateWorldToLocal(ByVal localPlane As IOnPlane, ByVal worldVector As IOn3dVector) As On3dVector
        Dim xFormWorldToLocal As New OnXform
        xFormWorldToLocal.ChangeBasis(localPlane.xaxis, localPlane.yaxis, localPlane.zaxis, OnPlane.World_xy.xaxis, OnPlane.World_xy.yaxis, OnPlane.World_xy.zaxis)
        Dim res As On3dVector = New On3dVector(worldVector) * xFormWorldToLocal
        xFormWorldToLocal.Dispose()
        Return res
    End Function


    ''' <summary>
    ''' Esegue la conversione delle coordinate di un array di punti; da verificare, mai provata
    ''' </summary>
    ''' <param name="localPlane"></param>
    ''' <param name="worldArray"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function ArrayWorldToLocal(ByVal localPlane As IOnPlane, ByVal worldArray As IOn3dPointArray) As On3dPointArray
        Dim xFormWorldToLocal As New OnXform
        xFormWorldToLocal.ChangeBasis(localPlane.xaxis, localPlane.yaxis, localPlane.zaxis, OnPlane.World_xy.xaxis, OnPlane.World_xy.yaxis, OnPlane.World_xy.zaxis)
        Dim traslation As New On3dVector(localPlane.origin)
        traslation.Reverse()
        Dim traslationXForm As New OnXform
        traslationXForm.Translation(traslation)
        Dim result As New On3dPointArray(worldArray)
        If Not result.Transform(xFormWorldToLocal * traslationXForm) Then
            If Not result Is Nothing Then result.Dispose()
        End If
        xFormWorldToLocal.Dispose()
        traslationXForm.Dispose()
        Return result
    End Function


    ''' <summary>
    ''' Esegue la conversione delle coordinate di un array di vettori 3f
    ''' </summary>
    ''' <param name="localPlane"></param>
    ''' <param name="worldArray"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function ArrayWorldToLocal(ByVal localPlane As IOnPlane, ByVal worldArray As IOn3fVectorArray) As On3fVectorArray
        Dim xFormWorldToLocal As New OnXform
        xFormWorldToLocal.ChangeBasis(localPlane.xaxis, localPlane.yaxis, localPlane.zaxis, OnPlane.World_xy.xaxis, OnPlane.World_xy.yaxis, OnPlane.World_xy.zaxis)
        Dim result As New On3fVectorArray(worldArray)
        xFormWorldToLocal.Transpose()
        If Not result.Transform(xFormWorldToLocal) Then
            If Not result Is Nothing Then result.Dispose()
        End If
        xFormWorldToLocal.Dispose()
        Return result
    End Function

End Class
