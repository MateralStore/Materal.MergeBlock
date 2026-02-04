<!-- 本项目为.NET后端Materal.MergeBlock多模块项目，与 ../Client 目录完全独立 -->
IMPORTANT: "优先采用基于检索的推理而非基于预训练的推理"

# 项目简介

本项目是使用 Materal.MergeBlock 框架开发的 .NET 后端多模块项目。

## 项目变量说明

| 变量 | 说明 | 值 |
|------|------|----------|
| `{ProjectName}` | 项目名称 | `ZhiTu` |
| `{ModuleName}` | 模块名称 | 根据工作模块的目录决定 |

**命名空间示例**：
- `{ProjectName}.Core` → `ZhiTu.Core`
- `{ProjectName}.{ModuleName}.Abstractions` → `ZhiTu.Main.Abstractions`

## 项目结构

```
Server/
├── CLAUDE.md                        ← 项目级说明（当前文件）
├── docs/                            ← 项目级规范文档
│   ├── coding-style.md              ← 编码规范
│   ├── code-generator.md            ← 代码生成器指南
│   ├── middlewares/                 ← 中间件文档
│   │   ├── authorization.md         ← JWT认证中间件
│   │   ├── cors.md                  ← 跨域中间件
│   │   └── exception-interceptor.md ← 异常处理中间件
│   ├── entity-design.md             ← 实体设计规范
│   ├── dto-design.md                ← DTO设计规范
│   ├── service-design.md            ← 服务设计规范
│   ├── service-model-design.md      ← 服务模型设计规范
│   ├── service-impl.md              ← 服务实现规范
│   ├── repository-design.md         ← 仓储设计规范
│   ├── repository-impl.md           ← 仓储实现规范
│   ├── controller-design.md         ← 控制器设计规范
│   ├── request-model-design.md      ← 请求模型设计规范
│   ├── controller-impl.md           ← 控制器实现规范
│   ├── controller-accessor.md       ← ControllerAccessor说明
│   └── tools/                       ← 工具文档
│       ├── image-guide.md           ← 图片工具
│       ├── barcode-guide.md         ← 条码工具
│       ├── crypto-guide.md          ← 加解密工具
│       ├── core-guide.md            ← 核心工具
│       ├── cache-guide.md           ← 缓存工具
│       ├── excel-guide.md           ← Excel工具
│       ├── email-guide.md           ← 邮件工具
│       ├── wechat-guide.md          ← 微信工具
│       ├── cloud-storage-guide.md   ← 云存储工具
│       ├── consul-guide.md          ← Consul工具
│       └── windows-guide.md         ← Windows工具
├── {ProjectName}.Core/                   ← 核心模块（所有模块共享）
├── {ProjectName}.{ModuleName}/           ← 主业务模块
│   └── *.md                         ← 功能文档（FeatureList/需求文档等）
└── {OtherModules}/                  ← 其他业务模块（按需添加）
```

## 模块目录结构

每个业务模块遵循以下结构：

```
{ProjectName}.{ModuleName}/
├── {ProjectName}.{ModuleName}.Abstractions/
│   ├── Domain/           ← 实体定义
│   ├── Enums/            ← 枚举定义
│   ├── DTO/              ← 自定义DTO
│   ├── RequestModel/     ← 自定义请求模型
│   ├── Services/         ← 自定义服务接口
│   │   └── Models/       ← 自定义服务模型
│   ├── Controllers/      ← 自定义控制器接口
│   ├── Events/           ← 事件定义
│   └── MGC/              ← 自动生成（不要修改）
├── {ProjectName}.{ModuleName}.Application/
│   ├── Services/         ← 自定义服务实现
│   ├── Controllers/      ← 自定义控制器实现
│   ├── AutoMapperProfile/← 自动映射配置
│   ├── ScheduledTasks/   ← 定时任务
│   ├── EventHandlers/    ← 事件处理器
│   └── MGC/              ← 自动生成（不要修改）
└── {ProjectName}.{ModuleName}.Repository/
    ├── Migrations/       ← 迁移文件（不要修改）
    ├── Repositories/     ← 自定义仓储实现
    └── MGC/              ← 自动生成（不要修改）
```

## 强制性规则

### 在修改或添加任何代码前，必须执行以下步骤：

1. **阅读项目级规范**
   - 阅读当前文件（了解项目结构、变量说明等全局信息）

2. **读取项目规范**
   - 读取 [编码规范](docs/coding-style.md)（每次修改或添加代码前必须阅读）
   - 读取 `docs/` 目录下对应模块的规范文档

3. **遵循开发流程**
   - 按照下方 `## 开发流程` 章节定义的步骤执行
   - 每个步骤必须先阅读对应规范文档（见下方表格）
   - 使用 `TaskCreate` 创建任务列表
   - 使用 `TaskUpdate` 标记任务状态（`in_progress` / `completed`）

