using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;

namespace GameModules.Utilities
{
    /// <summary>
    /// Script kiểm tra grammar và ngữ pháp của tên functions/properties/methods
    /// </summary>
    public class FunctionGrammarChecker : EditorWindow
    {
        [MenuItem("Tools/Code Quality/Function Grammar Checker")]
        public static void ShowWindow()
        {
            GetWindow<FunctionGrammarChecker>("Function Grammar Checker");
        }

        #region GUI Variables
        private Vector2 scrollPosition;
        private bool checkMethods = true;
        private bool checkProperties = true;
        private bool checkEvents = true;
        private bool checkEnglishGrammar = true;
        private bool checkVerbNounOrder = true;
        private bool checkPluralSingular = true;
        private bool checkCommonTypos = true;
        private string searchPath = "Assets/_GameModules";
        #endregion

        #region Data Structures
        [Serializable]
        public class GrammarIssue
        {
            public string filePath;
            public int lineNumber;
            public string issueType;
            public string problemText;
            public string suggestedFix;
            public string description;
            public GrammarSeverity severity;
        }

        public enum GrammarSeverity
        {
            Info,
            Warning,
            Error
        }

        private List<GrammarIssue> foundIssues = new List<GrammarIssue>();
        #endregion

        #region Grammar Rules and Dictionaries

        // Common English verbs for methods
        private static readonly HashSet<string> CommonVerbs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // Action verbs
            "Get", "Set", "Add", "Remove", "Delete", "Create", "Update", "Refresh", "Reset", "Clear",
            "Load", "Save", "Open", "Close", "Start", "Stop", "Pause", "Resume", "Play", "Initialize",
            "Destroy", "Spawn", "Enable", "Disable", "Activate", "Deactivate", "Toggle", "Switch",
            "Move", "Rotate", "Scale", "Transform", "Translate", "Convert", "Parse", "Validate",
            "Check", "Verify", "Confirm", "Cancel", "Apply", "Execute", "Invoke", "Trigger", "Fire",
            "Calculate", "Compute", "Process", "Handle", "Manage", "Control", "Monitor", "Watch",
            "Show", "Hide", "Display", "Render", "Draw", "Paint", "Fill", "Empty", "Connect", "Disconnect",
            "Send", "Receive", "Push", "Pull", "Fetch", "Submit", "Post", "Upload", "Download",
            "Search", "Find", "Filter", "Sort", "Order", "Group", "Select", "Choose", "Pick"
        };

