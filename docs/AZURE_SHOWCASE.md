# Azure Showcase & Deployment Guide (V5)

This guide documents the optional, cost-controlled deployment showcase for the `dotnet-blazor-multihost-starter` Web/API host and PostgreSQL database using Azure Infrastructure as Code (Bicep) and GitHub Actions.

---

## 1. Cost Estimation & Subscription Guardrails

> **Azure for Students Context:** Azure for Students provides a credit limit (typically $100 for 12 months) and access to select free services. To avoid unexpected charges or draining the student credit:
> - Run the showcase **on-demand**.
> - Execute the **Teardown** command once the demonstration or smoke test is completed.

| Resource | Service & SKU | Estimated Cost | Notes |
|---|---|---|---|
| **Web & API Host** | Azure App Service (Linux), **Basic B1** | ~$13.00 / month (~$0.018 / hr) | Supports custom domains, SSL, Always-On, and .NET 10. |
| **Database** | Azure Database for PostgreSQL Flexible Server, **Burstable B1ms** (32 GB storage) | ~$22.00 / month (~$0.030 / hr) | Can be stopped when not in use to halt compute charges. |
| **Monitoring** | Log Analytics Workspace & Application Insights | ~$0.00 (under 5 GB/mo free tier) | Retention set to 30 days. |
| **Networking & Bandwidth** | Egress bandwidth | ~$0.00 (under 100 GB/mo free tier) | HTTPS enforced. |
| **Total Continuous** | | **~$35.00 / month** | If left running 24/7. |
| **Total Ephemeral Demo** | (2 hours active testing) | **~$0.10 total** | Highly recommended model for CV/learning! |

---

## 2. Architecture Overview

```text
GitHub Actions (OIDC Federated Credential)
  │
  ├─> 1. Provision Bicep IaC
  │     ├── Resource Group: rg-blazor-multihost-showcase (e.g. in polandcentral)
  │     ├── Log Analytics & Application Insights
  │     ├── PostgreSQL Flexible Server (Burstable B1ms)
  │     └── App Service Plan (Basic B1) & Linux Web App
  │
  ├─> 2. Idempotent Database Migration
  │     └── dotnet ef migrations script --idempotent ──> psql against Azure PostgreSQL
  │
  ├─> 3. Deploy Web/API App
  │     └── dotnet publish ──> Azure App Service
  │
  └─> 4. Health Probe Verification
        ├── GET /healthz (200 OK)
        └── GET /health/ready (200 OK after checking database connection)
```

---

## 3. Deployment Methods

### Option A: Via GitHub Actions (Recommended)

1. **Configure Azure OIDC Federation in GitHub**:
   In your GitHub repository settings under **Settings** > **Secrets and variables** > **Actions**, add:
   - `AZURE_CLIENT_ID`: App registration Application (Client) ID.
   - `AZURE_TENANT_ID`: Microsoft Entra Tenant ID.
   - `AZURE_SUBSCRIPTION_ID`: Azure Subscription ID.
   - `AZURE_POSTGRES_PASSWORD`: Strong password for `starter_admin`.
   - `ENTRA_TENANT_ID`, `ENTRA_API_CLIENT_ID`, `ENTRA_WEB_CLIENT_ID`, `ENTRA_MAUI_CLIENT_ID`: Entra application identifiers.

2. **Trigger the Workflow**:
   - Go to the **Actions** tab in GitHub.
   - Select the **Deploy to Azure Showcase (V5)** workflow.
   - Click **Run workflow**, choose the target region (default `polandcentral`), and click **Run workflow**.

---

### Option B: Via Local Azure CLI & Bicep

If you have `az` CLI installed locally:

1. **Login and set subscription**:
   ```bash
   az login
   az account set --subscription "<YOUR_SUBSCRIPTION_ID>"
   ```

2. **Create Resource Group**:
   ```bash
   az group create --name rg-blazor-multihost-showcase --location polandcentral
   ```

3. **Deploy Bicep Template**:
   ```bash
   az deployment group create \
     --resource-group rg-blazor-multihost-showcase \
     --template-file infra/main.bicep \
     --parameters \
       environmentName="showcase" \
       dbAdminPassword="<YOUR_STRONG_PASSWORD>" \
       entraTenantId="<TENANT_ID>" \
       entraApiClientId="<API_CLIENT_ID>" \
       entraWebClientId="<WEB_CLIENT_ID>" \
       entraMauiClientId="<MAUI_CLIENT_ID>"
   ```

4. **Deploy Database Schema**:
   ```bash
   dotnet ef database update \
     --project src/BlazorMultihostStarter.Infrastructure \
     --startup-project src/BlazorMultihostStarter.Web \
     --context BlazorMultihostStarter.Infrastructure.Data.StarterDbContext \
     --connection "Host=<PG_SERVER_FQDN>;Port=5432;Database=starter_db;Username=starter_admin;Password=<YOUR_STRONG_PASSWORD>;SSL Mode=Require;Trust Server Certificate=true"
   ```

5. **Deploy Application**:
   ```bash
   dotnet publish src/BlazorMultihostStarter.Web -c Release -o ./publish
   az webapp deploy \
     --resource-group rg-blazor-multihost-showcase \
     --name "<APP_SERVICE_NAME>" \
     --src-path ./publish.zip \
     --type zip
   ```

---

## 4. Post-Deployment Verification

1. **Check Live Probe**:
   ```bash
   curl -i https://<app-name>.azurewebsites.net/healthz
   ```
   Expected response: `200 OK` (`Healthy`).

2. **Check Database Readiness Probe**:
   ```bash
   curl -i https://<app-name>.azurewebsites.net/health/ready
   ```
   Expected response: `200 OK` (`Healthy`).

3. **Open Application**:
   Navigate to `https://<app-name>.azurewebsites.net` in the browser.

---

## 5. Teardown Procedure (Cost Control)

To stop all recurring charges immediately after testing:

```bash
az group delete --name rg-blazor-multihost-showcase --yes --no-wait
```

Deleting the resource group removes the App Service, PostgreSQL instance, Application Insights, and all associated compute/storage resources.
