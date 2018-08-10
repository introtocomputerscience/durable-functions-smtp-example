# Project Title

This example demonstrates how to use Durable Functions to send a confirmation email via SMTP(Example using gmail). The user has 15 minutes to confirm or deny the email by clicking a link. The link in the email contains a code that is validated server side to ensure it was a valid click. This could easily be implemented to integrate within registration for a website or application.

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. See deployment for notes on how to deploy the project on a live system.

### Prerequisites

- Visual Studio 2017
- Azure Functions and Web Jobs Tools Visual Studio Extension
- Azure Storage Emulator
- CURL or PostMan

### Installing

Clone the repository to your local machine

Open in Visual Studio and add a local.settings.json file with the following contents:

```
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "AzureWebJobsDashboard": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "DisplayName": "John Smith",
    "SMTPUsername": "yoursmtpuser@gmail.com",
    "SMTPPassword": "smtppassword",
    "SMTPHost": "smtp.gmail.com",
    "SMTPPort": "587"
  }
}
```
**Note:** If you are using Gmail then you need to do the following:
- Sign into Gmail. 
- Go to the “Less secure apps” section in My Account. 
- Next to “Allow less secure apps: OFF,” select the toggle switch to turn on. (Note to G Suite users: This setting is hidden if your administrator has locked less secure app account access.)

Run the project in Visual Studio and make a request using your favorite tool like PostMan or CURL
```
POST http://localhost:7071/api/orchestrators/RequestApproval
{
	"Requestor": {
		"FirstName": "CJ",
		"LastName": "van der Smissen"
	},
	"Email": "test@test.com"
}
```

## Deployment

- Right click the project and select Publish
- Follow the prompts to publish in Azure
- Login to Azure Portal and update the application settings to the appropriate values you previously used in local.settings.json

## Built With

* [Azure Functions](https://azure.microsoft.com/en-us/services/functions/) - Platform
* [Durable Task](https://github.com/Azure/durabletask) - Framework
* [AzureFunctions.Autofac](https://github.com/introtocomputerscience/azure-function-autofac-dependency-injection) - Dependency Injection

## Authors

* **CJ van der Smissen** - *Initial work* - [Intro to Computer Science](https://github.com/introtocomputerscience)

See also the list of [contributors](https://github.com/introtocomputerscience/durable-functions-smtp-example/contributors) who participated in this project.

## License

This project is licensed under the GPL License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments

* Thanks to [Paco de la Cruz](https://twitter.com/pacodelacruz) for an initial outline of [Approval Workflow using SendGrid](https://blog.mexia.com.au/azure-durable-functions-approval-workflow-with-sendgrid)

