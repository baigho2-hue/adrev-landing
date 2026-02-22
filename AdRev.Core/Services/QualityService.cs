using AdRev.Domain.Quality;
using AdRev.Domain.Protocols;
using AdRev.Domain.Enums;
using AdRev.Domain.Models;
using System.Collections.Generic;

namespace AdRev.Core.Services
{
    public class QualityService
    {
        public List<QualityChecklist> GetAllChecklists()
        {
            return new List<QualityChecklist>
            {
                GetCOREQ(),
                GetSTROBE(),
                GetPRISMA(),
                GetCARE(),
                GetSRQR(),
                GetCONSORT(),
                GetGRAMMS()
            };
        }

        public List<QualityChecklist> GetRecommendedChecklists(ResearchProject project)
        {
            var checklists = new List<QualityChecklist>();

            if (project.Domain == ScientificDomain.Biomedical)
            {
                if (project.StudyType == StudyType.Qualitative)
                {
                    checklists.Add(GetCOREQ());
                    checklists.Add(GetSRQR());
                }
                else if (project.StudyType == StudyType.Quantitative)
                {
                    if (project.EpidemiologyType == EpidemiologicalStudyType.RandomizedControlledTrial)
                    {
                        checklists.Add(GetCONSORT());
                        checklists.Add(GetSTROBE());
                    }
                    else
                    {
                        checklists.Add(GetSTROBE());
                        checklists.Add(GetCONSORT());
                    }
                }
                else // Mixed
                {
                    checklists.Add(GetGRAMMS());
                    checklists.Add(GetCOREQ());
                    checklists.Add(GetSTROBE());
                }
                checklists.Add(GetPRISMA());
                checklists.Add(GetCARE());
            }
            else if (project.Domain == ScientificDomain.History)
            {
                checklists.Add(GetHISTORIC()); // Heuristique & Critique Source
                if (project.StudyType == StudyType.Qualitative) checklists.Add(GetSRQR());
            }
            else if (project.Domain == ScientificDomain.Geography || project.Domain == ScientificDomain.SpacePhysics)
            {
                checklists.Add(GetSPATIAL()); // Qualité Spatial & Modélisation
                if (project.StudyType == StudyType.Quantitative) checklists.Add(GetSTROBE());
            }
            else if (project.Domain == ScientificDomain.SocialSciences)
            {
                checklists.Add(GetSRQR());
                checklists.Add(GetSOCIOLOGY()); // Ethnography & Survey Ethics
                if (project.StudyType == StudyType.Mixed) checklists.Add(GetGRAMMS());
            }
            else
            {
                // General fallback
                checklists.Add(GetSRQR());
                checklists.Add(GetSTROBE());
                checklists.Add(GetGRAMMS());
            }

            return checklists;
        }

