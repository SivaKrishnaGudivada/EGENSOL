# We choose exact tag (not 'latest'), to be sure that new version wont break creating image
FROM mcr.microsoft.com/mssql/server:2019-latest

USER root
# Create app directory
RUN mkdir -p /usr/src/app
WORKDIR /usr/src/app

# Copy initialization scripts
COPY *.sh /usr/src/app
COPY *.sql /usr/src/app
RUN chmod +x /usr/src/app/run-initialization.sh


ENV SA_PASSWORD ChangePassword123
ENV ACCEPT_EULA Y
ENV MSSQL_PID Express

# Expose port 1433 in case accesing from other container
EXPOSE 1433

# Note: If you want to start MsSQL only (without initialization script) you can comment bellow line out, CMD entry from base image will be taken
CMD /bin/bash ./entrypoint.sh