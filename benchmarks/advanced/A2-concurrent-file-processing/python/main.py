import time
import math
import sys
import os
import shutil
from concurrent.futures import ThreadPoolExecutor
from collections import defaultdict
import threading

NUM_FILES = 100
LINES_PER_FILE = 100_000
RUNS = 10
SEED = 42
TOP_N = 100

WORDS = [
    "the", "be", "to", "of", "and", "a", "in", "that", "have", "I",
    "it", "for", "not", "on", "with", "he", "as", "you", "do", "at",
    "this", "but", "his", "by", "from", "they", "we", "her", "she", "or",
    "an", "will", "my", "one", "all", "would", "there", "their", "what", "so",
    "up", "out", "if", "about", "who", "get", "which", "go", "me", "when",
    "make", "can", "like", "time", "no", "just", "him", "know", "take", "people",
    "into", "year", "your", "good", "some", "could", "them", "see", "other", "than",
    "then", "now", "look", "only", "come", "its", "over", "think", "also", "back",
    "after", "use", "two", "how", "our", "work", "first", "well", "way", "even",
    "new", "want", "because", "any", "these", "give", "day", "most", "us", "great",
    "between", "need", "large", "often", "hand", "high", "place", "old", "while", "mean",
    "keep", "let", "begin", "seem", "help", "show", "hear", "play", "run", "move",
    "live", "believe", "bring", "happen", "write", "provide", "sit", "stand", "lose", "pay",
    "meet", "include", "continue", "set", "learn", "change", "lead", "understand", "watch", "follow",
    "stop", "create", "speak", "read", "allow", "add", "spend", "grow", "open", "walk",
    "win", "teach", "offer", "remember", "love", "consider", "appear", "buy", "wait", "serve",
    "die", "send", "expect", "build", "stay", "fall", "cut", "reach", "kill", "remain",
    "suggest", "raise", "pass", "sell", "require", "report", "decide", "pull", "develop", "eat",
    "return", "hold", "cover", "point", "turn", "start", "close", "small", "number", "group",
    "always", "music", "those", "both", "mark", "call", "ask", "late", "home", "last",
    "long", "best", "still", "find", "head", "body", "water", "word", "money", "story",
    "fact", "month", "lot", "right", "study", "book", "eye", "job", "business", "issue",
    "side", "kind", "four", "room", "heart", "friend", "power", "city", "house", "party",
    "world", "area", "company", "problem", "during", "family", "government", "country", "question", "school",
    "state", "program", "information", "system", "service", "part", "idea", "table", "game", "child",
    "process", "since", "line", "result", "team", "model", "product", "market", "level", "local",
    "computer", "field", "car", "force", "food", "community", "end", "light", "real", "history",
    "political", "social", "general", "personal", "public", "national", "court", "young", "council", "war",
    "health", "age", "face", "policy", "research", "street", "law", "door", "office", "trade",
    "report", "student", "human", "data", "form", "value", "rate", "land", "project", "control",
    "action", "support", "order", "today", "figure", "class", "mother", "special", "case", "reason",
    "morning", "record", "air", "nature", "north", "sound", "effort", "fish", "plant", "true",
    "paper", "space", "event", "range", "plan", "type", "police", "road", "view", "south",
    "board", "cover", "price", "letter", "current", "future", "present", "past", "foreign", "central",
    "digital", "global", "final", "major", "natural", "popular", "cultural", "serious", "recent", "common",
    "left", "simple", "entire", "clear", "certain", "single", "source", "detail", "standard", "share",
    "modern", "potential", "key", "strong",
]


def generate_files(data_dir):
    state = SEED
    word_count = len(WORDS)
    for f in range(NUM_FILES):
        file_path = os.path.join(data_dir, f"file_{f:03d}.txt")
        with open(file_path, "w") as fh:
            lines = []
            for _ in range(LINES_PER_FILE):
                state = (state * 1103515245 + 12345) & 0x7FFFFFFF
                wc = (state % 11) + 5
                words_on_line = []
                for _ in range(wc):
                    state = (state * 1103515245 + 12345) & 0x7FFFFFFF
                    words_on_line.append(WORDS[state % word_count])
                lines.append(" ".join(words_on_line))
            fh.write("\n".join(lines))
            fh.write("\n")


def process_file(file_path):
    local_counts = defaultdict(int)
    with open(file_path, "r") as fh:
        for line in fh:
            for word in line.split():
                local_counts[word] += 1
    return local_counts


def main():
    print("=== BENCHMARK: A2 - Concurrent File Processing ===")
    print("Language: Python")
    print(f"Runtime:  Python {sys.version.split()[0]}")
    print()

    data_dir = os.path.join(os.getcwd(), "data")
    os.makedirs(data_dir, exist_ok=True)
    generate_files(data_dir)

    timings = []

    for run in range(RUNS):
        start = time.perf_counter()

        global_counts = defaultdict(int)
        lock = threading.Lock()

        def merge_counts(local):
            with lock:
                for word, count in local.items():
                    global_counts[word] += count

        file_paths = [os.path.join(data_dir, f"file_{f:03d}.txt") for f in range(NUM_FILES)]

        with ThreadPoolExecutor() as executor:
            futures = [executor.submit(process_file, fp) for fp in file_paths]
            for future in futures:
                merge_counts(future.result())

        elapsed = (time.perf_counter() - start) * 1000.0
        timings.append(elapsed)
        print(f"Run {run + 1:2}: {elapsed:.2f} ms")

        if run == RUNS - 1:
            sorted_words = sorted(global_counts.items(), key=lambda x: (-x[1], x[0]))
            top = sorted_words[:TOP_N]

            print()
            print("Verification:")
            print(f"  Total unique words: {len(global_counts)}")
            print("  Top 5 words:")
            for i in range(min(5, len(top))):
                print(f"    {i + 1}. {top[i][0]} = {top[i][1]}")
            print(f"  Word #100: {top[min(99, len(top) - 1)][0]} = {top[min(99, len(top) - 1)][1]}")

    print()
    min_t = min(timings)
    max_t = max(timings)
    avg_t = sum(timings) / RUNS
    variance = sum((t - avg_t) ** 2 for t in timings) / RUNS
    stddev = math.sqrt(variance)

    print(f"Min:    {min_t:.2f} ms")
    print(f"Avg:    {avg_t:.2f} ms")
    print(f"Max:    {max_t:.2f} ms")
    print(f"StdDev: {stddev:.2f} ms")

    shutil.rmtree(data_dir)


if __name__ == "__main__":
    main()
