# Rx_Patient_PrescriptionSync
 Rexall needs to allow automated fulfillment software to pull a patient's prescription data from a central microservice.

The Problem: 
Rexall needs to allow automated fulfillment software (like automated pill-packers, inventory bots, or AI drug interaction scanners) to pull a patient's prescription data from a central microservice. 
Because this data is protected by strict healthcare laws (like PIPEDA in Canada or HIPAA), a standard login screen won't work.

We must secure this machine-to-machine connection using an OAuth 2.0 Client Credentials model.

 
