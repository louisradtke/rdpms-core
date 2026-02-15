using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using RDPMS.Core.Persistence;

#nullable disable

namespace RDPMS.Core.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(RDPMSPersistenceContext))]
    [Migration("20260214101500_AddMetadataColumnTargetForCollectionColumns")]
    public partial class AddMetadataColumnTargetForCollectionColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MetaDataCollectionColumn_ParentCollectionId",
                table: "MetaDataCollectionColumn");

            migrationBuilder.AddColumn<byte>(
                name: "Target",
                table: "MetaDataCollectionColumn",
                type: "INTEGER",
                nullable: false,
                defaultValue: (byte)1);

            migrationBuilder.CreateIndex(
                name: "IX_MetaDataCollectionColumn_ParentCollectionId_MetadataKey_Target",
                table: "MetaDataCollectionColumn",
                columns: new[] { "ParentCollectionId", "MetadataKey", "Target" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MetaDataCollectionColumn_ParentCollectionId_MetadataKey_Target",
                table: "MetaDataCollectionColumn");

            migrationBuilder.DropColumn(
                name: "Target",
                table: "MetaDataCollectionColumn");

            migrationBuilder.CreateIndex(
                name: "IX_MetaDataCollectionColumn_ParentCollectionId",
                table: "MetaDataCollectionColumn",
                column: "ParentCollectionId");
        }
    }
}
