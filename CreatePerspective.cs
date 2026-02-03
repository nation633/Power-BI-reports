using System.Linq;

// 1. Configuration
var perspectiveName = "Relationship Centre Banker Support Tracker";
var employeeTableTargetName = "DIM_EMPLOYEE";
var employeeIdColumn = "EMPLOYEE_ID";

// 2. Find Tables (Robust Matching)
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

// 5. Create Relationships

// A. Employee Dimension (Banker Location/Details)
EnsureRelationship(casesTable, "BranchCallerEmployeeNumber", employeeTable, employeeIdColumn);
EnsureRelationship(activitiesTable, "BranchEmployeeNumber", employeeTable, employeeIdColumn);

// B. Date Dimension (Time Intelligence)
EnsureRelationship(casesTable, "Date_key", dateTable, "DATE_KEY");
EnsureRelationship(activitiesTable, "Date_key", dateTable, "DATE_KEY");

// Note: A direct relationship between Activities and Cases cannot be created because Cases_History_tbl
// contains duplicate CaseNumbers (it is a transactional history table, not a unique dimension).
// Instead, we use a DAX measure with TREATAS to handle the filtering virtually.

// 6. Create Missing Columns for Filters (Slicers)

// "Calling Party" on Cases: 'Branch' if BranchCallerEmployeeNumber exists, else 'Client'
if (!casesTable.Columns.Any(c => c.Name.Equals("Calling Party", StringComparison.InvariantCultureIgnoreCase)))
{
    var col = casesTable.AddCalculatedColumn("Calling Party");
    col.Expression = "IF(ISBLANK('" + casesTable.Name + "'[BranchCallerEmployeeNumber]), \"Client\", \"Branch\")";
    Output("Created Calculated Column: 'Calling Party' in " + casesTable.Name);
}

// "Calling Party" on Activities: Same logic
if (!activitiesTable.Columns.Any(c => c.Name.Equals("Calling Party", StringComparison.InvariantCultureIgnoreCase)))
{
    var col = activitiesTable.AddCalculatedColumn("Calling Party");
    col.Expression = "IF(ISBLANK('" + activitiesTable.Name + "'[BranchEmployeeNumber]), \"Client\", \"Branch\")";
    Output("Created Calculated Column: 'Calling Party' in " + activitiesTable.Name);
}

// 7. Create Virtual Relationship Measure (Workaround for Duplicates)
var measureName = "Banker Support Activities";
// Remove old version if it exists to ensure expression is updated
var oldMeas = activitiesTable.Measures.FirstOrDefault(m => m.Name.Equals(measureName, StringComparison.InvariantCultureIgnoreCase));
if (oldMeas != null)
{
    oldMeas.Delete();
}

var meas = activitiesTable.AddMeasure(measureName);
meas.Expression = "CALCULATE(COUNTROWS('" + activitiesTable.Name + "'), TREATAS(VALUES('" + casesTable.Name + "'[CaseNumber]), '" + activitiesTable.Name + "'[CaseNumber]))";
meas.FormatString = "#,0";
Output("Created Virtual Relationship Measure: " + measureName);


// 8. Add Objects to Perspective
var tablesToInclude = new[] { casesTable, activitiesTable, employeeTable, dateTable };

foreach (var table in tablesToInclude)
{
    table.InPerspective[perspective] = true;
    foreach (var col in table.Columns) col.InPerspective[perspective] = true;
    foreach (var m in table.Measures) m.InPerspective[perspective] = true;
    foreach (var hier in table.Hierarchies) hier.InPerspective[perspective] = true;
}

Output("Script Complete. Perspective updated with Virtual Relationship Measure.");
