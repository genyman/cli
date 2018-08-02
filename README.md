# Genyman
Genyman CLI - Code Generator Tool
## Getting Started
Genyman is a **[genyman](http://genyman.net)** code generator. If you haven't installed **genyman** run following command:
```
dotnet tool install -g genyman
```
_Genyman is a .NET Core Global tool and thereby you need .NET Core version 2.1 installed._
## New Configuration file 
```
genyman new
```
## Sample Configuration file 
```
{
    "genyman": {
        "packageId": "Genyman",
        "info": "This is a Genyman configuration file - https://genyman.github.io/docs/"
    },
    "configuration": {
        "prefix": "YourPrefix",
        "toolName": "YourToolName",
        "description": "Another great Genyman generator!"
    }
}
```
## Documentation 
### Class Configuration
| Name | Type | Req | Description |
| --- | --- | :---: | --- |
| Prefix | String | * | The prefix of your Genyman package; your name, company, identifier for Nuget |
| ToolName | String | * | The name of the tool |
| Description | String | * | A description of what the tool does |
