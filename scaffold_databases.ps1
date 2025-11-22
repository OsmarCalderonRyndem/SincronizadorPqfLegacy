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
$INFRA_PROJ = "SincronizadorPqfLegacy.Infrastructure\SincronizadorPqfLegacy.Infrastructure.csproj"
$API_PROJ = "SincronizadorPqfLegacy.API\SincronizadorPqfLegacy.API.csproj"  
$APPSETTINGS_PATH = "SincornizadorPqfLegacy.API\appsettings.json"
$BASE_NAMESPACE = "SincornizadorPqfLegacy.Infrastructure.Persistence"

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

    $tablesToScaffold = $null
    $filterTables = Read-Host "¿Desea seleccionar tablas específicas? (s/n) [Default: n]"
    if ($filterTables.ToLower() -eq "s") {
        $inputTables = Read-Host "Ingrese los nombres de las tablas separados por coma (ej: Users, dbo.Orders)"
        $tablesToScaffold = ($inputTables -split ",") | ForEach-Object { $.Trim() } | Where-Object { -not [string]::IsNullOrWhiteSpace($) }
        
        if ($tablesToScaffold.Count -eq 0) {
            Write-Host "No se ingresaron tablas. Se cancela la operación." -ForegroundColor Yellow
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
