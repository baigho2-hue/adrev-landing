# Documentation Technique et Rapport de Projet : AdRev

## 1. Pourquoi AdRev ? Intérêt et Valeur Ajoutée

### Le Problème
La recherche scientifique médicale est impitoyable. Des milliers d'heures de travail sont souvent rejetées par les revues prestigieuses non pas à cause du sujet, mais à cause de **failles méthodologiques** ou de **statistiques inappropriées**. Les chercheurs cliniciens, souvent experts dans leur domaine médical, ne sont pas toujours des statisticiens aguerris.

### La Solution AdRev (Advanced Research Visibility)
AdRev agit comme un **superviseur méthodologique virtuel**. Ce n'est pas seulement un outil de saisie, c'est un garde-fou scientifique qui impose la rigueur nécessaire à la publication de rang A.

### Les 3 Piliers de Valeur
1.  **Conformité Méthodologique Automatisée** :
    -   L'application empêche les erreurs courantes (ex: choisir un test paramétrique sur une distribution non normale).
    -   Elle intègre nativement les standards internationaux (checklist CONSORT pour les essais cliniques, STROBE pour l'observationnel).
    
2.  **Gain de Temps Massif** :
    -   Tout est intégré dans un seul flux : plus besoin de jongler entre Word pour le protocole, Excel pour la saisie et SPSS/R pour l'analyse.
    -   La génération de manuscrit exporte un document `.docx` déjà formaté selon les normes académiques (IMRAD).

3.  **Démocratisation de la Science de Pointe** :
    -   Rend accessible des méthodes complexes (Échantillonnage en grappe avec effet de plan, analyses multivariées) via une interface simple qui masque la complexité mathématique.

---

## 2. Vue d'ensemble du Projet

**AdRev** est une application Desktop Windows qui guide l'utilisateur de la conception de son protocole jusqu'à la publication.

Le projet a évolué d'une application Python initiale vers une solution robuste en **C# .NET 8** avec une interface **WPF** moderne.

### Objectif Principal
Transformer le chaos des données brutes en publications scientifiques valides, en minimisant les erreurs méthodologiques et en automatisant les calculs statistiques.

---

## 3. Architecture Technique

### Stack Technologique
-   **Plateforme** : Windows (WPF).
-   **Framework** : .NET 8.
-   **Langage** : C#.
-   **Design UI** : Stylisation personnalisée (Glassmorphism, thèmes Premium).
-   **Analyse de Données** : `MathNet.Numerics` pour les calculs statistiques.
-   **Licence & Sécurité** : Système propriétaire basé sur le HWID, validé via un backend Cloudflare Workers.

### Structure de la Solution (`AdRev.sln`)

1.  **AdRev.Desktop** (Projet principal) :
    -   Interface utilisateur (Vues XAML, ViewModels).
    -   Points d'entrée : `App.xaml`, `MainWindow.xaml`.
    -   Fenêtres clés : `ProjectWindow` (éditeur principal), `ProtocolWindow`.

2.  **AdRev.Domain** (Cœur métier) :
    -   Modèles de données (`ResearchProtocol`, `Variable`, `DataEntry`).
    -   Énumérations (`StudyType`, `SamplingType`).

3.  **AdRev.Core** (Services & Logique) :
    -   Services de persistance (JSON/SQLite).
    -   Moteurs de calcul statistique et exportateurs.

---

## 4. Flux Fonctionnel (User Journey)

L'application suit un pipeline de recherche linéaire :

### A. Dashboard (Accueil)
-   Gestion des projets et vérification de licence.

### B. Éditeur de Projet (`ProjectWindow`)
1.  **Protocole** : Définition du contexte, objectifs, et méthodologie (Calcul de taille d'échantillon, Deff, Perdus de vue).
2.  **Variables** : Création du dictionnaire de données et visualisation du formulaire.
3.  **Saisie** : Saisie manuelle ou import (CSV/Excel).
4.  **Analyse** : Statistiques descriptives et tests d'hypothèse (Chi2, Student, etc.) automatisés.
5.  **Qualité & Export** : Validation qualité (Checklists) et génération du rapport Word.

---

## 5. Historique du Projet

-   **Phase 1 (Jan 2026)** : Migration complète de Python vers C#/.NET pour performance et maintenabilité.
-   **Phase 2** : Ajout des calculateurs d'échantillon, designer de variables et import de données.
-   **Phase 3** : Support des études multicentriques, échantillonnage en grappe et checklists qualité dynamiques.
-   **Phase 4 (Actuel)** : Intégration Cloudflare pour les licences et raffinement de l'UI.

---

## 6. Guide pour le Développeur (Setup)

1.  **Prérequis** : Visual Studio 2022, .NET 8 SDK.
2.  **Build** : Ouvrir `AdRev.sln`, restaurer les paquets, compiler `AdRev.Desktop`.
3.  **Notes** : Les données projets sont dans `Documents/AdRev/Projects`.
