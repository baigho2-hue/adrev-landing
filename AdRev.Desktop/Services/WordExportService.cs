using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using AdRev.Domain.Protocols;
using AdRev.Domain.Models;
using AdRev.Domain.Variables;
using AdRev.Domain.Enums;

namespace AdRev.Desktop.Services
{
    public class WordExportService
    {
        private const string MainFont = "Times New Roman";
        private const string FontSize = "24"; // 12pt * 2
        private const string LineSpacing = "360"; // 1.5 * 240

        public void ExportProtocolToWord(ResearchProtocol protocol, string filePath)
        {
            CreateWordDocument(filePath, (body) =>
            {
                AddMainTitle(body, "PROTOCOLE DE RECHERCHE");
                AddParagraph(body, $"Titre : {protocol.Title}", true);
                
                if (protocol.PrincipalAuthor != null)
                {
                    AddParagraph(body, $"Auteur Principal : {protocol.PrincipalAuthor.FirstName} {protocol.PrincipalAuthor.LastName}");
                    AddParagraph(body, $"Institution : {protocol.PrincipalAuthor.Institution}");
                }
                AddParagraph(body, "--------------------------------------------------");

                AddHeading(body, "1. INTRODUCTION ET PROBLÉMATIQUE");
                AddParagraph(body, protocol.Context);
                AddParagraph(body, protocol.ProblemJustification);

                AddHeading(body, "2. OBJECTIFS");
                AddParagraph(body, $"Objectif Général : {protocol.GeneralObjective}", true);
                AddParagraph(body, "Objectifs Spécifiques :");
                AddParagraph(body, protocol.SpecificObjectives);

                AddHeading(body, "3. MÉTHODOLOGIE");
                AddParagraph(body, $"Type d'étude : {protocol.StudyType}");
                AddParagraph(body, $"Cadre : {protocol.StudySetting}");
                AddParagraph(body, $"Population : {protocol.StudyPopulation}");
                AddParagraph(body, protocol.InclusionCriteria);
                AddParagraph(body, protocol.ExclusionCriteria);
                AddParagraph(body, protocol.SamplingMethod);
                AddParagraph(body, protocol.DataCollection);
                AddParagraph(body, protocol.DataAnalysis);
                AddParagraph(body, protocol.Ethics);

                AddHeading(body, "4. DISCUSSION ET CONCLUSION");
                AddParagraph(body, protocol.StudyLimitations);
                AddParagraph(body, protocol.Conclusion);

                if (!string.IsNullOrWhiteSpace(protocol.References))
                {
                    AddPageBreak(body);
                    AddHeading(body, "RÉFÉRENCES BIBLIOGRAPHIQUES");
                    AddParagraph(body, protocol.References);
                }
            });
        }

        public void ExportGlobalReport(ResearchProject project, string filePath)
        {
            CreateWordDocument(filePath, (body) =>
            {
                // TITLE PAGE
                string titlePrefix = project.AcademicLevel switch
                {
                    AcademicLevel.Thesis => "THÈSE DE DOCTORAT",
                    AcademicLevel.MasterMemoir => "MÉMOIRE DE MASTER",
                    AcademicLevel.ScientificArticle => "MANUSCRIT D'ARTICLE",
                    AcademicLevel.NGOReport => "RAPPORT D'IMPACT / HUMANITAIRE",
                    _ => "RAPPORT D'ÉTUDE"
                };
                
                if (!string.IsNullOrEmpty(project.TargetJournalName) && project.AcademicLevel == AcademicLevel.ScientificArticle)
                {
                    titlePrefix += $"\n(Cible : {project.TargetJournalName})";
                }

                AddMainTitle(body, titlePrefix);
                AddParagraph(body, $"Titre : {project.Title}", true);
                AddParagraph(body, $"Auteur(s) : {project.Authors}");
                AddParagraph(body, $"Institution : {project.Institution}");
                AddParagraph(body, "--------------------------------------------------");

                // SECTION 0: EXECUTIVE SUMMARY (NGO ONLY)
                if (project.AcademicLevel == AcademicLevel.NGOReport)
                {
                    AddHeading(body, "RÉSUMÉ EXÉCUTIF");
                    AddParagraph(body, "Synthèse des points clés et de l'impact (à compléter).", true);
                    AddParagraph(body, project.ConclusionContent); // Use conclusion as base for summary
                    AddPageBreak(body);
                }

                // SECTION 1: PROTOCOLE (SYNTHÈSE) -> INTRODUCTION & METHODOLOGIE IMRAD
                AddHeading(body, "I. CONTEXTE ET MÉTHODOLOGIE (IMRAD)");
                AddParagraph(body, "Introduction (Problématique & Justification) :", true);
                AddParagraph(body, project.ProblemJustification ?? "Voir section Protocole"); 
                AddParagraph(body, $"Objectif Général : {project.GeneralObjective}");
                
                AddParagraph(body, "Méthodologie détaillée :", true);
                AddParagraph(body, $"Type d'étude : {project.StudyType}");
                AddParagraph(body, project.DataAnalysisPlan);

                // SECTION 2: RÉSULTATS
                AddHeading(body, "II. RÉSULTATS");
                AddParagraph(body, "Présentation des données (Tableaux et Figures annuaels).");

                // SECTION 3: DISCUSSION
                AddHeading(body, "III. DISCUSSION");
                AddParagraph(body, project.DiscussionContent);
                AddParagraph(body, "Limites méthodologiques :", true);
                AddParagraph(body, project.LimitationsContent);

                // SECTION 5: CONCLUSION & RECOMMANDATIONS
                AddHeading(body, "V. CONCLUSION ET RECOMMANDATIONS");
                if (project.AcademicLevel == AcademicLevel.NGOReport)
                {
                     AddParagraph(body, "RECOMMANDATIONS OPÉRATIONNELLES", true);
                }
                AddParagraph(body, project.ConclusionContent);

                AddPageBreak(body);

                // SECTION 6: RÉFÉRENCES
                AddHeading(body, "VI. RÉFÉRENCES BIBLIOGRAPHIQUES");
                if (project.References != null && project.References.Any())
                {
                    foreach (var r in project.References.OrderBy(x => x.FormattedString))
                    {
                        AddParagraph(body, $"• {r.FormattedString}");
                    }
                }
                else
                {
                    AddParagraph(body, "Aucune référence citée.");
                }
            });
        }

