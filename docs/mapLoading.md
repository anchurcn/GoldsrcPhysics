# Notes
每个地图都由固体和固体实体和点实体构成
地图的Model[0]就是地图的静态地形
WorldSpawn实体是每个地图都有的实体
地图有许多个Model，每个Model里面有许多的子模型，因为每个子模型应用不同的材质。
出于性能目的，如果只是贴图不同而其他渲染操作相同，可以合并贴图、UV、子模型，这样可以合并为一个draw call
对于物理引擎，合并模型也能提高性能，而在物理世界只考虑几何模型，所以我们可以合并所有静态的子模型。
预览版：排除不可见固体实体的模型、排除贴上不参与碰撞贴图的模型（如天空）
正式版：只加载静态地形，包括func_wall实体的模型，排除贴上不参与碰撞贴图的模型（如天空），其他实体的模型应参与游戏逻辑（如门应该是Kinematic刚体，一般的物件应该是动态刚体）


# MapLoader.cs

## bsp model[0]

From bsp nodes[0], find all faces of model[0]. Then generate vertices and indeces array.