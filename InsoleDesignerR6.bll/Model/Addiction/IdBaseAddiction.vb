Imports System.IO
Imports RMA.OpenNURBS
Imports InsoleDesigner.bll.IdElement3dManager
Imports GeometryUtils = InsoleDesigner.bll.IdGeometryUtils
Imports RMA.Rhino


''*********************************************************************************************************************
''*** Questa classe generalizza gli scarichi oliva e goccia(metatarsal dome) che hanno solo modelli cad diversi     ***
''*********************************************************************************************************************


Public Class IdBaseAddiction
    Inherits IdAddiction


#Region " CONSTRUCTOR "

    Public Sub New(ByVal side As eSide, ByVal type As eAddictionType, ByVal model As eAddictionModel, ByVal size As eAddictionSize)
        MyBase.New(side, type, model, size)
    End Sub

#End Region


#Region " Funzioni CAD "


    Public Shared Function ProcessBlendIntersection(getObj As MRhinoGetObject, ByVal closeIntersection As Boolean) As MRhinoObjRef
        If getObj.GetObjects(0, Integer.MaxValue) <> IRhinoGet.result.object Then Return Nothing
        If getObj.ObjectCount = 0 Then Return Nothing
        Dim maxLength As Double = 0.0
        Dim curveIndex As Integer = -1
        For i As Integer = 0 To getObj.ObjectCount - 1
            Dim currentLenght As Double
            getObj.Object(i).Curve.GetLength(currentLenght)
            If currentLenght > maxLength Then
                maxLength = currentLenght
                curveIndex = i
            End If
        Next
        For i As Integer = 0 To getObj.ObjectCount - 1
            If i <> curveIndex Then RhUtil.RhinoApp.ActiveDoc.DeleteObject(getObj.Object(i))
        Next
        If curveIndex = -1 Then
            Return Nothing
        End If
        Dim result As MRhinoObjRef = getObj.Object(curveIndex)
        If closeIntersection And Not result.Curve.IsClosed Then
            Dim closedCurve As OnCurve = result.Curve.DuplicateCurve
            Dim startPoint As On3dPoint = closedCurve.PointAtStart
            closedCurve.SetEndPoint(startPoint)
            startPoint.Dispose()
            RhUtil.RhinoApp.ActiveDoc.ReplaceObject(result, closedCurve)
        End If
        Return result
    End Function


#End Region


#Region " SERIALIZZAZIONE / DESERIALIZZAZIONE "


    Public Overrides Function Serialize(ByRef archive As RMA.OpenNURBS.OnBinaryArchive) As Boolean
        Return MyBase.CommonSerialize(archive)
        ''Sviluppi futuri...
    End Function

    Public Overrides Function Deserialize(ByRef archive As RMA.OpenNURBS.OnBinaryArchive) As Boolean
        ''Sviluppi futuri...
        Return True
    End Function


#End Region


#Region " IClonable "


    Public Overrides Function Clone() As Object
        Dim plainObject As IdAddiction = IdAddictionFactory.Create(Me.Side, Me.Type, Me.Model, Me.Size)
        MyBase.CloneCommonField(plainObject)
        Dim result As IdBaseAddiction = DirectCast(plainObject, IdBaseAddiction)
        Return result
    End Function


#End Region


End Class
