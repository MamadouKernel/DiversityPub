/**
 * Script pour la récupération de la position GPS réelle
 * Utilise l'API Geolocation du navigateur pour obtenir la vraie position
 */

class GPSLocationService {
    constructor() {
        this.isTracking = false;
        this.trackingInterval = null;
        this.updateInterval = 30000; // 30 secondes par défaut
        this.onPositionUpdate = null;
    }

    /**
     * Demande l'autorisation et récupère la position actuelle
     * @returns {Promise<Position>} Position GPS
     */
    async getCurrentPosition() {
        return new Promise((resolve, reject) => {
            if (!navigator.geolocation) {
                reject(new Error('Géolocalisation non supportée par ce navigateur'));
                return;
            }

            const options = {
                enableHighAccuracy: true,  // Précision maximale
                timeout: 10000,           // Timeout de 10 secondes
                maximumAge: 60000         // Cache de 1 minute
            };

            navigator.geolocation.getCurrentPosition(
                (position) => {
                    console.log('Position GPS récupérée:', position);
                    resolve(position);
                },
                (error) => {
                    console.error('Erreur de géolocalisation:', error);
                    reject(error);
                },
                options
            );
        });
    }

    /**
     * Démarre le suivi continu de la position
     * @param {number} interval - Intervalle de mise à jour en millisecondes
     * @param {Function} callback - Fonction appelée à chaque mise à jour
     */
    startTracking(interval = 30000, callback = null) {
        if (this.isTracking) {
            console.warn('Le suivi GPS est déjà actif');
            return;
        }

        if (!navigator.geolocation) {
            console.error('Géolocalisation non supportée');
            return;
        }

        this.isTracking = true;
        this.updateInterval = interval;
        this.onPositionUpdate = callback;

        const options = {
            enableHighAccuracy: true,
            timeout: 10000,
            maximumAge: 0  // Pas de cache pour le suivi continu
        };

        // Démarrer le suivi
        this.watchId = navigator.geolocation.watchPosition(
            (position) => {
                console.log('Nouvelle position GPS:', position);
                this.handlePositionUpdate(position);
            },
            (error) => {
                console.error('Erreur de suivi GPS:', error);
                this.handlePositionError(error);
            },
            options
        );

        console.log('Suivi GPS démarré');
    }

    /**
     * Arrête le suivi de la position
     */
    stopTracking() {
        if (!this.isTracking) {
            return;
        }

        if (this.watchId) {
            navigator.geolocation.clearWatch(this.watchId);
            this.watchId = null;
        }

        this.isTracking = false;
        console.log('Suivi GPS arrêté');
    }

    /**
     * Gère la mise à jour de position
     * @param {Position} position - Position GPS
     */
    handlePositionUpdate(position) {
        const positionData = {
            latitude: position.coords.latitude,
            longitude: position.coords.longitude,
            accuracy: position.coords.accuracy,
            altitude: position.coords.altitude,
            altitudeAccuracy: position.coords.altitudeAccuracy,
            heading: position.coords.heading,
            speed: position.coords.speed,
            timestamp: position.timestamp
        };

        // Appeler le callback si défini
        if (this.onPositionUpdate) {
            this.onPositionUpdate(positionData);
        }

        // Envoyer la position au serveur
        this.sendPositionToServer(positionData);
    }

    /**
     * Gère les erreurs de géolocalisation
     * @param {PositionError} error - Erreur de géolocalisation
     */
    handlePositionError(error) {
        let errorMessage = 'Erreur de géolocalisation';

        switch (error.code) {
            case error.PERMISSION_DENIED:
                errorMessage = 'Accès à la géolocalisation refusé';
                break;
            case error.POSITION_UNAVAILABLE:
                errorMessage = 'Position GPS indisponible';
                break;
            case error.TIMEOUT:
                errorMessage = 'Timeout de la géolocalisation';
                break;
        }

        console.error(errorMessage, error);
        
        // Afficher une notification à l'utilisateur
        this.showNotification(errorMessage, 'error');
    }

    /**
     * Envoie la position au serveur
     * @param {Object} positionData - Données de position
     */
    async sendPositionToServer(positionData) {
        try {
            const response = await fetch('/AgentTerrain/UpdatePosition', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': this.getAntiForgeryToken()
                },
                body: JSON.stringify({
                    latitude: positionData.latitude,
                    longitude: positionData.longitude,
                    precision: positionData.accuracy
                })
            });

            const result = await response.json();
            
            if (result.success) {
                console.log('Position envoyée au serveur:', result.message);
            } else {
                console.error('Erreur lors de l\'envoi de la position:', result.message);
            }
        } catch (error) {
            console.error('Erreur lors de l\'envoi de la position:', error);
        }
    }

    /**
     * Récupère le token anti-forgery
     * @returns {string} Token anti-forgery
     */
    getAntiForgeryToken() {
        const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenElement ? tokenElement.value : '';
    }

    /**
     * Affiche une notification
     * @param {string} message - Message à afficher
     * @param {string} type - Type de notification (success, error, warning)
     */
    showNotification(message, type = 'info') {
        // Créer une notification Bootstrap
        const alertClass = type === 'error' ? 'alert-danger' : 
                          type === 'success' ? 'alert-success' : 
                          type === 'warning' ? 'alert-warning' : 'alert-info';

        const notification = document.createElement('div');
        notification.className = `alert ${alertClass} alert-dismissible fade show position-fixed`;
        notification.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
        notification.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;

        document.body.appendChild(notification);

        // Auto-suppression après 5 secondes
        setTimeout(() => {
            if (notification.parentNode) {
                notification.remove();
            }
        }, 5000);
    }

    /**
     * Vérifie si la géolocalisation est disponible
     * @returns {boolean} True si disponible
     */
    isGeolocationSupported() {
        return !!navigator.geolocation;
    }

    /**
     * Demande l'autorisation de géolocalisation
     * @returns {Promise<boolean>} True si autorisé
     */
    async requestPermission() {
        try {
            const position = await this.getCurrentPosition();
            return true;
        } catch (error) {
            console.error('Permission de géolocalisation refusée:', error);
            return false;
        }
    }
}

// Instance globale du service GPS
window.gpsService = new GPSLocationService();

// Fonctions utilitaires pour l'utilisation dans les vues
window.GPSUtils = {
    /**
     * Démarre le suivi GPS pour un agent
     */
    startGPSTracking: function() {
        if (!window.gpsService.isGeolocationSupported()) {
            alert('La géolocalisation n\'est pas supportée par votre navigateur');
            return;
        }

        window.gpsService.requestPermission().then(permitted => {
            if (permitted) {
                window.gpsService.startTracking(30000, (position) => {
                    console.log('Position mise à jour:', position);
                    // Ici vous pouvez ajouter du code pour mettre à jour l'interface
                });
            } else {
                alert('L\'autorisation de géolocalisation est nécessaire pour le suivi GPS');
            }
        });
    },

    /**
     * Arrête le suivi GPS
     */
    stopGPSTracking: function() {
        window.gpsService.stopTracking();
    },

    /**
     * Récupère la position actuelle une seule fois
     */
    getCurrentPosition: function() {
        return window.gpsService.getCurrentPosition();
    }
}; 