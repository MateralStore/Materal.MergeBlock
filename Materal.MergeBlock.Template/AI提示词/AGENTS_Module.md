# YueHeShe.Main 模块开发规范

本模块遵循 Materal.MergeBlock 框架开发规范。

## 项目变量说明

| 变量 | 说明 | 值 |
|------|------|----------|
| `{ProjectName}` | 项目名称 | `YueHeShe` |
| `{ModuleName}` | 模块名称 | `Main` |

## 强制性规则

### 在修改或添加任何代码前，必须执行以下步骤：

1. **阅读项目级规范**
   - 阅读 [项目级 CLAUDE.md](../CLAUDE.md)（了解项目结构、变量说明等全局信息）

2. **读取项目规范**
   - 读取 [编码规范](../docs/coding-style.md)（每次修改或添加代码前必须阅读）
   - 读取 `../docs/` 目录下对应模块的规范文档（按需读取，非一次性）

3. **遵循开发流程**
   - 按照下方 `## 开发流程` 章节定义的步骤执行
   - 每个步骤中明确要求"先阅读对应规范文档"
   - **按需读取**：每个步骤只读取对应的规范文档，而非一次性读取所有

3. **检查清单**
   - 设计前：检查是否符合对应模块的规范文档
   - 实现后：检查是否符合 [编码规范](../docs/coding-style.md)
   - 提交前：运行 `MMB GeneratorCode` 和 `dotnet build`
   - **第三方库检查**：需要引用第三方库时，**必须**先检查 `../docs/tools/` 目录下是否有可用的工具，避免重复引入已有功能

> **重要**：如果违反上述规则，AI 的回答将被视为不合格。

---

## 开发流程

按照以下步骤进行开发：

1. **需求分析**
   - 询问要实现的功能是什么
   - 询问业务规则是什么
   - 询问是否有功能文档可供参考

2. **判断是否需要设计实体**
   - 判断是否需要新增或修改数据实体
   - 如果不需要，跳过本步骤
   - 如果需要，根据 [实体设计规范](../docs/entity-design.md) 进行设计
   - 检查是否符合 [编码规范](../docs/coding-style.md)
   - 设计完成后，运行代码生成器：`MMB GeneratorCode`

3. **判断是否需要设计服务**
   - 判断是否需要业务逻辑处理
   - 如果不需要，跳过本步骤
   - 如果需要，执行以下子步骤：
     - 根据 [服务设计规范](../docs/service-design.md) 设计服务接口
     - 检查是否符合 [编码规范](../docs/coding-style.md)
     - 根据 [DTO 设计规范](../docs/dto-design.md) 设计自定义 DTO
     - 根据 [服务模型设计规范](../docs/service-model-design.md) 设计服务模型
     - 检查是否符合 [编码规范](../docs/coding-style.md)
     - 运行代码生成器：`MMB GeneratorCode`
     - 根据 [服务实现规范](../docs/service-impl.md) 实现服务
       - 检查是否符合 [编码规范](../docs/coding-style.md)
       - 判断是否需要新增仓储方法
       - 如果需要，执行子步骤：
         - 根据 [仓储设计规范](../docs/repository-design.md) 设计仓储
         - 检查是否符合 [编码规范](../docs/coding-style.md)
         - 根据 [仓储实现规范](../docs/repository-impl.md) 实现仓储
         - 检查是否符合 [编码规范](../docs/coding-style.md)
     - 运行代码生成器：`MMB GeneratorCode`

4. **判断是否需要设计控制器**
   - 判断是否需要对外暴露 API 接口
   - 如果不需要，跳过本步骤
   - 如果需要，根据 [控制器设计规范](../docs/controller-design.md) 判断是否可以使用代码生成器提供的标准映射
     - 如果可以
       - 给对应服务接口添加 `[MapperController]` 特性
       - 根据 [请求模型设计规范](../docs/request-model-design.md) 设计与对应服务模型属性一致的请求模型
       - 检查是否符合 [编码规范](../docs/coding-style.md)
       - 运行代码生成器：`MMB GeneratorCode`
     - 如果不可以
       - 根据 [控制器设计规范](../docs/controller-design.md) 设计控制器接口
         - 根据 [请求模型设计规范](../docs/request-model-design.md) 设计请求模型
       - 检查是否符合 [编码规范](../docs/coding-style.md)
       - 根据 [控制器实现规范](../docs/controller-impl.md) 实现控制器
       - 检查是否符合 [编码规范](../docs/coding-style.md)
       - 运行代码生成器：`MMB GeneratorCode`

5. **尝试构建项目** - 运行 `dotnet build` 验证代码是否有编译错误
   - 如果有编译错误
     - 修复错误
     - 运行代码生成器：`MMB GeneratorCode`
     - 重新编译

### 使用子任务管理复杂需求

当需求涉及多个接口或功能点时，可以使用子任务来管理：

1. **先分析后创建**
   - 首先分析需求涉及的接口数量和复杂度
   - 确认哪些接口由代码生成器自动生成（如枚举列表接口）
   - 识别哪些接口需要手动实现

2. **创建任务列表**
   ```
   # 分析完成后创建
   TaskCreate - 分析需求和检查现有代码
   TaskCreate - 接口1实现
   TaskCreate - 接口2实现
   ...
   ```

3. **并行/串行执行**
   - 独立的需求可以并行执行（如多个不相关的接口）
   - 有依赖的需求需要串行执行（如 A 接口返回的数据是 B 接口的输入）

4. **避免重复工作**
   - 先检查是否有现成的服务或方法可以复用
   - 先确认代码生成器是否会生成相关接口

---

## 规范文档索引

按需阅读 `../docs/` 目录下的规范文档：

| 文档 | 说明 |
|------|------|
| [代码生成器指南](../docs/code-generator.md) | 代码生成器使用规范 |
| [编码规范](../docs/coding-style.md) | 编码风格、命名规范、注释规范 |
| [实体设计规范](../docs/entity-design.md) | 实体设计规范 |
| [DTO 设计规范](../docs/dto-design.md) | DTO 设计规范 |
| [服务设计规范](../docs/service-design.md) | 服务层设计规范 |
| [服务模型设计规范](../docs/service-model-design.md) | 服务模型设计规范 |
| [服务实现规范](../docs/service-impl.md) | 服务实现规范 |
| [仓储设计规范](../docs/repository-design.md) | 仓储设计规范 |
| [仓储实现规范](../docs/repository-impl.md) | 仓储实现规范 |
| [控制器设计规范](../docs/controller-design.md) | 控制器设计规范 |
| [请求模型设计规范](../docs/request-model-design.md) | 请求模型设计规范 |
| [控制器实现规范](../docs/controller-impl.md) | 控制器实现规范 |

---

## 工具使用

使用工具库前请先阅读对应工具文档（位于 `../docs/tools/` 目录），详情请参考项目级 CLAUDE.md 中的工具使用说明。

**编写代码前请先阅读对应模块的规范文档。**
