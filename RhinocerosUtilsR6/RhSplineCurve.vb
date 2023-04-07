Imports RMA.OpenNURBS

'**********************************************
'*** Classe per la gestione di curve Spline ***
'**********************************************

Public Class RhSplineCurve
    Implements IDisposable

    Public Enum eKnotsMode As Integer
        uniform = 0
        chord = 1
        sqrtChord = 2
    End Enum

    Public Enum eEndPointsStyle As Integer
        natural = 0
        parabolic = 1
        cubic = 2
        periodic = 3
    End Enum


#Region " Fields & Initialization "

    Dim mEndPointsStyle As eEndPointsStyle

    Dim mT() As Double
    Dim mXCoefficients As OnMatrix
    Dim mYCoefficients As OnMatrix
    Dim mZCoefficients As OnMatrix

    Private Sub New()
    End Sub

    Public Sub New(ByVal points As IOn3dPointArray, ByVal knotsMode As eKnotsMode, ByVal endPointsStyle As eEndPointsStyle)
        Me.Interpolate(points, knotsMode, endPointsStyle)
    End Sub

    Public Function Interpolate(ByVal points As IOn3dPointArray, ByVal knotsMode As eKnotsMode, ByVal endPointsStyle As eEndPointsStyle) As Boolean
        If points Is Nothing Then Return False
        mEndPointsStyle = endPointsStyle

        If mEndPointsStyle = eEndPointsStyle.periodic Then
            If points.Count < 3 Then Return False
        Else
            If points.Count < 2 Then Return False
        End If

        Dim pointsCopy As New On3dPointArray()
        For i As Integer = 0 To points.Count - 1
            pointsCopy.Append(points(i))
        Next
        If mEndPointsStyle = eEndPointsStyle.periodic Then
            If points.First.CompareTo(points.Last) <> 0 Then
                pointsCopy.Append(points(0))
            End If
        End If

        'Parametro
        ReDim mT(pointsCopy.Count - 1)
        mT(0) = 0
        For i As Integer = 1 To pointsCopy.Count - 1
            Select Case knotsMode
                Case eKnotsMode.uniform
                    mT(i) = i
                Case eKnotsMode.chord
                    mT(i) = mT(i - 1) + pointsCopy(i - 1).DistanceTo(pointsCopy(i))
                Case eKnotsMode.sqrtChord
                    mT(i) = mT(i - 1) + Math.Sqrt(pointsCopy(i - 1).DistanceTo(pointsCopy(i)))
            End Select
        Next

        Dim pointCoordinates() As Double = pointsCopy.CopyToArray()
        pointsCopy.Dispose()
        If Not mXCoefficients Is Nothing Then mXCoefficients.Dispose()
        If Not mYCoefficients Is Nothing Then mYCoefficients.Dispose()
        If Not mZCoefficients Is Nothing Then mZCoefficients.Dispose()
        If mEndPointsStyle = eEndPointsStyle.periodic Then
            mXCoefficients = Me.ComputePeriodicCoefficientsMatrix(pointCoordinates, 0)
            mYCoefficients = Me.ComputePeriodicCoefficientsMatrix(pointCoordinates, 1)
            mZCoefficients = Me.ComputePeriodicCoefficientsMatrix(pointCoordinates, 2)
        Else
            mXCoefficients = Me.ComputeCoefficientsMatrix(pointCoordinates, 0)
            mYCoefficients = Me.ComputeCoefficientsMatrix(pointCoordinates, 1)
            mZCoefficients = Me.ComputeCoefficientsMatrix(pointCoordinates, 2)
        End If

        Return (Not mXCoefficients Is Nothing) And (Not mYCoefficients Is Nothing) And (Not mZCoefficients Is Nothing)
    End Function


    Public Function GetOffsetPoints(ByVal offsetStart As Double, ByVal offsetEnd As Double) As On3dPointArray
        Dim result As New On3dPointArray
        Dim iStart As Integer = 0
        Dim iEnd As Integer = mT.GetUpperBound(0)
        If mEndPointsStyle = eEndPointsStyle.periodic Then
            iStart = 1
            iEnd = mT.GetUpperBound(0) - 1
        End If
        For i As Integer = iStart To iEnd
            Dim point As On3dPoint = Me.PointAt(mT(i))
            Dim tangent As On3dVector = Me.FirstDerivativeAt(mT(i))
            tangent.Unitize()
            tangent.Rotate(Math.PI / 2, OnUtil.On_zaxis)
            Dim offset As Double = offsetStart + (offsetEnd - offsetStart) * (mT(i) - mT(iStart)) / (mT(iEnd) - mT(iStart))
            point += tangent * offset
            result.Append(point)
            point.Dispose()
            tangent.Dispose()
        Next
        Return result
    End Function

