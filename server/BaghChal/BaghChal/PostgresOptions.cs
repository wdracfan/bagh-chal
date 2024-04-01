public class PostgresOptions
{
    public string ConnectionString { get; set; }
}

/*
docker run --rm --detach --name postgres --publish 5432:5432 --env "POSTGRES_DB=baghchal" --env "POSTGRES_USER=postgres" --env "POSTGRES_PASSWORD=postgres" postgres
*/