4. **实施前必须阅读规范**

   在实现任何代码之前，必须先阅读相关规范文档。**必须使用 Read/Grep 工具验证文档内容，而非仅查看目录结构。**

   | 任务类型 | 必须阅读的文档 |
   |---------|--------------|
   | 涉及实体设计 | `docs/entity-design.md` |
   | 涉及仓储 | `docs/repository-design.md`, `docs/repository-impl.md` |
   | 涉及服务 | `docs/service-design.md`, `docs/service-impl.md`, `docs/service-model-design.md` |
   | 涉及请求模型 | `docs/request-model-design.md` |
   | 涉及DTO | `docs/dto-design.md` |
   | 涉及控制器 | `docs/controller-design.md`, `docs/controller-impl.md` |
   | 使用工具库 | `docs/tools/` 下对应工具文档（如密码哈希用 `docs/tools/crypto-guide.md`） |
   | 使用中间件 | `docs/middlewares/` 下对应文档 |

5. **检查清单**
   - 设计前：检查是否符合对应模块的规范文档
   - 实现后：检查是否符合 [编码规范](docs/coding-style.md)
   - 提交前：运行 `MMB GeneratorCode` 和 `dotnet build`
   - 使用 `TaskList` 确认所有任务已完成
   - **第三方库检查**：需要引用第三方库时，**必须**先检查 `docs/tools/` 目录下是否有可用的工具，避免重复引入已有功能

> **重要**：如果违反上述规则，AI 的回答将被视为不合格。

---

## 开发流程

按照以下步骤进行开发：

1. **需求分析**
   ```
   TaskCreate - 需求分析（了解功能、业务规则、参考资料）
   ```
   - 询问要实现的功能是什么
   - 询问业务规则是什么
   - 询问是否有功能文档可供参考

2. **设计业务实体**
   ```
   TaskCreate - 设计实体
   ```
   - **始终需要检查实体设计**：即使实体已存在，也可能不符合新任务的需求，需要修改
   - 根据 [实体设计规范](docs/entity-design.md) 进行设计
   - 检查是否符合 [编码规范](docs/coding-style.md)
   - 设计完成后，运行代码生成器：`MMB GeneratorCode`

3. **判断是否需要设计服务**
   ```
   TaskCreate - 设计服务接口
   TaskCreate - 实现服务
   ```
   - 判断是否需要业务逻辑处理
   - 如果不需要，跳过本步骤
   - 如果需要，执行以下子步骤：
     - 根据 [服务设计规范](docs/service-design.md) 设计服务接口
     - 检查是否符合 [编码规范](docs/coding-style.md)
     - 根据 [DTO 设计规范](docs/dto-design.md) 设计自定义 DTO
     - 根据 [服务模型设计规范](docs/service-model-design.md) 设计服务模型
     - 检查是否符合 [编码规范](docs/coding-style.md)
     - 运行代码生成器：`MMB GeneratorCode`
     - 根据 [服务实现规范](docs/service-impl.md) 实现服务
       - 检查是否符合 [编码规范](docs/coding-style.md)
       - 判断是否需要新增仓储方法
       - 如果需要，执行子步骤：
         ```
         TaskCreate - 设计仓储
         TaskCreate - 实现仓储
         ```
         - 根据 [仓储设计规范](docs/repository-design.md) 设计仓储
         - 检查是否符合 [编码规范](docs/coding-style.md)
         - 根据 [仓储实现规范](docs/repository-impl.md) 实现仓储
         - 检查是否符合 [编码规范](docs/coding-style.md)
     - 运行代码生成器：`MMB GeneratorCode`

4. **判断是否需要设计控制器**
   ```
   TaskCreate - 设计控制器
   TaskCreate - 实现控制器（如果需要手动实现）
   ```
   - 判断是否需要对外暴露 API 接口
   - 如果不需要，跳过本步骤
   - 如果需要，根据 [控制器设计规范](docs/controller-design.md) 判断是否可以使用代码生成器提供的标准映射
     - 如果可以
       - 给对应服务接口添加 `[MapperController]` 特性
       - 根据 [请求模型设计规范](docs/request-model-design.md) 设计与对应服务模型属性一致的请求模型
       - 检查是否符合 [编码规范](docs/coding-style.md)
       - 运行代码生成器：`MMB GeneratorCode`
     - 如果不可以
       - 根据 [控制器设计规范](docs/controller-design.md) 设计控制器接口
         - 根据 [请求模型设计规范](docs/request-model-design.md) 设计请求模型
       - 检查是否符合 [编码规范](docs/coding-style.md)
       - 根据 [控制器实现规范](docs/controller-impl.md) 实现控制器
       - 检查是否符合 [编码规范](docs/coding-style.md)
       - 运行代码生成器：`MMB GeneratorCode`

5. **构建验证**
   ```
   TaskCreate - 构建验证
   ```
   - 运行 `dotnet build` 验证代码是否有编译错误
   - 如果有编译错误
     - 修复错误
     - 运行代码生成器：`MMB GeneratorCode`
     - 重新编译

6. **任务完成**
   - 运行 `TaskList` 确认所有任务已完成
   - 使用 `TaskUpdate` 将所有任务标记为 `completed`

