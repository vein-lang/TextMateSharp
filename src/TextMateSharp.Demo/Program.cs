﻿using System;
using System.Collections.Generic;
using System.IO;

using TextMateSharp.Grammars;
using TextMateSharp.Internal.Themes.Reader;
using TextMateSharp.Internal.Utils;
using TextMateSharp.Registry;
using TextMateSharp.Themes;

namespace TextMateSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length < 3)
                {
                    Console.WriteLine("Usage TextMateSharp.Demo <fileToParse.cs> <grammar> <themefile>");
                    Console.WriteLine("EXAMPLE TextMateSharp.Demo .\\testdata\\samplefiles\\sample.cs .\\testdata\\grammars\\csharp.tmLanguage.json .\\testdata\\themes\\dark_vs.json");

                    return;
                }

                string fileToParse = Path.GetFullPath(args[0]);
                string grammarFile = Path.GetFullPath(args[1]);
                string themeFile = Path.GetFullPath(args[2]);

                if (!File.Exists(fileToParse))
                {
                    Console.WriteLine("No such file to parse: {0}", args[0]);
                    return;
                }

                if (!File.Exists(grammarFile))
                {
                    Console.WriteLine("No such file to parse: {0}", args[1]);
                    return;
                }

                if (!File.Exists(themeFile))
                {
                    Console.WriteLine("No such file to parse: {0}", args[2]);
                    return;
                }

                IRegistryOptions options = new DemoRegistryOptions(grammarFile, themeFile);

                Registry.Registry registry = new Registry.Registry(options);

                string[] textLines = File.ReadAllText(fileToParse).Split(Environment.NewLine);

                IGrammar grammar = registry.LoadGrammar("source.cs");

                foreach (string line in textLines)
                {
                    Console.WriteLine(string.Format("Tokenizing line: {0}", line));

                    ITokenizeLineResult result = grammar.TokenizeLine(line);

                    foreach (IToken token in result.GetTokens())
                    {
                        int startIndex = (token.StartIndex > line.Length) ?
                            line.Length : token.StartIndex;
                        int endIndex = (token.EndIndex > line.Length) ?
                            line.Length : token.EndIndex;

                        if (startIndex == endIndex)
                            continue;

                        Console.WriteLine(string.Format(
                            "  - token from {0} to {1} -->{2}<-- with scopes {3}",
                            startIndex,
                            endIndex,
                            line.Substring(startIndex, endIndex - startIndex),
                            string.Join(",", token.Scopes)));

                        foreach (string scopeName in token.Scopes)
                        {
                            Theme theme = registry.GetTheme();
                            List<ThemeTrieElementRule> themeRules =
                                theme.Match(scopeName);

                            foreach (ThemeTrieElementRule themeRule in themeRules)
                            {
                                Console.WriteLine(
                                    "      - Matched theme rule: " +
                                    "[bg: {0}, fg:{1}, fontStyle: {2}]",
                                    theme.GetColor(themeRule.background),
                                    theme.GetColor(themeRule.foreground),
                                    themeRule.fontStyle);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
            }
        }

        class DemoRegistryOptions : IRegistryOptions
        {
            private string _grammarFile;
            private string _themeFile;

            internal DemoRegistryOptions(string grammarFile, string themeFile)
            {
                _grammarFile = grammarFile;
                _themeFile = themeFile;
            }

            public string GetFilePath(string scopeName)
            {
                return _grammarFile;
            }

            public ICollection<string> GetInjections(string scopeName)
            {
                return null;
            }

            public StreamReader GetInputStream(string scopeName)
            {
                return new StreamReader(GetFilePath(scopeName));
            }

            public IRawTheme GetTheme()
            {
                using (StreamReader reader = new StreamReader(_themeFile))
                {
                    return ThemeReader.ReadThemeSync(reader);
                }
            }
        }
    }
}
