using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RDPMS.Core.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DataSetsUsedForJobs",
                columns: table => new
                {
                    SourceDatasetId = table.Column<Guid>(type: "TEXT", nullable: false),
                    JobId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataSetsUsedForJobs", x => new { x.JobId, x.SourceDatasetId });
                });

            migrationBuilder.CreateTable(
                name: "DataStores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataStores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PipelineInstances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LocalId = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PipelineInstances", x => x.Id);
                    table.UniqueConstraint("AK_PipelineInstances_LocalId", x => x.LocalId);
                });

            migrationBuilder.CreateTable(
                name: "Types",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Abbreviation = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    MimeType = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Types", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataContainers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    DefaultDataStoreId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataContainers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataContainers_DataStores_DefaultDataStoreId",
                        column: x => x.DefaultDataStoreId,
                        principalTable: "DataStores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LocalId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    State = table.Column<int>(type: "INTEGER", nullable: false),
                    LastUpdateStamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedStamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StartedStamp = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TerminatedStamp = table.Column<DateTime>(type: "TEXT", nullable: true),
                    OutputDataStoreId = table.Column<Guid>(type: "TEXT", nullable: true),
                    OutputContainerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PipelineInstanceId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.Id);
                    table.UniqueConstraint("AK_Jobs_LocalId", x => x.LocalId);
                    table.ForeignKey(
                        name: "FK_Jobs_DataContainers_OutputContainerId",
                        column: x => x.OutputContainerId,
                        principalTable: "DataContainers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Jobs_DataStores_OutputDataStoreId",
                        column: x => x.OutputDataStoreId,
                        principalTable: "DataStores",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Jobs_PipelineInstances_PipelineInstanceId",
                        column: x => x.PipelineInstanceId,
                        principalTable: "PipelineInstances",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DataSets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AncestorIds = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    CreateStamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreateJobId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DataContainerId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataSets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataSets_DataContainers_DataContainerId",
                        column: x => x.DataContainerId,
                        principalTable: "DataContainers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DataSets_Jobs_CreateJobId",
                        column: x => x.CreateJobId,
                        principalTable: "Jobs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DataFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    FileTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreationStamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsTimeSeries = table.Column<bool>(type: "INTEGER", nullable: false),
                    BeginStamp = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedStamp = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DataSetId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DataStoreId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataFiles_DataSets_DataSetId",
                        column: x => x.DataSetId,
                        principalTable: "DataSets",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DataFiles_DataStores_DataStoreId",
                        column: x => x.DataStoreId,
                        principalTable: "DataStores",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DataFiles_Types_FileTypeId",
                        column: x => x.FileTypeId,
                        principalTable: "Types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DataSetUsedForJobs",
                columns: table => new
                {
                    SourceDatasetsId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SourceForJobsId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataSetUsedForJobs", x => new { x.SourceDatasetsId, x.SourceForJobsId });
                    table.ForeignKey(
                        name: "FK_DataSetUsedForJobs_DataSets_SourceDatasetsId",
                        column: x => x.SourceDatasetsId,
                        principalTable: "DataSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DataSetUsedForJobs_Jobs_SourceForJobsId",
                        column: x => x.SourceForJobsId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MetadataJsonField",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    DataSetId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetadataJsonField", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MetadataJsonField_DataSets_DataSetId",
                        column: x => x.DataSetId,
                        principalTable: "DataSets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    DataSetId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tags_DataSets_DataSetId",
                        column: x => x.DataSetId,
                        principalTable: "DataSets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LogSection",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LogContent = table.Column<string>(type: "TEXT", nullable: true),
                    StoredFileId = table.Column<Guid>(type: "TEXT", nullable: true),
                    JobId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogSection", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LogSection_DataFiles_StoredFileId",
                        column: x => x.StoredFileId,
                        principalTable: "DataFiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LogSection_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DataContainers_DefaultDataStoreId",
                table: "DataContainers",
                column: "DefaultDataStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_DataFiles_DataSetId",
                table: "DataFiles",
                column: "DataSetId");

            migrationBuilder.CreateIndex(
                name: "IX_DataFiles_DataStoreId",
                table: "DataFiles",
                column: "DataStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_DataFiles_FileTypeId",
                table: "DataFiles",
                column: "FileTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DataSets_CreateJobId",
                table: "DataSets",
                column: "CreateJobId");

            migrationBuilder.CreateIndex(
                name: "IX_DataSets_DataContainerId",
                table: "DataSets",
                column: "DataContainerId");

            migrationBuilder.CreateIndex(
                name: "IX_DataSetUsedForJobs_SourceForJobsId",
                table: "DataSetUsedForJobs",
                column: "SourceForJobsId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_OutputContainerId",
                table: "Jobs",
                column: "OutputContainerId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_OutputDataStoreId",
                table: "Jobs",
                column: "OutputDataStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_PipelineInstanceId",
                table: "Jobs",
                column: "PipelineInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_LogSection_JobId",
                table: "LogSection",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_LogSection_StoredFileId",
                table: "LogSection",
                column: "StoredFileId");

            migrationBuilder.CreateIndex(
                name: "IX_MetadataJsonField_DataSetId",
                table: "MetadataJsonField",
                column: "DataSetId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_DataSetId",
                table: "Tags",
                column: "DataSetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataSetsUsedForJobs");

            migrationBuilder.DropTable(
                name: "DataSetUsedForJobs");

            migrationBuilder.DropTable(
                name: "LogSection");

            migrationBuilder.DropTable(
                name: "MetadataJsonField");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "DataFiles");

            migrationBuilder.DropTable(
                name: "DataSets");

            migrationBuilder.DropTable(
                name: "Types");

            migrationBuilder.DropTable(
                name: "Jobs");

            migrationBuilder.DropTable(
                name: "DataContainers");

            migrationBuilder.DropTable(
                name: "PipelineInstances");

            migrationBuilder.DropTable(
                name: "DataStores");
        }
    }
}
