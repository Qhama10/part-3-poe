using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MySql.Data.MySqlClient;

namespace WpfApp2
{
    public partial class MainWindow : Window
    {
        private Random rand = new Random();

         
        private readonly DatabaseHelper db = new DatabaseHelper();

        // files 
        private readonly string historyFile = "ChatHistory.txt";
        private readonly string interestsFile = "UserInterests.txt";
        private readonly string favoritesFile = "UserFavorites.txt";

        // user memory 
        private string userName = "";
        private string favouriteTopic = "";
        private string lastTopic = "";
        private bool waitingForName = true;
        private bool waitingForEmotion = false;
        private bool inTopicFlow = false;

        // task flow 
        private bool waitingForTaskDescription = false;
        private bool waitingForReminder = false;
        private bool waitingForReminderDays = false;
        private string pendingTaskTitle = "";
        private string pendingTaskDescription = "";

        // quiz flow 
        private bool inQuiz = false;
        private int quizIndex = 0;
        private int quizScore = 0;
        private bool waitingForAnswer = false;

        // dictionary of actvity log 
        private List<string> activityLog = new List<string>();

        private List<string> chatLog = new List<string>();
        private List<FavoriteTopic> favoriteTopics = new List<FavoriteTopic>();

       // data that contains the tips for the user 
        private string[] phishingTips =
        {
            "Never click suspicious links in emails — hover over them first to check the real URL.",
            "Always verify the sender's email address before responding to any request.",
            "Be wary of urgent messages asking for passwords or personal info — that's a red flag.",
            "Legitimate companies will never ask for your password via email.",
            "Look out for misspellings in email addresses, like 'support@paypa1.com'."
        };

        private string[] passwordTips =
        {
            "Use a mix of uppercase, lowercase, numbers, and symbols in your passwords.",
            "Never reuse the same password across multiple sites.",
            "Avoid using personal info like your name or birthday in passwords.",
            "Consider using a password manager to keep track of strong passwords.",
            "Enable two-factor authentication wherever possible."
        };

        private string[] cybersecurityTips =
        {
            "Keep your operating system and apps updated to patch vulnerabilities.",
            "Use reputable antivirus software and keep it updated.",
            "Avoid public Wi-Fi for sensitive accounts — use a VPN if you must.",
            "Back up important data regularly to an external drive or cloud.",
            "Never plug in unknown USB drives — they can carry malware."
        };

        private string[] scamTips =
        {
            "If an offer sounds too good to be true, it probably is.",
            "Never send money to someone you've only met online.",
            "Government agencies never demand urgent payment over the phone.",
            "Always verify charities before donating — use trusted charity checkers.",
            "Be suspicious of unexpected calls asking for personal details."
        };

        private string[] privacyTips =
        {
            "Review your social media privacy settings regularly.",
            "Avoid sharing too much personal information online.",
            "Check app permissions before installing — does it really need your location?",
            "Use a VPN on public networks to encrypt your traffic.",
            "Private/incognito browsers can help reduce tracking."
        };

        private Dictionary<string, string[]> topicTips;

      // the questions in the quiz which user has to choose coorect answer from 
        private List<QuizQuestion> quizQuestions = new List<QuizQuestion>
        {
            new QuizQuestion(
                "What should you do if you receive an email asking for your password?",
                new[] { "A) Reply with your password", "B) Delete the email", "C) Report it as phishing", "D) Ignore it" },
                "C",
                "Correct! Reporting phishing emails helps prevent scams and protects others."),

            new QuizQuestion(
                "True or False: Using the same password for multiple accounts is safe.",
                new[] { "A) True", "B) False" },
                "B",
                "False! Reusing passwords means one breach can compromise all your accounts."),

            new QuizQuestion(
                "What does 2FA stand for?",
                new[] { "A) Two-Factor Authentication", "B) Two-File Access", "C) Twice-Failed Attempt", "D) Two-Firewall Antivirus" },
                "A",
                "Correct! Two-Factor Authentication adds an extra layer of security beyond just a password."),

            new QuizQuestion(
                "Which of these is the strongest password?",
                new[] { "A) password123", "B) John1990", "C) Tr0ub4dor&3!", "D) 12345678" },
                "C",
                "Correct! A mix of uppercase, lowercase, numbers and symbols makes passwords much harder to crack."),

            new QuizQuestion(
                "True or False: Public Wi-Fi is always safe to use for banking.",
                new[] { "A) True", "B) False" },
                "B",
                "False! Public Wi-Fi can be intercepted. Always use a VPN or mobile data for banking."),

            new QuizQuestion(
                "What is phishing?",
                new[] { "A) A type of malware", "B) Tricking users into revealing info via fake messages", "C) A firewall technique", "D) Encrypting your data" },
                "B",
                "Correct! Phishing uses deceptive emails or messages to steal personal information."),

            new QuizQuestion(
                "Which is safest when downloading software?",
                new[] { "A) Any website", "B) Email attachments", "C) Official developer websites", "D) Torrent sites" },
                "C",
                "Correct! Always download software from official sources to avoid malware."),

            new QuizQuestion(
                "True or False: Antivirus software alone is enough to protect you online.",
                new[] { "A) True", "B) False" },
                "B",
                "False! Antivirus is important but you also need good habits like strong passwords and software updates."),

            new QuizQuestion(
                "What should you do before clicking a link in an email?",
                new[] { "A) Click it immediately", "B) Hover over it to check the real URL", "C) Forward it to friends", "D) Reply to ask if it is real" },
                "B",
                "Correct! Hovering over a link reveals its true destination before you click."),

            new QuizQuestion(
                "True or False: It is safe to share your password with a trusted friend.",
                new[] { "A) True", "B) False" },
                "B",
                "False! You should never share passwords — even with people you trust."),

            new QuizQuestion(
                "What does a VPN do?",
                new[] { "A) Speeds up your internet", "B) Encrypts your internet connection for privacy", "C) Removes viruses", "D) Blocks all ads" },
                "B",
                "Correct! A VPN encrypts your traffic and hides your IP address for safer browsing."),

            new QuizQuestion(
                "Which of these is a sign of a phishing email?",
                new[] { "A) Comes from your bank's official domain", "B) Has your full name and account details", "C) Creates urgency and asks you to click a link now", "D) Has no attachments" },
                "C",
                "Correct! Urgency and suspicious links are classic phishing tactics.")
        };

