# Résumé des Modifications - Méthodologie Avancée

## 📅 Date : Janvier 2026

## 🎯 Objectif
Enrichir AdRev pour prendre en charge des méthodologies de recherche plus complexes, notamment les études multicentriques et l'échantillonnage en grappe.

## ✨ Nouvelles Fonctionnalités

### 1. Études Multicentriques
**Fichiers modifiés :**
- `AdRev.Domain/Protocols/ResearchProtocol.cs`
- `AdRev.Desktop/ProtocolWindow.xaml` (à modifier manuellement)
- `AdRev.Desktop/ProtocolWindow.xaml.cs`

**Champs ajoutés :**
- `bool IsMulticentric` : Indique si l'étude est multicentrique
- `string StudyCenters` : Liste des centres participants

**Interface utilisateur :**
- Checkbox "Étude Multicentrique"
- Champ texte pour lister les centres (affiché conditionnellement)

### 2. Types d'Échantillonnage

**Nouveau fichier :**
- `AdRev.Domain/Enums/SamplingType.cs`

**Types disponibles :**

**Probabilistes :**
- Aléatoire Simple
- Systématique
- Stratifié
- En Grappe (Cluster)
- À Plusieurs Degrés
- Stratifié en Grappes

**Non Probabilistes :**
- De Convenance
- Raisonné (Purposive)
- Boule de Neige
- Par Quotas

**Autres :**
- Exhaustif (Recensement)

### 3. Échantillonnage Avancé

**Champs ajoutés au modèle ResearchProtocol :**
- `SamplingType SamplingType` : Type d'échantillonnage (énumération)
- `bool IsStratified` : Échantillonnage stratifié
- `string StratificationCriteria` : Critères de stratification
- `bool IsClusterSampling` : Échantillonnage en grappe
- `int ClusterSize` : Taille moyenne des grappes
- `double DesignEffect` : Effet de plan (Deff) pour ajuster la taille d'échantillon
- `double ExpectedLossRate` : Taux de perdus de vue attendu (%)

**Interface utilisateur :**
- ComboBox pour sélectionner le type d'échantillonnage
- Panels conditionnels pour :
  - Stratification (critères)
  - Échantillonnage en grappe (taille des grappes, effet de design)
  - Taux de perdus de vue

### 4. Logique Dynamique

**Gestionnaires d'événements ajoutés :**
- `IsMulticentricCheckBox_Checked/Unchecked` : Affiche/cache les centres
- `SamplingTypeComboBox_SelectionChanged` : Affiche les panels appropriés selon le type
- `IsStratifiedCheckBox_Checked/Unchecked` : Affiche/cache critères de stratification
- `IsClusterCheckBox_Checked/Unchecked` : Affiche/cache paramètres de grappe

## 📊 Formules et Calculs

### Ajustement pour l'Effet de Plan
```
N ajusté = N calculé × Design Effect
```

### Ajustement pour Perdus de Vue
```
N final = N ajusté / (1 - taux de perdus de vue)
```

### Exemple Complet
```
N₀ = 384 (Cochran)
N₁ = 384 × 1.5 (Design Effect) = 576
N final = 576 / 0.90 (10% perdus de vue) = 640 sujets
```

## 📝 Documentation Créée

**Fichier :** `Documentation/Guide_Methodologie_Avancee.md`

**Contenu :**
1. Vue d'ensemble des nouvelles fonctionnalités
2. Guide des études multicentriques
3. Description détaillée de tous les types d'échantillonnage
4. Explication de l'effet de plan (Design Effect)
5. Guide sur les perdus de vue
6. Exemple pratique complet avec tous les ajustements
7. Bonnes pratiques et conseils
8. Ressources bibliographiques

## 🔧 Actions Requises

### ⚠️ IMPORTANT : Modification Manuelle du XAML
Le fichier `AdRev.Desktop/ProtocolWindow.xaml` nécessite une modification manuelle en raison de problèmes d'encodage.

**Emplacement :** Après la ligne 237 (après "Population d'étude")

**Code à ajouter :**

```xml
<!-- Section Étude Multicentrique -->
<Border Background="#F5F5F5" BorderBrush="#DDDDDD" BorderThickness="1" CornerRadius="5" Padding="10" Margin="0,0,0,15">
    <StackPanel>
        <CheckBox x:Name="IsMulticentricCheckBox" Content="Étude Multicentrique" FontWeight="SemiBold" Margin="0,0,0,10" Checked="IsMulticentricCheckBox_Checked" Unchecked="IsMulticentricCheckBox_Unchecked"/>
        
        <StackPanel x:Name="MulticentricDetailsPanel" Visibility="Collapsed">
            <TextBlock Text="Centres participants (un par ligne) :" Style="{StaticResource LabelStyle}" FontSize="11"/>
            <TextBox x:Name="StudyCentersTextBox" Height="60" TextWrapping="Wrap" AcceptsReturn="True" ToolTip="Listez les centres participants à l'étude"/>
        </StackPanel>
    </StackPanel>
</Border>
```

**Puis, après les critères d'inclusion/exclusion (ligne 254), ajouter :**