        private QualityChecklist GetCOREQ()
        {
            return new QualityChecklist
            {
                Name = "COREQ (32 items)",
                Description = "Consolidated criteria for reporting qualitative research (Entretiens & Focus Groups)",
                SourceUrl = "https://www.equator-network.org/reporting-guidelines/coreq",
                Sections = new List<ChecklistSection>
                {
                    // Domain 1: Research team and reflexivity
                    new ChecklistSection
                    {
                        Name = "Domaine 1 : Équipe de recherche et réflexivité",
                        Items = new List<ChecklistItem>
                        {
                            // Personal Characteristics
                            new ChecklistItem { Id="1", Requirement="Interviewer/facilitateur", Description="Qui a mené les entretiens ou les groupes de discussion ?"},
                            new ChecklistItem { Id="2", Requirement="Qualifications", Description="Quelles étaient les qualifications du chercheur (MD, PhD) ?"},
                            new ChecklistItem { Id="3", Requirement="Profession", Description="Quelle était leur profession au moment de l'étude ?"},
                            new ChecklistItem { Id="4", Requirement="Genre", Description="Le genre du chercheur était-il précisé ?"},
                            new ChecklistItem { Id="5", Requirement="Expérience et formation", Description="Le chercheur avait-il de l'expérience ou une formation qualitative ?"},
                            
                            // Relationship with participants
                            new ChecklistItem { Id="6", Requirement="Relation établie", Description="Une relation a-t-elle été établie avant le début de l'étude ?"},
                            new ChecklistItem { Id="7", Requirement="Connaissance du chercheur", Description="Quels renseignements les participants avaient-ils sur le chercheur ?"},
                            new ChecklistItem { Id="8", Requirement="Caractéristiques de l'interviewer", Description="Quelles caractéristiques (biais, hypothèses) ont été rapportées ?"}
                        }
                    },
                    // Domain 2: Study design
                    new ChecklistSection
                    {
                        Name = "Domaine 2 : Conception de l'étude",
                        Items = new List<ChecklistItem>
                        {
                            // Theoretical framework
                            new ChecklistItem { Id="9", Requirement="Orientation méthodologique", Description="Quelle méthode (Grounded Theory, Phénoménologie...) ?"},
                            
                            // Participant selection
                            new ChecklistItem { Id="10", Requirement="Échantillonnage", Description="Comment les participants ont-ils été sélectionnés ?"},
                            new ChecklistItem { Id="11", Requirement="Méthode d'approche", Description="Comment ont-ils été approchés (mail, téléphone, face-à-face) ?"},
                            new ChecklistItem { Id="12", Requirement="Taille de l'échantillon", Description="Combien de participants ?"},
                            new ChecklistItem { Id="13", Requirement="Non-participation", Description="Combien ont refusé ou abandonné ?"},
                            
                            // Setting
                            new ChecklistItem { Id="14", Requirement="Cadre de collecte", Description="Où les données ont-elles été collectées (domicile, clinique) ?"},
                            new ChecklistItem { Id="15", Requirement="Présence de tiers", Description="Y avait-il d'autres personnes présentes ?"},
                            new ChecklistItem { Id="16", Requirement="Description de l'échantillon", Description="Données démographiques importantes fournies ?"},
                            
                            // Data collection
                            new ChecklistItem { Id="17", Requirement="Guide d'entretien", Description="Questions, prompts, guides utilisés ?"},
                            new ChecklistItem { Id="18", Requirement="Entretiens répétés", Description="Les participants ont-ils été interrogés plusieurs fois ?"},
                            new ChecklistItem { Id="19", Requirement="Enregistrement audio/visuel", Description="La collecte a-t-elle été enregistrée ?"},
                            new ChecklistItem { Id="20", Requirement="Notes de terrain", Description="Des notes ont-elles été prises pendant ou après ?"},
                            new ChecklistItem { Id="21", Requirement="Durée", Description="Quelle était la durée des entretiens ?"},
                            new ChecklistItem { Id="22", Requirement="Saturation des données", Description="La saturation a-t-elle été atteinte ?"},
                            new ChecklistItem { Id="23", Requirement="Retour des transcrits", Description="Les transcrits ont-ils été renvoyés aux participants pour correction ?"}
                        }
                    },
                    // Domain 3: Analysis and findings
                    new ChecklistSection
                    {
                        Name = "Domaine 3 : Analyse et résultats",
                        Items = new List<ChecklistItem>
                        {
                            // Data analysis
                            new ChecklistItem { Id="24", Requirement="Nombre de codeurs", Description="Combien de chercheurs ont codé les données ?"},
                            new ChecklistItem { Id="25", Requirement="Arbre de codage", Description="La description de l'arbre de codage est-elle fournie ?"},
                            new ChecklistItem { Id="26", Requirement="Dérivation des thèmes", Description="Les thèmes ont-ils été identifiés à l'avance ou ont-ils émergé ?"},
                            new ChecklistItem { Id="27", Requirement="Logiciel", Description="Quel logiciel a été utilisé (NVivo, Atlas.ti) ?"},
                            new ChecklistItem { Id="28", Requirement="Vérification par les participants", Description="Les participants ont-ils validé les résultats (Member checking) ?"},
                            
                            // Reporting
                            new ChecklistItem { Id="29", Requirement="Citations présentées", Description="Des citations (verbatims) illustrent-elles les thèmes ?"},
                            new ChecklistItem { Id="30", Requirement="Cohérence données/résultats", Description="Il y a-t-il une cohérence entre les données présentées et les résultats ?"},
                            new ChecklistItem { Id="31", Requirement="Clarté des thèmes majeurs", Description="Les thèmes majeurs sont-ils clairement présentés ?"},
                            new ChecklistItem { Id="32", Requirement="Clarté des thèmes mineurs", Description="Les thèmes mineurs sont-ils discutés ?"}
                        }
                    }
                }
            };
        }

