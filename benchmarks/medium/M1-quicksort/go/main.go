package main

import (
	"crypto/sha256"
	"fmt"
	"math"
	"strconv"
	"time"
)

func partition(arr []int32, lo, hi int) int {
	pivot := arr[hi]
	i := lo - 1
	for j := lo; j < hi; j++ {
		if arr[j] <= pivot {
			i++
			arr[i], arr[j] = arr[j], arr[i]
		}
	}
	arr[i+1], arr[hi] = arr[hi], arr[i+1]
	return i + 1
}

func quickSort(arr []int32, lo, hi int) {
	type pair struct{ lo, hi int }
	stack := make([]pair, 0, 64)
	stack = append(stack, pair{lo, hi})
	for len(stack) > 0 {
		top := stack[len(stack)-1]
		stack = stack[:len(stack)-1]
		l, h := top.lo, top.hi
		if l < h {
			p := partition(arr, l, h)
			stack = append(stack, pair{l, p - 1})
			stack = append(stack, pair{p + 1, h})
		}
	}
}

func main() {
	const N = 5_000_000
	const RUNS = 10
	timings := make([]float64, 0, RUNS)

	fmt.Println("=== BENCHMARK: M1 - Custom QuickSort ===")
	fmt.Println("Language: Go")
	fmt.Println("Runtime:  Go")
	fmt.Println()

	arr := make([]int32, N)
	var hash string

	for run := 1; run <= RUNS; run++ {
		// Generate array with LCG (seed=42), interpreted as int32
		var state uint32 = 42
		for i := 0; i < N; i++ {
			state = state*1664525 + 1013904223
			arr[i] = int32(state)
		}

		start := time.Now()
		quickSort(arr, 0, N-1)
		elapsed := float64(time.Since(start).Nanoseconds()) / 1e6
		timings = append(timings, elapsed)
		fmt.Printf("Run %2d: %.2f ms\n", run, elapsed)

		if run == RUNS {
			h := sha256.New()
			comma := []byte{','}
			for i, v := range arr {
				if i > 0 {
					h.Write(comma)
				}
				h.Write([]byte(strconv.Itoa(int(v))))
			}
			hash = fmt.Sprintf("%x", h.Sum(nil))
		}
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
	fmt.Printf("Verification: SHA-256 = %s\n", hash)
}
