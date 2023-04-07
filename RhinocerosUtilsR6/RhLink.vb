

'*******************************************************************************************
'*** Classe per la gestione del congiungimento di punti. E' utilizzata da altre funzioni ***
'*******************************************************************************************

Public Class RhLink
    Implements IComparable

    Public Indice0 As Integer
    Public Indice1 As Integer
    Public Distanza As Double

    Public Sub New(ByVal index0 As Integer, ByVal index1 As Integer, ByVal distance As Double)
        Indice0 = index0
        Indice1 = index1
        Distanza = distance
    End Sub

    Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo
        Return Me.Distanza.CompareTo(DirectCast(obj, RhLink).Distanza)
    End Function

End Class



Public Class RhLinks
    Inherits CollectionBase

    Default Public Property Item(ByVal indice As Integer) As RhLink
        Get
            Return DirectCast(Me.List.Item(indice), RhLink)
        End Get
        Set(ByVal link As RhLink)
            Me.List.Item(indice) = link
        End Set
    End Property

    Public Sub Add(ByVal nuovoLink As RhLink)
        Me.List.Add(nuovoLink)
    End Sub

    Public Sub Sort()
        Me.InnerList.Sort()
    End Sub

    Public Function RaggruppaPunti(ByVal indicePuntoPartenza As Integer) As ArrayList
        Dim valutato(Me.Count - 1) As Boolean
        For i As Integer = 0 To valutato.GetUpperBound(0)
            valutato(i) = False
        Next
        Dim percorso As New ArrayList
        percorso.Add(indicePuntoPartenza)
        Propagazione(valutato, indicePuntoPartenza, percorso)
        percorso.Sort()
        Dim res As New ArrayList
        Dim ultimoIndice As Integer = -1
        For i As Integer = 0 To percorso.Count - 1
            If CInt(percorso(i)) > ultimoIndice Then
                res.Add(percorso(i))
                ultimoIndice = CInt(percorso(i))
            End If
        Next
        Return res
    End Function


    Private Sub Propagazione(ByVal valutato() As Boolean, ByVal indiceInizio As Integer, ByRef percorso As ArrayList)
        For i As Integer = 0 To Me.Count - 1
            Dim legame As RhLink = Me(i)
            If legame.Indice0 = indiceInizio Then
                If Not valutato(i) Then
                    valutato(i) = True
                    percorso.Add(legame.Indice1)
                    Propagazione(valutato, legame.Indice1, percorso)
                End If
            ElseIf legame.Indice1 = indiceInizio Then
                If Not valutato(i) Then
                    valutato(i) = True
                    percorso.Add(legame.Indice0)
                    Propagazione(valutato, legame.Indice0, percorso)
                End If
            End If
        Next
    End Sub

End Class
