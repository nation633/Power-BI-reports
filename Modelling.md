# Data Modelling Documentation: Relationship Centre Banker Support Tracker

## Overview
This document details the configuration applied to the SSAS Tabular Model to support the **Relationship Centre Banker Support Tracker**.

The solution creates a dedicated Perspective and implements a **Star Schema** design. This effectively **joins** the two fact tables (`Cases` and `Activities`) by linking them to common dimensions (`Employee` and `Date`), creating a **combined view** without physically duplicating data. This approach ensures the model remains **lightweight, efficient, and fast**.

## Changes Implemented

### 1. Perspective
*   **Name:** `Relationship Centre Banker Support Tracker`
*   **Content:** Contains only the relevant tables to replicate the original report's scope:
    *   `Cases_History_tbl` (Fact / Dimension)
    *   `CRM_Activities` (Fact)
    *   `DIM_EMPLOYEE` (Dimension)
    *   `DIM_DATE` (Dimension)

### 2. Relationship Strategy (The "Combined View")
To allow you to see Cases and Activities side-by-side (e.g., "Show me Cases and Activities for Region X"), we established active relationships to shared dimensions. This provides the functionality of a "Join" but is much faster and cleaner.

*   **Employee Context (Joined by Staff Number):**
    *   `Cases_History_tbl[BranchCallerEmployeeNumber]` $\rightarrow$ `DIM_EMPLOYEE[EMPLOYEE_ID]`
    *   `CRM_Activities[BranchEmployeeNumber]` $\rightarrow$ `DIM_EMPLOYEE[EMPLOYEE_ID]`
    *   *Result:* The **Matching Columns** from your SQL (Area, Region, Title, Position) are now available in `DIM_EMPLOYEE`. Dragging any of these columns into a report will filter **both** Cases and Activities tables instantly.

*   **Date Context (Joined by Date):**
    *   `Cases_History_tbl[Date_key]` $\rightarrow$ `DIM_DATE[DATE_KEY]`
    *   `CRM_Activities[Date_key]` $\rightarrow$ `DIM_DATE[DATE_KEY]`
    *   *Result:* Enables time-series analysis (e.g., "Last Month") across both datasets simultaneously.

*   **Case Context (Joined by Case Number):**
    *   `CRM_Activities[CaseNumber]` $\rightarrow$ `Cases_History_tbl[CaseNumber]`
    *   *Result:* Allows you to slice Activities by Case attributes (e.g., Portfolio, Product) just like in the SQL joins. `Cases` effectively acts as a lookup table for `Activities`.

## Verification
When you connect to this Perspective in Power BI:
1.  Select **Region** or **Area** from **DIM_EMPLOYEE**.
2.  Select **Portfolio** from **Cases_History_tbl**.
3.  Select measures or counts from the Fact tables.
4.  The report will automatically display the combined counts, sliced by both Employee location and Case portfolio, maintaining the exact behavior of your SQL query joins.
