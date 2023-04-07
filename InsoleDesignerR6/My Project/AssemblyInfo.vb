Imports System
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports Rhino.PlugIns



' Plug-in Description Attributes - all of these are optional.
' These will show in Rhino's option dialog, in the tab Plug-ins.
<Assembly: PlugInDescription(DescriptionType.Address, "via del Consorzio 29 - 60015 Falconara Marittima (AN)")>
<Assembly: PlugInDescription(DescriptionType.Country, "ITALY")>
<Assembly: PlugInDescription(DescriptionType.Email, "info@duna.it")>
<Assembly: PlugInDescription(DescriptionType.Phone, "+39-071-9157065")>
<Assembly: PlugInDescription(DescriptionType.Fax, "none")>
<Assembly: PlugInDescription(DescriptionType.Organization, "Duna S.R.L.")>
<Assembly: PlugInDescription(DescriptionType.UpdateUrl, "none")>
<Assembly: PlugInDescription(DescriptionType.WebSite, "www.duna.it")>

' Icons should be Windows .ico files And contain 32-bit images in the following sizes 16, 24, 32, 48, And 256.
' This is a Rhino 6-only description.
<Assembly: PlugInDescription(DescriptionType.Icon, "InsoleDesignerR6.EmbeddedResources.plugin-utility.ico")>

' General Information about an assembly is controlled through the following 
' set of attributes. Change these attribute values to modify the information
' associated with an assembly.
<Assembly: AssemblyTitle("InsoleDesignerR6")>

' This will be used also for the plug-in description.
<Assembly: AssemblyDescription("InsoleDesigner per Rhinoceros 6 utility plug-in")>

<Assembly: AssemblyConfiguration("")>
<Assembly: AssemblyCompany("Duna SRL")>
<Assembly: AssemblyProduct("InsoleDesignerR6")>
<Assembly: AssemblyCopyright("Copyright ©  2013")>
<Assembly: AssemblyTrademark("")>
<Assembly: AssemblyCulture("")>

' Setting ComVisible to false makes the types in this assembly not visible 
' to COM components.  If you need to access a type in this assembly from 
' COM, set the ComVisible attribute to true on that type.
<Assembly: ComVisible(False)>

'The following GUID is for the ID of the typelib if this project is exposed to COM
<Assembly: Guid("ce0e7f61-3aba-4c03-b7d9-f5e9425fea6d")> ' This will also be the Guid of the Rhino plug-in

' Version information for an assembly consists of the following four values:
'
'      Major Version
'      Minor Version 
'      Build Number
'      Revision
'
' You can specify all the values or you can default the Build and Revision Numbers 
' by using the '*' as shown below:
' <Assembly: AssemblyVersion("1.0.*")> 

<Assembly: AssemblyVersion("5.0.0.0")>
<Assembly: AssemblyFileVersion("5.0.0.0")>

' Make compatible With Rhino Installer Engine
<Assembly: AssemblyInformationalVersion("2")>
