# 🚀 Démarrage Rapide - Nouvelles Fonctionnalités

## 📌 En 30 secondes

AdRev supporte maintenant :
- ✅ Études multicentriques
- ✅ Échantillonnage en grappe
- ✅ 13 types d'échantillonnage
- ✅ Ajustements automatiques (Design Effect, perdus de vue)

## ⚡ Action Immédiate Requise

**Fichier à modifier :** `AdRev.Desktop/ProtocolWindow.xaml`

**Où ?** Ouvrez `README_Methodologie_Avancee.md` et copiez les 2 sections UI marquées

**Temps estimé :** 10 minutes

## 🎯 Utilisation Rapide

### 1. Étude Multicentrique
```
☑ Étude Multicentrique
┗━ Centres :
    - CHU de Bamako
    - Hôpital Gabriel Touré
    - CSRéf Koulikoro
```

### 2. Échantillonnage en Grappe
```
Type d'échantillonnage: [En Grappe (Cluster) ▼]
☑ Échantillonnage en grappe
┣━ Taille moyenne des grappes: 25
┗━ Design Effect: 1.8
```

### 3. Perdus de Vue
```
Taux de perdus de vue attendu: [10] %
```

### 4. Calcul Automatique
```
N_base = 384 (Cochran)
× 1.8 (Design Effect) = 692
÷ 0.90 (perdus 10%) = 769 sujets finaux
```

## 📊 Formules Utiles

**Design Effect :**
```
Deff = 1 + (taille_grappe - 1) × ICC
```

**Taille Finale :**
```
N_final = (N_base × Deff) / (1 - taux_perdus%)
```

**Nombre de Grappes :**
```
nb_grappes = N_final / taille_moyenne_grappe
```

## 🧪 Test Rapide

1. Créer protocole
2. Sélectionner "Quantitative" → "Transversale"
3. Type échantillonnage : "En Grappe"
4. Cocher grappe, entrer : taille=30, Deff=1.5
5. Perdus de vue : 10%
6. Calculer Cochran : p=50%, d=5%
7. Observer : N passe de 384 → 576 → 640

## 📚 Documentation

| Besoin | Fichier |
|--------|---------|
| Instructions complètes | `README_Methodologie_Avancee.md` |
| Guide académique | `Documentation/Guide_Methodologie_Avancee.md` |
| Liste modifications | `Documentation/CHANGELOG_Methodologie_Avancee.md` |
| Statut final | `INTEGRATION_COMPLETE.md` |

## ✅ Checklist

- [x] Modèle de données enrichi
- [x] Code-behind modifié
- [x] Service générateur créé
- [x] Documentation rédigée
- [ ] **XAML UI à ajouter** ⚠️
- [ ] Tester fonctionnalités
- [ ] Valider avec protocole réel

## 🆘 Problème ?

1. Vérifier que XAML est modifié
2. Recompiler le projet
3. Consulter README_Methodologie_Avancee.md
4. Vérifier les using en haut des fichiers C#

## 🎓 Exemple Complet

**Contexte :** Étude prévalence diabète en zones rurales de 3 régions

**Configuration :**
- Multicentrique : 3 régions ✓
- Type : En Grappe (villages)
- Taille grappe : 25 personnes
- Design Effect : 1.8
- Perdus de vue : 10%

**Résultat :**
- N base : 384
- N ajusté Deff : 692
- N final : 769
- Villages : 31 (≈10/région)

**Temps de configuration :** < 3 minutes

---

**🎯 Objectif : Passer d'un simple calculateur à un système professionnel de méthodologie de recherche !**
