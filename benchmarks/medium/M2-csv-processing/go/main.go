package main

import (
	"bufio"
	"crypto/sha256"
	"fmt"
	"math"
	"os"
	"sort"
	"strconv"
	"strings"
	"time"
)

var firstNames = []string{"James", "Mary", "John", "Patricia", "Robert",
	"Jennifer", "Michael", "Linda", "David", "Elizabeth"}
var lastNames = []string{"Smith", "Johnson", "Williams", "Brown", "Jones",
	"Garcia", "Miller", "Davis", "Rodriguez", "Martinez"}
var departments = []string{"Engineering", "Marketing", "Sales", "HR",
	"Finance", "Operations", "Support", "Legal"}

const N = 1_000_000
const RUNS = 10
const csvFile = "input.csv"
const outputFile = "output.csv"

type deptStats struct {
	count int
	total float64
	max   float64
	min   float64
}

type result struct {
	dept  string
	count int
	avg   float64
	max   float64
	min   float64
}

func generateCSV() {
	f, _ := os.Create(csvFile)
	w := bufio.NewWriterSize(f, 1<<20)
	w.WriteString("id,first_name,last_name,email,department,salary\n")
	state := uint32(42)
	for i := 0; i < N; i++ {
		state = state*1664525 + 1013904223
		salary := 30000.0 + (float64(state)/4294967295.0)*120000.0
		first := firstNames[i%10]
		last := lastNames[i%10]
		dept := departments[i%8]
		fmt.Fprintf(w, "%d,%s,%s,%s.%s@company.com,%s,%.2f\n",
			i+1, first, last, strings.ToLower(first), strings.ToLower(last), dept, salary)
	}
	w.Flush()
	f.Close()
}

func process() {
	f, _ := os.Open(csvFile)
	scanner := bufio.NewScanner(f)
	scanner.Buffer(make([]byte, 1<<20), 1<<20)

	scanner.Scan() // skip header

	groups := make(map[string]*deptStats, 8)

	for scanner.Scan() {
		line := scanner.Text()
		// Find last comma for salary
		lastComma := strings.LastIndexByte(line, ',')
		salary, _ := strconv.ParseFloat(line[lastComma+1:], 64)
		if salary > 75000.0 {
			// Find department (second to last field)
			prevComma := strings.LastIndexByte(line[:lastComma], ',')
			dept := line[prevComma+1 : lastComma]
			g, ok := groups[dept]
			if !ok {
				g = &deptStats{min: math.MaxFloat64, max: -math.MaxFloat64}
				groups[dept] = g
			}
			g.count++
			g.total += salary
			if salary > g.max {
				g.max = salary
			}
			if salary < g.min {
				g.min = salary
			}
		}
	}
	f.Close()

	results := make([]result, 0, len(groups))
	for dept, g := range groups {
		results = append(results, result{
			dept:  dept,
			count: g.count,
			avg:   g.total / float64(g.count),
			max:   g.max,
			min:   g.min,
		})
	}
	sort.Slice(results, func(i, j int) bool {
		return results[i].avg > results[j].avg
	})

	out, _ := os.Create(outputFile)
	w := bufio.NewWriter(out)
	w.WriteString("department,count,avg_salary,max_salary,min_salary\n")
	for _, r := range results {
		fmt.Fprintf(w, "%s,%d,%.2f,%.2f,%.2f\n", r.dept, r.count, r.avg, r.max, r.min)
	}
	w.Flush()
	out.Close()
}

func main() {
	timings := make([]float64, 0, RUNS)

	fmt.Println("=== BENCHMARK: M2 - CSV Processing ===")
	fmt.Println("Language: Go")
	fmt.Println("Runtime:  Go")
	fmt.Println()

	generateCSV()

	for run := 1; run <= RUNS; run++ {
		start := time.Now()
		process()
		elapsed := float64(time.Since(start).Nanoseconds()) / 1e6
		timings = append(timings, elapsed)
		fmt.Printf("Run %2d: %.2f ms\n", run, elapsed)
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

	// Verification: SHA-256 of output.csv
	data, _ := os.ReadFile(outputFile)
	h := sha256.Sum256(data)
	fmt.Printf("Verification: SHA-256 = %x\n", h)

	os.Remove(csvFile)
	os.Remove(outputFile)
}
