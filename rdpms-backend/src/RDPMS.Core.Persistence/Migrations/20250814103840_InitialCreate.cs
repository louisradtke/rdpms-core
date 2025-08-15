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
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    DefaultDataStoreId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projects_DataStores_DefaultDataStoreId",
                        column: x => x.DefaultDataStoreId,
                        principalTable: "DataStores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DataCollections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ParentProjectId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DefaultDataStoreId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataCollections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataCollections_DataStores_DefaultDataStoreId",
                        column: x => x.DefaultDataStoreId,
                        principalTable: "DataStores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DataCollections_Projects_ParentProjectId",
                        column: x => x.ParentProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Label",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ParentProjectId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Label", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Label_Projects_ParentProjectId",
                        column: x => x.ParentProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Types",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Abbreviation = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    MimeType = table.Column<string>(type: "TEXT", nullable: true),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Types", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Types_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id");
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
                    OutputCollectionEntityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PipelineInstanceId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Jobs_DataCollections_OutputCollectionEntityId",
                        column: x => x.OutputCollectionEntityId,
                        principalTable: "DataCollections",
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
                name: "LabelSharingPolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SharedLabelId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectSharedToId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CanRead = table.Column<bool>(type: "INTEGER", nullable: false),
                    CanWrite = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabelSharingPolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabelSharingPolicies_Label_SharedLabelId",
                        column: x => x.SharedLabelId,
                        principalTable: "Label",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LabelSharingPolicies_Projects_ProjectSharedToId",
                        column: x => x.ProjectSharedToId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DataSets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AncestorDatasetIds = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedStamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DeletedStamp = table.Column<DateTime>(type: "TEXT", nullable: true),
                    State = table.Column<int>(type: "INTEGER", nullable: false),
                    CreateJobId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DataCollectionEntityId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataSets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataSets_DataCollections_DataCollectionEntityId",
                        column: x => x.DataCollectionEntityId,
                        principalTable: "DataCollections",
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
                    Size = table.Column<long>(type: "INTEGER", nullable: false),
                    Hash = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedStamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DeletedStamp = table.Column<DateTime>(type: "TEXT", nullable: true),
                    BeginStamp = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EndStamp = table.Column<DateTime>(type: "TEXT", nullable: true),
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
                name: "DataSetUsedForJobsRelation",
                columns: table => new
                {
                    SourceDatasetsId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SourceForJobsId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataSetUsedForJobsRelation", x => new { x.SourceDatasetsId, x.SourceForJobsId });
                    table.ForeignKey(
                        name: "FK_DataSetUsedForJobsRelation_DataSets_SourceDatasetsId",
                        column: x => x.SourceDatasetsId,
                        principalTable: "DataSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DataSetUsedForJobsRelation_Jobs_SourceForJobsId",
                        column: x => x.SourceForJobsId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LabelsAssignedToDataSetsRelation",
                columns: table => new
                {
                    LabelId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DataSetId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabelsAssignedToDataSetsRelation", x => new { x.LabelId, x.DataSetId });
                    table.ForeignKey(
                        name: "FK_LabelsAssignedToDataSetsRelation_DataSets_DataSetId",
                        column: x => x.DataSetId,
                        principalTable: "DataSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LabelsAssignedToDataSetsRelation_Label_LabelId",
                        column: x => x.LabelId,
                        principalTable: "Label",
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
                    SourceName = table.Column<string>(type: "TEXT", nullable: false),
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
                name: "IX_DataCollections_DefaultDataStoreId",
                table: "DataCollections",
                column: "DefaultDataStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_DataCollections_ParentProjectId",
                table: "DataCollections",
                column: "ParentProjectId");

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
                name: "IX_DataSets_DataCollectionEntityId",
                table: "DataSets",
                column: "DataCollectionEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_DataSetUsedForJobsRelation_SourceForJobsId",
                table: "DataSetUsedForJobsRelation",
                column: "SourceForJobsId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_LocalId",
                table: "Jobs",
                column: "LocalId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_OutputCollectionEntityId",
                table: "Jobs",
                column: "OutputCollectionEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_OutputDataStoreId",
                table: "Jobs",
                column: "OutputDataStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_PipelineInstanceId",
                table: "Jobs",
                column: "PipelineInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_State",
                table: "Jobs",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_Label_ParentProjectId",
                table: "Label",
                column: "ParentProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_LabelsAssignedToDataSetsRelation_DataSetId",
                table: "LabelsAssignedToDataSetsRelation",
                column: "DataSetId");

            migrationBuilder.CreateIndex(
                name: "IX_LabelSharingPolicies_ProjectSharedToId",
                table: "LabelSharingPolicies",
                column: "ProjectSharedToId");

            migrationBuilder.CreateIndex(
                name: "IX_LabelSharingPolicies_SharedLabelId",
                table: "LabelSharingPolicies",
                column: "SharedLabelId");

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
                name: "IX_PipelineInstances_LocalId",
                table: "PipelineInstances",
                column: "LocalId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_DefaultDataStoreId",
                table: "Projects",
                column: "DefaultDataStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_DataSetId",
                table: "Tags",
                column: "DataSetId");

            migrationBuilder.CreateIndex(
                name: "IX_Types_ProjectId",
                table: "Types",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataSetsUsedForJobs");

            migrationBuilder.DropTable(
                name: "DataSetUsedForJobsRelation");

            migrationBuilder.DropTable(
                name: "LabelsAssignedToDataSetsRelation");

            migrationBuilder.DropTable(
                name: "LabelSharingPolicies");

            migrationBuilder.DropTable(
                name: "LogSection");

            migrationBuilder.DropTable(
                name: "MetadataJsonField");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Label");

            migrationBuilder.DropTable(
                name: "DataFiles");

            migrationBuilder.DropTable(
                name: "DataSets");

            migrationBuilder.DropTable(
                name: "Types");

            migrationBuilder.DropTable(
                name: "Jobs");

            migrationBuilder.DropTable(
                name: "DataCollections");

            migrationBuilder.DropTable(
                name: "PipelineInstances");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "DataStores");
        }
    }
}
