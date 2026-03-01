package main

import (
	"fmt"
	"math"
	"time"
)

func main() {
	const N = 10_000_000
	const RUNS = 10
	const EXPECTED int64 = 50000005000000

	timings := make([]float64, 0, RUNS)

	fmt.Println("=== BENCHMARK: B3 - Array Sum ===")
	fmt.Println("Language: Go")
	fmt.Printf("Runtime:  Go\n")
	fmt.Println()

	var result int64
	for i := 1; i <= RUNS; i++ {
		start := time.Now()
		arr := make([]int64, N)
		for j := 0; j < N; j++ {
			arr[j] = int64(j + 1)
		}
		var s int64
		for _, v := range arr {
			s += v
		}
		result = s
		elapsed := float64(time.Since(start).Nanoseconds()) / 1e6 // ms
		timings = append(timings, elapsed)
		fmt.Printf("Run %2d: %.2f ms\n", i, elapsed)
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
	avg := sum / float64(len(timings))
	variance := 0.0
	for _, t := range timings {
		variance += (t - avg) * (t - avg)
	}
	variance /= float64(len(timings))
	stddev := math.Sqrt(variance)

	fmt.Printf("Min:    %.2f ms\n", minT)
	fmt.Printf("Avg:    %.2f ms\n", avg)
	fmt.Printf("Max:    %.2f ms\n", maxT)
	fmt.Printf("StdDev: %.2f ms\n", stddev)
	fmt.Println()
	status := "✗"
	if result == EXPECTED {
		status = "✓"
	}
	fmt.Printf("Verification: %d %s\n", result, status)
}