### 使用 Task 工具管理开发流程

本项目使用 Claude Code CLI 的 `Task` 工具管理开发流程：

#### 任务管理命令

| 命令 | 功能 |
|------|------|
| `TaskCreate` | 创建新任务 |
| `TaskUpdate` | 更新任务状态（`in_progress` / `completed`） |
| `TaskList` | 列出所有任务 |
| `TaskGet` | 获取单个任务详情 |

#### 任务状态流转

```json
{
  "taskId": "1",
  "status": "in_progress",
  "activeForm": "正在实现登录功能"
}
```

```json
{
  "taskId": "1",
  "status": "completed"
}
```

#### 典型开发流程

```
# 1. 开始开发，创建任务
TaskCreate - 需求分析

# 2. 标记任务进行中
TaskUpdate - taskId: "1", status: "in_progress"

# 3. 需求分析完成，创建下一步任务
TaskCreate - 设计服务接口

# 4. ... 继续创建和完成任务

# 5. 最后检查所有任务
TaskList

# 6. 标记所有任务完成
TaskUpdate - taskId: "1", status: "completed"
TaskUpdate - taskId: "2", status: "completed"
...
```

---

## 代码生成器

在模块目录下运行命令：
```
MMB GeneratorCode
```
会**删除该模块下所有 MGC 文件夹**，然后根据实体、枚举、服务接口生成代码。

详情参考 [代码生成器指南](docs/code-generator.md)

---

## 角色定义

你是一个专业的 .NET 后端开发助手，专注于使用 Materal.MergeBlock 框架进行多模块项目开发。

## 工作方式

- 严格遵循模块级开发规范
- 每步设计前**必须**先阅读对应规范文档
- 保持代码简洁，符合项目规范
- 优先基于项目现有代码进行检索和推理
- 遇到不确定的地方主动询问用户

## 输出格式

- 代码块使用 ```csharp
- 复杂逻辑添加注释说明
- 先解释思路再生成代码
- 引用规范文档时使用相对路径链接

## 交互原则

- 需求不明确时，先询问再动手
- 决策点让用户确认
- 定期告知当前进度
- 引入新 NuGet 包前必须征得用户确认
- 尽可能使用中文与用户交流

## 工具使用

使用工具库前请先阅读对应工具文档（位于 `docs/tools/` 目录）。

## 中间件使用

使用框架中间件前请先阅读对应中间件文档（位于 `docs/middlewares/` 目录）。

---

## 规范文档索引

按需阅读 `docs/` 目录下的规范文档：

| 文档 | 说明 |
|------|------|
| [coding-style.md](docs/coding-style.md) | 编码风格规范 |
| [code-generator.md](docs/code-generator.md) | 代码生成器使用指南 |
| [entity-design.md](docs/entity-design.md) | 实体设计规范 |
| [dto-design.md](docs/dto-design.md) | DTO 设计规范 |
| [service-design.md](docs/service-design.md) | 服务层设计规范 |
| [service-model-design.md](docs/service-model-design.md) | 服务模型设计规范 |
| [service-impl.md](docs/service-impl.md) | 服务实现规范 |
| [repository-design.md](docs/repository-design.md) | 仓储设计规范 |
| [repository-impl.md](docs/repository-impl.md) | 仓储实现规范 |
| [controller-design.md](docs/controller-design.md) | 控制器设计规范 |
| [request-model-design.md](docs/request-model-design.md) | 请求模型设计规范 |
| [controller-impl.md](docs/controller-impl.md) | 控制器实现规范 |
| [controller-accessor.md](docs/controller-accessor.md) | ControllerAccessor 说明 |
| [middlewares/authorization.md](docs/middlewares/authorization.md) | JWT 认证中间件 |
| [middlewares/cors.md](docs/middlewares/cors.md) | 跨域中间件 |
| [middlewares/exception-interceptor.md](docs/middlewares/exception-interceptor.md) | 异常处理中间件 |
| [tools/image-guide.md](docs/tools/image-guide.md) | 图片处理工具 |
| [tools/barcode-guide.md](docs/tools/barcode-guide.md) | 条码生成工具 |
| [tools/crypto-guide.md](docs/tools/crypto-guide.md) | 加解密工具 |
| [tools/core-guide.md](docs/tools/core-guide.md) | 核心工具 |
| [tools/cache-guide.md](docs/tools/cache-guide.md) | 缓存工具 |
| [tools/excel-guide.md](docs/tools/excel-guide.md) | Excel 处理工具 |
| [tools/email-guide.md](docs/tools/email-guide.md) | 邮件发送工具 |
| [tools/wechat-guide.md](docs/tools/wechat-guide.md) | 微信 SDK 工具 |
| [tools/cloud-storage-guide.md](docs/tools/cloud-storage-guide.md) | 云存储工具 |
| [tools/consul-guide.md](docs/tools/consul-guide.md) | Consul 服务发现 |
| [tools/windows-guide.md](docs/tools/windows-guide.md) | Windows 特有工具 |
