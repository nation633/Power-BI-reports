# Data Modelling Documentation: Relationship Centre Banker Support Tracker

## Overview
This document details the changes applied to the SSAS Tabular Model to support the **Relationship Centre Banker Support Tracker** report. The primary goal was to create a dedicated Perspective for this report and ensure the underlying data relationships were established to support the analysis of "Branch Caller" support requests without bloating the model size.

## Actions Taken

### 1. Perspective Creation
*   **Name:** `Relationship Centre Banker Support Tracker`
*   **Purpose:** To provide a focused subset of the model specifically for this report, reducing clutter for report authors and end-users.
*   **Logic:** The script checks if a perspective with this name exists. If not, it creates it.

### 2. Relationship Management (Star Schema Implementation)
To maintain model efficiency and avoid importing redundant text columns (like Region, Area, Title) into the Fact tables, we utilized a Star Schema approach by linking Fact tables directly to the `DIM_EMPLOYEE` dimension.

*   **Relationship 1: Cases to Employee**
    *   **From Table:** `Cases_History_tbl`
    *   **From Column:** `BranchCallerEmployeeNumber` (Matches `NB` + Staff Number format)
    *   **To Table:** `DIM_EMPLOYEE`
    *   **To Column:** `EMPLOYEE_ID`
    *   **Behavior:** Active, One-Directional (Dimension filters Fact).

*   **Relationship 2: Activities to Employee**
    *   **From Table:** `CRM_Activities`
    *   **From Column:** `BranchEmployeeNumber` (Matches `NB` + Staff Number format)
    *   **To Table:** `DIM_EMPLOYEE`
    *   **To Column:** `EMPLOYEE_ID`
    *   **Behavior:** Active, One-Directional (Dimension filters Fact).

**Note:** The script was designed to *check* for these relationships first. It only creates them if they do not already exist, preventing duplication or errors.

### 3. Object Inclusion
The following tables and all their child objects (Columns, Measures, Hierarchies) were added to the new Perspective:
*   `Cases_History_tbl` (Fact)
*   `CRM_Activities` (Fact)
*   `DIM_EMPLOYEE` (Dimension - providing Staff details, Location, and Hierarchy)
*   `DIM_DATE` (Dimension - providing Date context)

## Outcome
1.  **Model Optimization:** By relying on relationships rather than flattening the data (joining in SQL), we saved significant storage space and processing time. Columnar storage databases (like SSAS Tabular) perform best with narrow fact tables and relationships to dimensions.
2.  **Report Usability:** Users connecting to the "Relationship Centre Banker Support Tracker" perspective will see only the relevant tables.
3.  **Analytical Capability:** Users can now drag fields like `Region`, `Area`, or `Position` from `DIM_EMPLOYEE` and analyze `Case Count` or `Activity Count` from the Fact tables seamlessly.
4.  **Automation:** The provided C# script automates this configuration in Tabular Editor, ensuring consistency and repeatability.
