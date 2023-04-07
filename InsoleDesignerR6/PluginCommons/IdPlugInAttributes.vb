Imports System.Linq
Imports System.Reflection
Imports Rhino.PlugIns


'''<summary>
''' The following class is required for all Rhino.NET plug-ins.
''' These are used to display plug-in information in the plug-in manager.
''' Any string will work for these attributes, so if you don't have a fax
''' number it is OK to enter something like "none"
'''</summary>
''' 
Public Class IdPlugInAttributes


    Public Shared Function GetEmail() As String
        Return  GetAttribute(DescriptionType.Email) 
    End Function

    Public Shared Function GetAddress() As String
        Return GetAttribute(DescriptionType.Address) 
    End Function

    Public Shared Function GetCountry() As String
        Return GetAttribute(DescriptionType.Country) 
    End Function

    Public Shared Function GetFax() As String
        Return ""
    End Function

    Public Shared Function GetOrganization() As String
        Return GetAttribute(DescriptionType.Organization) 
    End Function

    Public Shared Function GetPhone() As String
        Return GetAttribute(DescriptionType.Phone) 
    End Function

    Public Shared Function GetUpdateURL() As String
        Return ""
    End Function

    Public Shared Function GetWebsite() As String
        Return GetAttribute(DescriptionType.WebSite) 
    End Function

    Private Shared Function GetAttribute(param As DescriptionType) As String
        Dim assembly = System.Reflection.Assembly.GetExecutingAssembly()
        Dim attributes As Object() = assembly.GetCustomAttributes()
        If attributes Is Nothing OrElse attributes.Length = 0 Then Return ""
        Dim pluginAttr = attributes.Where(Function(x) x.GetType().FullName = "Rhino.PlugIns.PlugInDescriptionAttribute").ToList().Cast(Of PlugInDescriptionAttribute)()
        If Not pluginAttr.Any() Then Return ""
        Dim attribute = pluginAttr.FirstOrDefault(Function(x) x.DescriptionType = param)
            
        Return IIf(attribute Is Nothing, "", attribute.Value)
    End Function


End Class
