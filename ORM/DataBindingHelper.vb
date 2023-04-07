Imports System.Windows.Forms
Imports System.Reflection
Imports ORM.IdDataSet
Imports ORM.IdDataSetTableAdapters
Imports System.Collections.ObjectModel
Imports ORM


Public Class DataBindingHelper



#Region " Utils "


    ''' <summary>
    ''' Associa le righe di un data table ad una combobox
    ''' </summary>
    ''' <param name="comboBox">Controllo grafico sul quale eseguire il binding dei dati</param>
    ''' <param name="displayMember">Nome del campo(colonna) dalla quale leggere le stringhe da visualizzare</param>
    ''' <param name="valueMember">Nome del campo(colonna) dalla quale leggere i valori da assegnare</param>
    ''' <param name="dataSource">Data table dalla quale leggere i dati</param>
    ''' <param name="sort">Se eseguire un ordinamento o meno</param>
    ''' <param name="sortBy">Nome della colonna con la quale ordinare (solitamente uguale a 'displayMember' oppure 'valueMember')</param>
    Public Shared Sub SetComboboxBinding(ByRef comboBox As ComboBox, ByVal displayMember As String, ByVal valueMember As String, ByVal dataSource As DataTable,
                                         ByVal sort As Boolean, Optional ByVal sortBy As String = "")

        'Associo campo da cui prendere i valori e le relative stringhe da visualizzare
        comboBox.DisplayMember = displayMember
        comboBox.ValueMember = valueMember

        If sort AndAlso Not String.IsNullOrEmpty(sortBy) AndAlso dataSource.Columns.IndexOf(sortBy) <> -1 Then

            'Ordinamento
            Dim sortedSource As DataTable = dataSource.Clone
            For Each row As DataRow In (From entry In dataSource Order By entry(sortBy) Ascending Select entry)
                'For Each row As DataRow In dataSource.Select.OrderBy(Function(item) item(sortBy))
                sortedSource.ImportRow(row)
            Next

            comboBox.DataSource = sortedSource
        Else
            comboBox.DataSource = dataSource
        End If

    End Sub


#End Region


#Region " FILL TABLES "

    Public Shared Sub FillLanguageTable(ByRef languageTable As LanguageDataTable)
        Dim adapter As New LanguageTableAdapter
        adapter.Fill(languageTable)
        adapter.Dispose()
    End Sub

    Public Shared Sub FillGenreTable(ByRef dataset As IdDataSet)
        Dim adapter As New GenreTableAdapter
        adapter.Fill(dataset.Genre)
        adapter.Dispose()
    End Sub

    Public Shared Sub FillLastModelTable(ByRef dataset As IdDataSet)
        Dim adapter As New LastModelTableAdapter
        adapter.Fill(dataset.LastModel)
        adapter.Dispose()
    End Sub

    Public Shared Sub FillSizeTable(ByRef dataset As IdDataSet)
        Dim adapter As New SizeTableAdapter
        adapter.Fill(dataset.Size)
        adapter.Dispose()
    End Sub

    Public Shared Sub FillGenreSizeTable(ByRef dataset As IdDataSet)
        Dim adapter As New Genre_SizeTableAdapter
        adapter.Fill(dataset.Genre_Size)
        adapter.Dispose()
    End Sub

    Public Shared Sub FillPathologyTable(ByRef dataset As IdDataSet)
        Dim adapter As New PathologyTableAdapter
        adapter.Fill(dataset.Pathology)
        adapter.Dispose()
    End Sub

    Public Shared Sub FillThicknessTable(ByRef dataset As IdDataSet)
        Dim adapter As New ThicknessTableAdapter
        adapter.Fill(dataset.Thickness)
        adapter.Dispose()
    End Sub

    Public Shared Sub FillVaultTable(ByRef dataset As IdDataSet)
        Dim adapter As New VaultTableAdapter
        adapter.Fill(dataset.Vault)
        adapter.Dispose()
    End Sub

    Public Shared Sub FillBottomTypeTable(ByRef dataset As IdDataSet)
        Dim adapter As New BottomTypeTableAdapter
        adapter.Fill(dataset.BottomType)
        adapter.Dispose()
    End Sub

    Public Shared Sub FillTemplateTable(ByRef dataset As IdDataSet)
        Dim adapter As New TemplateTableAdapter
        adapter.Fill(dataset.Template)
        adapter.Dispose()
    End Sub

    Public Shared Sub FillTemplatePathologyTable(ByRef dataset As IdDataSet)
        Dim adapter As New Template_PathologyTableAdapter
        adapter.Fill(dataset.Template_Pathology)
        adapter.Dispose()
    End Sub

    Public Shared Sub FillPatientTable(ByRef dataset As IdDataSet)
        Dim adapter As New PatientTableAdapter
        adapter.Fill(dataset.Patient)
        adapter.Dispose()
    End Sub

    Public Shared Sub FillTemplatePatientTable(ByRef dataset As IdDataSet)
        Dim adapter As New Template_PatientTableAdapter
        adapter.Fill(dataset.Template_Patient)
        adapter.Dispose()
    End Sub

    Public Shared Sub FillUserTable(ByRef dataset As IdDataSet)
        Dim adapter As New UserTableAdapter
        adapter.Fill(dataset.User)
        adapter.Dispose()
    End Sub


#End Region



End Class
