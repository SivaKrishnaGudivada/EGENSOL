# EGENSOL
## Running the application locally ##

*Requires docker installed*

1) git clone this repository
2) Run: docker compose up in the base directory
3) After the containers spin up, run :docker exec mssql-server-db /bin/sh "/usr/src/app/entrypoint.sh" to create the backing database schema for the applications
4) Browse to http://localhost:3000/swagger for rest api and http://localhost:5000 for bulk order api 
