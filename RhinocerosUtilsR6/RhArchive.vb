Imports RMA.Rhino
Imports RMA.OpenNURBS


'*********************************************************************************************
'*** Interfaccia da implementare per le proprie classi dati da serializzare nel file 3dm   ***
'*** Prendere in considerazione la classe RhArchive nelle RhinocerosUtils per semplificare ***
'*** BISOGNA PORTARLA NEI PROGETTI CHE LA IMPLEMENTANO ALTRIMENTI I TEST FALLISCONO        ***
'*********************************************************************************************

'Public Interface IOnSerializable

'    Function Serialize(ByRef archive As OnBinaryArchive) As Boolean

'    Function Deserialize(ByRef archive As OnBinaryArchive) As Boolean

'End Interface



'*****************************************************************
'*** Classe di supporto alla serializzazione dati di un PlugIn ***
'*****************************************************************

Public Class RhArchive


    ''' <summary>
    ''' Ritorna l'elenco dei campi privati e pubblici di una classe
    ''' </summary>
    ''' <param name="classToAnalayze"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function GetAllFields(ByVal classToAnalayze As Object) As Reflection.FieldInfo()
        Try
            'Mergio i campi PUBLIC e PRIVATE
            Dim fields() As Reflection.FieldInfo = classToAnalayze.GetType.GetFields(Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic)
            Dim publicFields() As Reflection.FieldInfo = classToAnalayze.GetType.GetFields(Reflection.BindingFlags.Instance Or Reflection.BindingFlags.Public)
            Dim index As Integer = fields.GetUpperBound(0)
            ReDim Preserve fields(fields.GetUpperBound(0) + publicFields.Length)
            For i As Integer = 0 To publicFields.Length - 1
                fields(index + i + 1) = publicFields(i)
            Next
            Return fields
        Catch ex As Exception
            Return Nothing
        End Try
    End Function



    ''' <summary>
    ''' Serializza una classe
    ''' </summary>
    ''' <param name="archive"></param>
    ''' <param name="classToSerialize"></param>
    ''' <param name="fieldNotIncluded">E' una lista nella quale includere in nomi dei campi da non serializzare</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function SerializeClass(ByRef archive As OnBinaryArchive, ByVal classToSerialize As Object, Optional ByVal fieldNotIncluded As List(Of String) = Nothing) As Boolean
        Dim fields() As Reflection.FieldInfo = RhArchive.GetAllFields(classToSerialize)
        If fieldNotIncluded Is Nothing Then fieldNotIncluded = New List(Of String) 'Se è Nothing la istanzio a lita vuota per evitare errori
        Try
            ' *** HEADER ***
            'Serializzo il numero dei campi
            archive.WriteInt(fields.Length)
            'Serializza una lista di [boolean]che indica se un campo è o no Nothing 
            For i As Integer = 0 To fields.Length - 1
                archive.WriteBool(fields(i).GetValue(classToSerialize) Is Nothing)
            Next
            ' *** VALORI ***
            'Eseguo la serializzazione su campi
            For i As Integer = 0 To fields.Length - 1
                Dim field As Reflection.FieldInfo = fields(i)
                'Verifico che non sia tra i campi da non serializzare e che non sia un campo COSTANTE
                If Not fieldNotIncluded.Contains(field.Name) AndAlso (Not field.IsLiteral) Then
                    If Not SerializeField(archive, classToSerialize, field) Then                        
                        Debug.Print("Errore nella serializzazione del campo '" & field.Name & "'")
                        Return False
                    End If
                End If
            Next
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function



    ''' <summary>
    ''' Deserializza una classe
    ''' </summary>
    ''' <param name="archive"></param>
    ''' <param name="classToDeserialize"></param>
    ''' <param name="fieldNotIncluded">E' una lista nella quale includere in nomi dei campi da non deserializzare</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function DeserializeClass(ByRef archive As OnBinaryArchive, ByVal classToDeserialize As Object, Optional ByVal fieldNotIncluded As List(Of String) = Nothing) As Boolean
        If fieldNotIncluded Is Nothing Then fieldNotIncluded = New List(Of String) 'Se è Nothing la istanzio a lita vuota per evitare errori
        Dim fields() As Reflection.FieldInfo = RhArchive.GetAllFields(classToDeserialize)
        Try
            ' *** HEADER ***
            'Deserializzo il numero dei campi
            Dim fieldCount As Integer = -1
            archive.ReadInt(fieldCount)
            'Deserializza l'array di [boolean] che indica se un campo è o no Nothing 
            Dim arrayFieldNothing(fieldCount - 1) As Boolean
            For i As Integer = 0 To fieldCount - 1
                archive.ReadBool(arrayFieldNothing(i))
            Next
            ' *** VALORI ***
            'Eseguo la Deserializzazione su campi
            For i As Integer = 0 To fields.Length - 1
                Dim field As Reflection.FieldInfo = fields(i)
                'Se è una campo non contenuto nell'elenco dei campi da non deserializzare e se è un campo costante allor lo deserializzo
                If Not fieldNotIncluded.Contains(field.Name) And (Not field.IsLiteral) Then
                    If arrayFieldNothing(i) Then
                        'Verifico che il campo non sia NOTHING; in tal caso lo imposto a Nothing 
                        Try
                            Dim tmpString As String = ""
                            archive.ReadString(tmpString)
                            If (tmpString = "NOTHING") Then field.SetValue(classToDeserialize, Nothing)
                        Catch ex2 As Exception
                            Debug.Print("Errore nella deserializzazione del campo di valore NOTHING e di nome '" & field.Name & "'")
                            Return False
                        End Try
                    Else
                        'Se non è NOTHING lo deserializzo correttamente
                        If Not DeserializeField(archive, classToDeserialize, field) Then
                            Debug.Print("Errore nella deserializzazione del campo '" & field.Name & "'")
                            Return False
                        End If
                    End If
                End If
            Next
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function




    ''' <summary>
    ''' Serializza un campo di una classe
    ''' </summary>
    ''' <param name="archive"></param>
    ''' <param name="classToSerialize"></param>
    ''' <param name="field"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function SerializeField(ByRef archive As OnBinaryArchive, ByVal classToSerialize As Object, ByVal field As Reflection.FieldInfo) As Boolean
        Try
            'Se è Nothing il valore del campo, per evitare errori nella serializzazione, scrivo una stringa "NOTHING" come segnaposto
            If (field.GetValue(classToSerialize) Is Nothing) Then
                archive.WriteString("NOTHING")
                Return True
            End If
            'Altrimenti guardo il tipo ed esego la serializzazione con il metodo opportuno
            Select Case field.FieldType.ToString
                Case GetType(String).ToString
                    archive.WriteString(DirectCast(field.GetValue(classToSerialize), String))
                Case GetType(Double).ToString
                    archive.WriteDouble(DirectCast(field.GetValue(classToSerialize), Double))
                Case GetType(Integer).ToString
                    archive.WriteInt(DirectCast(field.GetValue(classToSerialize), Integer))
                Case GetType(Boolean).ToString
                    archive.WriteBool(DirectCast(field.GetValue(classToSerialize), Boolean))
                Case GetType(On3dPoint).ToString, GetType(IOn3dPoint).ToString
                    archive.WritePoint(DirectCast(field.GetValue(classToSerialize), On3dPoint))
                Case GetType(OnLine).ToString, GetType(IOnLine).ToString
                    archive.WriteLine(DirectCast(field.GetValue(classToSerialize), OnLine))
                Case GetType(On3dVector).ToString, GetType(IOn3dVector).ToString
                    archive.WriteVector(DirectCast(field.GetValue(classToSerialize), On3dVector))
                Case GetType(OnPlane).ToString, GetType(IOnPlane).ToString
                    archive.WritePlane(DirectCast(field.GetValue(classToSerialize), OnPlane))

                    '### NOTA ANDREA FINAURINI ### 
                    'Ci sono sicuramente altri CASE specifici da aggiugnere 
                    ' TO_DO...

                Case Else
                    archive.WriteObject(DirectCast(field.GetValue(classToSerialize), OnObject))
            End Select
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function



    ''' <summary>
    ''' Deserializza un campo
    ''' </summary>
    ''' <param name="archive"></param>
    ''' <param name="classToDeserialize"></param>
    ''' <param name="field"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function DeserializeField(ByRef archive As OnBinaryArchive, ByVal classToDeserialize As Object, ByVal field As Reflection.FieldInfo) As Boolean
        Try
            'Cerco il tipo del campo e lo deserializzo
            Select Case field.FieldType.ToString
                Case GetType(String).ToString
                    Dim tmpString As String = ""
                    archive.ReadString(tmpString)
                    field.SetValue(classToDeserialize, tmpString)
                Case GetType(Double).ToString
                    Dim tmpDouble As Double
                    archive.ReadDouble(tmpDouble)
                    field.SetValue(classToDeserialize, tmpDouble)
                Case GetType(Integer).ToString
                    Dim tmpInteger As Integer
                    archive.ReadInt(tmpInteger)
                    field.SetValue(classToDeserialize, tmpInteger)
                Case GetType(Boolean).ToString
                    Dim tmpBoolean As Boolean
                    archive.ReadBool(tmpBoolean)
                    field.SetValue(classToDeserialize, tmpBoolean)
                Case GetType(On3dPoint).ToString, GetType(IOn3dPoint).ToString
                    Dim tmpOn3dPoint As New On3dPoint
                    archive.ReadPoint(tmpOn3dPoint)
                    field.SetValue(classToDeserialize, tmpOn3dPoint)
                Case GetType(OnLine).ToString, GetType(IOnLine).ToString
                    Dim tmpOnLine As New OnLine
                    archive.ReadLine(tmpOnLine)
                    field.SetValue(classToDeserialize, tmpOnLine)
                Case GetType(On3dVector).ToString, GetType(IOn3dVector).ToString
                    Dim tmpOn3dVector As New On3dVector
                    archive.ReadVector(tmpOn3dVector)
                    field.SetValue(classToDeserialize, tmpOn3dVector)
                Case GetType(OnPlane).ToString, GetType(IOnPlane).ToString
                    Dim tmpOnPlane As New OnPlane
                    archive.ReadPlane(tmpOnPlane)
                    field.SetValue(classToDeserialize, tmpOnPlane)

                    '### NOTA ANDREA FINAURINI ### 
                    'Ci sono sicuramenti altri CASE specifici da aggiugnere 
                    ' TO_DO...

                Case Else
                    Dim tmpOnObject As OnObject = Nothing
                    'Correzione per i campi Interfacce
                    Dim classTypeName As String = field.FieldType.Name
                    If field.FieldType.Name.StartsWith("I") Then classTypeName = field.FieldType.Name.Substring(1, field.FieldType.Name.Length - 1)
                    'Correzione per le OnCurve che non posso essere istanziate
                    Dim classType As Type = field.FieldType
                    Select Case classTypeName
                        Case "OnCurve"
                            classTypeName = "OnNurbsCurve"
                            classType = Type.GetType("RMA.OpenNURBS." & classTypeName & ", Rhino_DotNet, Version=4.0.61206.0, Culture=neutral, PublicKeyToken=552281e97c755530")
                    End Select
                    tmpOnObject = DirectCast(Activator.CreateInstance(classType), OnObject)
                    archive.ReadObject(tmpOnObject)
                    field.SetValue(classToDeserialize, tmpOnObject)
            End Select
            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function


End Class