        private QualityChecklist GetSTROBE()
        {
            return new QualityChecklist
            {
                Name = "STROBE",
                Description = "Strengthening the Reporting of Observational Studies in Epidemiology (Transversales, Cohortes, Cas-Témoins)",
                Sections = new List<ChecklistSection>
                {
                    new ChecklistSection
                    {
                        Name = "Introduction",
                        Items = new List<ChecklistItem> { 
                            new ChecklistItem { Id="1", Requirement="Contexte et Justification", Description="Expliquer le rationnel scientifique et théorique." },
                            new ChecklistItem { Id="2", Requirement="Objectifs", Description="Objectif principal et hypothèses pré-spécifiées." } 
                        }
                    },
                    new ChecklistSection
                    {
                        Name = "Méthodes",
                        Items = new List<ChecklistItem> { 
                            new ChecklistItem { Id="3", Requirement="Type d'étude", Description="Conception de l'étude claire." },
                            new ChecklistItem { Id="4", Requirement="Cadre", Description="Lieux et dates de collecte." },
                            new ChecklistItem { Id="5", Requirement="Participants", Description="Critères d'inclusion, d'exclusion, sources." },
                            new ChecklistItem { Id="6", Requirement="Variables", Description="Toutes les variables (résultat, expositions, facteurs) définies." },
                            new ChecklistItem { Id="7", Requirement="Sources de données", Description="Mesures, outils validés ?" },
                            new ChecklistItem { Id="8", Requirement="Biais", Description="Efforts pour traiter les biais potentiels." },
                            new ChecklistItem { Id="9", Requirement="Taille d'étude", Description="Justification de la taille (calcul de puissance)." }
                        }
                    },
                     new ChecklistSection
                    {
                        Name = "Résultats",
                        Items = new List<ChecklistItem> { 
                            new ChecklistItem { Id="10", Requirement="Participants (Flowchart)", Description="Nombres à chaque étape (éligibles, inclus, analysés)." },
                            new ChecklistItem { Id="11", Requirement="Données descriptives", Description="Caractéristiques socio-démographiques." },
                            new ChecklistItem { Id="12", Requirement="Résultats principaux", Description="Estimations non ajustées et ajustées (IC 95%)." }
                        }
                    }
                    ,
                     new ChecklistSection
                    {
                        Name = "Discussion",
                        Items = new List<ChecklistItem> { 
                            new ChecklistItem { Id="18", Requirement="Résultats clés", Description="Résumé des principaux résultats (référence aux objectifs)." },
                            new ChecklistItem { Id="19", Requirement="Limitations", Description="Biais, imprécisions, validité interne/externe." },
                            new ChecklistItem { Id="20", Requirement="Interprétation", Description="Comparaison avec autres études, hypothèses." },
                            new ChecklistItem { Id="21", Requirement="Généralisabilité", Description="Validité externe (à qui s'appliquent les résultats ?)." }
                        }
                    }
                }
            };
        }

        private QualityChecklist GetPRISMA()
        {
            return new QualityChecklist
            {
                Name = "PRISMA",
                Description = "Preferred Reporting Items for Systematic Reviews and Meta-Analyses",
                Sections = new List<ChecklistSection>
                {
                    new ChecklistSection
                    {
                        Name = "Titre & Résumé",
                        Items = new List<ChecklistItem> { 
                            new ChecklistItem { Id="1", Requirement="Titre", Description="Identifier le rapport comme une revue systématique." },
                            new ChecklistItem { Id="2", Requirement="Résumé", Description="Résumé structuré (méthodes, résultats, conclusion)." }
                        }
                    },
                    new ChecklistSection
                    {
                        Name = "Méthodes",
                        Items = new List<ChecklistItem> { 
                            new ChecklistItem { Id="5", Requirement="Critères d'éligibilité", Description="PICO (Population, Intervention, Comparator, Outcome)." },
                            new ChecklistItem { Id="6", Requirement="Sources d'information", Description="Bases de données interrogées (PubMed, etc.), dates." },
                            new ChecklistItem { Id="7", Requirement="Stratégie de recherche", Description="Chaîne de recherche complète pour au moins une base." },
                            new ChecklistItem { Id="8", Requirement="Processus de sélection", Description="Qui a trié ? (Doubles aveugle ?)" },
                            new ChecklistItem { Id="11", Requirement="Évaluation du risque de biais", Description="Outils utilisés (ex: RoB 2, ROBINS-I)." }
                        }
                    },
                    new ChecklistSection
                    {
                        Name = "Résultats",
                        Items = new List<ChecklistItem> { 
                            new ChecklistItem { Id="16", Requirement="Sélection des études", Description="Diagramme de flux (PRISMA Flow Diagram)." },
                            new ChecklistItem { Id="18", Requirement="Risque de biais", Description="Présentation des résultats de l'évaluation du biais." }
                        }
                    }
                }
            };
        }

