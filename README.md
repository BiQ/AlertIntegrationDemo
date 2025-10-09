# Alert Integration Demo

_Code examples for 3rd party developers integrating to the [BiQ Alert](https://biq.dk/biqs-loesninger/biq-alert/) system_

## High-level context

- The customer has a functional CRM installation. To simplify matters, we use a SQLite database as a stand-in for the customer CRM.
- The customer wants to set up an automatic two-way sync between their CRM and BiQ Alert. At BiQ we call this a "shadow database", as it shadows what happens in the customer CRM. 
- BiQ Alert is now able to notify the customer CRM automatically of important changes to their users.
- Manually or automatically accepted changes are reflected automatically in the customer CRM.

## Functionality

- Maintaining a shadow database of the customer CRM at BiQ
- Retrieving updates to the customer CRM from BiQ
- Retrieving notifications about customer changes from BiQ

## Introduction 
A demo Visual Studio Solution for integration with BiQ CustomerShadow and AlertChanges API

It is a minimal solution for 2-way integration, only the most common fields are used.

## Getting Started with the demo
1. Get an API-key, `tenantId` and `shadowSourceId` from your contact person at BiQ. Write them into the [ConfigValues.cs](AlertIntegrationDemo/ConfigValues.cs) file.
2. Run CustomerSystemInitializer, our fake CRM; this creates a SQLite database in a file. There is just one table: `Customers` and it is populated with 5 test customers.
   ```
   $ dotnet run --project CustomerSystemInitializer
   ```
   You can see the customers in the "CRM" like this:
   ```
   $ echo "select * from customers" | sqlite3 customersystem.db
   10001||2024-01-02|||erhverv||The Pizza Company ApS|1||40563350||||||||||Sværtegade||11||||København K|1118||||||||||||
   2||2024-01-03|||erhverv||Pico Pizza Nørrebro ApS|1||39972190||||||||||Skyttegade||3||||København N|2200||||||||||||
   ...snip...
   ```
3. Start ShadowWriter. This will send all (5) customers to CustomerShadow at BiQ, and it will keep posting updates to CustomerShadow when the customers in the dummy customer system are modified. Keep this running.
    ```
    $ dotnet run --project ShadowWriter
    Starting Shadow maintainer
    Got 5 modified customers. New timestamp:...
    ```
4. Start ChangeReader. This will read proposed changes to the test customers from BiQ's ApprovedChanges. Keep this running.
   ```
   $ dotnet run --project ChangeReader
   Starting Change Reader
   Local storage file for bookmark not found A new default file (changereaderstorage.json) has been created.
   Read 0 changes. New bookmark
   ```
5. Start NotificationReader. This will read notifications about the test customers from BiQ. Keep this running.
   ```
   $ dotnet run --project NotificationReader
   Starting Notification Reader
   Local storage file for bookmark not found A new default file (notificationreaderstorage.json) has been created.
   Customer (00005) is dead! - date and time of death: 9/30/2018 12:00:00 AM
   Got 20 notifications. New bookmark: 10/9/2025 4:41:04 PM +02:00
   ```

Now you have a running demo of a 2-way integration between a customer system and BiQ Alert. 
You can visit the BiQ Alert UI to see the customers in CustomerShadow and to propose changes to them. 
You can also see notifications about the customers.
If you make changes to the customers in the dummy customer system, these changes will be posted to BiQ Alert, see the `sqlite3` command above for ideas of how to do this.

By experimenting with each of the pieces, you should be able to develop an intuition of how a real integration is put together.

## Developing your own integration

You need to implement 3 parts: 

* A Shadow Maintainer. 

This proccess should post all new customers to BiQ's customerShadow and put all modified customers to BiQ's customerShadow. The proccess should also delete customers from BiQ's customerShadow when a customer are deleted from the master system. And be able to post all customers to BiQ's customerShadow at integration start or at integration restart.

The documentation for the customerShadow API can be found her [customerShadow API](https://alert.biq.dk/swagger-ui/#!/Shadow32Customers/CustomerPostRequesttenantidshadowsourcesshadowsourceidcustomers_Post)

In this demo most of the Shadow Maintainer funtionality is in the file /ShadowWriter/Program.cs

* A Change Reader. 
 
This process should retrieve approved changes from BiQ and update the customer system accordingly. The proccess shold keep track of which changes has been processed by storing a bookmark or a timestamp.

The documentation for the approvedChanges API can be found her [approvedChanges API](https://alert-changes.biq.dk/swagger-ui/)

In this demo most of the Change Reader funtionality is in the file /ChangeReader/Program.cs

* A Notification Reader.

This process should retrieve notifications from BiQ and take appropriate action on relevant notification types. The proccess shold keep track of which notifications has been processed, by storing a bookmark or a timestamp.

In this demo most of the Notification Reader funtionality is in the file /NotiicationReader/Program.cs
