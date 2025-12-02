param (
    [string]$SpecificDatabase 
)

chcp 65001 | Out-Null
$OutputEncoding = [Console]::OutputEncoding = [Console]::InputEncoding = [System.Text.Encoding]::UTF8

Write-Host "========================================================" -ForegroundColor Cyan
Write-Host "          ProquifaDotNet.ETL EF Core Scaffolding"
Write-Host "========================================================`n" -ForegroundColor Cyan

# ========================================================
# 1. Configuración general
# ========================================================
$INFRA_PROJ = "Infrastructure\SincronizadorPqfLegacy.Infrastructure.csproj"
$API_PROJ = "API\SincronizadorPqfLegacy.API.csproj"  
$APPSETTINGS_PATH = "API\appsettings.json"
$BASE_NAMESPACE = "Infrastructure.Persistence"

# Configuración de las bases de datos a procesar
$databases = @(
    @{
        ConnName         = "PConnectProquifaDotNet"
        ContextName      = "PConnectProquifaDotNetContext"
        ContextDir       = "Persistence/PConnectProquifaDotNet/Contexts"
        EntitiesDir      = "Persistence/PConnectProquifaDotNet/Entities"
        Namespace        = "$BASE_NAMESPACE.PConnectProquifaDotNet.Entities"
        ContextNamespace = "$BASE_NAMESPACE.PConnectProquifaDotNet.Contexts"
    },
    @{
        ConnName         = "ProquifaDotNet"
        ContextName      = "ProquifaDotNetContext"
        ContextDir       = "Persistence/ProquifaDotNet/Contexts"
        EntitiesDir      = "Persistence/ProquifaDotNet/Entities"
        Namespace        = "$BASE_NAMESPACE.ProquifaDotNet.Entities"
        ContextNamespace = "$BASE_NAMESPACE.ProquifaDotNet.Contexts"
    },
    @{
        ConnName         = "PConnect"
        ContextName      = "PConnectContext"
        ContextDir       = "Persistence/PConnect/Contexts"
        EntitiesDir      = "Persistence/PConnect/Entities"
        Namespace        = "$BASE_NAMESPACE.PConnect.Entities"
        ContextNamespace = "$BASE_NAMESPACE.PConnect.Contexts"
    }
)

# ========================================================
# 2. Verificar proyectos
# ========================================================
Write-Host "[INFO] Verificando archivos del proyecto..." -ForegroundColor Yellow
if (-not (Test-Path $INFRA_PROJ)) {
    Write-Host "[ERROR] No se encontró el proyecto: $INFRA_PROJ" -ForegroundColor Red
    exit 1
}
if (-not (Test-Path $API_PROJ)) {
    Write-Host "[ERROR] No se encontró el proyecto: $API_PROJ" -ForegroundColor Red
    exit 1
}
if (-not (Test-Path $APPSETTINGS_PATH)) {
    Write-Host "[ERROR] No se encontró appsettings.json en: $APPSETTINGS_PATH" -ForegroundColor Red
    exit 1
}

Write-Host "[INFO] Compilando proyecto API antes del scaffold..." -ForegroundColor Yellow
dotnet build "$API_PROJ"
if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] Falló la compilación del proyecto. Revise los errores arriba." -ForegroundColor Red
    exit 1
}
Write-Host "[OK] Proyectos verificados`n" -ForegroundColor Green

# ========================================================
# 3. Leer ConnectionStrings
# ========================================================
Write-Host "[INFO] Leyendo ConnectionStrings de $APPSETTINGS_PATH..." -ForegroundColor Yellow
try {
    $content = Get-Content $APPSETTINGS_PATH -Raw -Encoding UTF8
    # Limpieza básica de comentarios JSON
    $content = $content -replace '(?s)/\*.*?\*/', ''
    $content = $content -replace '(?m)^\s*//.*$', ''
    
    $json = $content | ConvertFrom-Json
    
    if (-not $json.ConnectionStrings) {
        Write-Host "[ERROR] No se encontró la sección 'ConnectionStrings' en appsettings.json" -ForegroundColor Red
        exit 1
    }
}
catch {
    Write-Host "[ERROR] Error leyendo appsettings.json: $_" -ForegroundColor Red
    exit 1
}

# ========================================================
# 4. Función de Scaffolding
# ========================================================

