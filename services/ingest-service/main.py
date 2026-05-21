import os
from fastapi import FastAPI
from fastapi_health import health
import psycopg2

app = FastAPI(title="ingest-service")

DB_HOST = os.environ.get("POSTGRES_HOST", "")
DB_USER = os.environ.get("POSTGRES_USER", "")
DB_PASSWORD = os.environ.get("POSTGRES_PASSWORD", "")
DB_NAME = os.environ.get("POSTGRES_DB", "")
DB_PORT = os.environ.get("POSTGRES_PORT", "")

def is_database_online():
    try:
        conn = psycopg2.connect(
            host=DB_HOST, user=DB_USER, 
            password=DB_PASSWORD, dbname=DB_NAME, connect_timeout=3
        )
        conn.close()
        return True
    except Exception:
        return False

# Automatically responds with 200 if True, or 503 if False
app.add_api_route("/health", health([is_database_online]))

@app.post("/upload")
def upload_stub() -> dict:
    return {"message": "upload stub — not yet implemented"}