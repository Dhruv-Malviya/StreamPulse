param(
    [string]$service,
    [string]$action,
    [int]$port = 0
)

$defaults = @{
    "user"    = 8080
    "catalog" = 8081
    "ingest" = 8082
}

if ($port -eq 0) { $port = $defaults[$service] }

switch ($action) {
    "build" {
        docker build -t "$service-service:dev" "./services/$service-service"
    }
    "run" {
        docker run --rm -p "${port}:8080" "$service-service:dev"
    }
    "build-all" {
        foreach ($svc in $defaults.Keys) {
            docker build -t "$svc-service:dev" "./services/$svc-service"
        }
    }
    "stop" {
        docker ps -q | ForEach-Object { docker stop $_ }
    }
}