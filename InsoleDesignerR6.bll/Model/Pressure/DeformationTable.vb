Imports RMA.OpenNURBS


Public Class DeformationTable


  Public Property Name As String  
  Private intervalValues As List(Of Tuple(Of OnInterval, Double))


  Public Sub New ()
    intervalValues = New List(Of Tuple(Of OnInterval, Double))
  End Sub

  ''' <summary>
  ''' Dato il valore di pressione ritorna la deformazione in base alla tabella selezionata
  ''' </summary>
  ''' <param name="pressureVal"></param>
  ''' <param name="continueVal">Se TRUE calcola valori lienari rispetto agli estremi dell'intervallo altrimenti ritorna valori discreti</param>
  ''' <returns></returns>
  Public Function GetDeformation(pressureVal As Double, Optional ByVal continueVal As Boolean = True) As Double
    If continueVal Then
      Dim selectedTuple As Tuple(Of OnInterval, Double) = Nothing
      For Each tuple As Tuple(Of OnInterval, Double) In intervalValues
        If pressureVal >= tuple.Item1.Min() And pressureVal < tuple.Item1.Max() Then
          selectedTuple = tuple
        End If
      Next
      If selectedTuple Is Nothing Then Return GetDeformationOutOfInterval(pressureVal)
      If Math.Abs(selectedTuple.Item2 - 0) < 0.00001 Then Return 0
      Dim prevTuple = intervalValues.Where(Function(x) x.Item1.Max() < selectedTuple.Item1.Max()).OrderByDescending(Function(x) x.Item1.Max()).Take(1).FirstOrDefault()
      Dim nextTuple = intervalValues.Where(Function(x) x.Item1.Max() > selectedTuple.Item1.Max()).OrderBy(Function(x) x.Item1.Max()).Take(1).FirstOrDefault()
      If prevTuple Is Nothing And nextTuple Is Nothing Then Return GetDeformationOutOfInterval(pressureVal)
      Dim gapMinMax = selectedTuple.Item1.Max() - selectedTuple.Item1.Min()
      Dim deformation = selectedTuple.Item2
      Dim gapPressure As Double = 0
      Dim prevVal As Double = 0
      If prevTuple Is Nothing Then
        gapPressure = nextTuple.Item2 - deformation
      Else
        gapPressure = deformation - prevTuple.Item2
        prevVal = prevTuple.Item2
      End If
      Dim intervalIncrement = gapPressure / gapMinMax
      Return prevVal + (pressureVal - selectedTuple.Item1.Min) * intervalIncrement
    Else
      For Each tuple As Tuple(Of OnInterval, Double) In intervalValues
        If pressureVal >= tuple.Item1.Min() And pressureVal < tuple.Item1.Max() Then
          Return tuple.Item2
        End If
      Next
      Return GetDeformationOutOfInterval(pressureVal)
    End If
  End Function

  ''' <summary>
  ''' Prendo la tupla con limite massimo, se è maggiore torno massima deformazione altrimenti zero
  ''' </summary>    
  Private Function GetDeformationOutOfInterval(pressureVal As Double) As Double
    Dim maxInterval = intervalValues.OrderBy(Function(x) x.Item1.Max()).FirstOrDefault()
    If pressureVal > maxInterval.Item1.Max() Then
      Return maxInterval.Item2
    Else
      Return 0
    End If
  End Function

  Public Sub AddInterval(min As Double, max As Double, value As Double)   
    Dim interval = New OnInterval(min, max) 
    intervalValues.Add(New Tuple(Of OnInterval,Double)(interval, value))
  End Sub


  Public ReadOnly Property GetDataSource() As List(Of DeformDTO)
    Get
      Return (From row In intervalValues Select New DeformDTO With {
        .MinP = row.Item1.Min(),
        .MaxP = row.Item1.Max(),
        .Deformation = row.Item2
        }).ToList()
    End Get
  End Property


  Public class DeformDTO    
        Public Property MinP As String 
        Public Property MaxP As String 
        Public Property Deformation As String 
   End Class

End Class
