# 📖 Manuel d'Utilisation - AdRev Science Suite 2.0

Bienvenue dans le manuel officiel d'**AdRev**, votre suite complète pour la recherche scientifique d'excellence. Ce document vous guidera à travers toutes les fonctionnalités, des bases de la gestion de projet aux outils avancés de bibliographie et de collecte de données.

---

## 📑 Sommaire
1. [Installation & Activation](#1-installation--activation)
2. [Gestion de Projets & Synchronisation Cloud](#2-gestion-de-projets--synchronisation-cloud)
3. [La Bibliothèque Intelligente (Smart Library)](#3-la-bibliothèque-intelligente-smart-library)
4. [Conception de Variables & Formulaires](#4-conception-de-variables--formulaires)
5. [Collecte de Données & Dictaphone](#5-collecte-de-données--dictaphone)
6. [Rédaction & Exportation de Protocoles](#6-rédaction--exportation-de-protocoles)
7. [Sécurité & Conformité (Audit Trail & Anonymisation)](#7-sécurité--conformité-audit-trail--anonymisation)
8. [Conseils Techniques & Support](#8-conseils-techniques--support)

---

## 1. Installation & Activation

### Installation
Lancez l'installeur `AdRev_Setup.msi`. Une fois l'installation terminée, un raccourci sera créé sur votre bureau.

### Activation de la Licence
Au premier lancement, une fenêtre d'activation apparaît :
1. Copiez votre **Hardware ID (HWID)** affiché en bas de la fenêtre.
2. Envoyez cet identifiant à l'équipe support pour recevoir votre clé.
3. Collez votre clé dans le champ prévu et cliquez sur **Activer**.
4. L'application se débloquera instantanément pour toute la durée de votre abonnement.

---

## 2. Gestion de Projets & Synchronisation Cloud

### Créer un nouveau projet
Cliquez sur "Nouveau Projet" sur le tableau de bord.
*   **Titre & Auteurs** : Renseignez les informations de base de votre étude.
*   **Emplacement** : Choisissez un dossier local.
*   **Suggestions Cloud** : AdRev détecte automatiquement vos dossiers **OneDrive** ou **Google Drive**. Cliquez sur ces boutons pour sauvegarder votre projet directement sur le cloud (recommandé pour la sécurité de vos données).

---

## 3. La Bibliothèque Intelligente (Smart Library)

C'est ici que vous gérez vos sources de recherche.

### Ajout de documents
*   **Document** : Importez un PDF ou Word depuis votre ordinateur. AdRev créera une copie dans le dossier du projet.
*   **DOI / PMID / HAL / ISBN** : Cliquez sur ce bouton pour importer une source sans rien taper. Entrez simplement l'identifiant (ex: `10.1038/s41586-020-2012-7` ou `hal-01614131`). AdRev récupérera automatiquement le titre, les auteurs, le journal et l'année.

### Recherche & Aperçu
*   **Recherche Plein Texte** : La barre de recherche cherche non seulement dans les titres, mais aussi **à l'intérieur** du texte de vos PDF.
*   **Lecteur Intégré** : L'onglet "LIRE / APERÇU" vous permet de lire vos PDF directement dans AdRev sans logiciel externe.

### Gestion des Citations
Cochez la case **"Marquer comme cité dans le manuscrit"**. Le document sera alors ajouté automatiquement à la liste officielle des références de votre projet, prête pour l'exportation finale.

---

## 4. Conception de Variables & Formulaires

Avant de collecter des données, vous devez définir votre dictionnaire de variables.
1. Allez dans l'onglet **VARIABLES**.
2. Cliquez sur **Concepteur de Variables**.
3. Ajoutez vos variables :
    *   **Quantitatives** (Âge, poids, glycémie...).
    *   **Qualitatives** (Sexe, zone géographique, stade clinique...).
    *   **Mémos/Libres** (Pour les observations longues).
4. Regroupez-les par sections (ex: "Données Sociodémographiques", "Paramètres Biologiques") pour un formulaire plus clair.

---

## 5. Collecte de Données & Dictaphone

### Saisie de données
L'onglet **SAISIE DE DONNÉES** génère automatiquement un formulaire basé sur vos variables.
*   **Sauvegarde Automatique** : AdRev sauvegarde vos saisies toutes les 30 secondes pour éviter toute perte.
*   **Validation** : Les champs marqués comme "obligatoires" dans le concepteur doivent être remplis pour finaliser l'entrée.

### Dictaphone Numérique (Qualitatif)
Pour les études qualitatives ou les interviews :
1. Cliquez sur **Enregistrer** au sommet du formulaire.
2. Parlez ou enregistrez l'interview.
3. Cliquez sur **Arrêter & Sauvegarder** pour lier le fichier audio (`.wav`) directement à votre base de données.

---

## 6. Rédaction & Exportation de Protocoles

### Assistant de Protocole
Allez dans la section **PROTOCOLE**. Suivez les sections structurées (Introduction, Méthodes, Éthique) basées sur les standards internationaux.
*   **Score de Complétude** : AdRev calcule en temps réel si votre protocole contient tous les éléments requis par les comités d'éthique.

### Exportation Word
Une fois terminé, cliquez sur **Exporter en Word**. AdRev génère un document `.docx` professionnel incluant :
*   Votre mise en page.
*   Vos tableaux de variables.
*   Votre bibliographie (documents marqués comme cités).

---

## 7. Sécurité & Conformité (Audit Trail & Anonymisation)

AdRev intègre des fonctionnalités avancées pour garantir la protection des données sensibles (RGPD / HIPAA).

### Protection des Profils
*   Chaque utilisateur dispose d'un profil sécurisé. Vos informations personnelles et vos clés d'activation sont chiffrées localement sur votre machine.

### Journal d'Audit (Audit Trail)
*   Toute modification critique (création de projet, modification de données, exportations) est automatiquement enregistrée dans un journal d'audit infalsifiable.
*   Vous pouvez consulter cet historique dans l'onglet **Traçabilité (Logs)** de votre protocole pour prouver la rigueur de votre démarche scientifique.

### Exportation Sécurisée & Anonymisation
*   Lors de l'exportation vers Excel ou CSV, vous avez l'option d'**Anonymiser les données**.
*   Si cochée, l'application masquera automatiquement toutes les variables marquées comme "Sensibles" (Noms, prénoms, coordonnées...), remplaçant les valeurs par `[ANONYMISÉ]`.

---

## 8. Conseils Techniques & Support

*   **Poids des photos/PDF** : Évitez de charger des PDF de plus de 50 Mo pour conserver une fluidité optimale.
*   **Format de recherche** : Pour les DOI, assurez-vous de copier-coller la chaîne complète (commençant généralement par `10.`).

**Besoin d'aide supplémentaire ?**
*   📧 **Email Support** : support@adrev-science.com
*   💬 **WhatsApp** : +223 79 27 64 70
*   🌐 **Site Web** : [www.adrev-science.com](https://www.adrev-science.com)

---
*AdRev - Propulser la Science par la Rigueur.*
