# UbuntuService
basic service on ubuntu with rabbitmq
All is fine, but here is no pingToAll infrastructure(it's in RabbitMQReceiver project on my git)
Schema for this solution:

- UbuntuService.exe project is MasterClient. It work like cycle while(true) or like service on ubuntu. Has some DataProvider(Entity Framework Core(nuget)), also Logger class. It's necessary to have RabbitMQ.Client(nuget). And need Sharing.dll

- Sharing.dll(serializator, DTO - same class which will be to use on both sides(Clien and MasterClient))

- API.dll client's side dll(output 3 dll for client app:
  1)RabbitMQ.Client(nuget) 
  2)Sharing.dll 
  3)API.dll - based on RabbitMQ.Client.dll(nuget) and Sharing.dll. It has switch and ping logic etc.
