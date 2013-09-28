InnerDb
=======
InnerDb is an open-source, deployment-free NoSQL database. It runs entirely within process, and starts and stops with your applications. It supports backup and restore.

What It Is
==========
v1.0 should include basic features:

- Starting and stopping the database
- Connecting and disconnecting 
- List, and select/use a database
- Indexing on fields
- Atomic operations
- Import/export a database from a ZIP file
- Statistics monitoring (table scans vs. index lookups)

What It Isn't
=============
InnerDb isn't everything. It's not an enterprise database. We don't intend to support:

- Remote deployments (eg. application is on one machine and talks over the network to the database)
