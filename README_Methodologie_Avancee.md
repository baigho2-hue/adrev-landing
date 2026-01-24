# 🎯 Méthodologies Avancées - AdRev

## 📌 Résumé Rapide

Votre demande a été intégrée avec succès ! AdRev peut maintenant prendre en compte :

✅ **Études Multicentriques**  
✅ **Échantillonnage en Grappe (Cluster Sampling)**  
✅ **Échantillonnage Stratifié**  
✅ **Effet de Plan (Design Effect)** pour ajustements  
✅ **Taux de perdus de vue**  
✅ **10+ types d'échantillonnage** (aléatoire, systématique, boule de neige, etc.)

---

## 🚀 Ce qui a été fait automatiquement

### 1. Modèle de données enrichi
**Fichier:** `AdRev.Domain/Protocols/ResearchProtocol.cs`
- ✅ Ajout de `IsMulticentric` et `StudyCenters`
- ✅ Ajout de `SamplingType` (énumération)
- ✅ Ajout de `IsStratified`, `StratificationCriteria`
- ✅ Ajout de `IsClusterSampling`, `ClusterSize`, `DesignEffect`
- ✅ Ajout de `ExpectedLossRate`

### 2. Nouvelle énumération créée
**Fichier:** `AdRev.Domain/Enums/SamplingType.cs`
- ✅ 13 types d'échantillonnage disponibles
- ✅ Descriptions françaises pour chaque type

### 3. Logique UI interactive
**Fichier:** `AdRev.Desktop/ProtocolWindow.xaml.cs`
- ✅ Gestionnaires d'événements pour visibilité conditionnelle
- ✅ Initialisation des ComboBox
- ✅ Récupération des données dans CreateProtocol_Click
- ✅ 6 nouvelles méthodes event handler

### 4. Service utilitaire créé
**Fichier:** `AdRev.Core/Protocols/SamplingDescriptionGenerator.cs`
- ✅ Génération automatique de descriptions d'échantillonnage
- ✅ Calculs des ajustements (Design Effect, perdus de vue)
- ✅ Recommandations contextuelles
- ✅ Méthodes helper pour tous les calculs

### 5. Documentation complète
**Fichier:** `Documentation/Guide_Methodologie_Avancee.md`
- ✅ Guide de 200+ lignes sur toutes les fonctionnalités
- ✅ Exemples pratiques avec calculs complets
- ✅ Formules et références bibliographiques
- ✅ Bonnes pratiques

**Fichier:** `Documentation/CHANGELOG_Methodologie_Avancee.md`
- ✅ Liste complète des modifications
- ✅ Instructions pour modifications manuelles
- ✅ Tests à effectuer

---

## ⚠️ ACTION REQUISE : Modification Manuelle du XAML

