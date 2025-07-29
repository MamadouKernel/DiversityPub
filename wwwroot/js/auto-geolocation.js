/**
 * Service de géolocalisation automatique silencieux
 * Fonctionne en arrière-plan sans notifications visibles
 */
class AutoGeolocationService {
    constructor() {
        this.isInitialized = false;
        this.permissionGranted = false;
        this.currentPosition = null;
        this.watchId = null;
        this.onPermissionGranted = null;
        this.onPermissionDenied = null;
        this.onPositionUpdate = null;
    }

    /**
     * Initialise automatiquement la géolocalisation (silencieux)
     * Cette méthode doit être appelée dès la connexion de l'agent
     */
    async initializeOnLogin() {
        console.log('🔍 Initialisation silencieuse de la géolocalisation...');
        
        if (!this.isSupported()) {
            console.error('❌ Géolocalisation non supportée par ce navigateur');
            return false;
        }

        try {
            // Vérifier si la permission est déjà accordée
            const permission = await this.checkPermission();
            
            if (permission === 'granted') {
                console.log('✅ Permission déjà accordée (silencieux)');
                this.permissionGranted = true;
                this.startAutomaticTracking();
                return true;
            } else if (permission === 'prompt') {
                console.log('🤔 Demande de permission silencieuse...');
                return await this.requestPermissionAutomatically();
            } else {
                console.log('❌ Permission refusée (silencieux)');
                return false;
            }
        } catch (error) {
            console.error('❌ Erreur lors de l\'initialisation silencieuse:', error);
            return false;
        }
    }

    /**
     * Vérifie si la géolocalisation est supportée
     */
    isSupported() {
        return 'geolocation' in navigator;
    }

    /**
     * Vérifie le statut de la permission
     */
    async checkPermission() {
        if ('permissions' in navigator) {
            try {
                const permission = await navigator.permissions.query({ name: 'geolocation' });
                return permission.state;
            } catch (error) {
                console.warn('⚠️ Impossible de vérifier la permission:', error);
                return 'prompt';
            }
        }
        return 'prompt';
    }

    /**
     * Demande automatiquement la permission (silencieux)
     */
    async requestPermissionAutomatically() {
        return new Promise((resolve) => {
            // Pas de notification visible - demande directe
            navigator.geolocation.getCurrentPosition(
                (position) => {
                    console.log('✅ Permission accordée automatiquement (silencieux)');
                    this.permissionGranted = true;
                    this.currentPosition = this.formatPosition(position);
                    this.startAutomaticTracking();
                    
                    if (this.onPermissionGranted) {
                        this.onPermissionGranted(this.currentPosition);
                    }
                    
                    resolve(true);
                },
                (error) => {
                    console.log('❌ Permission refusée par l\'utilisateur (silencieux)');
                    this.permissionGranted = false;
                    
                    if (this.onPermissionDenied) {
                        this.onPermissionDenied(error);
                    }
                    
                    resolve(false);
                },
                {
                    enableHighAccuracy: true,
                    timeout: 15000, // 15 secondes de timeout
                    maximumAge: 0
                }
            );
        });
    }

    /**
     * Démarre le suivi automatique (silencieux)
     */
    startAutomaticTracking() {
        if (!this.permissionGranted) {
            console.warn('⚠️ Permission non accordée, impossible de démarrer le suivi');
            return;
        }

        console.log('🔄 Démarrage du suivi automatique silencieux...');
        
        const options = {
            enableHighAccuracy: true,
            timeout: 10000,
            maximumAge: 0
        };

        this.watchId = navigator.geolocation.watchPosition(
            (position) => {
                const newPosition = this.formatPosition(position);
                this.currentPosition = newPosition;
                
                console.log('📍 Position mise à jour automatiquement (silencieux)');
                
                // Envoyer automatiquement au serveur
                this.sendPositionToServer(newPosition);
                
                if (this.onPositionUpdate) {
                    this.onPositionUpdate(newPosition);
                }
            },
            (error) => {
                console.error('❌ Erreur de géolocalisation:', error);
                this.handleGeolocationError(error);
            },
            options
        );

        // Envoyer la position initiale
        if (this.currentPosition) {
            this.sendPositionToServer(this.currentPosition);
        }
    }

    /**
     * Arrête le suivi automatique
     */
    stopAutomaticTracking() {
        if (this.watchId) {
            navigator.geolocation.clearWatch(this.watchId);
            this.watchId = null;
            console.log('⏹️ Suivi automatique arrêté');
        }
    }

    /**
     * Formate la position pour l'utilisation
     */
    formatPosition(position) {
        return {
            latitude: position.coords.latitude,
            longitude: position.coords.longitude,
            accuracy: position.coords.accuracy,
            timestamp: new Date(position.timestamp)
        };
    }

    /**
     * Envoie la position au serveur (silencieux)
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
                console.log('✅ Position envoyée au serveur (silencieux)');
                return true;
            } else {
                console.error('❌ Erreur lors de l\'envoi:', result.message);
                return false;
            }
        } catch (error) {
            console.error('❌ Erreur réseau lors de l\'envoi:', error);
            return false;
        }
    }

    /**
     * Gère les erreurs de géolocalisation (silencieux)
     */
    handleGeolocationError(error) {
        let message = '';
        
        switch (error.code) {
            case error.PERMISSION_DENIED:
                message = 'Permission de géolocalisation refusée';
                break;
            case error.POSITION_UNAVAILABLE:
                message = 'Position indisponible';
                break;
            case error.TIMEOUT:
                message = 'Délai d\'attente dépassé';
                break;
            default:
                message = 'Erreur de géolocalisation inconnue';
                break;
        }

        console.error('❌ Erreur de géolocalisation:', message);
    }

    /**
     * Obtient la position actuelle
     */
    getCurrentPosition() {
        return this.currentPosition;
    }

    /**
     * Vérifie si le suivi est actif
     */
    isTracking() {
        return this.watchId !== null;
    }

    /**
     * Définit les callbacks
     */
    setCallbacks(onPermissionGranted, onPermissionDenied, onPositionUpdate) {
        this.onPermissionGranted = onPermissionGranted;
        this.onPermissionDenied = onPermissionDenied;
        this.onPositionUpdate = onPositionUpdate;
    }
}

// Export pour utilisation globale
window.AutoGeolocationService = AutoGeolocationService; 