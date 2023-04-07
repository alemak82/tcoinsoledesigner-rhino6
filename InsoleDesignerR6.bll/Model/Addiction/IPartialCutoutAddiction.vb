Imports System.IO
Imports RMA.OpenNURBS
Imports InsoleDesigner.bll.IdElement3dManager
Imports InsoleDesigner.bll.AbstractCutoutCommons
Imports RMA.Rhino


Public Interface IPartialCutoutAddiction
    Inherits ICutoutAddiction


    Property Side() As eSide
    Property Depth() As Double
    Property InsoleTopSrfCopyId() As Guid

    Function GetTrimmedTopSurface(ByVal topSurface As eTopSurface, ByVal insoleSrfRefId As Guid, ByVal cuttingSrfRefId As Guid) As Guid

    ''' <summary>
    ''' Crea la superficie di sweep estesa per la ricostruione della superficie finale del cutout/ferro di cavallo parziale
    ''' </summary>
    ''' <param name="extrusionCurveCutoutSrfRef"></param>
    ''' <param name="trimmedInsoleTopCopy"></param>
    ''' <param name="borderCurve"></param>
    ''' <param name="offsetCurve"></param>
    ''' <returns></returns>
    Function GetSweepSurface(ByRef extrusionCurveCutoutSrfRef As MRhinoObjRef, ByRef trimmedInsoleTopCopy As MRhinoObjRef, ByRef borderCurve As OnCurve, ByRef offsetCurve As OnCurve) As OnSurface
End Interface