#End Region


#Region " Functions "

    Public Function PointAt(ByVal t As Double) As On3dPoint
        Dim si As Integer = 0
        For i As Integer = 0 To mT.GetUpperBound(0)
            If t > mT(i) Then si = i
        Next
        If si > mT.GetUpperBound(0) - 1 Then si = mT.GetUpperBound(0) - 1
        Dim x As Double = mXCoefficients(si, 0) + mXCoefficients(si, 1) * (t - mT(si)) + mXCoefficients(si, 2) * (t - mT(si)) ^ 2 + mXCoefficients(si, 3) * (t - mT(si)) ^ 3
        Dim y As Double = mYCoefficients(si, 0) + mYCoefficients(si, 1) * (t - mT(si)) + mYCoefficients(si, 2) * (t - mT(si)) ^ 2 + mYCoefficients(si, 3) * (t - mT(si)) ^ 3
        Dim z As Double = mZCoefficients(si, 0) + mZCoefficients(si, 1) * (t - mT(si)) + mZCoefficients(si, 2) * (t - mT(si)) ^ 2 + mZCoefficients(si, 3) * (t - mT(si)) ^ 3
        Return New On3dPoint(x, y, z)
    End Function


    Public Function FirstDerivativeAt(ByVal t As Double) As On3dVector
        Dim si As Integer = 0
        For i As Integer = 0 To mT.GetUpperBound(0)
            If t > mT(i) Then si = i
        Next
        If si > mT.GetUpperBound(0) - 1 Then si = mT.GetUpperBound(0) - 1
        Dim x As Double = mXCoefficients(si, 1) + 2 * mXCoefficients(si, 2) * (t - mT(si)) + 3 * mXCoefficients(si, 3) * (t - mT(si)) ^ 2
        Dim y As Double = mYCoefficients(si, 1) + 2 * mYCoefficients(si, 2) * (t - mT(si)) + 3 * mYCoefficients(si, 3) * (t - mT(si)) ^ 2
        Dim z As Double = mZCoefficients(si, 1) + 2 * mZCoefficients(si, 2) * (t - mT(si)) + 3 * mZCoefficients(si, 3) * (t - mT(si)) ^ 2
        Return New On3dVector(x, y, z)
    End Function

    Public Function SecondDerivativeAt(ByVal t As Double) As On3dVector
        Dim si As Integer = 0
        For i As Integer = 0 To mT.GetUpperBound(0)
            If t > mT(i) Then si = i
        Next
        If si > mT.GetUpperBound(0) - 1 Then si = mT.GetUpperBound(0) - 1
        Dim x As Double = 2 * mXCoefficients(si, 2) + 6 * mXCoefficients(si, 3) * (t - mT(si))
        Dim y As Double = 2 * mYCoefficients(si, 2) + 6 * mYCoefficients(si, 3) * (t - mT(si))
        Dim z As Double = 2 * mZCoefficients(si, 2) + 6 * mZCoefficients(si, 3) * (t - mT(si))
        Return New On3dVector(x, y, z)
    End Function


    Public Function Domain() As OnInterval
        If mT Is Nothing Then Return Nothing
        If mT.Length < 2 Then Return Nothing
        Return New OnInterval(mT(0), mT(mT.GetUpperBound(0)))
    End Function


    Public Function ConvertToBezierCurveArray() As List(Of OnBezierCurve)
        If mT Is Nothing Then Return Nothing
        If mEndPointsStyle = eEndPointsStyle.periodic Then
            If mT.Length < 3 Then Return Nothing
        Else
            If mT.Length < 2 Then Return Nothing
        End If
        Dim result As New List(Of OnBezierCurve)
        Dim B1 As OnMatrix = Me.GetInvertedBezierBasisMatrix

        Dim cv As New On3dPoint
        Dim S As New OnMatrix(4, 3)
        Dim G As New OnMatrix(4, 3)

        For si As Integer = 0 To mT.GetUpperBound(0) - 1
            Dim c As Double = mT(si + 1) - mT(si)
            For p As Integer = 0 To 3
                S(p, 0) = mXCoefficients(si, 3 - p) * c ^ (3 - p)
                S(p, 1) = mYCoefficients(si, 3 - p) * c ^ (3 - p)
                S(p, 2) = mZCoefficients(si, 3 - p) * c ^ (3 - p)
            Next
            Dim bezier As New OnBezierCurve(3, True, 4)
            G.Multiply(B1, S)
            For i As Integer = 0 To 3
                cv.Set(G(i, 0), G(i, 1), G(i, 2))
                bezier.SetCV(i, cv)
            Next
            result.Add(bezier)
        Next
        cv.Dispose()
        S.Dispose()
        G.Dispose()
        B1.Dispose()
        Return result
    End Function


    Public Function ConvertToPolyCurve() As OnPolyCurve
        Dim result As New OnPolyCurve()
        Dim beziers As List(Of OnBezierCurve) = Me.ConvertToBezierCurveArray
        For i As Integer = 0 To beziers.Count - 1
            Dim nurbsCurve As New OnNurbsCurve
            beziers(i).GetNurbForm(nurbsCurve)
            result.Append(nurbsCurve)
            nurbsCurve.Dispose()
            beziers(i).Dispose()
        Next
        Return result
    End Function


    Public Function ConvertToNurbsCurve() As OnNurbsCurve
        Dim beziers As List(Of OnBezierCurve) = Me.ConvertToBezierCurveArray
        If beziers Is Nothing Then Return Nothing

        Dim result As New OnNurbsCurve(3, False, 4, beziers.Count + 3)
        If beziers.Count = 1 Then
            beziers(0).GetNurbForm(result)
            beziers(0).Dispose()
            Return result
        End If

        Dim cv As New On3dPoint
        Dim cv0 As New On3dPoint
        Dim cv1 As New On3dPoint
        Dim cv2 As New On3dPoint
        Dim cv3 As New On3dPoint
        Dim line0 As New OnLine
        Dim line1 As New OnLine
        Dim a As Double, b As Double

        If mEndPointsStyle = eEndPointsStyle.periodic Then
            'Primo cv
            beziers(beziers.Count - 2).GetCV(1, cv0)
            beziers(beziers.Count - 2).GetCV(2, cv1)
            beziers(beziers.Count - 1).GetCV(1, cv2)
            beziers(beziers.Count - 1).GetCV(2, cv3)
            line0.Create(cv0, cv1)
            line1.Create(cv2, cv3)
            OnUtil.ON_Intersect(line0, line1, a, b)
            cv = line0.PointAt(a)
            result.SetCV(0, cv)

            'Secondo cv
            beziers(beziers.Count - 1).GetCV(1, cv0)
            beziers(beziers.Count - 1).GetCV(2, cv1)
            beziers(0).GetCV(1, cv2)
            beziers(0).GetCV(2, cv3)
            line0.Create(cv0, cv1)
            line1.Create(cv2, cv3)
            OnUtil.ON_Intersect(line0, line1, a, b)
            cv = line0.PointAt(a)
            result.SetCV(1, cv)

            result.SetKnot(0, mT(0) + mT(mT.GetUpperBound(0) - 2) - mT(mT.GetUpperBound(0)))
            result.SetKnot(1, mT(0) + mT(mT.GetUpperBound(0) - 1) - mT(mT.GetUpperBound(0)))
            result.SetKnot(2, mT(0))
        Else
            'Primo cv
            beziers(0).GetCV(0, cv)
            result.SetCV(0, cv)

            'Secondo cv
            beziers(0).GetCV(1, cv)
            result.SetCV(1, cv)

            result.SetKnot(0, mT(0))
            result.SetKnot(1, mT(0))
            result.SetKnot(2, mT(0))
        End If

        For i As Integer = 0 To beziers.Count - 2
            beziers(i).GetCV(1, cv0)
            beziers(i).GetCV(2, cv1)
            beziers(i + 1).GetCV(1, cv2)
            beziers(i + 1).GetCV(2, cv3)
            line0.Create(cv0, cv1)
            line1.Create(cv2, cv3)
            OnUtil.ON_Intersect(line0, line1, a, b)
            cv = line0.PointAt(a)
            result.SetCV(i + 2, cv)
            result.SetKnot(i + 3, mT(i + 1))
        Next

        If mEndPointsStyle = eEndPointsStyle.periodic Then
            'Penultimo cv
            result.GetCV(1, cv)
            result.SetCV(beziers.Count + 1, cv)

            'Ultimo cv
            result.GetCV(2, cv)
            result.SetCV(beziers.Count + 2, cv)

            result.SetKnot(beziers.Count + 2, mT(mT.GetUpperBound(0)))
            result.SetKnot(beziers.Count + 3, mT(mT.GetUpperBound(0)) + mT(1) - mT(0))
            result.SetKnot(beziers.Count + 4, mT(mT.GetUpperBound(0)) + mT(2) - mT(0))
        Else
            'Penultimo cv
            beziers(beziers.Count - 1).GetCV(2, cv)
            result.SetCV(beziers.Count + 1, cv)

            'Ultimo cv
            beziers(beziers.Count - 1).GetCV(3, cv)
            result.SetCV(beziers.Count + 2, cv)
            result.SetKnot(beziers.Count + 2, mT(beziers.Count))
            result.SetKnot(beziers.Count + 3, mT(beziers.Count))
            result.SetKnot(beziers.Count + 4, mT(beziers.Count))
        End If

        cv0.Dispose()
        cv1.Dispose()
        cv2.Dispose()
        cv3.Dispose()
        line0.Dispose()
        line1.Dispose()

        cv.Dispose()
        For i As Integer = 0 To beziers.Count - 1
            beziers(i).Dispose()
        Next
        Return result
    End Function

