# Pack de Soumission Google Play Store - AdRev Collect 🚀

Ce document contient tous les éléments nécessaires pour publier l'application sur la Google Play Console.

---

## 1. Informations de la Fiche Logicielle

### Titre de l'application (Max 30 car.)
**AdRev Collect**

### Description Courte (Max 80 car.)
Collecte de données de recherche médicale simplifiée et synchronisation sécurisée.

### Description Longue (Max 4000 car.)
Transformez votre smartphone en un puissant outil de recherche clinique avec **AdRev Collect**. 

Conçue spécifiquement pour les médecins, chercheurs et étudiants, AdRev Collect est l'extension mobile de la suite AdRev Science Suite. Elle permet une collecte de données rigoureuse sur le terrain, même sans connexion internet.

**Fonctionnalités Clés :**
*   **Formulaires Intelligents :** Saisie de données structurées selon vos protocoles de recherche (Quantitatif, Qualitatif, Mixte).
*   **Sécurité Renforcée :** Protection par code PIN et chiffrement des données locales (Conforme HIPAA/RGPD).
*   **Mode Hors-Ligne :** Collectez vos données n'importe où, elles seront sauvegardées localement en toute sécurité.
*   **Synchronisation par QR Code :** Scannez un code sur votre version Desktop pour jumeler l'appareil et transférer vos données sans fil.
*   **Traçabilité & Éthique :** Intégration native avec le journal d'audit d'AdRev Desktop pour une transparence totale des modifications.
*   **Anonymisation des Exports :** Exportez vos données avec un masquage automatique des variables sensibles pour un partage éthique.
*   **Bibliothèque de Protocoles :** Emportez tous vos formulaires de recherche partout avec vous.

9. Proposez une version 1.1 (ApplicationVersion 2) qui inclut les dernières optimisations de terrain.

---

## 2. Éléments Graphiques Requis

*   **Icône de l'application :** 512 x 512 pixels, format PNG ou WEBP (Max 1 Mo).
*   **Graphisme de présentation :** 1024 x 500 pixels, format PNG ou WEBP.
*   **Captures d'écran (Phone/Desktop Suite) :**
    1.  **Dashboard de Recherche** : Vue d'ensemble avec KPIs et projets récents.
    2.  **Journal d'Audit & Traçabilité** : Écran prouvant la conformité (Étape 15 du protocole).
    3.  **Exportation Sécurisée & Anonymisation** : Fenêtre d'export avec l'option de masquage des données sensibles.
    4.  **Synchronisation QR Code** : Écran de jumelage smartphone pour la collecte mobile.
*   **Captures d'écran (Tablette) :** Google Play exige désormais des captures pour tablettes 7" et 10".

---

## 3. Configuration Technique (Build AAB)

Google Play n'accepte plus les `.apk` mais exige le format **Android App Bundle (.aab)**.

### Commande de génération :
```powershell
dotnet publish AdRev.Mobile/AdRev.Mobile.csproj -f net8.0-android -c Release
```

### Emplacement du fichier :
`AdRev.Mobile\bin\Release\net8.0-android\publish\com.adrev.mobile-Signed.aab`

---

## 4. Signature de l'App (Keystore)

Avant la soumission, l'application doit être signée numériquement.
Si vous n'avez pas encore de keystore, voici comment en créer un :

1.  Ouvrez un terminal.
2.  Exécutez :
    ```powershell
    keytool -genkey -v -keystore adrev-release.keystore -alias adrev_alias -keyalg RSA -keysize 2048 -validity 10000
    ```
3.  Ajoutez ces propriétés dans votre fichier `AdRev.Mobile.csproj` (ou utilisez la CI/CD) :
    ```xml
    <PropertyGroup Condition="'$(Configuration)' == 'Release'">
        <AndroidKeyStore>True</AndroidKeyStore>
        <AndroidSigningKeyStore>adrev-release.keystore</AndroidSigningKeyStore>
        <AndroidSigningStorePass>VOTRE_MOT_DE_PASSE</AndroidSigningStorePass>
        <AndroidSigningKeyAlias>adrev_alias</AndroidSigningKeyAlias>
        <AndroidSigningKeyPass>VOTRE_MOT_DE_PASSE</AndroidSigningKeyPass>
    </PropertyGroup>
    ```

---

## 5. Politique de Confidentialité (Obligatoire)

Google exige une URL pointant vers votre politique de confidentialité.
**Modèle rapide :**
"AdRev Collect ne collecte aucune donnée personnelle à l'insu de l'utilisateur. Toutes les données médicales saisies sont stockées localement sur l'appareil et ne sont transmises qu'au logiciel AdRev Desktop désigné par l'utilisateur via un jumelage direct ou un export manuel. Aucune donnée n'est envoyée vers des serveurs tiers sans consentement explicite."

---

## 6. Coordonnées de Support
*   **Email :** baigho2@gmail.com
*   **Site Web :** [https://adrev.science](https://adrev.science)
*   **Lien Politique de Confidentialité :** [https://adrev.science/privacy](https://adrev.science/privacy)
