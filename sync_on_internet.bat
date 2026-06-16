@echo off
chcp 65001 > nul
title Détecteur Réseau Professionnel - PROJLRR

REM Initialisation du compteur de temps (1 heure max)
set compteur=0
set max_tentatives=550

:BOUCLE
cls
echo ===================================================
echo   [ÉCOUTE] Vérification de la connectivité...
echo ===================================================
echo.
echo Test en cours (Vrai routage Internet)...
echo Tentative : %compteur% / %max_tentatives% (~1h maximum)
echo.

REM Test .NET ultra-fiable : ping vers Google
powershell -Command "$p = New-Object System.Net.NetworkInformation.Ping; try { if ($p.Send('8.8.8.8', 3000).Status -eq 'Success') { exit 0 } else { exit 1 } } catch { exit 1 }"

REM Si le code de retour n'est pas 0, on va à la section d'attente
if NOT "%errorlevel%"=="0" goto PAS_INTERNET


:REUSSITE
echo.
echo ---------------------------------------------------
echo   [CONNEXION VALIDÉE] Vrai Internet disponible ! 🎉
echo   Lancement de la synchronisation...
echo ---------------------------------------------------
echo.

REM Lancement de votre PowerShell de synchronisation
powershell -ExecutionPolicy Bypass -File "C:\Users\HP\PROJLRR\auto_sync.ps1"

echo.
echo ===================================================
echo   [FIN] Synchronisation terminée.
echo ===================================================
echo.
pause
exit


:PAS_INTERNET
echo.
echo [%TIME%] ❌ Pas de vrai Internet détecté.

REM On augmente le compteur de 1
set /a compteur+=1

REM Si le compteur atteint le maximum (1 heure écoulée), on abandonne
if %compteur% GEQ %max_tentatives% goto ABANDON_TIMEOUT

echo           Nouvelle vérification dans 5 secondes...
timeout /t 5 /nobreak >nul
goto BOUCLE


:ABANDON_TIMEOUT
echo.
echo ===================================================
echo   [ALERTE - TIMEOUT] Aucun Internet depuis 1 heure.
echo   Arrêt de sécurité pour préserver la batterie.
echo ===================================================
echo.
echo La fenêtre va se fermer automatiquement...
timeout /t 10
exit