using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity;

//该脚本用来追踪Spatial Understanding的请求，即把所有请求都用一种数据结构存储起来，用一个独立的线程一次性传给Spatial Understanding
public enum ObjectType
{
    SquareBuliding,
    WideBuilding,
    TallBuliding,
    Tree,
    Tumbleweed,
    Mine
}


//PlacementQuery包含放置定义/规则/限制/维度/物体类型
public struct PlacementQuery
{
    public PlacementQuery(SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition placementDefinition,Vector3 dimensions,ObjectType objType,List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule> placementRules=null,List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementConstraint> placementConstraints=null)
    {
        PlacementDefinition = placementDefinition;
        PlacementRules = placementRules;
        PlacementConstraints = placementConstraints;
        Dimensions = dimensions;
        ObjType = objType;
       
    }

    public readonly SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition PlacementDefinition;
    public readonly Vector3 Dimensions;
    public readonly ObjectType ObjType;
    public readonly List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule> PlacementRules;
    public readonly List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementConstraint> PlacementConstraints;

}