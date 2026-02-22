using AdRev.Domain.Enums;
using System.Collections.Generic;

namespace AdRev.Core.Resources
{
    public static class AppendixTemplates
    {
        public static string GetTemplate(AppendixType type)
        {
            return type switch
            {
                AppendixType.InformedConsent => 
                    "FORMULAIRE DE CONSENTEMENT ÉCLAIRÉ\n\n" +
                    "Projet : [Titre du Projet]\n" +
                    "Investigateur : [Nom]\n\n" +
                    "Je soussigné(e) [Nom du Participant] :\n" +
                    "- Confirme avoir lu et compris la notice d'information.\n" +
                    "- Avoir eu l'occasion de poser des questions.\n" +
                    "- Comprendre que ma participation est volontaire et que je peux me retirer à tout moment sans justification.\n" +
                    "- Accepter que mes données soient traitées de manière anonyme.\n\n" +
                    "Signature du participant : ____________________\n" +
                    "Date : ___/___/202_",

                AppendixType.InformationNotice =>
                    "NOTICE D'INFORMATION AUX PARTICIPANTS\n\n" +
                    "Introduction : Vous êtes invité à participer à une étude intitulée [Titre].\n" +
                    "Objectif : Cette étude vise à [Objectif].\n" +
                    "Procédure : Il vous sera demandé de [Description].\n" +
                    "Confidentialité : Vos données seront anonymisées et stockées de manière sécurisée.\n" +
                    "Droits : Vous disposez d'un droit d'accès et de rectification de vos données.\n" +
                    "Contact : Pour toute question, contactez [Email/Tel].",

                AppendixType.ConfidentialityAgreement =>
                    "ENGAGEMENT DE CONFIDENTIALITÉ\n\n" +
                    "Dans le cadre de ma collaboration au projet [Titre],\n" +
                    "Je, soussigné(e) [Nom], m'engage à :\n" +
                    "1. Garder strictement confidentielles toutes les informations collectées.\n" +
                    "2. Ne pas divulguer les identités des participants.\n" +
                    "3. Détruire les enregistrements audios après transcription.\n\n" +
                    "Fait à : __________, le ___/___/202_",

                AppendixType.InterviewGuide =>
                    "GUIDE D'ENTRETIEN SEMI-DIRECTIF\n\n" +
                    "Thème 1 : Introduction et parcours (5-10 min)\n" +
                    "- Pouvez-vous vous présenter ?\n" +
                    "- [Question de relance]\n\n" +
                    "Thème 2 : Expérience centrale (20-30 min)\n" +
                    "- Racontez-moi votre expérience avec [Sujet]...\n\n" +
                    "Thème 3 : Perspectives et clôture (5 min)\n" +
                    "- Qu'est-ce qui pourrait être amélioré ?\n" +
                    "- Avez-vous quelque chose à ajouter ?",

                AppendixType.SearchStrategy =>
                    "STRATÉGIE DE RECHERCHE (PRISMA Requirement)\n\n" +
                    "Base de données : PubMed / MEDLINE\n" +
                    "Date de recherche : [Date]\n\n" +
                    "Équation de recherche :\n" +
                    "(\"[Sujet A]\"[Mesh] OR \"[Sujet A]\"[Title/Abstract])\n" +
                    "AND (\"[Sujet B]\"[Mesh] OR \"[Sujet B]\"[Title/Abstract])\n" +
                    "AND (\"2015/01/01\"[Date - Publication] : \"3000\"[Date - Publication])\n\n" +
                    "Nombre de résultats : [N]",

                _ => "Description ou contenu de l'annexe à saisir ici..."
            };
        }
    }
}
