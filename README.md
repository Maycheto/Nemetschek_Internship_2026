# Nemetschek_Internship_2026

Shkolo 2.0

## Branch Onboarding: Run And Build With Docker

This project runs with Docker Compose from the NemeBook folder.

### 1. Prerequisites

- Docker Desktop installed and running
- Docker Compose available (included in modern Docker Desktop)
- Port 1433 available on your machine
- Port 8081 available on your machine (default host port for the web app)

### 2. First-Time Setup

1. Open a terminal in the repository root.
2. Go to the compose folder:

```powershell
cd NemeBook
```

3. Confirm the environment file exists:

```powershell
Get-ChildItem .env
```

4. If needed, edit .env values before first run:
- MSSQL_SA_PASSWORD
- WEB_HOST_PORT (default: 8081)
- ASPNETCORE_ENVIRONMENT

### 3. Build And Start Containers

Run this from NemeBook:

```powershell
docker compose up --build -d
```

This starts:
- mssql on host port 1433
- web on host port WEB_HOST_PORT (default 8081)

### 4. Verify Everything Is Running

Check service status:

```powershell
docker compose ps
```

Verify web is reachable:

```powershell
Invoke-WebRequest -UseBasicParsing http://localhost:8081 | Select-Object StatusCode,StatusDescription
```

Expected result: 200 OK

### 5. Rebuild After Code Changes

```powershell
docker compose up --build -d
```

### 6. View Logs

Web logs:

```powershell
docker compose logs web --tail 100
```

SQL logs:

```powershell
docker compose logs mssql --tail 100
```

### 7. Stop Containers

```powershell
docker compose down
```

### 8. Reset Database Volume (Destructive)

Use this only when you need a clean SQL state:

```powershell
docker compose down -v
```

### Troubleshooting

- If web fails to start with a port bind error, change WEB_HOST_PORT in .env (for example 8082), then run docker compose up --build -d again.
- If SQL is slow on first boot, wait until mssql is healthy in docker compose ps.
- If startup fails after pulling latest changes, run docker compose down, then docker compose up --build -d.

## Deploy to DigitalOcean Droplet (ASP.NET + MSSQL)

This repository includes:
- .github/workflows/deploy-do-droplet.yml
- NemeBook/Mssql/Dockerfile
- NemeBook/deploy/docker-compose.droplet.yml

### 1. Create the Droplet

1. Create a new Ubuntu Droplet (the $5/month plan is enough for a starter setup).
2. Add your SSH public key during Droplet creation.
3. Enable Docker during provisioning (DigitalOcean docker cloud-init recipe style). Example:

```yaml
#cloud-config
package_update: true
packages:
	- docker.io
	- docker-compose-plugin
runcmd:
	- systemctl enable docker
	- systemctl start docker
	- mkdir -p /home/root/nemebook
```

### 2. Create DOCR

1. In DigitalOcean, create a private Container Registry.
2. Set DOCR_NAMESPACE in .github/workflows/deploy-do-droplet.yml to your registry namespace.

### 3. Add GitHub Secrets

Add these repository secrets:
- DOCREDS (DigitalOcean registry token/password)
- DOCREDS_USER (registry username)
- DROP_HOST (Droplet public IP)
- DROP_SSH_KEY (private key matching the Droplet SSH key)

### 4. Prepare Droplet Runtime Env File

Create /home/root/nemebook/.env on the Droplet:

```bash
MSSQL_SA_PASSWORD=UseARealStrongPasswordHere
WEB_HOST_PORT=8081
ASPNETCORE_ENVIRONMENT=Production
APP_BASE_URL=http://<droplet-ip>:8081
```

### 5. Deployment Flow

On every push to main, GitHub Actions will:
- Build and push nemebook-web and nemebook-mssql images to DOCR.
- Copy NemeBook/deploy/docker-compose.droplet.yml to /home/root/nemebook.
- SSH into the Droplet.
- Login to DOCR, pull latest images, and restart services via Docker Compose.

### 6. Verify on Droplet

```bash
cd /home/root/nemebook
docker compose -f docker-compose.droplet.yml ps
docker compose -f docker-compose.droplet.yml logs web --tail 100
```
