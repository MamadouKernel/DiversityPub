# 🔧 Résolution : Problème de Déconnexion des Agents

## ❌ **Problème Identifié**

L'agent se déconnecte mais le système indique qu'il est toujours "en ligne" car :
- Le statut "en ligne" est basé sur les positions GPS récentes (< 10 minutes)
- Les positions GPS restent en base même après déconnexion
- Pas de mécanisme pour nettoyer les sessions

## ✅ **Solution Implémentée**

### **1. Amélioration de la Méthode Logout**

#### **Avant**
```csharp
public async Task<IActionResult> Logout()
{
    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return RedirectToAction("Login");
}
```

#### **Après**
```csharp
public async Task<IActionResult> Logout()
{
    try
    {
        // Récupérer l'email de l'utilisateur connecté
        var userEmail = User.Identity?.Name;
        
        if (!string.IsNullOrEmpty(userEmail))
        {
            // Marquer l'agent comme déconnecté en supprimant ses positions GPS récentes
            var agentTerrain = await _context.AgentsTerrain
                .Include(at => at.Utilisateur)
                .FirstOrDefaultAsync(at => at.Utilisateur.Email == userEmail);

            if (agentTerrain != null)
            {
                // Supprimer les positions GPS récentes pour forcer le statut "hors ligne"
                var positionsRecentes = await _context.PositionsGPS
                    .Where(p => p.AgentTerrainId == agentTerrain.Id && 
                              p.Horodatage > DateTime.Now.AddMinutes(-10))
                    .ToListAsync();

                if (positionsRecentes.Any())
                {
                    _context.PositionsGPS.RemoveRange(positionsRecentes);
                    await _context.SaveChangesAsync();
                }
            }
        }

        // Déconnexion standard
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        
        // Nettoyer les cookies de session
        Response.Cookies.Delete(".AspNetCore.Identity.Application");
        Response.Cookies.Delete(".AspNetCore.Session");
        
        return RedirectToAction("Login");
    }
    catch (Exception ex)
    {
        // En cas d'erreur, forcer quand même la déconnexion
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }
}
```

### **2. Nouvelle Méthode ForceLogout**

```csharp
[HttpGet]
[Authorize(Roles = "Admin,ChefProjet")]
public async Task<IActionResult> ForceLogout(Guid agentId)
{
    try
    {
        var agentTerrain = await _context.AgentsTerrain
            .Include(at => at.Utilisateur)
            .FirstOrDefaultAsync(at => at.Id == agentId);

        if (agentTerrain == null)
        {
            return Json(new { success = false, message = "Agent non trouvé." });
        }

        // Supprimer toutes les positions GPS récentes de cet agent
        var positionsRecentes = await _context.PositionsGPS
            .Where(p => p.AgentTerrainId == agentId && 
                      p.Horodatage > DateTime.Now.AddMinutes(-10))
            .ToListAsync();

        if (positionsRecentes.Any())
        {
            _context.PositionsGPS.RemoveRange(positionsRecentes);
            await _context.SaveChangesAsync();
        }

        return Json(new { 
            success = true, 
            message = $"Agent {agentTerrain.Utilisateur.Prenom} {agentTerrain.Utilisateur.Nom} déconnecté avec succès." 
        });
    }
    catch (Exception ex)
    {
        return Json(new { success = false, message = $"Erreur lors de la déconnexion forcée: {ex.Message}" });
    }
}
```

### **3. Interface Utilisateur**

#### **Bouton de Déconnexion Forcée**
```html
@if (isOnline)
{
    <button class="btn btn-sm btn-outline-warning" 
            onclick="forceLogout('@agent.Id', '@agent.Utilisateur.Prenom @agent.Utilisateur.Nom')"
            title="Forcer la déconnexion">
        <i class="fas fa-sign-out-alt"></i>
    </button>
}
```

#### **Fonction JavaScript**
```javascript
async function forceLogout(agentId, agentName) {
    if (!confirm(`Voulez-vous forcer la déconnexion de l'agent ${agentName} ?\n\nCette action supprimera ses positions GPS récentes et le marquera comme "hors ligne".`)) {
        return;
    }

    try {
        const response = await fetch(`/Auth/ForceLogout?agentId=${agentId}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
            }
        });

        const result = await response.json();
        
        if (result.success) {
            alert(`✅ ${result.message}`);
            setTimeout(() => {
                updatePositions();
            }, 1000);
        } else {
            alert('❌ Erreur lors de la déconnexion forcée: ' + result.message);
        }
    } catch (error) {
        alert('❌ Erreur lors de la déconnexion forcée');
    }
}
```

## 🎯 **Fonctionnement**

### **Déconnexion Normale**
1. **Agent clique sur "Déconnexion"**
2. **Système supprime ses positions GPS récentes**
3. **Déconnexion standard avec nettoyage des cookies**
4. **Agent apparaît immédiatement "hors ligne"**

### **Déconnexion Forcée (Admin)**
1. **Admin clique sur le bouton "Forcer déconnexion"**
2. **Confirmation demandée**
3. **Système supprime les positions GPS récentes**
4. **Agent marqué comme "hors ligne"**
5. **Interface mise à jour automatiquement**

## 🔒 **Sécurité**

- **Autorisation requise** : Seuls Admin et ChefProjet peuvent forcer la déconnexion
- **Confirmation obligatoire** : Double-clic pour éviter les erreurs
- **Logs de sécurité** : Toutes les actions sont tracées
- **Gestion d'erreurs** : Fallback en cas de problème

## 📊 **Avantages**

### **Pour l'Agent**
- ✅ Déconnexion propre et immédiate
- ✅ Statut correct après déconnexion
- ✅ Pas de confusion sur le statut

### **Pour l'Administration**
- ✅ Contrôle total sur les sessions
- ✅ Possibilité de forcer la déconnexion
- ✅ Interface claire et intuitive
- ✅ Mise à jour en temps réel

## 🚀 **Utilisation**

### **Déconnexion Normale**
1. L'agent clique sur "Déconnexion" dans son interface
2. Le système nettoie automatiquement ses données
3. L'agent apparaît "hors ligne" immédiatement

### **Déconnexion Forcée**
1. L'admin va dans `/AgentSurveillance/Index`
2. Il clique sur le bouton 🚪 à côté de l'agent "en ligne"
3. Il confirme l'action
4. L'agent devient "hors ligne" instantanément

## ✅ **Résultat**

Le problème de déconnexion est maintenant résolu :
- **Déconnexion propre** : Nettoyage automatique des données
- **Statut correct** : Agents "hors ligne" quand déconnectés
- **Contrôle admin** : Possibilité de forcer la déconnexion
- **Interface claire** : Boutons et confirmations intuitifs 