using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalAcademico.Data.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeCursoActivo : Migration
    {
        private const string NormalizeSql = """
ALTER TABLE "Cursos"
    ALTER COLUMN "Activo" TYPE boolean USING CASE WHEN "Activo"::text IN ('1', 'true', 't', 'yes', 'y') THEN true ELSE false END;
""";

        private const string RevertSql = """
ALTER TABLE "Cursos"
    ALTER COLUMN "Activo" TYPE integer USING CASE WHEN "Activo" THEN 1 ELSE 0 END;
""";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider == "Npgsql.EntityFrameworkCore.PostgreSQL")
            {
                migrationBuilder.Sql(NormalizeSql);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider == "Npgsql.EntityFrameworkCore.PostgreSQL")
            {
                migrationBuilder.Sql(RevertSql);
            }
        }
    }
}
