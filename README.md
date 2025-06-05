# Alert Integration Demo

_Code examples for 3rd party developers integrating to the BiQ Alert system_

## Functionality

- Maintaining a shadow data base of the customer at BiQ
- Retieving updates to the customers from BiQ
- Retieving notifications about the customers from BiQ

## Introduction 
A demo Visual Studio Solution for integration with BiQ CustomerShadow and AlertChanges API

It is a minimal solution for 2-way integration, only the most obvious fields are used.

## Getting Started
1. Get a API-key, tenantId and shadowSourceId from your contact person at BiQ. Write them into the 'ConfigValues.cs' file in the commen AlertIntegrationDemo project.

2. Create a dummy customer system as a stand in for your production customer management system. This is done by starting CustomerSystemInitializer; this createase a SQLite database in a file - it has one table: Customers and it is populatede with 5 test customers. (The file location can be changed using the App.config)

3. Start ShadowWriter. This will send all (5) customers to CustomerShadow at BiQ, and it will keep posting updates to CustomerShadow when the customers in the dummy customer system are modified. Keep this running.

4. Start ChangeReader or ChangeCallbackReceiver. This will read proposed changes to the test customers from BiQ's ApprovedChanges. Keep it running.
   I. ChangeReader will read changes at a regular interval (e.g. every minute)
   II. ChangeCallbackReceiver is a REST site that will get called by BIQ when changes are approved. You will need to register the URL at BiQ ([approvedChanges API](https://alert-changes.biq.dk/swagger) posting to the 'subscribe-to-approvedchanges' method tagged with 'Optional Integration Helpers')

5. Start NotificationReader. This will read notifications about the test customers from BiQ. Keep this running.

## Developing your own integration

You need to implement 3 parts: 

* A Shadow Maintainer. 

This proccess should post all new customers to BiQ's customerShadow and put all modified customers to BiQ's customerShadow. The proccess should also delete customers from BiQ's customerShadow when a customer are deleted from the master system. And be able to post all customers to BiQ's customerShadow at integration start or at integration restart.

The documentation for the customerShadow API can be found her [customerShadow API](https://alert.biq.dk/swagger)

In this demo most of the Shadow Maintainer funtionality is in the file /ShadowWriter/Program.cs

* A Change Reader or a webhook that receives callbacks from BiQ. 
 
This process should retrieve approved changes from BiQ and update the customer system accordingly. The proccess shold keep track of which changes has been processed by storing a bookmark or a timestamp.

The documentation for the approvedChanges API can be found her [approvedChanges API](https://alert-changes.biq.dk/swagger)

In this demo most of the Change Reader funtionality is in the file /ChangeReader/Program.cs and /ChangeReader/ReadingHelper.cs

And for the ChangeCallbackReceiver demo most of the functionality is in the file /ChangeCallbackReceiver/Program.cs and /ChangeReader/ReadingHelper.cs

* A Notification Reader.

This process should retrieve notifications from BiQ and take appropriate action on relevant notification types. The proccess shold keep track of which notifications has been processed, by storing a bookmark or a timestamp.

In this demo most of the Notification Reader funtionality is in the file /NotiicationReader/Program.cs
