package main

import (
	"fmt"
	"math"
	"time"
)

const N = 1000
const RUNS = 10

func main() {
	fmt.Println("=== BENCHMARK: A1 - Matrix Multiplication ===")
	fmt.Println("Language: Go")
	fmt.Printf("Runtime:  go%s\n", "1.x")
	fmt.Println()

	// Initialize matrices (flat row-major)
	a := make([]float64, N*N)
	b := make([]float64, N*N)
	for i := 0; i < N; i++ {
		for j := 0; j < N; j++ {
			a[i*N+j] = float64((i*1000+j)%97) * 0.01
			b[i*N+j] = float64((j*1000+i)%89) * 0.01
		}
	}

	c := make([]float64, N*N)
	timings := make([]float64, RUNS)

	for run := 0; run < RUNS; run++ {
		for i := range c {
			c[i] = 0.0
		}

		start := time.Now()
		for i := 0; i < N; i++ {
			for k := 0; k < N; k++ {
				aik := a[i*N+k]
				for j := 0; j < N; j++ {
					c[i*N+j] += aik * b[k*N+j]
				}
			}
		}
		elapsed := time.Since(start).Seconds() * 1000.0
		timings[run] = elapsed
		fmt.Printf("Run %2d: %.2f ms\n", run+1, elapsed)
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
	avg := sum / float64(RUNS)
	varSum := 0.0
	for _, t := range timings {
		varSum += (t - avg) * (t - avg)
	}
	stddev := math.Sqrt(varSum / float64(RUNS))

	fmt.Printf("Min:    %.2f ms\n", minV)
	fmt.Printf("Avg:    %.2f ms\n", avg)
	fmt.Printf("Max:    %.2f ms\n", maxV)
	fmt.Printf("StdDev: %.2f ms\n", stddev)
	fmt.Println()
	fmt.Println("Verification:")
	fmt.Printf("  C[0][0]     = %.6f\n", c[0])
	fmt.Printf("  C[500][500] = %.6f\n", c[500*N+500])
	fmt.Printf("  C[999][999] = %.6f\n", c[999*N+999])
}
