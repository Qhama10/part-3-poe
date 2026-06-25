#  SafeChat — Cybersecurity Awareness Bot

A WPF desktop chatbot built in C# that educates users on cybersecurity topics through conversation, quizzes, and a task management system backed by a MySQL database.

---

##  Table of Contents

- [About](#about)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Usage](#usage)
- [Project Structure](#project-structure)
- [Database Setup](#database-setup)

---

## About

SafeChat is a conversational cybersecurity awareness assistant designed to help everyday users learn how to stay safe online. It greets users by name, remembers their interests, responds to their emotions, and delivers tips on topics like phishing, passwords, scams, and privacy — all through a friendly chat interface.

---

## Features

###  Conversational Chat
- Greets users by name and remembers them across sessions
- Detects and responds to emotions (worried, curious, frustrated, happy)
- Persists chat history, user interests, and favourite topics to local files
- Plays a welcome audio greeting on launch (`WelcomeMessage.wav`)

###  Cybersecurity Tips
Ask about any of these topics to receive randomised, practical tips:

| Topic | Keywords |
|---|---|
| Phishing | `phishing`, `fake email`, `phish` |
| Passwords | `password`, `passphrase` |
| Cybersecurity | `cybersecurity`, `cyber security` |
| Scams | `scam`, `fraud` |
| Privacy | `privacy`, `personal data` |

Type "more tips" or "yes" after a tip to get another one on the same topic.

###  Task Manager (MySQL)
Manage cybersecurity to-dos with full CRUD support:

| Command | What it does |
|---|---|
| `add task` | Create a new task with title, description, and optional reminder |
| `view tasks` | List all tasks with status and reminder dates |
| `complete [id]` | Mark a task as done |
| `delete [id]` | Remove a task |
| `show reminders` | View upcoming reminder dates |

Tasks and reminders are stored persistently in a **MySQL database**.

###  Cybersecurity Quiz
- 12 multiple-choice and true/false questions
- Instant feedback with explanations after each answer
- Final score with a personalised performance message
- Commands: `start quiz`, `stop quiz`

###  Activity Log
Tracks everything that happens in the session. Type `show activity log` to view the last 10 actions.

---

## Tech Stack

- **Language:** C# (.NET / WPF)
- **UI Framework:** WPF (XAML)
- **Database:** MySQL via `MySql.Data` (MySql.Data.MySqlClient)
- **Audio:** `System.Media.SoundPlayer`
- **IDE:** Visual Studio (`.slnx` solution file)

---

## Prerequisites

Before running the project, make sure you have:

- [Visual Studio 2022+](https://visualstudio.microsoft.com/) with the **.NET Desktop Development** workload
- [MySQL Server](https://dev.mysql.com/downloads/mysql/) installed and running locally
- `MySql.Data` NuGet package (install via NuGet Package Manager)

---

## Getting Started

1. **Clone the repository**
   ```bash
   git clone https://github.com/Qhama10/part-3-poe.git
   cd part-3-poe
   ```

2. **Open the solution** in Visual Studio
   ```
   part 3 poe.slnx
   ```

3. **Configure your database connection**
   Open `DatabaseHelper.cs` and update the connection string with your MySQL credentials:
   ```csharp
   // Example — update with your actual password
   private string connectionString = "server=localhost;database=safechat;uid=root;pwd=YOUR_PASSWORD;";
   ```

4. **Create the database** in MySQL:
   ```sql
   CREATE DATABASE safechat;
   ```
   The `tasks` table is created automatically when the app first launches.

5. **Restore NuGet packages** — Visual Studio should do this automatically, or run:
   ```
   Tools → NuGet Package Manager → Manage NuGet Packages for Solution → Restore
   ```

6. **Build and run** with `F5` or the green ▶ button.

---

## Usage

Once the app starts:

1. Enter your **name** when prompted
2. Ask about any cybersecurity topic — e.g. `tell me about phishing`
3. Say `add task` to create a cybersecurity to-do item
4. Say `start quiz` to test your knowledge
5. Say `help` at any time to see available commands

### Example commands

```
tell me about phishing
how do I create a strong password?
I'm interested in privacy
add task
view tasks
complete 1
delete 2
start quiz
show activity log
show reminders
how are you
```

---

## Project Structure

```
part-3-poe/
├── App.xaml                # Application entry point
├── App.xaml.cs
├── MainWindow.xaml         # Main chat UI (XAML layout)
├── MainWindow.xaml.cs      # All chatbot logic (~1300 lines)
├── DatabaseHelper.cs       # MySQL connection helper
├── AssemblyInfo.cs
├── WelcomeMessage.wav      # Greeting audio file
├── part 3 poe.csproj       # Project file
└── part 3 poe.slnx         # Solution file
```

---

## Database Setup

The app auto-creates the `tasks` table on first run using:

```sql
CREATE TABLE IF NOT EXISTS tasks (
    id              INT AUTO_INCREMENT PRIMARY KEY,
    title           VARCHAR(255) NOT NULL,
    description     TEXT,
    reminder_date   DATETIME,
    is_completed    BOOLEAN DEFAULT FALSE,
    date_created    DATETIME DEFAULT CURRENT_TIMESTAMP
);
```

If the database connection fails, a pop-up will guide you to fix your credentials in `DatabaseHelper.cs`.

---

## Author

**Qhama10** — [GitHub Profile](https://github.com/Qhama10)
