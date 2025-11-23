using UnityDevHub.API.Data.Entities;

namespace UnityDevHub.API.Extensions;

public static class TaskColumnExtensions
{
    private static readonly string[] CompletedColumnNames = { "Done", "Completed" };

    public static bool IsCompleted(this TaskColumn column)
    {
        if (column == null) return false;
        return CompletedColumnNames.Contains(column.Name);
    }
}
