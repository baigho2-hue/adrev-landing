# Guide : Méthodologie Avancée - Études Multicentriques et Échantillonnage en Grappe

## Vue d'ensemble

AdRev a été enrichi pour prendre en charge des méthodologies de recherche plus complexes, notamment :

- **Études Multicentriques** : Études menées dans plusieurs centres/sites
- **Échantillonnage en Grappe (Cluster Sampling)** : Méthode d'échantillonnage où les sujets sont groupés
- **Échantillonnage Stratifié** : Division de la population en sous-groupes homogènes
- **Effet de Plan (Design Effect)** : Correction de la taille d'échantillon pour les designs complexes
- **Taux de Perdus de Vue** : Prise en compte des abandons/perdus de vue

## 1. Études Multicentriques

### Quand utiliser ?
- Lorsque l'étude se déroule dans plusieurs hôpitaux, cliniques ou centres de recherche
- Pour augmenter la taille de l'échantillon
- Pour améliorer la généralisabilité des résultats

### Configuration
1. Cocher la case **"Étude Multicentrique"**
2. Lister les centres participants (un par ligne) dans le champ qui apparaît

### Exemple
```
Centre Hospitalier Universitaire de Bamako
Hôpital Gabriel Touré
Centre de Santé de Référence de Koulikoro
```

## 2. Types d'Échantillonnage

### Échantillonnage Probabiliste

#### a) Aléatoire Simple
- Chaque sujet a la même probabilité d'être sélectionné
- Le plus simple, mais peut être difficile si la population est dispersée

#### b) Systématique  
- Sélection d'un sujet tous les k individus (ex: 1 personne sur 10)
- Facile à mettre en œuvre

#### c) Stratifié
- Division de la population en strates (groupes homogènes)
- Échantillonnage aléatoire dans chaque strate
- **Exemple de critères de stratification** : âge, sexe, niveau socio-économique, zone géographique

**Configuration :**
1. Sélectionner "Stratifié" dans le type d'échantillonnage
2. Cocher "Échantillonnage stratifié"
3. Préciser les critères (ex: "Âge (<40 ans, ≥40 ans), Sexe (Homme, Femme)")

#### d) En Grappe (Cluster Sampling)
- Sélection de groupes (grappes) plutôt que d'individus
- Utile quand la population est géographiquement dispersée
- **Exemples de grappes** : écoles, villages, quartiers, familles

**Configuration :**
1. Sélectionner "En Grappe (Cluster)" dans le type d'échantillonnage
2. Cocher "Échantillonnage en grappe"
3. Indiquer :
   - **Taille moyenne des grappes** : nombre moyen de sujets par grappe (ex: 30 élèves par école)
   - **Effet de plan (Design Effect)** : généralement entre 1.5 et 2.0 (voir section suivante)

#### e) À Plusieurs Degrés (Multi-stage)
- Échantillonnage en plusieurs étapes
- **Exemple** : 
  - 1er degré : sélection de régions
  - 2ème degré : sélection de districts dans chaque région
  - 3ème degré : sélection de ménages dans chaque district

#### f) Stratifié en Grappes
- Combinaison de stratification et d'échantillonnage en grappe
- **Exemple** : Stratifier par zone (urbain/rural), puis échantillonner des villages (grappes) dans chaque strate

### Échantillonnage Non Probabiliste

#### a) De Convenance
- Sélection des sujets facilement accessibles
- Rapide et peu coûteux, mais biais de sélection important

#### b) Raisonné (Purposive)
- Sélection intentionnelle de sujets ayant des caractéristiques spécifiques
- Utilisé en recherche qualitative

#### c) Boule de Neige
- Les participants recrutent d'autres participants
- Utile pour les populations difficiles d'accès

#### d) Par Quotas
- Sélection pour respecter des proportions prédéfinies
- Similaire au stratifié, mais non probabiliste

## 3. Effet de Plan (Design Effect - Deff)

### Définition
L'effet de plan mesure l'augmentation de la variance due à un design d'échantillonnage complexe (grappe, stratifié) par rapport à un échantillonnage aléatoire simple.

### Formule  
```
Taille ajustée = Taille calculée × Design Effect
```

