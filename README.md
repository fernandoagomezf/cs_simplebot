# Bot conversacional con reconocimiento de intenciones.

Este es un bot conversacional que puede reconocer la intención que expresa el usuario durante una conversación natural, a partir de un catálogo de intenciones predefinido. La aplicación es un servicio web sobre ASP.NET Core, y que utiliza el Microsoft Bot Framework para gestionar y mantener el estado de la conversación.

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)
![C#](https://img.shields.io/badge/C%23-13.0-239120?logo=c-sharp)
![Bot](https://img.shields.io/badge/Microsoft_Bot_Framework-0078D4?logo=dependabot&logoColor=white)
![License](https://img.shields.io/badge/License-MIT-green)

## Características

La aplicación, al ser ejecutada, permite interactuar con un servicio web en la URL ```http://localhost:3978/api/messages```, y enviar y recibir conversaciones utilizando el protocolo definido por el Microsoft Bot Framework. 

Alternativamente, se puede utilizar el Bot Framework Emulator para interactuar en formato de conversación.

![Img-1](blob/main/docs/bot_confident.png)

Una vez conectado al emulador, el bot responderá con un mensaje genérico de bienvenida, tras lo cual, le solicitará al usuario que ingrese una acción a realizar. El usuario podrá escribir un texto solicitando algo, tras lo cual el bot responderá con alguna de dos opciones: 

* Si el bot reconoce la intención, con un porcentaje de confidencia superior al 70%, le indicará al usuario que ha entendido la intención con éxito, y la mostrará en negritas, concluyendo la conversación.
* Si el bot no reconoce la intención, o el porcentaje de confidencia es inferior al 70%, le mostrará al usuario las cinco intenciones más probables (ordenadas del mayor al menor por porcentaje de confidencia), y le pedirá que elija a la que se refiere, o que escriba "ninguna". 
    - Si el usuario ingresa un número del 1 al 5, el bot agradece e internamente guarda el texto relacionado con la intención seleccionada por el usuario, para "aprender" a reconocerla en futuras interacciones. 
    - Si el usuario ingresa "ninguna", el bot se disculpa y concluye la conversación. 

![Img-2](blob/main/docs/bot_nonconfident.png)

El bot utiliza un catálogo de intenciones, el cual se almacena en una base de datos, y el cual puede ser definido y extendido modificando el contenido de esta tabla. Asimismo, se utiliza una tabla de declaraciones (_utterance_) en donde hay ejemplos de texto relacionado con intenciones, y que se usa para entrenar al bot. 

## Arquitectura

La aplicación utiliza la estructura general provista por el Microsoft Bot Framework. Esto incluye:

* Una aplicación en ASP.NET Core 9, la cual es la encargada de proveer la infraestructura de la aplicación para interactuar en el contexto de un servicio web (WebAPI). 
* Un bot registrado en Azure, que se encarga de gestionar el diálogo y mantener el estado entre interacciones. 
* Un diálogo, el cual se encarga de elegir las conversaciones en modo cascada (es decir, una tras otra de forma secuencial) dependiendo del estado, y reaccionar ante el texto ingresado por el usuario. 

![Img-3](blob/main/docs/architecture.png)

Adicionalmente, se añaden componentes adicionales que realizan la tarea de detección de la intención. 

* Base de datos en Microsoft SQL Server, el cual contiene dos tablas:
    - Intents - almacena las intenciones reconocidas
    - Utterances - almacena ejemplos de declaraciones o conversaciones (_features_) que están ya asociados a una intención (_target_). 
* Clasificador - Un sencillo clasificador de texto que realiza las siguientes tareas:
    - Preprocesamiento - normaliza textos y remueve palabras vacías (_stop words_).
    - Entrenamiento - utiliza la tabla Utterances para entrenar al clasificador, bajo el algoritmo de clasificador bayesiano ingenuo, que consiste en contar las palabras, asumiendo que éstas son independientes entre sí y calcular la probabilidad con base en la frecuencia de las mismas. 
    - Predicción - clasifica un texto (_utterance_) con base en los datos entrenados, y genera un porcentaje de confidencia (probabilidad), tras lo cual obtiene la intención con la confidencia más alta. 

## Base de datos

La base de datos es muy sencilla, sol contiene dos tablas: 

* Intents - catálogo de intenciones
* Utterances - datos de entrenamiento (con _features_ y _targets_).

La base de datos ha sido modelada utilizando Entity Framework Core, y se puede utilizar migraciones (EF Core Migrations) para generarla. 

1. En primer lugar, compilar la aplicación. En este caso, usamos Visual Studio Code, por lo que es necesario ejecutar lo siguiente desde la paleta de comandos (asumiendo que se tienen intaladas las extensiones de C#);

    ```
    Ctrl+Shift+P
    > .NET: Build
    ```

2. Crear una base de datos vacía en tu instalación de Microsoft SQL Server. En mi caso, utilizo LocalDB, una instancia local para desarrollo. 

3. Hay que ajustar la cadena de conexiones. En el archivo ``appsettings.json``, en caso de ser necesario, modificar la cadena BotContext. 

    ```
    "ConnectionStrings": {
        "BotContext":"Server=(LocalDb)\\MSSQLLocalDB;Trusted_Connection=True;Database=botdemo"
    },
    ```

4. Ejecutar el comando de migraciones para actualizar la base de datos. 

    ```
    dotnet ef database update --context BotContext
    ```

5. Cargar la información. La aplicación no tiene una interfaz para cargar o modificar información a la base de datos, así que tendrás que cargarla manualmente. Sin embargo, puedes usar estos dos archivos CSV para importar los datos y tener un punto de partida. 
    - [IntentData.csv](blob/main/data/IntentData.csv)
    - [UtteranceData.csv](blob/main/data/UtteranceData.csv)

## Despliegue

La aplicación se puede ejecutar directamente desde el Bot Framework Emulator. Una vez instalada (ver enlaces importantes), abre la aplicación y haz clic en "Open bot". Tras lo cual, aparecerá una ventana de diálogo que te solicitará inforamción. Para probar en tu ambiente local, solo necesitas la URL de tu localhost: 

![](blob/main/docs/bot_connect.png)

Para publicar el bot hacia un ambiente Microsoft Azure, consulta los siguientes documentos:

* [Despliegue hacia un grupo de recursos nuevo](IntentBot/DeploymentTemplates/DeployWithNewResourceGroup/readme.md)
* [Despliegue hacia un grupo de recursos existente](IntentBot/DeploymentTemplates/DeployUseExistResourceGroup/readme.md)

## Enlaces importantes

Estos son algunos enlaces importantes hacia herramientas y librerías. 

* [Documentación de Microsoft Bot Framework](https://learn.microsoft.com/en-us/azure/bot-service/index-bf-sdk?view=azure-bot-service-4.0)
* [Emulador de Microsoft Bot Framework](https://github.com/microsoft/BotFramework-Emulator)
* [Migraciones de Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/?tabs=dotnet-core-cli)
* [Microsoft SQL Server Express LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb?view=sql-server-ver17)