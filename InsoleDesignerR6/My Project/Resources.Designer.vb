﻿'------------------------------------------------------------------------------
' <auto-generated>
'     Il codice è stato generato da uno strumento.
'     Versione runtime:4.0.30319.42000
'
'     Le modifiche apportate a questo file possono provocare un comportamento non corretto e andranno perse se
'     il codice viene rigenerato.
' </auto-generated>
'------------------------------------------------------------------------------

Option Strict On
Option Explicit On

Imports System

Namespace My.Resources
    
    'Questa classe è stata generata automaticamente dalla classe StronglyTypedResourceBuilder.
    'tramite uno strumento quale ResGen o Visual Studio.
    'Per aggiungere o rimuovere un membro, modificare il file con estensione ResX ed eseguire nuovamente ResGen
    'con l'opzione /str oppure ricompilare il progetto VS.
    '''<summary>
    '''  Classe di risorse fortemente tipizzata per la ricerca di stringhe localizzate e così via.
    '''</summary>
    <Global.System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0"),  _
     Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
     Global.System.Runtime.CompilerServices.CompilerGeneratedAttribute(),  _
     Global.Microsoft.VisualBasic.HideModuleNameAttribute()>  _
    Friend Module Resources
        
        Private resourceMan As Global.System.Resources.ResourceManager
        
        Private resourceCulture As Global.System.Globalization.CultureInfo
        
        '''<summary>
        '''  Restituisce l'istanza di ResourceManager nella cache utilizzata da questa classe.
        '''</summary>
        <Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Advanced)>  _
        Friend ReadOnly Property ResourceManager() As Global.System.Resources.ResourceManager
            Get
                If Object.ReferenceEquals(resourceMan, Nothing) Then
                    Dim temp As Global.System.Resources.ResourceManager = New Global.System.Resources.ResourceManager("InsoleDesigner.Resources", GetType(Resources).Assembly)
                    resourceMan = temp
                End If
                Return resourceMan
            End Get
        End Property
        
        '''<summary>
        '''  Esegue l'override della proprietà CurrentUICulture del thread corrente per tutte le
        '''  ricerche di risorse eseguite utilizzando questa classe di risorse fortemente tipizzata.
        '''</summary>
        <Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Advanced)>  _
        Friend Property Culture() As Global.System.Globalization.CultureInfo
            Get
                Return resourceCulture
            End Get
            Set
                resourceCulture = value
            End Set
        End Property
		
		 '''<summary>
        '''  Cerca una stringa localizzata simile a Tutti.
        '''</summary>
        Friend ReadOnly Property All() As String
            Get
                Return ResourceManager.GetString("All", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property Allinea() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("Allinea", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una stringa localizzata simile a Filtra.
        '''</summary>
        Friend ReadOnly Property ApplyFilter() As String
            Get
                Return ResourceManager.GetString("ApplyFilter", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property BarraMetatarsale_3450() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("BarraMetatarsale_3450", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property BarraMetatarsale_3869() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("BarraMetatarsale_3869", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property BarraMetatarsale_3870() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("BarraMetatarsale_3870", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una stringa localizzata simile a Fondo.
        '''</summary>
        Friend ReadOnly Property BottomType() As String
            Get
                Return ResourceManager.GetString("BottomType", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una stringa localizzata simile a Raccorda.
        '''</summary>
        Friend ReadOnly Property btnAddictionLink() As String
            Get
                Return ResourceManager.GetString("btnAddictionLink", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una stringa localizzata simile a Proietta.
        '''</summary>
        Friend ReadOnly Property btnAddictionProject() As String
            Get
                Return ResourceManager.GetString("btnAddictionProject", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una stringa localizzata simile a Per proiettare le curve, selezionare in ordine:
        '''                                    il punto al centro della base dello scarico (punto1)
        '''                                    il punto1 traslato in Y (punto2)
        '''                                    la superfice superiore del plantare
        '''                                    il punto1 traslato in Z
        '''                                    il punto2 traslato in Z.
        '''</summary>
        Friend ReadOnly Property btnProjectOrFilletToolTip() As String
            Get
                Return ResourceManager.GetString("btnProjectOrFilletToolTip", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property CopyDown() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("CopyDown", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property CopyUp() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("CopyUp", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property CurvePiede() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("CurvePiede", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property CurvePlantare() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("CurvePlantare", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property Cutout_parziale() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("Cutout_parziale", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property Cutout_totale() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("Cutout_totale", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property Database_read() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("Database_read", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property Database_save() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("Database_save", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property DeformInsoleByPressure() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("DeformInsoleByPressure", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property Delete() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("Delete", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una stringa localizzata simile a 0.
        '''</summary>
        Friend ReadOnly Property Expired() As String
            Get
                Return ResourceManager.GetString("Expired", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property FerroDiCavallo_parziale() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("FerroDiCavallo_parziale", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property FerroDiCavallo_totale() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("FerroDiCavallo_totale", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property Fine() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("Fine", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property FinePlantare() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("FinePlantare", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property Foot_side_both() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("Foot_side_both", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property Foot_side_both_16x16() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("Foot_side_both_16x16", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property Foot_side_left() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("Foot_side_left", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property Foot_side_left_11x19() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("Foot_side_left_11x19", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property Foot_side_left_16x16() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("Foot_side_left_16x16", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property Foot_side_right() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("Foot_side_right", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property Foot_side_right_11x19() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("Foot_side_right_11x19", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property Foot_side_right_16x16() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("Foot_side_right_16x16", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property Goccia() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("Goccia", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property GuidePlantare() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("GuidePlantare", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una stringa localizzata simile a Spessore tallone.
        '''</summary>
        Friend ReadOnly Property HeelThickness() As String
            Get
                Return ResourceManager.GetString("HeelThickness", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property Hide() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("Hide", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property IdSplash() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("IdSplash", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property Import() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("Import", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property Key() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("Key", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una stringa localizzata simile a Forma.
        '''</summary>
        Friend ReadOnly Property Last() As String
            Get
                Return ResourceManager.GetString("Last", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property LoadLD() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("LoadLD", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property LockScale() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("LockScale", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property matrix() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("matrix", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property minus() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("minus", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una stringa localizzata simile a Non presente.
        '''</summary>
        Friend ReadOnly Property NotPresent() As String
            Get
                Return ResourceManager.GetString("NotPresent", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property Oliva_3423() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("Oliva_3423", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property Oliva_3435() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("Oliva_3435", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property Oliva_3447() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("Oliva_3447", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property Oliva_6648() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("Oliva_6648", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una stringa localizzata simile a Patologie.
        '''</summary>
        Friend ReadOnly Property Pathologies() As String
            Get
                Return ResourceManager.GetString("Pathologies", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una stringa localizzata simile a Paziente.
        '''</summary>
        Friend ReadOnly Property Patient() As String
            Get
                Return ResourceManager.GetString("Patient", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property PianoInclinato() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("PianoInclinato", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property plus() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("plus", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una stringa localizzata simile a Presente.
        '''</summary>
        Friend ReadOnly Property Present() As String
            Get
                Return ResourceManager.GetString("Present", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property PressureImage() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("PressureImage", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property PressureMap() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("PressureMap", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property ProfileTCO() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("ProfileTCO", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property ProjectPressure() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("ProjectPressure", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una stringa localizzata simile a Rimuovi filtro.
        '''</summary>
        Friend ReadOnly Property RemoveFilter() As String
            Get
                Return ResourceManager.GetString("RemoveFilter", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property ric_tallone() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("ric_tallone", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property Ruler() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("Ruler", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property Scarichi() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("Scarichi", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property ScarichiMinus() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("ScarichiMinus", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property ScarichiPlus() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("ScarichiPlus", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property Settings() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("Settings", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property Show() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("Show", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una stringa localizzata simile a Taglia.
        '''</summary>
        Friend ReadOnly Property Size() As String
            Get
                Return ResourceManager.GetString("Size", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property Sottopiede() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("Sottopiede", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una stringa localizzata simile a Inizia modifica interattiva.
        '''</summary>
        Friend ReadOnly Property StartInteractiveChange() As String
            Get
                Return ResourceManager.GetString("StartInteractiveChange", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una stringa localizzata simile a Inizia sostituzione interattiva.
        '''</summary>
        Friend ReadOnly Property StartInteractiveSubstitution() As String
            Get
                Return ResourceManager.GetString("StartInteractiveSubstitution", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una stringa localizzata simile a Termina modifica interattiva.
        '''</summary>
        Friend ReadOnly Property StopInteractiveChange() As String
            Get
                Return ResourceManager.GetString("StopInteractiveChange", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una stringa localizzata simile a Termina sostituzione interattiva.
        '''</summary>
        Friend ReadOnly Property StopInteractiveSubstitution() As String
            Get
                Return ResourceManager.GetString("StopInteractiveSubstitution", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property SuperficiPlantare() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("SuperficiPlantare", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una risorsa localizzata di tipo System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property SupportoVolta_3030() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("SupportoVolta_3030", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una stringa localizzata simile a Spessore.
        '''</summary>
        Friend ReadOnly Property Thickness() As String
            Get
                Return ResourceManager.GetString("Thickness", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Cerca una stringa localizzata simile a Volta.
        '''</summary>
        Friend ReadOnly Property Vault() As String
            Get
                Return ResourceManager.GetString("Vault", resourceCulture)
            End Get
        End Property
		
    End Module
End Namespace