### Valeurs typiques
- **Deff = 1.0** : Échantillonnage aléatoire simple (pas d'ajustement)
- **Deff = 1.5 à 2.0** : Échantillonnage en grappe typique
- **Deff > 2.0** : Grappes très homogènes (forte corrélation intra-classe)

### Exemple
Si votre calcul de base donne **N = 384 sujets** et que vous utilisez un échantillonnage en grappe avec Deff = 1.8 :
```
N ajusté = 384 × 1.8 = 691 sujets
```

### Comment estimer le Deff ?
1. **Littérature** : Chercher des études similaires
2. **Étude pilote** : Calculer la corrélation intra-classe (ICC)
3. **Formule** : Deff = 1 + (m - 1) × ICC
   - m = taille moyenne de la grappe
   - ICC = coefficient de corrélation intra-classe (généralement 0.01 à 0.05)

## 4. Taux de Perdus de Vue

### Définition
Proportion attendue de participants qui ne compléteront pas l'étude (abandon, décès, perte de contact).

### Formule d'ajustement
```
N ajusté = N calculé / (1 - taux de perdus de vue)
```

### Valeurs typiques
- **Études transversales** : 5-10%
- **Études de cohorte courte (< 1 an)** : 10-15%
- **Études de cohorte longue (> 1 an)** : 15-25%
- **Essais cliniques** : 10-20%

### Exemple
Si N calculé = 400 et taux de perdus de vue = 15% :
```
N ajusté = 400 / (1 - 0.15) = 400 / 0.85 = 471 sujets
```

## 5. Calcul Complet avec Ajustements

### Exemple pratique : Étude multicentrique en grappe

**Contexte** :
- Étude sur la prévalence du diabète en milieu rural
- 3 centres (régions sanitaires)
- Échantillonnage en grappe (villages = grappes)

**Étapes** :

1. **Calcul de base (Cochran)** :
   - Prévalence attendue : 10%
   - Précision : 3%
   - IC 95% (Z=1.96)
   - **N₀ = 384 sujets**

2. **Ajustement pour Design Effect** :
   - Taille moyenne des grappes : 25 personnes/village
   - ICC estimé : 0.02 (littérature)
   - Deff = 1 + (25 - 1) × 0.02 = 1.48 ≈ **1.5**
   - **N₁ = 384 × 1.5 = 576 sujets**

3. **Ajustement pour perdus de vue** :
   - Taux attendu : 10%
   - **N final = 576 / 0.90 = 640 sujets**

4. **Répartition par centre** :
   - Si équitable : 640 / 3 ≈ **213 sujets par centre**

5. **Nombre de grappes** :
   - **640 / 25 = 26 villages** au total
   - Soit environ 9 villages par centre

### Texte généré pour le protocole
```
La taille d'échantillon a été calculée selon la formule de Cochran pour une prévalence attendue de 10%,
avec une précision de 3% et un niveau de confiance de 95%, donnant un échantillon de base de 384 sujets.

En raison de l'échantillonnage en grappe (villages), un effet de plan (Design Effect) de 1.5 a été appliqué,
portant l'échantillon à 576 sujets. 

Anticipant un taux de perdus de vue de 10%, l'échantillon final requis est de 640 sujets, répartis
équitablement dans les 3 centres participants (environ 213 sujets par centre), correspondant à 26 villages
avec une moyenne de 25 personnes par village.
```

## 6. Conseils Pratiques

### ✅ Bonnes Pratiques
- Toujours justifier le choix de la méthode d'échantillonnage
- Documenter les sources pour le Design Effect
- Être réaliste sur le taux de perdus de vue
- Pour les études multicentriques, vérifier la faisabilité de recrutement dans chaque centre

### ⚠️ À Éviter
- Sous-estimer l'effet de plan (risque de manque de puissance)
- Ignorer les perdus de vue (échantillon final insuffisant)
- Utiliser un échantillonnage en grappe sans ajustement
- Confondre échantillonnage stratifié et par quotas

### 📚 Ressources
- OMS : Sample Size Determination in Health Studies
- Bennett S. et al. (1991). A simplified general method for cluster-sample surveys
- Lwanga SK, Lemeshow S. (1991). Sample size determination in health studies

## 7. Validation dans AdRev

Le système AdRev valide automatiquement :
- La cohérence entre le type d'étude et la méthode d'échantillonnage
- La présence de justifications pour les choix méthodologiques
- La description complète de la procédure d'échantillonnage

---

**Version** : 2.0  
**Date** : Janvier 2026  
**Auteur** : Équipe AdRev