        private QualityChecklist GetCARE()
        {
             return new QualityChecklist
            {
                Name = "CARE",
                Description = "Case Report Guidelines (Rapport de Cas)",
                Sections = new List<ChecklistSection>
                {
                    new ChecklistSection
                    {
                        Name = "Éléments liminaires",
                        Items = new List<ChecklistItem> { 
                            new ChecklistItem { Id="1", Requirement="Titre", Description="Identifier le 'Case Report' dans le titre." },
                            new ChecklistItem { Id="2", Requirement="Mots-clés", Description="2 à 5 mots-clés." }
                        }
                    },
                    new ChecklistSection
                    {
                        Name = "Présentation du cas",
                        Items = new List<ChecklistItem> { 
                            new ChecklistItem { Id="5a", Requirement="Informations patient", Description="Données dé-identifiées, préoccupations principales." },
                            new ChecklistItem { Id="5b", Requirement="Histoire clinique", Description="Antécédents pertinents." },
                            new ChecklistItem { Id="6", Requirement="Chronologie (Timeline)", Description="Chronologie des événements cliniques." },
                            new ChecklistItem { Id="7", Requirement="Évaluation Diagnostique", Description="Méthodes diagnostiques, défis, pronostic." },
                            new ChecklistItem { Id="8", Requirement="Intervention Thérapeutique", Description="Types, doses, durée des traitements." },
                            new ChecklistItem { Id="9", Requirement="Suivi et Résultats", Description="Résultats évalués par clinicien et patient." }
                        }
                    },
                    new ChecklistSection
                    {
                        Name = "Discussion & Consentement",
                        Items = new List<ChecklistItem> { 
                            new ChecklistItem { Id="10", Requirement="Discussion", Description="Points forts, limites, littérature." },
                            new ChecklistItem { Id="11", Requirement="Perspective Patient", Description="Opinion du patient sur son cas." },
                            new ChecklistItem { Id="12", Requirement="Consentement éclairé", Description="Le patient a-t-il signé un consentement ?" }
                        }
                    }
                }
            };
        }

