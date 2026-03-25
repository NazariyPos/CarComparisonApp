# Архітектура та бізнес-логіка

Проєкт побудовано за шаровим підходом:
- `Controllers` — HTTP-рівень;
- `Services` — бізнес-логіка;
- `Models/DTOs` — контракти даних;
- `Data/*.json` — файлове сховище (прототип).

Типовий flow: Client -> Controller -> Service -> Data -> HTTP response.
