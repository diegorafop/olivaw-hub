using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Owh.OlivawCore.Dados.Extensions;

public static class ConstraintExtensions
{
    private const int MaximumNameSize = 63;
    private const string ForeignKeyPostfix = "_fk";
    private const string PrimaryKeyPostfix = "_pk";
    private const string IndexPostfix = "_ix";
    private const string IdKey = "id";

    public static void SetupConstraints
    (
        this ModelBuilder modelBuilder
    )
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var key in entity.GetKeys())
                key.SetName(PrimaryKeyName(entity).ToLower());

            foreach (var key in entity.GetForeignKeys())
                key.SetConstraintName(ForeignKeyName(entity, key).ToLower());

            foreach (var key in entity.GetIndexes())
                key.SetDatabaseName(IndexName(entity, key).ToLower());
        }
    }

    private static string PrimaryKeyName
    (
        IMutableEntityType entity
    )
    {
        string tableName = entity.GetTableName();
        string constraintName = $"{tableName}{PrimaryKeyPostfix}";

        if (constraintName.Length > MaximumNameSize && !string.IsNullOrEmpty(tableName))
            constraintName = $"{tableName[..(MaximumNameSize - PrimaryKeyPostfix.Length)]}{PrimaryKeyPostfix}";

        return constraintName;
    }

    private static string ForeignKeyName
    (
        IMutableEntityType entity,
        IMutableForeignKey key
    )
    {
        string tablePrimaryName = key.PrincipalEntityType.GetTableName();
        string tableSecondaryName = entity.GetTableName();
        string fieldName = GetFieldName(key);
        string constraintName = $"{tablePrimaryName}_{tableSecondaryName}_{fieldName}{ForeignKeyPostfix}";

        if (constraintName.Length > MaximumNameSize)
            constraintName = SplitForeignKeyName(tablePrimaryName, tableSecondaryName, fieldName);

        return constraintName;
    }

    private static string GetFieldName
    (
        IMutableForeignKey key
    )
    {
        if (key.Properties == null)
            return IdKey;

        if (key.Properties.Count == 0)
            return IdKey;

        return key
            .Properties[0]
            .Name
            .Trim();
    }

    private static string IndexName
    (
        IMutableEntityType entity,
        IMutableIndex key
    )
    {
        string tableName = entity.GetTableName();
        string fieldName = GetFieldName(key);
        string indexName = $"{tableName}_{fieldName}{IndexPostfix}";

        if (indexName.Length > MaximumNameSize)
            indexName = SplitIndexName(tableName, fieldName);

        return indexName;
    }

    private static string GetFieldName
    (
        IMutableIndex key
    )
    {
        if (key.Properties == null)
            return IdKey;

        if (key.Properties.Count == 0)
            return IdKey;

        return key
            .Properties[0]
            .Name
            .Trim();
    }

    private static string SplitIndexName
    (
        string tableName,
        string fieldName
    )
    {
        var builder = new ConstraintBuilder
        {
            TableNamePrimary = tableName,
            TableNameSecondary = string.Empty,
            FieldName = fieldName,
            MaximumNameLength = MaximumNameSize,
            PostfixName = IndexPostfix,
        };

        builder.Build();

        return builder
            .GetName()
            .Replace("__", "_");
    }

    private static string SplitForeignKeyName
    (
        string tableNamePrimary,
        string tableNameSecondary,
        string fieldName
    )
    {
        var builder = new ConstraintBuilder
        {
            TableNamePrimary = tableNamePrimary,
            TableNameSecondary = tableNameSecondary,
            FieldName = fieldName,
            MaximumNameLength = MaximumNameSize,
            PostfixName = ForeignKeyPostfix,
            IdKey = IdKey
        };

        builder.Build();
        return builder.GetName();
    }
}

public class ConstraintBuilder
{
    public string TableNamePrimary { get; set; }
    public string TableNameSecondary { get; set; }
    public string FieldName { get; set; }

    public int TableNamePrimaryLength { get; set; }
    public int TableNameSecondaryLength { get; set; }
    public int FieldNameLength { get; set; }

    public int MaximumNameLength { get; set; }
    public string PostfixName { get; set; }
    public string IdKey { get; set; }

    public string GetName()
    {
        return $"{TableNamePrimary}_{TableNameSecondary}_{FieldName}{PostfixName}";
    }

    public void Build()
    {
        SetManyLength();
        SetManyName();
    }

    private void SetManyName()
    {
        SetFieldName();
        SetTablePrimaryName();
        SetTableSecondaryName();
    }

    private void SetManyLength()
    {
        SetFieldNameLength();
        SetManyTableNameLength();
    }

    private void SetTableSecondaryName()
    {
        TableNameSecondary = string.IsNullOrEmpty(TableNameSecondary) ? string.Empty :
            TableNameSecondary
                .Replace(TableNamePrimary, string.Empty)
                .PadRight(TableNameSecondaryLength)
                [..TableNameSecondaryLength]
                .TrimEnd();
    }

    private void SetTablePrimaryName()
    {
        TableNamePrimary =
            TableNamePrimary
                .PadRight(TableNamePrimaryLength)[..TableNamePrimaryLength]
                .TrimEnd();
    }

    private void SetFieldName()
    {
        if (FieldNameHasId())
            FieldName = $"{FieldName[..(FieldNameLength - 2)].Trim()}{IdKey}";

        FieldName = FieldName[..FieldNameLength].Trim();
    }

    private bool FieldNameHasId()
    {
        return !string.IsNullOrEmpty(IdKey)
            && FieldName.EndsWith(IdKey, StringComparison.OrdinalIgnoreCase);
    }

    private void SetManyTableNameLength()
    {
        int maximumNameLength = MaximumNameLength - PostfixName.Length - FieldNameLength - 2;
        TableNamePrimaryLength = (int)Math.Floor(d: (decimal)((maximumNameLength) / 2));

        if (TableNamePrimary.Length < TableNamePrimaryLength)
        {
            TableNamePrimaryLength = TableNamePrimary.Length;
            TableNameSecondaryLength = maximumNameLength - TableNamePrimaryLength;
        }
        else if (TableNameSecondary.Length < TableNameSecondaryLength)
        {
            TableNameSecondaryLength = TableNameSecondary.Length;
            TableNamePrimaryLength = maximumNameLength - TableNameSecondaryLength;
        }
        else
            TableNameSecondaryLength = maximumNameLength - TableNamePrimaryLength;
    }

    private int SetFieldNameLength()
    {
        int maximumNameLength = MaximumNameLength - PostfixName.Length;
        FieldNameLength = (int)Math.Floor(d: (decimal)((maximumNameLength) / 3));
        int fieldNameMaximumOffsetLength = FieldNameLength;

        if (FieldName.Length < FieldNameLength)
            FieldNameLength = FieldName.Length;
        else if (FieldName.Length < (FieldNameLength + fieldNameMaximumOffsetLength))
            FieldNameLength = FieldName.Length;
        else
            FieldNameLength += fieldNameMaximumOffsetLength;
        return FieldNameLength;
    }
}
