# ────────────────────────────────────────────────────────────────────────────
# Stage 1 — builder
# ────────────────────────────────────────────────────────────────────────────
FROM python:3.12-slim AS builder

WORKDIR /build

# Create the virtualenv that the runtime stage will inherit.
RUN python -m venv /opt/venv

# Activate it for all subsequent RUN commands.
ENV PATH="/opt/venv/bin:$PATH"

# Copy requirements first so Docker can cache this layer.
# A code-only change won't reinstall packages — same principle as
# `dotnet restore` before `COPY . .` in the .NET Dockerfiles.
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt


# ────────────────────────────────────────────────────────────────────────────
# Stage 2 — runtime
# ────────────────────────────────────────────────────────────────────────────
FROM python:3.12-slim AS runtime

WORKDIR /app

# Run as a non-root user — same discipline as the .NET services.
RUN adduser --disabled-password --gecos "" appuser
USER appuser

# Pull in the pre-built virtualenv from the builder stage.
COPY --from=builder /opt/venv /opt/venv

# Activate the venv so Python resolves packages from it.
ENV PATH="/opt/venv/bin:$PATH"

# Copy application source.
COPY main.py .

EXPOSE 8080

# uvicorn is the ASGI server; --host 0.0.0.0 makes it reachable inside Docker.
ENTRYPOINT ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8080"]