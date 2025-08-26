
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace SimpleBot.Services;

public class RuleBasedClassifier {
    private readonly ILogger<RuleBasedClassifier> _logger;
    private readonly Dictionary<string, List<string>> _categoryKeywords;

    public RuleBasedClassifier(ILogger<RuleBasedClassifier> logger) {
        _logger = logger;

        _categoryKeywords = new Dictionary<string, List<string>> {
            {
                "Login Issues",
                new List<string> {
                    "login", "log in", "sign in", "password",
                    "forgot password", "account locked", "can't access",
                    "authentication", "credentials"
                }
            },
            {
                "Billing & Payments",
                new List<string> {
                    "invoice", "charge", "payment", "refund",
                    "bill", "pricing", "subscription", "credit card",
                    "how much", "invoice number"
                }
            },
            {
                "Software Bugs",
                new List<string> {
                    "bug", "error", "crash", "freeze", "doesn't work",
                    "broken", "glitch", "something wrong", "error code"
                }
            }
        };
    }

    public (string MainCategory, Dictionary<string, string> Entities) AnalyzeText(string userInput) {
        var entities = new Dictionary<string, string>();

        // --- Entity Extraction using Regex ---
        // Extract Error Codes (e.g., "ERR-123", "AUTH-105")
        var errorCodePattern = @"[A-Z]+\-\d{3,5}";
        var errorCodeMatch = Regex.Match(userInput, errorCodePattern, RegexOptions.IgnoreCase);
        if (errorCodeMatch.Success) {
            entities.Add("ErrorCode", errorCodeMatch.Value);
        }

        // Extract potential invoice numbers (e.g., "INV-2023-9876")
        var invoicePattern = @"INV-\d{4}-\d{4,6}";
        var invoiceMatch = Regex.Match(userInput, invoicePattern, RegexOptions.IgnoreCase);
        if (invoiceMatch.Success) {
            entities.Add("InvoiceNumber", invoiceMatch.Value);
        }

        // --- Category Classification using Keyword Matching ---
        var inputLower = userInput.ToLowerInvariant();
        var matchedCategories = new List<string>();

        foreach (var category in _categoryKeywords) {
            // Check if any keyword for this category exists in the user input
            if (category.Value.Any(keyword => inputLower.Contains(keyword.ToLower()))) {
                matchedCategories.Add(category.Key);
            }
        }

        // Simple logic: pick the first matched category if multiple are found.
        // A more advanced version could use scoring.
        string mainCategory = matchedCategories.FirstOrDefault() ?? "Uncategorized";

        _logger.LogInformation($"Analyzed input: '{userInput}'. Category: '{mainCategory}'. Entities found: {entities.Count}");
        return (mainCategory, entities);
    }
}