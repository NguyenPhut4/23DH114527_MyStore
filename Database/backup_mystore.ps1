# =====================================================================
#  SAO LUU DATABASE MyStore
#  Tao file MyStore_<ngay>_<gio>.bak trong thu muc Backup mac dinh cua SQL Server.
#
#  Cach chay:  chuot phai -> Run with PowerShell
#         hoac: powershell -ExecutionPolicy Bypass -File Database\backup_mystore.ps1 -Server "TEN_MAY\SQLEXPRESS"
# =====================================================================
param(
    [string]$Server = "$env:COMPUTERNAME\LAPTOP",
    [string]$Database = "MyStore"
)

$stamp = Get-Date -Format "yyyyMMdd_HHmmss"
$query = @"
SET NOCOUNT ON;
DECLARE @dir NVARCHAR(4000) = CAST(SERVERPROPERTY('InstanceDefaultBackupPath') AS NVARCHAR(4000));
DECLARE @file NVARCHAR(4000) = @dir + N'\$($Database)_$stamp.bak';
BACKUP DATABASE [$Database] TO DISK = @file WITH COPY_ONLY, INIT, CHECKSUM, STATS = 50;
RESTORE VERIFYONLY FROM DISK = @file WITH CHECKSUM;
SELECT N'FILE=' + @file;
"@

Write-Host "Dang sao luu [$Database] tren $Server ..." -ForegroundColor Cyan
$out = & sqlcmd -S $Server -E -C -b -h -1 -W -Q $query 2>&1
$out | ForEach-Object { Write-Host "  $_" }

if ($LASTEXITCODE -eq 0) {
    $file = ($out | Where-Object { $_ -like 'FILE=*' }) -replace '^FILE=', ''
    Write-Host "`nSao luu thanh cong: $file" -ForegroundColor Green
    Write-Host "Khoi phuc: chay Database\restore_mystore.ps1" -ForegroundColor Gray
} else {
    Write-Host "`nSao luu THAT BAI (xem thong bao o tren)." -ForegroundColor Red
}
if ($Host.Name -eq 'ConsoleHost' -and -not $env:NO_PAUSE) { Read-Host "`nNhan Enter de dong" }
