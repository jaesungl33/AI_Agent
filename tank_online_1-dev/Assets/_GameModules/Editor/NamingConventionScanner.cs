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
    /// Script để quét và cảnh báo về các lỗi Naming Convention trong project
    /// </summary>
    public class NamingConventionScanner : EditorWindow
    {
        [MenuItem("Tools/Code Quality/Naming Convention Scanner")]
        public static void ShowWindow()
        {
            GetWindow<NamingConventionScanner>("Naming Convention Scanner");
        }

        #region GUI Variables
        private Vector2 scrollPosition;
        private bool showClassIssues = true;
        private bool showMethodIssues = true;
        private bool showFieldIssues = true;
        private bool showPropertyIssues = true;
        private bool showVariableIssues = true;
        private bool showParameterIssues = true;
        private bool showConstantIssues = true;
        private bool showFileIssues = true;
        private bool autoFix = false;
        private bool strictMode = false; // More strict checking, include Unity patterns
        private string searchPath = "Assets/_GameModules";
        #endregion

        #region Data Structures
        [Serializable]
        public class NamingIssue
        {
            public string filePath;
            public int lineNumber;
            public string issueType;
            public string problemText;
            public string suggestedFix;
            public string description;
            public NamingSeverity severity;
        }

        public enum NamingSeverity
        {
            Warning,
            Error,
            Info
        }

        private List<NamingIssue> foundIssues = new List<NamingIssue>();
        #endregion

        #region Naming Rules Configuration
        private static readonly Dictionary<string, Regex> NamingRules = new Dictionary<string, Regex>
        {
            // Classes, Interfaces, Structs, Enums - PascalCase
            ["Class"] = new Regex(@"^[A-Z][a-zA-Z0-9]*$"),
            ["Interface"] = new Regex(@"^I[A-Z][a-zA-Z0-9]*$"),
            ["Struct"] = new Regex(@"^[A-Z][a-zA-Z0-9]*$"),
            ["Enum"] = new Regex(@"^[A-Z][a-zA-Z0-9]*$"),
            
            // Methods, Properties - PascalCase
            ["Method"] = new Regex(@"^[A-Z][a-zA-Z0-9]*$"),
            ["Property"] = new Regex(@"^[A-Z][a-zA-Z0-9]*$"),
            
            // Fields - camelCase with underscore prefix for private
            ["PrivateField"] = new Regex(@"^_[a-z][a-zA-Z0-9]*$"),
            ["PublicField"] = new Regex(@"^[A-Z][a-zA-Z0-9]*$"),
            ["ProtectedField"] = new Regex(@"^[a-z][a-zA-Z0-9]*$"),
            
            // Local Variables, Parameters - camelCase
            ["LocalVariable"] = new Regex(@"^[a-z][a-zA-Z0-9]*$"),
            ["Parameter"] = new Regex(@"^[a-z][a-zA-Z0-9]*$"),
            
            // Constants - UPPER_CASE
            ["Constant"] = new Regex(@"^[A-Z][A-Z0-9_]*$"),
            
            // Events - PascalCase starting with "On"
            ["Event"] = new Regex(@"^On[A-Z][a-zA-Z0-9]*$"),
        };

        private static readonly HashSet<string> CommonPrefixes = new HashSet<string>
        {
            "btn", "lbl", "txt", "img", "pnl", "scr", "obj", "go", "tr", "rb", "col"
        };

        #region Exclusion Patterns
        private static readonly HashSet<string> ExclusionPatterns = new HashSet<string>
        {
            "using ", "namespace ", "#region", "#endregion", "#if", "#endif", "#else",
            "//", "/*", "*/", "///", "[SerializeField]", "[Header(", "[Tooltip(",
            "[System.Serializable]", "[Serializable]", "[ContextMenu(", "[MenuItem(",
            "[Rpc(", "[Networked", "[Capacity(", "=>", "get;", "set;", "get =>", "set =>"
        };

        private static readonly HashSet<string> CommonUnityNamespaces = new HashSet<string>
        {
            "UnityEngine", "UnityEditor", "Unity.Collections", "Unity.Mathematics",
            "Unity.Jobs", "Unity.Burst", "Unity.Entities", "Unity.Physics",
            "Unity.Rendering", "Unity.Transforms", "Unity.Networking",
            "System", "System.Collections", "System.Linq", "System.Threading",
            "Fusion", "TMPro", "Cinemachine", "DG.Tweening"
        };

        private static readonly HashSet<string> ReservedKeywords = new HashSet<string>
        {
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked",
            "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else",
            "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for",
            "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock",
            "long", "namespace", "new", "null", "object", "operator", "out", "override", "params",
            "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short",
            "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true",
            "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual",
            "void", "volatile", "while"
        };
        #endregion
        #endregion

        #region GUI
        void OnGUI()
        {
            GUILayout.Label("Naming Convention Scanner", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Configuration
            GUILayout.Label("Configuration", EditorStyles.boldLabel);
            searchPath = EditorGUILayout.TextField("Search Path:", searchPath);
            autoFix = EditorGUILayout.Toggle("Auto Fix (Experimental):", autoFix);
            strictMode = EditorGUILayout.Toggle("Strict Mode (Check Unity patterns):", strictMode);
            
            EditorGUILayout.Space();
            
            // Issue Type Filters
            GUILayout.Label("Issue Types to Check:", EditorStyles.boldLabel);
            showClassIssues = EditorGUILayout.Toggle("Classes & Types", showClassIssues);
            showMethodIssues = EditorGUILayout.Toggle("Methods", showMethodIssues);
            showFieldIssues = EditorGUILayout.Toggle("Fields", showFieldIssues);
            showPropertyIssues = EditorGUILayout.Toggle("Properties", showPropertyIssues);
            showVariableIssues = EditorGUILayout.Toggle("Local Variables", showVariableIssues);
            showParameterIssues = EditorGUILayout.Toggle("Method Parameters", showParameterIssues);
            showConstantIssues = EditorGUILayout.Toggle("Constants", showConstantIssues);
            showFileIssues = EditorGUILayout.Toggle("File Names", showFileIssues);

            EditorGUILayout.Space();

            // Scan Button
            if (GUILayout.Button("Scan Project", GUILayout.Height(30)))
            {
                ScanProject();
            }

            EditorGUILayout.Space();

            // Results
            if (foundIssues.Count > 0)
            {
                GUILayout.Label($"Found {foundIssues.Count} Naming Issues:", EditorStyles.boldLabel);
                
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                
                foreach (var issue in foundIssues)
                {
                    DrawIssue(issue);
                }
                
                EditorGUILayout.EndScrollView();

                EditorGUILayout.Space();
                
                if (GUILayout.Button("Export Report"))
                {
                    ExportReport();
                }
                
                if (autoFix && GUILayout.Button("Apply Auto Fixes"))
                {
                    ApplyAutoFixes();
                }
            }
        }

        private void DrawIssue(NamingIssue issue)
        {
            Color originalColor = GUI.color;
            
            // Set color based on severity
            switch (issue.severity)
            {
                case NamingSeverity.Error:
                    GUI.color = new Color(1f, 0.7f, 0.7f);
                    break;
                case NamingSeverity.Warning:
                    GUI.color = new Color(1f, 1f, 0.7f);
                    break;
                case NamingSeverity.Info:
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
        private void ScanProject()
        {
            foundIssues.Clear();
            
            if (!Directory.Exists(searchPath))
            {
                Debug.LogError($"Search path does not exist: {searchPath}");
                return;
            }

            var csFiles = Directory.GetFiles(searchPath, "*.cs", SearchOption.AllDirectories);
            
            EditorUtility.DisplayProgressBar("Scanning", "Analyzing files...", 0f);
            
            try
            {
                for (int i = 0; i < csFiles.Length; i++)
                {
                    var filePath = csFiles[i].Replace('\\', '/');
                    EditorUtility.DisplayProgressBar("Scanning", $"Analyzing {Path.GetFileName(filePath)}", (float)i / csFiles.Length);
                    
                    ScanFile(filePath);
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

            Debug.Log($"Naming Convention Scan Complete. Found {foundIssues.Count} issues.");
        }

        private void ScanFile(string filePath)
        {
            try
            {
                var lines = File.ReadAllLines(filePath);
                var content = File.ReadAllText(filePath);

                // Check file name
                if (showFileIssues)
                {
                    CheckFileName(filePath);
                }

                // Analyze each line
                for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
                {
                    var line = lines[lineIndex].Trim();
                    var lineNumber = lineIndex + 1;

                    // Skip lines that should be excluded from naming convention checks
                    if (ShouldExcludeLine(line))
                        continue;

                    // Check different code elements
                    CheckClassDeclaration(filePath, lineNumber, line);
                    CheckMethodDeclaration(filePath, lineNumber, line);
                    CheckFieldDeclaration(filePath, lineNumber, line);
                    CheckPropertyDeclaration(filePath, lineNumber, line);
                    CheckVariableDeclaration(filePath, lineNumber, line);
                    CheckConstantDeclaration(filePath, lineNumber, line);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error scanning file {filePath}: {ex.Message}");
            }
        }

        /// <summary>
        /// Check if a line should be excluded from naming convention analysis
        /// </summary>
        private bool ShouldExcludeLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return true;

            // Always exclude these patterns regardless of strict mode
            var alwaysExclude = new[] { "//", "/*", "*/", "///", "#region", "#endregion", "#if", "#endif", "#else" };
            if (alwaysExclude.Any(pattern => line.StartsWith(pattern)))
                return true;

            // In non-strict mode, exclude more Unity patterns
            if (!strictMode)
            {
                // Check against exclusion patterns
                foreach (var pattern in ExclusionPatterns)
                {
                    if (line.StartsWith(pattern) || line.Contains(pattern))
                        return true;
                }

                // Check for Unity/System namespace imports
                if (line.StartsWith("using "))
                {
                    foreach (var ns in CommonUnityNamespaces)
                    {
                        if (line.Contains(ns))
                            return true;
                    }
                }

                // Exclude attribute lines
                if (line.StartsWith("[") && line.EndsWith("]"))
                    return true;

                // Exclude property accessors and lambda expressions
                if (line.Contains("=>") || line.Equals("get;") || line.Equals("set;") || 
                    line.Contains("get =>") || line.Contains("set =>"))
                    return true;
            }

            return false;
        }

        #region Specific Checks
        private void CheckFileName(string filePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            
            if (!NamingRules["Class"].IsMatch(fileName))
            {
                var suggestedName = ToPascalCase(fileName);
                
                foundIssues.Add(new NamingIssue
                {
                    filePath = filePath,
                    lineNumber = 1,
                    issueType = "File Name",
                    problemText = $"File name '{fileName}' should be PascalCase",
                    suggestedFix = $"Rename to '{suggestedName}.cs'",
                    description = "File names should match their main class name and use PascalCase",
                    severity = NamingSeverity.Warning
                });
            }
        }

        private void CheckClassDeclaration(string filePath, int lineNumber, string line)
        {
            if (!showClassIssues) return;

            var classMatch = Regex.Match(line, @"(?:public|private|protected|internal)?\s*(?:abstract|sealed|static)?\s*(class|interface|struct|enum)\s+(\w+)");
            
            if (classMatch.Success)
            {
                var type = classMatch.Groups[1].Value;
                var name = classMatch.Groups[2].Value;
                
                // Skip Unity-generated or common generic names
                if (IsUnityGeneratedOrCommonName(name))
                    return;
                
                var expectedRule = type == "interface" ? "Interface" : "Class";
                
                if (!NamingRules[expectedRule].IsMatch(name))
                {
                    var suggestedName = type == "interface" ? "I" + ToPascalCase(name.TrimStart('I')) : ToPascalCase(name);
                    
                    foundIssues.Add(new NamingIssue
                    {
                        filePath = filePath,
                        lineNumber = lineNumber,
                        issueType = $"{char.ToUpper(type[0]) + type.Substring(1)} Name",
                        problemText = $"{type} '{name}' doesn't follow naming convention",
                        suggestedFix = $"Rename to '{suggestedName}'",
                        description = GetNamingRuleDescription(expectedRule),
                        severity = NamingSeverity.Error
                    });
                }
            }
        }

        /// <summary>
        /// Check if name is Unity-generated or common pattern that should be ignored
        /// </summary>
        private bool IsUnityGeneratedOrCommonName(string name)
        {
            // Unity Editor classes
            if (name.EndsWith("Editor") || name.EndsWith("Inspector") || name.EndsWith("Drawer"))
                return false; // These should still follow naming convention

            // Unity common patterns
            var unityPatterns = new[] { 
                "MonoBehaviour", "ScriptableObject", "NetworkBehaviour", 
                "INetworkStruct", "INetworkEvent", "IDisposable"
            };
            
            return unityPatterns.Any(pattern => name.Contains(pattern));
        }

        private void CheckMethodDeclaration(string filePath, int lineNumber, string line)
        {
            if (!showMethodIssues) return;

            var methodMatch = Regex.Match(line, @"(?:public|private|protected|internal)?\s*(?:static|virtual|override|abstract)?\s*(?:\w+\s+)?(\w+)\s*\(([^)]*)\)\s*(?:{|;)");
            
            if (methodMatch.Success && !line.Contains("="))
            {
                var name = methodMatch.Groups[1].Value;
                var parameters = methodMatch.Groups[2].Value;
                
                // Skip constructors and operators
                if (char.IsLower(name[0]) || name.Contains("operator") || name.Contains("Main"))
                    return;

                // Check method name
                if (!NamingRules["Method"].IsMatch(name))
                {
                    var suggestedName = ToPascalCase(name);
                    
                    foundIssues.Add(new NamingIssue
                    {
                        filePath = filePath,
                        lineNumber = lineNumber,
                        issueType = "Method Name",
                        problemText = $"Method '{name}' should be PascalCase",
                        suggestedFix = $"Rename to '{suggestedName}'",
                        description = "Method names should use PascalCase",
                        severity = NamingSeverity.Warning
                    });
                }

                // Check parameters
                CheckMethodParameters(filePath, lineNumber, parameters);
            }
        }

        private void CheckMethodParameters(string filePath, int lineNumber, string parameters)
        {
            if (!showParameterIssues || string.IsNullOrWhiteSpace(parameters)) return;

            // Match parameter patterns: type paramName, out type paramName, ref type paramName, etc.
            var paramMatches = Regex.Matches(parameters, @"(?:out|ref|in|params)?\s*\w+(?:\[\])?\s+(\w+)");
            
            foreach (Match match in paramMatches)
            {
                var paramName = match.Groups[1].Value;
                
                // Skip if it's a type or keyword
                if (ReservedKeywords.Contains(paramName) || char.IsUpper(paramName[0]))
                    continue;

                if (!NamingRules["Parameter"].IsMatch(paramName))
                {
                    var suggestedName = ToCamelCase(paramName.TrimStart('_')); // Remove underscore if present
                    
                    foundIssues.Add(new NamingIssue
                    {
                        filePath = filePath,
                        lineNumber = lineNumber,
                        issueType = "Parameter Name",
                        problemText = $"Parameter '{paramName}' should be camelCase (no underscore)",
                        suggestedFix = $"Rename to '{suggestedName}'",
                        description = "Parameter names should use camelCase without underscore prefix",
                        severity = NamingSeverity.Info
                    });
                }
            }
        }

        private void CheckFieldDeclaration(string filePath, int lineNumber, string line)
        {
            if (!showFieldIssues) return;

            var fieldMatch = Regex.Match(line, @"(public|private|protected|internal)?\s*(?:static|readonly)?\s*\w+\s+(\w+)\s*[;=]");
            
            if (fieldMatch.Success)
            {
                var visibility = fieldMatch.Groups[1].Value;
                var name = fieldMatch.Groups[2].Value;
                
                // Skip constants and properties
                if (line.Contains("const") || line.Contains("{") || char.IsUpper(name[0]) && name == name.ToUpper())
                    return;

                string expectedRule;
                if (visibility == "private" || string.IsNullOrEmpty(visibility))
                    expectedRule = "PrivateField";
                else if (visibility == "public")
                    expectedRule = "PublicField";
                else
                    expectedRule = "ProtectedField";

                if (!NamingRules[expectedRule].IsMatch(name))
                {
                    string suggestedName;
                    if (expectedRule == "PrivateField")
                        suggestedName = "_" + ToCamelCase(name.TrimStart('_'));
                    else if (expectedRule == "PublicField")
                        suggestedName = ToPascalCase(name);
                    else
                        suggestedName = ToCamelCase(name);

                    foundIssues.Add(new NamingIssue
                    {
                        filePath = filePath,
                        lineNumber = lineNumber,
                        issueType = "Field Name",
                        problemText = $"Field '{name}' doesn't follow naming convention",
                        suggestedFix = $"Rename to '{suggestedName}'",
                        description = GetNamingRuleDescription(expectedRule),
                        severity = NamingSeverity.Warning
                    });
                }
            }
        }

        private void CheckPropertyDeclaration(string filePath, int lineNumber, string line)
        {
            if (!showPropertyIssues) return;

            var propertyMatch = Regex.Match(line, @"(?:public|private|protected|internal)?\s*(?:static|virtual|override)?\s*\w+\s+(\w+)\s*{");
            
            if (propertyMatch.Success)
            {
                var name = propertyMatch.Groups[1].Value;
                
                if (!NamingRules["Property"].IsMatch(name))
                {
                    var suggestedName = ToPascalCase(name);
                    
                    foundIssues.Add(new NamingIssue
                    {
                        filePath = filePath,
                        lineNumber = lineNumber,
                        issueType = "Property Name",
                        problemText = $"Property '{name}' should be PascalCase",
                        suggestedFix = $"Rename to '{suggestedName}'",
                        description = "Property names should use PascalCase",
                        severity = NamingSeverity.Warning
                    });
                }
            }
        }

        private void CheckVariableDeclaration(string filePath, int lineNumber, string line)
        {
            if (!showVariableIssues) return;

            // Local variables (inside methods)
            var localVarMatch = Regex.Match(line, @"^\s*(?:var|\w+)\s+(\w+)\s*[=;]");
            
            if (localVarMatch.Success && !line.Contains("public") && !line.Contains("private") && !line.Contains("protected"))
            {
                var name = localVarMatch.Groups[1].Value;
                
                if (!NamingRules["LocalVariable"].IsMatch(name) && !char.IsUpper(name[0]))
                {
                    var suggestedName = ToCamelCase(name);
                    
                    foundIssues.Add(new NamingIssue
                    {
                        filePath = filePath,
                        lineNumber = lineNumber,
                        issueType = "Variable Name",
                        problemText = $"Variable '{name}' should be camelCase",
                        suggestedFix = $"Rename to '{suggestedName}'",
                        description = "Local variable names should use camelCase",
                        severity = NamingSeverity.Info
                    });
                }
            }
        }

        private void CheckConstantDeclaration(string filePath, int lineNumber, string line)
        {
            if (!showConstantIssues) return;

            var constMatch = Regex.Match(line, @"const\s+\w+\s+(\w+)\s*=");
            
            if (constMatch.Success)
            {
                var name = constMatch.Groups[1].Value;
                
                if (!NamingRules["Constant"].IsMatch(name))
                {
                    var suggestedName = ToConstantCase(name);
                    
                    foundIssues.Add(new NamingIssue
                    {
                        filePath = filePath,
                        lineNumber = lineNumber,
                        issueType = "Constant Name",
                        problemText = $"Constant '{name}' should be UPPER_CASE",
                        suggestedFix = $"Rename to '{suggestedName}'",
                        description = "Constant names should use UPPER_CASE with underscores",
                        severity = NamingSeverity.Warning
                    });
                }
            }
        }
        #endregion

        #endregion

        #region Helper Methods
        private string GetNamingRuleDescription(string ruleType)
        {
            return ruleType switch
            {
                "Class" => "Class names should use PascalCase (e.g., PlayerController)",
                "Interface" => "Interface names should start with 'I' and use PascalCase (e.g., IMovable)",
                "Method" => "Method names should use PascalCase (e.g., UpdatePlayer)",
                "PrivateField" => "Private fields should start with underscore and use camelCase (e.g., _playerHealth)",
                "PublicField" => "Public fields should use PascalCase (e.g., PlayerName)",
                "ProtectedField" => "Protected fields should use camelCase (e.g., playerData)",
                "Property" => "Property names should use PascalCase (e.g., PlayerHealth)",
                "LocalVariable" => "Local variables should use camelCase (e.g., currentHealth)",
                "Parameter" => "Parameter names should use camelCase WITHOUT underscore (e.g., newValue, not _newValue)",
                "Constant" => "Constants should use UPPER_CASE with underscores (e.g., MAX_HEALTH)",
                _ => "Unknown naming rule"
            };
        }

        private string ToPascalCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            
            var words = Regex.Split(input, @"[_\s]+").Where(w => !string.IsNullOrEmpty(w));
            return string.Concat(words.Select(word => char.ToUpper(word[0]) + word.Substring(1).ToLower()));
        }

        private string ToCamelCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            
            var pascalCase = ToPascalCase(input);
            return char.ToLower(pascalCase[0]) + pascalCase.Substring(1);
        }

        private string ToConstantCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            
            return Regex.Replace(input, @"([a-z])([A-Z])", "$1_$2").ToUpper();
        }
        #endregion

        #region Export and Auto Fix
        private void ExportReport()
        {
            var reportPath = EditorUtility.SaveFilePanel("Export Naming Report", "", "NamingConventionReport", "txt");
            
            if (!string.IsNullOrEmpty(reportPath))
            {
                var report = GenerateReport();
                File.WriteAllText(reportPath, report);
                Debug.Log($"Report exported to: {reportPath}");
            }
        }

        private string GenerateReport()
        {
            var report = $"Naming Convention Report\nGenerated: {DateTime.Now}\n";
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

        private void ApplyAutoFixes()
        {
            Debug.LogWarning("Auto-fix is experimental and may cause issues. Please backup your project first!");
            
            if (!EditorUtility.DisplayDialog("Apply Auto Fixes", 
                "This will automatically rename variables, methods, and classes to match naming conventions. " +
                "This is experimental and may break your code. Do you want to continue?", 
                "Yes", "No"))
            {
                return;
            }

            int fixedCount = 0;
            var processedFiles = new HashSet<string>();

            foreach (var issue in foundIssues.Where(i => !string.IsNullOrEmpty(i.suggestedFix)))
            {
                // For now, just log what would be fixed
                Debug.Log($"Would fix: {issue.problemText} -> {issue.suggestedFix}");
                fixedCount++;
            }

            Debug.Log($"Auto-fix simulation complete. {fixedCount} issues would be fixed.");
            EditorUtility.DisplayDialog("Auto Fix Complete", $"Simulated fixing {fixedCount} issues. Check console for details.", "OK");
        }
        #endregion
    }
}
