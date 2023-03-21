# Крестики-нолики API
![TicTakToe](https://img.shields.io/static/v1?label=.NET&message=7.0&color=brightgreen)![](https://img.shields.io/static/v1?label=ASP.NETCore&message=7.0&color=brightgreen)![](https://img.shields.io/static/v1?label=EntityFramewrokCore&message=7.0&color=brightgreen)![](https://img.shields.io/static/v1?label=SignalR&message=Core&color=critical)

![](https://github.com/vasjen/SCGame/blob/master/wwwroot/logo.png?raw=true)
## Обзор

Серверная часть мобильной игры крестики-нолики, разработанная по стандарту RESTful API. Игровой процесс построен на взаимодействии с тремя контроллерами, каждый из которых работает в пределах своей зоны ответственности.
Стандартные правила игры, игра автоматически заканчивается по совершению 9 ходов, если до этого победить никому из игроков не удалось. Каждый ход проверяется на допустимость - очередность хода, возможность выбора данной клетки, наличие прав пользователя на ход в этой партии.
Для игры требуется регистрация/авторизация. Реализовано через JWT.
## Содержимое
- [Как запустить](#Установка)
- [Использованные технологии](#Технологии)
- [Документация](#Документация)
- [Реализация](#Реализация)
- [Сущности](#Сущности)
- [Описание контроллеров](#Контроллеры)
- [Клиентская часть](#Клиенты)


## installation
Для запуска потребуется VSCode или Visual Studio. Приложение используется MSSQL, пользователь по умолчанию 'sa', указан в appsettings.json. В случае запуска из под другого пользователя подправить строку "GameDbConnection" в файле.
Пароль от пользователя храниться в secrets.json. Перед запуском потребуется предварительно создать хранилище и поместить туда пароль.
```
dotnet user-secrets init
dotnet user-secrets set "DbPassword" "YOUR_PASSWORD"
dotnet run
```
По умолчанию приложение запускается на 5082 порту. 
## Документация
После запуска, документация будет доступна по ссылке http://localhost:5182/Index.html, страница отладки доступна по ссылке http://localhost:5182/swagger/index.html


## Технологии
* ASP.NET Core - логика, контроллеры и отладка
* Entity Framework Core - подключение к БД
* ASP.NET Core Idenity и JWT - регистрация и получение токена после аутентификации
* Swagger - отладка контроллеров, тестирование
* Redoc -  документирование API
* SignalR - реализация возможности игры удаленных пользователей

## Реализация

Игра протекат по стандартным правилам. Для начала игры нужно иметь двух зарегистрированных пользователей, логины должны быть уникальными. Большая часть запросов к API доступна только авторизованным пользователям.
Для получения авторизованного токена нужно сделать запрос к соответствующему контроллеру и поулчить Bearer токен (действителен 20 минут) и добавить в заголовок запроса.
```
curl -X 'POST' \
  'http://localhost:5182/User/Create' \
  -H 'accept: */*' \
  -H 'Content-Type: application/json' \
  -d '{
  "userName": "string",
  "password": "string",
  "email": "string"
}'
```
После получения токена и добавления в заголовок запроса можно
- создать новую игру
- создать приглашение и отправить зарегистрированному пользователю
- принять запрос и удалить, если вас пригласили
- делать ходы в партии

Игра начинается только после принятие вторым игроком приглашения, только после этого в игре возможны делать ходы. Игровое поле представляет из себя запись 9 ячеек с индексами [1-9]. Это позволяет все игровое поле хранить в базе данных и выгружать по запросу. 
После каждого хода проверяется на ступило ли условия окончания игры - истекли ходы или кто-то выиграл. Для этого данные преобразуются в двумерный массив и выполняется проверка, ход заносится в БД, после чего данные возвращаются снова в виде последовательности проиндексированных ячеек.
При начале игры создается игровая комната (SignalR Hub), что позволяет игрокам в режиме реального времени совершать ходы и видеть изменения игрового процесса.

## Сущности
<details>
Основная сущность - игра. Содержит в себе информаци о участниках, очередность и количество ходов, игровое поле (доска).
  <summary>Game</summary>


```
"Game": {
        "type": "object",
        "properties": {
          "gameId": {
            "type": "integer",
            "format": "int32"
          },
          "status": {
            "$ref": "#/components/schemas/GameStatus"
          },
          "queue": {
            "type": "boolean",
            "nullable": true
          },
          "firstPlayerName": {
            "type": "string",
            "nullable": true
          },
          "firstPlayerId": {
            "type": "string",
            "nullable": true
          },
          "winner": {
            "type": "string",
            "nullable": true
          },
          "secondPlayerName": {
            "type": "string",
            "nullable": true
          },
          "secondPlayerId": {
            "type": "string",
            "nullable": true
          },
          "moves": {
            "type": "integer",
            "format": "int32"
          },
          "boardId": {
            "type": "integer",
            "format": "int32"
          },
          "board": {
            "$ref": "#/components/schemas/Board"
          },
          "createTime": {
            "type": "string",
            "format": "date-time"
          }
        },
        "additionalProperties": false
      }
````
  </details>
  
  <details>
  Игровое поле (доска). 9 клеток пронумерованных по порядку. В случае хода первого игрока выбранному полю присваивается значение 1, в случае второго - 2.
  <summary>Board</summary>
  
  ```
   "Board": {
        "type": "object",
        "properties": {
          "boardId": {
            "type": "integer",
            "format": "int32"
          },
          "fieldId": {
            "type": "integer",
            "format": "int32"
          },
          "fields": {
            "type": "array",
            "items": {
              "$ref": "#/components/schemas/Field"
            },
            "nullable": true
          }
  }
  ``` 
 </details>
 
 <details>
  Игровая клетка (ячейка).
  <summary>Field</summary>
  
  ```
   "Field": {
        "type": "object",
        "properties": {
          "fieldId": {
            "type": "integer",
            "format": "int32"
          },
          "fieldIndex": {
            "type": "integer",
            "format": "int32"
          },
          "fieldValue": {
            "type": "integer",
            "format": "int32",
            "nullable": true
          }
        },
        "additionalProperties": false
      }
  ``` 
 </details>
 
 <details>
  Приглашение. Содержит информаци о игре, на которую пригласили, а также информацию о приглашенном. В случае принятия в сущность игра вносистя информация и принявшем приглашение как о втором игроке, создается игровка комната и игра начиается, в случае отказа - приглашение удаляется
  <summary>Field</summary>
  
  ```
  "Invite": {
        "type": "object",
        "properties": {
          "inviteId": {
            "type": "integer",
            "format": "int32"
          },
          "gameId": {
            "type": "integer",
            "format": "int32"
          },
          "firstPlayer": {
            "type": "string",
            "nullable": true
          },
          "secondPlayer": {
            "type": "string",
            "nullable": true
          },
          "game": {
            "$ref": "#/components/schemas/Game"
          }
        },
        "additionalProperties": false
      }
  ``` 
 </details>
 
## Контроллеры
Для игры используется 3 контроллера, полное описание изложено в [документации](#Документация).
* UserController. Ответственен за пользователя. Позволяет зарегистрировать пользователя, проверить есть ли пользователь с таким именем и получать авторизационный токен.
* GameController. Отвечает за игровой процесс. Выдает список партий, начинает игру, делает ходы во время активной игры.
* InviteController. Отвечает за механизм приглашений. Позволяет создать новое, отослать кому-либо из пользователей, принять или пригласить.

## Клиенты
Для реализации онлайн игры была использована библиотека SignalR. После начала игры создается игровая комната, являющаяся оберткой Hub класса

Создание комнаты:
```
RoomRequest room = new(userName+"_room"+game.GameId);
         _registry.CreateRoom(room.Room);
```
Hub подключается как endpoint:
```
app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<PlayRoomHub>("/playroom");
                endpoints.MapDefaultControllerRoute();
            });
```
Для реализации онлайна необходимо реализовать подключение к хабу.
Например, на JavaScript подключение к комнате выглядит следующим образом:
```
var currentRoom = ""

    let connection = new signalR.HubConnectionBuilder()
        .withUrl("/playroom")
        .build();
        
const join = (room) => connection.start()
        .then(() => connection.invoke('JoinRoom', {room}))
        .then((history) => {
            console.log('message history', history)
            currentRoom = room
        })
```
Все участники игры могут обмениваться ходами через вызов метод SendMessage,
```
const send = (message) => connection.send('SendMessage', {message, room: currentRoom})
```
передавая в качестве сообщения сущность Game, которая возвращается от контроллера после каждого сделанного хода.
После проверки работоспособности данного подхода, дальнейшая разработка клиента игры не производилась.







