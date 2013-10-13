InnerDb
=======
InnerDb is an open-source, deployment-free NoSQL database. It runs entirely within process, and starts and stops with your applications. It supports backup and restore.

What It Is
==========
v1.0 should include basic features:

- Connecting and disconnecting clients (done)
- List, select, and use a database (done)
- Starting and stopping the database (done)
- Add, edit, and delete operations (done)
- Indexing on fields (done)
- Atomic and isolated transactions (in progress)
- Data lives in a zip file (done)
- Statistics monitoring (table scans vs. index lookups)

What It Isn't
=============
InnerDb isn't everything. It's not an enterprise database. We don't intend to support:

- Remote deployments (eg. application is on one machine and talks over the network to the database)
- Sharding
- Scheduled backups

Current Limitations
===================
Currently:
- **IDs must be unique.** Using duplicate IDs (even across types) will cause issues. The client will generally manage IDs for you.
- **All types must implement Equals.** Since we serialize/deserialize, we can't rely on equivalent references.