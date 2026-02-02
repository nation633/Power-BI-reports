using System.Linq;

// 1. Configuration
var perspectiveName = "Relationship Centre Banker Support Tracker";
var employeeTableTargetName = "DIM_EMPLOYEE"; // The Dimension to link to
var employeeIdColumn = "EMPLOYEE_ID"; // The key in DIM_EMPLOYEE

// 2. Find Tables (Robust Matching)
// We look for tables that *contain* the expected names to handle minor variations,
// but prioritize exact matches if available.
var casesTable = Model.Tables.FirstOrDefault(t => t.Name.Equals("Cases_History_tbl", StringComparison.InvariantCultureIgnoreCase))
                 ?? Model.Tables.FirstOrDefault(t => t.Name.Contains("Cases") && t.Name.Contains("History"));

var activitiesTable = Model.Tables.FirstOrDefault(t => t.Name.Equals("CRM_Activities", StringComparison.InvariantCultureIgnoreCase))
                      ?? Model.Tables.FirstOrDefault(t => t.Name.Contains("Activities") && t.Name.Contains("CRM"));

var employeeTable = Model.Tables.FirstOrDefault(t => t.Name.Equals(employeeTableTargetName, StringComparison.InvariantCultureIgnoreCase));

var dateTable = Model.Tables.FirstOrDefault(t => t.Name.Contains("DIM_DATE") || t.Name.Contains("DimDate"));

// Diagnostic Output
if (casesTable == null) Output("Error: Could not find 'Cases_History_tbl'.");
else Output("Found Cases Table: " + casesTable.Name);

if (activitiesTable == null) Output("Error: Could not find 'CRM_Activities'.");
else Output("Found Activities Table: " + activitiesTable.Name);

if (employeeTable == null) Output("Error: Could not find 'DIM_EMPLOYEE'.");
else Output("Found Employee Table: " + employeeTable.Name);

if (casesTable == null || activitiesTable == null || employeeTable == null || dateTable == null)
{
    Error("Aborting: Missing one or more required tables.");
    return;
}

// 3. Create Perspective
if (!Model.Perspectives.Any(p => p.Name == perspectiveName))
{
    Model.AddPerspective(perspectiveName);
}
var perspective = Model.Perspectives[perspectiveName];

// 4. Helper Delegate for Relationships
// Defined as Action to avoid "local function" compilation errors in some TE environments
Action<Table, string, Table, string> EnsureRelationship = (fromTable, fromColName, toTable, toColName) =>
{
    var fromCol = fromTable.Columns.FirstOrDefault(c => c.Name.Equals(fromColName, StringComparison.InvariantCultureIgnoreCase));
    var toCol = toTable.Columns.FirstOrDefault(c => c.Name.Equals(toColName, StringComparison.InvariantCultureIgnoreCase));

    if (fromCol == null)
    {
        Output("Warning: Column '" + fromColName + "' not found in table '" + fromTable.Name + "'. Relationship skipped.");
        return;
    }
    if (toCol == null)
    {
        Output("Warning: Column '" + toColName + "' not found in table '" + toTable.Name + "'. Relationship skipped.");
        return;
    }

    if (!Model.Relationships.Any(r => r.FromColumn == fromCol && r.ToColumn == toCol))
    {
        var rel = Model.AddRelationship();
        rel.FromColumn = fromCol;
        rel.ToColumn = toCol;
        rel.IsActive = true;
        rel.CrossFilteringBehavior = CrossFilteringBehavior.OneDirection;
        Output("Created Relationship: " + fromTable.Name + "[" + fromColName + "] -> " + toTable.Name + "[" + toColName + "]");
    }
    else
    {
        Output("Relationship already exists: " + fromTable.Name + " -> " + toTable.Name);
    }
};

// 5. Create Relationships (Joining the Facts to Common Dimensions)
// Cases -> Employee
EnsureRelationship(casesTable, "BranchCallerEmployeeNumber", employeeTable, employeeIdColumn);
// Cases -> Date
EnsureRelationship(casesTable, "Date_key", dateTable, "DATE_KEY");

// Activities -> Employee
EnsureRelationship(activitiesTable, "BranchEmployeeNumber", employeeTable, employeeIdColumn);
// Activities -> Date
EnsureRelationship(activitiesTable, "Date_key", dateTable, "DATE_KEY");


// 6. Create Integration Measures
// These measures replicate the SQL logic (filtering) inside the model
// so users can just drag-and-drop without writing complex DAX or SQL.

// Measure: Banker Support Cases (Filtered for 'Service Request')
if (!casesTable.Measures.Any(m => m.Name == "Banker Support Cases"))
{
    var m = casesTable.AddMeasure("Banker Support Cases");
    m.Expression = "CALCULATE(COUNTROWS('" + casesTable.Name + "'), '" + casesTable.Name + "'[CaseType] = \"Service Request\")";
    m.FormatString = "#,0";
    m.Description = "Count of cases where CaseType is 'Service Request', replicating the banker support report logic.";
}

// Measure: Banker Support Activities
if (!activitiesTable.Measures.Any(m => m.Name == "Banker Support Activities"))
{
    var m = activitiesTable.AddMeasure("Banker Support Activities");
    m.Expression = "COUNTROWS('" + activitiesTable.Name + "')"; // Add filters here if SQL had specific activity types
    m.FormatString = "#,0";
}

// 7. Add Objects to Perspective
var tablesToInclude = new[] { casesTable, activitiesTable, employeeTable, dateTable };

foreach (var table in tablesToInclude)
{
    table.InPerspective[perspective] = true;
    foreach (var col in table.Columns) col.InPerspective[perspective] = true;
    foreach (var meas in table.Measures) meas.InPerspective[perspective] = true;
    foreach (var hier in table.Hierarchies) hier.InPerspective[perspective] = true;
}

Output("Script Complete. Perspective '" + perspectiveName + "' updated.");
