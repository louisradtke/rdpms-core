# Research Data and Pipe Management System (RDPMS)

RDPMS is a project that follows a twofold approach: (1) it may store and process data that was collected especially in robotics and autonomous driving context and (2) may supports sharing and tracking (for legal compliance reasons) of data across multiple instances.

More information can be found in [our proposal](./docs/radtke-proposal-distributed-rdm-system-2024.pdf) for the [RDMxSE workshop](https://nfdixcs.org/event/working-workshop-on-research-data-management-for-and-in-software-engineering-rdmxse%EF%BB%BF). 

## List of requirements and potentially planned features

Remarks
- The current list does not include any requirements related to sharing data.
- "FSD" refers to a project and concepts by a group of Formula Student teams: ETH Zürich, TU Hamburg, KIT, NTNU Trondheim, Uni Bayreuth, and TU Dortmund.

| Enumeration | Requirement Text                                                                            | Priority |
| ----------- | ------------------------------------------------------------------------------------------- | -------- |
| A           | **General**                                                                                 |          |
| A.1         | Group multiple data files to a set (1:1, 1:many) [FSD A.1]                                  | HIGH     |
| A.2         | Search/Filter by existence of topics, content of topics, metadata [FSD 1.2]                 | MOD      |
| A.3         | Tagging data [FSD A.5]                                                                      | MOD      |
| A.4         | Grouping data (A.1) can be done by a plugin                                                 | MOD      |
| B           | **Pipeline-related. A pipeline consists of several jobs.**                                  |          |
| B.1         | Pipelines are executed on behalf of triggers                                                | HIGH     |
| B.2         | Job-executors should be able to process any type of file                                    | HIGH     |
| B.3         | Triggers can be an upload [FSD B.1.IV]                                                      | HIGH     |
| B.4         | Triggers can be a plugin [FSD B.1.V]                                                        | HIGH     |
| B.5         | Triggers can be an arbitrary signal, like an API call                                       | HIGH     |
| B.6         | Triggers can be the setting of a tag                                                        | HIGH     |
| B.7         | Pipelines should be able to create content to be presented in the web interface [FSD B.1.I] | HIGH     |
| C           | **Backend**                                                                                 |          |
| C.1         | (Entire) system configuration should be file-based/readonly/non-runtime [FSD C.1]           |          |
| C.1.I       | Non-runtime configuration of connection info for file storages, S3-buckets, etc.            | T.B.D.   |
| C.1.II      | (Explicitly not?) Non-runtime configuration of pipelines                                    | T.B.D.   |
| C.2         | Should manage arbitrary amount of storage [FSD C.2]                                         | HIGH     |
| C.3         | "Flexible way" to add custom metadata [FSD C.6.I]                                           | T.B.D.   |
| C.4         | Storage of logs in S3 [FSD C.6.I]                                                           | MOD      |
| C.5         | Storage of logs in filesystem [FSD C.6.II]                                                  | T.B.D.   |
| C.6         | What about a dedicated/existing log management system?                                      | T.B.D.   |
| D           | **Web-based user interface**                                                                |          |
| D.1         | Display artifacts (see point 2.f) [FSD D.2]                                                 | HIGH     |
| D.2         | Navigate logs [FSD D.1]                                                                     | LOW      |
| D.3         | Search for (user-defined) metadata [FSD D.3.I-III]                                          | LOW      |
| D.4         | Upload files via GUI (low priority) [FSD D.4.I]                                             | MOD      |
| D.5         | Download files via GUI (low priority) [FSD D.4.II]                                          | MOD      |
| D.6         | Optimized downloading of data (only download the data we need) [FSD D.4.III]                | LOW      |
| E           | **API-based interface**                                                                     |          |
| E.1         | Upload data via API [FSD E.1.I]                                                             | MOD      |
| E.2         | Download data via API [FSD E.1.II]                                                          | HIGH     |
| E.3         | Optimized downloading of data (only download the data we need) [FSD E.1.IV]                 | LOW      |
| E.4         | Update Metadata [FSD E.3]                                                                   | MOD      |
| E.5         | CLI tool that wraps the API [FSD E.4]                                                       | LOW      |
| E.6         | Search and filtering functionality [FSD E.2.I-III]                                          | LOW      |
| F           | **Security**                                                                                |          |
| F.1         | Support user/password authentication [FSD F.3.I]                                            | HIGH     |
| F.2         | Passkeys                                                                                    | LOW      |
| F.3         | Google/Microsoft accounts                                                                   | LOW      |
| F.4         | OAuth 2.0                                                                                   | LOW      |
| F.5         | Support permissions [FSD F.4]                                                               |          |
| F.5.I       | Support users                                                                               | HIGH     |
| F.5.II      | Support organizations                                                                       | T.B.D.   |
| F.5.III     | Support groups                                                                              | LOW      |
| F.5.IV      | Support roles                                                                               | LOW      |
| G           | **Supported file formats**                                                                  |          |
| G.1         | Base system should be file-type agnostic                                                    | HIGH     |
| G.2         | System should have a notion of file types through plugins [FSD G.4]                         | MOD      |
| G.3         | Bundled plugins should support                                                              |          |
| G.3.I       | ROS1 (rosbag) [FSD G.1.I]                                                                   | HIGH     |
| G.3.II      | ROS2 (MCAP) [FSD G.1.II]                                                                    | HIGH     |
| G.3.III     | BLF (CAN) [FSD G.2.1]                                                                       | LOW      |
| G.3.IV      | MF4 (CAN) [FSD G.2.1]                                                                       | LOW      |
| G.4         | Metadata                                                                                    |          |
| G.4.I       | JSON/XML/YAML for custom metadata [FSD G.3.I-III]                                           | MOD      |
| G.4.II      | System should have a notion of types (e.g., ROS or CAN DBC message types)                   | T.B.D.   |
| H           | **Development**                                                                             |          |
| H.1         | Plugin system for user-defined processing [FSD H.2] (Duplicate)                             | HIGH     |
| H.2         | Should support schema change/plugin version change/upgrade [FSD H.3]                        | HIGH     |
| H.3         | System should facilitate migration from other platforms [FSD H.4]                           | LOW      |
| I           | **Deployment**                                                                              |          |
| I.1         | System should run on cloud services [FSD I.2.I]                                             | LOW      |
| I.2         | System should run on private cloud environments [FSD I.2.II]                                | HIGH     |
