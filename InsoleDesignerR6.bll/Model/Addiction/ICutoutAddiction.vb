Imports System.IO
Imports RMA.OpenNURBS
Imports InsoleDesigner.bll.AbstractCutoutCommons
Imports InsoleDesigner.bll.IdElement3dManager
Imports RMA.Rhino


Public Interface ICutoutAddiction


    Function GetCurveToExtrude() As IOnCurve

    Sub ClearCurveID()

    Property FilletRadius(ByVal type As eHorseShoeFilletCrv) As Double

    Function GetCurvesID(ByVal type As eHorseShoeCrv) As Guid

    Sub UpdateCurveId(ByVal horseShoeCrv As eHorseShoeCrv, ByVal oldId As Guid, ByVal newId As String)

    Function GetCurveRef(ByVal type As eHorseShoeCrv) As MRhinoObjRef

    Function MaxFilletRadius(ByVal type As eHorseShoeFilletCrv) As Decimal

    Function CurveIsInDoc(ByVal type As eHorseShoeCrv) As Boolean

    Function AddCurveToDoc(ByVal type As eHorseShoeCrv, ByVal curve As OnCurve, ByVal oldId As Guid) As MRhinoCurveObject

    Sub RotateCurve(ByVal type As eHorseShoeStraightCrv, ByVal angle As Double, Optional ByVal rotationCenter As On3dPoint = Nothing)

    Function GetCenterOfRotation(ByVal type As eHorseShoeStraightCrv) As On3dPoint

    Function SetCurveLenght(ByVal type As eHorseShoeStraightCrv, ByVal totLenght As Double) As Boolean

    Function CreateFillet(ByVal type As eHorseShoeFilletCrv, Optional ByVal docRedraw As Boolean = True) As Boolean

    Function AreCurvesParallel(ByVal type1 As eHorseShoeStraightCrv, ByVal type2 As eHorseShoeStraightCrv) As Boolean

    Sub DeleteAllCurves()

    Sub DeleteCurve(ByVal type As eHorseShoeCrv)

    Function CreateSrfFromCurves(ByVal layerName As String, ByVal extrusionLenght As Double) As Boolean


End Interface

