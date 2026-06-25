using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfApp2
{
    public partial class MainWindow : Window
    {
        private Random rand = new Random();

        // for the databse 

        private readonly DatabaseHelper db = new DatabaseHelper();

        // files that store user information
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

        // for the tasks 
        private bool waitingForTaskDescription = false;
        private bool waitingForReminder = false;
        private bool waitingForReminderDays = false;
        private string pendingTaskTitle = "";
        private string pendingTaskDescription = "";

        private List<string> chatLog = new List<string>();
        private List<FavoriteTopic> favoriteTopics = new List<FavoriteTopic>();

        // ---------------- TIP DATA ----------------
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
            "Consider using a password manager.",
            "Enable two-factor authentication wherever possible."
        };

        private string[] cybersecurityTips =
        {
            "Keep your operating system and apps updated.",
            "Use reputable antivirus software.",
            "Avoid public Wi-Fi for sensitive accounts without a VPN.",
            "Back up important data regularly.",
            "Unknown USB drives can carry malware."
        };

        private string[] scamTips =
        {
            "If an offer sounds too good to be true, it probably is.",
            "Never send money to someone you've only met online.",
            "Government agencies never demand urgent payment over the phone.",
            "Verify charities before donating.",
            "Be suspicious of unexpected calls asking for personal details."
        };

        private string[] privacyTips =
        {
            "Review social media privacy settings regularly.",
            "Avoid sharing too much personal information online.",
            "Check app permissions before installing.",
            "Use a VPN on public networks.",
            "Private browsers can help reduce tracking."
        };

        private Dictionary<string, string[]> topicTips;

        // ---------------- CONSTRUCTOR ----------------
        public MainWindow()
        {
            InitializeComponent();

            topicTips = new Dictionary<string, string[]>
            {
                { "phishing", phishingTips },
                { "password", passwordTips },
                { "cybersecurity", cybersecurityTips },
                { "scam", scamTips },
                { "privacy", privacyTips }
            };

            Audio.PlayGreeting();
        }

        // ---------------- WINDOW LOADED ----------------
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadChatHistory();
            LoadInterest();

            Say("Bot: Welcome to the Cybersecurity Awareness Bot! 🔐");
            Say("Bot: What is your name?");
        }

        // ---------------- SAVE CHAT ----------------
        private void SaveChatToFile(string msg)
        {
            try
            {
                File.AppendAllText(historyFile, msg + Environment.NewLine);
            }
            catch
            {
            }
        }

        // ---------------- LOAD CHAT HISTORY ----------------
        private void LoadChatHistory()
        {
            try
            {
                if (File.Exists(historyFile))
                {
                    ChatHistory.Items.Add("========== START OF CHAT HISTORY ==========");

                    string[] lines = File.ReadAllLines(historyFile);

                    foreach (string line in lines)
                    {
                        ChatHistory.Items.Add(line);
                    }

                    ChatHistory.Items.Add("=========== END OF CHAT HISTORY ===========");
                }
            }
            catch
            {
                ChatHistory.Items.Add("Bot: Failed to load chat history.");
            }
        }

        // ---------------- SAVE INTEREST ----------------
        private void SaveInterest()
        {
            try
            {
                if (!string.IsNullOrEmpty(favouriteTopic))
                {
                    string data = $"{favouriteTopic}|{DateTime.Now}";
                    File.WriteAllText(interestsFile, data);
                }
            }
            catch
            {
            }
        }

        // ---------------- LOAD INTEREST ----------------
        private void LoadInterest()
        {
            try
            {
                if (File.Exists(interestsFile))
                {
                    string content = File.ReadAllText(interestsFile);

                    if (content.Contains("|"))
                    {
                        favouriteTopic = content.Split('|')[0];
                    }
                    else
                    {
                        favouriteTopic = content;
                    }

                    if (!string.IsNullOrEmpty(favouriteTopic))
                    {
                        lastTopic = GetMatchingTopic(favouriteTopic);
                    }
                }

                LoadFavorites();
            }
            catch
            {
            }
        }

        // ---------------- SAVE FAVORITES ----------------
        private void SaveFavorites()
        {
            try
            {
                var lines = favoriteTopics.Select(f => f.ToString()).ToList();
                File.WriteAllLines(favoritesFile, lines);
            }
            catch
            {
            }
        }

        // ---------------- LOAD FAVORITES ----------------
        private void LoadFavorites()
        {
            try
            {
                if (File.Exists(favoritesFile))
                {
                    var lines = File.ReadAllLines(favoritesFile);
                    favoriteTopics.Clear();

                    foreach (var line in lines)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            try
                            {
                                favoriteTopics.Add(FavoriteTopic.FromString(line));
                            }
                            catch { }
                        }
                    }
                }
            }
            catch
            {
            }

            UpdateFavoritesDisplay();
        }

        // ---------------- UPDATE FAVORITES DISPLAY ----------------
        private void UpdateFavoritesDisplay()
        {
            try
            {
                FavoriteTopicsPanel.Children.Clear();

                if (favoriteTopics.Count == 0)
                {
                    var noFavText = new TextBlock
                    {
                        Text = "No saved favorites yet.\nSay 'I'm interested in [topic]' to save!",
                        Foreground = new SolidColorBrush(Color.FromRgb(74, 127, 175)),
                        FontSize = 10,
                        TextAlignment = TextAlignment.Center,
                        FontFamily = new FontFamily("Segoe UI"),
                        Margin = new Thickness(0, 5, 0, 5)
                    };
                    FavoriteTopicsPanel.Children.Add(noFavText);
                }
                else
                {
                    var sortedFavorites = favoriteTopics.OrderByDescending(f => f.DateSaved).ToList();

                    foreach (var fav in sortedFavorites)
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

                        var dateText = new TextBlock
                        {
                            Text = $"Saved: {fav.DateSaved:MMM dd, yyyy h:mm tt}",
                            Foreground = new SolidColorBrush(Color.FromRgb(74, 127, 175)),
                            FontSize = 9,
                            Margin = new Thickness(8, 2, 0, 0)
                        };

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
                        stack.Children.Add(dateText);
                        border.Child = stack;

                        FavoriteTopicsPanel.Children.Add(border);
                    }
                }
            }
            catch
            {
            }
        }

        // ---------------- ADD FAVORITE ----------------
        private void AddFavorite(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
                return;

            var existing = favoriteTopics.FirstOrDefault(f =>
                f.Topic.Equals(topic, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                existing.DateSaved = DateTime.Now;
                Say($"Bot: Updated your interest in {topic}! (Saved on {DateTime.Now:MMM dd, yyyy h:mm tt})");
            }
            else
            {
                favoriteTopics.Add(new FavoriteTopic
                {
                    Topic = topic,
                    DateSaved = DateTime.Now
                });
                Say($"Bot: 📌 Added {topic} to your favorites! (Saved on {DateTime.Now:MMM dd, yyyy h:mm tt})");
            }

            SaveFavorites();
            UpdateFavoritesDisplay();
        }

        // ---------------- REMOVE FAVORITE ----------------
        private void RemoveFavorite_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null && button.Tag != null)
            {
                string topic = button.Tag.ToString();
                var toRemove = favoriteTopics.FirstOrDefault(f =>
                    f.Topic.Equals(topic, StringComparison.OrdinalIgnoreCase));

                if (toRemove != null)
                {
                    favoriteTopics.Remove(toRemove);
                    SaveFavorites();
                    UpdateFavoritesDisplay();
                    Say($"Bot: Removed {topic} from your favorites.");
                }
            }
        }

        // ---------------- FAVORITE TOPIC CLICK ----------------
        private void FavoriteTopic_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null && button.Tag != null)
            {
                string topic = button.Tag.ToString();
                UserInput.Text = topic;
                SendButton_Click(sender, e);
            }
        }

        // ---------------- SAVE CURRENT INTEREST ----------------
        private void SaveCurrentInterest_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(lastTopic))
            {
                AddFavorite(lastTopic);
            }
            else if (!string.IsNullOrEmpty(favouriteTopic))
            {
                AddFavorite(favouriteTopic);
            }
            else
            {
                Say("Bot: I don't see a current topic to save. Try asking about a cybersecurity topic first!");
            }
        }

        // ---------------- CLEAR FAVORITES ----------------
        private void ClearFavorites_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to clear all your favorite topics?",
                                          "Clear Favorites",
                                          MessageBoxButton.YesNo,
                                          MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                favoriteTopics.Clear();
                SaveFavorites();
                UpdateFavoritesDisplay();
                Say("Bot: 🗑 Cleared all your favorite topics.");
            }
        }

        // ---------------- SEND BUTTON ----------------
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string input = UserInput.Text.Trim();

            if (string.IsNullOrWhiteSpace(input))
                return;

            Say("You: " + input);

            UserInput.Clear();

            string lower = input.ToLower();

            if (waitingForName)
            {
                userName = input;
                waitingForName = false;

                Say("Bot: Nice to meet you, " + userName + "! 😊");
                Say("Bot: I'm here to help you stay safe online.");

                if (!string.IsNullOrEmpty(favouriteTopic))
                {
                    Say($"Bot: I remember you're interested in {favouriteTopic}. Would you like tips about that?");
                }
                else
                {
                    Say("Bot: Ask me about phishing, passwords, scams, privacy, or cybersecurity.");
                }
                return;
            }

            if (waitingForEmotion)
            {
                waitingForEmotion = false;
                HandleEmotionReply(lower);
                return;
            }

            bool sentimentDetected = DetectEmotion(lower);

            if (inTopicFlow && IsFollowUp(lower))
            {
                HandleFollowUp();
                return;
            }

            Respond(lower, sentimentDetected);
        }

        // ---------------- FOLLOW-UP DETECTION ----------------
        private bool IsFollowUp(string input)
        {
            return input.Contains("another tip") ||
                   input.Contains("tell me more") ||
                   input.Contains("more info") ||
                   input.Contains("more tips") ||
                   input.Contains("yes") ||
                   input.Contains("ok") ||
                   input.Contains("okay");
        }

        // ---------------- HANDLE FOLLOW-UP ----------------
        private void HandleFollowUp()
        {
            if (!string.IsNullOrEmpty(lastTopic) && topicTips.ContainsKey(lastTopic))
            {
                string tip = topicTips[lastTopic][rand.Next(topicTips[lastTopic].Length)];

                Say("Bot: Here's another " + lastTopic + " tip:");
                Say("Bot: " + tip);
                Say("Bot: Want another tip or another topic?");
            }
        }

        // ---------------- MAIN RESPONSE ----------------
        private void Respond(string input, bool sentimentDetected)
        {
            try
            {
                if (input.Contains("interested in") ||
                    input.Contains("like ") && (input.Contains("topic") || input.Contains("subject")) ||
                    input.Contains("my favourite") ||
                    input.Contains("my favorite"))
                {
                    string topic = "";

                    if (input.Contains("interested in"))
                    {
                        topic = input.Substring(input.IndexOf("interested in") + 13).Trim();
                    }
                    else if (input.Contains("my favourite") || input.Contains("my favorite"))
                    {
                        topic = input.Substring(input.IndexOf("favorite") + 8).Trim();
                        topic = topic.Replace("topic is", "").Replace("subject is", "").Trim();
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
                        Say("Bot: It's a crucial part of staying safe online!");
                        Say($"Bot: As someone interested in {favouriteTopic}, here's a tip for you:");

                        if (!string.IsNullOrEmpty(lastTopic) && topicTips.ContainsKey(lastTopic))
                        {
                            Say("Bot: " + topicTips[lastTopic][rand.Next(topicTips[lastTopic].Length)]);
                        }
                        else
                        {
                            Say("Bot: " + cybersecurityTips[rand.Next(cybersecurityTips.Length)]);
                        }
                    }
                    else
                    {
                        Say("Bot: What topic are you interested in? (phishing, passwords, scams, privacy, or cybersecurity)");
                    }
                    return;
                }

                if (input.Contains("remind me") ||
                    input.Contains("what do i like") ||
                    input.Contains("what am i interested in") ||
                    input.Contains("my interest") ||
                    input.Contains("what did i tell you"))
                {
                    if (!string.IsNullOrEmpty(favouriteTopic))
                    {
                        Say($"Bot: Based on our conversation, you're interested in {favouriteTopic}. 🔍");
                        Say($"Bot: {GetPersonalizedFollowUp(favouriteTopic)}");
                    }
                    else
                    {
                        Say("Bot: I don't have any saved interests yet. Tell me 'I'm interested in [topic]' to help me remember!");
                    }
                    return;
                }

                if (input.Contains("password"))
                {
                    lastTopic = "password";
                    inTopicFlow = true;

                    string tip = passwordTips[rand.Next(passwordTips.Length)];
                    Say("Bot: " + tip);
                    Say("Bot: Want another password tip?");
                    return;
                }

                if (input.Contains("phishing"))
                {
                    lastTopic = "phishing";
                    inTopicFlow = true;

                    string tip = phishingTips[rand.Next(phishingTips.Length)];
                    Say("Bot: " + tip);
                    Say("Bot: Want another phishing tip?");
                    return;
                }

                if (input.Contains("cybersecurity") || input.Contains("cyber security"))
                {
                    lastTopic = "cybersecurity";
                    inTopicFlow = true;

                    string tip = cybersecurityTips[rand.Next(cybersecurityTips.Length)];
                    Say("Bot: " + tip);
                    Say("Bot: Want another cybersecurity tip?");
                    return;
                }

                if (input.Contains("scam"))
                {
                    lastTopic = "scam";
                    inTopicFlow = true;

                    string tip = scamTips[rand.Next(scamTips.Length)];
                    Say("Bot: " + tip);
                    Say("Bot: Want another scam tip?");
                    return;
                }

                if (input.Contains("privacy"))
                {
                    lastTopic = "privacy";
                    inTopicFlow = true;

                    string tip = privacyTips[rand.Next(privacyTips.Length)];
                    Say("Bot: " + tip);
                    Say("Bot: Want another privacy tip?");
                    return;
                }

                if (input.Contains("how are you"))
                {
                    Say("Bot: I'm doing well 😊");
                    Say("Bot: How are you feeling today?");
                    waitingForEmotion = true;
                    return;
                }

                if (input.Contains("help"))
                {
                    Say("Bot: I can help with:");
                    Say("Bot: • Phishing");
                    Say("Bot: • Passwords");
                    Say("Bot: • Scams");
                    Say("Bot: • Privacy");
                    Say("Bot: • Cybersecurity");
                    return;
                }

                Say("Bot: I didn't quite understand that.");
                Say("Bot: Try asking about phishing, passwords, scams, privacy, or cybersecurity.");
            }
            catch
            {
                Say("Bot: Something went wrong 😅");
            }
        }

        // ---------------- PERSONALIZED FOLLOW-UP ----------------
        private string GetPersonalizedFollowUp(string topic)
        {
            string lowerTopic = topic.ToLower();

            if (lowerTopic.Contains("phishing"))
                return "Would you like more tips about identifying phishing attempts?";
            if (lowerTopic.Contains("password"))
                return "Remember to use strong, unique passwords for each account!";
            if (lowerTopic.Contains("scam"))
                return "Always verify unexpected requests, even if they seem urgent.";
            if (lowerTopic.Contains("privacy"))
                return "Regular privacy checkups are essential for online safety.";
            if (lowerTopic.Contains("cyber"))
                return "Staying updated is key to cybersecurity!";

            return "Would you like more tips about " + topic + "?";
        }

        // ---------------- MATCH TOPIC ----------------
        private string GetMatchingTopic(string text)
        {
            string lower = text.ToLower();

            if (lower.Contains("phishing") || lower.Contains("phish")) return "phishing";
            if (lower.Contains("password") || lower.Contains("pass")) return "password";
            if (lower.Contains("scam") || lower.Contains("fraud")) return "scam";
            if (lower.Contains("privacy") || lower.Contains("private")) return "privacy";
            if (lower.Contains("cyber") || lower.Contains("security")) return "cybersecurity";

            return "";
        }

        // ---------------- HANDLE EMOTION ----------------
        private void HandleEmotionReply(string input)
        {
            if (input.Contains("worried") ||
                input.Contains("nervous") ||
                input.Contains("scared"))
            {
                Say("Bot: It's okay to feel that way. Cybersecurity can feel overwhelming sometimes.");
                Say("Bot: Here's a helpful tip:");
                Say("Bot: " + cybersecurityTips[rand.Next(cybersecurityTips.Length)]);
            }
            else if (input.Contains("happy") ||
                     input.Contains("good") ||
                     input.Contains("great"))
            {
                Say("Bot: That's awesome 😊");
                Say("Bot: What cybersecurity topic would you like to explore?");
            }
            else if (input.Contains("frustrated") ||
                     input.Contains("confused"))
            {
                Say("Bot: I understand. I'll keep things simple for you.");
                Say("Bot: " + cybersecurityTips[rand.Next(cybersecurityTips.Length)]);
            }
            else
            {
                Say("Bot: Thanks for sharing 😊");
            }
        }

        // ---------------- DETECT EMOTION ----------------
        private bool DetectEmotion(string input)
        {
            if (input.Contains("worried") ||
                input.Contains("nervous"))
            {
                Say("Bot: I understand you're worried. Let's work through it together.");
                return true;
            }

            if (input.Contains("curious"))
            {
                Say("Bot: I like your curiosity 😊");
                return true;
            }

            if (input.Contains("frustrated") ||
                input.Contains("confused"))
            {
                Say("Bot: I understand. Cybersecurity can get confusing sometimes.");
                return true;
            }

            return false;
        }

        // ---------------- QUICK TOPICS ----------------
        private void QuickTopic_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;

            if (btn == null)
                return;

            string tag = btn.Tag?.ToString() ?? "";

            if (string.IsNullOrEmpty(tag))
                return;

            UserInput.Text = tag;
            SendButton_Click(sender, e);
        }

        // ---------------- CLEAR CHAT ----------------
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ChatHistory.Items.Clear();
            chatLog.Clear();

            try
            {
                if (File.Exists(historyFile))
                {
                    File.Delete(historyFile);
                }
            }
            catch
            {
            }

            Say("Bot: Chat history cleared 😊");
        }

        // ---------------- SAY METHOD ----------------
        private void Say(string msg)
        {
            ChatHistory.Items.Add(msg);
            chatLog.Add(msg);
            SaveChatToFile(msg);

            if (ChatHistory.Items.Count > 0)
            {
                ChatHistory.ScrollIntoView(ChatHistory.Items[ChatHistory.Items.Count - 1]);
            }
        }

        // ---------------- CHAT HISTORY BUTTON ----------------
        private void BtnChatbot_Click(object sender, RoutedEventArgs e)
        {
            ChatHistory.Items.Add("========== CURRENT SESSION HISTORY ==========");

            foreach (var msg in chatLog)
            {
                ChatHistory.Items.Add(msg);
            }

            ChatHistory.Items.Add("=========== END OF SESSION HISTORY ===========");
            ChatHistory.ScrollIntoView(ChatHistory.Items[ChatHistory.Items.Count - 1]);
        }

        // ---------------- HELP BUTTON ----------------
        private void BtnHelp_Click(object sender, RoutedEventArgs e)
        {
            Say("Bot: Here's how I can help you:");
            Say("Bot: 📍 Ask about: Phishing, Passwords, Scams, Privacy, Cybersecurity");
            Say("Bot: 💾 Say 'I'm interested in [topic]' - I'll remember it!");
            Say("Bot: 🔍 Say 'Remind me what I'm interested in'");
            Say("Bot: 😊 Tell me how you're feeling");
            Say("Bot: ❓ Type 'help' anytime to see this menu");
        }

        // ---------------- EXIT BUTTON ----------------
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(userName))
            {
                Say($"Bot: Goodbye, {userName}! Stay safe online 😊");
            }
            else
            {
                Say("Bot: Goodbye! Stay safe online 😊");
            }

            Application.Current.Shutdown();
        }
    }

    // ---------------- FAVORITE TOPIC CLASS ----------------
    public class FavoriteTopic
    {
        public string Topic { get; set; }
        public DateTime DateSaved { get; set; }

        public override string ToString()
        {
            return $"{Topic}|{DateSaved}";
        }

        public static FavoriteTopic FromString(string data)
        {
            var parts = data.Split('|');
            return new FavoriteTopic
            {
                Topic = parts[0],
                DateSaved = DateTime.Parse(parts[1])
            };
        }
    }

    // ---------------- AUDIO ----------------
    public static class Audio
    {
        public static void PlayGreeting()
        {
            try
            {
                if (File.Exists("WelcomeMessage.wav"))
                {
                    SoundPlayer player = new SoundPlayer("WelcomeMessage.wav");
                    player.Play();
                }
            }
            catch
            {
            }
        }
    }
}