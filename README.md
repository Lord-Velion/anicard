# 🧝🏻‍♀️ AniCard

Backend для хранения и обмена карточек персонажей Emotion Creators.

Проект разрабатывается для развития навыков backend-разработки и демонстрации контекста знаний для работодателей.

## 📬 Контакты
[![Telegram](https://img.shields.io/badge/Telegram-2CA5E0?style=for-the-badge&logo=telegram&logoColor=white)](https://t.me/LordVelion)
[![Email](https://img.shields.io/badge/Email-D14836?style=for-the-badge&logo=gmail&logoColor=white)](mailto:dimaion666@gmail.com)

## 📖 Содержание

- [Что это за проект](#что-это-за-проект)
- [Стек технологий](#стек-технологий)
- [Функционал, что можно делать](#функционал-что-можно-делать)
- [Как запустить](#как-запустить)
- [Применение ИИ в разработке](#применение-ии-в-разработке)

## Что это за проект

**AniCard** - это backend система сайта, предназначенного для хранения карточек Emotion Creators.

**Emotion Creators** - это программа для создания аниме персонажей.

![Character editor](README/1.png)

В **Emotion Creators** можно создавать PNG-файлы, включающие в себя метаданные внешности персонажа, которыми можно делиться с другими пользователями редактора.

<table>
  <!-- Первый ряд: изображения (2,3,4) – серая линия снизу -->
  <tr style="border-bottom: 1px solid gray;">
    <td align="center"><img src="README/2.png" width="150" style="border-radius: 12px;"></td>
    <td align="center"><img src="README/3.png" width="150"></td>
    <td align="center"><img src="README/4.png" width="150"></td>
  </tr>
  <!-- Второй ряд: имена (Alya, Sakura, Asuna) – чёрная линия снизу -->
  <tr style="border-bottom: 1px solid black;">
    <td align="center"><strong>Alya</strong></td>
    <td align="center"><strong>Sakura</strong></td>
    <td align="center"><strong>Asuna</strong></td>
  </tr>
  <!-- Третий ряд: следующие изображения (5,6,7) -->
  <tr>
    <td align="center"><img src="README/5.png" width="150"></td>
    <td align="center"><img src="README/6.png" width="150"></td>
    <td align="center"><img src="README/7.png" width="150"></td>
  </tr>
  <!-- Четвёртый ряд: следующие имена (Gremory, Zero Two, Nezuko) -->
  <tr>
    <td align="center"><strong>Gremory</strong></td>
    <td align="center"><strong>Zero Two</strong></td>
    <td align="center"><strong>Nezuko Kamado</strong></td>
  </tr>
</table>

Проект находится в процессе активной разработки.

## Стек технологий

**Языки**  
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white)
![Python](https://img.shields.io/badge/Python-3776AB?style=for-the-badge&logo=python&logoColor=white)

**Backend**  
![.NET](https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![EF Core](https://img.shields.io/badge/EF_Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![LINQ](https://img.shields.io/badge/LINQ-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![ASP.NET Core Identity](https://img.shields.io/badge/Identity-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![JWT](https://img.shields.io/badge/JWT-000000?style=for-the-badge&logo=JSON%20web%20tokens&logoColor=white)

**Тестирование**  
![xUnit](https://img.shields.io/badge/xUnit-5E5E5E?style=for-the-badge&logo=xunit&logoColor=white)

**База данных**  
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-4169E1?style=for-the-badge&logo=postgresql&logoColor=white)

**Инфраструктура**  
![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)

## Функционал, что можно делать
- Регистрироваться и входить (получать JWT-токен)
- Загружать карточки персонажей
- Искать персонажей по параметрам
- Скачивать карточки персонажей
- Удалять карточки персонажей
- Обновлять информацию по карточкам персонажей

## Как запустить
Нужны Git и Docker на компьютере.

Открыть терминал, перейти в нужную директорию.

Ввести команды (Linux, WSL):
- `git clone https://github.com/Lord-Velion/anicard.git`
- `docker-compose -f compose.dev.yml up --build`

Дождаться запуска контейнеров.

В браузере ввести: `http://localhost:8080/swagger`

## Применение ИИ в проекте

![Gemini](https://img.shields.io/badge/Gemini-8E75B2?style=for-the-badge&logo=googlegemini&logoColor=white)
![DeepSeek](https://img.shields.io/badge/DeepSeek-4A6FA5?style=for-the-badge&logo=data:image/svg%2bxml;base64,...)
![ChatGPT](https://img.shields.io/badge/ChatGPT-74aa9c?style=for-the-badge&logo=openai&logoColor=white)
![Opencode](https://img.shields.io/badge/Opencode-555555?style=for-the-badge)

Работа на данный момент идёт по принципу:
- 60% - Google + Gemini
- 25% - DeepSeek + ChatGPT
- 15% - Opencode

Строится понимание пользы ИИ-систем и границ их возможностей.


