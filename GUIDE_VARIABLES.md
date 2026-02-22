# 📝 Module "Masque de Saisie" & Variables

**Nouveauté AdRev 2.0 !** 
Vous pouvez désormais transformer votre protocole en outils de collecte concrets, comme dans Epi Info 7 ("Make View").

---

## 🚀 À quoi ça sert ?

Ce module fait le pont entre la **théorie** (votre méthodologie) et la **pratique** (votre terrain). Il permet de :
1.  **Définir le dictionnaire des variables** (Codebook).
2.  **Créer le masque de saisie** (Types de données, contraintes).
3.  **Générer automatiquement la Fiche d'Enquête** (Cahier d'Observation - CRF).

---

## 🛠️ Comment l'utiliser ?

### 1. Accéder au Concepteur
Dans l'onglet **Méthodologie**, après la section échantillonnage, cliquez sur le bouton vert :
> **[ 📝 Dictionnaire des Variables / Masque de Saisie ]**

### 2. Créer vos Variables
Une interface s'ouvre (similaire à Google Forms ou Epi Info).
- Cliquez sur **"➕ Nouvelle Variable"**.
- Remplissez les champs :
  - **Question (Prompt) :** "Quel est l'âge du patient ?"
  - **Nom (BDD) :** `AGE_ANN` (généré automatiquement)
  - **Type :** Nombre Entier, Date, Texte, Choix Multiple...
  - **Groupe :** "Données Cliniques" (pour organiser la fiche)

### 3. Types de Variables Disponibles

| Type | Usage | Exemple |
|------|-------|---------|
| **Texte Court** | Noms, Villes, Codes | *"Bamako"* |
| **Nombre Entier** | Âge, Nbre enfants | *45* |
| **Nombre Décimal** | Poids, HbA1c, Taille | *75.5* |
| **Date** | Date visite, Naissance | *12/05/2024* |
| **Oui / Non** | Symptôme présent ? | *Oui* |
| **Choix Unique** | Liste déroulante | *Homme / Femme* |
| **Choix Multiples** | Plusieurs réponses | *Toux / Fièvre / Douleur* |
| **Memo** | Texte long | *Commentaires...* |

### 4. Exporter la Fiche d'Enquête
Une fois vos variables définies :
1.  Cliquez sur le bouton orange **"🖨️ Exporter Fiche d'Enquête"** (en bas à gauche).
2.  Le logiciel génère instantanément un fichier texte (.txt) propre et formaté.
3.  Le fichier s'ouvre automatiquement.
4.  **Astuce :** Copiez-collez le contenu dans Word pour finaliser la mise en page !

---

## 📊 Exemple de Résultat

Voici ce que AdRev génère automatiquement :

```text
# FICHE D'ENQUÊTE / CAHIER D'OBSERVATION
________________________________________________________________________________
TITRE : ÉTUDE PRÉVALENCE DIABÈTE
CODE ÉTUDE : ADREV-BKO-01
DATE : ____ / ____ / ________
________________________________________________________________________________

## DONNÉES SOCIODÉMOGRAPHIQUES
--------------------------------------------------
**Sexe du participant (*)**
   ( ) Masculin
   ( ) Féminin
   _SEXE_

**Âge (années révolues) (*)**
   |__|__|__|
   _AGE_

## DONNÉES CLINIQUES
--------------------------------------------------
**Antécédents familiaux de diabète ?**
   [ ] OUI    [ ] NON
   _ATCD_DIAB_

**Glycémie à jeun (g/L)**
   |__|__| , |__|__|
   _GLYCEMIE_
```

---

## 💡 Conseils Méthodologiques

*   **Nommer vos variables** clairement (ex: `PAS_MMHG` pour Pression Artérielle Systolique).
*   Utilisez les **Groupes** pour structurer votre questionnaire (Sociodémographie, Clinique, Biologie...).
*   Cochez **"Obligatoire"** pour les variables clés (critères d'inclusion, outcome principal).
*   L'export généré sert de **Cahier d'Observation (CRF)** papier pour les enquêteurs.

---

**C'est cette structure qui servira plus tard à créer la base de données d'analyse (Excel, SPSS, Epi Info).**
