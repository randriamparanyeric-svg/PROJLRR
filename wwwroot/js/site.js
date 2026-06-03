


// --- FONCTION DE NAVIGATION SÉCURISÉE ---
async function naviguerVersPageSecurisee(url) {
    // On appelle votre barrière de sécurité existante (Code PIN / 1h glissante)
    if (await validerAccesGeneral()) {
        // Si validé, on redirige vers l'URL
        window.location.href = url;
    }
}

// --- LOGIQUE DE SÉCURITÉ (PIN 2026) ---
async function validerAccesGeneral() {
    // 1. Vérification silencieuse du temps (1 heure)
    if (estAutorise()) return true;

    // 2. L'affichage MODERNE avec SweetAlert2
    const { value: pin } = await Swal.fire({
        title: '🔑 SESSION VERROUILLÉE',
        html: '<b style="color: #166534;">Veuillez saisir le code d\'autorisation</b><br><small>(Session glissante de 1h)</small>',
        input: 'password', // Cache les caractères par des points
        inputAttributes: {
            autocapitalize: 'off',
            autocorrect: 'off',
            autocomplete: 'new-password', // <--- AJOUTÉ ICI pour bloquer la prédiction
            spellcheck: 'false'           // <--- AJOUTÉ ICI pour bloquer le correcteur
        },
        showCancelButton: true,
        confirmButtonText: 'Valider',
        cancelButtonText: 'Annuler',
        confirmButtonColor: '#2c3e50', // Couleur élégante
        background: '#f0fdf4', // Vert très clair pastel
        inputValidator: (value) => {
            if (!value) return 'Le code est requis !';
        }
    });

    // 3. Logique de validation
    if (pin === "2026") {
        const maintenant = new Date().getTime();
        sessionStorage.setItem("osief_access_unlocked", "true");
        sessionStorage.setItem("osief_timestamp", maintenant);
        
        // Petit effet de succès moderne
        await Swal.fire({
            icon: 'success',
            title: 'Accès autorisé',
            background: '#f0fdf4',
            timer: 1000,
            showConfirmButton: false
        });
        
        return true;
    } else if (pin) {
        Swal.fire({
            icon: 'error',
            title: 'Erreur',
            text: 'Code incorrect',
            background: '#fff5f5' // Rouge clair pour l'erreur
        });
        return false;
    }
    return false;
}

// --- FONCTION DE VÉRIFICATION DE TEMPS ---
function estAutorise() {
    const uneHeure = 60 * 60 * 1000; 
    const sessionTime = sessionStorage.getItem("osief_timestamp");
    const maintenant = new Date().getTime();
    
    if (!sessionTime || (maintenant - sessionTime > uneHeure)) {
        sessionStorage.removeItem("osief_access_unlocked");
        sessionStorage.removeItem("osief_timestamp");
        return false;
    }
    
    // Si valide, on met à jour le timestamp pour prolonger la session
    sessionStorage.setItem("osief_timestamp", maintenant); 
    return true;
}