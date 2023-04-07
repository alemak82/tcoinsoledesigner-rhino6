Imports InsoleDesigner.bll
Imports Rhino
Imports Rhino.PlugIns
Imports InsoleDesigner.bll.IdAlias


'''<summary>
''' <para>Every RhinoCommon .rhp assembly must have one and only one PlugIn-derived
''' class. DO NOT create instances of this class yourself. It is the
''' responsibility of Rhino to create an instance of this class.</para>
''' <para>To complete plug-in information, please also see all PlugInDescription
''' attributes in AssemblyInfo.vb (you might need to click "Project" ->
''' "Show All Files" to see it in the "Solution Explorer" window).</para>
'''</summary>
Public Class IdPlugIn
    Inherits Rhino.PlugIns.PlugIn


#Region " SINGLETON "


    Shared _instance As IdPlugIn

    Public Sub New()
        _instance = Me
    End Sub

    '''<summary>Gets the only instance of the IdPlugIn plug-in.</summary>
    Public Shared ReadOnly Property Instance() As IdPlugIn
        Get
            Return _instance
        End Get
    End Property

    Public Shared Function ThisPlugIn() As IdPlugIn
        Return Instance()
    End Function


#End Region


#Region " Costanti Integrazione LastDesigner "

    Public Const LD_LAST_TOP_RIGHT As String = "Last Top R"
    Public Const LD_LAST_TOP_LEFT As String = "Last Top L"
    Public Const LD_LAST_UPPER_RIGHT As String = "Last Upper R"
    Public Const LD_LAST_UPPER_LEFT As String = "Last Upper L"
    Public Const LD_LAST_SOLE_RIGHT As String = "Last Sole R"
    Public Const LD_LAST_SOLE_LEFT As String = "Last Sole L"
    Public Const LD_FOOT_SCAN_RIGHT As String = "Foot Scan R"
    Public Const LD_FOOT_SCAN_LEFT As String = "Foot Scan L"

#End Region


#Region " Costanti DEBUG "

#If DEBUG Then

    ' "D:\Dropbox\PrivateBox\LAVORO\MakkioSoft\CLIENTI\Duna\Insole Designer\"
    Public Const BASE_ID_DIR As String = "D:\OneDrive - enhance holistic functionalities\Lupin\LAVORO\MakkioSoft\CLIENTI\Duna\Insole Designer\"

#End If

#End Region


#Region " Enum "

    Public Enum ePluginPhase
        start = 0
        importDone
        footCurvesDone
        insoleUpperCurveDone
        insoleGuidesDone
        surfacesInsoleDone
        addictionsDone
        finalInsoleDone
        autoSetting
    End Enum


#End Region


#Region " Fields "


    Public DisplayConduit As IdDisplayConduit

    'Puntatore all'oggetto Panel
    Public MainPanel As ctlMainPanel

    ''Puntatore all'ggetto che permette l'intercettazione degli eventi di Rhinoceros
    'Public EventWatcher As IdEventWatcher

    'Timer per la gestione dello Splash del PlugIn
    Dim WithEvents mLoadingTimer As Timers.Timer

    ''Gestore del mouse da abilitare per essere usato(vedi sotto MouseManager.Enabled = True)
    'Public WithEvents MouseManager As New RhMouseManager


#End Region

    ' You can override methods here to change the plug-in behavior on
    ' loading and shut down, add options pages to the Rhino _Option command
    ' and maintain plug-in wide options in a document.
    Protected Overrides Function OnLoad(ByRef errorMessage As String) As LoadReturnCode
        Try


            ' Creazione del Main Panel
            MainPanel = New ctlMainPanel()


        Catch ex As Exception
            MsgBox(LanguageManager.Message(235) & vbCrLf & vbCrLf & ex.Message, MsgBoxStyle.Critical, My.Application.Info.Title)
            IdLanguageManager.PromptError(ex.Message)
            Return LoadReturnCode.ErrorShowDialog
        End Try
        Return LoadReturnCode.Success
    End Function
    
    Protected Overrides Sub OnShutdown()

    End Sub

End Class