        private QualityChecklist GetSRQR()
        {
            return new QualityChecklist
            {
                Name = "SRQR",
                Description = "Standards for Reporting Qualitative Research (21 items)",
                Sections = new List<ChecklistSection>
                {
                    new ChecklistSection
                    {
                        Name = "Titre & Résumé",
                        Items = new List<ChecklistItem>
                        {
                            new ChecklistItem { Id="1", Requirement="Titre", Description="Décrit clairement la recherche qualitative." },
                            new ChecklistItem { Id="2", Requirement="Résumé", Description="Résumé structuré (Poblème, Design, Résultats, Conclusion)." }
                        }
                    },
                    new ChecklistSection
                    {
                        Name = "Introduction",
                        Items = new List<ChecklistItem>
                        {
                            new ChecklistItem { Id="3", Requirement="Problématique", Description="Justification de la recherche." },
                            new ChecklistItem { Id="4", Requirement="Question de recherche", Description="But et questions spécifiques." }
                        }
                    },
                    new ChecklistSection
                    {
                        Name = "Méthodes : Fondements",
                        Items = new List<ChecklistItem>
                        {
                            new ChecklistItem { Id="5", Requirement="Approche et Paradigme", Description="Ex: Phénoménologie, Grounded Theory..." },
                            new ChecklistItem { Id="6", Requirement="Position du chercheur", Description="Biais potentiels, réflexivité." },
                            new ChecklistItem { Id="7", Requirement="Cadre de l'étude", Description="Lieu et contexte social." },
                            new ChecklistItem { Id="8", Requirement="Échantillonnage", Description="Stratégie et critères de sélection." }
                        }
                    },
                    new ChecklistSection
                    {
                        Name = "Méthodes : Collecte & Analyse",
                        Items = new List<ChecklistItem>
                        {
                            new ChecklistItem { Id="9", Requirement="Instruments de collecte", Description="Outils utilisés (guide d'entretien, etc.)." },
                            new ChecklistItem { Id="10", Requirement="Procédure de collecte", Description="Comment les données ont été recueillies." },
                            new ChecklistItem { Id="11", Requirement="Analyse des données", Description="Méthode d'analyse (codage, thématique)." },
                            new ChecklistItem { Id="12", Requirement="Crédibilité (Trustworthiness)", Description="Techniques : triangulation, validation par les pairs..." }
                        }
                    },
                    new ChecklistSection
                    {
                        Name = "Éthique & Résultats",
                        Items = new List<ChecklistItem>
                        {
                            new ChecklistItem { Id="13", Requirement="Éthique", Description="Comité d'éthique et consentement." },
                            new ChecklistItem { Id="14", Requirement="Résultats/Findings", Description="Présentation claire des thèmes." },
                            new ChecklistItem { Id="15", Requirement="Lien avec les données", Description="Utilisation de citations (verbatims)." }
                        }
                    }
                }
            };
        }
        private QualityChecklist GetCONSORT()
        {
            return new QualityChecklist
            {
                Name = "CONSORT",
                Description = "Consolidated Standards of Reporting Trials (Essais Randomisés)",
                Sections = new List<ChecklistSection>
                {
                    new ChecklistSection
                    {
                        Name = "Titre & Résumé",
                        Items = new List<ChecklistItem>
                        {
                            new ChecklistItem { Id="1a", Requirement="Identification comme essai randomisé", Description="Le titre doit inclure 'Essai Randomisé'." },
                            new ChecklistItem { Id="1b", Requirement="Résumé structuré", Description="Présentation structurée du design, des méthodes et résultats." }
                        }
                    },
                    new ChecklistSection
                    {
                        Name = "Introduction",
                        Items = new List<ChecklistItem>
                        {
                            new ChecklistItem { Id="2a", Requirement="Contexte scientifique", Description="Justification et état des connaissances." },
                            new ChecklistItem { Id="2b", Requirement="Objectifs et Hypothèses", Description="Objectifs précis et hypothèses testées." }
                        }
                    },
                    new ChecklistSection
                    {
                        Name = "Méthodes : Design & Participants",
                        Items = new List<ChecklistItem>
                        {
                            new ChecklistItem { Id="3a", Requirement="Design de l'essai", Description="Description (ex: parallèle, factoriel) et ratio d'allocation." },
                            new ChecklistItem { Id="3b", Requirement="Changements de design", Description="Modifications après le début de l'essai, avec justifications." },
                            new ChecklistItem { Id="4a", Requirement="Critères d'éligibilité", Description="Critères d'inclusion et d'exclusion des participants." },
                            new ChecklistItem { Id="4b", Requirement="Cadre et lieux", Description="Où les données ont-elles été collectées ?" }
                        }
                    },
                    new ChecklistSection
                    {
                        Name = "Méthodes : Interventions & Critères",
                        Items = new List<ChecklistItem>
                        {
                            new ChecklistItem { Id="5", Requirement="Interventions", Description="Détails de l'intervention pour chaque groupe (suffisant pour réplication)." },
                            new ChecklistItem { Id="6a", Requirement="Critères de jugement (Outcomes)", Description="Définition précise des critères primaires et secondaires." },
                            new ChecklistItem { Id="6b", Requirement="Changement de critères", Description="Toute modification des critères après le début." }
                        }
                    },
                    new ChecklistSection
                    {
                        Name = "Méthodes : Taille d'échantillon",
                        Items = new List<ChecklistItem>
                        {
                            new ChecklistItem { Id="7a", Requirement="Calcul de la taille", Description="Comment la taille d'échantillon a été déterminée ?" },
                            new ChecklistItem { Id="7b", Requirement="Analyses intérimaires", Description="Directives d'arrêt si applicables." }
                        }
                    },
                    new ChecklistSection
                    {
                        Name = "Méthodes : Randomisation",
                        Items = new List<ChecklistItem>
                        {
                            new ChecklistItem { Id="8a", Requirement="Génération de la séquence", Description="Méthode utilisée (ex: table de nombres aléatoires)." },
                            new ChecklistItem { Id="8b", Requirement="Type de randomisation", Description="Détails (ex: bloc, stratification)." },
                            new ChecklistItem { Id="9", Requirement="Mécanisme de secret d'allocation", Description="Comment la séquence a été cachée jusqu'à l'assignation ?" },
                            new ChecklistItem { Id="10", Requirement="Mise en œuvre", Description="Qui a généré la séquence, qui a recruté ?" }
                        }
                    },
                    new ChecklistSection
                    {
                        Name = "Méthodes : Aveugle (Blinding)",
                        Items = new List<ChecklistItem>
                        {
                            new ChecklistItem { Id="11a", Requirement="Procédure d'aveugle", Description="Qui était en aveugle (participants, soignants, évaluateurs) ?" },
                            new ChecklistItem { Id="11b", Requirement="Similitude des interventions", Description="Si applicable, description de la similitude." }
                        }
                    },
                    new ChecklistSection
                    {
                        Name = "Méthodes : Analyses Statistiques",
                        Items = new List<ChecklistItem>
                        {
                            new ChecklistItem { Id="12a", Requirement="Méthodes statistiques", Description="Méthodes pour comparer les groupes sur les critères primaires." },
                            new ChecklistItem { Id="12b", Requirement="Analyses additionnelles", Description="Analyses de sous-groupes, ajustements." }
                        }
                    }
                }
            };
        }
        private QualityChecklist GetGRAMMS()
        {
            return new QualityChecklist
            {
                Name = "GRAMMS",
                Description = "Good Reporting of A Mixed Methods Study (6 items)",
                Sections = new List<ChecklistSection>
                {
                    new ChecklistSection
                    {
                        Name = "Design Mixte",
                        Items = new List<ChecklistItem>
                        {
                            new ChecklistItem { Id="1", Requirement="Justification du design mixte", Description="Pourquoi utiliser à la fois du quanti et du quali ?" },
                            new ChecklistItem { Id="2", Requirement="Description du design", Description="Séquentiel, concomitant ? Quelle priorité ?" },
                            new ChecklistItem { Id="3", Requirement="Méthodes détaillées", Description="Échantillonnage et analyse pour chaque méthode." }
                        }
                    },
                    new ChecklistSection
                    {
                        Name = "Intégration",
                        Items = new List<ChecklistItem>
                        {
                            new ChecklistItem { Id="4", Requirement="Interface d'intégration", Description="Où et comment les données se rejoignent-elles ?" },
                            new ChecklistItem { Id="5", Requirement="Complémentarité", Description="Comment une méthode répond aux limites de l'autre ?" },
                            new ChecklistItem { Id="6", Requirement="Plus-value", Description="Ce que le mélange a apporté par rapport à une seule méthode." }
                        }
                    }
                }
            };
        }

