#!/bin/bash
docker run --rm -dp 8081:80 --name stfileserver -v ./db_scripts/data/icons:/usr/share/nginx/html nginx


