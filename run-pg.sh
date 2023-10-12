docker run -dp 3241:5432\
    --name my-pg\
    --network my-ntw\
    --env-file ./.env\
    postgres
docker run -dp 8080:8080\
    --name my-pg-adminer\
    --network my-ntw\
    --env-file ./.env\
    adminer