        private QualityChecklist GetHISTORIC()
        {
            return new QualityChecklist
            {
                Name = "Critéres de Recherche Historique",
                Description = "Critique de source et heuristique",
                Sections = new List<ChecklistSection>
                {
                    new ChecklistSection
                    {
                        Name = "Heuristique (Recherche des sources)",
                        Items = new List<ChecklistItem>
                        {
                            new ChecklistItem { Id="H1", Requirement="Diversité des sources", Description="Archives publiques, privées, orales, iconographiques." },
                            new ChecklistItem { Id="H2", Requirement="Authenticité", Description="Vérification du support et de la provenance." }
                        }
                    },
                    new ChecklistSection
                    {
                        Name = "Critique Externe & Interne",
                        Items = new List<ChecklistItem>
                        {
                            new ChecklistItem { Id="H3", Requirement="Critique d'origine", Description="Qui est l'auteur ? Quand ? Où ?" },
                            new ChecklistItem { Id="H4", Requirement="Critique de restitution", Description="Le texte est-il original ou altéré ?" },
                            new ChecklistItem { Id="H5", Requirement="Critique de crédibilité", Description="L'auteur était-il témoin ? Avait-il un intérêt à mentir ?" }
                        }
                    }
                }
            };
        }

        private QualityChecklist GetSPATIAL()
        {
            return new QualityChecklist
            {
                Name = "Qualité de Recherche Spatiale (SIG/Physique)",
                Description = "Précision spatiale et rigueur des données",
                Sections = new List<ChecklistSection>
                {
                    new ChecklistSection
                    {
                        Name = "Données & Géoréférencement",
                        Items = new List<ChecklistItem>
                        {
                            new ChecklistItem { Id="S1", Requirement="Système de coordonnées (CRS)", Description="Spécification du système de projection utilisé." },
                            new ChecklistItem { Id="S2", Requirement="Précision spatiale", Description="Résolution des données (mètres, km, degrés)." }
                        }
                    },
                    new ChecklistSection
                    {
                        Name = "Modélisation",
                        Items = new List<ChecklistItem>
                        {
                            new ChecklistItem { Id="S3", Requirement="Topologie", Description="Vérification des erreurs de superposition/continuité." },
                            new ChecklistItem { Id="S4", Requirement="Validation terrain (Ground Truthing)", Description="Concordance entre modèle et réalité." }
                        }
                    }
                }
            };
        }

        private QualityChecklist GetSOCIOLOGY()
        {
            return new QualityChecklist
            {
                Name = "Rigueur en Sciences Sociales",
                Description = "Engagement et éthique de terrain",
                Sections = new List<ChecklistSection>
                {
                    new ChecklistSection
                    {
                        Name = "Postures de recherche",
                        Items = new List<ChecklistItem>
                        {
                            new ChecklistItem { Id="SS1", Requirement="Réflexivité", Description="Analyse de l'influence du chercheur sur le terrain." },
                            new ChecklistItem { Id="SS2", Requirement="Engagement prolongé", Description="Temps passé sur le terrain pour la saturation." }
                        }
                    },
                    new ChecklistSection
                    {
                        Name = "Éthique sociale",
                        Items = new List<ChecklistItem>
                        {
                            new ChecklistItem { Id="SS3", Requirement="Anonymisation", Description="Protection stricte de l'identité des enquêtés." },
                            new ChecklistItem { Id="SS4", Requirement="Restitution", Description="Partage des résultats avec la communauté étudiée." }
                        }
                    }
                }
            };
        }