        // constructor- methods that runs automatically when object is created 
        public MainWindow()
        {
            InitializeComponent();

            topicTips = new Dictionary<string, string[]>
            {
                { "phishing",      phishingTips },
                { "password",      passwordTips },
                { "cybersecurity", cybersecurityTips },
                { "scam",          scamTips },
                { "privacy",       privacyTips }
            };

            Audio.PlayGreeting();
            InitialiseDatabase();
        }

        // database init
        private void InitialiseDatabase()
        {
            try
            {
                using (var conn = db.GetConnection())
                {
                    conn.Open();
                    string sql = @"CREATE TABLE IF NOT EXISTS tasks (
                        id INT AUTO_INCREMENT PRIMARY KEY,
                        title VARCHAR(255) NOT NULL,
                        description TEXT,
                        reminder_date DATETIME,
                        is_completed BOOLEAN DEFAULT FALSE,
                        date_created DATETIME DEFAULT CURRENT_TIMESTAMP
                    );";
                    new MySqlCommand(sql, conn).ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("DB connection failed: " + ex.Message +
                    "\n\nCheck your password in DatabaseHelper.cs.", "Database Error");
            }
        }
// where the window loads 
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadChatHistory();
            LoadInterest();

            Say("Bot: Welcome to the Cybersecurity Awareness Bot, Safechat! ");
            Say("Bot: Please enter your name ?");
        }