```xml
<!-- Section Échantillonnage Avancé -->
<Border Background="#FFF9E6" BorderBrush="#FFD700" BorderThickness="1" CornerRadius="5" Padding="10" Margin="0,10,0,15">
    <StackPanel>
        <TextBlock Text="⚙️ Configuration de l'Échantillonnage" FontWeight="Bold" FontSize="13" Margin="0,0,0,10"/>
        
        <TextBlock Text="Type d'échantillonnage :" Style="{StaticResource LabelStyle}" FontSize="11"/>
        <ComboBox x:Name="SamplingTypeComboBox" Height="32" Padding="8" VerticalContentAlignment="Center" FontSize="12" Margin="0,0,0,10" SelectionChanged="SamplingTypeComboBox_SelectionChanged"/>
        
        <!-- Options Stratification -->
        <StackPanel x:Name="StratificationPanel" Visibility="Collapsed" Margin="0,0,0,10">
            <CheckBox x:Name="IsStratifiedCheckBox" Content="Échantillonnage stratifié" FontSize="11" Margin="0,0,0,5" Checked="IsStratifiedCheckBox_Checked" Unchecked="IsStratifiedCheckBox_Unchecked"/>
            <StackPanel x:Name="StratificationDetailsPanel" Visibility="Collapsed">
                <TextBlock Text="Critères de stratification :" FontSize="10" Foreground="#666" Margin="15,0,0,2"/>
                <TextBox x:Name="StratificationCriteriaTextBox" Height="40" TextWrapping="Wrap" AcceptsReturn="True" Margin="15,0,0,0" FontSize="11" ToolTip="Ex: Âge, Sexe, Région géographique..."/>
            </StackPanel>
        </StackPanel>
        
        <!-- Options Grappe -->
        <StackPanel x:Name="ClusterPanel" Visibility="Collapsed" Margin="0,0,0,10">
            <CheckBox x:Name="IsClusterCheckBox" Content="Échantillonnage en grappe" FontSize="11" Margin="0,0,0,5" Checked="IsClusterCheckBox_Checked" Unchecked="IsClusterCheckBox_Unchecked"/>
            <StackPanel x:Name="ClusterDetailsPanel" Visibility="Collapsed">
                <Grid Margin="15,0,0,0">
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="*"/>
                        <ColumnDefinition Width="10"/>
                        <ColumnDefinition Width="*"/>
                    </Grid.ColumnDefinitions>
                    <StackPanel>
                        <TextBlock Text="Taille moyenne des grappes :" FontSize="10" Foreground="#666"/>
                        <TextBox x:Name="ClusterSizeTextBox" Height="28" FontSize="11" ToolTip="Nombre moyen de sujets par grappe"/>
                    </StackPanel>
                    <StackPanel Grid.Column="2">
                        <TextBlock Text="Effet de plan (Design Effect) :" FontSize="10" Foreground="#666"/>
                        <TextBox x:Name="DesignEffectTextBox" Text="1.5" Height="28" FontSize="11" ToolTip="Généralement entre 1.5 et 2.0"/>
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
            <TextBlock Text="Taux de perdus de vue attendu :" VerticalAlignment="Center" FontSize="11"/>
            <TextBox x:Name="ExpectedLossRateTextBox" Grid.Column="2" Text="10" Height="28" FontSize="11" VerticalContentAlignment="Center" ToolTip="En pourcentage"/>
            <TextBlock Grid.Column="3" Text=" %" VerticalAlignment="Center" FontSize="11" Margin="5,0,0,0"/>
        </Grid>
    </StackPanel>
</Border>
```

## ✅ Fichiers Modifiés Automatiquement

1. ✅ `AdRev.Domain/Protocols/ResearchProtocol.cs` - Modèle enrichi
2. ✅ `AdRev.Domain/Enums/SamplingType.cs` - Nouvelle énumération créée
3. ✅ `AdRev.Desktop/ProtocolWindow.xaml.cs` - Logique ajoutée
4. ✅ `Documentation/Guide_Methodologie_Avancee.md` - Documentation créée

## ⏳ À Faire Manuellement

1. ⚠️ `AdRev.Desktop/ProtocolWindow.xaml` - Ajouter les sections UI (voir code ci-dessus)
2. 🔄 Mettre à jour `ProtocolValidator.cs` si nécessaire pour valider les nouveaux champs
3. 🔄 Mettre à jour les services d'export (Word, PDF) pour inclure les nouvelles informations

## 🧪 Tests à Effectuer

1. **Test Multicentrique :**
   - Cocher/décocher "Étude Multicentrique"
   - Vérifier l'affichage du champ centres
   - Sauvegarder et vérifier la persistence

2. **Test Échantillonnage Stratifié :**
   - Sélectionner "Stratifié" dans le type
   - Cocher "Échantillonnage stratifié"
   - Entrer des critères

3. **Test Échantillonnage en Grappe :**
   - Sélectionner "En Grappe (Cluster)"
   - Cocher "Échantillonnage en grappe"
   - Entrer taille des grappes et effet de design
   - Vérifier les calculs

4. **Test Combiné :**
   - Étude multicentrique + échantillonnage en grappe stratifié
   - Avec perdus de vue
   - Valider la cohérence des données sauvegardées

## 📚 Références Utilisées

1. OMS : Sample Size Determination in Health Studies
2. Bennett S. et al. (1991). A simplified general method for cluster-sample surveys
3. Lwanga SK, Lemeshow S. (1991). Sample size determination in health studies
4. Cochran WG. (1977). Sampling Techniques. 3rd ed.

## 🎓 Cas d'Usage Typiques

### Cas 1 : Enquête de prévalence en milieu rural
- Type : Transversale descriptive
- Échantillonnage : En grappe (villages)
- Design Effect : 1.8
- Multicentrique : 3 districts

### Cas 2 : Essai clinique multicentrique
- Type : ERC
- Échantillonnage : Aléatoire simple
- Multicentrique : 5 hôpitaux
- Stratifié : Par centre
- Perdus de vue : 15%

### Cas 3 : Étude cas-témoins appariés
- Type : Cas-Témoins
- Échantillonnage : Apparié (existant)
- Stratifié : Âge, sexe
- Multicentrique : Non

---

**Note :** Cette mise à jour apporte des capacités de niveau professionnel à AdRev, permettant de gérer des protocoles de recherche complexes conformes aux standards internationaux.