function Run-Scaffold {
    param (
        $dbConfig,
        $tables
    )

    $connString = $json.ConnectionStrings.$($dbConfig.ConnName)
    if (-not $connString) {
        Write-Host "[ERROR] No se encontró ConnectionString para '$($dbConfig.ConnName)'." -ForegroundColor Red
        return
    }

    Write-Host "--------------------------------------------------------"
    Write-Host "Procesando: $($dbConfig.ConnName)" -ForegroundColor Cyan
    Write-Host "Context:    $($dbConfig.ContextName)"
    Write-Host "ContextDir: $($dbConfig.ContextDir)"
    Write-Host "EntitiesDir:$($dbConfig.EntitiesDir)"
    if ($tables) {
        Write-Host "Tablas:     $($tables -join ', ')" -ForegroundColor Yellow
    }
    else {
        Write-Host "Tablas:     TODAS" -ForegroundColor Green
    }
    Write-Host "--------------------------------------------------------"

    $escapedConnectionString = '"' + $connString + '"'
    
    $arguments = @(
        "ef", "dbcontext", "scaffold", $escapedConnectionString, "Microsoft.EntityFrameworkCore.SqlServer",
        "--project", "$INFRA_PROJ",
        "--startup-project", "$API_PROJ",
        "--output-dir", "$($dbConfig.EntitiesDir)",
        "--context-dir", "$($dbConfig.ContextDir)",
        "--context", "$($dbConfig.ContextName)",
        "--namespace", "$($dbConfig.Namespace)",
        "--context-namespace", "$($dbConfig.ContextNamespace)",
        "--use-database-names",
        "--data-annotations",
        "--force",
        "--no-onconfiguring"
    )

    if ($tables) {
        foreach ($t in $tables) {
            $arguments += "--table"
            $arguments += $t
        }
    }

    Write-Host "[CMD] dotnet $($arguments -join ' ')" -ForegroundColor DarkGray
    
    Start-Process "dotnet" -ArgumentList $arguments -Wait -NoNewWindow

    if ($LASTEXITCODE -eq 0) {
        Write-Host "[OK] Scaffold exitoso para $($dbConfig.ConnName)" -ForegroundColor Green
        
        # Limpieza OnConfiguring
        $contextFile = Join-Path (Split-Path $INFRA_PROJ -Parent) "$($dbConfig.ContextDir)\$($dbConfig.ContextName).cs"
        if (Test-Path $contextFile) {
            $ctxContent = Get-Content $contextFile -Raw
            if ($ctxContent -match "protected override void OnConfiguring") {
                Write-Host "[INFO] Comentando OnConfiguring..."
                $newCtxContent = $ctxContent -replace '(?s)protected override void OnConfiguring.*?protected override void OnModelCreating', @"
        /*
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }
        */

        protected override void OnModelCreating
"@
                Set-Content $contextFile $newCtxContent -Encoding UTF8
            }
        }
    }
    else {
        Write-Host "[ERROR] Falló el scaffold." -ForegroundColor Red
    }
}

# ========================================================
# 5. Menú Interactivo
# ========================================================

function Get-ExistingTableNames {
    param (
        $dbConfig
    )

    $infraRoot = Split-Path $INFRA_PROJ -Parent
    $entitiesPath = Join-Path $infraRoot $dbConfig.EntitiesDir
    $contextPath = Join-Path $infraRoot "$($dbConfig.ContextDir)\$($dbConfig.ContextName).cs"

    if (-not (Test-Path $entitiesPath)) { return @() }
    
    $contextContent = Get-Content $contextPath -Raw
    $tables = @()

    $files = Get-ChildItem $entitiesPath -Filter "*.cs"
    foreach ($file in $files) {
        $content = Get-Content $file.FullName -Raw
        
        # 1. Try to find [Table("Name")]
        if ($content -match '\[Table\("([^"]+)"') {
            $tables += $matches[1]
        }
        else {
            # 2. Find class name and look up in Context
            if ($content -match 'public partial class (\w+)') {
                $className = $matches[1]
                # Look for DbSet<ClassName> PropertyName
                if ($contextContent -match "DbSet<$className>\s+(\w+)") {
                    $tables += $matches[1]
                }
            }
        }
    }

    return $tables | Select-Object -Unique
}

while ($true) {
    Write-Host "`nSeleccione la base de datos para Scaffold:" -ForegroundColor Cyan
    $i = 1
    foreach ($db in $databases) {
        Write-Host "$i. $($db.ConnName)"
        $i++
    }
    Write-Host "Q. Salir"

    $choice = Read-Host "Opción"
    if ($choice -match "^[qQ]") { break }

    try {
        $index = [int]$choice - 1
    }
    catch {
        Write-Host "Entrada no válida." -ForegroundColor Red
        continue
    }

    if ($index -lt 0 -or $index -ge $databases.Count) {
        Write-Host "Opción inválida." -ForegroundColor Red
        continue
    }

    $selectedDb = $databases[$index]

    Write-Host "`nModo de operación:" -ForegroundColor Cyan
    Write-Host "1. Scaffold Completo (Todas las tablas de la BD)"
    Write-Host "2. Seleccionar tablas específicas (Sobrescribe Contexto)"
    Write-Host "3. Agregar tablas (Mantiene existentes + Nuevas)"
    
    $mode = Read-Host "Seleccione opción [Default: 1]"
    
    $tablesToScaffold = $null

    if ($mode -eq "2") {
        $inputTables = Read-Host "Ingrese las tablas (separadas por coma)"
        $tablesToScaffold = ($inputTables -split ",") | ForEach-Object { $_.Trim() } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
    }
    elseif ($mode -eq "3") {
        Write-Host "Detectando tablas existentes..." -ForegroundColor Yellow
        $existing = Get-ExistingTableNames -dbConfig $selectedDb
        
        if ($existing.Count -gt 0) {
            Write-Host "Existentes: $($existing -join ', ')" -ForegroundColor Gray
        }

        $inputTables = Read-Host "Ingrese NUEVAS tablas (separadas por coma)"
        $newTables = ($inputTables -split ",") | ForEach-Object { $_.Trim() } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }

        if ($newTables.Count -gt 0) {
            $tablesToScaffold = $existing + $newTables
            $tablesToScaffold = $tablesToScaffold | Select-Object -Unique
        }
        else {
            Write-Host "No se ingresaron tablas nuevas." -ForegroundColor Red
            continue
        }
    }

    Run-Scaffold -dbConfig $selectedDb -tables $tablesToScaffold
    
    Write-Host "`nPresione Enter para continuar..."
    Read-Host
}

Write-Host "========================================================"
Write-Host "Proceso finalizado" -ForegroundColor Cyan
Write-Host "========================================================"
