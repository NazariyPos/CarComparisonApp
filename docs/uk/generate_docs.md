# Генерація документації

Цей гайд описує, як генерувати документацію проєкту за допомогою `Swashbuckle (Swagger)` і `DocFX`, а також як вона публікується через GitHub Pages.

## Передумови

- Встановлений `.NET SDK 8+`
- Перейдіть у корінь репозиторію:

```bash
cd CarComparisonApp
```

## 1) Генерація OpenAPI (Swagger) JSON

Swagger JSON генерується із запущеного API.

### Варіант A: вручну

1. Запустіть API:

```bash
dotnet run --project CarComparisonApi/CarComparisonApi.csproj --urls http://localhost:5056
```

2. В іншому терміналі збережіть Swagger JSON:

```bash
curl http://localhost:5056/swagger/v1/swagger.json -o docs/swagger.json
```

### Варіант B: PowerShell (автоматичний start/stop)

```powershell
$p = Start-Process dotnet -ArgumentList 'run --project CarComparisonApi/CarComparisonApi.csproj --urls http://localhost:5056' -PassThru
Start-Sleep -Seconds 8
try {
    Invoke-WebRequest -UseBasicParsing http://localhost:5056/swagger/v1/swagger.json -OutFile docs/swagger.json
}
finally {
    Stop-Process -Id $p.Id -Force
}
```

## 2) Генерація сайту документації DocFX

1. Встановіть `DocFX` локально в папку `.tools` (одноразово):

```bash
dotnet tool install docfx --tool-path ./.tools
```

2. Запустіть генерацію за `docfx.json`:

```bash
./.tools/docfx docfx.json
```

Після успішного виконання:
- API-метадані генеруються в `api/`
- статичний сайт документації генерується в `_site/`

## 3) Перевірка якості документації

Запустіть валідацію DocFX з перетворенням warning на error:

```bash
./.tools/docfx build docfx.json --warningsAsErrors
```

Швидкі допоміжні скрипти:

PowerShell:
```powershell
powershell -ExecutionPolicy Bypass -File scripts/verify-docs.ps1
```

Bash:
```bash
./scripts/verify-docs.sh
```

## 4) Публікація через GitHub Pages (CI/CD)

Файл CI/CD workflow:
- `.github/workflows/docs-pages.yml`

Що робить workflow:
1. запускається на `push` у `main` (або вручну);
2. збирає сайт DocFX;
3. публікує `_site` у GitHub Pages через GitHub Actions.

Одноразове налаштування репозиторію:
1. Відкрийте `Settings` -> `Pages`.
2. Встановіть `Source` = `GitHub Actions`.

Після першого успішного деплою документація буде доступна за адресою:
- `https://<github-username>.github.io/CarComparisonApp/`

## 5) Перегляд згенерованої документації локально

Відкрийте у браузері:

- `_site/index.html`

## 6) Архів документації (опційно)

Щоб створити архів для передачі/здачі:

```powershell
if (Test-Path documentation-archive.zip) { Remove-Item documentation-archive.zip -Force }
Compress-Archive -Path _site,docs/swagger.json,docfx.json,docs/index.md,docs/toc.yml -DestinationPath documentation-archive.zip
```

Результат: `documentation-archive.zip` у корені репозиторію.

## Примітка щодо Git

Папки `api/` та `_site/` є згенерованими артефактами і зазвичай не мають комітитися в репозиторій.
