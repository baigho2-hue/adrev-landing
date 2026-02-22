# Changelog - AdRev v1.0.1

## 📅 Date : Février 2026

## 🎯 Objectif
Amélioration de la stabilité, correction de bugs critiques de navigation et ajout d'une documentation complète pour la production vidéo et l'utilisation logicielle.

## ✨ Nouvelles Améliorations & Corrections

### 1. Manuel d'Utilisation & PDR Vidéo
- **PDR_Videos_AdRev.md** : Création d'un plan de réalisation détaillé pour la génération de vidéos de formation (5 scénarios clés).
- **Manuel_Utilisateur_AdRev.md** : Rédaction d'un manuel utilisateur complet couvrant les 10 étapes du projet et les fonctionnalités avancées.

### 2. Correction de la Navigation (Profil)
- **WelcomeWindow.xaml** : Ajout d'un bouton de fermeture (X) pour permettre un retour sécurisé vers l'application principale depuis la section Aide.

### 3. Zéro Avertissement de Build
- Correction massive des avertissements de nullabilité (`CS86xx`) dans les services de données et l'analyse statistique.
- Suppression des avertissements de compatibilité `NU1701` (LiveCharts) pour un build plus propre.

### 4. Robustesse des Données
- Amélioration de l'importation Excel/CSV avec une gestion robuste des valeurs nulles.
- Sécurisation du moteur de calcul des graphiques (Histogrammes/ScatterPlots) contre les données malformées.

## 📦 Mise à Jour du Setup
- Version bump vers **1.0.1.0**.
- Script de build `build-setup.ps1` mis à jour pour générer `AdRev1.0.1.msi`.
