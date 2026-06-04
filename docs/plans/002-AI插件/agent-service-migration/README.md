# Agent 服务迁移设计根目录

本目录用于承载通用 Agent 服务迁移到 MMB / `Materal.MergeBlock.AI` 的设计与实施计划。

## 文件

- `implementation-plan.md`：解决迁移缺陷的分阶段实施计划。
- `testing-plan.md`：自动化测试、手工验证和 Demo 测试页面优化计划。

## 输入材料

- 上一级通用缺陷清单：已剔除业务域和客户端宿主专有内容。
- `../design.md`：`Materal.MergeBlock.AI` 总体设计。
- `../impl-01-core.md`：AI Core 抽象与模块计划。
- `../impl-02-web-agent-host.md`：AI Web Host 与 Remote Tool Gateway 计划。
- `../impl-03-agent-runtime-bridge.md`：Runtime Bridge 计划。

## 设计边界

- 本设计只处理通用 Agent Host、远程客户端工具、运行时桥接、模型配置、审计、checkpoint 和 watchdog。
- 本设计不包含具体业务域工具、具体客户端宿主能力或 Provider 专用包在框架层的实现。
- 业务模块通过 `IAIAgentRuntime`、工具契约、prompt contributor、server-executed tools 和审查 gate 接入自身能力。
- 测试页面只作为通用 Agent Host 验证台，不承载具体业务域或客户端宿主专有行为。
