GoogleStorageCmdlets
===================

A PowerShell Cmdlet for interacting with the [Google Storage API](https://cloud.google.com/storage/docs/json_api/) (similar in intent to their [gsutil](https://cloud.google.com/storage/docs/gsutil]) python utility)

Hopefully this will be useful for people who need to manage Google Storage in windows environments as part of application deployments or other back end tasks.

Working:
- Client configuration (ClientId, ClientSecret, Project set up and persistence)
- Unauthenticated access to public buckets
- OAuth2 workflow
- Authenticated access to buckets
- bucket export (authenticated and unauthenticated)

TODO:
- Object cmdlets
- bucket import
- single object Import/Export (i.e. upload/download)
- ACL management
