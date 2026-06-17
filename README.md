# Rx_Patient_PrescriptionSync
 Rexall needs to allow automated fulfillment software to pull a patient's prescription data from a central microservice.

The Problem: 
Rexall needs to allow automated fulfillment software (like automated pill-packers, inventory bots, or AI drug interaction scanners) to pull a patient's prescription data from a central microservice. 
Because this data is protected by strict healthcare laws (like PIPEDA in Canada or HIPAA), a standard login screen won't work.

We must secure this machine-to-machine connection using an OAuth 2.0 Client Credentials model.

:)

To test The application
1. Step 1: Trigger the Firewall (Expect 401)Change the URL in your browser's address bar to look exactly like this and press

 Enter:👉 http://localhost:5042/api/v1/pharmacy/dispense-queueWhat 
 
 The screen will display "Rexall Gateway Error: Missing Authorization Header.
 
Custom IdentityGatewayMiddleware is completely active and successfully blocking unauthenticated access to the patient database!

 2. Step 2:  Get a Security TokenNow, let's pretend to be an authorized Rexall internal service and request a valid security token. 
 
 Navigate to this URL in a new browser tab:👉 http://localhost:5042/api/get-token

A raw text JSON string that looks like this:{"token":"eyJhbGciOiJIUzI1NiIsIn..."}

 
Step 3: Unlock the Gateway Because web browsers cannot easily attach custom security headers to a standard URL click, you have two choices to complete your final test:

Option A (Easiest): Open a tool like Postman,

 create a new GET request to http://localhost:5042/api/v1/pharmacy/dispense-queue,
 

  go to the Authorization tab, select Bearer Token, and paste that long token string in.

  
  Option B (Fastest): Open your powershell, paste this command, and press Enter:

PS C:\Users\ABaldeviso> $headers = @{ "Authorization" = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJyb2xlIjoiQXV0b21hdGVkRGlzcGVuc2VyQm90IiwibG9jYXRpb25faWQiOiJTdG9yZS1NaXNzaXNzYXVnYS0wNDEyIiwic2NvcGUiOiJwcmVzY3JpcHRpb25zLnN5bmMiLCJuYmYiOjE3ODE3MjEzNDcsImV4cCI6MTc4MTcyMzE0NywiaWF0IjoxNzgxNzIxMzQ3LCJpc3MiOiJSZXhhbGxJZGVudGl0eVNlcnZlciIsImF1ZCI6IlJleGFsbEdhdGV3YXkifQ.1VtfkhG5sgtZt1dwrjdVFpkikgX1roY_2h93iGYAkKE" }
PS C:\Users\ABaldeviso> Invoke-RestMethod -Uri "http://localhost:5042/api/v1/pharmacy/dispense-queue" -Headers $headers


This is the updated test how to generate Bearer JWT dynamically.

# 1. Ask for a dynamic token for Store-Toronto-0999 with "prescriptions.sync" scope
$body = @{
    ClientId = "RexallBot-01"
    ClientSecret = "CRITICAL_PHARMACY_SECRET_KEY_KEEP_THIS_LONG_AND_SAFE_2026!"
    RequestedLocation = "Store-Toronto-0999"
    RequestedScope = "prescriptions.sync"
} | ConvertTo-Json

$tokenResponse = Invoke-RestMethod -Uri "http://localhost:5042/api/get-token" -Method Post -Body $body -ContentType "application/json"

# 2. Extract and view your brand-new dynamic token response
$tokenResponse 