        //enter key support 
        private void UserInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SendButton_Click(sender, e);
        }

        // send button
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string input = UserInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(input)) return;

            Say("You: " + input);
            UserInput.Clear();

            string lower = input.ToLower();

            // ── Name collection ──
            if (waitingForName)
            {
                // say is a method helper so that theres no need to write the code over and ovver again.
                userName = input;
                waitingForName = false;
                Say($"Bot: Nice to meet you, {userName}! ");
                Say("Bot: I'm here to help you stay safe online.");
                if (!string.IsNullOrEmpty(favouriteTopic))
                    Say($"Bot: I remember that you are interested in {favouriteTopic}. Would you like tips about that?");
                else
                    Say("Bot: You are welcome to ask me about phishing, passwords, scams, privacy, or cybersecurity.");
                Say("Bot: You can also type 'add task', 'view tasks', 'start quiz', or 'show activity log'.");
                return;
            }

            // ── Emotion reply ──
            if (waitingForEmotion)
            {
                waitingForEmotion = false;
                HandleEmotionReply(lower);// this is when user types emotion word in lowercase , so its not case sensitive 
                return;
            }

            // ── Quiz answer ──
            if (inQuiz && waitingForAnswer)
            {
                HandleQuizAnswer(input.Trim().ToUpper()); //accepts users answer regardless of wether its uppercase or lowercase 
                return;
            }

            // ── Task description ──
            if (waitingForTaskDescription)
            {
                pendingTaskDescription = input;
                waitingForTaskDescription = false;// false because the user already has put a description 
                waitingForReminder = true;// true so there can be conversation flow and so it can display message asking the user if they need a reminder 
                Say("Bot: Got it! Would you like a reminder for this task? (yes / no)");
                return;
            }

            // ── Reminder yes/no ──
            if (waitingForReminder)
            {
                waitingForReminder = false;
                if (lower.Contains("yes") || lower.Contains("y"))
                {
                    waitingForReminderDays = true;
                    Say("Bot: In how many days would you like the reminder? (enter a number)");
                }
                else
                {
                    SaveTaskToDb(pendingTaskTitle, pendingTaskDescription, null);
                    LogActivity($"Task added: '{pendingTaskTitle}' (no reminder)");
                    Say($"Bot: Task '{pendingTaskTitle}' saved with no reminder.");
                    pendingTaskTitle = pendingTaskDescription = "";
                }
                return;
            }

            // ── Reminder days ──
            if (waitingForReminderDays)
            {
                waitingForReminderDays = false;
                if (int.TryParse(lower.Trim(), out int days))
                {
                    //the code here records and calculates everything 
                    DateTime reminderDate = DateTime.Now.AddDays(days);
                    SaveTaskToDb(pendingTaskTitle, pendingTaskDescription, reminderDate);
                    LogActivity($"Task added: '{pendingTaskTitle}' (reminder in {days} days on {reminderDate:MMM dd, yyyy})");
                    Say($"Bot:  Task '{pendingTaskTitle}' saved! I'll remind you on {reminderDate:dddd, MMM dd yyyy}."); //displays the message to the user 
                }
                else
                {
                    SaveTaskToDb(pendingTaskTitle, pendingTaskDescription, null);
                    LogActivity($"Task added: '{pendingTaskTitle}' (no reminder — invalid days entered)");
                    Say($"Bot:  Task '{pendingTaskTitle}' saved without a reminder (invalid number entered).");
                }
                pendingTaskTitle = pendingTaskDescription = "";
                return;
            }

            //  detect intent first 
            string intent = DetectIntent(lower);

            switch (intent)
            {
                case "add_task": StartAddTask(lower); return;
                case "view_tasks": ShowTasks(); return;
                case "complete_task": PromptCompleteTask(); return;
                case "delete_task": PromptDeleteTask(); return;
                case "start_quiz": StartQuiz(); return;
                case "stop_quiz": StopQuiz(); return;
                case "activity_log": ShowActivityLog(); return;
                case "show_reminders": ShowReminders(); return;
            }

            // allows sentiment to be in lowercase 
            bool sentimentDetected = DetectEmotion(lower);

            // follow up 
            if (inTopicFlow && IsFollowUp(lower))
            {
                HandleFollowUp();
                return;
            }

            Respond(lower, sentimentDetected);
        }

        // intent detection 
        private string DetectIntent(string input) // analyses users input and checks for keywords or phrases 
        {
            // Add task variations
            if (input.Contains("add task") || input.Contains("create task") ||
                input.Contains("new task") || input.Contains("add a task") ||
                input.Contains("set a task") || input.Contains("make a task") ||
                input.Contains("remind me to") || input.Contains("remind me about") ||
                input.Contains("set reminder") || input.Contains("add reminder") ||
                (input.Contains("remember") && input.Contains("to")) ||
                input.Contains("enable 2fa") || input.Contains("enable two-factor") ||
                input.Contains("set up 2fa"))
                return "add_task";

            // View tasks
            if (input.Contains("view task") || input.Contains("show task") ||
                input.Contains("list task") || input.Contains("my task") ||
                input.Contains("what tasks") || input.Contains("show my tasks") ||
                input.Contains("display task") || input.Contains("see my tasks"))
                return "view_tasks";

            // Complete task
            if (input.Contains("complete task") || input.Contains("mark as done") ||
                input.Contains("finish task") || input.Contains("done with task") ||
                input.Contains("task done") || input.Contains("completed task") ||
                input.Contains("mark complete") || input.Contains("mark done"))
                return "complete_task";

            // Delete task
            if (input.Contains("delete task") || input.Contains("remove task") ||
                input.Contains("cancel task") || input.Contains("get rid of task"))
                return "delete_task";

            // Quiz
            if (input.Contains("start quiz") || input.Contains("play quiz") ||
                input.Contains("take quiz") || input.Contains("begin quiz") ||
                input.Contains("quiz me") || input.Contains("test me") ||
                input.Contains("test my knowledge") || input.Contains("play game"))
                return "start_quiz";

            if (input.Contains("stop quiz") || input.Contains("quit quiz") ||
                input.Contains("exit quiz") || input.Contains("end quiz"))
                return "stop_quiz";

            // Activity log
            if (input.Contains("activity log") || input.Contains("show log") ||
                input.Contains("what have you done") || input.Contains("recent actions") ||
                input.Contains("history") || input.Contains("what did you do") ||
                input.Contains("action log") || input.Contains("show activity"))
                return "activity_log";

            // Reminders
            if (input.Contains("show reminder") || input.Contains("my reminder") ||
                input.Contains("upcoming reminder") || input.Contains("what reminders"))
                return "show_reminders";

            return "none";
        }

       // Part for add task 


        private void StartAddTask(string input)
        {
            
            string title = ExtractTaskTitle(input);// checks if user has included a title 

            if (string.IsNullOrWhiteSpace(title))
            {
                Say("Bot: Sure! What is the title of the task you'd like to add?");
                waitingForTaskDescription = false;
                waitingForReminder = false;
                
                pendingTaskTitle = "";
                
                waitingForTaskTitle = true;
            }
            else
            {
                pendingTaskTitle = title;
                waitingForTaskDescription = true;
                Say($"Bot: Great! I'll add a task: \"{title}\".");
                Say("Bot: Can you give me a short description for this task?");
            }

            LogActivity($"User initiated adding a task via NLP.");
        }

        private bool waitingForTaskTitle = false;

        private string ExtractTaskTitle(string input)
        {
            // Strip common NLP prefixes to extract the actual task name
            string[] prefixes = {
                "add task", "create task", "new task", "add a task", "set a task",
                "make a task", "remind me to", "remind me about", "set reminder to",
                "set reminder for", "add reminder to", "add reminder for",
                "remember to", "remember that", "i need to", "i want to",
                "can you remind me to", "please add task", "please remind me to"
            };

            string result = input;
            foreach (var prefix in prefixes)
            {
                if (result.StartsWith(prefix))
                {
                    result = result.Substring(prefix.Length).Trim();
                    break;
                }
            }

            // Clean up punctuation
            result = result.Trim('.', '!', '?', ',');

            // If nothing meaningful left, return empty
            return result.Length > 2 ? result : "";
        }

        // For the to-do task 

        private void SaveTaskToDb(string title, string description, DateTime? reminderDate)
        {
            try
            {
                using (var conn = db.GetConnection())
                {
                    conn.Open();
                    string sql = "INSERT INTO tasks (title, description, reminder_date) VALUES (@t, @d, @r)";
                    var cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@t", title);// paramters- replace placeholders with actual values 
                    cmd.Parameters.AddWithValue("@d", description);
                    cmd.Parameters.AddWithValue("@r", reminderDate.HasValue ? (object)reminderDate.Value : DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Say("Bot:  Task could not be saved to the database : " + ex.Message);
            }
        }

        // For the viewing of the task


        private void ShowTasks()
        {
            try
            {
                using (var conn = db.GetConnection())
                {
                    conn.Open();
                    string sql = "SELECT id, title, description, reminder_date, is_completed FROM tasks ORDER BY date_created DESC";
                    var cmd = new MySqlCommand(sql, conn);
                    var reader = cmd.ExecuteReader();

                    bool hasTasks = false;
                    Say("Bot: ─────────────────────────────────");
                    Say("Bot: YOUR CYBERSECURITY TASKS:");
                    Say("Bot: ─────────────────────────────────");

                    while (reader.Read())
                    {
                        hasTasks = true;
                        int id = reader.GetInt32("id");
                        string title = reader.GetString("title");
                        string desc = reader.IsDBNull(reader.GetOrdinal("description")) ? "No description" : reader.GetString("description");
                        bool completed = reader.GetBoolean("is_completed");
                        string status = completed ? " Done" : " Pending";
                        string reminderStr = "";

                        if (!reader.IsDBNull(reader.GetOrdinal("reminder_date")))
                        {
                            DateTime rd = reader.GetDateTime("reminder_date");
                            reminderStr = $" | Reminder: {rd:MMM dd, yyyy}";
                        }

                        Say($"Bot: [{id}] {status} — {title}");
                        Say($"Bot:        {desc}{reminderStr}");
                    }

                    if (!hasTasks)
                        Say("Bot: You have no tasks yet! Say 'add task' to create one.");

                    Say("Bot: ─────────────────────────────────");
                    Say("Bot: Say 'complete task [id]' or 'delete task [id]' to manage tasks.");

                    LogActivity("User viewed task list.");
                }
            }
            catch (Exception ex)
            {
                Say("Bot:  Could not load tasks: " + ex.Message);
            }
        }

       // Display the reminders 


        private void ShowReminders()
        {
            try
            {
                using (var conn = db.GetConnection())
                {
                    conn.Open();
                    string sql = "SELECT title, reminder_date FROM tasks WHERE reminder_date IS NOT NULL AND is_completed = FALSE ORDER BY reminder_date ASC";
                    var reader = new MySqlCommand(sql, conn).ExecuteReader();

                    Say("Bot:  UPCOMING REMINDERS:");
                    bool any = false;
                    while (reader.Read())
                    {
                        any = true;
                        string title = reader.GetString("title");
                        DateTime rd = reader.GetDateTime("reminder_date");
                        string when = rd < DateTime.Now ? "OVERDUE!" : $"on {rd:dddd, MMM dd yyyy}";
                        Say($"Bot:  • {title} — {when}");
                    }
                    if (!any) Say("Bot: No upcoming reminders.");
                    LogActivity("User viewed reminders.");
                }
            }
            catch (Exception ex)
            {
                Say("Bot: ⚠️ " + ex.Message);
            }
        }

        // Prompt complete or delete 


        private void PromptCompleteTask()
        {
            ShowTasks();
            Say("Bot: Type the task ID number to mark complete, e.g. 'complete 2'");
            // Actual completion is handled in Respond() via direct number detection
        }

        private void PromptDeleteTask()
        {
            ShowTasks();
            Say("Bot: Type the task ID number to delete, e.g. 'delete 2'");
        }

        private void CompleteTask(int id)
        {
            try
            {
                using (var conn = db.GetConnection())
                {
                    conn.Open();
                    string title = GetTaskTitle(id, conn);
                    new MySqlCommand($"UPDATE tasks SET is_completed = TRUE WHERE id = {id}", conn).ExecuteNonQuery();
                    Say($"Bot:  Task '{title}' marked as completed!");
                    LogActivity($"Task completed: '{title}' (ID {id})");
                }
            }
            catch (Exception ex) { Say("Bot: ⚠️ " + ex.Message); }
        }

        private void DeleteTask(int id)
        {
            try
            {
                using (var conn = db.GetConnection())
                {
                    conn.Open();
                    string title = GetTaskTitle(id, conn);
                    new MySqlCommand($"DELETE FROM tasks WHERE id = {id}", conn).ExecuteNonQuery();
                    Say($"Bot:  Task '{title}' deleted.");
                    LogActivity($"Task deleted: '{title}' (ID {id})");
                }
            }
            catch (Exception ex) { Say("Bot: ⚠️ " + ex.Message); }
        }

        private string GetTaskTitle(int id, MySqlConnection conn)
        {
            try
            {
                var r = new MySqlCommand($"SELECT title FROM tasks WHERE id = {id}", conn).ExecuteReader();
                if (r.Read()) return r.GetString("title");
            }
            catch { }
            return $"#{id}";
        }

       
        // The official quiz 


        private void StartQuiz()
        {
            inQuiz = true;
            quizIndex = 0;
            quizScore = 0;
            waitingForAnswer = false;
            LogActivity("The quiz has started.");
            Say("Bot:  Welcome to the Cybersecurity Quiz!");
            Say($"Bot: There are {quizQuestions.Count} questions. Type the letter of your answer (A, B, C or D).");
            Say("Bot: ─────────────────────────────────");
            AskQuizQuestion();
        }

        private void StopQuiz()
        {
            inQuiz = false;
            waitingForAnswer = false;
            Say("Bot: Quiz stopped. Type 'start quiz' to try again anytime!");
            LogActivity("Quiz stopped early.");
        }

        private void AskQuizQuestion()
        {
            if (quizIndex >= quizQuestions.Count)
            {
                EndQuiz();
                return;
            }

            var q = quizQuestions[quizIndex];
            Say($"Bot: ❓ Question {quizIndex + 1}/{quizQuestions.Count}:");
            Say($"Bot: {q.Question}");
            foreach (var opt in q.Options)
                Say($"Bot:   {opt}");

            waitingForAnswer = true;
        }

        private void HandleQuizAnswer(string answer)
        {
            // Accept just the letter or the full option text
            string letter = answer.Length > 0 ? answer[0].ToString() : "";

            var q = quizQuestions[quizIndex];

            if (letter == q.CorrectAnswer)
            {
                quizScore++;
                Say("Bot:  Correct! " + q.Explanation);
            }
            else
            {
                Say($"Bot:  Not quite. The correct answer was {q.CorrectAnswer}. {q.Explanation}");
            }

            quizIndex++;
            waitingForAnswer = false;

            if (quizIndex < quizQuestions.Count)
            {
                Say("Bot: ─────────────────────────────────");
                AskQuizQuestion();
            }
            else
            {
                EndQuiz();
            }
        }

        private void EndQuiz()
        {
            inQuiz = false;
            waitingForAnswer = false;
            int total = quizQuestions.Count;
            double pct = (double)quizScore / total * 100;

            Say("Bot: ═════════════════════════════════");
            Say($"Bot:  QUIZ COMPLETE! Your score is: {quizScore}/{total} ({pct:F0}%)");

            if (pct >= 90)
                Say("Bot:  Outstanding! You're seems like you are a cybersecurity pro!");
            else if (pct >= 70)
                Say("Bot: Great job! You have great cybersecurity knowledge.");
            else if (pct >= 50)
                Say("Bot: There is always room to learn more.");
            else
                Say("Bot: Keep learning — cybersecurity knowledge protects you every day!");

            Say("Bot: ══════════════════════════════════");
            LogActivity($"Quiz completed — Score: {quizScore}/{total} ({pct:F0}%)");
        }

        
        private void LogActivity(string action)
        {
            string entry = $"[{DateTime.Now:HH:mm:ss}] {action}";
            activityLog.Add(entry);
        }

        private void ShowActivityLog()
        {
            Say("Bot: ─────────────────────────────────");
            Say("Bot:  RECENT ACTIVITY LOG:");

            if (activityLog.Count == 0)
            {
                Say("Bot: No activity recorded yet.");
            }
            else
            {
                // Show last 10 actions
                var recent = activityLog.Skip(Math.Max(0, activityLog.Count - 10)).ToList();
                for (int i = 0; i < recent.Count; i++)
                    Say($"Bot: {i + 1}. {recent[i]}");

                if (activityLog.Count > 10)
                    Say($"Bot: ... and {activityLog.Count - 10} earlier actions. (Showing last 10)");
            }

            Say("Bot: ─────────────────────────────────");
        }

        // The Main Responses enhanced 
        private void Respond(string input, bool sentimentDetected)
        {
            try
            {
                // Handle "complete [number]" or "delete [number]" inline
                if ((input.StartsWith("complete") || input.StartsWith("mark done") || input.StartsWith("finish"))
                    && ExtractNumber(input) > 0)
                {
                    CompleteTask(ExtractNumber(input));
                    return;
                }

                if ((input.StartsWith("delete") || input.StartsWith("remove"))
                    && ExtractNumber(input) > 0)
                {
                    DeleteTask(ExtractNumber(input));
                    return;
                }

                // Waiting for task title (no prefix detected earlier)
                if (waitingForTaskTitle)
                {
                    waitingForTaskTitle = false;
                    pendingTaskTitle = input.Trim('.', '!', '?');
                    waitingForTaskDescription = true;
                    Say($"Bot: Got it! Task title: \"{pendingTaskTitle}\".");
                    Say("Bot: Now give me a short description for this task.");
                    return;
                }

                // Interest / favourite topic detection
                if (input.Contains("interested in") ||
                    (input.Contains("like") && (input.Contains("topic") || input.Contains("subject"))) ||
                    input.Contains("my favourite") || input.Contains("my favorite"))
                {
                    string topic = "";
                    if (input.Contains("interested in"))
                        topic = input.Substring(input.IndexOf("interested in") + 13).Trim();
                    else if (input.Contains("my favourite") || input.Contains("my favorite"))
                    {
                        int idx = input.IndexOf("favorite") >= 0 ? input.IndexOf("favorite") + 8 : input.IndexOf("favourite") + 9;
                        topic = input.Substring(idx).Trim().Replace("topic is", "").Replace("subject is", "").Trim();
                    }
                    topic = topic.Trim('.', '!', '?');

                    if (!string.IsNullOrEmpty(topic))
                    {
                        favouriteTopic = topic;
                        lastTopic = GetMatchingTopic(topic);
                        SaveInterest();
                        AddFavorite(topic);
                        inTopicFlow = true;
                        Say($"Bot: Great! I'll remember that you're interested in {favouriteTopic}. 📝");
                        if (!string.IsNullOrEmpty(lastTopic) && topicTips.ContainsKey(lastTopic))
                            Say("Bot: " + topicTips[lastTopic][rand.Next(topicTips[lastTopic].Length)]);
                        else
                            Say("Bot: " + cybersecurityTips[rand.Next(cybersecurityTips.Length)]);
                        LogActivity($"User expressed interest in: {topic}");
                    }
                    else
                        Say("Bot: What topic are you interested in? (phishing, passwords, scams, privacy, cybersecurity)");
                    return;
                }

                // Memory recall
                if (input.Contains("remind me") && !input.Contains("remind me to") && !input.Contains("remind me about") ||
                    input.Contains("what do i like") || input.Contains("what am i interested in") ||
                    input.Contains("my interest") || input.Contains("what did i tell you"))
                {
                    if (!string.IsNullOrEmpty(favouriteTopic))
                    {
                        Say($"Bot: Based on our conversation, you're interested in {favouriteTopic}. ");
                        Say($"Bot: {GetPersonalizedFollowUp(favouriteTopic)}");
                    }
                    else
                        Say("Bot: I don't have any saved interests yet. Tell me 'I'm interested in [topic]'!");
                    return;
                }

                // Topic keywords
                if (input.Contains("password") || input.Contains("passphrase"))
                {
                    lastTopic = "password"; inTopicFlow = true;
                    Say("Bot: " + passwordTips[rand.Next(passwordTips.Length)]);
                    Say("Bot: Want another password tip?");
                    return;
                }
                if (input.Contains("phishing") || input.Contains("phish") || input.Contains("fake email"))
                {
                    lastTopic = "phishing"; inTopicFlow = true;
                    Say("Bot: " + phishingTips[rand.Next(phishingTips.Length)]);
                    Say("Bot: Want another phishing tip?");
                    return;
                }
                if (input.Contains("cybersecurity") || input.Contains("cyber security") || input.Contains("cyber"))
                {
                    lastTopic = "cybersecurity"; inTopicFlow = true;
                    Say("Bot: " + cybersecurityTips[rand.Next(cybersecurityTips.Length)]);
                    Say("Bot: Want another cybersecurity tip?");
                    return;
                }
                if (input.Contains("scam") || input.Contains("fraud") || input.Contains("con "))
                {
                    lastTopic = "scam"; inTopicFlow = true;
                    Say("Bot: " + scamTips[rand.Next(scamTips.Length)]);
                    Say("Bot: Want another scam tip?");
                    return;
                }
                if (input.Contains("privacy") || input.Contains("private") || input.Contains("personal data"))
                {
                    lastTopic = "privacy"; inTopicFlow = true;
                    Say("Bot: " + privacyTips[rand.Next(privacyTips.Length)]);
                    Say("Bot: Want another privacy tip?");
                    return;
                }

                if (input.Contains("how are you"))
                {
                    Say("Bot: I'm doing well, thank you. ");
                    Say("Bot: How are you feeling today?");
                    waitingForEmotion = true;
                    return;
                }

                if (input.Contains("help") || input.Contains("what can you do") || input.Contains("commands"))
                {
                    BtnHelp_Click(null, null);
                    return;
                }

                if (input.Contains("thank"))
                {
                    Say("Bot: You are welcome! Stay safe online ");
                    return;
                }

                if (input.Contains("hello") || input.Contains("hi ") || input.StartsWith("hi") || input.Contains("hey"))
                {
                    Say($"Bot: Hello{(string.IsNullOrEmpty(userName) ? "" : " " + userName)}! How can I help you today?");
                    return;
                }

                // Fallback with helpful suggestions
                Say("Bot: I'm not sure I understood that. Here are some things you can try:");
                Say("Bot:  Ask about: phishing, passwords, scams, privacy, cybersecurity");
                Say("Bot: 'add task', 'view tasks', 'complete task', 'delete task'");
                Say("Bot: 'start quiz'");
                Say("Bot: 'show activity log'");
            }
            catch
            {
                Say("Bot: Something went wrong ,please try again.");
            }
        }

        private int ExtractNumber(string input)
        {
            foreach (var word in input.Split(' '))
                if (int.TryParse(word, out int n)) return n;
            return -1;
        }

      // responsible for doing a follow-up on


        private bool IsFollowUp(string input)
        {
            return input.Contains("another tip") || input.Contains("tell me more") ||
                   input.Contains("more info") || input.Contains("more tips") ||
                   input.Contains("yes") || input.Contains("ok") ||
                   input.Contains("okay") || input.Contains("sure");
        }

        private void HandleFollowUp()
        {
            if (!string.IsNullOrEmpty(lastTopic) && topicTips.ContainsKey(lastTopic))
            {
                Say($"Bot: Here's another {lastTopic} tip:");
                Say("Bot: " + topicTips[lastTopic][rand.Next(topicTips[lastTopic].Length)]);
                Say("Bot: Want another tip or ask about a different topic?");
            }
        }

        // responsible for handling users emotions 
        private bool DetectEmotion(string input)
        {
            if (input.Contains("worried") || input.Contains("nervous") || input.Contains("scared"))
            {
                Say("Bot: I understand you're worried. Let's work through it together. ");
                return true;
            }
            if (input.Contains("curious"))
            {
                Say("Bot: I love your curiosity!  That's the best way to stay safe online.");
                return true;
            }
            if (input.Contains("frustrated") || input.Contains("confused") || input.Contains("lost"))
            {
                Say("Bot: I understand. Cybersecurity can feel overwhelming — I'll keep it simple.");
                return true;
            }
            return false;
        }

        private void HandleEmotionReply(string input) // the replies it gives based on what the user resposd with. Whether good or bad responses.
        {
            if (input.Contains("worried") || input.Contains("nervous") || input.Contains("scared"))
            {
                Say("Bot: It's completely okay to feel that way. Here's a reassuring tip:");
                Say("Bot: " + cybersecurityTips[rand.Next(cybersecurityTips.Length)]);
            }
            else if (input.Contains("happy") || input.Contains("good") || input.Contains("great") || input.Contains("fine"))
            {
                Say("Bot: That's awesome! What cybersecurity topic would you like to explore?");
            }
            else if (input.Contains("frustrated") || input.Contains("confused"))
            {
                Say("Bot: I understand. Let me give you a simple tip:");
                Say("Bot: " + cybersecurityTips[rand.Next(cybersecurityTips.Length)]);
            }
            else
            {
                Say("Bot: Thank you for sharing! What can I help you with today?");
            }
        }

        // This below is gives response if user only replies with one word 
        private string GetPersonalizedFollowUp(string topic)
        {
            string t = topic.ToLower();
            if (t.Contains("phishing")) return "Would you like more tips on identifying phishing attempts?";
            if (t.Contains("password")) return "Remember to use strong, unique passwords for each account!";
            if (t.Contains("scam")) return "Always verify unexpected requests, even if they seem urgent.";
            if (t.Contains("privacy")) return "Regular privacy checkups are essential for online safety.";
            if (t.Contains("cyber")) return "Staying updated is key to good cybersecurity!";
            return "Would you like more tips about " + topic + "?";
        }

        // gives response if user use abbreviation instead of the full word 
        private string GetMatchingTopic(string text)
        {
            string lower = text.ToLower();
            if (lower.Contains("phish")) return "phishing";
            if (lower.Contains("password") || lower.Contains("pass")) return "password";
            if (lower.Contains("scam") || lower.Contains("fraud")) return "scam";
            if (lower.Contains("privacy") || lower.Contains("private")) return "privacy";
            if (lower.Contains("cyber") || lower.Contains("security")) return "cybersecurity";
            return "";
        }

       
        private void QuickTopic_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;
            string tag = btn.Tag?.ToString() ?? "";
            if (string.IsNullOrEmpty(tag)) return;
            UserInput.Text = tag;
            SendButton_Click(sender, e);
        }

       // responsible for handling the buttons 
        private void BtnViewTasks_Click(object sender, RoutedEventArgs e) => ShowTasks();
        private void BtnStartQuiz_Click(object sender, RoutedEventArgs e) => StartQuiz();
        private void BtnActivityLog_Click(object sender, RoutedEventArgs e) => ShowActivityLog();

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ChatHistory.Items.Clear();
            chatLog.Clear();
            try { if (File.Exists(historyFile)) File.Delete(historyFile); } catch { }
            Say("Bot: Your chat history is all cleared up.");
        }

        private void BtnChatbot_Click(object sender, RoutedEventArgs e)
        {
            ChatHistory.Items.Add("========== CURRENT SESSION HISTORY ==========");
            foreach (var msg in chatLog) ChatHistory.Items.Add(msg);
            ChatHistory.Items.Add("=========== END OF SESSION HISTORY ===========");
            ChatHistory.ScrollIntoView(ChatHistory.Items[ChatHistory.Items.Count - 1]);
        }

        private void BtnHelp_Click(object sender, RoutedEventArgs e)// when user asks for help the following will be displayed 
        {
            Say("Bot: ─────── HOW I CAN HELP ───────");
            Say("Bot: Topics: phishing, passwords, scams, privacy, cybersecurity");
            Say("Bot: Tasks:  'add task', 'view tasks', 'complete task [id]', 'delete task [id]'");
            Say("Bot: Reminders: 'show reminders'");
            Say("Bot: Quiz:   'start quiz'");
            Say("Bot: Log:    'show activity log'");
            Say("Bot: Memory: 'I'm interested in [topic]'");
            Say("Bot: Recall: 'remind me what I'm interested in'");
            Say("Bot: ──────────────────────────────");
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Say(!string.IsNullOrEmpty(userName)
                ? $"Bot: Goodbye, {userName}! Have a lovely day and stay safe. "
                : "Bot: Goodbye! Have a lovely day and stay safe. ");
            Application.Current.Shutdown();
        }

        // commands the chatbot on what to say to the user 
        private void Say(string msg)
        {
            ChatHistory.Items.Add(msg);
            chatLog.Add(msg);
            SaveChatToFile(msg);
            if (ChatHistory.Items.Count > 0)
                ChatHistory.ScrollIntoView(ChatHistory.Items[ChatHistory.Items.Count - 1]);
        }

        // unchange part 1 and 2 
        private void SaveChatToFile(string msg)
        {
            try { File.AppendAllText(historyFile, msg + Environment.NewLine); } catch { }
        }

        private void LoadChatHistory()
        {
            try
            {
                if (File.Exists(historyFile))
                {
                    ChatHistory.Items.Add("========== START OF CHAT HISTORY ==========");
                    foreach (string line in File.ReadAllLines(historyFile))
                        ChatHistory.Items.Add(line);
                    ChatHistory.Items.Add("=========== END OF CHAT HISTORY ===========");
                }
            }
            catch { ChatHistory.Items.Add("Bot: Failed to load chat history."); }
        }

        private void SaveInterest()
        {
            try
            {
                if (!string.IsNullOrEmpty(favouriteTopic))
                    File.WriteAllText(interestsFile, $"{favouriteTopic}|{DateTime.Now}");
            }
            catch { }
        }

        private void LoadInterest()
        {
            try
            {
                if (File.Exists(interestsFile))
                {
                    string content = File.ReadAllText(interestsFile);
                    favouriteTopic = content.Contains("|") ? content.Split('|')[0] : content;
                    if (!string.IsNullOrEmpty(favouriteTopic))
                        lastTopic = GetMatchingTopic(favouriteTopic);
                }
                LoadFavorites();
            }
            catch { }
        }

        private void SaveFavorites()
        {
            try { File.WriteAllLines(favoritesFile, favoriteTopics.Select(f => f.ToString())); } catch { }
        }

        private void LoadFavorites()
        {
            try
            {
                if (File.Exists(favoritesFile))
                {
                    var lines = File.ReadAllLines(favoritesFile);
                    favoriteTopics.Clear();
                    foreach (var line in lines)
                        if (!string.IsNullOrWhiteSpace(line))
                            try { favoriteTopics.Add(FavoriteTopic.FromString(line)); } catch { }
                }
            }
            catch { }
            UpdateFavoritesDisplay();
        }

        private void UpdateFavoritesDisplay()
        {
            try
            {
                FavoriteTopicsPanel.Children.Clear();
                if (favoriteTopics.Count == 0)
                {
                    FavoriteTopicsPanel.Children.Add(new TextBlock
                    {
                        Text = "No saved favorites yet.\nSay 'I'm interested in [topic]' to save!",
                        Foreground = new SolidColorBrush(Color.FromRgb(74, 127, 175)),
                        FontSize = 10,
                        TextAlignment = TextAlignment.Center,
                        FontFamily = new FontFamily("Segoe UI"),
                        Margin = new Thickness(0, 5, 0, 5)
                    });
                }
                else
                {
                    foreach (var fav in favoriteTopics.OrderByDescending(f => f.DateSaved))
                    {
                        var border = new Border
                        {
                            Background = new SolidColorBrush(Color.FromRgb(15, 25, 35)),
                            CornerRadius = new CornerRadius(4),
                            Margin = new Thickness(0, 3, 0, 3),
                            Padding = new Thickness(5)
                        };
                        var stack = new StackPanel();
                        var topicButton = new Button
                        {
                            Content = $"📌  {fav.Topic}",
                            Background = new SolidColorBrush(Color.FromRgb(30, 58, 95)),
                            Foreground = new SolidColorBrush(Color.FromRgb(255, 215, 0)),
                            FontSize = 11,
                            FontWeight = FontWeights.SemiBold,
                            Height = 28,
                            HorizontalContentAlignment = HorizontalAlignment.Left,
                            Padding = new Thickness(8, 0, 0, 0),
                            Cursor = Cursors.Hand,
                            BorderThickness = new Thickness(0),
                            Tag = fav.Topic
                        };
                        topicButton.Click += FavoriteTopic_Click;

                        var removeButton = new Button
                        {
                            Content = "✖",
                            Background = new SolidColorBrush(Color.FromRgb(44, 26, 26)),
                            Foreground = new SolidColorBrush(Color.FromRgb(229, 115, 115)),
                            Width = 22,
                            Height = 22,
                            FontSize = 10,
                            Cursor = Cursors.Hand,
                            BorderThickness = new Thickness(0),
                            Margin = new Thickness(5, 0, 0, 0),
                            Tag = fav.Topic
                        };
                        removeButton.Click += RemoveFavorite_Click;

                        var buttonStack = new StackPanel { Orientation = Orientation.Horizontal };
                        buttonStack.Children.Add(topicButton);
                        buttonStack.Children.Add(removeButton);
                        stack.Children.Add(buttonStack);
                        stack.Children.Add(new TextBlock
                        {
                            Text = $"Saved: {fav.DateSaved:MMM dd, yyyy h:mm tt}",
                            Foreground = new SolidColorBrush(Color.FromRgb(74, 127, 175)),
                            FontSize = 9,
                            Margin = new Thickness(8, 2, 0, 0)
                        });
                        border.Child = stack;
                        FavoriteTopicsPanel.Children.Add(border);
                    }
                }
            }
            catch { }
        }

        private void AddFavorite(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic)) return;
            var existing = favoriteTopics.FirstOrDefault(f => f.Topic.Equals(topic, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                existing.DateSaved = DateTime.Now;
                Say($"Bot: Updated your interest in {topic}!");
            }
            else
            {
                favoriteTopics.Add(new FavoriteTopic { Topic = topic, DateSaved = DateTime.Now });
                Say($"Bot: 📌 Added {topic} to your favorites!");
            }
            SaveFavorites();
            UpdateFavoritesDisplay();
        }

        private void RemoveFavorite_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn?.Tag == null) return;
            string topic = btn.Tag.ToString();
            var toRemove = favoriteTopics.FirstOrDefault(f => f.Topic.Equals(topic, StringComparison.OrdinalIgnoreCase));
            if (toRemove != null)
            {
                favoriteTopics.Remove(toRemove);
                SaveFavorites();
                UpdateFavoritesDisplay();
                Say($"Bot: Removed {topic} from your favorites.");
            }
        }

        private void FavoriteTopic_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn?.Tag == null) return;
            UserInput.Text = btn.Tag.ToString();
            SendButton_Click(sender, e);
        }

        private void SaveCurrentInterest_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(lastTopic)) AddFavorite(lastTopic);
            else if (!string.IsNullOrEmpty(favouriteTopic)) AddFavorite(favouriteTopic);
            else Say("Bot: No current topic to save. Ask about a cybersecurity topic first!");
        }

        private void ClearFavorites_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Clear all favorite topics?", "Clear Favorites",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                favoriteTopics.Clear();
                SaveFavorites();
                UpdateFavoritesDisplay();
                Say("Bot:  Cleared all your favorite topics.");
            }
        }
    }

  // here are the supporting classes 
    public class QuizQuestion // makes use of get and set so that it will actually get the user info and set it.
    {
        public string Question { get; set; }
        public string[] Options { get; set; }
        public string CorrectAnswer { get; set; }
        public string Explanation { get; set; }

        public QuizQuestion(string question, string[] options, string correctAnswer, string explanation)
        {
            Question = question;
            Options = options;
            CorrectAnswer = correctAnswer;
            Explanation = explanation;
        }
    }

    public class FavoriteTopic
    {
        public string Topic { get; set; }
        public DateTime DateSaved { get; set; }

        public override string ToString() => $"{Topic}|{DateSaved}";

        public static FavoriteTopic FromString(string data)
        {
            var parts = data.Split('|');
            return new FavoriteTopic { Topic = parts[0], DateSaved = DateTime.Parse(parts[1]) };
        }
    }

    public static class Audio
    {
        public static void PlayGreeting()
        {
            try
            {
                if (File.Exists("WelcomeMessage.wav"))
                    new SoundPlayer("WelcomeMessage.wav").Play();
            }
            catch { }
        }
    }
}