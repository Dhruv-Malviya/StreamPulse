from fastapi import FastAPI

app = FastAPI(title="ingest-service")


@app.get("/health")
def health() -> dict:
    return {"status": "ok", "service": "ingest-service"}


@app.post("/upload")
def upload_stub() -> dict:
    return {"message": "upload stub — not yet implemented"}