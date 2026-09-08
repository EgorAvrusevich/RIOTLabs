# Лабораторная работа №1 — Интернет-магазин (Вариант 1)

## Описание
Клиент-серверное приложение на C# с использованием TCP и JSON-протокола.

## Архитектура
```
OnlineStore/
├── OnlineStore.Shared/     # Общие модели и протокол
├── OnlineStore.Server/     # Сервер (TCP, порт 5000)
├── OnlineStore.Client/     # Консольный клиент
└── OnlineStore.Tests/      # Unit-тесты (xUnit)
```

## Сущности
- Customer — клиент
- Product — товар
- Order — заказ
- OrderItem — позиция заказа
- Payment — платеж

## Операции
1. **GetProducts** — получить список товаров
2. **CreateOrder** — создать заказ
3. **CancelOrder** — отменить заказ
4. **PayOrder** — оплатить заказ

## События
- OrderCreated
- OrderCancelled
- PaymentCompleted
- StockUpdated

## Запуск

### 1. Сборка
```bash
dotnet build OnlineStore.sln
```

### 2. Запуск сервера
```bash
cd OnlineStore.Server
dotnet run
```

### 3. Запуск клиента
```bash
cd OnlineStore.Client
dotnet run
```

### 4. Запуск тестов
```bash
cd OnlineStore.Tests
dotnet test
```

## Примеры запросов (JSON)

### GetProducts
```json
{"operation":"GetProducts","payload":{}}
```

### CreateOrder
```json
{"operation":"CreateOrder","payload":{"customerId":1,"items":[{"productId":1,"quantity":2}]}}
```

### CancelOrder
```json
{"operation":"CancelOrder","payload":{"orderId":1}}
```

### PayOrder
```json
{"operation":"PayOrder","payload":{"orderId":1,"amount":100000}}
```

## Тесты
- ✅ Успешное создание заказа
- ✅ Неизвестная операция
- ✅ Некорректные данные (недостаточно товара)
- ✅ Недоступный сервер
- ✅ Отмена заказа
- ✅ Оплата заказа