Le fichier `AdRev.Desktop/ProtocolWindow.xaml` nécessite l'ajout manuel de l'interface utilisateur (problème d'encodage UTF-8).

### 📍 Étape 1 : Section Études Multicentriques

**Emplacement:** Après la ligne contenant `<TextBox x:Name="PopulationTextBox"...` (environ ligne 237)

**Ajouter:**
```xml
<!-- Section Étude Multicentrique -->
<Border Background="#F5F5F5" BorderBrush="#DDDDDD" BorderThickness="1" 
        CornerRadius="5" Padding="10" Margin="0,0,0,15">
    <StackPanel>
        <CheckBox x:Name="IsMulticentricCheckBox" Content="Étude Multicentrique" 
                  FontWeight="SemiBold" Margin="0,0,0,10" 
                  Checked="IsMulticentricCheckBox_Checked" 
                  Unchecked="IsMulticentricCheckBox_Unchecked"/>
        
        <StackPanel x:Name="MulticentricDetailsPanel" Visibility="Collapsed">
            <TextBlock Text="Centres participants (un par ligne) :" 
                       Style="{StaticResource LabelStyle}" FontSize="11"/>
            <TextBox x:Name="StudyCentersTextBox" Height="60" 
                     TextWrapping="Wrap" AcceptsReturn="True" 
                     ToolTip="Listez les centres participants à l'étude"/>
        </StackPanel>
    </StackPanel>
</Border>
```

### 📍 Étape 2 : Section Échantillonnage Avancé

**Emplacement:** Après la section Critères d'Inclusion/Exclusion, avant "Calcul de la Taille de l'Échantillon" (environ ligne 255)

**Ajouter:**
```xml
<!-- Section Échantillonnage Avancé -->
<Border Background="#FFF9E6" BorderBrush="#FFD700" BorderThickness="1" 
        CornerRadius="5" Padding="10" Margin="0,10,0,15">
    <StackPanel>
        <TextBlock Text="⚙️ Configuration de l'Échantillonnage" 
                   FontWeight="Bold" FontSize="13" Margin="0,0,0,10"/>
        
        <TextBlock Text="Type d'échantillonnage :" 
                   Style="{StaticResource LabelStyle}" FontSize="11"/>
        <ComboBox x:Name="SamplingTypeComboBox" Height="32" Padding="8" 
                  VerticalContentAlignment="Center" FontSize="12" Margin="0,0,0,10" 
                  SelectionChanged="SamplingTypeComboBox_SelectionChanged"/>
        
        <!-- Options Stratification -->
        <StackPanel x:Name="StratificationPanel" Visibility="Collapsed" Margin="0,0,0,10">
            <CheckBox x:Name="IsStratifiedCheckBox" Content="Échantillonnage stratifié" 
                      FontSize="11" Margin="0,0,0,5" 
                      Checked="IsStratifiedCheckBox_Checked" 
                      Unchecked="IsStratifiedCheckBox_Unchecked"/>
            <StackPanel x:Name="StratificationDetailsPanel" Visibility="Collapsed">
                <TextBlock Text="Critères de stratification :" 
                           FontSize="10" Foreground="#666" Margin="15,0,0,2"/>
                <TextBox x:Name="StratificationCriteriaTextBox" Height="40" 
                         TextWrapping="Wrap" AcceptsReturn="True" Margin="15,0,0,0" 
                         FontSize="11" ToolTip="Ex: Âge, Sexe, Région géographique..."/>
            </StackPanel>
        </StackPanel>
        
        <!-- Options Grappe -->
        <StackPanel x:Name="ClusterPanel" Visibility="Collapsed" Margin="0,0,0,10">
            <CheckBox x:Name="IsClusterCheckBox" Content="Échantillonnage en grappe" 
                      FontSize="11" Margin="0,0,0,5" 
                      Checked="IsClusterCheckBox_Checked" 
                      Unchecked="IsClusterCheckBox_Unchecked"/>
            <StackPanel x:Name="ClusterDetailsPanel" Visibility="Collapsed">
                <Grid Margin="15,0,0,0">
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="*"/>
                        <ColumnDefinition Width="10"/>
                        <ColumnDefinition Width="*"/>
                    </Grid.ColumnDefinitions>
                    <StackPanel>
                        <TextBlock Text="Taille moyenne des grappes :" 
                                   FontSize="10" Foreground="#666"/>
                        <TextBox x:Name="ClusterSizeTextBox" Height="28" FontSize="11" 
                                 ToolTip="Nombre moyen de sujets par grappe"/>
                    </StackPanel>
                    <StackPanel Grid.Column="2">
                        <TextBlock Text="Effet de plan (Design Effect) :" 
                                   FontSize="10" Foreground="#666"/>
                        <TextBox x:Name="DesignEffectTextBox" Text="1.5" Height="28" 
                                 FontSize="11" ToolTip="Généralement entre 1.5 et 2.0"/>
                    </StackPanel>
                </Grid>
            </StackPanel>
        </StackPanel>
        
        <!-- Taux de perdus de vue -->
        <Grid Margin="0,5,0,0">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="Auto"/>
                <ColumnDefinition Width="10"/>
                <ColumnDefinition Width="100"/>
                <ColumnDefinition Width="Auto"/>
            </Grid.ColumnDefinitions>
            <TextBlock Text="Taux de perdus de vue attendu :" 
                       VerticalAlignment="Center" FontSize="11"/>
            <TextBox x:Name="ExpectedLossRateTextBox" Grid.Column="2" Text="10" 
                     Height="28" FontSize="11" VerticalContentAlignment="Center" 
                     ToolTip="En pourcentage"/>
            <TextBlock Grid.Column="3" Text=" %" VerticalAlignment="Center" 
                       FontSize="11" Margin="5,0,0,0"/>
        </Grid>
    </StackPanel>
</Border>
```

### 📍 (Optionnel) Étape 3 : Améliorer les Calculs

Dans `ProtocolWindow.xaml.cs`, à la fin de la méthode `CalculateCochran_Click` (ligne ~129), remplacer :

**AVANT:**
```csharp
SamplingTextBox.Text = $"La taille d'échantillon minimal a été calculée selon la formule de {formulaUsed}. " +
                       $"Avec une prévalence attendue de {p*100}%, une marge d'erreur de {d*100}% et un niveau de confiance de 95% (Z={z}), " +
                       $"le nombre de sujets requis est de {nCeiling}.";
```

**APRÈS:**
```csharp
// Appliquer ajustements si nécessaire
int adjustedN = nCeiling;
string adjustmentDetails = "";

// Ajustement pour Design Effect (si échantillonnage en grappe)
if (IsClusterCheckBox?.IsChecked == true)
{
    double deff = double.TryParse(DesignEffectTextBox?.Text, System.Globalization.NumberStyles.Any, 
                                  System.Globalization.CultureInfo.InvariantCulture, out double d) ? d : 1.5;
    int deffAdjusted = (int)Math.Ceiling(nCeiling * deff);
    adjustmentDetails += $" Avec un effet de plan de {deff:F2}, la taille passe à {deffAdjusted} sujets.";
    adjustedN = deffAdjusted;
}

// Ajustement pour perdus de vue
double lossRate = double.TryParse(ExpectedLossRateTextBox?.Text, out double lr) ? lr : 0.0;
if (lossRate > 0)
{
    int finalAdjusted = (int)Math.Ceiling(adjustedN / (1.0 - lossRate / 100.0));
    adjustmentDetails += $" En prévoyant {lossRate:F0}% de perdus de vue, la taille finale est de {finalAdjusted} sujets.";
    adjustedN = finalAdjusted;
}

ResultCochran.Text = $"N requis = {adjustedN} sujets{(adjustedN != nCeiling ? $" (base: {nCeiling})" : "")}";

SamplingTextBox.Text = $"La taille d'échantillon minimal a été calculée selon la formule de {formulaUsed}. " +
                       $"Avec une prévalence attendue de {p*100}%, une marge d'erreur de {d*100}% et un niveau de confiance de 95% (Z={z}), " +
                       $"le nombre de sujets requis est de {nCeiling}.{adjustmentDetails}";
```

---

## 🧪 Tests Recommandés

### Test 1 : Étude Multicentrique Simple
1. Créer un nouveau protocole
2. Cocher "Étude Multicentrique"
3. Ajouter 3 centres
4. Sauvegarder et vérifier

### Test 2 : Échantillonnage en Grappe
1. Sélectionner "Quantitative" → "Transversale"
2. Dans Type d'échantillonnage : "En Grappe (Cluster)"
3. Cocher "Échantillonnage en grappe"
4. Taille grappes: 30, Design Effect: 1.8
5. Calculer avec Cochran (p=50%, d=5%)
6. Observer l'ajustement automatique

### Test 3 : Stratifié + Perdus de vue
1. Type d'échantillonnage : "Stratifié"
2. Cocher "Échantillonnage stratifié"
3. Critères : "Âge (<40 ans, ≥40 ans), Sexe (H, F)"
4. Perdus de vue : 15%
5. Calculer et vérifier les ajustements

### Test 4 : Combinaison Complète
1. Multicentrique : OUI (5 centres)
2. Type : "Stratifié en Grappes"
3. Stratifié : OUI (Zone urbaine/rurale)
4. Grappe : OUI (Villages, taille=25, Deff=2.0)
5. Perdus de vue : 20%
6. Vérifier que tout se sauvegarde correctement

---

## 📊 Exemple d'Utilisation Complète

### Scénario : Enquête de Prévalence du Diabète en Milieu Rural

**Configuration:**
- Type d'étude : Quantitative → Transversale Descriptive
- Multicentrique : ✓ (3 régions sanitaires)
- Type échantillonnage : En Grappe (Cluster)
- Grappe : ✓
  - Taille : 25 personnes/village
  - Design Effect : 1.8
- Perdus de vue : 10%

**Calcul:**
1. Base (Cochran) : p=10%, d=3% → **N₀ = 384**
2. + Design Effect (1.8) → **N₁ = 692**
3. + Perdus de vue (10%) → **N final = 769**
4. → **26 villages** (769/25) répartis dans 3 régions

**Texte généré automatiquement:**
> « La taille d'échantillon minimal a été calculée selon la formule de Cochran (population infinie). Avec une prévalence attendue de 10%, une marge d'erreur de 3% et un niveau de confiance de 95% (Z=1.96), le nombre de sujets requis est de 384. Avec un effet de plan de 1.80, la taille passe à 692 sujets. En prévoyant 10% de perdus de vue, la taille finale est de 769 sujets. »

---

## 📚 Documentation Disponible

1. `Documentation/Guide_Methodologie_Avancee.md` - Guide complet (200+ lignes)
2. `Documentation/CHANGELOG_Methodologie_Avancee.md` - Liste des modifications
3. Ce fichier - Instructions d'utilisation rapide

---

## 🔗 Ressources Supplémentaires

### Formules Clés

**Design Effect (Deff):**
```
Deff = 1 + (m - 1) × ICC
```
- m = taille moyenne de la grappe
- ICC = coefficient de corrélation intra-classe

**Taille ajustée:**
```
N_ajusté = N_base × Deff
```

**Perdus de vue:**
```
N_final = N_ajusté / (1 - taux%)
```

### Valeurs Typiques

| Contexte | Design Effect | Perdus de vue |
|----------|--------------|---------------|
| Transversale simple | 1.0 | 5-10% |
| Transversale en grappe | 1.5-2.0 | 10% |
| Cohorte < 1 an | 1.0 | 10-15% |
| Cohorte > 1 an | 1.0 | 15-25% |
| ERC | 1.0 | 10-20% |

---

## ✅ Checklist Finale

- [x] Modèle de données enrichi
- [x] Énumération SamplingType créée
- [x] Gestionnaires d'événements ajoutés
- [x] Service SamplingDescriptionGenerator créé
- [x] Documentation complète rédigée
- [ ] **XAML UI à ajouter manuellement (REQUIS)**
- [ ] Tests fonctionnels
- [ ] Validation avec protocole réel

---

## 🆘 Support

Si vous rencontrez des problèmes :

1. **Vérifier** que tous les using sont corrects dans les fichiers C#
2. **Recompiler** le projet après modifications XAML
3. **Consulter** `Documentation/Guide_Methodologie_Avancee.md` pour exemples
4. **Tester** progressivement chaque fonctionnalité

---

**Bonne utilisation d'AdRev avec ses nouvelles capacités avancées ! 🚀**
