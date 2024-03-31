# Запуск проекта 

### В данном проекте в файле appsettings.json поменять в строке {"ConnectionString": "Server=localhost;Port=5432;Database=ApplicationsDataBase;User id=postgres;Password=1928"} значение Password на значение пароль от pgAdmin вашего устройства
#### Password=здесь_указывается_пароль_от_postgres

### Открыть папку проекта (папку в которой хранится CollectingApplicationsAPI.csproj) и открыть из неё PowerShell

### Далее надо прописать две команды

### dotnet ef migrations add Initial -c ApplicationContext --project ..\CollectingApplicationsAPI
### dotnet ef database update Initial -c ApplicationContext --project ..\CollectingApplicationsAPI

### Первая команда создает миграции, вторая на основе этих миграций создает базу

## Если выполнить всё правильно, то проект готов к проверке на работоспособность







