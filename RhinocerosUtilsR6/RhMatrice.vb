Imports System.Math


''' <summary>
''' Calsse per la gestione di operazioni comuni con le matrici
''' </summary>
''' <remarks></remarks>
<Serializable()> Public Class RhMatrice

    Dim m(,) As Double


    Public Sub New(ByVal ultimoIndiceRighe As Integer, ByVal ultimoIndiceCol As Integer)
        ReDim m(ultimoIndiceRighe, ultimoIndiceCol)
    End Sub

    Public Sub New(ByVal other As RhMatrice)
        ReDim m(other.NumeroRighe - 1, other.NumeroColonne - 1)
        For i As Integer = 0 To other.NumeroRighe - 1
            For j As Integer = 0 To other.NumeroColonne - 1
                Me.m(i, j) = other(i, j)
            Next
        Next
    End Sub

    Default Public Property Item(ByVal i As Integer, ByVal j As Integer) As Double
        Get
            Return m(i, j)
        End Get
        Set(ByVal Value As Double)
            m(i, j) = Value
        End Set
    End Property

    Public ReadOnly Property NumeroRighe() As Integer
        Get
            Return m.GetUpperBound(0) + 1
        End Get
    End Property

    Public ReadOnly Property NumeroColonne() As Integer
        Get
            Return m.GetUpperBound(1) + 1
        End Get
    End Property

    Public ReadOnly Property MinCount() As Integer
        Get
            If m.GetUpperBound(0) <= m.GetUpperBound(1) Then Return m.GetUpperBound(0) + 1
            Return m.GetUpperBound(1) + 1
        End Get
    End Property


    Private Function SwapRows(ByVal row0 As Integer, ByVal row1 As Integer) As Boolean
        Dim b As Boolean = False
        If 0 <= row0 And row0 <= m.GetUpperBound(0) And 0 <= row1 And row1 <= m.GetUpperBound(0) Then
            If row0 <> row1 Then
                Dim temp As Double
                For j As Integer = 0 To m.GetUpperBound(1)
                    temp = m(row0, j)
                    m(row0, j) = m(row1, j)
                    m(row1, j) = temp
                Next
            End If
            b = True
        End If
        Return b
    End Function

    Private Function SwapCols(ByVal col0 As Integer, ByVal col1 As Integer) As Boolean
        Dim b As Boolean = False
        If 0 <= col0 And col0 <= m.GetUpperBound(1) And 0 <= col1 And col1 <= m.GetUpperBound(1) Then
            If col0 <> col1 Then
                Dim temp As Double
                For i As Integer = 0 To m.GetUpperBound(0)
                    temp = m(i, col0)
                    m(i, col0) = m(i, col1)
                    m(i, col1) = temp
                Next
            End If
            b = True
        End If
        Return b
    End Function

    Private Sub RowScale(ByVal dest_row As Integer, ByVal s As Double)
        For j As Integer = 0 To m.GetUpperBound(1)
            m(dest_row, j) *= s
        Next
    End Sub

    Private Sub RowOp(ByVal dest_row As Integer, ByVal s As Double, ByVal src_row As Integer)
        For j As Integer = 0 To m.GetUpperBound(1)
            m(dest_row, j) += s * m(src_row, j)
        Next
    End Sub

    Public Sub Zero()
        For i As Integer = 0 To Me.NumeroRighe - 1
            For j As Integer = 0 To Me.NumeroColonne - 1
                m(i, j) = 0
            Next
        Next
    End Sub

    Public Sub SetDiagonal(ByVal d As Double)
        Dim n As Integer = MinCount
        Zero()
        For i As Integer = 0 To n - 1
            m(i, i) = d
        Next
    End Sub


    Public Function Riduci(ByVal zeroTolerance As Double, ByRef B() As Double, ByRef pivot As Double) As Integer
        Dim t As Double
        Dim x, piv As Double
        Dim i, j, k, ix, rank As Integer
        piv = 0.0
        rank = 0
        Dim n As Integer = CInt(IIf(m.GetUpperBound(0) <= m.GetUpperBound(1), m.GetUpperBound(0), m.GetUpperBound(1)))

        For k = 0 To n
            ix = k
            x = Abs(Me.m(ix, k))
            For i = k + 1 To Me.m.GetUpperBound(0)
                If Abs(Me.m(i, k)) > x Then
                    ix = i
                    x = Abs(Me.m(ix, k))
                End If
            Next
            If (x < piv Or k = 0) Then piv = x
            If (x <= zeroTolerance) Then Exit For
            rank = rank + 1

            'Scambia righe della matrice e B
            Me.SwapRows(ix, k)
            t = B(ix) : B(ix) = B(k) : B(k) = t

            'Prodotto scalare della riga k della matrice e B
            x = 1.0 / m(k, k)
            m(k, k) = 1.0
            For j = k + 1 To Me.m.GetUpperBound(0)
                m(k, j) = x * m(k, j)
            Next
            B(k) = x * B(k)

            ' Azzera la colonna k per le righe sotto m(k,k)
            For i = k + 1 To m.GetUpperBound(0)
                x = -m(i, k)
                m(i, k) = 0.0
                If Abs(x) > zeroTolerance Then
                    For j = k + 1 To Me.m.GetUpperBound(0)
                        m(i, j) = x * m(k, j) + m(i, j)
                    Next
                    B(i) = B(i) + x * B(k)
                End If
            Next
        Next

        pivot = piv

        Return rank

    End Function

    Public Function Risolvi(ByVal zeroTolerance As Double, ByVal B() As Double, ByRef X() As Double) As Boolean
        Dim i, j As Integer
        Dim s As Double = 0
        If m.GetUpperBound(1) > m.GetUpperBound(0) Then
            Return False     'sotto determinata
        End If
        If B.GetUpperBound(0) < Me.m.GetUpperBound(1) Or B.GetUpperBound(0) > Me.m.GetUpperBound(0) Then
            Return False     'sotto determinata
        End If
        For i = m.GetUpperBound(1) + 1 To B.GetUpperBound(0)
            If Abs(B(i)) > zeroTolerance Then
                Return False        ' Sovra determinata
            End If
        Next

        'Risolvi
        Dim n As Integer = m.GetUpperBound(1)
        If Not X Is B Then
            X(n) = B(n)
        End If
        For i = n - 1 To 0 Step -1
            s = 0
            For j = i + 1 To n
                s = s + m(i, j) * X(j)
            Next
            X(i) = B(i) - s
        Next
        Return True
    End Function


    Public Function TrasformaMatrice(ByVal Rotazione As RhMatrice) As RhMatrice
        If Rotazione.NumeroRighe <> Rotazione.NumeroColonne Then Return Nothing '<-- Verifica che Rotazione sia quadrata
        If Rotazione.NumeroRighe <> Me.NumeroRighe Then Return Nothing '<-- Verifica che le righe di Rotazione sono le stesse di matrice
        Dim ultimoIndice As Integer = Rotazione.NumeroRighe - 1

        'ESEGUE IL PRODOTTO TRA LA MATRICE DI RIGIDEZZA NEL RIF. LOCALE E LA MATRICE DI ROTAZIONE
        Dim Q(ultimoIndice, ultimoIndice) As Double
        For i As Integer = 0 To ultimoIndice
            For j As Integer = 0 To ultimoIndice
                For t As Integer = 0 To ultimoIndice
                    Q(i, j) = Q(i, j) + m(i, t) * Rotazione(t, j)
                Next
            Next
        Next

        'ESEGUE IL PRODOTTO TRA LA MATRICE DI ROTAZIONE TRASPOSTA E LA MATRICE RISULTANTE DAL PRODOTTO PRECEDENTE 
        Dim S As New RhMatrice(ultimoIndice, ultimoIndice)
        For i As Integer = 0 To ultimoIndice
            For j As Integer = 0 To ultimoIndice
                For t As Integer = 0 To ultimoIndice
                    S(i, j) = S(i, j) + Rotazione(t, i) * Q(t, j)       ' ---> la Rotazione viene trasposta
                Next
            Next
        Next
        Return S
    End Function

    Public Shared Function Prodotto(ByVal A As RhMatrice, ByVal B As RhMatrice) As RhMatrice
        If A Is Nothing Or B Is Nothing Then Return Nothing
        If A.NumeroRighe < 1 Or A.NumeroColonne < 1 Then Return Nothing
        If B.NumeroRighe < 1 Or B.NumeroColonne < 1 Then Return Nothing
        If A.NumeroColonne <> B.NumeroRighe Then Return Nothing

        Dim result As New RhMatrice(A.NumeroRighe - 1, B.NumeroColonne - 1)
        For i As Integer = 0 To A.NumeroRighe - 1
            For j As Integer = 0 To B.NumeroColonne - 1
                result(i, j) = 0.0
                For s As Integer = 0 To A.NumeroColonne - 1
                    result(i, j) += A(i, s) * B(s, j)
                Next
            Next
        Next
        Return result
    End Function


    Public Function Transpose() As Boolean
        Dim rc As Boolean = False
        Dim row_count As Integer = Me.NumeroRighe
        Dim col_count As Integer = Me.NumeroColonne
        If row_count > 0 And col_count > 0 Then
            rc = True
            If row_count = col_count Then
                Dim t As Double
                For i As Integer = 0 To row_count - 1
                    For j As Integer = i + 1 To row_count - 1
                        t = m(i, j)
                        m(i, j) = m(j, i)
                        m(j, i) = t
                    Next
                Next
            Else
                Dim A As New RhMatrice(Me)
                ReDim m(col_count - 1, row_count - 1)
                For i As Integer = 0 To row_count - 1
                    For j As Integer = 0 To col_count - 1
                        m(j, i) = A(i, j)
                    Next
                Next
            End If
        End If
        Return rc
    End Function

    Public Function Invert(ByVal zero_tolerance As Double) As Boolean

        Dim i As Integer, j As Integer, k As Integer, ix As Integer, jx As Integer, rank As Integer
        Dim x As Double
        Dim n As Integer = MinCount
        If n < 1 Then Return False

        Dim inversa As New RhMatrice(Me.NumeroColonne - 1, Me.NumeroRighe - 1)
        Dim col(n - 1) As Integer
        inversa.SetDiagonal(1.0)
        rank = 0

        For k = 0 To n - 1
            '// find largest value in sub matrix
            ix = k : jx = k
            x = Abs(m(ix, jx))
            For i = k To n - 1
                For j = k To n - 1
                    If Abs(m(i, j)) > x Then
                        ix = i
                        jx = j
                        x = Abs(m(ix, jx))
                    End If
                Next
            Next
            SwapRows(k, ix)
            inversa.SwapRows(k, ix)

            SwapCols(k, jx)
            col(k) = jx

            If x <= zero_tolerance Then Exit For

            x = 1.0 / m(k, k)
            m(k, k) = 1.0

            '  ON_ArrayScale( m_col_count-k-1, x, &this_m[k][k+1], &this_m[k][k+1] );
            For jj As Integer = 0 To m.GetUpperBound(1) - k - 1
                m(k, k + 1 + jj) *= x
            Next
            inversa.RowScale(k, x)

            '// zero m(<>k,k)'s 
            For i = 0 To n - 1
                If i <> k Then
                    x = -m(i, k)
                    m(i, k) = 0.0
                    If Abs(x) > zero_tolerance Then
                        'ON_Array_aA_plus_B( m_col_count-k-1, x, &this_m[k][k+1], &this_m[i][k+1], &this_m[i][k+1] );
                        For jj As Integer = 0 To m.GetUpperBound(1) - k - 1
                            m(i, k + 1 + jj) = x * m(k, k + 1 + jj) + m(i, k + 1 + jj)
                        Next
                        inversa.RowOp(i, x, k)
                    End If
                End If
            Next
        Next

        '// take care of column swaps
        For i = k - 1 To 0 Step -1
            If i <> col(i) Then
                inversa.SwapRows(i, col(i))
            End If
        Next

        m = inversa.m
        Return (k = n)
    End Function

    ''' <summary>
    ''' Definita come: Inv(At * A) * At
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function InversaMoorePenrose() As RhMatrice
        Dim trasposta As New RhMatrice(Me)
        trasposta.Transpose()

        Dim result As RhMatrice = RhMatrice.Prodotto(trasposta, Me)
        result.Invert(0.00000000001)
        result = RhMatrice.Prodotto(result, trasposta)
        Return result
    End Function

End Class
