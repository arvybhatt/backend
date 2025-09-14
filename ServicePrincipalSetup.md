# Using Service Principal Authentication for Key Vault

For security reasons, store your Service Principal credentials in user secrets during development, not in appsettings.Development.json.

## 1. Add your Service Principal details to user secrets

```powershell
# Initialize user secrets (run once)
dotnet user-secrets init --project C:\Users\Arvind.Bhatt\Documents\Personal-Repos\Backend\Backend.csproj

# Set your Service Principal details
dotnet user-secrets set "ServicePrincipal:TenantId" "your-tenant-id" --project C:\Users\Arvind.Bhatt\Documents\Personal-Repos\Backend\Backend.csproj
dotnet user-secrets set "ServicePrincipal:ClientId" "your-client-id" --project C:\Users\Arvind.Bhatt\Documents\Personal-Repos\Backend\Backend.csproj
dotnet user-secrets set "ServicePrincipal:ClientSecret" "your-client-secret" --project C:\Users\Arvind.Bhatt\Documents\Personal-Repos\Backend\Backend.csproj
```

## 2. Make sure your Service Principal has permissions

Your service principal needs access to read secrets from your Key Vault.

```powershell
# Get your service principal object ID
$objectId = az ad sp show --id your-client-id --query id -o tsv

# Grant permission to read secrets
az keyvault set-policy --name your-keyvault-name --object-id $objectId --secret-permissions get list
```

## 3. Add the secret to Key Vault

Make sure your Key Vault has the Service Bus connection string stored as a secret:

```powershell
az keyvault secret set --vault-name your-keyvault-name --name "ServiceBusConnectionString" --value "Endpoint=sb://servicebus-arvind.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=FzW+eePv8OmgLwjp9f4wRrShxn3YgtkPr+ASbBnkUZc="
```

## 4. Update your appsettings.Development.json

Replace placeholders with your actual values. Do not commit this file with real values.

## 5. Verify Key Vault access

You can verify your service principal has access to Key Vault by running:

```powershell
az login --service-principal -u your-client-id -p your-client-secret --tenant your-tenant-id
az keyvault secret show --vault-name your-keyvault-name --name ServiceBusConnectionString
```
