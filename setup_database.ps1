# Script PowerShell pour créer la base de données DiversityPub
Write-Host "=== Configuration de la base de données DiversityPub ===" -ForegroundColor Green

# Vérifier si SQL Server est accessible
Write-Host "Vérification de la connexion SQL Server..." -ForegroundColor Yellow

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = "Server=localhost;Integrated Security=true;TrustServerCertificate=true"
    $connection.Open()
    Write-Host "✓ Connexion SQL Server réussie" -ForegroundColor Green
    $connection.Close()
}
catch {
    Write-Host "✗ Erreur de connexion SQL Server: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Assurez-vous que SQL Server est en cours d'exécution" -ForegroundColor Yellow
    exit 1
}

# Lire le contenu du script SQL
Write-Host "Lecture du script SQL..." -ForegroundColor Yellow
$sqlScript = Get-Content "create_database.sql" -Raw

if (-not $sqlScript) {
    Write-Host "✗ Fichier create_database.sql non trouvé" -ForegroundColor Red
    exit 1
}

# Exécuter le script SQL
Write-Host "Exécution du script SQL..." -ForegroundColor Yellow

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = "Server=localhost;Integrated Security=true;TrustServerCertificate=true"
    $connection.Open()
    
    $command = New-Object System.Data.SqlClient.SqlCommand($sqlScript, $connection)
    $command.CommandTimeout = 300 # 5 minutes
    
    $result = $command.ExecuteNonQuery()
    
    Write-Host "✓ Base de données créée avec succès!" -ForegroundColor Green
    Write-Host "✓ Tables créées: $result" -ForegroundColor Green
    
    $connection.Close()
}
catch {
    Write-Host "✗ Erreur lors de la création de la base de données: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "=== Configuration terminée ===" -ForegroundColor Green
Write-Host "Vous pouvez maintenant démarrer l'application DiversityPub" -ForegroundColor Cyan
Write-Host "Identifiants par défaut:" -ForegroundColor Cyan
Write-Host "  Email: admin@diversitypub.ci" -ForegroundColor White
Write-Host "  Mot de passe: Admin123" -ForegroundColor White 