        // Common nouns for properties and return values
        private static readonly HashSet<string> CommonNouns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Health", "Damage", "Position", "Rotation", "Scale", "Size", "Count", "Length", "Width", "Height",
            "Speed", "Velocity", "Acceleration", "Force", "Power", "Energy", "Level", "Score", "Point",
            "Player", "Enemy", "Target", "Object", "Component", "Transform", "Renderer", "Collider",
            "Color", "Material", "Texture", "Sprite", "Animation", "Sound", "Audio", "Music", "Effect",
            "Name", "ID", "Index", "Key", "Value", "Data", "Info", "State", "Status", "Type", "Mode",
            "Time", "Duration", "Delay", "Interval", "Range", "Distance", "Angle", "Direction",
            "List", "Array", "Collection", "Set", "Dictionary", "Map", "Queue", "Stack", "Pool"
        };

        // Adjectives that should come before nouns
        private static readonly HashSet<string> CommonAdjectives = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Current", "Previous", "Next", "Last", "First", "Initial", "Final", "Default", "Custom",
            "Active", "Inactive", "Enabled", "Disabled", "Visible", "Hidden", "Selected", "Available",
            "Valid", "Invalid", "Empty", "Full", "New", "Old", "Recent", "Latest", "Original",
            "Maximum", "Minimum", "Total", "Average", "Random", "Local", "Global", "Static", "Dynamic",
            "Public", "Private", "Protected", "Internal", "Readonly", "Const", "Temp", "Temporary"
        };

        // Common typos and incorrect spellings
        private static readonly Dictionary<string, string> CommonTypos = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Common programming typos
            {"Lenght", "Length"}, {"Widht", "Width"}, {"Heigth", "Height"}, {"Strenght", "Strength"},
            {"Recieve", "Receive"}, {"Seperate", "Separate"}, {"Definately", "Definitely"},
            {"Occurence", "Occurrence"}, {"Recomend", "Recommend"}, {"Independant", "Independent"},
            {"Transfered", "Transferred"}, {"Prefered", "Preferred"}, {"Refered", "Referred"},
            {"Begining", "Beginning"}, {"Writting", "Writing"}, {"Runing", "Running"},
            {"Stoping", "Stopping"}, {"Geting", "Getting"}, {"Seting", "Setting"},
            {"Colision", "Collision"}, {"Posision", "Position"}, {"Rotacion", "Rotation"},
            {"Direccion", "Direction"}, {"Conection", "Connection"}, {"Inicialization", "Initialization"},
            {"Syncronize", "Synchronize"}, {"Aply", "Apply"}, {"Procces", "Process"},
            {"Acces", "Access"}, {"Adress", "Address"}, {"Sucess", "Success"}, {"Progres", "Progress"},
            {"Complet", "Complete"}, {"Creat", "Create"}, {"Updat", "Update"}, {"Delet", "Delete"},
            
            // Game-specific typos
            {"Playr", "Player"}, {"Enemie", "Enemy"}, {"Weapn", "Weapon"}, {"Ammuniton", "Ammunition"},
            {"Expirience", "Experience"}, {"Charactr", "Character"}, {"Levl", "Level"}, {"Scor", "Score"},
            {"Helth", "Health"}, {"Invenory", "Inventory"},
            {"Upgrd", "Upgrade"}, {"Equipmnt", "Equipment"}, {"Skil", "Skill"}, {"Abilty", "Ability"},
            
            // Unity-specific
            {"Instaniate", "Instantiate"}, {"Destory", "Destroy"}, {"Awak", "Awake"},
            {"FixedUpdat", "FixedUpdate"}, {"LateUpdat", "LateUpdate"}, {"Colider", "Collider"},
            {"Rigidbdy", "Rigidbody"}, {"Transfom", "Transform"}, {"Camra", "Camera"}, {"Canva", "Canvas"}
        };

        // Words that should be plural
        private static readonly HashSet<string> ShouldBePlural = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Child", "Item", "Element", "Component", "Object", "Player", "Enemy", "Weapon", "Tool",
            "Resource", "Asset", "File", "Data", "Point", "Vector", "Vertex", "Triangle", "Polygon"
        };

        // Invalid method name patterns
        private static readonly Regex[] InvalidMethodPatterns = new Regex[]
        {
            new Regex(@"^get[A-Z].*s$"), // GetItems should be GetItemList or GetAllItems
            new Regex(@"^set[A-Z].*s$"), // SetItems should be SetItemList
            new Regex(@"^[a-z]"), // Should start with uppercase
            new Regex(@"^\d"), // Should not start with number
            new Regex(@"[_]"), // Should not contain underscore (except for Unity messages)
            new Regex(@"^(do|make|go|run)$", RegexOptions.IgnoreCase), // Too vague
        };

        #endregion

        #region GUI
        void OnGUI()
        {
            GUILayout.Label("Function Grammar Checker", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Configuration
            GUILayout.Label("Configuration", EditorStyles.boldLabel);
            searchPath = EditorGUILayout.TextField("Search Path:", searchPath);

            EditorGUILayout.Space();

            // Check Types
            GUILayout.Label("Check Types:", EditorStyles.boldLabel);
            checkMethods = EditorGUILayout.Toggle("Methods/Functions", checkMethods);
            checkProperties = EditorGUILayout.Toggle("Properties", checkProperties);
            checkEvents = EditorGUILayout.Toggle("Events", checkEvents);

            EditorGUILayout.Space();

            // Grammar Options
            GUILayout.Label("Grammar Checks:", EditorStyles.boldLabel);
            checkEnglishGrammar = EditorGUILayout.Toggle("English Grammar", checkEnglishGrammar);
            checkVerbNounOrder = EditorGUILayout.Toggle("Verb-Noun Order", checkVerbNounOrder);
            checkPluralSingular = EditorGUILayout.Toggle("Plural/Singular", checkPluralSingular);
            checkCommonTypos = EditorGUILayout.Toggle("Common Typos", checkCommonTypos);

            EditorGUILayout.Space();

            // Scan Button
            if (GUILayout.Button("Scan Grammar Issues", GUILayout.Height(30)))
            {
                ScanGrammarIssues();
            }

            EditorGUILayout.Space();

            // Results
            if (foundIssues.Count > 0)
            {
                GUILayout.Label($"Found {foundIssues.Count} Grammar Issues:", EditorStyles.boldLabel);

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

                foreach (var issue in foundIssues)
                {
                    DrawIssue(issue);
                }

                EditorGUILayout.EndScrollView();

                EditorGUILayout.Space();

                if (GUILayout.Button("Export Grammar Report"))
                {
                    ExportReport();
                }
            }
        }

        private void DrawIssue(GrammarIssue issue)
        {
            Color originalColor = GUI.color;

            // Set color based on severity
            switch (issue.severity)
            {
                case GrammarSeverity.Error:
                    GUI.color = new Color(1f, 0.7f, 0.7f);
                    break;
                case GrammarSeverity.Warning:
                    GUI.color = new Color(1f, 1f, 0.7f);
                    break;
                case GrammarSeverity.Info:
                    GUI.color = new Color(0.7f, 0.7f, 1f);
                    break;
            }

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField($"[{issue.severity}] {issue.issueType}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"File: {issue.filePath}:{issue.lineNumber}");
            EditorGUILayout.LabelField($"Problem: {issue.problemText}");

            if (!string.IsNullOrEmpty(issue.suggestedFix))
            {
                EditorGUILayout.LabelField($"Suggested: {issue.suggestedFix}", EditorStyles.miniLabel);
            }

            if (!string.IsNullOrEmpty(issue.description))
            {
                EditorGUILayout.LabelField($"Description: {issue.description}", EditorStyles.miniLabel);
            }

            // Open file button
            if (GUILayout.Button("Open File", GUILayout.Width(100)))
            {
                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(issue.filePath);
                AssetDatabase.OpenAsset(asset, issue.lineNumber);
            }

            EditorGUILayout.EndVertical();

            GUI.color = originalColor;
        }
        #endregion

        #region Scanning Logic
        private void ScanGrammarIssues()
        {
            foundIssues.Clear();

            if (!Directory.Exists(searchPath))
            {
                Debug.LogError($"Search path does not exist: {searchPath}");
                return;
            }

            var csFiles = Directory.GetFiles(searchPath, "*.cs", SearchOption.AllDirectories);

            EditorUtility.DisplayProgressBar("Scanning", "Analyzing grammar...", 0f);

            try
            {
                for (int i = 0; i < csFiles.Length; i++)
                {
                    var filePath = csFiles[i].Replace('\\', '/');
                    EditorUtility.DisplayProgressBar("Scanning", $"Analyzing {Path.GetFileName(filePath)}", (float)i / csFiles.Length);

                    ScanFileForGrammar(filePath);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            // Sort issues by severity and file
            foundIssues = foundIssues.OrderBy(i => i.severity)
                                   .ThenBy(i => i.filePath)
                                   .ThenBy(i => i.lineNumber)
                                   .ToList();

            Debug.Log($"Grammar Check Complete. Found {foundIssues.Count} issues.");
        }

        private void ScanFileForGrammar(string filePath)
        {
            try
            {
                var lines = File.ReadAllLines(filePath);

                for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
                {
                    var line = lines[lineIndex].Trim();
                    var lineNumber = lineIndex + 1;

                    if (ShouldSkipLine(line))
                        continue;

                    // Check methods
                    if (checkMethods)
                    {
                        CheckMethodGrammar(filePath, lineNumber, line);
                    }

                    // Check properties
                    if (checkProperties)
                    {
                        CheckPropertyGrammar(filePath, lineNumber, line);
                    }

                    // Check events
                    if (checkEvents)
                    {
                        CheckEventGrammar(filePath, lineNumber, line);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error scanning file {filePath}: {ex.Message}");
            }
        }

        private bool ShouldSkipLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return true;

            // Skip comments, using statements, etc.
            var skipPatterns = new[] { "//", "/*", "using ", "namespace ", "#region", "#endregion", "[" };
            return skipPatterns.Any(pattern => line.StartsWith(pattern));
        }

        #endregion

        #region Grammar Checking Methods

        private void CheckMethodGrammar(string filePath, int lineNumber, string line)
        {
            // Match method declarations
            var methodMatch = Regex.Match(line, @"(?:public|private|protected|internal)?\s*(?:static|virtual|override|abstract)?\s*(?:\w+\s+)?(\w+)\s*\([^)]*\)\s*(?:{|;)");

            if (methodMatch.Success && !line.Contains("="))
            {
                var methodName = methodMatch.Groups[1].Value;

                // Skip constructors, Unity messages, and operators
                if (IsUnityMessage(methodName) || methodName.Contains("operator") || char.IsLower(methodName[0]))
                    return;

                CheckMethodNaming(filePath, lineNumber, methodName);
            }
        }

        private void CheckPropertyGrammar(string filePath, int lineNumber, string line)
        {
            // Match property declarations
            var propertyMatch = Regex.Match(line, @"(?:public|private|protected|internal)?\s*(?:static|virtual|override)?\s*\w+\s+(\w+)\s*{");

            if (propertyMatch.Success)
            {
                var propertyName = propertyMatch.Groups[1].Value;
                CheckPropertyNaming(filePath, lineNumber, propertyName);
            }
        }

        private void CheckEventGrammar(string filePath, int lineNumber, string line)
        {
            // Match event declarations
            var eventMatch = Regex.Match(line, @"event\s+\w+\s+(\w+)");

            if (eventMatch.Success)
            {
                var eventName = eventMatch.Groups[1].Value;
                CheckEventNaming(filePath, lineNumber, eventName);
            }
        }

        private void CheckMethodNaming(string filePath, int lineNumber, string methodName)
        {
            // Check for typos
            if (checkCommonTypos)
            {
                var typoCheck = CheckForTypos(methodName);
                if (typoCheck.hasTypo)
                {
                    AddIssue(filePath, lineNumber, "Method Typo",
                        $"Method '{methodName}' contains typo: '{typoCheck.typoWord}'",
                        $"Rename to '{typoCheck.correctedName}'",
                        "Common spelling mistake detected", GrammarSeverity.Warning);
                }
            }

            // Check verb-noun order for methods
            if (checkVerbNounOrder)
            {
                CheckMethodVerbNounOrder(filePath, lineNumber, methodName);
            }

            // Check for vague method names
            CheckForVagueNames(filePath, lineNumber, methodName, "Method");

            // Check method should start with verb
            if (checkEnglishGrammar && !StartsWithVerb(methodName) && !IsPropertyLikeMethod(methodName))
            {
                var suggestion = SuggestVerbForMethod(methodName);
                AddIssue(filePath, lineNumber, "Method Grammar",
                    $"Method '{methodName}' should start with a verb",
                    suggestion,
                    "Methods typically represent actions and should start with verbs like Get, Set, Add, etc.",
                    GrammarSeverity.Warning);
            }
        }

        private void CheckPropertyNaming(string filePath, int lineNumber, string propertyName)
        {
            // Check for typos
            if (checkCommonTypos)
            {
                var typoCheck = CheckForTypos(propertyName);
                if (typoCheck.hasTypo)
                {
                    AddIssue(filePath, lineNumber, "Property Typo",
                        $"Property '{propertyName}' contains typo: '{typoCheck.typoWord}'",
                        $"Rename to '{typoCheck.correctedName}'",
                        "Common spelling mistake detected", GrammarSeverity.Warning);
                }
            }

            // Properties should be nouns or adjective+noun
            if (checkEnglishGrammar && StartsWithVerb(propertyName))
            {
                var suggestion = ConvertVerbToNoun(propertyName);
                AddIssue(filePath, lineNumber, "Property Grammar",
                    $"Property '{propertyName}' should be a noun, not a verb",
                    $"Consider renaming to '{suggestion}'",
                    "Properties represent state/data and should be nouns, not actions",
                    GrammarSeverity.Warning);
            }

            // Check plural/singular for collections
            if (checkPluralSingular)
            {
                CheckPluralSingular(filePath, lineNumber, propertyName, "Property");
            }
        }

        private void CheckEventNaming(string filePath, int lineNumber, string eventName)
        {
            // Events should start with "On" typically
            if (!eventName.StartsWith("On") && checkEnglishGrammar)
            {
                AddIssue(filePath, lineNumber, "Event Grammar",
                    $"Event '{eventName}' should typically start with 'On'",
                    $"Consider renaming to 'On{eventName}'",
                    "Events typically follow the pattern OnSomethingHappened",
                    GrammarSeverity.Info);
            }

            // Check for typos
            if (checkCommonTypos)
            {
                var typoCheck = CheckForTypos(eventName);
                if (typoCheck.hasTypo)
                {
                    AddIssue(filePath, lineNumber, "Event Typo",
                        $"Event '{eventName}' contains typo: '{typoCheck.typoWord}'",
                        $"Rename to '{typoCheck.correctedName}'",
                        "Common spelling mistake detected", GrammarSeverity.Warning);
                }
            }
        }

        #endregion

        #region Helper Methods

        private bool IsUnityMessage(string methodName)
        {
            var unityMessages = new[] {
                "Awake", "Start", "Update", "FixedUpdate", "LateUpdate", "OnEnable", "OnDisable",
                "OnDestroy", "OnApplicationPause", "OnApplicationFocus", "OnApplicationQuit",
                "OnTriggerEnter", "OnTriggerExit", "OnTriggerStay", "OnCollisionEnter",
                "OnCollisionExit", "OnCollisionStay", "OnGUI", "OnDrawGizmos", "OnValidate"
            };
            return unityMessages.Contains(methodName);
        }

        private bool StartsWithVerb(string name)
        {
            // Split camelCase and check first word
            var words = SplitCamelCase(name);
            if (words.Length == 0) return false;

            return CommonVerbs.Contains(words[0]);
        }

        private bool IsPropertyLikeMethod(string methodName)
        {
            // Methods that act like properties (getters/setters)
            return methodName.StartsWith("Is") || methodName.StartsWith("Has") || methodName.StartsWith("Can");
        }

        private string SuggestVerbForMethod(string methodName)
        {
            // Suggest appropriate verbs based on method name
            if (methodName.Contains("Health") || methodName.Contains("Value") || methodName.Contains("Count"))
                return $"Get{methodName}";
            if (methodName.Contains("List") || methodName.Contains("Array") || methodName.Contains("Collection"))
                return $"Get{methodName}";

            return $"Process{methodName}";
        }

        private string ConvertVerbToNoun(string verbName)
        {
            var words = SplitCamelCase(verbName);
            if (words.Length == 0) return verbName;

            // Convert common verbs to nouns
            var verbToNoun = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"Get", ""}, {"Set", ""}, {"Add", "Addition"}, {"Remove", "Removal"},
                {"Create", "Creation"}, {"Update", "UpdateData"}, {"Delete", "Deletion"},
                {"Move", "Position"}, {"Rotate", "Rotation"}, {"Scale", "ScaleValue"},
                {"Start", "StartTime"}, {"Stop", "StopTime"}, {"Play", "PlayState"},
                {"Calculate", "Calculation"}, {"Process", "ProcessData"}
            };

            if (verbToNoun.TryGetValue(words[0], out string nounReplacement))
            {
                if (string.IsNullOrEmpty(nounReplacement))
                {
                    // Remove the verb (like Get, Set)
                    return string.Join("", words.Skip(1));
                }
                else
                {
                    // Replace verb with noun
                    words[0] = nounReplacement;
                    return string.Join("", words);
                }
            }

            return verbName;
        }

        private void CheckMethodVerbNounOrder(string filePath, int lineNumber, string methodName)
        {
            var words = SplitCamelCase(methodName);
            if (words.Length < 2) return;

            // Check if it follows Verb + Noun pattern
            string firstWord = words[0];
            string secondWord = words[1];

            if (CommonNouns.Contains(firstWord) && CommonVerbs.Contains(secondWord))
            {
                // Wrong order: Noun + Verb instead of Verb + Noun
                string suggested = $"{secondWord}{firstWord}" + string.Join("", words.Skip(2));
                AddIssue(filePath, lineNumber, "Method Order",
                    $"Method '{methodName}' has wrong word order (Noun + Verb)",
                    $"Consider '{suggested}' (Verb + Noun)",
                    "Methods should follow Verb + Noun pattern (e.g., GetHealth, not HealthGet)",
                    GrammarSeverity.Warning);
            }
        }

        private void CheckPluralSingular(string filePath, int lineNumber, string name, string type)
        {
            if (name.EndsWith("List") || name.EndsWith("Array") || name.EndsWith("Collection") ||
                name.EndsWith("Set") || name.EndsWith("Queue") || name.EndsWith("Stack"))
            {
                // Should use plural form before collection type
                var words = SplitCamelCase(name);
                for (int i = 0; i < words.Length - 1; i++)
                {
                    if (ShouldBePlural.Contains(words[i]) && !words[i].EndsWith("s"))
                    {
                        var suggestion = name.Replace(words[i], words[i] + "s");
                        AddIssue(filePath, lineNumber, $"{type} Plural",
                            $"{type} '{name}' should use plural form for collections",
                            $"Consider '{suggestion}'",
                            "Collection properties/variables should use plural nouns",
                            GrammarSeverity.Info);
                    }
                }
            }
        }

        private void CheckForVagueNames(string filePath, int lineNumber, string name, string type)
        {
            var vagueNames = new[] { "Do", "Make", "Go", "Run", "Handle", "Process", "Execute", "Perform", "Work" };

            var words = SplitCamelCase(name);
            if (words.Length > 0 && vagueNames.Contains(words[0], StringComparer.OrdinalIgnoreCase))
            {
                AddIssue(filePath, lineNumber, $"{type} Clarity",
                    $"{type} '{name}' uses vague verb '{words[0]}'",
                    "Use more specific verbs like Get, Set, Add, Remove, Update, etc.",
                    "Avoid vague verbs. Be specific about what the method does",
                    GrammarSeverity.Info);
            }
        }

        private (bool hasTypo, string typoWord, string correctedName) CheckForTypos(string name)
        {
            var words = SplitCamelCase(name);

            for (int i = 0; i < words.Length; i++)
            {
                if (CommonTypos.TryGetValue(words[i], out string correction))
                {
                    words[i] = correction;
                    return (true, words[i], string.Join("", words));
                }
            }

            return (false, "", name);
        }

        private string[] SplitCamelCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return new string[0];

            // Split on uppercase letters, keeping the uppercase letter with the following text
            var words = Regex.Split(input, @"(?<!^)(?=[A-Z])")
                              .Where(w => !string.IsNullOrEmpty(w))
                              .ToArray();

            return words;
        }

        private void AddIssue(string filePath, int lineNumber, string issueType, string problemText,
                             string suggestedFix, string description, GrammarSeverity severity)
        {
            foundIssues.Add(new GrammarIssue
            {
                filePath = filePath,
                lineNumber = lineNumber,
                issueType = issueType,
                problemText = problemText,
                suggestedFix = suggestedFix,
                description = description,
                severity = severity
            });
        }

        #endregion

        #region Export
        private void ExportReport()
        {
            var reportPath = EditorUtility.SaveFilePanel("Export Grammar Report", "", "FunctionGrammarReport", "txt");

            if (!string.IsNullOrEmpty(reportPath))
            {
                var report = GenerateReport();
                File.WriteAllText(reportPath, report);
                Debug.Log($"Grammar report exported to: {reportPath}");
            }
        }

        private string GenerateReport()
        {
            var report = $"Function Grammar Report\nGenerated: {DateTime.Now}\n";
            report += $"Total Issues Found: {foundIssues.Count}\n\n";

            var groupedIssues = foundIssues.GroupBy(i => i.issueType);

            foreach (var group in groupedIssues)
            {
                report += $"{group.Key} Issues ({group.Count()}):\n";
                report += new string('-', 40) + "\n";

                foreach (var issue in group)
                {
                    report += $"[{issue.severity}] {issue.filePath}:{issue.lineNumber}\n";
                    report += $"  Problem: {issue.problemText}\n";
                    if (!string.IsNullOrEmpty(issue.suggestedFix))
                        report += $"  Suggested: {issue.suggestedFix}\n";
                    report += "\n";
                }
                report += "\n";
            }

            return report;
        }
        #endregion
    }
}