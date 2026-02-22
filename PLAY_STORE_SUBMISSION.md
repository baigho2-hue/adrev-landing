# Pack de Soumission Google Play Store - AdRev Collect 🚀

Ce document contient tous les éléments nécessaires pour publier l'application sur la Google Play Console.

---

## 1. Informations de la Fiche Logicielle

### Titre de l'application (Max 30 car.)
**AdRev Collect** (v1.5)

### Description Courte (Max 80 car.)
Collecte de données de recherche médicale simplifiée et synchronisation sécurisée.

### Description Longue (Max 4000 car.)
**AdRev Collect : L'excellence de la recherche clinique, partout, tout le temps.**

Transformez votre smartphone en un terminal de saisie de données de haute précision avec **AdRev Collect**, l'extension de terrain indispensable de la suite **AdRev Science Suite**. 

Que vous soyez médecin, doctorant, chercheur indépendant ou membre d'une CRO, AdRev Collect a été conçu pour répondre aux exigences de rigueur scientifique les plus strictes (standards STROBE/CONSORT) tout en offrant une ergonomie moderne adaptée aux contraintes du terrain.

**POURQUOI CHOISIR ADREV COLLECT ?**

🔹 **AUTONOMIE TOTALE (OFFLINE)**
Plus besoin de connexion Wi-Fi ou 4G dans les services hospitaliers ou lors de missions de terrain isolées. Collectez vos données hors-ligne en toute sérénité. L'application synchronise vos données dès que vous retrouvez votre poste de travail.

🔹 **SÉCURITÉ & CONFIDENTIALITÉ MÉDICALE**
Parce que la donnée de santé est sensible, nous avons fait le choix de la sécurité maximale :
*   **Chiffrement Local :** Toutes les données stockées sur le téléphone sont chiffrées (AES-256).
*   **Zéro Cloud Subi :** Vos données ne transitent par aucun serveur tiers. Elles restent sur votre appareil jusqu'au jumelage direct avec votre ordinateur professionnel.
*   **Conformité RGPD :** Anonymisation native des variables sensibles dès la source.

🔹 **SYNCHRONISATION MAGIQUE PAR QR CODE**
Oubliez les câbles et les configurations complexes. Un simple scan du QR Code généré par votre logiciel AdRev Desktop (Windows/Mac) suffit pour :
1. Télécharger vos protocoles de recherche sur votre mobile.
2. Synchroniser les données collectées vers votre base d'analyse finale.

🔹 **RIGUEUR MÉTHODOLOGIQUE NATIVE**
AdRev Collect respecte la structure de vos variables définies en amont. Les erreurs de saisie sont minimisées grâce à un typage intelligent des champs (Quantitatif, Qualitatif, Binaire, Temporel) et des contrôles de cohérence en temps réel.

**MODÈLE FREEMIUM & FLEXIBILITÉ**

AdRev Collect s'adapte à vos besoins :
*   **Version LITE (Gratuite) :** Parfaite pour tester l'application ou pour les petits projets de recherche (jusqu'à 1 projet actif et 20 enregistrements).
*   **Version MOBILE PRO :** Débloquez la puissance illimitée (projets multiples, stockage de données illimité, exportation avancée au format CSV/Excel pour analyse immédiate) via un achat unique sur le Play Store.
*   **Version UNIVERSAL :** Accès inclus pour les détenteurs d'une licence AdRev Science Suite Elite sur Desktop.

**À PROPOS D'ADREV SCIENCE**
AdRev est une suite logicielle dédiée à l'excellence académique. Notre mission est d'accompagner les scientifiques de la conception du protocole à la rédaction du manuscrit IMRAD, en garantissant une traçabilité et une rigueur statistique irréprochables.

*Note : Cette application nécessite le jumelage avec une instance AdRev pour l'exploitation complète des données.*

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
**URL Live :** [https://adrev-landing.onrender.com/privacy.html](https://adrev-landing.onrender.com/privacy.html) (Remplacez par votre domaine final si besoin).

**Modèle rapide :**
"AdRev Collect ne collecte aucune donnée personnelle à l'insu de l'utilisateur. Toutes les données médicales saisies sont stockées localement sur l'appareil et ne sont transmises qu'au logiciel AdRev Desktop désigné par l'utilisateur via un jumelage direct ou un export manuel. Aucune donnée n'est envoyée vers des serveurs tiers sans consentement explicite."

---

---

## 6. Coordonnées de Support
*   **Email :** baigho2@gmail.com
*   **Site Web :** [https://adrev-landing.onrender.com](https://adrev-landing.onrender.com)
*   **Lien Politique de Confidentialité :** [https://adrev-landing.onrender.com/privacy.html](https://adrev-landing.onrender.com/privacy.html)

---

## 7. Monétisation & Achats In-App (Modèle Freemium)

L'application suit un modèle "Honnête vis-à-vis de Google" via la **Google Play Billing Library**.

### Modèle Commercial :
*   **Gratuit :** Accès à 1 projet de recherche, saisie limitée à 20 formulaires.
*   **Achat In-App (Solo Pro) :** Déblocage illimité des projets et exports CSV/Excel.

### Détails Techniques pour la Console :
*   **Type de produit :** Produit géré (Achat unique ou Abonnement).
*   **Identifiant du produit (SKU) :** `adrev_collect_pro_unlock`
*   **Nom du produit :** AdRev Collect Pro - Licence Illimitée
*   **Prix suggéré :** (À définir dans la console, ex: 14.99€)

### Déclaration de Facturation :
*   **Permission requise :** `com.android.vending.BILLING`
*   **Système utilisé :** Intégration native Google Play Billing v6+.