        public void EvaluateProtocol(QualityChecklist checklist, ResearchProtocol protocol)
        {
            if (checklist == null || protocol == null) return;

            foreach (var section in checklist.Sections)
            {
                foreach (var item in section.Items)
                {
                    if (checklist.Name.Contains("COREQ"))
                        EvaluateCOREQ(item, protocol);
                    else if (checklist.Name.Contains("STROBE"))
                        EvaluateSTROBE(item, protocol);
                    else if (checklist.Name.Contains("CONSORT"))
                        EvaluateCONSORT(item, protocol);
                    else if (checklist.Name.Contains("SRQR"))
                        EvaluateSRQR(item, protocol);
                    else if (checklist.Name.Contains("GRAMMS"))
                        EvaluateGRAMMS(item, protocol);
                    else if (checklist.Name.Contains("PRISMA"))
                        EvaluatePRISMA(item, protocol);
                    else if (checklist.Name.Contains("CARE"))
                        EvaluateCARE(item, protocol);
                    else if (checklist.Name.Contains("Historique"))
                        EvaluateHISTORIC(item, protocol);
                    else if (checklist.Name.Contains("Spatiale"))
                        EvaluateSPATIAL(item, protocol);
                    else if (checklist.Name.Contains("Sociales"))
                        EvaluateSOCIOLOGY(item, protocol);
                }
            }
        }
        
        private void EvaluateHISTORIC(ChecklistItem item, ResearchProtocol protocol)
        {
            switch (item.Id)
            {
                case "H1": item.IsMet = !string.IsNullOrWhiteSpace(protocol.DataCollection); break;
                case "H2": item.IsMet = !string.IsNullOrWhiteSpace(protocol.StudySetting); break;
            }
        }

        private void EvaluateSPATIAL(ChecklistItem item, ResearchProtocol protocol)
        {
            switch (item.Id)
            {
                case "S1": item.IsMet = protocol.StudySetting?.ToLower().Contains("coordonnées") == true || protocol.StudySetting?.ToLower().Contains("projection") == true; break;
                case "S2": item.IsMet = !string.IsNullOrWhiteSpace(protocol.DataAnalysis); break;
            }
        }

        private void EvaluateSOCIOLOGY(ChecklistItem item, ResearchProtocol protocol)
        {
            switch (item.Id)
            {
                case "SS1": item.IsMet = !string.IsNullOrWhiteSpace(protocol.ConceptualModel); break;
                case "SS3": item.IsMet = protocol.Ethics?.ToLower().Contains("anonym") == true; break;
            }
        }

        private void EvaluateCOREQ(ChecklistItem item, ResearchProtocol protocol)
        {
            switch (item.Id)
            {
                // Domain 1: Research team
                // Items 1-8 relate to dynamic Author details or extra fields not yet in Protocol. 
                // We'll skip or set to false/check specific fields if available.

                // Domain 2: Study design
                case "9": // Orientation méthodologique
                    item.IsMet = protocol.QualitativeApproach != QualitativeApproach.Inductive; 
                    // Note: Inductive is default, so hard to know if user consciously chose it, but it's "filled".
                    // Let's assume Valid as it's a required Enum.
                    item.IsMet = true; 
                    break;
                case "10": // Échantillonnage
                    item.IsMet = !string.IsNullOrWhiteSpace(protocol.SamplingMethod);
                    break;
                case "14": // Cadre de collecte
                    item.IsMet = !string.IsNullOrWhiteSpace(protocol.StudySetting);
                    break;
                case "17": // Guide d'entretien => DataCollection
<<<<<<< HEAD
                    item.IsMet = protocol.Appendices.Any(a => a.Type == AppendixType.InterviewGuide) || !string.IsNullOrWhiteSpace(protocol.DataCollection);
                    break;
                case "32": // Consentement (Generic check if we used COREQ sections)
                    item.IsMet = protocol.Appendices.Any(a => a.Type == AppendixType.InformedConsent);
=======
                    item.IsMet = !string.IsNullOrWhiteSpace(protocol.DataCollection);
>>>>>>> origin/main
                    break;
            }
        }

