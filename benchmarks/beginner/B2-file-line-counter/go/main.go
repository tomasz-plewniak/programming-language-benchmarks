package main

import (
	"bufio"
	"fmt"
	"math"
	"os"
	"path/filepath"
	"runtime"
	"time"
)

const LINES = 1_000_000
const RUNS = 10

var dataDir string
var filePath string

func init() {
	dir, _ := filepath.Abs(filepath.Join(".", "..", "..", "..", "..", "data"))
	dataDir = dir
	filePath = filepath.Join(dataDir, "b2_lines.txt")
}

func writeFile() {
	os.MkdirAll(dataDir, 0755)
	f, err := os.Create(filePath)
	if err != nil {
		panic(err)
	}
	defer f.Close()
	w := bufio.NewWriter(f)
	for i := 1; i <= LINES; i++ {
		fmt.Fprintf(w, "Line %d: The quick brown fox jumps over the lazy dog\n", i)
	}
	w.Flush()
}

func readAndCount() int {
	f, err := os.Open(filePath)
	if err != nil {
		panic(err)
	}
	defer f.Close()
	scanner := bufio.NewScanner(f)
	count := 0
	for scanner.Scan() {
		count++
	}
	return count
}

func main() {
	writeTimings := make([]float64, RUNS)
	readTimings := make([]float64, RUNS)

	fmt.Println("=== BENCHMARK: B2 - File Line Counter ===")
	fmt.Println("Language: Go")
	fmt.Printf("Runtime:  Go %s\n", runtime.Version())
	fmt.Println()

	var count int
	for i := 0; i < RUNS; i++ {
		start := time.Now()
		writeFile()
		writeElapsed := float64(time.Since(start).Microseconds()) / 1000.0

		start = time.Now()
		count = readAndCount()
		readElapsed := float64(time.Since(start).Microseconds()) / 1000.0

		writeTimings[i] = writeElapsed
		readTimings[i] = readElapsed
		total := writeElapsed + readElapsed
		fmt.Printf("Run %2d: Write %.2f ms | Read %.2f ms | Total %.2f ms\n", i+1, writeElapsed, readElapsed, total)
	}

	fmt.Println()
	for _, pair := range [][2]interface{}{{"Write", writeTimings}, {"Read", readTimings}} {
		label := pair[0].(string)
		timings := pair[1].([]float64)
		minT, maxT, sum := timings[0], timings[0], 0.0
		for _, t := range timings {
			sum += t
			if t < minT { minT = t }
			if t > maxT { maxT = t }
		}
		avg := sum / float64(RUNS)
		variance := 0.0
		for _, t := range timings {
			variance += (t - avg) * (t - avg)
		}
		stddev := math.Sqrt(variance / float64(RUNS))
		fmt.Printf("%s — Min: %.2f ms | Avg: %.2f ms | Max: %.2f ms | StdDev: %.2f ms\n", label, minT, avg, maxT, stddev)
	}

	totals := make([]float64, RUNS)
	for i := range totals {
		totals[i] = writeTimings[i] + readTimings[i]
	}
	minT, maxT, sum := totals[0], totals[0], 0.0
	for _, t := range totals {
		sum += t
		if t < minT { minT = t }
		if t > maxT { maxT = t }
	}
	avg := sum / float64(RUNS)
	variance := 0.0
	for _, t := range totals {
		variance += (t - avg) * (t - avg)
	}
	stddev := math.Sqrt(variance / float64(RUNS))
	fmt.Printf("Total — Min: %.2f ms | Avg: %.2f ms | Max: %.2f ms | StdDev: %.2f ms\n", minT, avg, maxT, stddev)

	fmt.Println()
	status := "✗"
	if count == LINES {
		status = "✓"
	}
	fmt.Printf("Verification: %d %s\n", count, status)

	os.Remove(filePath)
}
