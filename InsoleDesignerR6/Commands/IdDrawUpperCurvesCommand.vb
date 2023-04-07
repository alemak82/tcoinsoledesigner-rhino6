Imports RMA.Rhino
Imports RMA.OpenNURBS
Imports RhinoUtils.RhViewport
Imports RhinoUtils.RhGeometry
Imports InsoleDesigner.bll.IdAlias
Imports InsoleDesigner.bll.IdLanguageManager
Imports InsoleDesigner.bll.IdElement3dManager
Imports GeometryUtils = InsoleDesigner.bll.IdGeometryUtils
Imports System.Reflection
Imports InsoleDesigner.bll
Imports InsoleDesigner.IdPlugIn
Imports RhinoUtils


Public Class IdDrawUpperCurvesCommand
    
    #Region " Constant "

    ''' <summary>
    ''' Delta che viene aggiunto alla traslazione delle curve definite dall'utente
    ''' </summary>
    Private Const TRANSLATION_DELTA As Double = 15

    ''' <summary>
    ''' Distanza al di sopra della quale non raccordo
    ''' </summary>
    Private Const MAX_CURVE_DISTANCE As Double = 20

    ''' <summary>
    ''' Fattore moltiplicativo tra la distanza delle intersezioni e il diametro del cilindro per raccordare
    ''' </summary>
    Private Const TRIMMING_FACTOR As Double = 3

    ''' <summary>
    ''' Distanza a cui fare offset della curva finale del plantare - A REGIME = 2
    ''' </summary>
    Public Const OFFSET_CURVE_DISTANCE As Double = 3 '2

    ''' <summary>
    ''' Tolleranza per il comando rebuild sulla curva di profilo del plantare (valore precedente 0.0001)
    ''' </summary>
    Private Const REBUILD_TOLERANCE As Double = 0.0001  'Raffaele dice 0.001

#End Region

End Class