        private void EvaluateSTROBE(ChecklistItem item, ResearchProtocol protocol)
        {
            switch (item.Id)
            {
                case "1": // Contexte et Justification
                    item.IsMet = !string.IsNullOrWhiteSpace(protocol.Context) && !string.IsNullOrWhiteSpace(protocol.ProblemJustification);
                    break;
                case "2": // Objectifs
                    item.IsMet = !string.IsNullOrWhiteSpace(protocol.GeneralObjective);
                    break;
                case "3": // Type d'étude
                    item.IsMet = true; // Enum always set
                    break;
                case "4": // Cadre
                    item.IsMet = !string.IsNullOrWhiteSpace(protocol.StudySetting);
                    break;
                case "5": // Participants
                    item.IsMet = !string.IsNullOrWhiteSpace(protocol.InclusionCriteria) && !string.IsNullOrWhiteSpace(protocol.ExclusionCriteria);
                    break;
                case "6": // Variables
                    item.IsMet = protocol.Variables != null && protocol.Variables.Count > 0;
                    break;
<<<<<<< HEAD
                case "7": // Sources de données
                    item.IsMet = protocol.Appendices.Any(a => a.Type == AppendixType.Questionnaire || a.Type == AppendixType.DataCollectionForm);
                    break;
=======
>>>>>>> origin/main
                case "9": // Taille d'étude
                    item.IsMet = !string.IsNullOrWhiteSpace(protocol.SamplingMethod);
                    break;
            }
        }

        private void EvaluateCONSORT(ChecklistItem item, ResearchProtocol protocol)
        {
            switch (item.Id)
            {
                case "1a": // Titre
                    item.IsMet = protocol.Title?.ToLower().Contains("random") == true;
                    break;
                case "2a": // Contexte
                    item.IsMet = !string.IsNullOrWhiteSpace(protocol.Context);
                    break;
                case "2b": // Objectifs
                    item.IsMet = !string.IsNullOrWhiteSpace(protocol.GeneralObjective);
                    break;
                case "4a": // Éligibilité
                    item.IsMet = !string.IsNullOrWhiteSpace(protocol.InclusionCriteria);
                    break;
                case "4b": // Cadre
                    item.IsMet = !string.IsNullOrWhiteSpace(protocol.StudySetting);
                    break;
                case "7a": // Taille
                    item.IsMet = !string.IsNullOrWhiteSpace(protocol.SamplingMethod);
                    break;
                case "12a": // Stats
                    item.IsMet = !string.IsNullOrWhiteSpace(protocol.DataAnalysis);
                    break;
            }
        }
        private void EvaluateSRQR(ChecklistItem item, ResearchProtocol protocol)
        {
             switch (item.Id)
            {
                case "1": item.IsMet = !string.IsNullOrWhiteSpace(protocol.Title); break;
                case "3": item.IsMet = !string.IsNullOrWhiteSpace(protocol.ProblemJustification); break;
                case "4": item.IsMet = !string.IsNullOrWhiteSpace(protocol.ResearchQuestion); break;
                case "8": item.IsMet = !string.IsNullOrWhiteSpace(protocol.SamplingMethod); break;
                case "11": item.IsMet = !string.IsNullOrWhiteSpace(protocol.DataAnalysis); break;
                case "13": item.IsMet = !string.IsNullOrWhiteSpace(protocol.Ethics); break;
            }
        }

        private void EvaluateGRAMMS(ChecklistItem item, ResearchProtocol protocol)
        {
            switch (item.Id)
            {
                case "1": item.IsMet = !string.IsNullOrWhiteSpace(protocol.ProblemJustification); break;
                case "3": item.IsMet = !string.IsNullOrWhiteSpace(protocol.SamplingMethod) && !string.IsNullOrWhiteSpace(protocol.DataAnalysis); break;
            }
        }

        private void EvaluatePRISMA(ChecklistItem item, ResearchProtocol protocol)
        {
            switch (item.Id)
            {
                case "1": item.IsMet = protocol.Title?.ToLower().Contains("systematic") == true || protocol.Title?.ToLower().Contains("revue") == true; break;
                case "5": item.IsMet = !string.IsNullOrWhiteSpace(protocol.InclusionCriteria); break;
                case "6": item.IsMet = !string.IsNullOrWhiteSpace(protocol.DataCollection); break;
<<<<<<< HEAD
                case "7": item.IsMet = protocol.Appendices.Any(a => a.Type == AppendixType.SearchStrategy); break;
=======
>>>>>>> origin/main
            }
        }

        private void EvaluateCARE(ChecklistItem item, ResearchProtocol protocol)
        {
            switch (item.Id)
            {
                case "1": item.IsMet = protocol.Title?.ToLower().Contains("case") == true || protocol.Title?.ToLower().Contains("cas") == true; break;
                case "5a": item.IsMet = !string.IsNullOrWhiteSpace(protocol.StudyPopulation); break;
                case "8": item.IsMet = !string.IsNullOrWhiteSpace(protocol.DataCollection); break;
            }
        }
    }
}
