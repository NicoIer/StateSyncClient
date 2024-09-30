# GameNetwork

局内相关的网络同步组件。

类似一个树状的结构`Entity`，`Entity`下面挂载很多不同类型的`Component`。

`Component`是一个数据结构，用于存储数据，`Component`的数据会被同步到其他客户端。

Unity客户端需要在`Entity`的基础上构建一个`MonoBehaviour`，用于处理`Entity`。

Unity客户端需要在`Component`的基础上构建一个`MonoBehaviour`，用于处理`Component`。

## NetworkTransform

- 权威客户端创建，其他客户端同步。
- 权威客户端指定其他客户端是否需要进行插值同步。
- 权威客户端定时广播自己的位置信息(存在变化才广播)，其中包含时间戳。
- 其他客户端接收到位置信息后，根据时间戳进行插值同步。