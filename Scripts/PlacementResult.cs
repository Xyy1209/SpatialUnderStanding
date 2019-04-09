using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity;



//放置结果包含放置的位置/朝向（Z轴）/维度/物体类型
public class PlacementResult 
{
    //构造方法。包含放置的位置和朝向Forward
    public PlacementResult(SpatialUnderstandingDllObjectPlacement.ObjectPlacementResult result,Vector3 dimensions,ObjectType objType)
    {
        _result = result;
        Dimensions = dimensions;
        ObjType = objType;

    }

    //该类的成员变量
    public Vector3 Position { get { return _result.Position; } }
    public Vector3 Normal { get { return _result.Forward; } }
    public Vector3 Dimensions { get; private set; }
    public ObjectType ObjType { get; private set; }


    private readonly SpatialUnderstandingDllObjectPlacement.ObjectPlacementResult _result;

}
