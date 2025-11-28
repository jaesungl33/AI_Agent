param(
    [string]$InputFile,
    [string]$OutputFile = "CHANGELOG.md",
    [string]$Branch = "dev",
    [int]$OpenEditor = 0
)

$ErrorActionPreference = 'Stop'

# Configuration
$typeOrder = @(
    'feat', 'fix', 'perf', 'ref', 'refactor',
    'docs', 'test', 'build', 'ci', 'style', 'chore', 'revert', 'others'
)

$sectionTitles = @{
    'feat'     = '### Features'
    'fix'      = '### Bug Fixes'
    'perf'     = '### Performance Improvements'
    'ref'      = '### Improvements'
    'refactor' = '### Refactoring'
    'docs'     = '### Documentation'
    'test'     = '### Tests'
    'build'    = '### Build System'
    'ci'       = '### CI/CD'
    'style'    = '### Code Style'
    'chore'    = '### Chores'
    'revert'   = '### Reverts'
    'others'   = '### Others'
}

# Read commits
Write-Host "  Parsing commits..." -ForegroundColor Cyan
$lines = Get-Content -LiteralPath $InputFile -Encoding UTF8

$token = '(feat|fix|perf|ref|refactor|docs|chore|ci|test|build|style|revert)'
$pattern = "(?<type>$token)(\((?<scope>[^)]+)\))?:\s*(?<subject>.*?)(?=\s*(?:;|,)?\s*$token(?:\(|:)|$)"

$dateGroups = @{}

foreach ($line in $lines) {
    if ([string]::IsNullOrWhiteSpace($line)) { continue }
    
    $parts = $line -split '\|', 3
    if ($parts.Count -ne 3) { continue }
    
    $date = $parts[0].Trim()
    $hash = $parts[1].Trim()
    $subject = $parts[2].Trim()
    
    if (-not $dateGroups.ContainsKey($date)) {
        $dateGroups[$date] = @{}
        foreach ($t in $typeOrder) {
            $dateGroups[$date][$t] = @{}
        }
    }
    
    $matches = [regex]::Matches($subject, $pattern, 'IgnoreCase')
    
    if ($matches.Count -eq 0) {
        $type = 'others'
        $scope = '_'
        $msg = $subject.Trim().TrimEnd('.')
        
        if (-not $dateGroups[$date][$type].ContainsKey($scope)) {
            $dateGroups[$date][$type][$scope] = @()
        }
        $dateGroups[$date][$type][$scope] += @{msg = $msg; hash = $hash}
        continue
    }
    
    foreach ($m in $matches) {
        $type = $m.Groups['type'].Value.ToLower()
        $scope = $m.Groups['scope'].Value
        if ([string]::IsNullOrEmpty($scope)) { $scope = '_' }
        
        $msg = $m.Groups['subject'].Value.Trim().TrimEnd('.')
        
        if (-not $dateGroups[$date][$type].ContainsKey($scope)) {
            $dateGroups[$date][$type][$scope] = @()
        }
        $dateGroups[$date][$type][$scope] += @{msg = $msg; hash = $hash}
    }
}

Write-Host "  Grouped into $($dateGroups.Count) dates" -ForegroundColor Green

# Generate markdown
Write-Host "  Building markdown..." -ForegroundColor Cyan

$sb = New-Object System.Text.StringBuilder

$null = $sb.AppendLine("# Changelog")
$null = $sb.AppendLine('')
$null = $sb.AppendLine("> Generated from branch: ``$Branch``")
$null = $sb.AppendLine("> Generated at: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')")
$null = $sb.AppendLine('')
$null = $sb.AppendLine('---')
$null = $sb.AppendLine('')

$sortedDates = $dateGroups.Keys | Sort-Object -Descending

$totalDates = 0
$totalCommits = 0

foreach ($date in $sortedDates) {
    $hasContent = $false
    foreach ($t in $typeOrder) {
        if ($dateGroups[$date][$t].Count -gt 0) {
            $hasContent = $true
            break
        }
    }
    
    if (-not $hasContent) { continue }
    
    $totalDates++
    
    try {
        $dateObj = [DateTime]::ParseExact($date, 'yyyy-MM-dd', $null)
        $dayName = $dateObj.ToString('dddd', [System.Globalization.CultureInfo]::GetCultureInfo('en-US'))
        $formattedDate = "## $date ($dayName)"
    } catch {
        $formattedDate = "## $date"
    }
    
    $null = $sb.AppendLine($formattedDate)
    $null = $sb.AppendLine('')
    
    foreach ($t in $typeOrder) {
        if ($dateGroups[$date][$t].Count -eq 0) { continue }
        
        $sectionTitle = $sectionTitles[$t]
        $null = $sb.AppendLine($sectionTitle)
        $null = $sb.AppendLine('')
        
        foreach ($scopeName in $dateGroups[$date][$t].Keys) {
            $items = $dateGroups[$date][$t][$scopeName]
            if ($items.Count -eq 0) { continue }
            
            foreach ($item in $items) {
                $totalCommits++
                
                if ($scopeName -ne '_') {
                    $null = $sb.AppendLine("- **($scopeName)** $($item.msg) ``$($item.hash)``")
                } else {
                    $null = $sb.AppendLine("- $($item.msg) ``$($item.hash)``")
                }
            }
        }
        
        $null = $sb.AppendLine('')
    }
    
    $null = $sb.AppendLine('---')
    $null = $sb.AppendLine('')
}

$null = $sb.AppendLine('---')
$null = $sb.AppendLine('')
$null = $sb.AppendLine("**Total:** $totalCommits commits across $totalDates days")
$null = $sb.AppendLine('')

$txt = $sb.ToString()
$utf8BOM = New-Object System.Text.UTF8Encoding $true
[System.IO.File]::WriteAllText($OutputFile, $txt, $utf8BOM)

Write-Host "  Written to $OutputFile" -ForegroundColor Green
Write-Host "  $totalCommits commits" -ForegroundColor Green
Write-Host "  $totalDates days" -ForegroundColor Green

if ($OpenEditor -eq 1) {
    Write-Host ""
    Write-Host "  Opening in Notepad..." -ForegroundColor Yellow
    Start-Process notepad.exe -ArgumentList $OutputFile
}