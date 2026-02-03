# How to Build and Design the NEDBANK 4.1 Power BI Template

This guide provides step-by-step instructions on how to use the `NEDBANK_4.1_Template.json` theme file and how to build standard visuals that align with the team's reporting standards.

## 1. Importing the Theme

To ensure all visuals adhere to the corporate identity and standard formatting, you must first import the theme file.

1.  Open **Power BI Desktop**.
2.  Go to the **View** ribbon.
3.  Click the dropdown arrow in the **Themes** gallery.
4.  Select **Browse for themes**.
5.  Locate and select the `NEDBANK_4.1_Template.json` file from this repository.
6.  Once imported, the report canvas background, default colors, and visual styles will update automatically.
    *   **Note:** This theme is designed to aggressively override existing styles. If you have a report with manual formatting, applying this theme should reset most elements (charts, matrices, etc.) to the standard design. If some elements persist, you may need to select the visual and choose "Reset to default" in the format pane, though the theme handles most cases.

## 2. Visual specific configurations

The theme file automatically applies specific settings to certain visuals. Below explains what happens and how to configure your visuals to leverage these defaults.

### A. Matrix Visuals (Table Layout)

The theme forces Matrix visuals to adopt a "Tabular" layout style by default, removing the stepped layout often seen in Pivot Tables.

*   **Default Behavior:**
    *   **Layout:** Tabular (`Stepped Layout` is turned OFF).
    *   **Style Preset:** None (This ensures no default Power BI styles override our custom colors).
    *   **Headers:** Green background with White text (Centrally aligned).
    *   **Values:** White background with Black text. alternating row colors are removed to maintain a clean look.
    *   **Word Wrap:** Enabled for values.

*   **How to Build:**
    1.  Add a **Matrix** visual to your canvas.
    2.  Drag your Dimensions into **Rows** and **Columns**.
    3.  Drag your Measures into **Values**.
    4.  *Note:* You do not need to manually disable "Stepped Layout" or set the row headers—the theme handles this.

### B. Line and Clustered Column Charts

This visual has a specific color palette assigned to ensuring distinction between the Column series and the Line series.

*   **Color Palette Order:**
    1.  **Column 1:** `#CEDC00` (Light Green/Yellow)
    2.  **Column 2:** `#006342` (Dark Green)
    3.  **Column 3:** `#115740` (Deep Green)
    4.  **Line 1:** `#78BE20` (Bright Green)
    5.  **Line 2:** `#F2A900` (Orange/Gold)
    6.  **Line 3:** `#FA4616` (Red/Orange)

*   **How to Build:**
    1.  Select the **Line and Clustered Column Chart** visual.
    2.  Add fields to the **X-axis**.
    3.  Add up to 3 measures to **Column y-axis**. They will automatically take the first 3 colors (Greens).
    4.  Add up to 3 measures to **Line y-axis**. They will automatically take the next 3 colors (Bright Green, Orange, Red).

### C. Clustered Column Charts

Standard column charts are restricted to a specific corporate green.

*   **Default Behavior:**
    *   All bars will default to `#006633` (Corporate Green).

*   **How to Build:**
    1.  Select the **Clustered Column Chart**.
    2.  Add your Dimension to the **X-axis** and Measure to the **Y-axis**.
    3.  The bars will automatically be uniform green. If you add a Legend, they will currently stay this single color unless you manually override specific data points, as the theme restricts the palette for this visual type to ensure consistency.

## 3. General Design Principles

*   **Backgrounds:** The page background is White (`#FFFFFF`).
*   **Filter Pane:** The filter pane is styled with a light gray background (`#f9f9f9`) and specific text sizes to remain unobtrusive.
*   **Text Classes:** Standard text (Callouts, Titles, Labels) defaults to **Calibri** font.

---
*Maintained by the Reporting Team. Please do not modify `NEDBANK_4.1_Template.json` without approval.*
