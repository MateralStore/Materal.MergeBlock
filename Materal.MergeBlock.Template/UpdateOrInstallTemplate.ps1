# 设置错误处理
$ErrorActionPreference = "Stop"

# 设置控制台编码为 UTF-8
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Materal 模板更新/安装脚本" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 定义模板名称
$templates = @(
    "Materal.MergeBlock.CoreTemplate",
    "Materal.MergeBlock.ModuleTemplate"
)

# 1. 查询当前已安装的模板列表
Write-Host "步骤 1: 查询当前已安装的模板..." -ForegroundColor Yellow
dotnet new list | Out-Null
Write-Host "已安装的模板列表已获取" -ForegroundColor Green
Write-Host ""

# 2. 检查并卸载已安装的模板
Write-Host "步骤 2: 检查并卸载已存在的模板..." -ForegroundColor Yellow
foreach ($template in $templates) {
    $templatePath = Join-Path $PSScriptRoot $template
    $absolutePath = (Resolve-Path $templatePath -ErrorAction SilentlyContinue).Path
    
    if (-not $absolutePath) {
        $absolutePath = $templatePath
    }
    
    Write-Host "  尝试卸载: $template" -ForegroundColor Cyan
    
    $ErrorActionPreference = "Continue"
    dotnet new uninstall "`"$absolutePath`"" 2>&1 | Out-Null
    $exitCode = $LASTEXITCODE
    $ErrorActionPreference = "Stop"
    
    if ($exitCode -eq 0) {
        Write-Host "  [?] 已卸载: $template" -ForegroundColor Green
    } else {
        Write-Host "  [i] 模板未安装或已卸载，将继续安装" -ForegroundColor Gray
    }
}
Write-Host ""

# 3. 安装当前目录下的模板
Write-Host "步骤 3: 安装模板..." -ForegroundColor Yellow
foreach ($template in $templates) {
    $templatePath = Join-Path $PSScriptRoot $template
    
    if (Test-Path $templatePath) {
        Write-Host "  正在安装: $template" -ForegroundColor Cyan
        $result = dotnet new install $templatePath 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  [?] 安装成功: $template" -ForegroundColor Green
        } else {
            Write-Host "  [?] 安装失败: $template" -ForegroundColor Red
            Write-Host "  错误信息: $result" -ForegroundColor Red
        }
    } else {
        Write-Host "  [?] 模板目录不存在: $templatePath" -ForegroundColor Red
    }
}
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "模板更新/安装完成!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
