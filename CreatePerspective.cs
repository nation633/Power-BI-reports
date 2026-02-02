using System.Linq;

// Name of the perspective to create
var perspectiveName = "Relationship Centre Banker Support Tracker";

// 1. Create the Perspective if it does not exist
if (!Model.Perspectives.Any(p => p.Name == perspectiveName))
{
    Model.AddPerspective(perspectiveName);
}

var perspective = Model.Perspectives[perspectiveName];

// 2. Define Tables to be included
var casesTable = Model.Tables.FirstOrDefault(t => t.Name == "Cases_History_tbl");
var activitiesTable = Model.Tables.FirstOrDefault(t => t.Name == "CRM_Activities");
var employeeTable = Model.Tables.FirstOrDefault(t => t.Name == "DIM_EMPLOYEE");
var dateTable = Model.Tables.FirstOrDefault(t => t.Name == "DIM_DATE");

if (casesTable == null || activitiesTable == null || employeeTable == null || dateTable == null)
{
    Error("One or more required tables (Cases_History_tbl, CRM_Activities, DIM_EMPLOYEE, DIM_DATE) could not be found in the model.");
    return;
}

// 3. Helper function to safely create relationships
void EnsureRelationship(Table fromTable, string fromColName, Table toTable, string toColName)
{
    var fromCol = fromTable.Columns.FirstOrDefault(c => c.Name == fromColName);
    var toCol = toTable.Columns.FirstOrDefault(c => c.Name == toColName);

    if (fromCol == null || toCol == null)
    {
        Output("Warning: Could not create relationship between " + fromTable.Name + "[" + fromColName + "] and " + toTable.Name + "[" + toColName + "] because columns were not found.");
        return;
    }

    // Check if relationship already exists
    if (!Model.Relationships.Any(r => r.FromColumn == fromCol && r.ToColumn == toCol))
    {
        var rel = Model.AddRelationship();
        rel.FromColumn = fromCol;
        rel.ToColumn = toCol;
        rel.IsActive = true; // Assuming these are the primary relationships for this analysis
        rel.CrossFilteringBehavior = CrossFilteringBehavior.OneDirection; // Standard Dim -> Fact filtering
    }
}

// 4. Create Relationships
// Cases -> Employee
EnsureRelationship(casesTable, "BranchCallerEmployeeNumber", employeeTable, "EMPLOYEE_ID");

// Activities -> Employee
EnsureRelationship(activitiesTable, "BranchEmployeeNumber", employeeTable, "EMPLOYEE_ID");

// 5. Add Tables and Objects to Perspective
var tablesToInclude = new[] { casesTable, activitiesTable, employeeTable, dateTable };

foreach (var table in tablesToInclude)
{
    // Add Table to Perspective
    table.InPerspective[perspective] = true;

    // Add all Columns
    foreach (var col in table.Columns)
    {
        col.InPerspective[perspective] = true;
    }

    // Add all Measures
    foreach (var meas in table.Measures)
    {
        meas.InPerspective[perspective] = true;
    }

    // Add all Hierarchies
    foreach (var hier in table.Hierarchies)
    {
        hier.InPerspective[perspective] = true;
    }
}
