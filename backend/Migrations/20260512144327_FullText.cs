using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class FullText : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (!migrationBuilder.IsSqlite())
                throw new InvalidOperationException("Operation not supported for non-SQLite databases");

            migrationBuilder.Sql(
                // lang=sql
                """
                CREATE VIRTUAL TABLE FullTextIndex USING fts5
                (
                    Content,
                    content='DocumentChunks',
                    content_rowid='Id',
                    tokenize = porter
                );

                CREATE TRIGGER t1_ai
                    AFTER INSERT
                    ON DocumentChunks
                BEGIN
                    INSERT INTO FullTextIndex(rowid, Content)
                    VALUES (new.Id, new.Content);
                END;
                CREATE TRIGGER t1_ad
                    AFTER DELETE
                    ON DocumentChunks
                BEGIN
                    INSERT INTO FullTextIndex(FullTextIndex, rowid)
                    VALUES ('delete', old.Id);
                END;
                CREATE TRIGGER t1_au
                    AFTER UPDATE
                    ON DocumentChunks
                BEGIN
                    INSERT INTO FullTextIndex(FullTextIndex, rowid)
                    VALUES ('delete', old.Id);

                    INSERT INTO FullTextIndex(rowid, Content)
                    VALUES (new.Id, new.Content);
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (!migrationBuilder.IsSqlite())
                throw new InvalidOperationException("Operation not supported for non-SQLite databases");

            migrationBuilder.Sql(
                // lang=sql
                """
                DROP TRIGGER IF EXISTS t1_ai;
                DROP TRIGGER IF EXISTS t1_ad;
                DROP TRIGGER IF EXISTS t1_au;
                DROP TABLE IF EXISTS FullTextIndex;
                """);
        }
    }
}
