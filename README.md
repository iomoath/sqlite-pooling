## SQLite Connection Pooling Demo with Stress Tests

This project provides a demonstration of SQLite connection pooling and includes stress tests. It's designed to be a flexible template that can be adapted to other database engines as needed.

## Core Components
### Data Layer

The Data Layer in this project is responsible for managing database connections and communicating with the chosen database engine. Its generic design makes it adaptable to various database systems with minimal changes required.

The `SQLiteConnectionManager.cs` class, manages the database connection pool. It ensures a smooth and efficient handling of database connections, opening, closing and disposing them as necessary.


### Stress Test
The `ThreadSafetyTest2.cs` class executes a robust concurrent read-write test using one million distinct threads. If an error occurs during either the read or write operations to the database, the application will raise an exception. However, this test should not be considered exhaustive—it's a stress test designed to validate the reliability of the connection pooling under heavy load.


## Application Architecture

![Architecture](resources/Multi-layer-app_sql_pooling.png?raw=true "Architecture")
