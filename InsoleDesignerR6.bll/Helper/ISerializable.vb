Imports RMA.OpenNURBS
Imports InsoleDesigner.bll.IdElement3dManager


Public Interface IOnSerializable

    Function Serialize(ByRef archive As OnBinaryArchive) As Boolean

    Function Deserialize(ByRef archive As OnBinaryArchive) As Boolean

End Interface


Public Interface IOnSerializableSide

     Function Serialize(ByRef archive As OnBinaryArchive, side As eSide) As Boolean

    Function Deserialize(ByRef archive As OnBinaryArchive, side As eSide) As Boolean

End Interface