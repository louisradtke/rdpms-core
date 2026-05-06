using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RDPMS.Core.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(RDPMSPersistenceContext))]
    [Migration("20260506120000_AddDatasetMetadataProjectionCache")]
    public partial class AddDatasetMetadataProjectionCache : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BeginStamp",
                table: "DataSets",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndStamp",
                table: "DataSets",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsTimeSeries",
                table: "DataSets",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TimeSeriesCacheError",
                table: "DataSets",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TimeSeriesCacheRefreshedAt",
                table: "DataSets",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TimeSeriesCacheSourceMetadataFieldId",
                table: "DataSets",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TimeSeriesCacheSourceStamp",
                table: "DataSets",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TimeSeriesCacheVersion",
                table: "DataSets",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedStamp",
                table: "MetadataJsonFields",
                type: "TEXT",
                nullable: false,
                defaultValue: DateTime.UnixEpoch);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedStamp",
                table: "MetadataJsonFields",
                type: "TEXT",
                nullable: false,
                defaultValue: DateTime.UnixEpoch);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BeginStamp",
                table: "DataSets");

            migrationBuilder.DropColumn(
                name: "EndStamp",
                table: "DataSets");

            migrationBuilder.DropColumn(
                name: "IsTimeSeries",
                table: "DataSets");

            migrationBuilder.DropColumn(
                name: "TimeSeriesCacheError",
                table: "DataSets");

            migrationBuilder.DropColumn(
                name: "TimeSeriesCacheRefreshedAt",
                table: "DataSets");

            migrationBuilder.DropColumn(
                name: "TimeSeriesCacheSourceMetadataFieldId",
                table: "DataSets");

            migrationBuilder.DropColumn(
                name: "TimeSeriesCacheSourceStamp",
                table: "DataSets");

            migrationBuilder.DropColumn(
                name: "TimeSeriesCacheVersion",
                table: "DataSets");

            migrationBuilder.DropColumn(
                name: "CreatedStamp",
                table: "MetadataJsonFields");

            migrationBuilder.DropColumn(
                name: "UpdatedStamp",
                table: "MetadataJsonFields");
        }
    }
}
