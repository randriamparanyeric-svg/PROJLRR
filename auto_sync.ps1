# ==========================================================================
#    APPLICATION DE SYNCHRONISATION PROFESSIONNELLE — EDITION ERIC GERALDIN v3.6
# ==========================================================================
Clear-Host

# ─── CONFIGURATION DES CHEMINS ET ACCES ───────────────────────────────────
$dbDownloads   = "C:\Users\HP\Downloads\PERSLRRSANSCODE.db"
$dbLocal       = "C:\Users\HP\PROJLRR\PERSLRRSANSCODE.db"
$sqlScript     = "C:\Users\HP\PROJLRR\sync.sql"
$sqliteExe     = "C:\Users\HP\PROJLRR\sqlite3.exe"
$localWwwroot  = "C:\Users\HP\PROJLRR\wwwroot"

$ftpServerDb   = "ftp://site71333.siteasp.net/wwwroot/wwwroot/PERSLRRSANSCODE.db"
$ftpBaseUrl    = "ftp://site71333.siteasp.net/wwwroot/wwwroot"
$ftpUser       = "site71333"
$ftpPass       = 'x!9L@Rf2d4G+'

$heureDebut    = Get-Date -Format "HH:mm:ss"
$chrono        = [System.Diagnostics.Stopwatch]::StartNew()
$ftpAuth       = "$($ftpUser):$($ftpPass)"

# ─── DECORATION ALPHANUMERIQUE : ERIC GERALDIN ────────────────────────────
Write-Host " -----------------------------------------------------------------------" -ForegroundColor DarkGray
Write-Host "  _____ ____  ___ ____    ____ _____ ____    _    _     ____ ___ _   _" -ForegroundColor Magenta
Write-Host " | ____|  _ \|_ _/ ___|  / ___| ____|  _ \  / \  | |   |  _ \_ _| \ | |" -ForegroundColor Magenta
Write-Host " |  _| | |_) || | |     | |  _|  _| | |_) |/ _ \ | |   | | | | ||  \| |" -ForegroundColor Cyan
Write-Host " | |___|  _ < | | |___  | |_| | |___|  _ </ ___ \| |___| |_| | || |\  |" -ForegroundColor Cyan
Write-Host " |_____|_| \_\___\____|  \____|_____|_| \_/_/   \_\_____|____/___|_| \_|" -ForegroundColor Magenta
Write-Host " -----------------------------------------------------------------------" -ForegroundColor DarkGray
Write-Host "  [►] Lancement        : $heureDebut" -ForegroundColor Gray
Write-Host "  [►] Instance-Moteur  : MonsterASP Deep Smart Sync Engine" -ForegroundColor Gray
Write-Host "  [►] Fichier Cible    : PERSLRRSANSCODE.db" -ForegroundColor Gray
Write-Host "  [►] Mode Executif    : Bidirectionnel Dynamique (Last Write Wins)" -ForegroundColor Green
Write-Host " -----------------------------------------------------------------------" -ForegroundColor DarkGray
Write-Host ""

# ─── ETAPE 1 : TELECHARGEMENT DE LA BASE ──────────────────────────────────
Write-Host " [1/4] ── Connexion FTP & Recuperation de la base distante..." -ForegroundColor Yellow
Write-Progress -Activity "Moteur de Synchronisation ERIC GERALDIN" -Status "Telechargement de la base de donnees depuis MonsterASP..." -PercentComplete 15

curl.exe --ftp-pasv -u $ftpAuth -o $dbDownloads $ftpServerDb 2>$null

