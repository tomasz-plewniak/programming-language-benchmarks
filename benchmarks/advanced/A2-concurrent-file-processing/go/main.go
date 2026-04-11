package main

import (
	"bufio"
	"fmt"
	"math"
	"os"
	"path/filepath"
	"sort"
	"strings"
	"sync"
	"time"
)

const (
	numFiles     = 100
	linesPerFile = 100_000
	runs         = 10
	seed         = 42
	topN         = 100
)

var words = []string{
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
}

func generateFiles(dataDir string) {
	state := int64(seed)
	wordCount := int64(len(words))
	for f := 0; f < numFiles; f++ {
		filePath := filepath.Join(dataDir, fmt.Sprintf("file_%03d.txt", f))
		file, err := os.Create(filePath)
		if err != nil {
			panic(err)
		}
		writer := bufio.NewWriter(file)
		for line := 0; line < linesPerFile; line++ {
			state = (state*1103515245 + 12345) & 0x7FFFFFFF
			wc := int(state%11) + 5
			for w := 0; w < wc; w++ {
				if w > 0 {
					writer.WriteByte(' ')
				}
				state = (state*1103515245 + 12345) & 0x7FFFFFFF
				writer.WriteString(words[state%wordCount])
			}
			writer.WriteByte('\n')
		}
		writer.Flush()
		file.Close()
	}
}

type wordCount struct {
	word  string
	count int64
}

func main() {
	fmt.Println("=== BENCHMARK: A2 - Concurrent File Processing ===")
	fmt.Println("Language: Go")
	fmt.Printf("Runtime:  go%s\n", "1.x")
	fmt.Println()

	dataDir := filepath.Join(".", "data")
	os.MkdirAll(dataDir, 0755)
	generateFiles(dataDir)

	timings := make([]float64, runs)

	for run := 0; run < runs; run++ {
		start := time.Now()

		var mu sync.Mutex
		globalCounts := make(map[string]int64)
		var wg sync.WaitGroup

		for f := 0; f < numFiles; f++ {
			wg.Add(1)
			go func(fileIdx int) {
				defer wg.Done()
				filePath := filepath.Join(dataDir, fmt.Sprintf("file_%03d.txt", fileIdx))
				file, err := os.Open(filePath)
				if err != nil {
					panic(err)
				}
				defer file.Close()

				localCounts := make(map[string]int64)
				scanner := bufio.NewScanner(file)
				scanner.Buffer(make([]byte, 1024*1024), 1024*1024)
				for scanner.Scan() {
					line := scanner.Text()
					for _, word := range strings.Fields(line) {
						localCounts[word]++
					}
				}

				mu.Lock()
				for word, count := range localCounts {
					globalCounts[word] += count
				}
				mu.Unlock()
			}(f)
		}
		wg.Wait()

		elapsed := time.Since(start).Seconds() * 1000.0
		timings[run] = elapsed
		fmt.Printf("Run %2d: %.2f ms\n", run+1, elapsed)

		if run == runs-1 {
			sorted := make([]wordCount, 0, len(globalCounts))
			for word, count := range globalCounts {
				sorted = append(sorted, wordCount{word, count})
			}
			sort.Slice(sorted, func(i, j int) bool {
				if sorted[i].count != sorted[j].count {
					return sorted[i].count > sorted[j].count
				}
				return sorted[i].word < sorted[j].word
			})

			fmt.Println()
			fmt.Println("Verification:")
			fmt.Printf("  Total unique words: %d\n", len(globalCounts))
			fmt.Println("  Top 5 words:")
			for i := 0; i < 5 && i < len(sorted); i++ {
				fmt.Printf("    %d. %s = %d\n", i+1, sorted[i].word, sorted[i].count)
			}
			idx := 99
			if idx >= len(sorted) {
				idx = len(sorted) - 1
			}
			fmt.Printf("  Word #100: %s = %d\n", sorted[idx].word, sorted[idx].count)
		}
	}

	fmt.Println()
	minV, maxV := math.Inf(1), math.Inf(-1)
	sum := 0.0
	for _, t := range timings {
		if t < minV {
			minV = t
		}
		if t > maxV {
			maxV = t
		}
		sum += t
	}
	avg := sum / float64(runs)
	varSum := 0.0
	for _, t := range timings {
		varSum += (t - avg) * (t - avg)
	}
	stddev := math.Sqrt(varSum / float64(runs))

	fmt.Printf("Min:    %.2f ms\n", minV)
	fmt.Printf("Avg:    %.2f ms\n", avg)
	fmt.Printf("Max:    %.2f ms\n", maxV)
	fmt.Printf("StdDev: %.2f ms\n", stddev)

	os.RemoveAll(dataDir)
}
