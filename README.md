# Research Data and Pipe Management System (RDPMS)

RDPMS is a project that follows a twofold approach: (1) it may store and process data that was collected especially in robotics and autonomous driving context and (2) may support sharing and tracking (for legal compliance reasons) of data across multiple instances.

The "tracking" may be implemented as "taint": it shall be clearly traceable, which parts (in time) of a dataset were processed to which datasets. Thus, a deletion request may be performed with minimal loss and effort.

More information can be found in [our proposal](./docs/radtke-proposal-distributed-rdm-system-2024.pdf) for the [RDMxSE workshop](https://nfdixcs.org/event/working-workshop-on-research-data-management-for-and-in-software-engineering-rdmxse%EF%BB%BF). 


## Conventions

- All timestamps are UTC. Only on the visualization "layer", they may be converted to local time.


## Roadmap

### Phase 1: Backend setup (core)

1. create ASP.NET solution and EF data context (Sqlite vs. Postgres)
2. establish basic data model containing at least:
   - data files (immutable)
   - data sets (immutable after creation, needs some kind of state)
   - data stores
   - metadata for data sets
     - tags
     - json
   - pipeline instances
   - pipeline job instances
   - job queue (not for dispatchment or scheduling)
   - artifacts (redundant to data files/sets? UI may accept data of specific type (PNG vs. MCAP) instead of artifact in general.)
   - taint of data (files and sets)
   - Q: what, if some data was just uploaded to an S3 bucket and now has to be indexed, just like in RBB? can a job alter the metadata of its source, but not create an artifact?
3. create interfaces and mockups for:
   - general system configuration
   - data stores
   - pipelines
4. create REST ASP.NET API and basic types
5. build debugging system (eval docker)

### Phase 2: Web UI setup

1. decide on UI framework and JS vs. TS (e.g. Svelte switched back to annotated JS, still supporting TS)
2. build auto generation of JS REST client
3. create basic dialogues
   - data sets
   - pipelines viewer and job viewer

### Phase 3: Setting up base security infrastructure

1. permission system/user ids (a lot depends on this and it will be horror to retrofit)
   - users
   - abstraction for ownership entities (user/group/organizations)
   - username + password auth
2. Embedding of auth into Web UI

### Phase 4: Setting up pipeline system

1. decide on pipeline backend (Snakemake vs. ..., see below)
2. create microservice for pipeline tool
   - trigger pipelines by core
   - add core as target for dispatching jobs

What speaks against Snakemake and other DAG-workflow tools: pipelines are demand-triggered instead of being triggered by new content. But maybe this is not necessary, since inputs of a specific form come in, we want to transform them into a standardized format. Still, we won't need the DAG-features, unless we want to aggregate multiple data sets (like multiple traces we want to process using [STARS](https://github.com/tudo-aqua/stars)).

### Phase 5: Build distributed worker system

1. determine requirements to work dispatchment system
2. decide on worker engine (sth. existing vs. abusing GitLab vs. self-made)
3. implementation (either directly into backend or as microservice)


## List of requirements and potentially planned features

Remarks:
- The current list does not include any requirements related to sharing data.
- "FSD" refers to a project and concepts by a group of Formula Student teams: ETH Zürich, TU Hamburg, KIT, NTNU Trondheim, Uni Bayreuth, and TU Dortmund.

| Enumeration | Requirement Text                                                                                | Priority |
| ----------- | ----------------------------------------------------------------------------------------------- | -------- |
| A           | **General**                                                                                     |          |
| A.1         | Group multiple data files to a set (1:1, 1:many) [FSD A.1]                                      | HIGH     |
| A.2         | Search/Filter by existence of topics, content of topics, metadata [FSD 1.2]                     | MOD      |
| A.3         | Tagging data [FSD A.5]                                                                          | MOD      |
| A.4         | Grouping data (A.1) can be done by a plugin                                                     | MOD      |
| B           | **Pipeline-related. A pipeline consists of several jobs.**                                      |          |
| B.1         | Pipelines are executed on behalf of triggers                                                    | HIGH     |
| B.2         | Job-executors should be able to process any type of file                                        | HIGH     |
| B.3         | Triggers can be an upload [FSD B.1.IV]                                                          | HIGH     |
| B.4         | Triggers can be a plugin [FSD B.1.V]                                                            | HIGH     |
| B.5         | Triggers can be an arbitrary signal, like an API call                                           | HIGH     |
| B.6         | Triggers can be the setting of a tag                                                            | HIGH     |
| B.7         | Pipelines should be able to create content to be presented in the web interface [FSD B.1.I]     | HIGH     |
| B.8         | Pipeline execution                                                                              |          |
| B.8.1       | Pipelines consume data sets (incl. single files) and produce artifacts (also data sets?)        | HIGH     |
| B.8.2       | For efficiency reasons, input pipelines should exist, that directly process raw data on device. | MOD      |
| C           | **Backend**                                                                                     |          |
| C.1         | (Entire) system configuration should be file-based/readonly/non-runtime [FSD C.1]               |          |
| C.1.I       | Non-runtime configuration of connection info for file storages, S3-buckets, etc.                | T.B.D.   |
| C.1.II      | (Explicitly not?) Non-runtime configuration of pipelines                                        | T.B.D.   |
| C.2         | Should manage arbitrary amount of storage [FSD C.2]                                             | HIGH     |
| C.3         | "Flexible way" to add custom metadata [FSD C.6.I]                                               | T.B.D.   |
| C.4         | Storage of logs in S3 [FSD C.6.I]                                                               | MOD      |
| C.5         | Storage of logs in filesystem [FSD C.6.II]                                                      | T.B.D.   |
| C.6         | What about a dedicated/existing log management system?                                          | T.B.D.   |
| D           | **Web-based user interface**                                                                    |          |
| D.1         | Display artifacts (see point 2.f) [FSD D.2]                                                     | HIGH     |
| D.2         | Navigate logs [FSD D.1]                                                                         | LOW      |
| D.3         | Search for (user-defined) metadata [FSD D.3.I-III]                                              | LOW      |
| D.4         | Upload files via GUI (low priority) [FSD D.4.I]                                                 | MOD      |
| D.5         | Download files via GUI (low priority) [FSD D.4.II]                                              | MOD      |
| D.6         | Optimized downloading of data (only download the data we need) [FSD D.4.III]                    | LOW      |
| E           | **API-based interface**                                                                         |          |
| E.1         | Upload data via API [FSD E.1.I]                                                                 | MOD      |
| E.2         | Download data via API [FSD E.1.II]                                                              | HIGH     |
| E.3         | Optimized downloading of data (only download the data we need) [FSD E.1.IV]                     | LOW      |
| E.4         | Update Metadata [FSD E.3]                                                                       | MOD      |
| E.5         | CLI tool that wraps the API [FSD E.4]                                                           | LOW      |
| E.6         | Search and filtering functionality [FSD E.2.I-III]                                              | LOW      |
| F           | **Security**                                                                                    |          |
| F.1         | Support user/password authentication [FSD F.3.I]                                                | HIGH     |
| F.2         | Passkeys                                                                                        | LOW      |
| F.3         | Google/Microsoft accounts                                                                       | LOW      |
| F.4         | OAuth 2.0                                                                                       | LOW      |
| F.5         | Support permissions [FSD F.4]                                                                   |          |
| F.5.I       | Support users                                                                                   | HIGH     |
| F.5.II      | Support organizations                                                                           | T.B.D.   |
| F.5.III     | Support groups                                                                                  | LOW      |
| F.5.IV      | Support roles                                                                                   | LOW      |
| G           | **Supported file formats**                                                                      |          |
| G.1         | Base system should be file-type agnostic                                                        | HIGH     |
| G.2         | System should have a notion of file types through plugins [FSD G.4]                             | MOD      |
| G.3         | Bundled plugins should support                                                                  |          |
| G.3.I       | ROS1 (rosbag) [FSD G.1.I]                                                                       | HIGH     |
| G.3.II      | ROS2 (MCAP) [FSD G.1.II]                                                                        | HIGH     |
| G.3.III     | BLF (CAN) [FSD G.2.1]                                                                           | LOW      |
| G.3.IV      | MF4 (CAN) [FSD G.2.1]                                                                           | LOW      |
| G.4         | Metadata                                                                                        |          |
| G.4.I       | JSON/XML/YAML for custom metadata [FSD G.3.I-III]                                               | MOD      |
| G.4.II      | System should have a notion of types (e.g., ROS or CAN DBC message types)                       | T.B.D.   |
| H           | **Development**                                                                                 |          |
| H.1         | Plugin system for user-defined processing [FSD H.2] (Duplicate)                                 | HIGH     |
| H.2         | Should support schema change/plugin version change/upgrade [FSD H.3]                            | HIGH     |
| H.3         | System should facilitate migration from other platforms [FSD H.4]                               | LOW      |
| I           | **Deployment**                                                                                  |          |
| I.1         | System should run on cloud services [FSD I.2.I]                                                 | LOW      |
| I.2         | System should run on private cloud environments [FSD I.2.II]                                    | HIGH     |
