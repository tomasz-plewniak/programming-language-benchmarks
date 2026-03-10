package main

import (
	"fmt"
	"math"
	"time"
)

const N = 5_000_000
const LOOKUPS = 1_000_000
const RUNS = 10

func main() {
	insertTimings := make([]float64, RUNS)
	lookupTimings := make([]float64, RUNS)

	fmt.Println("=== BENCHMARK: M3 - Hash Map Stress Test ===")
	fmt.Println("Language: Go")
	fmt.Println("Runtime:  Go")
	fmt.Println()

	var verifySum int64

	for run := 0; run < RUNS; run++ {
		// Insert phase
		start := time.Now()
		m := make(map[string]int64, N)
		for i := 0; i < N; i++ {
			key := fmt.Sprintf("user_%d", i)
			m[key] = int64(i)*31 + 7
		}
		insertElapsed := float64(time.Since(start).Nanoseconds()) / 1e6
		insertTimings[run] = insertElapsed

		// Lookup phase
		start = time.Now()
		state := uint32(42)
		var sum int64
		for j := 0; j < LOOKUPS; j++ {
			state = state*1664525 + 1013904223
			idx := uint64(state) % uint64(N)
			key := fmt.Sprintf("user_%d", idx)
			sum += m[key]
		}
		lookupElapsed := float64(time.Since(start).Nanoseconds()) / 1e6
		lookupTimings[run] = lookupElapsed

		verifySum = sum
		fmt.Printf("Run %2d: Insert: %8.2f ms | Lookup: %8.2f ms | Total: %8.2f ms\n",
			run+1, insertElapsed, lookupElapsed, insertElapsed+lookupElapsed)
	}

	fmt.Println()
	printStats("Insert", insertTimings)
	printStats("Lookup", lookupTimings)

	totalTimings := make([]float64, RUNS)
	for i := range RUNS {
		totalTimings[i] = insertTimings[i] + lookupTimings[i]
	}
	printStats("Total", totalTimings)

	fmt.Printf("Verification: sum = %d\n", verifySum)
}

func printStats(label string, timings []float64) {
	minT := timings[0]
	maxT := timings[0]
	sum := 0.0
	for _, t := range timings {
		sum += t
		if t < minT {
			minT = t
		}
		if t > maxT {
			maxT = t
		}
	}
	avg := sum / float64(len(timings))
	variance := 0.0
	for _, t := range timings {
		variance += (t - avg) * (t - avg)
	}
	variance /= float64(len(timings))
	stddev := math.Sqrt(variance)

	fmt.Printf("%s:\n", label)
	fmt.Printf("  Min:    %.2f ms\n", minT)
	fmt.Printf("  Avg:    %.2f ms\n", avg)
	fmt.Printf("  Max:    %.2f ms\n", maxT)
	fmt.Printf("  StdDev: %.2f ms\n", stddev)
	fmt.Println()
}