        public void ExportVariableSheetToWord(ResearchProtocol protocol, string filePath)
        {
            CreateWordDocument(filePath, (body) =>
            {
                AddMainTitle(body, "FICHE DE CONCEPTION DE VARIABLE / CAHIER D'OBSERVATION");
                AddParagraph(body, $"Projet : {protocol.Title}", true);
                AddParagraph(body, "--------------------------------------------------");

                if (protocol.Variables == null || !protocol.Variables.Any())
                {
                    AddParagraph(body, "Aucune variable définie.");
                    return;
                }

                var groups = protocol.Variables.GroupBy(v => string.IsNullOrWhiteSpace(v.GroupName) ? "GÉNÉRAL" : v.GroupName);
                foreach (var group in groups)
                {
                    AddHeading(body, $"SECTION : {group.Key.ToUpper()}");
                    foreach (var v in group)
                    {
                        string req = v.IsRequired ? " (*)" : "";
                        AddParagraph(body, $"{v.Prompt}{req}", true);
                        AddParagraph(body, $"   Type : {v.Type} | Nom : {v.Name}");
                        if (!string.IsNullOrEmpty(v.ChoiceOptions))
                            AddParagraph(body, $"   Options : {v.ChoiceOptions}");
                        AddParagraph(body, ""); // Spacer
                    }
                }
            });
        }

        private void CreateWordDocument(string filePath, Action<Body> populateAction)
        {
            using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document))
            {
                MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new Document();
                Body body = mainPart.Document.AppendChild(new Body());

                populateAction(body);

                mainPart.Document.Save();
            }
        }

        private void AddMainTitle(Body body, string text)
        {
            Paragraph para = body.AppendChild(new Paragraph());
            ParagraphProperties pp = para.AppendChild(new ParagraphProperties(
                new Justification() { Val = JustificationValues.Center },
                new SpacingBetweenLines() { After = "480", Line = "360", LineRule = LineSpacingRuleValues.Auto }
                ));

            Run run = para.AppendChild(new Run());
            RunProperties rp = run.AppendChild(new RunProperties(
                new RunFonts() { Ascii = MainFont, HighAnsi = MainFont },
                new FontSize() { Val = "32" }, // 16pt
                new Bold()
                ));
            run.AppendChild(new Text(text ?? ""));
        }

        private void AddHeading(Body body, string text)
        {
            Paragraph para = body.AppendChild(new Paragraph());
            ParagraphProperties pp = para.AppendChild(new ParagraphProperties(
                new SpacingBetweenLines() { Before = "240", After = "120", Line = LineSpacing, LineRule = LineSpacingRuleValues.Auto }
                ));

            Run run = para.AppendChild(new Run());
            RunProperties rp = run.AppendChild(new RunProperties(
                new RunFonts() { Ascii = MainFont, HighAnsi = MainFont },
                new FontSize() { Val = "28" }, // 14pt
                new Bold()
                ));
            run.AppendChild(new Text(text ?? ""));
        }

        private void AddParagraph(Body body, string? text, bool bold = false)
        {
            if (string.IsNullOrEmpty(text)) return;

            string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            foreach (var line in lines)
            {
                Paragraph para = body.AppendChild(new Paragraph());
                ParagraphProperties pp = para.AppendChild(new ParagraphProperties(
                    new SpacingBetweenLines() { Line = LineSpacing, LineRule = LineSpacingRuleValues.Auto },
                    new Justification() { Val = JustificationValues.Both }
                    ));

                Run run = para.AppendChild(new Run());
                RunProperties rp = run.AppendChild(new RunProperties(
                    new RunFonts() { Ascii = MainFont, HighAnsi = MainFont },
                    new FontSize() { Val = FontSize }
                    ));

                if (bold) rp.AppendChild(new Bold());
                
                run.AppendChild(new Text(line));
            }
        }

        private void AddPageBreak(Body body)
        {
            body.AppendChild(new Paragraph(new Run(new Break() { Type = BreakValues.Page })));
        }
    }
}
