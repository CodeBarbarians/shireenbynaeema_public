namespace Infrastructure
{
using Domain;

/// <summary>
/// Helper class to build a list of AuditCompareRow objects from a list of ActivityLogChangeView objects based on the action type (added, deleted, updated).
/// </summary>
public class AuditCompareBuilder
{
    /// <summary>
    /// Builds a collection of audit comparison rows based on the specified action and list of activity log changes.
    /// </summary>
    /// <remarks>The method treats the action parameter in a case-insensitive manner. If the action is not
    /// recognized, all changes are processed as updates. The returned list contains one row per change, with field
    /// values reflecting the before and after states.</remarks>
    /// <param name="action">The action type to apply when constructing audit rows. Supported values include "added", "deleted", and
    /// "softdeleted". The comparison is case-insensitive.</param>
    /// <param name="changes">The list of activity log changes to process. If null or empty, the method returns an empty collection.</param>
    /// <returns>A list of audit comparison rows representing the changes for the specified action. The list will be empty if no
    /// changes are provided.</returns>
    public List<AuditCompareRow> Build(string action, List<ActivityLogChangeView>? changes)
    {
        var rows = new List<AuditCompareRow>();
        if (changes == null || changes.Count == 0)
        {
            return rows;
        }

        var mode = action.ToLowerInvariant();

        foreach (var c in changes)
        {
            // CREATE
            if (mode == "added")
            {
                rows.Add(new AuditCompareRow
                {
                    Field = c.Field,
                    Before = c.Before,
                    After = c.After,
                });
                continue;
            }

            // DELETE
            if (mode == "deleted" || mode == "softdeleted")
            {
                rows.Add(new AuditCompareRow
                {
                    Field = c.Field,
                    Before = c.Before,
                    After = c.After,
                });
                continue;
            }

            // UPDATE
            rows.Add(new AuditCompareRow
            {
                Field = c.Field,
                Before = c.Before,
                After = c.After,
            });
        }

        return rows;
    }
}
}