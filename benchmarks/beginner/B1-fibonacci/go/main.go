package main

import (
	"fmt"
	"math"
	"runtime"
	"time"
)

func fib(n int) int {
	if n <= 1 {
		return n
	}
	return fib(n-1) + fib(n-2)
}

func main() {
	const N = 40
	const RUNS = 10
	timings := make([]float64, RUNS)

	fmt.Println("=== BENCHMARK: B1 - Fibonacci ===")
	fmt.Println("Language: Go")
	fmt.Printf("Runtime:  Go %s\n", runtime.Version())
	fmt.Println()

	var result int
	for i := 0; i < RUNS; i++ {
		start := time.Now()
		result = fib(N)
		elapsed := float64(time.Since(start).Microseconds()) / 1000.0 // ms
		timings[i] = elapsed
		fmt.Printf("Run %2d: %.2f ms\n", i+1, elapsed)
	}

	fmt.Println()
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
	avg := sum / float64(RUNS)
	variance := 0.0
	for _, t := range timings {
		diff := t - avg
		variance += diff * diff
	}
	stddev := math.Sqrt(variance / float64(RUNS))

	fmt.Printf("Min:    %.2f ms\n", minT)
	fmt.Printf("Avg:    %.2f ms\n", avg)
	fmt.Printf("Max:    %.2f ms\n", maxT)
	fmt.Printf("StdDev: %.2f ms\n", stddev)
	fmt.Println()
	status := "✗"
	if result == 102334155 {
		status = "✓"
	}
	fmt.Printf("Verification: %d %s\n", result, status)
}
