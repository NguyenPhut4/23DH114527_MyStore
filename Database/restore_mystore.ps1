# =====================================================================
#  KHOI PHUC DATABASE MyStore TU FILE BACKUP (.bak)
#  - Khong truyen -BakFile: tu lay ban backup MOI NHAT cua MyStore.
#  - CANH BAO: du lieu hien tai cua MyStore se bi GHI DE bang du lieu trong file backup.
#  - Dung duoc ca khi database MyStore da bi xoa hoan toan (se tao lai).
#
#  Cach chay:  chuot phai -> Run with PowerShell
#         hoac: powershell -ExecutionPolicy Bypass -File Database\restore_mystore.ps1 [-BakFile "D:\...\MyStore_x.bak"]
# =====================================================================
param(
    [string]$Server = "$env:COMPUTERNAME\LAPTOP",
    [string]$Database = "MyStore",
    [string]$BakFile = "",
    [switch]$Force
)

if (-not $BakFile) {
    $BakFile = (& sqlcmd -S $Server -E -C -h -1 -W -Q @"
SET NOCOUNT ON;
SELECT TOP 1 m.physical_device_name
FROM msdb.dbo.backupset b JOIN msdb.dbo.backupmediafamily m ON m.media_set_id = b.media_set_id
WHERE b.database_name = N'$Database' AND b.type = 'D'
ORDER BY b.backup_finish_date DESC;
"@ | Where-Object { $_ -match '\.bak$' } | Select-Object -First 1)
}
if (-not $BakFile) {
    Write-Host "Khong tim thay file backup nao cua $Database. Hay chay backup_mystore.ps1 truoc." -ForegroundColor Red
    exit 1
}

Write-Host "Se khoi phuc [$Database] tren $Server tu:`n  $BakFile" -ForegroundColor Yellow
Write-Host "Du lieu hien tai cua $Database se bi GHI DE." -ForegroundColor Yellow
if (-not $Force) {
    $ans = Read-Host "Go YES de tiep tuc"
    if ($ans -ne 'YES') { Write-Host "Da huy."; exit 0 }
}

$query = @"
SET NOCOUNT ON;
IF DB_ID(N'$Database') IS NOT NULL
    ALTER DATABASE [$Database] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
RESTORE DATABASE [$Database] FROM DISK = N'$BakFile' WITH REPLACE, CHECKSUM, STATS = 50;
ALTER DATABASE [$Database] SET MULTI_USER;
"@
& sqlcmd -S $Server -E -C -b -d master -Q $query
if ($LASTEXITCODE -eq 0) {
    Write-Host "`nKhoi phuc thanh cong." -ForegroundColor Green
} else {
    Write-Host "`nKhoi phuc THAT BAI. Neu DB dang o che do SINGLE_USER, chay:" -ForegroundColor Red
    Write-Host "  sqlcmd -S $Server -E -C -Q `"ALTER DATABASE [$Database] SET MULTI_USER`"" -ForegroundColor Red
}
if ($Host.Name -eq 'ConsoleHost' -and -not $Force) { Read-Host "`nNhan Enter de dong" }
