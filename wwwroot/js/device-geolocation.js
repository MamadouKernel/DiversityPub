/**
 * Service de géolocalisation directe de l'appareil
 * Récupère la position exacte via le navigateur
 */
class DeviceGeolocationService {
    constructor() {
        this.isTracking = false;
        this.watchId = null;
        this.currentPosition = null;
        this.onPositionUpdate = null;
        this.onError = null;
    }

    /**
     * Vérifie si la géolocalisation est disponible
     */
    isSupported() {
        return 'geolocation' in navigator;
    }

    /**
     * Demande la permission et récupère la position actuelle
     */
    async getCurrentPosition() {
        return new Promise((resolve, reject) => {
            if (!this.isSupported()) {
                reject(new Error('Géolocalisation non supportée'));
                return;
            }

            const options = {
                enableHighAccuracy: true, // Utilise le GPS si disponible
                timeout: 10000, // 10 secondes de timeout
                maximumAge: 0 // Pas de cache, position fraîche
            };

            navigator.geolocation.getCurrentPosition(
                (position) => {
                    this.currentPosition = {
                        latitude: position.coords.latitude,
                        longitude: position.coords.longitude,
                        accuracy: position.coords.accuracy,
                        timestamp: new Date(position.timestamp)
                    };
                    resolve(this.currentPosition);
                },
                (error) => {
                    reject(this.handleGeolocationError(error));
                },
                options
            );
        });
    }

    /**
     * Démarre le suivi en temps réel
     */
    startTracking(onPositionUpdate, onError) {
        if (!this.isSupported()) {
            if (onError) onError(new Error('Géolocalisation non supportée'));
            return false;
        }

        this.onPositionUpdate = onPositionUpdate;
        this.onError = onError;

        const options = {
            enableHighAccuracy: true,
            timeout: 10000,
            maximumAge: 0
        };

        this.watchId = navigator.geolocation.watchPosition(
            (position) => {
                const newPosition = {
                    latitude: position.coords.latitude,
                    longitude: position.coords.longitude,
                    accuracy: position.coords.accuracy,
                    timestamp: new Date(position.timestamp)
                };

                this.currentPosition = newPosition;
                
                if (this.onPositionUpdate) {
                    this.onPositionUpdate(newPosition);
                }
            },
            (error) => {
                if (this.onError) {
                    this.onError(this.handleGeolocationError(error));
                }
            },
            options
        );

        this.isTracking = true;
        console.log('Suivi de géolocalisation démarré');
        return true;
    }

    /**
     * Arrête le suivi en temps réel
     */
    stopTracking() {
        if (this.watchId) {
            navigator.geolocation.clearWatch(this.watchId);
            this.watchId = null;
            this.isTracking = false;
            console.log('Suivi de géolocalisation arrêté');
        }
    }

    /**
     * Envoie la position au serveur
     */
    async sendPositionToServer(position) {
        try {
            const response = await fetch('/AgentTerrain/UpdatePosition', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    latitude: position.latitude,
                    longitude: position.longitude,
                    precision: position.accuracy
                })
            });

            const result = await response.json();
            
            if (result.success) {
                console.log('Position envoyée avec succès:', position);
                return true;
            } else {
                console.error('Erreur lors de l\'envoi de la position:', result.message);
                return false;
            }
        } catch (error) {
            console.error('Erreur réseau lors de l\'envoi de la position:', error);
            return false;
        }
    }

    /**
     * Gère les erreurs de géolocalisation
     */
    handleGeolocationError(error) {
        let message = '';
        
        switch (error.code) {
            case error.PERMISSION_DENIED:
                message = 'Permission de géolocalisation refusée. Veuillez autoriser l\'accès à votre position.';
                break;
            case error.POSITION_UNAVAILABLE:
                message = 'Position indisponible. Vérifiez votre connexion GPS/WiFi.';
                break;
            case error.TIMEOUT:
                message = 'Délai d\'attente dépassé. Vérifiez votre connexion.';
                break;
            default:
                message = 'Erreur de géolocalisation inconnue.';
                break;
        }

        return new Error(message);
    }

    /**
     * Obtient la position actuelle et l'envoie au serveur
     */
    async updateCurrentPosition() {
        try {
            const position = await this.getCurrentPosition();
            const success = await this.sendPositionToServer(position);
            
            if (success) {
                console.log('Position mise à jour:', position);
                return position;
            } else {
                throw new Error('Échec de l\'envoi de la position');
            }
        } catch (error) {
            console.error('Erreur lors de la mise à jour de la position:', error);
            throw error;
        }
    }

    /**
     * Démarre le suivi automatique avec envoi au serveur
     */
    startAutoTracking(intervalMs = 30000) { // 30 secondes par défaut
        this.startTracking(
            async (position) => {
                // Envoyer automatiquement la position au serveur
                await this.sendPositionToServer(position);
            },
            (error) => {
                console.error('Erreur de géolocalisation:', error.message);
            }
        );

        // Envoyer la position immédiatement
        this.updateCurrentPosition();
    }

    /**
     * Obtient les informations de position formatées
     */
    getPositionInfo(position) {
        return {
            coordinates: `${position.latitude.toFixed(6)}, ${position.longitude.toFixed(6)}`,
            accuracy: `${Math.round(position.accuracy)}m`,
            timestamp: position.timestamp.toLocaleString('fr-FR'),
            isAccurate: position.accuracy <= 50 // Considéré précis si < 50m
        };
    }
}

// Export pour utilisation globale
window.DeviceGeolocationService = DeviceGeolocationService; 