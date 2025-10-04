using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalAcademico.Data.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeIdentityBooleanColumns : Migration
    {
        private const string NormalizeColumnsSql = """
ALTER TABLE "AspNetUsers"
    ALTER COLUMN "EmailConfirmed" TYPE boolean USING CASE WHEN "EmailConfirmed"::text IN ('1', 'true', 't', 'yes', 'y') THEN true ELSE false END,
    ALTER COLUMN "PhoneNumberConfirmed" TYPE boolean USING CASE WHEN "PhoneNumberConfirmed"::text IN ('1', 'true', 't', 'yes', 'y') THEN true ELSE false END,
    ALTER COLUMN "TwoFactorEnabled" TYPE boolean USING CASE WHEN "TwoFactorEnabled"::text IN ('1', 'true', 't', 'yes', 'y') THEN true ELSE false END,
    ALTER COLUMN "LockoutEnabled" TYPE boolean USING CASE WHEN "LockoutEnabled"::text IN ('1', 'true', 't', 'yes', 'y') THEN true ELSE false END;
""";

        private const string RevertColumnsSql = """
ALTER TABLE "AspNetUsers"
    ALTER COLUMN "EmailConfirmed" TYPE integer USING CASE WHEN "EmailConfirmed" THEN 1 ELSE 0 END,
    ALTER COLUMN "PhoneNumberConfirmed" TYPE integer USING CASE WHEN "PhoneNumberConfirmed" THEN 1 ELSE 0 END,
    ALTER COLUMN "TwoFactorEnabled" TYPE integer USING CASE WHEN "TwoFactorEnabled" THEN 1 ELSE 0 END,
    ALTER COLUMN "LockoutEnabled" TYPE integer USING CASE WHEN "LockoutEnabled" THEN 1 ELSE 0 END;
""";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider == "Npgsql.EntityFrameworkCore.PostgreSQL")
            {
                migrationBuilder.Sql(NormalizeColumnsSql);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider == "Npgsql.EntityFrameworkCore.PostgreSQL")
            {
                migrationBuilder.Sql(RevertColumnsSql);
            }
        }
    }
}
