#!/bin/bash

mv ../Project/DockerCompose
docker-compose -f docker-compose.yml -f docker-compose.override.yml -f docker-compose.vs.release.yml build
docker-compose -f docker-compose.yml -f docker-compose.override.yml -f docker-compose.vs.release.yml up -d