if (Test-Path $dbDownloads) {
    Write-Host "   [OK] Base distante telechargee avec succes." -ForegroundColor Green
    Write-Host ""

    # ─── ETAPE 2 : SYNCHRONISATION SQL ────────────────────────────────────
    Write-Host " [2/4] ── Fusion des tables et resolution des conflits..." -ForegroundColor Yellow
    Write-Progress -Activity "Moteur de Synchronisation ERIC GERALDIN" -Status "Traitement et fusion des tables SQL (Tri par Date)..." -PercentComplete 40
    
    & $sqliteExe $dbLocal ".read $sqlScript"
    
    Write-Host "   [OK] Traitement des modifications SQL termine." -ForegroundColor Green
    Write-Host ""

    # ─── ETAPE 3 : RENVOI DE LA BASE REELLEMENT MODIFIEE ──────────────────
    Write-Host " [3/4] ── Televersement de la base mise a jour vers le Cloud..." -ForegroundColor Yellow
    Write-Progress -Activity "Moteur de Synchronisation ERIC GERALDIN" -Status "Securisation et envoi de la base unifiee vers le serveur..." -PercentComplete 65
    
    curl.exe --ftp-pasv -T $dbLocal -u $ftpAuth $ftpServerDb 2>$null
    
    Write-Host "   [OK] Serveur cloud mis a jour avec succes." -ForegroundColor Green
    Write-Host ""

    # ─── ETAPE 4 : SYNCHRONISATION DES MEDIAS INTELLIGENTE ────────────────
    Write-Host " [4/4] ── Alignement et verification des repertoires medias..." -ForegroundColor Yellow
    
    $folders = @("photos", "signatures", "uploads")
    $totalFolders = $folders.Count
    $currentFolderIndex = 0
    
    foreach ($folder in $folders) {
        $currentFolderIndex++
        $progressionMedias = 65 + [int](($currentFolderIndex / $totalFolders) * 30)
        Write-Progress -Activity "Moteur de Synchronisation ERIC GERALDIN" -Status "Analyse structurelle du dossier : $folder" -PercentComplete $progressionMedias
        
        $localFolder = "$localWwwroot\$folder"
        $remoteFolderUrl = "$ftpBaseUrl/$folder"
        
        if (!(Test-Path $localFolder)) {
            New-Item -ItemType Directory -Path $localFolder | Out-Null
        }
        
        Write-Host "   -> Analyse du dossier : [$folder]" -ForegroundColor Cyan
        
        # 1. Recuperation de la liste des fichiers ET de leurs tailles sur le FTP
        $remoteListing = curl.exe -s --ftp-pasv -u $ftpAuth "$remoteFolderUrl/"
        $remoteFiles = @{}
        
        foreach ($line in $remoteListing) {
            if ([string]::IsNullOrWhiteSpace($line)) { continue }
            
            $rName = $null
            $rSize = 0
            $isDir = $false

            if ($line -match '^\d{2}-\d{2}-\d{2}\s+\d{2}:\d{2}[AP]M\s+(<DIR>|\d+)\s+(.+)$') {
                $sizeOrDir = $Matches[1]
                $rName = $Matches[2].Trim()
                if ($sizeOrDir -eq "<DIR>") { $isDir = $true } else { $rSize = [int64]$sizeOrDir }
            }
            elseif ($line -match '^([-d])[-rwx]{9}\s+\d+\s+\S+\s+\S+\s+(\d+)\s+[A-Za-z]{3}\s+\d+\s+[\d:]+\s+(.+)$') {
                $dirChar = $Matches[1]
                $rSize = [int64]$Matches[2]
                $rName = $Matches[3].Trim()
                if ($dirChar -eq "d") { $isDir = $true }
            }
            
            if ($null -eq $rName) {
                $parts = $line -split '\s+' | Where-Object { $_ -ne "" }
                if ($parts.Count -lt 3) { continue }
                if ($line -match "<DIR>" -or $line -like "d*") { continue }
                $rName = $parts[-1]
                $rSize = 0
            }

            if ($isDir -or $rName -match "^\.+$") { continue }
            $remoteFiles[$rName] = $rSize
        }
        
        # 2. ENVOI PC -> VERS FTP
        $localFiles = Get-ChildItem $localFolder -File
        foreach ($file in $localFiles) {
            if (!$remoteFiles.ContainsKey($file.Name)) {
                Write-Host "          [+] Envoi du nouveau fichier local : $($file.Name)" -ForegroundColor Gray
                $escapedName = [Uri]::EscapeDataString($file.Name)
                curl.exe --ftp-pasv -T "$($file.FullName)" -u $ftpAuth "$remoteFolderUrl/$escapedName" 2>$null
            }
        }
        
        # 3. RECEPTION FTP -> VERS PC
        foreach ($rName in $remoteFiles.Keys) {
            $localFilePath = "$localFolder\$rName"
            $remoteSize = $remoteFiles[$rName]
            $needDownload = $false
            
            if (!(Test-Path $localFilePath)) {
                $needDownload = $true
                Write-Host "          [+] Recuperation du nouveau fichier distant : $rName" -ForegroundColor Gray
            } else {
                $localFile = Get-Item $localFilePath
                if ($localFile.Length -ne $remoteSize) {
                    $needDownload = $true
                    Write-Host "          [!] Nouvelle photo en ligne detectee : $rName. Mise a jour du PC..." -ForegroundColor Yellow
                }
            }
            
            if ($needDownload) {
                if (Test-Path $localFilePath) { Remove-Item $localFilePath -Force | Out-Null }
                $escapedName = [Uri]::EscapeDataString($rName)
                curl.exe --ftp-pasv -u $ftpAuth -o $localFilePath "$remoteFolderUrl/$escapedName" 2>$null
            }
        }
    }
    
    Write-Progress -Activity "Moteur de Synchronisation ERIC GERALDIN" -Status "Traitement complet effectue !" -PercentComplete 100
    Write-Host "   [OK] Synchronisation des dossiers medias terminee." -ForegroundColor Green

    # ─── RAPPORT FINAL ────────────────────────────────────────────────────
    $chrono.Stop()
    $tempsTotal = [math]::Round($chrono.Elapsed.TotalSeconds, 2)
    Write-Progress -Activity "Moteur de Synchronisation ERIC GERALDIN" -Completed

    Write-Host ""
    Write-Host " =======================================================================" -ForegroundColor Gray
    Write-Host "   >>> SYNCHRONISATION APPLIQUEE AVEC SUCCES (MODE INTENT) <<<" -ForegroundColor Green
    Write-Host "   [+] Temps total du traitement : $tempsTotal secondes" -ForegroundColor Gray
    Write-Host " =======================================================================" -ForegroundColor Gray

} else {
    $chrono.Stop()
    $heureEchec = Get-Date -Format "HH:mm:ss"
    Write-Progress -Activity "Moteur de Synchronisation ERIC GERALDIN" -Completed
    Write-Host ""
    Write-Host " =======================================================================" -ForegroundColor Red
    Write-Host "   [-] ERREUR CRITIQUE : Le telechargement a echoue ($heureEchec)." -ForegroundColor Red
    Write-Host "   -> Tentative abandonnee : Pas de connexion internet ou FTP inaccessible." -ForegroundColor DarkRed
    Write-Host " =======================================================================" -ForegroundColor Red
    Write-Host ""
}

Start-Sleep -Seconds 4