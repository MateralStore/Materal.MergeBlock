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
# 1. 安装当前目录下的模板
Write-Host "安装模板..." -ForegroundColor Yellow
foreach ($template in $templates) {
    $templatePath = Join-Path $PSScriptRoot $template
    
    if (Test-Path $templatePath) {
        Write-Host "  正在安装: $template" -ForegroundColor Cyan
        $result = dotnet new install $templatePath --force 2>&1
        
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
