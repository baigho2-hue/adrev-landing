# AdRev 2.0 - Project Context Pack (NotebookLM Ready)

## Project Overview
**AdRev** is a specialized medical research methodology application built with WPF (.NET 8). It assists researchers in designing protocols, calculating sample sizes with advanced statistical adjustments, and managing qualitative/quantitative variables.

## Key Components

### 1. Research Protocol (`AdRev.Domain.Protocols.ResearchProtocol`)
The central entity. It contains:
- **Project Info**: Title, Authors (Principal + Co-authors).
- **Introduction**: Context, Problem Justification, Research Question, Hypotheses.
- **Objectives**: General and Specific.
- **Methodology**: Study Type, Setting, Conceptual Model.
- **Advanced Sampling**: 
    - Supports **13 types** (Stratified, Cluster, etc.).
    - Automatic adjustments for **Design Effect (Deff)** and **Expected Loss Rate**.
    - Handles multicentric study sites.
- **Variables**: Dynamic list of study variables (style Epi Info).
- **Ethics & Budget**: Dedicated sections for compliance and planning.

### 2. Statistical Core (`AdRev.Core.Services`)
Contains the logic for:
- **Sample Size Calculations**: Cochran, Comparison of Proportions.
- **Adjustments**: Mathematical formulas to scale sample size based on stratification or clustering.
- **Word Export**: Generates professional reports from the protocol data.

### 3. UI Layer (`AdRev.Desktop`)
- **MainWindow**: Dashboard for project management.
- **ProtocolWindow**: Multi-step wizard (Introduction, Objectives, Methodology, Sampling, etc.).
- **VariableDesigner**: Visual drag-and-drop tool for creating data entry masks.

## Business Logic Highlights
- **Dynamic Methodology**: The UI adapts based on `StudyType`. For example, selecting "Qualitative" enables specific fields like "Qualitative Approach".
- **Real-time Feedback**: Calculations update as the user types parameters (prevalence, precision, error margin).

## Technical Stack
- **Languages**: C#, XAML.
- **Framework**: .NET 8, WPF.
- **Data**: JSON-based serialization for project files.
