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
                name: "JsonSchemas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SchemaId = table.Column<string>(type: "TEXT", nullable: false),
                    SchemaString = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JsonSchemas", x => x.Id);
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
                name: "Slugs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    EntityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    State = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Slugs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataCollections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Slug = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DefaultDataStoreId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataCollections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataEntityMetadataJsonField",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DataSetId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DataFileId = table.Column<Guid>(type: "TEXT", nullable: true),
                    MetadataKey = table.Column<string>(type: "TEXT", nullable: false),
                    MetadataJsonFieldId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataEntityMetadataJsonField", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    FileTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SizeBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    SHA256Hash = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedStamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DeletedStamp = table.Column<DateTime>(type: "TEXT", nullable: true),
                    BeginStamp = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EndStamp = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataFiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MetadataJsonFields",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ValueId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetadataJsonFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MetadataJsonFields_DataFiles_ValueId",
                        column: x => x.ValueId,
                        principalTable: "DataFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JsonSchemaEntityMetadataJsonField",
                columns: table => new
                {
                    MetadataJsonFieldId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ValidatedSchemasId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JsonSchemaEntityMetadataJsonField", x => new { x.MetadataJsonFieldId, x.ValidatedSchemasId });
                    table.ForeignKey(
                        name: "FK_JsonSchemaEntityMetadataJsonField_JsonSchemas_ValidatedSchemasId",
                        column: x => x.ValidatedSchemasId,
                        principalTable: "JsonSchemas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JsonSchemaEntityMetadataJsonField_MetadataJsonFields_MetadataJsonFieldId",
                        column: x => x.MetadataJsonFieldId,
                        principalTable: "MetadataJsonFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DataSetJob",
                columns: table => new
                {
                    SourceDatasetsId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SourceForJobsId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataSetJob", x => new { x.SourceDatasetsId, x.SourceForJobsId });
                });

            migrationBuilder.CreateTable(
                name: "DataSetLabel",
                columns: table => new
                {
                    AssignedLabelsId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AssignedToDataSetsId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataSetLabel", x => new { x.AssignedLabelsId, x.AssignedToDataSetsId });
                });

            migrationBuilder.CreateTable(
                name: "DataSets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Slug = table.Column<string>(type: "TEXT", nullable: true),
                    AncestorDatasetIds = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedStamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DeletedStamp = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LifecycleState = table.Column<int>(type: "INTEGER", nullable: false),
                    DeletionState = table.Column<int>(type: "INTEGER", nullable: false),
                    CreateJobId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataSets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataSets_DataCollections_ParentId",
                        column: x => x.ParentId,
                        principalTable: "DataCollections",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DataSetTag",
                columns: table => new
                {
                    AssignedTagsId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AssignedToDataSetsId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataSetTag", x => new { x.AssignedTagsId, x.AssignedToDataSetsId });
                    table.ForeignKey(
                        name: "FK_DataSetTag_DataSets_AssignedToDataSetsId",
                        column: x => x.AssignedToDataSetsId,
                        principalTable: "DataSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DataStores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Slug = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    StorageType = table.Column<int>(type: "INTEGER", nullable: false),
                    EndpointUrl = table.Column<string>(type: "TEXT", nullable: true),
                    KeyPrefix = table.Column<string>(type: "TEXT", nullable: true),
                    Bucket = table.Column<string>(type: "TEXT", nullable: true),
                    AccessKeyReference = table.Column<string>(type: "TEXT", nullable: true),
                    SecretKeyReference = table.Column<string>(type: "TEXT", nullable: true),
                    UsePathStyle = table.Column<bool>(type: "INTEGER", nullable: true),
                    Region = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataStores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FileStorageReferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Algorithm = table.Column<int>(type: "INTEGER", nullable: false),
                    SizeBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    SHA256Hash = table.Column<string>(type: "TEXT", nullable: false),
                    StorageType = table.Column<int>(type: "INTEGER", nullable: false),
                    StoreFid = table.Column<Guid>(type: "TEXT", nullable: true),
                    FileFid = table.Column<Guid>(type: "TEXT", nullable: true),
                    Data = table.Column<byte[]>(type: "BLOB", nullable: true),
                    ObjectKey = table.Column<string>(type: "TEXT", nullable: true),
                    URL = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileStorageReferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileStorageReferences_DataFiles_FileFid",
                        column: x => x.FileFid,
                        principalTable: "DataFiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FileStorageReferences_DataStores_StoreFid",
                        column: x => x.StoreFid,
                        principalTable: "DataStores",
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
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Slug = table.Column<string>(type: "TEXT", nullable: true),
                    InheritFileTypes = table.Column<bool>(type: "INTEGER", nullable: false),
                    DefaultDataStoreId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projects_DataStores_DefaultDataStoreId",
                        column: x => x.DefaultDataStoreId,
                        principalTable: "DataStores",
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
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ParentProjectId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tags_Projects_ParentProjectId",
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

            migrationBuilder.InsertData(
                table: "Projects",
                columns: new[] { "Id", "DefaultDataStoreId", "Description", "InheritFileTypes", "Name", "Slug" },
                values: new object[] { new Guid("11f819a0-6857-4a9f-8a77-caf1a845776e"), null, "The instances global mockup project.", true, "_global", "_global" });

            migrationBuilder.CreateIndex(
                name: "IX_DataCollections_DefaultDataStoreId",
                table: "DataCollections",
                column: "DefaultDataStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_DataCollections_ParentId",
                table: "DataCollections",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_DataEntityMetadataJsonField_DataFileId",
                table: "DataEntityMetadataJsonField",
                column: "DataFileId");

            migrationBuilder.CreateIndex(
                name: "IX_DataEntityMetadataJsonField_DataFileId_MetadataKey",
                table: "DataEntityMetadataJsonField",
                columns: new[] { "DataFileId", "MetadataKey" });

            migrationBuilder.CreateIndex(
                name: "IX_DataEntityMetadataJsonField_DataSetId",
                table: "DataEntityMetadataJsonField",
                column: "DataSetId");

            migrationBuilder.CreateIndex(
                name: "IX_DataEntityMetadataJsonField_DataSetId_MetadataKey",
                table: "DataEntityMetadataJsonField",
                columns: new[] { "DataSetId", "MetadataKey" });

            migrationBuilder.CreateIndex(
                name: "IX_DataEntityMetadataJsonField_MetadataJsonFieldId",
                table: "DataEntityMetadataJsonField",
                column: "MetadataJsonFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_DataFiles_FileTypeId",
                table: "DataFiles",
                column: "FileTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DataFiles_ParentId",
                table: "DataFiles",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_DataSetJob_SourceForJobsId",
                table: "DataSetJob",
                column: "SourceForJobsId");

            migrationBuilder.CreateIndex(
                name: "IX_DataSetLabel_AssignedToDataSetsId",
                table: "DataSetLabel",
                column: "AssignedToDataSetsId");

            migrationBuilder.CreateIndex(
                name: "IX_DataSets_CreateJobId",
                table: "DataSets",
                column: "CreateJobId");

            migrationBuilder.CreateIndex(
                name: "IX_DataSets_ParentId",
                table: "DataSets",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_DataSetTag_AssignedToDataSetsId",
                table: "DataSetTag",
                column: "AssignedToDataSetsId");

            migrationBuilder.CreateIndex(
                name: "IX_DataStores_ParentId",
                table: "DataStores",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_FileStorageReferences_FileFid",
                table: "FileStorageReferences",
                column: "FileFid");

            migrationBuilder.CreateIndex(
                name: "IX_FileStorageReferences_StoreFid",
                table: "FileStorageReferences",
                column: "StoreFid");

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
                name: "IX_JsonSchemaEntityMetadataJsonField_ValidatedSchemasId",
                table: "JsonSchemaEntityMetadataJsonField",
                column: "ValidatedSchemasId");

            migrationBuilder.CreateIndex(
                name: "IX_Label_ParentProjectId",
                table: "Label",
                column: "ParentProjectId");

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
                name: "IX_MetadataJsonFields_ValueId",
                table: "MetadataJsonFields",
                column: "ValueId");

            migrationBuilder.CreateIndex(
                name: "IX_PipelineInstances_LocalId",
                table: "PipelineInstances",
                column: "LocalId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_DefaultDataStoreId",
                table: "Projects",
                column: "DefaultDataStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_ParentProjectId",
                table: "Tags",
                column: "ParentProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Types_ProjectId",
                table: "Types",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_DataCollections_DataStores_DefaultDataStoreId",
                table: "DataCollections",
                column: "DefaultDataStoreId",
                principalTable: "DataStores",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DataCollections_Projects_ParentId",
                table: "DataCollections",
                column: "ParentId",
                principalTable: "Projects",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DataEntityMetadataJsonField_DataFiles_DataFileId",
                table: "DataEntityMetadataJsonField",
                column: "DataFileId",
                principalTable: "DataFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DataEntityMetadataJsonField_DataSets_DataSetId",
                table: "DataEntityMetadataJsonField",
                column: "DataSetId",
                principalTable: "DataSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DataEntityMetadataJsonField_MetadataJsonFields_MetadataJsonFieldId",
                table: "DataEntityMetadataJsonField",
                column: "MetadataJsonFieldId",
                principalTable: "MetadataJsonFields",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DataFiles_DataSets_ParentId",
                table: "DataFiles",
                column: "ParentId",
                principalTable: "DataSets",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DataFiles_Types_FileTypeId",
                table: "DataFiles",
                column: "FileTypeId",
                principalTable: "Types",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DataSetJob_DataSets_SourceDatasetsId",
                table: "DataSetJob",
                column: "SourceDatasetsId",
                principalTable: "DataSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DataSetJob_Jobs_SourceForJobsId",
                table: "DataSetJob",
                column: "SourceForJobsId",
                principalTable: "Jobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DataSetLabel_DataSets_AssignedToDataSetsId",
                table: "DataSetLabel",
                column: "AssignedToDataSetsId",
                principalTable: "DataSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DataSetLabel_Label_AssignedLabelsId",
                table: "DataSetLabel",
                column: "AssignedLabelsId",
                principalTable: "Label",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DataSets_Jobs_CreateJobId",
                table: "DataSets",
                column: "CreateJobId",
                principalTable: "Jobs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DataSetTag_Tags_AssignedTagsId",
                table: "DataSetTag",
                column: "AssignedTagsId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DataStores_Projects_ParentId",
                table: "DataStores",
                column: "ParentId",
                principalTable: "Projects",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_DataStores_DefaultDataStoreId",
                table: "Projects");

            migrationBuilder.DropTable(
                name: "DataEntityMetadataJsonField");

            migrationBuilder.DropTable(
                name: "DataSetJob");

            migrationBuilder.DropTable(
                name: "DataSetLabel");

            migrationBuilder.DropTable(
                name: "DataSetTag");

            migrationBuilder.DropTable(
                name: "FileStorageReferences");

            migrationBuilder.DropTable(
                name: "JsonSchemaEntityMetadataJsonField");

            migrationBuilder.DropTable(
                name: "LabelSharingPolicies");

            migrationBuilder.DropTable(
                name: "LogSection");

            migrationBuilder.DropTable(
                name: "Slugs");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "JsonSchemas");

            migrationBuilder.DropTable(
                name: "MetadataJsonFields");

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
                name: "DataStores");

            migrationBuilder.DropTable(
                name: "Projects");
        }
    }
}
