using System;
using System.Collections.Generic;
using System.Linq;

namespace IntentBot.Infrastructure.Services;

public class SimpleSpanishTextPreprocessor
    : ITextPreprocessor {
    private readonly HashSet<string> _stopWords;

    public SimpleSpanishTextPreprocessor() {
        _stopWords = CreateStopWords();
    }

    public string Preprocess(string text) {
        var normalized = Normalize(text);
        var withoutStopWords = RemoveStopWords(normalized);
        var stemmed = Stem(withoutStopWords);

        return stemmed;
    }

    public string[] Tokenize(string text) {
        return text.ToLowerInvariant()
            .Split([' ', '.', ',', '!', '?', ';', ':', '"', '\'', '(', ')', '[', ']', '{', '}'],
                StringSplitOptions.RemoveEmptyEntries)
            .Where(word => word.Length > 2) // Ignore very short words
            .Distinct()
            .ToArray();
    }

    public string RemoveStopWords(string text) {
        var tokens = Tokenize(text);
        var filtered = tokens
            .Where(x => !_stopWords.Contains(x.ToLower()))
            .Where(x => x.Length > 2);

        return string.Join(" ", filtered);
    }

    public string Stem(string word) {
        // TODO: añadir algún tipo de lematizador (stemmer/lemmatizer), de momento no está soportado.
        return word;
    }

    public string Normalize(string text) {
        return text.ToLowerInvariant().Trim();
    }

    public string PrepareForClassification(string text) {
        var normalized = Normalize(text);
        var withoutStopWords = RemoveStopWords(normalized);
        return Stem(withoutStopWords);
    }

    private HashSet<string> CreateStopWords()   // probablemente debería salir de un archivo o de la bb.dd.
        => new() {
            // artículos
            "el", "la", "los", "las", "un", "una", "unos", "unas", "lo", "al", "del",            
            // pronombres
            "yo", "tú", "él", "ella", "usted", "nosotros", "nosotras", "vosotros", "vosotras",
            "ellos", "ellas", "ustedes", "me", "te", "se", "nos", "os", "le", "les",
            "mi", "mis", "tu", "tus", "su", "sus", "nuestro", "nuestra", "nuestros", "nuestras",            
            // preposiciones
            "a", "ante", "bajo", "cabe", "con", "contra", "de", "desde", "durante", "en", "entre",
            "hacia", "hasta", "mediante", "para", "por", "según", "sin", "so", "sobre", "tras",            
            // conjunciones
            "y", "e", "ni", "que", "o", "u", "pero", "mas", "aunque", "sin embargo", "no obstante",
            "porque", "pues", "ya que", "como", "si", "sino", "tal", "cual",            
            // verbos comunes irrelevantes (usualmente)
            "es", "son", "era", "eran", "soy", "eres", "somos", "sois", "está", "están", "estaba",
            "estar", "ser", "tener", "tiene", "tenía", "haber", "ha", "han", "había", "hacer",
            "hace", "hacía", "poder", "puede", "podía", "decir", "dice", "decía", "ir", "va", "iba",
            "ver", "ve", "veía", "dar", "doy", "daba", "saber", "sé", "sabía", "querer", "quiere",
            "quería", "llegar", "llega", "llegaba", "dejar", "deja", "dejaba",            
            // adverbios y otras palabras comunes
            "muy", "mucho", "poco", "tan", "tanto", "como", "así", "bien", "mal", "aquí", "allí",
            "ahora", "luego", "después", "antes", "siempre", "nunca", "también", "tampoco", "sí",
            "no", "quizás", "tal vez", "acaso", "quizá", "ya", "todavía", "aún", "cómo", "cuándo",
            "dónde", "por qué", "qué", "quién", "cuál", "cuáles", "cuyo", "cuyos", "cuya", "cuyas",
            "este", "esta", "estos", "estas", "ese", "esa", "esos", "esas", "aquel", "aquella",
            "aquellos", "aquellas", "mío", "mía", "míos", "mías", "tuyo", "tuya", "tuyos", "tuyas",
            "suyo", "suya", "suyos", "suyas", "nuestro", "nuestra", "nuestros", "nuestras",
            "vuestro", "vuestra", "vuestros", "vuestras"
        };
}