#End Region


#Region " Private functions "

    Private Function GetBezierBasisMatrix() As OnMatrix
        Dim result As New OnMatrix(4, 4)
        result(0, 0) = -1
        result(0, 1) = 3
        result(0, 2) = -3
        result(0, 3) = 1
        result(1, 0) = 3
        result(1, 1) = -6
        result(1, 2) = 3
        result(1, 3) = 0
        result(2, 0) = -3
        result(2, 1) = 3
        result(2, 2) = 0
        result(2, 3) = 0
        result(3, 0) = 1
        result(3, 1) = 0
        result(3, 2) = 0
        result(3, 3) = 0
        Return result
    End Function

    Private Function GetInvertedBezierBasisMatrix() As OnMatrix
        Dim result As New OnMatrix(4, 4)
        result(0, 0) = 0
        result(0, 1) = 0
        result(0, 2) = 0
        result(0, 3) = 1

        result(1, 0) = 0
        result(1, 1) = 0
        result(1, 2) = 1 / 3
        result(1, 3) = 1

        result(2, 0) = 0
        result(2, 1) = 1 / 3
        result(2, 2) = 2 / 3
        result(2, 3) = 1

        result(3, 0) = 1
        result(3, 1) = 1
        result(3, 2) = 1
        result(3, 3) = 1
        Return result
    End Function


    'La seguente funzione calcola i coefficienti solo nel caso 'natural' evitando la risoluzione del sistema delle curvature
    Private Function ComputeCoefficientsMatrixOld(ByVal pointCoordinates() As Double, ByVal dimension As Integer) As OnMatrix
        If mT Is Nothing OrElse mT.Length < 2 Then Return Nothing
        If pointCoordinates Is Nothing OrElse pointCoordinates.GetLength(0) < 6 Then Return Nothing
        If dimension < 0 Or dimension > 2 Then Return Nothing

        Dim n As Integer = mT.GetUpperBound(0)

        Dim a(n) As Double
        For i As Integer = 0 To n
            a(i) = pointCoordinates(3 * i + dimension)
        Next

        Dim b(n - 1) As Double
        Dim d(n - 1) As Double

        Dim h(n - 1) As Double
        For i As Integer = 0 To n - 1
            h(i) = mT(i + 1) - mT(i)
        Next

        Dim alfa(n - 1) As Double       'alfa(0) non è utilizzato
        For i As Integer = 1 To n - 1
            alfa(i) = 3 / h(i) * (a(i + 1) - a(i)) - 3 / h(i - 1) * (a(i) - a(i - 1))
        Next

        Dim c(n) As Double
        Dim l(n) As Double
        Dim mi(n) As Double
        Dim z(n) As Double

        l(0) = 1 : mi(0) = 0 : z(0) = 0
        For i As Integer = 1 To n - 1
            l(i) = 2 * (mT(i + 1) - mT(i - 1)) - h(i - 1) * mi(i - 1)
            mi(i) = h(i) / l(i)
            z(i) = (alfa(i) - h(i - 1) * z(i - 1)) / l(i)
        Next
        l(n) = 1 : z(n) = 0 : c(n) = 0

        For j As Integer = n - 1 To 0 Step -1
            c(j) = z(j) - mi(j) * c(j + 1)
            b(j) = (a(j + 1) - a(j)) / h(j) - h(j) * (c(j + 1) + 2 * c(j)) / 3
            d(j) = (c(j + 1) - c(j)) / (3 * h(j))
        Next

        Dim result As New OnMatrix(n, 4)
        For i As Integer = 0 To n - 1
            result(i, 0) = a(i)
            result(i, 1) = b(i)
            result(i, 2) = c(i)
            result(i, 3) = d(i)
        Next
        Return result
    End Function


    Private Function ComputeCoefficientsMatrix(ByVal pointCoordinates() As Double, ByVal dimension As Integer) As OnMatrix
        If mT Is Nothing OrElse mT.Length < 2 Then Return Nothing
        If pointCoordinates Is Nothing OrElse pointCoordinates.GetLength(0) < 6 Then Return Nothing
        If dimension < 0 Or dimension > 2 Then Return Nothing

        Dim n As Integer = mT.GetLength(0)  'numero di punti

        Dim y(n - 1) As Double
        For i As Integer = 0 To n - 1
            y(i) = pointCoordinates(3 * i + dimension)
        Next

        Dim h(n - 2) As Double
        For i As Integer = 0 To n - 2
            h(i) = mT(i + 1) - mT(i)
        Next

        'Dim matrix As New OnMatrix(n - 2, n - 2)  'Non metto nella matrice la prima e l'ultima equazione
        Dim matrix As New RhMatrice(n - 3, n - 3)
        For i As Integer = 0 To n - 3
            If i > 0 Then matrix(i, i - 1) = h(i)
            matrix(i, i) = 2 * (h(i) + h(i + 1))
            If i < n - 3 Then matrix(i, i + 1) = h(i + 1)
        Next
        Select Case mEndPointsStyle
            Case eEndPointsStyle.natural
                'Nothing ToDo
            Case eEndPointsStyle.parabolic
                matrix(0, 0) += h(0)
                matrix(n - 3, n - 3) += h(n - 2)
            Case eEndPointsStyle.cubic
                matrix(0, 0) += 2 * h(0)
                matrix(0, 1) -= h(0)
                matrix(n - 3, n - 4) -= h(n - 2)
                matrix(n - 3, n - 3) += 2 * h(n - 2)
        End Select

        Dim q(n - 3) As Double
        For i As Integer = 0 To n - 3
            q(i) = 6 * ((y(i + 2) - y(i + 1)) / h(i + 1) - (y(i + 1) - y(i)) / h(i))
        Next

        Dim X(n - 3) As Double

        Dim pivot As Double
        matrix.Riduci(0, q, pivot)
        matrix.Risolvi(0, q, X)

        Dim m(n - 1) As Double
        For i As Integer = 0 To n - 3
            m(i + 1) = X(i)
        Next
        Select Case mEndPointsStyle
            Case eEndPointsStyle.natural
                m(0) = 0
                m(n - 1) = 0
            Case eEndPointsStyle.parabolic
                m(0) = m(1)
                m(n - 1) = m(n - 2)
            Case eEndPointsStyle.cubic
                m(0) = 2 * m(1) - m(2)
                m(n - 1) = 2 * m(n - 2) - m(n - 3)
        End Select

        Dim result As New OnMatrix(n - 1, 4)
        For i As Integer = 0 To n - 2
            result(i, 0) = y(i)
            result(i, 1) = (y(i + 1) - y(i)) / h(i) - (m(i + 1) + 2 * m(i)) * h(i) / 6
            result(i, 2) = m(i) / 2
            result(i, 3) = (m(i + 1) - m(i)) / (6 * h(i))
        Next
        Return result
    End Function

    Private Function ComputePeriodicCoefficientsMatrix(ByVal pointCoordinates() As Double, ByVal dimension As Integer) As OnMatrix
        If mT Is Nothing OrElse mT.Length < 2 Then Return Nothing
        If pointCoordinates Is Nothing OrElse pointCoordinates.GetLength(0) < 6 Then Return Nothing
        If dimension < 0 Or dimension > 2 Then Return Nothing

        Dim n As Integer = mT.GetLength(0)  'numero di punti

        Dim y(n - 1) As Double
        For i As Integer = 0 To n - 1
            y(i) = pointCoordinates(3 * i + dimension)
        Next

        Dim h(n - 2) As Double
        For i As Integer = 0 To n - 2
            h(i) = mT(i + 1) - mT(i)
        Next

        Dim matrix As New RhMatrice(n - 1, n - 1)
        For i As Integer = 0 To n - 3
            matrix(i + 1, i) = h(i)
            matrix(i + 1, i + 1) = 2 * (h(i) + h(i + 1))
            matrix(i + 1, i + 2) = h(i + 1)
        Next
        'Condizione di tangenza
        matrix(0, n - 2) = h(n - 2)
        matrix(0, 0) = 2 * (h(n - 2) + h(0))
        matrix(0, 1) = h(0)
        'Condizione di continuità in curvatura
        matrix(n - 1, 0) = 1
        matrix(n - 1, n - 1) = -1

        Dim q(n - 1) As Double
        q(0) = 6 * ((y(1) - y(0)) / h(0) - (y(n - 1) - y(n - 2)) / h(n - 2))
        For i As Integer = 0 To n - 3
            q(i + 1) = 6 * ((y(i + 2) - y(i + 1)) / h(i + 1) - (y(i + 1) - y(i)) / h(i))
        Next
        q(n - 1) = 0

        Dim m(n - 1) As Double

        Dim pivot As Double
        matrix.Riduci(0, q, pivot)
        matrix.Risolvi(0, q, m)

        Dim result As New OnMatrix(n - 1, 4)
        For i As Integer = 0 To n - 2
            result(i, 0) = y(i)
            result(i, 1) = (y(i + 1) - y(i)) / h(i) - (m(i + 1) + 2 * m(i)) * h(i) / 6
            result(i, 2) = m(i) / 2
            result(i, 3) = (m(i + 1) - m(i)) / (6 * h(i))
        Next
        Return result
    End Function

#End Region


#Region " IDisposable Support "

    Private disposedValue As Boolean = False        ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                ' TODO: free managed resources when explicitly called
                If Not mXCoefficients Is Nothing Then mXCoefficients.Dispose()
                If Not mYCoefficients Is Nothing Then mYCoefficients.Dispose()
                If Not mZCoefficients Is Nothing Then mZCoefficients.Dispose()
            End If

            ' TODO: free shared unmanaged resources
        End If
        Me.disposedValue = True
    End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

#End Region

End Class


