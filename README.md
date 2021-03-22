# EGENSOL
git clone this repository
run : docker compose up
browse to http://localhost:3000/swagger for rest api and http://localhost:5000 for bulk order api

Run the following command to create the database schema that supports the applications:   
docker exec mssql-server-db /bin/sh "/usr/src/app/entrypoint.sh"
